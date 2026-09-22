namespace WorldServer.Data;

public enum EquipmentSlot
{
    Weapon = 1,
    Head = 2,
    Body = 3,
    Gloves = 4,
    Legs = 5,
    Feet = 6,
    Accessory1 = 7,
    Accessory2 = 8
}

public sealed class CharacterEquipment
{
    public Guid Id { get; set; }

    public Guid CharacterId { get; set; }

    public EquipmentSlot Slot { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public int RefinementLevel { get; set; }

    public bool IsTradable { get; set; }

    public DateTime EquippedAtUtc { get; set; }

    public GameCharacter Character { get; set; } = null!;
}
