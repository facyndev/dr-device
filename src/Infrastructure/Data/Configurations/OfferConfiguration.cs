using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        // Un Technician (User) puede tener muchas Offers, pero una Offer pertenece a un solo Technician
        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(o => o.TechnicianId)
               .OnDelete(DeleteBehavior.Restrict);

        // Una Offer pertenece a una Order
        builder.HasOne<Order>()
               .WithMany()
               .HasForeignKey(o => o.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
