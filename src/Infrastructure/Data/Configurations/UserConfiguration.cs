using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public const string NormalizedEmail = "NormalizedEmail";
    public const string EmailIndex = "IX_Users_NormalizedEmail";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.Address).HasMaxLength(500);
        builder.OwnsOne(user => user.Location, location =>
        {
            location.Property(value => value.Latitude).HasColumnName("Latitude").IsRequired();
            location.Property(value => value.Longitude).HasColumnName("Longitude").IsRequired();
        });

        builder.Property<string>(NormalizedEmail)
            .HasComputedColumnSql("lower(\"Email\")", stored: true);

        builder.HasIndex(NormalizedEmail)
            .IsUnique()
            .HasDatabaseName(EmailIndex);
    }
}
