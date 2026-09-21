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
public class StudyRoomController : Controller
{
    private readonly ApplicationDbContext _db;
    public StudyRoomController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var rooms = await _db.StudyRooms.OrderBy(r => r.RoomNumber).ToListAsync();
        return View(rooms);
    }

    [HttpGet]
    public async Task<IActionResult> Book(int id)
    {
        var room = await _db.StudyRooms.FindAsync(id);
        if (room == null) return NotFound();
        if (!room.IsAvailable)
        {
            TempData["Error"] = "This room is currently unavailable.";
            return RedirectToAction("Index");
        }

        return View(new StudyRoomBookingViewModel
        {
            StudyRoomId = room.Id,
            RoomNumber = room.RoomNumber,
            Price = room.Price
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(StudyRoomBookingViewModel model)
    {
        var room = await _db.StudyRooms.FindAsync(model.StudyRoomId);
        if (room == null) return NotFound();

        if (!room.IsAvailable)
        {
            TempData["Error"] = "Room just became unavailable.";
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            model.RoomNumber = room.RoomNumber;
            model.Price = room.Price;
            return View(model);
        }

        TempData["BookingData"] = System.Text.Json.JsonSerializer.Serialize(model);
        return RedirectToAction("Payment", new { id = room.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Payment(int id)
    {
        var room = await _db.StudyRooms.FindAsync(id);
        if (room == null) return NotFound();

        var data = TempData["BookingData"] as string;
        if (string.IsNullOrEmpty(data))
            return RedirectToAction("Book", new { id });

        TempData.Keep("BookingData");
        var model = System.Text.Json.JsonSerializer.Deserialize<StudyRoomBookingViewModel>(data)!;
        ViewBag.Room = room;
        return View(model);
    }

    [HttpPost, ActionName("ConfirmPayment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(int id)
    {
        var data = TempData["BookingData"] as string;
        if (string.IsNullOrEmpty(data))
        {
            TempData["Error"] = "Booking session expired.";
            return RedirectToAction("Index");
        }

        var model = System.Text.Json.JsonSerializer.Deserialize<StudyRoomBookingViewModel>(data)!;
        var room = await _db.StudyRooms.FindAsync(id);
        if (room == null) return NotFound();

        if (!room.IsAvailable)
        {
            TempData["Error"] = "Room just became unavailable.";
            return RedirectToAction("Index");
        }

        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var booking = new StudyRoomBooking
        {
            UserId = userId,
            StudyRoomId = room.Id,
            BookingDate = model.BookingDate,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            AmountPaid = room.Price,
            PaymentStatus = "Paid",
            Status = "Confirmed"
        };
        _db.StudyRoomBookings.Add(booking);

        room.IsAvailable = false;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Payment of RM{room.Price:F2} successful. Room {room.RoomNumber} booked!";
        return RedirectToAction("Success", new { bookingId = booking.Id });
    }

    public async Task<IActionResult> Success(int bookingId)
    {
        var booking = await _db.StudyRoomBookings
            .Include(b => b.StudyRoom)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null) return NotFound();
        return View(booking);
    }

    public async Task<IActionResult> Cancel(int id)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var booking = await _db.StudyRoomBookings
            .Include(b => b.StudyRoom)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
        if (booking == null) return NotFound();

        booking.Status = "Cancelled";
        if (booking.StudyRoom != null) booking.StudyRoom.IsAvailable = true;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Booking cancelled. Room is now available again.";
        return RedirectToAction("Index", "Dashboard");
    }
}