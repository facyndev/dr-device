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
        builder.Property<string>(NormalizedEmail)
            .HasComputedColumnSql("lower(\"Email\")", stored: true);

        builder.HasIndex(NormalizedEmail)
            .IsUnique()
            .HasDatabaseName(EmailIndex);
    }
}
