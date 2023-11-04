using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Data.Models.DTOs;

public class TaskItemDto
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TaskId { get; set; }
    public int? ParentId { get; set; }
    
    public string Path { get; set; }
    
    public string Name { get; set; }
    public string Details { get; set; }
    public string Assignees { get; set; }

    public DateTime CreationDate { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime LastStatusChangeDate { get; set; }
    public long PlannedTime { get; set; }

    public long ActualTime { get; set; }
    public DateTime? CompletionDate { get; set; }
}
