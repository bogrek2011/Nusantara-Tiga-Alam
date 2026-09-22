using Microsoft.EntityFrameworkCore;

namespace WorldServer.Data;

public sealed class WorldServerDbContext(
    DbContextOptions<WorldServerDbContext> options) : DbContext(options)
{
    public DbSet<GameCharacter> Characters => Set<GameCharacter>();

    public DbSet<CharacterEquipment> CharacterEquipment => Set<CharacterEquipment>();

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

        var equipment = modelBuilder.Entity<CharacterEquipment>();

        equipment.ToTable("character_equipment");

        equipment.HasKey(x => x.Id);

        equipment.Property(x => x.Slot)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        equipment.Property(x => x.ItemCode)
            .HasMaxLength(64)
            .IsRequired();

        equipment.Property(x => x.RefinementLevel)
            .HasDefaultValue(0)
            .IsRequired();

        equipment.Property(x => x.IsTradable)
            .HasDefaultValue(false)
            .IsRequired();

        equipment.Property(x => x.EquippedAtUtc)
            .IsRequired();

        equipment.HasIndex(x => new { x.CharacterId, x.Slot })
            .IsUnique();

        equipment.HasOne(x => x.Character)
            .WithMany()
            .HasForeignKey(x => x.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
