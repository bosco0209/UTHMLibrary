using System.ComponentModel.DataAnnotations;

namespace UTHMLibrary.Models;

public class EmailVerification
{
    [Key]
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiryAt { get; set; } = DateTime.UtcNow.AddMinutes(15);

    public bool IsUsed { get; set; } = false;

    public bool IsVerified { get; set; } = false;
}