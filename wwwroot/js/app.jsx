const { useEffect, useState } = React;

function App() {
    const [tasks, setTasks] = useState([]);
    const [title, setTitle] = useState("");
    const [priority, setPriority] = useState("Normal");
    const [isLoading, setIsLoading] = useState(true);
    const [isSaving, setIsSaving] = useState(false);
    const [editingTask, setEditingTask] = useState(null);
    const [error, setError] = useState("");

    useEffect(() => {
        loadTasks();
    }, []);

    async function loadTasks() {
        setIsLoading(true);
        setError("");

        try {
            const data = await window.taskApi.getTasks();
            setTasks(data);
        } catch (err) {
            setError(err.message || "Unable to load tasks.");
        } finally {
            setIsLoading(false);
        }
    }

    async function handleAddTask(event) {
        event.preventDefault();

        if (!title.trim()) {
            setError("Please enter a task title.");
            return;
        }

        setIsSaving(true);
        setError("");

        try {
            const created = await window.taskApi.addTask({ title: title.trim(), priority });
            setTasks(current => [...current, created]);
            setTitle("");
            setPriority("Normal");
        } catch (err) {
            setError(err.message || "Unable to add task.");
        } finally {
            setIsSaving(false);
        }
    }

    async function handleSaveEdit(event) {
        event.preventDefault();

        if (!editingTask.title.trim()) {
            setError("Please enter a task title.");
            return;
        }

        setIsSaving(true);
        setError("");

        try {
            const updated = await window.taskApi.updateTask(editingTask.id, {
                title: editingTask.title.trim(),
                priority: editingTask.priority
            });
            setTasks(current => current.map(task => (task.id === updated.id ? updated : task)));
            setEditingTask(null);
        } catch (err) {
            setError(err.message || "Unable to save changes.");
        } finally {
            setIsSaving(false);
        }
    }

    return (
        <div className="app-shell">
            <div className="page-card">
                <h1 className="page-title">K5 Assessment Starter</h1>
                <p className="page-subtitle">
                    React frontend with an ASP.NET Core API. The add and edit UI is already in place,
                    but the behavior is intentionally unfinished.
                </p>

                {error && <div className="message error" role="alert">{error}</div>}
                {isLoading && <div className="message info" role="status">Loading tasks...</div>}

                <form className="form-grid" onSubmit={handleAddTask}>
                    <input
                        className="input"
                        placeholder="Add a task title"
                        aria-label="New task title"
                        value={title}
                        onChange={event => setTitle(event.target.value)}
                    />

                    <select
                        className="select"
                        aria-label="New task priority"
                        value={priority}
                        onChange={event => setPriority(event.target.value)}
                    >
                        <option value="Low">Low</option>
                        <option value="Normal">Normal</option>
                        <option value="High">High</option>
                    </select>

                    <button className="primary-button" type="submit" disabled={isSaving}>
                        {isSaving ? "Saving..." : "Add Task"}
                    </button>
                </form>

                <div className="task-list">
                    {!isLoading && tasks.length === 0 && (
                        <div className="empty-state">No tasks to display.</div>
                    )}

                    {tasks.map(task => (
                        <TaskRow
                            key={task.id}
                            task={task}
                            onEdit={() => setEditingTask({ ...task })}
                        />
                    ))}
                </div>

                {editingTask && (
                    <form className="page-card" style={{ marginTop: 20, padding: 18 }} onSubmit={handleSaveEdit}>
                        <h3 style={{ marginTop: 0 }}>Edit Task</h3>

                        <div className="form-grid" style={{ marginBottom: 12 }}>
                            <input
                                className="input"
                                aria-label="Task title"
                                value={editingTask.title}
                                onChange={event => setEditingTask(current => ({ ...current, title: event.target.value }))}
                            />

                            <select
                                className="select"
                                aria-label="Task priority"
                                value={editingTask.priority}
                                onChange={event => setEditingTask(current => ({ ...current, priority: event.target.value }))}
                            >
                                <option value="Low">Low</option>
                                <option value="Normal">Normal</option>
                                <option value="High">High</option>
                            </select>
                        </div>

                        <div className="actions">
                            <button className="primary-button" type="submit" disabled={isSaving}>
                                {isSaving ? "Saving..." : "Save Changes"}
                            </button>

                            <button className="secondary-button" type="button" onClick={() => setEditingTask(null)}>
                                Cancel
                            </button>
                        </div>
                    </form>
                )}

                <div className="help-note">
                    Goals: wire up add/edit, validation, and polish.
                </div>
            </div>
        </div>
    );
}

function TaskRow({ task, onEdit }) {
    const badgeClass = `badge ${task.priority.toLowerCase()}`;

    return (
        <div className="task-card">
            <div>
                <div className={`task-title ${task.isComplete ? "complete" : ""}`}>
                    {task.title}
                </div>

                <div className="task-meta">
                    <span className={badgeClass}>{task.priority} priority</span>

                    <span className={`status ${task.isComplete ? "done" : "active"}`}>
                        <span className="status-icon" aria-hidden="true">
                            {task.isComplete ? "✓" : "○"}
                        </span>
                        {task.isComplete ? "Completed" : "Active"}
                    </span>
                </div>
            </div>

            <div className="actions">
                <button
                    className="small-button"
                    type="button"
                    onClick={onEdit}
                    aria-label={`Edit task: ${task.title}`}
                >
                    Edit
                </button>
            </div>
        </div>
    );
}

ReactDOM.createRoot(document.getElementById("root")).render(<App />);
