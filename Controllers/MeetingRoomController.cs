using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UTHMLibrary.Data;
using UTHMLibrary.Models;
using UTHMLibrary.ViewModels;
using UTHMLibrary.ViewModels;  // ← Add this at the top

namespace UTHMLibrary.Controllers;

[Authorize(Roles = "Student")]
public class MeetingRoomController : Controller
{
    private readonly ApplicationDbContext _db;
    public MeetingRoomController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var rooms = await _db.MeetingRooms.OrderBy(r => r.RoomNumber).ToListAsync();
        return View(rooms);
    }

    [HttpGet]
    public async Task<IActionResult> Book(int id)
    {
        var room = await _db.MeetingRooms.FindAsync(id);
        if (room == null) return NotFound();
        if (!room.IsAvailable)
        {
            TempData["Error"] = "This meeting room is unavailable.";
            return RedirectToAction("Index");
        }
        return View(new MeetingRoomBookingViewModel
        {
            MeetingRoomId = room.Id,
            RoomNumber = room.RoomNumber
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(MeetingRoomBookingViewModel model)
    {
        var room = await _db.MeetingRooms.FindAsync(model.MeetingRoomId);
        if (room == null) return NotFound();

        var matrics = new[] { model.Matric1, model.Matric2, model.Matric3, model.Matric4, model.Matric5 }
            .Select(m => (m ?? "").Trim().ToUpper())
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToArray();

        if (matrics.Length < 5 || matrics.Distinct().Count() < 5)
        {
            ModelState.AddModelError("", "Please provide 5 unique matric numbers.");
        }

        if (!ModelState.IsValid)
        {
            model.RoomNumber = room.RoomNumber;
            return View(model);
        }

        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var booking = new MeetingRoomBooking
        {
            UserId = userId,
            MeetingRoomId = room.Id,
            BookingDate = model.BookingDate,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Purpose = model.Purpose,
            MemberMatrics = string.Join(",", matrics),
            Status = "Pending"
        };
        _db.MeetingRoomBookings.Add(booking);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Booking request sent to admin for approval.";
        return RedirectToAction("Index", "Dashboard");
    }

    public async Task<IActionResult> Cancel(int id)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var booking = await _db.MeetingRoomBookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (booking == null) return NotFound();
        booking.Status = "Cancelled";
        await _db.SaveChangesAsync();
        TempData["Success"] = "Meeting room booking cancelled.";
        return RedirectToAction("Index", "Dashboard");
    }
}