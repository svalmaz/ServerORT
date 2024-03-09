using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServerORT.DbContext;
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
       
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration, MyDbContext context, IWebHostEnvironment environment)
        {
            _configuration = configuration;

            _context = context;
            _environment = environment;
        }
        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegisterDto request)
        {
            if (_context.Users == null)
            {
                return Problem("Oops... Something happened to the database");
            }

            if (_context.Users.Count(u => u.login == request.userEmail) > 0)
            {
                return Ok(new ApiResponse { Status = "failed", Message = "user with this email is already registered." });
            }
            else
            {
                string hashPass = BCrypt.Net.BCrypt.HashPassword(request.userPass);

                var new_user = new User()
                {
                    email = request.userEmail,
                    pass =  hashPass,
                    login = request.userName
                };

                _context.Users.Add(new_user);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse { Status = "successful", Message = "user has been created." });
            }
        
       
        }



        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto request)
        {
            if (_context.Users == null)
            {
                return Problem("Oops... Something happened to the database");
            }
            if (await _context.Users.CountAsync(u => u.login == request.userName ) > 0)
            {
                var user = await _context.Users
                 
                   .FirstAsync(u => u.login == request.userName);
                if (!BCrypt.Net.BCrypt.Verify(request.userPass, user.pass))
                {
                    return BadRequest("Wrong password.");
                }
                else
                {
                    string token = CreateToken(user);
                    return Ok(token);
                }
               
            }
            return Problem("Oops");
          
         
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.login)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays   (30),
                signingCredentials: cred

                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                      

            return jwt;
        } }
}
