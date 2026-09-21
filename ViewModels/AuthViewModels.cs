using System.ComponentModel.DataAnnotations;
using UTHMLibrary.Models;  // ← ADD THIS - FIXES THE ERRORS

namespace UTHMLibrary.ViewModels;

// ============================================
// AUTH VIEW MODELS
// ============================================

public class LoginViewModel
{
    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required, StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Matric Number")]
    public string MatricNumber { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Faculty")]
    public string Faculty { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class VerifyEmailViewModel
{
    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(6, MinimumLength = 6)]
    [Display(Name = "Verification Code")]
    public string Code { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(6, MinimumLength = 6)]
    public string Token { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResendVerificationViewModel
{
    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}

// ============================================
// ROOM BOOKING VIEW MODELS
// ============================================

public class StudyRoomBookingViewModel
{
    public int StudyRoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Booking Date")]
    public DateTime BookingDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Start Time")]
    public string StartTime { get; set; } = "09:00";

    [Required]
    [Display(Name = "End Time")]
    public string EndTime { get; set; } = "11:00";

    [Display(Name = "Purpose")]
    public string Purpose { get; set; } = string.Empty;
}

public class MeetingRoomBookingViewModel
{
    public int MeetingRoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Booking Date")]
    public DateTime BookingDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Start Time")]
    public string StartTime { get; set; } = "09:00";

    [Required]
    [Display(Name = "End Time")]
    public string EndTime { get; set; } = "11:00";

    [Required, StringLength(500)]
    [Display(Name = "Purpose")]
    public string Purpose { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Matric 1")]
    public string Matric1 { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Matric 2")]
    public string Matric2 { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Matric 3")]
    public string Matric3 { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Matric 4")]
    public string Matric4 { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Matric 5")]
    public string Matric5 { get; set; } = string.Empty;
}

// ============================================
// ADMIN VIEW MODELS
// ============================================

public class UploadResourceViewModel
{
    [Required, StringLength(200)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Category")]
    public string Category { get; set; } = "PastYearPaper";

    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Author")]
    public string Author { get; set; } = string.Empty;

    [Display(Name = "Faculty")]
    public string Faculty { get; set; } = string.Empty;

    [Display(Name = "Year")]
    public string Year { get; set; } = string.Empty;

    [Required]
    [Display(Name = "File")]
    public IFormFile? File { get; set; }
}

// ============================================
// DASHBOARD VIEW MODELS
// ============================================

public class DashboardViewModel
{
    public User? User { get; set; }
    public List<StudyRoomBooking>? StudyBookings { get; set; }
    public List<MeetingRoomBooking>? MeetingBookings { get; set; }
    public int ActiveStudyBookings { get; set; }
    public int PendingMeetingBookings { get; set; }
}

// ============================================
// ADMIN DASHBOARD VIEW MODELS
// ============================================

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalResources { get; set; }
    public int TotalStudyRooms { get; set; }
    public int AvailableStudyRooms { get; set; }
    public int TotalMeetingRooms { get; set; }
    public int PendingMeetingBookings { get; set; }
    public int ConfirmedStudyBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<StudyRoomBooking>? RecentStudyBookings { get; set; }
    public List<MeetingRoomBooking>? RecentMeetingBookings { get; set; }
}

// ============================================
// CHAT VIEW MODELS
// ============================================

public class ChatMessageViewModel
{
    public string Message { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string Sender { get; set; } = "user";
    public string Timestamp { get; set; } = string.Empty;
}

// ============================================
// ROOM MANAGEMENT VIEW MODELS
// ============================================

public class RoomManagementViewModel
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsAvailable { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}