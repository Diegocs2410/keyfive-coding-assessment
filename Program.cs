using K5.Assessment.Starter.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TaskStore>();

// Registers the MVC action invoker, model binding and validation pipeline that
// controllers rely on. Minimal APIs needed none of this.
builder.Services.AddControllers();

// [ApiController] answers failed model validation with an RFC 7807
// ProblemDetails body. That is a sensible default, but wwwroot/js/api.js reads
// `data.message`, so adopting it wholesale would downgrade every validation
// error in the UI to the generic "Failed to add task.".
//
// Overriding the factory keeps the existing { message } contract intact, so the
// frontend needs no changes. The alternative — embracing ProblemDetails and
// teaching tryReadError to read `errors` — is the better long-term call for a
// public API, but it is a breaking change for existing clients.
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

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Routes now come from the [Route]/[HttpGet]/[HttpPost] attributes on
// TasksController instead of being listed here.
app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
