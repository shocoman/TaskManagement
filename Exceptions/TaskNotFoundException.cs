using System;

namespace TaskManagement.Exceptions;

public class TaskNotFoundException : Exception
{
    public TaskNotFoundException(int id) : base($"No task found with id {id}")
    {
    }
}
