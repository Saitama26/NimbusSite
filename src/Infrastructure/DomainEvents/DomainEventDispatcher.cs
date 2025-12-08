using Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.DomainEvents;

internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        IMediator mediator,
        ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        if (domainEvents is null || domainEvents.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "Dispatching {EventCount} domain event(s)",
            domainEvents.Count);

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                _logger.LogDebug(
                    "Publishing domain event {EventType}",
                    domainEvent.GetType().Name);

                await _mediator.Publish(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing domain event {EventType}",
                    domainEvent.GetType().Name);
                throw;
            }
        }

        _logger.LogInformation(
            "Successfully dispatched {EventCount} domain event(s)",
            domainEvents.Count);
    }
}

