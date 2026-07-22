window.taskApi = {
    async getTasks() {
        const response = await fetch("/api/tasks");

        if (!response.ok) {
            throw new Error("Failed to load tasks.");
        }

        return await response.json();
    },

    async addTask(request) {
        const response = await fetch("/api/tasks", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        });

        if (!response.ok) {
            const message = await tryReadError(response);
            throw new Error(message || "Failed to add task.");
        }

        return await response.json();
    },

    async updateTask(id, request) {
        const response = await fetch(`/api/tasks/${id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        });

        if (!response.ok) {
            const message = await tryReadError(response);
            throw new Error(message || "Failed to update task.");
        }

        return await response.json();
    },

    async deleteTask(id) {
        const response = await fetch(`/api/tasks/${id}`, {
            method: "DELETE"
        });

        if (!response.ok) {
            const message = await tryReadError(response);
            throw new Error(message || "Failed to delete task.");
        }

        return await response.json();
    },

    async toggleTask(id) {
        const response = await fetch(`/api/tasks/${id}/toggle`, {
            method: "PUT"
        });

        if (!response.ok) {
            const message = await tryReadError(response);
            throw new Error(message || "Failed to toggle task.");
        }

        return await response.json();
    }
};

async function tryReadError(response) {
    try {
        const data = await response.json();
        return data.message || data.error || "";
    } catch {
        return "";
    }
}
