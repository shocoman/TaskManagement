export enum TaskStatus {
    NotStarted, InProgress, Suspended, Completed
}

export interface TaskNode {
    id: number,
    parentId: number | null
    name: string
    details: string
    assignees: string
    creationDate: Date
    status: TaskStatus
    lastStatusChangeDate: Date,
    plannedTime: number
    actualTime: number
    completionDate: Date | null
    subtasks: TaskNode[]
}

