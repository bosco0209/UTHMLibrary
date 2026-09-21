using System.ComponentModel.DataAnnotations;

namespace UTHMLibrary.Models;

public class MeetingRoom
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string RoomNumber { get; set; } = string.Empty;

    [StringLength(100)]
    public string Location { get; set; } = "Level 3, Main Library";

    public int Capacity { get; set; } = 10;

    public bool IsAvailable { get; set; } = true;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}

public class MeetingRoomBooking
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int MeetingRoomId { get; set; }
    public MeetingRoom? MeetingRoom { get; set; }

    public DateTime BookingDate { get; set; }

    [StringLength(10)]
    public string StartTime { get; set; } = string.Empty;

    [StringLength(10)]
    public string EndTime { get; set; } = string.Empty;

    [StringLength(500)]
    public string Purpose { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string MemberMatrics { get; set; } = string.Empty;

    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    [StringLength(500)]
    public string AdminNote { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}