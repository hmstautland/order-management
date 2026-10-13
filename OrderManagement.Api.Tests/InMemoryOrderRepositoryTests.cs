using OrderManagement.Api.Models;
using OrderManagement.Api.Repositories;

namespace OrderManagement.Api.Tests;

public sealed class InMemoryOrderRepositoryTests
{
    private static Order NewOrder() =>
        new(Guid.NewGuid(), "customer-123", 10m, OrderStatus.PendingPayment, DateTimeOffset.UtcNow);

    [Fact]
    public async Task FindAsync_ReturnsSavedOrder()
    {
        var repository = new InMemoryOrderRepository();
        var order = NewOrder();

        await repository.SaveAsync(order, CancellationToken.None);

        Assert.Equal(order, await repository.FindAsync(order.Id, CancellationToken.None));
    }

    [Fact]
    public async Task FindAsync_ReturnsNullForUnknownId()
    {
        var repository = new InMemoryOrderRepository();

        Assert.Null(await repository.FindAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Operations_ThrowWhenCancelled()
    {
        var repository = new InMemoryOrderRepository();
        var order = NewOrder();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            repository.SaveAsync(order, new CancellationToken(canceled: true)));
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            repository.FindAsync(order.Id, new CancellationToken(canceled: true)));
    }
}
