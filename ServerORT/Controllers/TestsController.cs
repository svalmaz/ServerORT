using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerORT.DbContext;
using ServerORT.Models;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace ServerORT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public TestsController(IConfiguration configuration, MyDbContext context, IWebHostEnvironment environment)
        {
            _configuration = configuration;

            _context = context;
            _environment = environment;
        }
        [HttpPost("AddTest")]
        [Authorize] 
        public async Task<IActionResult> VerifyTokenAsync(TestDTO test)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userStatus = identity.FindFirst("Status")?.Value;
                var userId = identity.FindFirst("Id")?.Value;

                if (userStatus == "teacher")
                {
                    int placeInCategory = await _context.TestsList
                               .Where(t => t.category == test.category)
                               .CountAsync() + 1;

                    var newTest = new TestList
                    {
                        title = test.title,
                        category = test.category,
                        videoUrl = test.videoUrl,
                        description = test.desc,
                        status = "inactive",
                        teacherId = Convert.ToInt32(userId),
                        place = placeInCategory
                    };

                    _context.TestsList.Add(newTest);
                    await _context.SaveChangesAsync();
                    return Ok("Wait a response");
                }
                else
                {
                    return Unauthorized("Access denied.");
                }
            }

            return BadRequest("Invalid token.");
        }
        [HttpGet("GetTests")]
     
        public async Task<List<TestList>> GetActiveTestsByCategoryAsync(string category)
        {
            var tests = await _context.TestsList
                .Where(t => t.category == category && t.status == "active")
                .ToListAsync();

            return tests;
        }
        [HttpPost("AcceptTest")]
      
        public async Task<ActionResult> ChangeTestStatusToActiveAsync(int testId)
        {
            var test = _context.TestsList.Where(u => u.id == testId).FirstOrDefault();
            test.status = "active";
            await _context.SaveChangesAsync();
            return Ok("Successeful");

         //   return false;
        }
        [HttpPost("AddQuestion")]

        [Authorize]

        public async Task<ActionResult> AddQuestionAsync(QuestionDTO questionDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                // Извлекаем статус пользователя из токена.
                var userStatus = identity.FindFirst("Status")?.Value;
                var userId = identity.FindFirst("Id")?.Value;

                if (userStatus == "teacher")
                {
                    var test = await _context.TestsList.FindAsync(questionDTO.testId);

                    if (test.teacherId != Convert.ToInt32(userId))
                    {
                        // Если testId не соответствует teacherId, возвращаем false
                        return Problem("Access denied.");
                    }
                    
                    var testExists = await _context.TestsList.AnyAsync(t => t.id == questionDTO.testId);

                    if (!testExists)
                    {
                        return Problem("test not found");
                    }



                    var newQuestion = new Question
                    {
                        title = questionDTO.title,
                        question = questionDTO.question,
                        a = questionDTO.a,
                        b = questionDTO.b,
                        c = questionDTO.c,
                        d = questionDTO.d,
                        answer = questionDTO.answer,
                        explanation = questionDTO.explanation,
                        testId = questionDTO.testId
                    };

                    _context.Questions.Add(newQuestion);

                    await _context.SaveChangesAsync();

                    return Ok("Success");
                }
                return Problem("Error");

            }
            return Problem("Error");

        }


    }
}
