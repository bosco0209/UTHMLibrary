using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UTHMLibrary.Data;

namespace UTHMLibrary.Controllers;

[Authorize(Roles = "Student")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    public DashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(userId);

        var studyBookings = await _db.StudyRoomBookings
            .Include(b => b.StudyRoom)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var meetingBookings = await _db.MeetingRoomBookings
            .Include(b => b.MeetingRoom)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        ViewBag.User = user;
        ViewBag.StudyBookings = studyBookings;
        ViewBag.MeetingBookings = meetingBookings;
        ViewBag.ActiveStudyBookings = studyBookings.Count(b => b.Status == "Confirmed");
        ViewBag.PendingMeetingBookings = meetingBookings.Count(b => b.Status == "Pending");
        return View();
    }
}