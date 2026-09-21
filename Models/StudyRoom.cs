using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UTHMLibrary.Models;

public class StudyRoom
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string RoomNumber { get; set; } = string.Empty;

    [StringLength(100)]
    public string Location { get; set; } = "Level 2, Main Library";

    public int Capacity { get; set; } = 4;

    public bool IsAvailable { get; set; } = true;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; } = 10.00m;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}

public class StudyRoomBooking
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int StudyRoomId { get; set; }
    public StudyRoom? StudyRoom { get; set; }

    public DateTime BookingDate { get; set; }

    [StringLength(10)]
    public string StartTime { get; set; } = string.Empty;

    [StringLength(10)]
    public string EndTime { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal AmountPaid { get; set; } = 10.00m;

    [StringLength(20)]
    public string PaymentStatus { get; set; } = "Paid";

    [StringLength(20)]
    public string Status { get; set; } = "Confirmed";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}