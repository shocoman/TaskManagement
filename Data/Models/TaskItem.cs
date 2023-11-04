using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using TaskManagement.Data.Models.DTOs;

namespace TaskManagement.Data.Models;

public class TaskItem
{
    [JsonPropertyName("id")] 
    public int TaskId { get; set; }
    public int? ParentId { get; set; }

    public string Name { get; set; }
    public string Details { get; set; }
    public string Assignees { get; set; }

    public DateTime CreationDate { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime LastStatusChangeDate { get; set; }
    public long PlannedTime { get; set; }

    public long ActualTime { get; set; }
    public DateTime? CompletionDate { get; set; }

    public List<TaskItem> Subtasks { get; set; }


    public static TaskItem ConvertFromDto(TaskItemDto itemDto) =>
        new()
        {
            TaskId = itemDto.TaskId,
            ParentId = itemDto.ParentId,
            Name = itemDto.Name,
            Details = itemDto.Details,
            Assignees = itemDto.Assignees,
            CreationDate = itemDto.CreationDate,
            Status = itemDto.Status,
            LastStatusChangeDate = itemDto.LastStatusChangeDate,
            PlannedTime = itemDto.PlannedTime,
            ActualTime = itemDto.ActualTime,
            CompletionDate = itemDto.CompletionDate,
            Subtasks = new List<TaskItem>()
        };

    public static TaskItemDto ConvertIntoDto(TaskItem taskItem, TaskItemDto parent)
    {
        string path = parent == null ? "/" : $"{parent.Path}{parent.TaskId}/";
        return new TaskItemDto
        {
            TaskId = taskItem.TaskId,
            ParentId = taskItem.ParentId,
            Path = path,
            Name = taskItem.Name,
            Details = taskItem.Details,
            Assignees = taskItem.Assignees,
            CreationDate = taskItem.CreationDate,
            Status = taskItem.Status,
            LastStatusChangeDate = taskItem.LastStatusChangeDate,
            PlannedTime = taskItem.PlannedTime,
            ActualTime = taskItem.ActualTime,
            CompletionDate = taskItem.CompletionDate
        };
    }
}
