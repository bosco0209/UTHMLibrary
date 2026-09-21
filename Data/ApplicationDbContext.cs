using Microsoft.EntityFrameworkCore;
using UTHMLibrary.Models;

namespace UTHMLibrary.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<StudyRoom> StudyRooms => Set<StudyRoom>();
    public DbSet<StudyRoomBooking> StudyRoomBookings => Set<StudyRoomBooking>();
    public DbSet<MeetingRoom> MeetingRooms => Set<MeetingRoom>();
    public DbSet<MeetingRoomBooking> MeetingRoomBookings => Set<MeetingRoomBooking>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();  // ← ADD THIS LINE
    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();
        builder.Entity<User>()
            .HasIndex(u => u.MatricNumber).IsUnique();

        builder.Entity<StudyRoom>()
            .HasIndex(r => r.RoomNumber).IsUnique();
        builder.Entity<MeetingRoom>()
            .HasIndex(r => r.RoomNumber).IsUnique();

        builder.Entity<StudyRoomBooking>()
            .HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId);
        builder.Entity<StudyRoomBooking>()
            .HasOne(b => b.StudyRoom).WithMany().HasForeignKey(b => b.StudyRoomId);

        builder.Entity<MeetingRoomBooking>()
            .HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId);
        builder.Entity<MeetingRoomBooking>()
            .HasOne(b => b.MeetingRoom).WithMany().HasForeignKey(b => b.MeetingRoomId);

        builder.Entity<StudyRoom>().Property(r => r.Price).HasColumnType("decimal(10,2)");
        builder.Entity<StudyRoomBooking>().Property(b => b.AmountPaid).HasColumnType("decimal(10,2)");
    }
}