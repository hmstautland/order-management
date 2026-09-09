using System.Collections.Concurrent;
using OrderManagement.Api.Models;

namespace OrderManagement.Api.Repositories;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> orders = new();

    public Task SaveAsync(Order order, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        orders[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<Order?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }
}
