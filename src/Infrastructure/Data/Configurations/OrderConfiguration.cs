using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // Un User puede tener muchas Orders, pero una Order pertenece a un solo User
        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        // Un Technician (User) puede estar en muchas Orders, pero una Order tiene un solo Technician (opcional)
        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(o => o.TechnicianId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        // Una Order pertenece a una Category
        builder.HasOne<Category>()
               .WithMany()
               .HasForeignKey(o => o.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
