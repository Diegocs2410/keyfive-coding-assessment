using K5.Assessment.Starter.Models;
using K5.Assessment.Starter.Services;
using Microsoft.AspNetCore.Mvc;

namespace K5.Assessment.Starter.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    readonly TaskStore store;

    // The DI container resolves TaskStore the same way it did for the minimal
    // API handlers; only the injection point moved from a parameter to a
    // constructor.
    public TasksController(TaskStore store)
    {
        this.store = store;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<TaskItem>> GetAll()
    {
        return Ok(store.GetAll());
    }

    [HttpPost]
    public ActionResult<TaskItem> Create(CreateTaskRequest request)
    {
        // Title/Priority shape is now enforced by the DataAnnotations on the
        // DTO, which [ApiController] validates before this body ever runs.
        // Whitespace-only titles still need a manual guard: [Required] accepts
        // "   " because it is not an empty string.
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title is required." });

        var task = store.Add(request.Title.Trim(), request.Priority);

        return Created($"/api/tasks/{task!.Id}", task);
    }

    [HttpPut("{id:int}")]
    public ActionResult<TaskItem> Update(int id, UpdateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title is required." });

        var task = store.Update(id, request.Title.Trim(), request.Priority);

        if (task is null)
            return NotFound(new { message = $"Task {id} was not found." });

        return Ok(task);
    }

    [HttpPut("{id:int}/toggle")]
    public ActionResult<TaskItem> ToggleComplete(int id)
    {
        var task = store.ToggleComplete(id);

        if (task is null)
            return NotFound(new { message = $"Task {id} was not found." });

        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    public ActionResult<TaskItem> Delete(int id)
    {
        var task = store.Delete(id);

        if (task is null)
            return NotFound(new { message = $"Task {id} was not found." });

        return Ok(task);
    }
}
