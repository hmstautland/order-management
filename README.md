# Order Management API

A .NET 8 Minimal API demonstrating dependency injection, configuration, structured logging, cancellation tokens, and asynchronous code.

## Abstract

Taking an order should be fast and reliable, and should not depend on payment or notification work finishing first. This service handles the intake step: a customer places an order, it is validated and stored with the status `PendingPayment`, and the caller gets an immediate response. Work stops if the client disconnects, so abandoned requests do not keep consuming resources.

Today the API supports creating and retrieving orders, backed by an in-memory store. The intended flow extends this into an event-driven process:

1. Customer places an order.
2. An `OrderCreated` event is published.
3. A payment worker processes the payment.
4. A `PaymentCompleted` or `PaymentFailed` event is published.
5. A notification worker sends the customer a confirmation.

Only step 1 is implemented so far.

## Context

- **Users:** UIs for customers, for example an e-commerce front end calling the API.
- **Stage:** demo/MVP. After the first week it will be refined and taken to production, so code is written production-minded.
- **Team and timeline:** solo-ish, about 1 week. Others will be involved before production.
- **Stack:** C# / .NET 8, ASP.NET Core Minimal API.

### Expected practices

Not hard rules, but expected in the code:

- Pass a `CancellationToken` through request-scoped async calls.
- Dependencies via constructor injection, registered at the composition root.
- Bind settings to typed options and inject them via `IOptions<T>` rather than reading configuration directly.
- Log with structured templates through `ILogger<T>`, not string interpolation.
- Use `async`/`await` end to end through every layer, even where the current implementation does no real I/O, so later changes don't alter signatures. No blocking on tasks (`.Result`, `.Wait()`).
- Validate input at the API boundary.

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
