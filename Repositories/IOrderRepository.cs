using OrderManagement.Api.Models;

namespace OrderManagement.Api.Repositories;

public interface IOrderRepository
{
    Task SaveAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> FindAsync(Guid id, CancellationToken cancellationToken);
}
