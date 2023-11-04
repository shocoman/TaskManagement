using System;

namespace TaskManagement.Exceptions;

public class TaskHasChildrenException : Exception
{
    public TaskHasChildrenException(int id) : base($"Task {id} isn't a terminal node and can't be deleted")
    {
    }
}
