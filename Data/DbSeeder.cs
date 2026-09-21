using UTHMLibrary.Models;

namespace UTHMLibrary.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext db)
    {
        // Seed Admin
        if (!db.Users.Any(u => u.Role == "Admin"))
        {
            db.Users.Add(new User
            {
                FullName = "Library Administrator",
                Email = "admin@uthm.edu.my",
                MatricNumber = "ADMIN001",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Faculty = "UTHM Library",
                Phone = "07-4537000",
                Role = "Admin"
            });
        }

        // Seed sample student
        if (!db.Users.Any(u => u.Email == "student@uthm.edu.my"))
        {
            db.Users.Add(new User
            {
                FullName = "Ahmad Bin Ali",
                Email = "student@uthm.edu.my",
                MatricNumber = "AI210001",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                Faculty = "FSKTM",
                Phone = "0123456789",
                Role = "Student"
            });
        }

        // Seed Study Rooms (10 rooms)
        if (!db.StudyRooms.Any())
        {
            for (int i = 1; i <= 10; i++)
            {
                db.StudyRooms.Add(new StudyRoom
                {
                    RoomNumber = $"SR-{i:D2}",
                    Location = "Level 2, Perpustakaan Tunku Tun Aminah",
                    Capacity = i <= 5 ? 4 : 6,
                    IsAvailable = i % 3 != 0,
                    Price = 10.00m,
                    Description = $"Quiet study room for {(i <= 5 ? 4 : 6)} persons with whiteboard and power outlets."
                });
            }
        }

        // Seed Meeting Rooms (5 rooms)
        if (!db.MeetingRooms.Any())
        {
            for (int i = 1; i <= 5; i++)
            {
                db.MeetingRooms.Add(new MeetingRoom
                {
                    RoomNumber = $"MR-{i:D2}",
                    Location = "Level 3, Perpustakaan Tunku Tun Aminah",
                    Capacity = 10,
                    IsAvailable = true,
                    Description = "Meeting room with projector, whiteboard and video conferencing setup."
                });
            }
        }

        // Seed sample resources
        if (!db.Resources.Any())
        {
            var samples = new[]
            {
                new Resource { Title = "Data Structures Final Exam 2023", Category = "PastYearPaper", Author = "FSKTM", Faculty = "FSKTM", Year = "2023", FilePath = "/uploads/sample.pdf", OriginalFileName = "sample.pdf", Description = "Sample past year paper for BIC10404." },
                new Resource { Title = "Introduction to Algorithms", Category = "EBook", Author = "Thomas H. Cormen", Faculty = "FSKTM", Year = "2022", FilePath = "/uploads/sample.pdf", OriginalFileName = "sample.pdf", Description = "Classic algorithm textbook." },
                new Resource { Title = "IEEE Citation Guide", Category = "Resource", Author = "UTHM Library", Faculty = "General", Year = "2024", FilePath = "/uploads/sample.pdf", OriginalFileName = "sample.pdf", Description = "Reference guide for IEEE citation style." },
                new Resource { Title = "Smart Library System - FYP Report 2023", Category = "ProjectReport", Author = "Nurul Ain", Faculty = "FSKTM", Year = "2023", FilePath = "/uploads/sample.pdf", OriginalFileName = "sample.pdf", Description = "Undergraduate final year project report." }
            };
            db.Resources.AddRange(samples);
        }

        db.SaveChanges();
    }
}