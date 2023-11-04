using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Data.Models;
using TaskManagement.Data.Repositories;
using TaskManagement.Exceptions;

namespace TaskManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly TaskRepository taskRepository;

        public TaskController(TaskRepository repository)
        {
            taskRepository = repository;
        }

        // GET: api/Task
        [HttpGet]
        public ActionResult<IEnumerable<TaskItem>> GetTasks() => taskRepository.GetAllTasks();

        // GET: api/Task/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            try
            {
                return await taskRepository.GetTaskAsync(id);
            }
            catch (TaskNotFoundException)
            {
                return NotFound();
            }
        }

        // PUT: api/Task/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, TaskItem taskItem)
        {
            try
            {
                await taskRepository.UpdateTaskAsync(id, taskItem);
            }
            catch (BadTaskException)
            {
                return BadRequest();
            }
            catch (TaskNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Task
        [HttpPost]
        public async Task<ActionResult<TaskItem>> PostTask(TaskItem taskItem)
        {
            ModelState.Remove(nameof(TaskItem.TaskId));
                
            var newTask = await taskRepository.AddTaskAsync(taskItem);
            return CreatedAtAction(nameof(GetTask), new { id = newTask.TaskId }, newTask);
        }

        // DELETE: api/Task/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                await taskRepository.DeleteTaskAsync(id);
            }
            catch (TaskNotFoundException)
            {
                return NotFound();
            }
            catch (TaskHasChildrenException)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // PATCH: api/Task/FinishTree/5
        [HttpPatch("FinishTree/{id}")]
        public async Task<IActionResult> PatchFinishTree(int id, TaskItem taskItem)
        {
            try
            {
                await taskRepository.MarkTaskTreeAsFinishedAsync(id, taskItem);
            }
            catch (BadTaskException)
            {
                return BadRequest();
            }
            catch (TaskUnfinishableException)
            {
                return BadRequest();
            }
            catch (TaskNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
