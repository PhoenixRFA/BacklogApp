using BacklogApp.Models.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BacklogApp.Services
{
    public interface IJwtTokenFactory
    {
        string BuildToken(string id, string name, out DateTime expired);
    }

    public class JwtTokenFactory : IJwtTokenFactory
    {
        private readonly JwtOptions _opts;
        
        public JwtTokenFactory(IOptions<JwtOptions> options)
        {
            _opts = options.Value;
        }

        public string BuildToken(string id, string name, out DateTime expires)
        {
            if(string.IsNullOrEmpty(_opts.Key)) throw new ArgumentNullException($"{nameof(JwtOptions)}.{nameof(_opts.Key)}", "key is empty");
            
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, name)
            };

            expires = DateTime.Now.AddMinutes(_opts.Lifetime);
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken
            (
                issuer: _opts.Issuer,
                audience: _opts.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
