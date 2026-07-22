using K5.Assessment.Starter.Models;
using K5.Assessment.Starter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TaskStore>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Using .NET Minimal APIs

app.MapGet("/api/tasks", (TaskStore store) =>
{
    return Results.Ok(store.GetAll());
});

app.MapPost("/api/tasks", CreateTask);
app.MapPut("/api/tasks/{id:int}", UpdateTask);

app.MapFallbackToFile("index.html");

app.Run();

static IResult CreateTask(CreateTaskRequest request, TaskStore store)
{
    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest(new { message = "Title is required." });

    if (!TaskStore.IsValidPriority(request.Priority))
        return Results.BadRequest(new { message = "Priority must be Low, Normal, or High." });

    var task = store.Add(request.Title.Trim(), request.Priority);

    // 201 Created + Location header pointing at the new resource
    return Results.Created($"/api/tasks/{task!.Id}", task);
}

static IResult UpdateTask(int id, UpdateTaskRequest request, TaskStore store)
{
    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest(new { message = "Title is required." });

    if (!TaskStore.IsValidPriority(request.Priority))
        return Results.BadRequest(new { message = "Priority must be Low, Normal, or High." });

    var task = store.Update(id, request.Title.Trim(), request.Priority);

    if (task is null)
        return Results.NotFound(new { message = $"Task {id} was not found." });

    return Results.Ok(task);
}


