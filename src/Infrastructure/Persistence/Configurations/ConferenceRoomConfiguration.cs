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
    }
}