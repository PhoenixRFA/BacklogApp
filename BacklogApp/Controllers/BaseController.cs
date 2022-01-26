using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BacklogApp.Controllers
{
    public class BaseController : ControllerBase
    {
        protected string Ip => Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"]
            : HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString()
                ?? "127.0.0.1";

        protected string GetUserId()
        {
            if(User.Identity == null || !User.Identity.IsAuthenticated) return string.Empty;

            string claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return claim;
        }
    }
}
