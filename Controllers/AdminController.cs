using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UTHMLibrary.Data;
using UTHMLibrary.Models;
using UTHMLibrary.ViewModels;

namespace UTHMLibrary.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // ============================================
    // DASHBOARD
    // ============================================

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalUsers = await _db.Users.CountAsync(u => u.Role == "Student");
        ViewBag.TotalResources = await _db.Resources.CountAsync();
        ViewBag.TotalStudyRooms = await _db.StudyRooms.CountAsync();
        ViewBag.AvailableStudyRooms = await _db.StudyRooms.CountAsync(r => r.IsAvailable);
        ViewBag.TotalMeetingRooms = await _db.MeetingRooms.CountAsync();
        ViewBag.PendingMeetingBookings = await _db.MeetingRoomBookings.CountAsync(b => b.Status == "Pending");
        ViewBag.ConfirmedStudyBookings = await _db.StudyRoomBookings.CountAsync(b => b.Status == "Confirmed");

        var paidBookings = await _db.StudyRoomBookings
            .Where(b => b.PaymentStatus == "Paid" && b.Status == "Confirmed")
            .ToListAsync();

        ViewBag.TotalRevenue = paidBookings.Sum(b => b.AmountPaid);

        ViewBag.RecentStudyBookings = await _db.StudyRoomBookings
            .Include(b => b.User).Include(b => b.StudyRoom)
            .OrderByDescending(b => b.CreatedAt).Take(5).ToListAsync();
        ViewBag.RecentMeetingBookings = await _db.MeetingRoomBookings
            .Include(b => b.User).Include(b => b.MeetingRoom)
            .OrderByDescending(b => b.CreatedAt).Take(5).ToListAsync();

        return View();
    }

    // ============================================
    // RESOURCES MANAGEMENT
    // ============================================

    public async Task<IActionResult> Resources(string? category)
    {
        var q = _db.Resources.AsQueryable();
        if (!string.IsNullOrEmpty(category)) q = q.Where(r => r.Category == category);
        ViewBag.Category = category;
        return View(await q.OrderByDescending(r => r.UploadedAt).ToListAsync());
    }

    [HttpGet]
    public IActionResult UploadResource() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadResource(UploadResourceViewModel model)
    {
        if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file to upload.");
            return View(model);
        }

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.File.FileName)}";
        var fullPath = Path.Combine(uploadsDir, fileName);
        using (var fs = new FileStream(fullPath, FileMode.Create))
        {
            await model.File.CopyToAsync(fs);
        }

        _db.Resources.Add(new Resource
        {
            Title = model.Title,
            Description = model.Description ?? "",
            Category = model.Category,
            Author = model.Author ?? "",
            Faculty = model.Faculty ?? "",
            Year = model.Year ?? "",
            FilePath = "/uploads/" + fileName,
            OriginalFileName = model.File.FileName
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Resource uploaded successfully.";
        return RedirectToAction("Resources");
    }

    public async Task<IActionResult> DeleteResource(int id)
    {
        var res = await _db.Resources.FindAsync(id);
        if (res == null) return NotFound();

        try
        {
            var physical = Path.Combine(_env.WebRootPath, res.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(physical)) System.IO.File.Delete(physical);
        }
        catch { /* ignore */ }

        _db.Resources.Remove(res);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Resource deleted.";
        return RedirectToAction("Resources");
    }

    // ============================================
    // STUDY ROOM MANAGEMENT
    // ============================================

    public async Task<IActionResult> StudyRooms()
        => View(await _db.StudyRooms.OrderBy(r => r.RoomNumber).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> ToggleStudyRoom(int id)
    {
        var room = await _db.StudyRooms.FindAsync(id);
        if (room == null) return NotFound();
        room.IsAvailable = !room.IsAvailable;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Room {room.RoomNumber} is now {(room.IsAvailable ? "Available" : "Unavailable")}.";
        return RedirectToAction("StudyRooms");
    }

    [HttpGet]
    public IActionResult AddStudyRoom() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudyRoom(StudyRoom room)
    {
        if (!ModelState.IsValid) return View(room);
        _db.StudyRooms.Add(room);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Study room added.";
        return RedirectToAction("StudyRooms");
    }

    // ============================================
    // MEETING ROOM MANAGEMENT
    // ============================================

    public async Task<IActionResult> MeetingRooms()
        => View(await _db.MeetingRooms.OrderBy(r => r.RoomNumber).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> ToggleMeetingRoom(int id)
    {
        var room = await _db.MeetingRooms.FindAsync(id);
        if (room == null) return NotFound();
        room.IsAvailable = !room.IsAvailable;
        await _db.SaveChangesAsync();
        return RedirectToAction("MeetingRooms");
    }

    // ============================================
    // BOOKINGS MANAGEMENT
    // ============================================

    public async Task<IActionResult> StudyBookings()
    {
        var list = await _db.StudyRoomBookings
            .Include(b => b.User).Include(b => b.StudyRoom)
            .OrderByDescending(b => b.CreatedAt).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> MeetingBookings()
    {
        var list = await _db.MeetingRoomBookings
            .Include(b => b.User).Include(b => b.MeetingRoom)
            .OrderByDescending(b => b.CreatedAt).ToListAsync();
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> ApproveMeetingBooking(int id)
    {
        var b = await _db.MeetingRoomBookings.FindAsync(id);
        if (b == null) return NotFound();
        b.Status = "Approved";
        b.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Meeting booking approved.";
        return RedirectToAction("MeetingBookings");
    }

    [HttpPost]
    public async Task<IActionResult> RejectMeetingBooking(int id, string? note)
    {
        var b = await _db.MeetingRoomBookings.FindAsync(id);
        if (b == null) return NotFound();
        b.Status = "Rejected";
        b.AdminNote = note ?? "";
        b.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Meeting booking rejected.";
        return RedirectToAction("MeetingBookings");
    }

    [HttpPost]
    public async Task<IActionResult> CancelStudyBooking(int id)
    {
        var b = await _db.StudyRoomBookings
            .Include(x => x.StudyRoom)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (b == null) return NotFound();

        b.Status = "Cancelled";
        if (b.StudyRoom != null)
        {
            b.StudyRoom.IsAvailable = true;
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Study booking cancelled, room released.";
        return RedirectToAction("StudyBookings");
    }

    // ============================================
    // USERS MANAGEMENT
    // ============================================

    public async Task<IActionResult> Users()
    {
        var users = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var userViewModels = new List<UserManagementViewModel>();

        foreach (var user in users)
        {
            var studyBookings = await _db.StudyRoomBookings.CountAsync(b => b.UserId == user.Id);
            var meetingBookings = await _db.MeetingRoomBookings.CountAsync(b => b.UserId == user.Id);

            userViewModels.Add(new UserManagementViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                MatricNumber = user.MatricNumber,
                Email = user.Email,
                Faculty = user.Faculty,
                Phone = user.Phone,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                TotalBookings = studyBookings + meetingBookings,
                ActiveBookings = studyBookings + meetingBookings
            });
        }

        ViewBag.TotalUsers = users.Count;
        ViewBag.AdminUsers = users.Count(u => u.Role == "Admin");
        ViewBag.StudentUsers = users.Count(u => u.Role == "Student");

        return View(userViewModels);
    }

    // ============================================
    // DELETE USER
    // ============================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Users");
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            var hasStudyBookings = await _db.StudyRoomBookings.AnyAsync(b => b.UserId == user.Id && b.Status != "Cancelled");
            var hasMeetingBookings = await _db.MeetingRoomBookings.AnyAsync(b => b.UserId == user.Id && b.Status != "Cancelled" && b.Status != "Rejected");

            if (hasStudyBookings || hasMeetingBookings)
            {
                TempData["Warning"] = $"User '{user.FullName}' has active bookings. Please cancel their bookings first or use 'Force Delete'.";
                return RedirectToAction("Users");
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"✅ User '{user.FullName}' has been removed successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to delete user: {ex.Message}";
        }

        return RedirectToAction("Users");
    }

    // ============================================
    // FORCE DELETE USER
    // ============================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceDeleteUser(int id)
    {
        try
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Users");
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            // Delete all user's bookings
            var studyBookings = await _db.StudyRoomBookings.Where(b => b.UserId == user.Id).ToListAsync();
            var meetingBookings = await _db.MeetingRoomBookings.Where(b => b.UserId == user.Id).ToListAsync();

            if (studyBookings.Any())
            {
                foreach (var booking in studyBookings)
                {
                    var room = await _db.StudyRooms.FindAsync(booking.StudyRoomId);
                    if (room != null && booking.Status != "Cancelled")
                    {
                        room.IsAvailable = true;
                    }
                }
                _db.StudyRoomBookings.RemoveRange(studyBookings);
            }

            if (meetingBookings.Any())
            {
                foreach (var booking in meetingBookings)
                {
                    var room = await _db.MeetingRooms.FindAsync(booking.MeetingRoomId);
                    if (room != null && booking.Status != "Cancelled" && booking.Status != "Rejected")
                    {
                        room.IsAvailable = true;
                    }
                }
                _db.MeetingRoomBookings.RemoveRange(meetingBookings);
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"✅ User '{user.FullName}' and all their bookings have been removed. They can now register again.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to delete user: {ex.Message}";
        }

        return RedirectToAction("Users");
    }

    // ============================================
    // USER DETAILS
    // ============================================

    public async Task<IActionResult> UserDetails(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var studyBookings = await _db.StudyRoomBookings
            .Include(b => b.StudyRoom)
            .Where(b => b.UserId == user.Id)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var meetingBookings = await _db.MeetingRoomBookings
            .Include(b => b.MeetingRoom)
            .Where(b => b.UserId == user.Id)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        ViewBag.StudyBookings = studyBookings;
        ViewBag.MeetingBookings = meetingBookings;
        ViewBag.TotalBookings = studyBookings.Count + meetingBookings.Count;

        return View(user);
    }
}