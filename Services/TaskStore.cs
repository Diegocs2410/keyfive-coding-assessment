using K5.Assessment.Starter.Models;

namespace K5.Assessment.Starter.Services;

public class TaskStore
{
    readonly List<TaskItem> tasks =
    [
        new TaskItem(1,"Review quarterly report", "High", false),
        new TaskItem(2,"Send follow-up email", "Normal", true),
        new TaskItem(3,"Schedule demo call", "Low", false)
    ];

    int nextId = 4;

    public IReadOnlyList<TaskItem> GetAll()
    {
        return tasks
            .OrderByDescending(x => PriorityRank(x.Priority))
            .ThenBy(x => x.Title)
            .ToList();
    }

    public TaskItem? Add(string title, string priority)
    {
        var task = new TaskItem(nextId++, title, priority, false);
        tasks.Add(task);
        return task;
    }

    public TaskItem? Update(int id, string title, string priority)
    {
        var index = tasks.FindIndex(t => t.Id == id);
        if (index < 0)
            return null;

        // records are immutable, so we build a copy with the new values
        // (`with` keeps the existing IsComplete flag untouched)
        var updated = tasks[index] with { Title = title, Priority = priority };
        tasks[index] = updated;
        return updated;
    }

    public TaskItem? ToggleComplete(int id)
    {
        var index = tasks.FindIndex(t => t.Id == id);
        if (index < 0)
            return null;
        // records are immutable, so we build a copy with the new values
        var updated = tasks[index] with { IsComplete = !tasks[index].IsComplete };
        tasks[index] = updated;
        return updated;
    }

    public TaskItem? Delete(int id)
    {
        var index = tasks.FindIndex(t => t.Id == id);
        if (index < 0)
            return null;
        var deleted = tasks[index];
        tasks.RemoveAt(index);
        return deleted;
    }

    public static bool IsValidPriority(string? value)
    {
        return value is "Low" or "Normal" or "High";
    }

    static int PriorityRank(string priority)
    {
        return priority switch
        {
            "High" => 3,
            "Normal" => 2,
            "Low" => 1,
            _ => 0
        };
    }
}
