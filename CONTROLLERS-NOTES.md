# Minimal APIs vs. Web API Controllers

A side-by-side of the same five task endpoints, implemented both ways.

- `feature/new-features` — minimal APIs (the shipped solution)
- `demo/web-api-controllers` — the same API rebuilt on controllers

## A note on terminology

This is **not MVC**. Full MVC is Model + View + Controller, where the View is
server-rendered (Razor). Here the view layer is React, so only the Controller
half applies — the accurate name is **Web API with controllers**.

## What actually changed

| File | Change |
|---|---|
| `Controllers/TasksController.cs` | new — five actions, attribute routing |
| `Models/CreateTaskRequest.cs` | validation moved to DataAnnotations |
| `Models/UpdateTaskRequest.cs` | validation moved to DataAnnotations |
| `Program.cs` | `AddControllers()` + `MapControllers()`; route table removed |
| `Services/TaskStore.cs` | **unchanged** |
| `wwwroot/**` | **unchanged** |

The last two rows are the point of the exercise: choosing between minimal APIs
and controllers is a **delivery-layer** decision. The business logic and the
HTTP contract never noticed.

## Same routes, different declaration

```csharp
// minimal API — routing lives in Program.cs
app.MapPut("/api/tasks/{id:int}/toggle", ToggleTaskComplete);

static IResult ToggleTaskComplete(int id, TaskStore store) { ... }
```

```csharp
// controller — routing lives next to the handler
[HttpPut("{id:int}/toggle")]
public ActionResult<TaskItem> ToggleComplete(int id) { ... }
```

Dependency injection is identical; only the injection point moved from a method
parameter to a constructor.

## The regression this migration introduced

`[ApiController]` validates DataAnnotations before the action body runs and
answers failures with **RFC 7807 ProblemDetails**:

```jsonc
// before — what wwwroot/js/api.js expects
{ "message": "Priority must be Low, Normal, or High." }

// after [ApiController], by default
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Priority": ["Priority must be Low, Normal, or High."] }
}
```

`tryReadError` reads `data.message || data.error`. Both are `undefined` here, so
every validation error silently degraded to the generic *"Failed to add task."*
The real reason was still on the wire — just under `errors.Priority[0]`.

404s were unaffected: they are returned explicitly from the action, not
produced by model validation.

### The fix

```csharp
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = context.ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text))
            ?? "Invalid request.";

        return new BadRequestObjectResult(new { message });
    };
});
```

The alternative is to adopt ProblemDetails and teach `tryReadError` to walk
`errors`. That is the better long-term call for a public API — it is a
standard, it carries per-field detail, and tooling understands it — but it is a
breaking change for existing clients. Preserving `{ message }` was the right
call here because the frontend is part of the same deliverable and rewriting it
buys nothing.

## Gotcha: DataAnnotations on positional records

```csharp
// throws InvalidOperationException at request time
public record CreateTaskRequest([property: Required] string Title = "");

// correct — the attribute belongs on the constructor parameter
public record CreateTaskRequest([Required] string Title = "");
```

MVC's validator rejects metadata found on the generated property of a record.
It compiles either way; it only fails when a request arrives.

Related: `[Required(AllowEmptyStrings = false)]` trims before checking, so it
already rejects `"   "`. The manual `IsNullOrWhiteSpace` guard kept in the
controller is redundant defence, not the mechanism.

## Trade-offs

| | Minimal APIs | Controllers |
|---|---|---|
| Ceremony | lowest — a lambda and a route | class, attributes, constructor |
| Throughput | faster, less allocation per request | action invoker + filter overhead |
| Grouping | manual (`MapGroup`) | natural, one class per resource |
| Cross-cutting concerns | per-endpoint filters | `[Authorize]`, action filters per class |
| Validation | manual, or a library like FluentValidation | DataAnnotations, automatic |
| Error shape | fully yours | ProblemDetails by default |
| OpenAPI / versioning | supported, less ergonomic | first-class tooling |
| Unit testing | call the static handler directly | instantiate the controller, inject fakes |

**When each wins:** minimal APIs for small surfaces, microservices and
high-throughput paths. Controllers once there are many resources sharing
authorization, filters, versioning and generated documentation.

For five endpoints against an in-memory store, minimal APIs are the right
call — which is why `main` keeps them.

## Verified behaviour

Every response on this branch is byte-identical to `feature/new-features`:

| Request | Status | Body |
|---|---|---|
| `GET /api/tasks` | 200 | three seeded tasks, sorted |
| `POST` valid | 201 | created task + `Location` header |
| `POST` blank title | 400 | `{"message":"Title is required."}` |
| `POST` priority `"Urgente"` | 400 | `{"message":"Priority must be Low, Normal, or High."}` |
| `PUT /api/tasks/2` | 200 | updated task, `IsComplete` preserved |
| `PUT /api/tasks/999` | 404 | `{"message":"Task 999 was not found."}` |
| `PUT /api/tasks/1/toggle` | 200 | task with `IsComplete` flipped |
| `DELETE /api/tasks/3` | 200 | the deleted task |

## Open questions worth raising

- **`PUT /toggle` is not idempotent.** PUT is required to be idempotent by
  spec; a toggle depends on prior state, so calling it twice is a no-op round
  trip. `PUT /api/tasks/{id}/status` with `{ "isComplete": true }` states the
  desired outcome and is safely repeatable.
- **`PUT /api/tasks/{id}` behaves like PATCH.** `UpdateTaskRequest` omits
  `IsComplete` and the handler preserves it with `with`. A strict PUT replaces
  the resource and would reset that flag. Either add `IsComplete` to the DTO or
  map the route as `PATCH`.
- **`TaskStore` is not thread-safe.** It is a singleton mutating a `List<T>`.
  Fine for an assessment; concurrent requests in production need a lock or a
  `ConcurrentDictionary`.
