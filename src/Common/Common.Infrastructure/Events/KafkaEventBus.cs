using Common.Application.Abstractions.Events;
using Common.Domain.Events;
using Common.Infrastructure.Events.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Common.Infrastructure.Events;

/// <summary>
/// Реализация публикации событий через Kafka
/// </summary>
public sealed class KafkaEventBus : IEventBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventBus> _logger;
    private readonly string _topicPrefix;
    private const string DefaultTopicPrefix = "domain-events";

    public KafkaEventBus(IConfiguration configuration, ILogger<KafkaEventBus> logger)
    {
        _logger = logger;
        
        // Try environment variable first, then configuration, then default
        var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
            ?? configuration["Kafka:BootstrapServers"]
            ?? "localhost:9092";
        
        _topicPrefix = Environment.GetEnvironmentVariable("KAFKA_TOPIC_PREFIX")
            ?? configuration["Kafka:TopicPrefix"]
            ?? DefaultTopicPrefix;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "nimbus-event-bus",
            Acks = Acks.All, // Ждем подтверждения от всех реплик
            EnableIdempotence = true, // Идемпотентность
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 100
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();

        _logger.LogInformation("Kafka EventBus initialized. BootstrapServers: {BootstrapServers}, TopicPrefix: {TopicPrefix}", bootstrapServers, _topicPrefix);
    }

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        var eventsList = new List<IIntegrationEvent> { integrationEvent };
        await PublishAsync(eventsList, cancellationToken);
    }

    public async Task PublishAsync(IEnumerable<IIntegrationEvent> integrationEvents, CancellationToken cancellationToken = default)
    {
        var eventsList = integrationEvents.ToList();
        if (!eventsList.Any())
        {
            return;
        }

        _logger.LogInformation("Publishing {EventCount} event(s) to Kafka", eventsList.Count);

        var tasks = eventsList.Select(async integrationEvent =>
        {
            try
            {
                var envelope = CreateEnvelope(integrationEvent);
                var message = JsonSerializer.Serialize(envelope);
                var topic = GetTopicName(integrationEvent);
                
                // Используем EventType как key для партиционирования
                var messageKey = envelope.EventType;

                var kafkaMessage = new Message<string, string>
                {
                    Key = messageKey,
                    Value = message,
                    Headers = new Headers
                    {
                        { "EventId", Encoding.UTF8.GetBytes(integrationEvent.EventId.ToString()) },
                        { "CorrelationId", Encoding.UTF8.GetBytes(envelope.CorrelationId.ToString()) },
                        { "Version", Encoding.UTF8.GetBytes(envelope.Version) }
                    }
                };

                if (envelope.TenantId.HasValue)
                {
                    kafkaMessage.Headers.Add("TenantId", Encoding.UTF8.GetBytes(envelope.TenantId.Value.ToString()));
                }

                var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

                _logger.LogDebug(
                    "Published event {EventType} to topic {Topic}, partition {Partition}, offset {Offset}",
                    envelope.EventType,
                    deliveryResult.Topic,
                    deliveryResult.Partition,
                    deliveryResult.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                var errorMessage = $"Error publishing event {integrationEvent.GetType().Name} to Kafka. Error: {ex.Error.Reason}";
                
                if (ex.Error.IsFatal)
                {
                    _logger.LogError(ex, errorMessage);
                    // При фатальной ошибке пробрасываем исключение
                    throw;
                }
                else
                {
                    // При временной ошибке логируем предупреждение, но не прерываем выполнение
                    _logger.LogWarning(ex, errorMessage + " Event may be lost.");
                    // Можно добавить retry логику или очередь для повторной отправки
                }
            }
            catch (KafkaException ex)
            {
                _logger.LogError(ex, "Kafka connection error while publishing event {EventType}: {Message}", 
                    integrationEvent.GetType().Name, ex.Message);
                // При ошибке подключения логируем, но не прерываем выполнение приложения
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error publishing event {EventType}",
                    integrationEvent.GetType().Name);
                // Пробрасываем только критические ошибки
                throw;
            }
        });

        await Task.WhenAll(tasks);
    }

    private static EventEnvelope CreateEnvelope(IIntegrationEvent integrationEvent)
    {
        // Извлекаем TenantId из события, если оно реализует IEventMetadata
        var tenantId = integrationEvent is IEventMetadata metadata ? metadata.TenantId : null;
        
        // Генерируем CorrelationId, если его нет
        var correlationId = integrationEvent is IEventMetadata meta && meta.CorrelationId != Guid.Empty
            ? meta.CorrelationId
            : Guid.NewGuid();

        var envelope = new EventEnvelope
        {
            EventType = integrationEvent.GetType().FullName ?? integrationEvent.GetType().Name,
            Version = "1.0", // Можно извлекать из атрибутов события
            Timestamp = integrationEvent.OccurredOn,
            CorrelationId = correlationId,
            TenantId = tenantId,
            Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType())
        };

        return envelope;
    }

    private string GetTopicName(IIntegrationEvent integrationEvent)
    {
        // Topic = namespace последняя часть
        // Например: Contracts.Tenants.Events.TenantCreatedEvent -> domain-events-tenants
        var eventType = integrationEvent.GetType();
        var namespaceParts = eventType.Namespace?.Split('.') ?? Array.Empty<string>();
        
        if (namespaceParts.Length > 0)
        {
            // Ищем модуль в namespace (например, Tenants, Users, Projects)
            // Обычно структура: Contracts.{Module}.Events.{EventName}
            var moduleName = namespaceParts
                .FirstOrDefault(p => p != "Contracts" && p != "Events" && !string.IsNullOrEmpty(p))
                ?.ToLowerInvariant() ?? "common";
            return $"{_topicPrefix}-{moduleName}";
        }

        return _topicPrefix;
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}

