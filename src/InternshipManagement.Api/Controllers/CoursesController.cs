using InternshipManagement.Api.Data;
using InternshipManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Get all courses (Interns/Admins)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _db.Courses
                .Select(c => new { c.Id, c.Title, c.Description, c.CreatedAt })
                .ToListAsync();
            return Ok(courses);
        }

        // Get my enrollments (Interns)
        [HttpGet("me")]
        [Authorize(Roles = "Intern")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var enrollments = await _db.UserCourses
                .Where(uc => uc.UserId == user.Id)
                .Include(uc => uc.Course)
                .Select(uc => new
                {
                    uc.Course!.Id,
                    uc.Course.Title,
                    uc.Course.Description,
                    uc.EnrolledAt
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        // Enroll in a course (Interns)
        [HttpPost("{courseId}/enroll")]
        [Authorize(Roles = "Intern")]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!await _db.Courses.AnyAsync(c => c.Id == courseId))
                return NotFound("Course not found");

            if (await _db.UserCourses.AnyAsync(uc => uc.UserId == user.Id && uc.CourseId == courseId))
                return BadRequest("Already enrolled");

            var uc = new UserCourse
            {
                UserId = user.Id,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            };

            _db.UserCourses.Add(uc);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Enrolled successfully" });
        }

        // Unenroll from a course (Interns)
        [HttpDelete("{courseId}/unenroll")]
        [Authorize(Roles = "Intern")]
        public async Task<IActionResult> Unenroll(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var uc = await _db.UserCourses
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.CourseId == courseId);

            if (uc == null) return NotFound("Not enrolled in this course");

            _db.UserCourses.Remove(uc);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Unenrolled successfully" });
        }

        // Get all enrollments (Admin only)
        [HttpGet("enrollments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllEnrollments()
        {
            var list = await _db.UserCourses
                .Include(uc => uc.Course)
                .Include(uc => uc.User)
                .Select(uc => new
                {
                    uc.Id,
                    uc.EnrolledAt,
                    UserId = uc.UserId,
                    Username = uc.User!.UserName,
                    FullName = uc.User.FullName, 
                    Email = uc.User!.Email,
                    ProfilePhoto = string.IsNullOrEmpty(uc.User.ProfilePhotoPath)
                        ? null
                        : $"{Request.Scheme}://{Request.Host}{uc.User.ProfilePhotoPath}", 
                    CourseId = uc.Course!.Id,
                    CourseTitle = uc.Course.Title
                })
                .ToListAsync();

            return Ok(list);
        }


        // Get enrollments for a specific course (Admin only)
        [HttpGet("{courseId}/enrollments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCourseEnrollments(int courseId)
        {
            var list = await _db.UserCourses
                .Where(uc => uc.CourseId == courseId)
                .Include(uc => uc.User)
                .Select(uc => new
                {
                    uc.UserId,
                    uc.User!.UserName,
                    uc.User!.Email,
                    uc.EnrolledAt
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
