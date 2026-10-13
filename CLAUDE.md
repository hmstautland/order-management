# CLAUDE.md

Read `README.md` first: it covers what this project is, who it's for, its status, stack, and expected practices.
Then read `context.md`: it covers my role, defaults, and where AI is and isn't involved.

## Project
Order intake API (C# / .NET 8, ASP.NET Core Minimal API), demo/MVP heading to production. Only step 1 of the intended event-driven flow exists: create and retrieve orders. `OrderCreated`/`PaymentCompleted`/`PaymentFailed` events and the payment and notification workers are not implemented.

## Commands
- Build: `dotnet build`
- Run: `dotnet run` (see `README.md` for endpoints and curl examples)
- No test project yet.

## Architecture
Endpoint (`Program.cs`) -> `IOrderService` (`Services/`) -> `IOrderRepository` (`Repositories/`).
- `Program.cs` is the composition root: registers dependencies, binds `OrderOptions`, maps `POST /orders` and `GET /orders/{id:guid}`.
- Lifetimes: `IOrderRepository` is a singleton (`InMemoryOrderRepository`, `ConcurrentDictionary`); `IOrderService` is scoped.
- Models in `Models/` are immutable records (`Order`, `CreateOrderRequest`).
- Settings live in `Configuration/` (`OrderOptions`, section `Orders`, `ProcessingDelayMilliseconds`) and are injected via `IOptions<T>`.
- Input validation happens at the endpoint (`Program.cs`) and returns `Results.ValidationProblem`.
- A `CancellationToken` flows from the endpoint through the service to `Task.Delay` and the repository.

## Conventions
- File-scoped namespaces rooted at `OrderManagement.Api.<Folder>`.
- Sealed implementations; each `I*` interface sits beside its implementation.
- Async end to end, no `.Result`/`.Wait()`; structured log templates via `ILogger<T>`.
- Contracts used across files (endpoint, interface, model, event, configuration, registration lifetime) are design: ask first (see `context.md`).
- Never commit or push; I review and commit.

## Known gaps
- `"PendingPayment"` is a magic string hard-coded in `OrderService.CreateAsync`. It is a payment-stage value.
- `Order.Status` is a `string`. Order status will span several stages (e.g. payment, shipping/delivery), so don't model it from `"PendingPayment"` alone.
- Changing the type of `Order.Status` is a contract change: ask first. The JSON the API returns must not change.
- Storage is in-memory only. The storage decision is mine; don't connect to or assume real storage.
- No event publishing or workers yet, and no tests.
## Flagged/Weeknes
Validation location: it's inline in the endpoint, with a single combined error under the key "request". Per-field keys (customerId, total) would be friendlier to clients. This would also be a clean place for an endpoint filter later.
Delay in the request path: Task.Delay before saving is a simulated delay. It should not survive to production.
Log level: the created-order log in the endpoint is Information, while the service logs at Debug. The two overlap and could be consolidated.
No idempotency: a client retry creates a duplicate order. A real intake API probably wants an idempotency key.
Persistence and events: SaveAsync followed by a future OrderCreated publish is a dual-write problem. Plan an outbox, or accept the inconsistency window, before adding events.
Storage: in-memory only. The singleton plus ConcurrentDictionary is thread-safe but loses data on restart. This is the owner's decision, so I'm not assuming a store.
Status change in the staged diff: Order.Status moved from string to an OrderStatus enum. JsonStringEnumConverter registered globally keeps the JSON output as a string, which meets the "JSON must not change" rule. Three consequences:
The enum currently has only PendingPayment, so it isn't yet modelled for shipping or delivery stages.
The converter is global, so it affects every enum in the API. Putting [JsonConverter] on the type instead would scope it.
Unknown values on deserialization will throw, which matters once other services consume this JSON.
Cleanup: Program.cs has an unused using Microsoft.Extensions.Options;.
Tests: new test files are staged, but I haven't run them.