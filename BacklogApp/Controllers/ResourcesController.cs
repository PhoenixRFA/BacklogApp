using BacklogApp.Managers;
using BacklogApp.Models.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BacklogApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : BaseController
    {
        private ResourcesManager Mng { get; }

        public ResourcesController(ResourcesManager mng)
        {
            Mng = mng;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult GetFile(string id)
        {
            ResourceViewModel? res = Mng.Get(id);

            if(res == null) return NotFound();

            return File(res.FileStream, res.MimeType ?? "application/octet-stream", res.FileName);
        }

        [HttpPost("{code}")]
        public IActionResult UploadFile(string code, IFormFile file)
        {
            string userId = GetUserId();

            string resourceId = Mng.Upload(file.OpenReadStream(), userId, file.FileName, file.ContentType, code);

            return Ok(new { resourceId });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteFile(string id)
        {
            string userId = GetUserId();
            bool res = Mng.Delete(id, userId);

            if(res) return NoContent();
            
            return BadRequest();
        }
    }
}
