using BacklogApp.Managers;
using BacklogApp.Models.Auth;
using BacklogApp.Models.Db;
using BacklogApp.Models.Options;
using BacklogApp.Models.Users;
using BacklogApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace BacklogApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private UsersManager Mng { get; }
        private IJwtTokenFactory TokenFactory { get; }
        private RefreshTokenOptions RefreshTokenOpts { get; }

        public AuthController(UsersManager mng, IJwtTokenFactory tokenFactory, IOptions<RefreshTokenOptions> refreshTokenOpts)
        {
            Mng = mng;
            TokenFactory = tokenFactory;
            RefreshTokenOpts = refreshTokenOpts.Value;
        }

        //[AllowAnonymous]
        [HttpPost("login")] //name: authenticate, signin
        public IActionResult Login(LoginModel model)
        {
            if (string.IsNullOrEmpty(model?.Username)) return BadRequest("login is empty");
            if (string.IsNullOrEmpty(model?.Password)) return BadRequest("password is empty");

            UserModel? user = Mng.GetByEmail(model.Username);
            if (user == null) return BadRequest("login and/or password are wrong");
            bool res = Mng.CheckPassword(user, model.Password);
            if (!res) return BadRequest("login and/or password are wrong");

            string bearer = TokenFactory.BuildToken(user.Id, user.Name!, out DateTime expired);

            RefreshToken? refreshToken = Mng.AddRefreshToken(user.Id, Ip);
            if (refreshToken == null) return StatusCode((int)HttpStatusCode.InternalServerError);

            SetRefreshTokenCookie(refreshToken.Token!, model.Remember);

            return Ok(new
            {
                token = new BearerViewModel(bearer, expired),
                user = new UserViewModel(user)
            });
        }

        //[Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            string? refreshToken = Request.Cookies[RefreshTokenOpts.CookieName];
            if (string.IsNullOrEmpty(refreshToken)) return NoContent();//BadRequest();//"refresh token is missing");

            Response.Cookies.Delete(RefreshTokenOpts.CookieName);
            Mng.RevokeToken(refreshToken, Ip);

            return NoContent();
        }

        //TODO: move to users controller
        //[AllowAnonymous]
        [HttpPost("register")] //name: signup
        public IActionResult Register(RegisterModel model)
        {
            if (string.IsNullOrEmpty(model?.Name)) return BadRequest("name is empty");
            if (string.IsNullOrEmpty(model?.Email)) return BadRequest("email is empty");

            UserModel? res = Mng.Add(model.Name, model.Email);
            if (res == null) return BadRequest();

            return CreatedAtAction(nameof(Login), new UserViewModel(res));
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            string userId = GetUserId();
            UserModel? user = Mng.GetById(userId);
            if (user == null) return StatusCode((int)HttpStatusCode.InternalServerError);

            return Ok(new UserViewModel(user));
        }

        //[AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            string? refreshToken = Request.Cookies[RefreshTokenOpts.CookieName];
            if (string.IsNullOrEmpty(refreshToken)) return BadRequest();//"refresh token is missing");

            RefreshTokenResult? res = Mng.RotateRefreshToken(refreshToken, Ip);
            if (res == null) return BadRequest();//"wrong refresh token");

            string bearer = TokenFactory.BuildToken(res.User.Id, res.User.Name!, out DateTime expired);

            bool isLongtermSession = GetSessionLifetime();
            SetRefreshTokenCookie(res.Token.Token!, isLongtermSession);

            return Ok(new
            {
                token = new BearerViewModel(bearer, expired),
                user = new UserViewModel(res.User)
            });
        }

        [Authorize]
        [HttpPut("password")]
        public IActionResult ChangePassword(ChangePasswordModel model)
        {
            if (string.IsNullOrEmpty(model.NewPassword)) return BadRequest("password is empty");
            if (string.IsNullOrEmpty(model.OldPassword)) return BadRequest("old password is empty");
            if (model.OldPassword == model.NewPassword) return BadRequest("old password the same as a new one");
            //TODO: add password validation

            string userId = GetUserId();
            Mng.ChangePassword(userId, model.OldPassword, model.NewPassword, Ip, out RefreshToken? refreshToken);
            
            if(refreshToken != null)
            {
                bool isLongtermSession = GetSessionLifetime();
                SetRefreshTokenCookie(refreshToken.Token, isLongtermSession);
            }

            return NoContent();
        }

        private void SetRefreshTokenCookie(string token, bool isLongterm)
        {
            var opts = new CookieOptions
            {
                HttpOnly = true,
                Expires = isLongterm ? DateTime.Now.AddDays(RefreshTokenOpts.Lifetime) : null,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                //TODO: enable for production
                //Secure = true
            };
            Response.Cookies.Append(RefreshTokenOpts.CookieName, token, opts);
            Response.Cookies.Append(RefreshTokenOpts.SessionLifetimeCookieName, string.Empty, opts);
        }

        private bool GetSessionLifetime()
        {
            return Request.Cookies.ContainsKey(RefreshTokenOpts.SessionLifetimeCookieName);
        }
    }
}
