namespace UTHMLibrary.ViewModels;

public class UserManagementViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MatricNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
}