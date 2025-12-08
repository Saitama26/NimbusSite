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
    private const string DefaultTopicPrefix = "domain-events";

    public KafkaEventBus(IConfiguration configuration, ILogger<KafkaEventBus> logger)
    {
        _logger = logger;
        
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

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

        _logger.LogInformation("Kafka EventBus initialized. BootstrapServers: {BootstrapServers}", bootstrapServers);
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var eventsList = new List<IDomainEvent> { domainEvent };
        await PublishAsync(eventsList, cancellationToken);
    }

    public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var eventsList = domainEvents.ToList();
        if (!eventsList.Any())
        {
            return;
        }

        _logger.LogInformation("Publishing {EventCount} event(s) to Kafka", eventsList.Count);

        var tasks = eventsList.Select(async domainEvent =>
        {
            try
            {
                var envelope = CreateEnvelope(domainEvent);
                var message = JsonSerializer.Serialize(envelope);
                var topic = GetTopicName(domainEvent);
                
                // Используем EventType как key для партиционирования
                var messageKey = envelope.EventType;

                var kafkaMessage = new Message<string, string>
                {
                    Key = messageKey,
                    Value = message,
                    Headers = new Headers
                    {
                        { "EventId", Encoding.UTF8.GetBytes(domainEvent.EventId.ToString()) },
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
                _logger.LogError(
                    ex,
                    "Error publishing event {EventType} to Kafka. Error: {Error}",
                    domainEvent.GetType().Name,
                    ex.Error.Reason);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing event {EventType}",
                    domainEvent.GetType().Name);
                throw;
            }
        });

        await Task.WhenAll(tasks);
    }

    private static EventEnvelope CreateEnvelope(IDomainEvent domainEvent)
    {
        // Извлекаем TenantId из события, если оно реализует IEventMetadata
        var tenantId = domainEvent is IEventMetadata metadata ? metadata.TenantId : null;
        
        // Генерируем CorrelationId, если его нет
        var correlationId = domainEvent is IEventMetadata meta && meta.CorrelationId != Guid.Empty
            ? meta.CorrelationId
            : Guid.NewGuid();

        var envelope = new EventEnvelope
        {
            EventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
            Version = "1.0", // Можно извлекать из атрибутов события
            Timestamp = domainEvent.OccurredOn,
            CorrelationId = correlationId,
            TenantId = tenantId,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
        };

        return envelope;
    }

    private static string GetTopicName(IDomainEvent domainEvent)
    {
        // Topic = namespace последняя часть
        // Например: Tenants.Domain.Events.TenantCreatedEvent -> domain-events-tenants
        var eventType = domainEvent.GetType();
        var namespaceParts = eventType.Namespace?.Split('.') ?? Array.Empty<string>();
        
        if (namespaceParts.Length > 0)
        {
            // Берем первую часть namespace (например, Tenants, Users)
            var moduleName = namespaceParts[0].ToLowerInvariant();
            return $"{DefaultTopicPrefix}-{moduleName}";
        }

        return DefaultTopicPrefix;
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}

