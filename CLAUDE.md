# CLAUDE.md

Read `README.md` first: it covers what this project is, who it's for, its status, stack, and expected practices.
Then read `context.md`: it covers my role, defaults, and where AI is and isn't involved.

## Commands
- Build: `dotnet build`
- Run: see `README.md`
- No test project yet.

## Architecture
Endpoint (`Program.cs`) -> `IOrderService` (`Services/`) -> `IOrderRepository` (`Repositories/`).
- Models in `Models/` are immutable records.

## Conventions
- File-scoped namespaces rooted at `OrderManagement.Api.<Folder>`.
- Sealed implementations; each `I*` interface sits beside its implementation.

## Known gaps
- Order status is a magic string (`"PendingPayment"`).
