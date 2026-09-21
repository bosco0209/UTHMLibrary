using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UTHMLibrary.Data;
using UTHMLibrary.Models;  // ← ADD THIS
using System.Security.Claims;

namespace UTHMLibrary.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Get current user
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _db.Users.FindAsync(int.Parse(userId ?? "0"));

        // Get user's faculty
        var userFaculty = user?.Faculty ?? "General";

        // Get current date for events
        var currentDate = DateTime.Now;
        var currentMonth = currentDate.Month;
        var currentYear = currentDate.Year;

        // ============================================
        // FACULTY HIGHLIGHTS
        // ============================================

        // 1. Past Year Papers from user's faculty
        var facultyPapers = await _db.Resources
            .Where(r => r.Category == "PastYearPaper" &&
                       (r.Faculty == userFaculty || string.IsNullOrEmpty(r.Faculty)))
            .OrderByDescending(r => r.Year)
            .Take(3)
            .ToListAsync();

        // 2. E-Books from user's faculty
        var facultyEBooks = await _db.Resources
            .Where(r => r.Category == "EBook" &&
                       (r.Faculty == userFaculty || string.IsNullOrEmpty(r.Faculty)))
            .OrderByDescending(r => r.UploadedAt)
            .Take(3)
            .ToListAsync();

        // 3. Project Reports from user's faculty
        var facultyProjects = await _db.Resources
            .Where(r => r.Category == "ProjectReport" &&
                       (r.Faculty == userFaculty || string.IsNullOrEmpty(r.Faculty)))
            .OrderByDescending(r => r.Year)
            .Take(3)
            .ToListAsync();

        // 4. General Resources from user's faculty
        var facultyResources = await _db.Resources
            .Where(r => r.Category == "Resource" &&
                       (r.Faculty == userFaculty || string.IsNullOrEmpty(r.Faculty)))
            .OrderByDescending(r => r.UploadedAt)
            .Take(3)
            .ToListAsync();

        // ============================================
        // UPCOMING EVENTS / NOTIFICATIONS
        // ============================================

        var events = new List<FacultyEvent>();

        // Exam season notifications
        if (currentMonth >= 4 && currentMonth <= 6)
        {
            events.Add(new FacultyEvent
            {
                Title = "📚 Final Exam Season",
                Description = "Past year papers are available for all faculties. Prepare for your exams!",
                Icon = "bi bi-file-earmark-text",
                Color = "danger",
                Link = "/Resources/PastYearPapers"
            });
        }
        else if (currentMonth >= 10 && currentMonth <= 12)
        {
            events.Add(new FacultyEvent
            {
                Title = "📝 Semester Final Exams",
                Description = "Access past year papers and study resources for your upcoming finals.",
                Icon = "bi bi-file-earmark-text",
                Color = "warning",
                Link = "/Resources/PastYearPapers"
            });
        }

        // Project submission reminder
        if (currentMonth >= 3 && currentMonth <= 5)
        {
            events.Add(new FacultyEvent
            {
                Title = "📊 FYP Submission Season",
                Description = "Final year project reports are being submitted. Check out the latest projects!",
                Icon = "bi bi-mortarboard",
                Color = "info",
                Link = "/Resources/ProjectReports"
            });
        }

        // New resources notification
        var newResourcesCount = await _db.Resources
            .Where(r => r.UploadedAt >= DateTime.Now.AddDays(-7))
            .CountAsync();

        if (newResourcesCount > 0)
        {
            events.Add(new FacultyEvent
            {
                Title = "🆕 New Resources Available",
                Description = $"{newResourcesCount} new resources have been added in the last 7 days.",
                Icon = "bi bi-newspaper",
                Color = "success",
                Link = "/Resources"
            });
        }

        // Faculty-specific events
        if (!string.IsNullOrEmpty(userFaculty))
        {
            var facultyResourcesCount = await _db.Resources
                .Where(r => r.Faculty == userFaculty && r.UploadedAt >= DateTime.Now.AddDays(-30))
                .CountAsync();

            if (facultyResourcesCount > 0)
            {
                events.Add(new FacultyEvent
                {
                    Title = $"📚 New {userFaculty} Resources",
                    Description = $"{facultyResourcesCount} new resources added for your faculty this month.",
                    Icon = "bi bi-building",
                    Color = "primary",
                    Link = "/Resources"
                });
            }
        }

        // ============================================
        // VIEWBAG
        // ============================================

        ViewBag.User = user;
        ViewBag.UserFaculty = userFaculty;
        ViewBag.FacultyPapers = facultyPapers;
        ViewBag.FacultyEBooks = facultyEBooks;
        ViewBag.FacultyProjects = facultyProjects;
        ViewBag.FacultyResources = facultyResources;
        ViewBag.Events = events;

        // Stats
        ViewBag.TotalResources = await _db.Resources.CountAsync();
        ViewBag.TotalStudyRooms = await _db.StudyRooms.CountAsync();
        ViewBag.AvailableStudyRooms = await _db.StudyRooms.CountAsync(r => r.IsAvailable);
        ViewBag.TotalMeetingRooms = await _db.MeetingRooms.CountAsync();

        return View();
    }

    public IActionResult About() => View();
    public IActionResult Error() => View();
}