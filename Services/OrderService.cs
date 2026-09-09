using Microsoft.Extensions.Options;
using OrderManagement.Api.Configuration;
using OrderManagement.Api.Models;
using OrderManagement.Api.Repositories;

namespace OrderManagement.Api.Services;

public sealed class OrderService(
    IOrderRepository repository,
    IOptions<OrderOptions> options,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<Order> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        await Task.Delay(options.Value.ProcessingDelayMilliseconds, cancellationToken);

        var order = new Order(
            Guid.NewGuid(),
            request.CustomerId.Trim(),
            request.Total,
            "PendingPayment",
            DateTimeOffset.UtcNow);

        await repository.SaveAsync(order, cancellationToken);
        logger.LogDebug("Order {OrderId} stored with status {Status}", order.Id, order.Status);
        return order;
    }

    public Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        repository.FindAsync(id, cancellationToken);
}
