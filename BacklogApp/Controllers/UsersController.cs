using BacklogApp.Managers;
using BacklogApp.Models.Auth;
using BacklogApp.Models.Db;
using BacklogApp.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BacklogApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private UsersManager Mng { get; }

        public UsersController(UsersManager mng)
        {
            Mng = mng;
        }

        [AllowAnonymous]
        [HttpGet("emailExists/{email}")]
        public IActionResult CheckEmail(string email)
        {
            bool res = Mng.IsEmailExists(email);

            return Ok(new { result = res });
        }

        [HttpPut("name")]
        public IActionResult ChangeName(ChangeNameModel model)
        {
            if (string.IsNullOrEmpty(model?.Name)) return BadRequest("name is empty");

            string userId = GetUserId();
            Mng.ChangeName(userId, model.Name);

            return NoContent();
        }

        [HttpPut("email")]
        public IActionResult ChangeEmail(ChangeEmailModel model)
        {
            //TODO: two step change: 1. enter email - send code to email 2. enter code - change email
            if (string.IsNullOrEmpty(model?.Email)) return BadRequest("email is empty");

            string userId = GetUserId();
            Mng.ChangeEmail(userId, model.Email);

            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("restorePassword")]
        public IActionResult RestorePassword(ChangeEmailModel user)
        {
            //TODO: send warning message to old email
            if(string.IsNullOrEmpty(user?.Email)) return BadRequest("email is empty");

            Mng.RestorePassword(user.Email, Ip);

            return NoContent();
        }

        [HttpPut("photo/{resourceId}")]
        public IActionResult AttachPhoto(string resourceId)
        {
            string userId = GetUserId();
            Mng.AttachPhoto(userId, resourceId);

            return Ok();
        }

        [HttpDelete("photo")]
        public IActionResult DeletePhoto()
        {
            string userId = GetUserId();
            Mng.DeletePhoto(userId);
            
            return NoContent();
        }
    }
}
