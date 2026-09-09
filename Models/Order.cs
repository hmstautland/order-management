namespace OrderManagement.Api.Models;

public sealed record Order(
    Guid Id,
    string CustomerId,
    decimal Total,
    string Status,
    DateTimeOffset CreatedAt);
