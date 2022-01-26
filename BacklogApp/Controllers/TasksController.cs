using BacklogApp.Managers;
using BacklogApp.Models.Db;
using BacklogApp.Models.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BacklogApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : BaseController
    {
        private TasksManager Mng { get; }

        public TasksController(TasksManager mng)
        {
            Mng = mng;
        }


        [HttpGet("project/{projId}")]
        public IActionResult GetInProject(string projId)
        {
            string userId = GetUserId();
            List<TaskViewModel> tasks = Mng.GetTasksInProject(projId, userId);

            return Ok(new
            {
                items = tasks
            });
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            string userId = GetUserId();
            TaskViewModel? task = Mng.Get(id, userId);

            if(task == null) return NotFound();

            return Ok(new
            {
                item = task
            });
        }

        [HttpPost]
        public IActionResult Create(CreateTaskModel model)
        {
            string userId = GetUserId();
            TaskViewModel? res = Mng.Create(model.Name, model.ProjectId, userId, model.Description, model.Deadline, model.Priority, model.Assessment);

            if(res == null) return BadRequest();

            return CreatedAtAction("Get", new { id = res.Id }, res);
        }

        [HttpPut("{id}")]
        public IActionResult Edit(string id, EditTaskModel model)
        {
            string userId = GetUserId();
            Mng.Change(id, userId, model);

            return NoContent();
        }

        [HttpPut("{id}/status/{status}")]
        public IActionResult EditStatus(string id, string status)
        {
            string userId = GetUserId();
            Mng.ChangeStatus(id, userId, status);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            string userId = GetUserId();
            Mng.Delete(id, userId);

            return NoContent();
        }
    }
}
