using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data.Models;
using TaskManagement.Data.Models.DTOs;
using TaskManagement.Exceptions;
using TaskStatus = TaskManagement.Data.Models.TaskStatus;

namespace TaskManagement.Data.Repositories;

public class TaskRepository
{
    private readonly TaskDbContext context;

    public TaskRepository(TaskDbContext ctx)
    {
        context = ctx;
    }

    private async Task FinishTree(int taskId, DateTime taskCompletionDate) =>
        await context.Tasks
            .Where(t => taskId == t.TaskId || t.Path.Contains("/" + taskId + "/"))
            .ForEachAsync(dto =>
            {
                if (dto.Status == TaskStatus.Completed) return;
                if (dto.Status == TaskStatus.InProgress)
                    dto.ActualTime += (long) (taskCompletionDate - dto.LastStatusChangeDate).TotalMilliseconds;

                dto.LastStatusChangeDate = taskCompletionDate;
                dto.CompletionDate = taskCompletionDate;
                dto.Status = TaskStatus.Completed;

                context.Entry(dto).State = EntityState.Modified;
            });

    private async Task<bool> IsTreeFinishable(int taskId)
    {
        var task = await context.Tasks.FindAsync(taskId);
        if (task == null)
            return false;

        if (task.Status != TaskStatus.InProgress && task.Status != TaskStatus.Completed)
            return false;

        return await context.Tasks
            .Where(t => t.Path.Contains("/" + taskId + "/"))
            .AllAsync(t => t.Status == TaskStatus.InProgress || t.Status == TaskStatus.Completed);
    }

    private bool TaskExists(int id) => context.Tasks.Any(e => e.TaskId == id);

    private TaskItem GetTaskWithSubtasksFromDto(TaskItemDto itemDto)
    {
        var descendants = context.Tasks
            .Where(t => t.Path.Contains("/" + itemDto.TaskId + "/"))
            .AsEnumerable()
            .Select(TaskItem.ConvertFromDto)
            .ToList();

        var descendantsDictionary = descendants.ToDictionary(t => t.TaskId, t => t);
        var task = TaskItem.ConvertFromDto(itemDto);
        descendantsDictionary.Add(itemDto.TaskId, task);

        // rebuild the task hierarchy
        foreach (var t in descendants)
        {
            if (t.ParentId != null)
                descendantsDictionary[(int) t.ParentId].Subtasks.Add(t);
        }

        return task;
    }

    private TaskItemDto ConvertTaskToDto(TaskItem taskItem)
    {
        TaskItemDto parent = null;
        if (taskItem.ParentId != null)
            parent = context.Tasks.First(t => t.TaskId == taskItem.ParentId);
        return TaskItem.ConvertIntoDto(taskItem, parent);
    }


    public List<TaskItem> GetAllTasks()
    {
        var allTasks = context.Tasks
            .Select(TaskItem.ConvertFromDto)
            .ToList();

        var rootNodes = allTasks.Where(t => t.ParentId == null).ToList();
        var descendants = allTasks.Where(t => t.ParentId != null).ToList();
        var descendantsDictionary = allTasks.ToDictionary(t => t.TaskId, t => t);

        // rebuild the task hierarchy
        foreach (var t in descendants)
        {
            if (t.ParentId != null)
                descendantsDictionary[(int) t.ParentId].Subtasks.Add(t);
        }

        return rootNodes;
    }

    public async Task<TaskItem> GetTaskAsync(int id)
    {
        var task = await context.Tasks.FindAsync(id);
        if (task == null)
            throw new TaskNotFoundException(id);
        return GetTaskWithSubtasksFromDto(task);
    }

    public async Task UpdateTaskAsync(int id, TaskItem taskItem)
    {
        if (id != taskItem.TaskId)
            throw new BadTaskException();

        context.Entry(ConvertTaskToDto(taskItem)).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TaskExists(id))
                throw new TaskNotFoundException(id);

            throw;
        }
    }

    public async Task<TaskItem> AddTaskAsync(TaskItem taskItem)
    {
        var taskDto = ConvertTaskToDto(taskItem);
        context.Tasks.Add(taskDto);
        await context.SaveChangesAsync();
        return TaskItem.ConvertFromDto(taskDto);
    }

    public async Task DeleteTaskAsync(int id)
    {
        var task = await context.Tasks.FindAsync(id);
        if (task == null)
            throw new TaskNotFoundException(id);

        // make sure the task doesn't have subtasks
        bool hasChildren = await context.Tasks.AnyAsync(t => t.ParentId == id);
        if (hasChildren)
            throw new TaskHasChildrenException(id);

        context.Tasks.Remove(task);
        await context.SaveChangesAsync();
    }

    public async Task MarkTaskTreeAsFinishedAsync(int id, TaskItem taskItem)
    {
        if (id != taskItem.TaskId || taskItem.CompletionDate == null)
            throw new BadTaskException();

        await using var transaction = await context.Database.BeginTransactionAsync();

        // get all children for the task
        var t = await context.Tasks.FindAsync(id);
        if (t == null)
            throw new TaskNotFoundException(id);

        // check if some tasks can't be finished
        bool isTreeFinishable = await IsTreeFinishable(id);
        if (!isTreeFinishable)
            throw new TaskUnfinishableException(t.TaskId);

        await FinishTree(id, (DateTime) taskItem.CompletionDate);

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
