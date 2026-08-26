using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ConferenceRoomConfiguration : IEntityTypeConfiguration<ConferenceRoom>
{
    public void Configure(EntityTypeBuilder<ConferenceRoom> builder)
    {
        builder.ToTable("conference_rooms");
        
        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cr => cr.Capacity)
            .IsRequired();

        builder.Property(cr => cr.HourlyRate)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasMany(cr => cr.Services)
            .WithMany(s => s.ConferenceRooms)
            .UsingEntity(j => j.ToTable("conference_room_services"));

        builder.HasIndex(cr => cr.Name);

        builder.HasIndex(cr => cr.Capacity);
        
        builder.HasData(
        new
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Зал A",
            Capacity = 50,
            HourlyRate = 2000m
        },
        new
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Зал B",
            Capacity = 100,
            HourlyRate = 3500m
        },
        new
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Зал C",
            Capacity = 30,
            HourlyRate = 1500m
        });
    }
}