using BacklogApp.Managers;
using BacklogApp.Models.Db;
using BacklogApp.Models.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BacklogApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : BaseController
    {
        public ProjectsManager Mng { get; }

        public ProjectsController(ProjectsManager mng)
        {
            Mng = mng;
        }

        [HttpGet]
        public IActionResult Get()
        {
            string userId = GetUserId();
            List<ProjectViewModel> projects = Mng.GetUserProjects(userId);

            return Ok(new
            {
                items = projects
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetProject(string id)
        {
            string userId = GetUserId();
            ProjectViewModel? project = Mng.Get(id, userId);

            if (project == null) return NotFound();

            return Ok(new
            {
                item = project
            });
        }

        [HttpPost]
        public IActionResult Create(CreateProjectModel model)
        {
            string userId = GetUserId();
            ProjectViewModel project = Mng.Create(model.Name, userId);

            return CreatedAtAction("GetProject", new { id = project.Id }, project);
        }

        [HttpPut("{id}/name")]
        public IActionResult ChangeName(string id, ChangeProjectNameModel model)
        {
            string userId = GetUserId();
            Mng.ChangeName(id, model.Name, userId);

            return NoContent();
        }

        [HttpPost("{id}/users/{newUserId}")]
        public IActionResult AddUser(string id, string newUserId)
        {
            string userId = GetUserId();
            Mng.AddUser(id, newUserId, userId);

            return NoContent();
        }

        [HttpDelete("{id}/users/{newUserId}")]
        public IActionResult DeleteUser(string id, string newUserId)
        {
            string userId = GetUserId();
            Mng.RemoveUser(id, newUserId, userId);

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
