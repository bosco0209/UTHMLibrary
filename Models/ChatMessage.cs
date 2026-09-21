using System.ComponentModel.DataAnnotations;

namespace UTHMLibrary.Models;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Response { get; set; } = string.Empty;

    [StringLength(20)]
    public string Sender { get; set; } = "user";

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;
}