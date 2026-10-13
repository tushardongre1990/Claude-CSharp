# C#/.NET Backend Interview Prep — 60 Days

A self-contained course that takes you from zero to **interview-ready at 5-years-of-experience depth** for C#/.NET backend roles.

- **Scope:** backend only. DSA, HLD and LLD are covered in separate repos.
- **Stack:** C# 14, .NET 10 (with notes on differences from .NET 6/8), ASP.NET Core, EF Core, SQL Server, Azure (primary) and AWS (overview).
- **How to use it:** read a chapter's `notes.md` (everything, including the full code, is inside it), run the code in its `code/` folder, then test yourself with `quiz.md` and `exercises.md`.

## Study rules

1. For each chapter: read the notes, run the code, take the quiz, do the exercises.
2. Write every mistake in [MISTAKES.md](MISTAKES.md). Revisit it every weekend.
3. If you fall behind, **cut cloud and advanced framework depth first**. Never cut SQL, async/await, API security or mock interviews.
4. **Start applying for jobs around Day 35–40.** Don't wait for the end.
5. Don't add more chapters unless a real interview exposes a gap.

## Progress tracker

| # | Chapter | Days | Status |
|---|---|---|---|
| | **Phase 1 — C# fundamentals and runtime** | | |
| 1 | [C# fundamentals and OOP](chapters/01-csharp-fundamentals-oop/notes.md) | 1–3 | Notes ready — quiz pending |
| 2 | [Collections, generics and exceptions](chapters/02-collections-generics-exceptions/notes.md) | 4–6 | Not started |
| 3 | [LINQ, delegates and advanced C#](chapters/03-linq-delegates-advanced-csharp/notes.md) | 7–8 | Not started |
| 4 | [.NET runtime and memory management](chapters/04-dotnet-runtime-memory/notes.md) | 9–10 | Not started |
| 5 | [Async/await, threading and concurrency](chapters/05-async-threading-concurrency/notes.md) | 11–13 | Not started |
| | **Phase 2 — ASP.NET Core and API development** | | |
| 6 | [ASP.NET Core, middleware and DI](chapters/06-aspnetcore-middleware-di/notes.md) | 14–15 | Not started |
| 7 | [REST APIs, model binding and validation](chapters/07-rest-apis-binding-validation/notes.md) | 16–17 | Not started |
| 8 | [Filters, error handling and observability](chapters/08-filters-errors-observability/notes.md) | 18–19 | Not started |
| 9 | [Authentication, authorization and security](chapters/09-authentication-authorization-security/notes.md) | 20–21 | Not started |
| 10 | [Common backend libraries and protocols](chapters/10-libraries-protocols/notes.md) | 22 | Not started |
| | **Phase 3 — EF Core and SQL Server** | | |
| 11 | [Entity Framework Core](chapters/11-ef-core/notes.md) | 23–25 | Not started |
| 12 | [SQL fundamentals and normalization](chapters/12-sql-fundamentals-normalization/notes.md) | 26–27 | Not started |
| 13 | [SQL interview query problems](chapters/13-sql-query-problems/notes.md) | 28 | Not started |
| 14 | [SQL Server performance](chapters/14-sql-server-performance/notes.md) | 29–31 | Not started |
| 15 | [Transactions and data consistency](chapters/15-transactions-consistency/notes.md) | 32–33 | Not started |
| 16 | [Dapper and ADO.NET](chapters/16-dapper-adonet/notes.md) | 34 | Not started |
| | **Phase 4 — Testing, caching and reliability** | | |
| 17 | [Testing .NET applications](chapters/17-testing/notes.md) | 35–36 | Not started |
| 18 | [Caching](chapters/18-caching/notes.md) | 37 | Not started |
| 19 | [Background jobs and messaging](chapters/19-background-jobs-messaging/notes.md) | 38–39 | Not started |
| 20 | [External API calls and resilience](chapters/20-http-resilience/notes.md) | 40–41 | Not started |
| | **Phase 5 — Production engineering and cloud** | | |
| 21 | [Performance and production diagnostics](chapters/21-performance-diagnostics/notes.md) | 42–43 | Not started |
| 22 | [Maintainable backend code](chapters/22-maintainable-code/notes.md) | 44 | Not started |
| 23 | [Docker, CI/CD and Kubernetes fundamentals](chapters/23-docker-cicd-kubernetes/notes.md) | 45–46 | Not started |
| 24 | [Azure (primary cloud)](chapters/24-azure/notes.md) | 47 | Not started |
| 25 | [AWS (secondary cloud)](chapters/25-aws/notes.md) | 48 | Not started |
| | **Phase 6 — Final interview preparation** | | |
| 26 | [Scenario-based troubleshooting](chapters/26-scenario-troubleshooting/notes.md) | 49–51 | Not started |
| 27 | [Advanced C# and ASP.NET Core revision](chapters/27-revision-csharp-aspnetcore/notes.md) | 52–53 | Not started |
| 28 | [SQL, EF Core and security revision](chapters/28-revision-sql-efcore-security/notes.md) | 54–55 | Not started |
| 29 | [Mock interviews and project walkthrough](chapters/29-mock-interviews/notes.md) | 56–58 | Not started |
| | Buffer and project review | 59 | — |
| | Final revision | 60 | — |

## Project: Order Management API

Lives in [project/OrderManagementApi/](project/OrderManagementApi/). Build it alongside the chapters.

- [ ] **API foundation** (Days 14–17): CRUD endpoints, DTOs, validation, DI, consistent error responses
- [ ] **Security and API behavior** (Days 18–22): JWT, resource-level authorization, **custom action filter**, OpenAPI docs
- [ ] **Database and query performance** (Days 23–34): EF Core migrations, relationships, transactions, **one read endpoint in both Dapper and EF Core**
- [ ] **Tests and reliability** (Days 35–41): unit and integration tests, caching, a background worker or message consumer, idempotency, resilient HTTP calls
- [ ] **Deployment** (Days 42–48): Docker, CI/CD pipeline, Azure deployment design, AWS service mapping
- [ ] **Interview demonstration** (Days 49–60): explain design decisions, troubleshoot failures, show tests, discuss trade-offs

## Study depth

| Study deeply | Learn at interview level | Overview only |
|---|---|---|
| C#, OOP, collections, LINQ | Reflection and expression trees | Native AOT details |
| Async/await and concurrency | PLINQ | Deep GC tuning |
| ASP.NET Core, DI, filters | gRPC and SignalR | Kubernetes cluster administration |
| JWT, authorization, API security | MediatR and mapping libraries | Advanced cloud networking |
| EF Core and SQL Server | Dapper and stored procedures | Specialized Azure/AWS services |
| Transactions and indexes | OpenTelemetry tooling | Distributed transaction internals |
| Testing and debugging | HybridCache, modern resilience APIs | Broker internals |
