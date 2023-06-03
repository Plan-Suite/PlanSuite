export class TotalTasksByCompletionStatus {
    public tasksByCompletionStatus: Array<TaskByCompletionStatus>;
}

export class TaskByCompletionStatus {
    public status: string;
    public count: number;
}
