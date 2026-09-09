namespace OrderManagement.Api.Models;

public sealed record CreateOrderRequest(string CustomerId, decimal Total);
