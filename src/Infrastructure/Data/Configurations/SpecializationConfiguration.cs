using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class SpecializationConfiguration : IEntityTypeConfiguration<Specialization>
{
    public void Configure(EntityTypeBuilder<Specialization> builder)
    {
        // Clave compuesta: un User no puede tener dos veces la misma Category
        builder.HasKey(s => new { s.UserId, s.CategoryId });

        // Muchos Users → muchas Categories a través de Specialization
        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Category>()
               .WithMany()
               .HasForeignKey(s => s.CategoryId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
