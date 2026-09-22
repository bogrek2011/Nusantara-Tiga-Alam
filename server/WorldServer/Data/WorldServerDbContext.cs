using Microsoft.EntityFrameworkCore;

namespace WorldServer.Data;

public sealed class WorldServerDbContext(
    DbContextOptions<WorldServerDbContext> options) : DbContext(options)
{
    public DbSet<GameCharacter> Characters => Set<GameCharacter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var character = modelBuilder.Entity<GameCharacter>();

        character.ToTable("characters");

        character.HasKey(x => x.Id);

        character.Property(x => x.Name)
            .HasMaxLength(64)
            .IsRequired();

        character.HasIndex(x => x.Name)
            .IsUnique();

        character.Property(x => x.Faction)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        character.Property(x => x.Level)
            .HasDefaultValue(1)
            .IsRequired();

        character.Property(x => x.DharmaPoints)
            .HasDefaultValue(0L)
            .IsRequired();

        character.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
