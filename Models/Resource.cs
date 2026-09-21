using System.ComponentModel.DataAnnotations;

namespace UTHMLibrary.Models;

public class Resource
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Category { get; set; } = string.Empty;

    [StringLength(100)]
    public string Author { get; set; } = string.Empty;

    [StringLength(50)]
    public string Faculty { get; set; } = string.Empty;

    [StringLength(20)]
    public string Year { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(100)]
    public string OriginalFileName { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int DownloadCount { get; set; } = 0;
}