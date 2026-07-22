using K5.Assessment.Starter.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TaskStore>();

// Registers the MVC action invoker, model binding and validation pipeline that
// controllers rely on. Minimal APIs needed none of this.
builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Routes now come from the [Route]/[HttpGet]/[HttpPost] attributes on
// TasksController instead of being listed here.
app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
