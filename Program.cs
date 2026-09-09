using Microsoft.Extensions.Options;
using OrderManagement.Api.Configuration;
using OrderManagement.Api.Models;
using OrderManagement.Api.Repositories;
using OrderManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OrderOptions>(builder.Configuration.GetSection(OrderOptions.SectionName));
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.MapPost("/orders", async (
    CreateOrderRequest request,
    IOrderService orders,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.CustomerId) || request.Total <= 0)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["request"] = ["CustomerId is required and Total must be greater than zero."]
        });
    }

    var order = await orders.CreateAsync(request, cancellationToken);
    logger.LogInformation(
        "Created order {OrderId} for customer {CustomerId} with total {Total}",
        order.Id,
        order.CustomerId,
        order.Total);

    return Results.Created($"/orders/{order.Id}", order);
});

app.MapGet("/orders/{id:guid}", async (
    Guid id,
    IOrderService orders,
    CancellationToken cancellationToken) =>
{
    var order = await orders.GetAsync(id, cancellationToken);
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.Run();
