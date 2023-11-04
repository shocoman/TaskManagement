using System;

namespace TaskManagement.Exceptions;

public class TaskUnfinishableException : Exception
{
    public TaskUnfinishableException(int id) : base($"Task with id {id} can't be recursively completed")
    {
    }
}
