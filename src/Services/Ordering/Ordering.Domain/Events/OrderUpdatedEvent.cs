using Ordering.Domain.Abstractions;

namespace Ordering.Domain.Events;

public record OrderUpdatedEvent(Order order) : IDomainEvent;