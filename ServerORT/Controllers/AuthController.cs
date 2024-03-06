using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ServerORT.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServerORT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public ActionResult<User> Register(UserDto request)
        {
            string hashPass = BCrypt.Net.BCrypt.HashPassword(request.userPass);
            user.userPass = hashPass;
            user.userName = request.userName;
            return Ok(user);
        }



        [HttpPost("login")]
        public ActionResult<User> Login(UserDto request)
        {
            if (user.userName != request.userName)
            {
                return BadRequest("User not found.");
            }
            if (!BCrypt.Net.BCrypt.Verify(request.userPass, user.userPass)) {
                return BadRequest("Wrong password.");
            }
            string token = CreateToken(user);
            return Ok(token);
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.userName)
            };
            Console.WriteLine("1");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            Console.WriteLine("1");

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays   (30),
                signingCredentials: cred

                );
            Console.WriteLine(token);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                      

            return jwt;
        } }
}
