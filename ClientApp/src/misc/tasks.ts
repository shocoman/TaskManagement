import {TaskNode, TaskStatus} from "./types";

export function parseTasksFromJson(jsonText: string): TaskNode[] {
    const parsedTask = JSON.parse(jsonText, (key, value) => {
        const isDate = ["creationDate", "completionDate", "lastStatusChangeDate"].includes(key);
        if (isDate && value !== null) {
            return new Date(value);
        }
        return value;
    });

    return Array.isArray(parsedTask) ? parsedTask : [parsedTask];
}

export function convertTaskToJson(task: TaskNode) {
    return JSON.stringify(task, (key, value) => {
        if (key === "status") {
            return parseInt(value);
        } else if (key === "subtasks") {
            return [];
        }
        return value;
    });
}


export function createNewDummyTask(parentId: number | null): TaskNode {
    const creationDate = new Date();
    return {
        id: -1,
        parentId: parentId,
        name: "New Task",
        details: "",
        assignees: "Nobody",
        status: TaskStatus.NotStarted,
        creationDate: creationDate,
        lastStatusChangeDate: creationDate,
        completionDate: null,
        plannedTime: 0,
        actualTime: 0,
        subtasks: []
    };
}

export function treeVisitor(tasks: TaskNode[], pred: (_: TaskNode, path: TaskNode[]) => void, path: TaskNode[] = []) {
    for (let [_i, task] of tasks.entries()) {
        path.push(task);
        pred(task, path);
        treeVisitor(task.subtasks, pred, path)
        path.pop();
    }
}

export function isValidStatusTransition(currentStatus: TaskStatus, nextStatus: TaskStatus) {
    let validDestinations = [];
    switch (nextStatus) {
        case TaskStatus.NotStarted:
            break;
        case TaskStatus.InProgress:
            validDestinations.push(TaskStatus.Suspended, TaskStatus.NotStarted);
            break;
        case TaskStatus.Suspended:
            validDestinations.push(TaskStatus.InProgress);
            break;
        case TaskStatus.Completed:
            validDestinations.push(TaskStatus.InProgress);
            break;
    }
    return nextStatus === currentStatus || validDestinations.includes(currentStatus);
}

export function tasksFilter(tasks: TaskNode[], pred: (_: TaskNode) => boolean): TaskNode[] {
    const newTasks: TaskNode[] = [];
    for (let task of tasks) {
        if (pred(task)) {
            const newTask = {...task};
            newTask.subtasks = tasksFilter(task.subtasks, pred);
            newTasks.push(newTask);
        }
    }
    return newTasks;
}
