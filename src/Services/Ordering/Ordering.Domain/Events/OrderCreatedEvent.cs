
using Ordering.Domain.Abstractions;

namespace Ordering.Domain.Events;

public record OrderCreatedEvent(Order order) : IDomainEvent;
