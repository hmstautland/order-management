# Order Management API

A small .NET 8 Minimal API demonstrating dependency injection, configuration, structured logging, cancellation tokens, and asynchronous code.

## Run

```bash
dotnet run
```

The API listens on the URL printed by ASP.NET Core, usually `http://localhost:5000` or `https://localhost:7000`.

## Endpoints

Create an order:

```bash
curl -X POST http://localhost:5000/orders \
  -H 'Content-Type: application/json' \
  -d '{"customerId":"customer-123","total":49.95}'
```

Retrieve an order:

```bash
curl http://localhost:5000/orders/{id}
```

`Orders:ProcessingDelayMilliseconds` is configured in `appsettings.json` and injected with `IOptions<OrderOptions>`.

## Project structure

- `Program.cs` is the composition root: it registers dependencies and maps HTTP endpoints.
- `Models/` contains request and response/domain records.
- `Configuration/` contains strongly typed application settings.
- `Services/` contains order use cases and the service abstraction.
- `Repositories/` contains persistence abstractions and the in-memory implementation.

Dependency injection is configured in `Program.cs`:

```csharp
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
```

ASP.NET Core creates a `CancellationToken` for each request when the endpoint declares one. It is canceled when the client disconnects, for example when the caller cancels a request or closes its connection. It can also be canceled by a server-side request timeout. The token is passed from the endpoint to `OrderService`, then to `Task.Delay` and the repository. Those operations stop by throwing `OperationCanceledException`, so work does not continue after the request is gone.
