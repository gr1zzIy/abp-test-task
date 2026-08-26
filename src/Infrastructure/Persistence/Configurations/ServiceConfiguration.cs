using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");
        
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasIndex(s => s.Name)
            .IsUnique();
        
        builder.HasData(
        new
        {
            Id = 1,
            Name = "Проєктор",
            Price = 500m
        },
        new
        {
            Id = 2,
            Name = "Wi-Fi",
            Price = 300m
        },
        new
        {
            Id = 3,
            Name = "Звук",
            Price = 700m
        });
    }
}