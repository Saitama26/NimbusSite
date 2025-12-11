using Common.Application.Abstractions.Events;
using Common.Domain.Events;
using Common.Infrastructure.Events.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Common.Infrastructure.Events;

/// <summary>
/// Реализация подписки на события из Kafka
/// </summary>
public sealed class KafkaEventSubscriber : IEventSubscriber, IHostedService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaEventSubscriber> _logger;
    private readonly ConcurrentDictionary<string, Func<IDomainEvent, CancellationToken, Task>> _handlers = new();
    private readonly string _groupId;
    private readonly string[] _topics;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _consumingTask;

    public KafkaEventSubscriber(
        IConfiguration configuration,
        ILogger<KafkaEventSubscriber> logger)
    {
        _logger = logger;
        
        // Try environment variable first, then configuration, then default
        var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
            ?? configuration["Kafka:BootstrapServers"]
            ?? "localhost:9092";
        _groupId = configuration["Kafka:ConsumerGroupId"] ?? "nimbus-consumer-group";
        var topicsConfig = configuration["Kafka:Topics"] ?? "";
        _topics = topicsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, // Ручной commit после обработки
            EnablePartitionEof = true,
            MetadataMaxAgeMs = 10000, // Обновляем метаданные каждые 10 секунд (быстрее)
            SessionTimeoutMs = 30000, // Таймаут сессии
            MaxPollIntervalMs = 300000, // Максимальный интервал между poll
            AllowAutoCreateTopics = false, // Не создаем топики автоматически
            ApiVersionRequest = true, // Запрашиваем версию API
            ApiVersionRequestTimeoutMs = 10000 // Таймаут запроса версии API
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        
        _logger.LogInformation(
            "Kafka EventSubscriber initialized. GroupId: {GroupId}, Topics: {Topics}",
            _groupId,
            string.Join(", ", _topics));
    }

    public async Task SubscribeAsync<TEvent>(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var eventTypeName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
        
        _logger.LogInformation("Subscribing to event {EventType}", eventTypeName);

        // Сохраняем handler
        _handlers[eventTypeName] = async (evt, ct) =>
        {
            if (evt is TEvent typedEvent)
            {
                await handler(typedEvent, ct);
            }
        };

        _logger.LogInformation("Subscribed to event {EventType}", eventTypeName);
        await Task.CompletedTask;
    }

    public async Task UnsubscribeAsync<TEvent>(CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var eventTypeName = typeof(TEvent).FullName ?? typeof(TEvent).Name;
        
        if (_handlers.TryRemove(eventTypeName, out _))
        {
            _logger.LogInformation("Unsubscribed from event {EventType}", eventTypeName);
        }

        await Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_topics.Length == 0)
        {
            _logger.LogWarning("No topics configured for Kafka consumer");
            return;
        }

        _logger.LogInformation("Starting Kafka event subscriber...");
        
        // Даем Kafka время на синхронизацию метаданных перед подпиской
        await Task.Delay(5000, cancellationToken); // Увеличено до 5 секунд
        
        int retryCount = 0;
        const int maxRetries = 5;
        
        while (retryCount < maxRetries)
        {
            try
            {
                _consumer.Subscribe(_topics);
                _logger.LogInformation("Successfully subscribed to topics: {Topics}", string.Join(", ", _topics));
                break; // Успешно подписались, выходим из цикла
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    _logger.LogError(ex, "Failed to subscribe to Kafka topics after {Retries} attempts. Consumer will retry connection in background.", maxRetries);
                    // Не прерываем запуск приложения, consumer будет пытаться подключиться в фоне
                    break;
                }
                _logger.LogWarning(ex, "Failed to subscribe to Kafka topics (attempt {Attempt}/{Max}). Retrying in 3 seconds...", retryCount, maxRetries);
                await Task.Delay(3000, cancellationToken);
            }
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _consumingTask = Task.Run(() => ConsumeMessages(_cancellationTokenSource.Token), cancellationToken);

        _logger.LogInformation("Kafka event subscriber started");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Kafka event subscriber...");
        
        _cancellationTokenSource?.Cancel();
        
        if (_consumingTask != null)
        {
            await _consumingTask;
        }

        _consumer.Close();
        _logger.LogInformation("Kafka event subscriber stopped");
    }

    private async Task ConsumeMessages(CancellationToken cancellationToken)
    {
        const int retryDelayMs = 5000; // 5 секунд между попытками переподключения
        var consecutiveErrors = 0;
        const int maxConsecutiveErrors = 10; // После 10 ошибок подряд увеличиваем задержку

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1)); // Таймаут 1 секунда

                    if (consumeResult == null)
                    {
                        // Таймаут - это нормально, продолжаем
                        continue;
                    }

                    if (consumeResult.IsPartitionEOF)
                    {
                        continue;
                    }

                    // Сброс счетчика ошибок при успешном получении сообщения
                    consecutiveErrors = 0;

                    _logger.LogDebug(
                        "Received message from topic {Topic}, partition {Partition}, offset {Offset}",
                        consumeResult.Topic,
                        consumeResult.Partition,
                        consumeResult.Offset);

                    await ProcessMessage(consumeResult, cancellationToken);

                    // Commit после успешной обработки
                    _consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    consecutiveErrors++;
                    
                    if (ex.Error.IsFatal)
                    {
                        _logger.LogError(ex, "Fatal error consuming message from Kafka: {Reason}", ex.Error.Reason);
                        // При фатальной ошибке ждем дольше перед следующей попыткой
                        await Task.Delay(retryDelayMs * 2, cancellationToken);
                    }
                    else if (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        // Топик еще не доступен - переподписываемся и ждем дольше для синхронизации метаданных
                        _logger.LogWarning(ex, "Topic not available yet: {Reason}. Resubscribing and waiting for metadata sync...", ex.Error.Reason);
                        try
                        {
                            // Переподписываемся для обновления метаданных
                            _consumer.Unsubscribe();
                            await Task.Delay(1000, cancellationToken); // Небольшая задержка перед переподпиской
                            _consumer.Subscribe(_topics);
                        }
                        catch (Exception subEx)
                        {
                            _logger.LogWarning(subEx, "Error during resubscription, will retry later");
                        }
                        // Ждем дольше, чтобы Kafka успел синхронизировать метаданные о топиках
                        await Task.Delay(retryDelayMs * 3, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning(ex, "Error consuming message from Kafka: {Reason}. Retrying...", ex.Error.Reason);
                        // При обычной ошибке ждем меньше
                        var delay = consecutiveErrors > maxConsecutiveErrors ? retryDelayMs * 2 : retryDelayMs;
                        await Task.Delay(delay, cancellationToken);
                    }
                }
                catch (KafkaException ex)
                {
                    consecutiveErrors++;
                    _logger.LogError(ex, "Kafka connection error: {Message}. Retrying in {Delay}ms...", ex.Message, retryDelayMs);
                    await Task.Delay(retryDelayMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    consecutiveErrors++;
                    _logger.LogError(ex, "Unexpected error in Kafka consumer. Retrying in {Delay}ms...", retryDelayMs);
                    await Task.Delay(retryDelayMs, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in Kafka consumer loop. Consumer stopped.");
            // Не пробрасываем исключение, чтобы не остановить приложение
        }
    }

    private async Task ProcessMessage(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<EventEnvelope>(consumeResult.Message.Value);

            if (envelope == null)
            {
                _logger.LogWarning("Failed to deserialize event envelope");
                return;
            }

            _logger.LogDebug(
                "Processing event {EventType} with correlation ID {CorrelationId}",
                envelope.EventType,
                envelope.CorrelationId);

            // Находим handler для этого типа события
            var handlerKey = _handlers.Keys.FirstOrDefault(k => envelope.EventType.Contains(k.Split('.').Last()));
            
            if (handlerKey != null && _handlers.TryGetValue(handlerKey, out var handler))
            {
                // Десериализуем событие
                var eventType = Type.GetType(envelope.EventType);
                if (eventType != null && typeof(IDomainEvent).IsAssignableFrom(eventType))
                {
                    var domainEvent = JsonSerializer.Deserialize(envelope.Payload, eventType) as IDomainEvent;
                    if (domainEvent != null)
                    {
                        await handler(domainEvent, cancellationToken);
                        _logger.LogDebug("Successfully processed event {EventType}", envelope.EventType);
                        return;
                    }
                }
            }

            _logger.LogWarning("No handler found for event type {EventType}", envelope.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event message");
            throw; // Пробрасываем, чтобы не commit сообщение
        }
    }

    public void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}

