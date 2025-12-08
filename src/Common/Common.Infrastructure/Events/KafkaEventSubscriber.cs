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
        
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _groupId = configuration["Kafka:ConsumerGroupId"] ?? "nimbus-consumer-group";
        var topicsConfig = configuration["Kafka:Topics"] ?? "";
        _topics = topicsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = _groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, // Ручной commit после обработки
            EnablePartitionEof = true
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
        _consumer.Subscribe(_topics);

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
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);

                    if (consumeResult.IsPartitionEOF)
                    {
                        continue;
                    }

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
                    _logger.LogError(ex, "Error consuming message from Kafka");
                    // Продолжаем обработку других сообщений
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Kafka consumer");
            throw;
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

