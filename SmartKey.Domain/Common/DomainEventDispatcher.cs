namespace SmartKey.Domain.Common
{
    public interface DomainEventDispatcher
    {
        Task DispatchAsync(IEnumerable<IDomainEvent> @event, CancellationToken ct = default);
    }
}
