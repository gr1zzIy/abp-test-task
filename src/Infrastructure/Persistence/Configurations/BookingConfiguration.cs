using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        
        builder.HasKey(b => b.Id);

        builder.Property(b => b.StartTime)
            .IsRequired();

        builder.Property(b => b.EndTime)
            .IsRequired();

        builder.Property(b => b.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(b => b.UserId)
            .IsRequired();
        
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            // Видалення користувача не повинно знищувати історію бронювань.
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(b => b.ConferenceRoom)
            .WithMany(cr => cr.Bookings)
            .HasForeignKey(b => b.ConferenceRoomId)
            // Забороняємо каскадне видалення, щоб видалення залу
            // не призводило до втрати історії бронювань.
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.SelectedServices)
            .WithMany(s => s.Bookings)
            .UsingEntity(j => j.ToTable("booking_services"));

        // Індекс оптимізує пошук бронювань конкретного залу
        // у заданому часовому проміжку.
        builder.HasIndex(b => new
        {
            b.ConferenceRoomId,
            b.StartTime,
            b.EndTime
        });
        
        builder.HasIndex(b => b.UserId);
    }
}