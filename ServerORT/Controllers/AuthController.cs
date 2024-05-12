using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServerORT.Commands;
using ServerORT.DbContext;
using ServerORT.Hub;
using ServerORT.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ServerORT.Commands;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using static System.Net.WebRequestMethods;
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

            if (_context.Users.Count(u => u.email == request.userEmail) > 0 )
            {
                return Ok(new ApiResponse { Status = "failed", Message = "user with this email is already registered." });
            }
            else if (_context.Users.Count(u => u.login == request.userName) > 0)
            {
                return Ok(new ApiResponse { Status = "failed", Message = "user with this username is already registered." });
            }
            else
            {
                if (request.userStatus == "student")
                {
                    string hashPass = BCrypt.Net.BCrypt.HashPassword(request.userPass);

                    var new_user = new User()
                    {
                        email = request.userEmail,
                        pass = hashPass,
                        login = request.userName,
                        status = "awaitsUser",
                        git = ""
                        
                    };

                    _context.Users.Add(new_user);
                    await _context.SaveChangesAsync();
					if (_context.Users.Count(u => u.email == request.userEmail) > 0)
					{
						var user = _context.Users.First(u => u.email == request.userEmail);
						//   RandomString random = new RandomString();

						ResetPass reset = new ResetPass();
                      //  Console.WriteLine(request.userEmail + "a");
						reset.SendEmail(request.userEmail.ToString(), "Сonfirm email", "Your link: " + "https://localhost:7140/api/Auth/acceptTeacher?teacherId=" + user.id);
						await _context.SaveChangesAsync();

						return Ok(new ApiResponse { Status = "successful", Message = "confirmation link sent by email" });
					} 
					return Ok(new ApiResponse { Status = "successful", Message = "user has been created." });
                }
                else if(request.userStatus == "teacher")
                {
                    string hashPass = BCrypt.Net.BCrypt.HashPassword(request.userPass);

                    var new_user = new User()
                    {
                        email = request.userEmail,
                        pass = hashPass,
                        login = request.userName,
                        status = "awaits"

                    };
                    _context.Users.Add(new_user);
					await _context.SaveChangesAsync();
                    //https://localhost:7140/api/Auth/acceptTeacher?teacherId=1
                   

						return Ok(new ApiResponse { Status = "successful", Message = "wait for a response" });
                }
                return Problem("Bad request");



            }


        }
        [HttpGet("getTeachers")]
        public async Task<ActionResult<User>> GetTeachers()
        {
            if (_context.Users == null)
            {
                return Problem("Oops... Something happened to the database");
            }
            else
            {
                List<User> teacher = await _context.Users.Where(u => u.status == "awaits").ToListAsync();
                return Ok(teacher);
            }
        }
        [HttpGet("acceptTeacher")]
        public async Task<ActionResult> AcceptTeacher(int teacherId)
        {
            if (_context == null)
            {
                return Problem("Oops... something happened to the database");
            }
            else
            {
                var teacher = _context.Users.Where(u => u.id ==  teacherId).FirstOrDefault();
                if(teacher == null)
                {
					return Problem("Oops... user has already been accepted");
				}
				if (teacher.status == "awaitsUser")
                {
					teacher.status = "student";

				}
                else
                {
					teacher.status = "teacher";

				}
				await _context.SaveChangesAsync();
                return Ok("Successeful");
            }
        }
        [HttpPost]
        [Route("changepass")]
        public async Task<ActionResult> PasswordChange(ChangePassDTO request)
        {
            if (_context == null)
            {
                return Problem("Oops... Something happened to the database");
            }
            if (_context.Users.Count(u => u.id == request.userId) > 0)
            {
                var user = _context.Users.First(u => u.id == request.userId);

                if (!BCrypt.Net.BCrypt.Verify(request.userOldPass, user.pass))
                {
                    return BadRequest("Wrong password.");
                }
                else
                {
                    user.pass = BCrypt.Net.BCrypt.HashPassword(request.userNewPass);


                    await _context.SaveChangesAsync();

                    return Ok(new ApiResponse { Status = "successful", Message = "password has been changed." });
                }
             
            }
            else
            {
                return Problem("Oops... Something happened to the database");
            }

        }
        [HttpPost]
        [Route("forgotPass")]
        public async Task<ActionResult> PasswordRecovery(string email)
        {

            if (_context.Users == null)
            {
                return Problem("Oops... Something happened to the database");
            }

            if (_context.Users.Count(u => u.email == email) > 0)
            {
                var user = _context.Users.First(u => u.email == email);
                //   RandomString random = new RandomString();
                string newPass = RandomPass(8, false);
                user.pass = BCrypt.Net.BCrypt.HashPassword(newPass);
                ResetPass reset = new ResetPass();
                reset.SendEmail(email, "New Pass", "Your new pass: " + newPass);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse { Status = "successful", Message = "password sent" });
            }
            else
            {
                return Problem("Failed, user with this email does not exist");
            }

        }
        string RandomPass(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
		[HttpGet("loginGit")]
		public IActionResult Login()
		{
			Console.WriteLine("Redirecting to GitHub for authentication...");
			return Challenge(new AuthenticationProperties { RedirectUri = "/api/Auth/getUserId1" }, "GitHub");
		}
		[HttpGet("GetUserId1")]
		public async Task<ActionResult> GetUserId1()
		{
            Console.WriteLine(gitId);
			if (await _context.Users.CountAsync(u => u.git == gitId) > 0)
			{
				var user = _context.Users.First(u => u.git == gitId);
				string token = CreateToken(user);
				return Ok(token);

			}
                

			//var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			// Действия после успешной аутентификации, например, перенаправление или возвращение данных
			return Problem("account is not connected");

		}
		[HttpGet("GetUserId")]
		public async Task<ActionResult> GetUserId()
		{
			if (await _context.Users.CountAsync(u => u.git == gitId) > 0)
            {

				return Ok(gitId);

			}


			//var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			// Действия после успешной аутентификации, например, перенаправление или возвращение данных
			return Problem("account is not connected");

		}
		public static string gitId = "";

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
                    if(user.status == "student" ||  user.status == "teacher" || user.status == "admin")
                    { 
                    string token = CreateToken(user);
                    return Ok(token);
                    }
                    else
                    {
                        return Ok(new ApiResponse { Status = "successful", Message = "wait for a response" });

                    }
                }
               
            }
            return Problem("Oops");
          
         
        }

        
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("Id", user.id.ToString()),
                new Claim("Status", user.status)
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
