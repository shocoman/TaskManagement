import {TaskNode} from "@/misc/types";
import {convertTaskToJson, createNewDummyTask, parseTasksFromJson} from "@/misc/tasks";


const apiUrl = "https://localhost:7109/api";

export async function getAllTasks() {
    const tasksResponse = await fetch(`${apiUrl}/task`);
    if (tasksResponse.ok) {
        const jsonText = await tasksResponse.text();
        return parseTasksFromJson(jsonText);
    }
    return [];
}

export async function getTask(id: number) {
    const tasksResponse = await fetch(`${apiUrl}/task/${id}`);
    if (tasksResponse.ok) {
        const jsonText = await tasksResponse.text();
        return parseTasksFromJson(jsonText)[0];
    }
    return null;
}

export async function addNewTask(parentId: number | null) {
    let newTask = createNewDummyTask(parentId);

    const response = await fetch(`${apiUrl}/task`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: convertTaskToJson(newTask)
    });

    if (response.ok) {
        const responseText = await response.text();
        return parseTasksFromJson(responseText)[0];
    } else {
        return null;
    }
}

export async function updateTask(id: number, updatedTask: TaskNode) {
    const response = await fetch(`${apiUrl}/task/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: convertTaskToJson(updatedTask)
    });

    return response.ok;
}

export async function finishTaskTree(id: number, updatedTask: TaskNode) {
    const response = await fetch(`${apiUrl}/Task/FinishTree/${id}`, {
        method: "PATCH",
        headers: {
            "Content-Type": "application/json",
        },
        body: convertTaskToJson(updatedTask)
    });

    return response.ok;
}

export async function deleteTask(id: number) {
    const response = await fetch(`${apiUrl}/task/${id}`, {
        method: "DELETE"
    });

    return response.ok;
}