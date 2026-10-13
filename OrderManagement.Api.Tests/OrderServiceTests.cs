using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrderManagement.Api.Configuration;
using OrderManagement.Api.Models;
using OrderManagement.Api.Repositories;
using OrderManagement.Api.Services;

namespace OrderManagement.Api.Tests;

public sealed class OrderServiceTests
{
    private readonly InMemoryOrderRepository repository = new();
    private readonly OrderService service;

    public OrderServiceTests()
    {
        var options = Options.Create(new OrderOptions { ProcessingDelayMilliseconds = 0 });
        service = new OrderService(repository, options, NullLogger<OrderService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_StoresTrimmedOrderAsPendingPayment()
    {
        var order = await service.CreateAsync(
            new CreateOrderRequest("  customer-123  ", 49.95m),
            CancellationToken.None);

        Assert.Equal("customer-123", order.CustomerId);
        Assert.Equal(49.95m, order.Total);
        Assert.Equal(OrderStatus.PendingPayment, order.Status);
        Assert.Equal(order, await service.GetAsync(order.Id, CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_ReturnsNullForUnknownId()
    {
        Assert.Null(await service.GetAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenCancelled()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.CreateAsync(new CreateOrderRequest("customer-123", 10m), cts.Token));
    }
}
