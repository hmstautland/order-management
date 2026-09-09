using OrderManagement.Api.Models;

namespace OrderManagement.Api.Services;

public interface IOrderService
{
    Task<Order> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken);
}
