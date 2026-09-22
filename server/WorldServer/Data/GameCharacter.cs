namespace WorldServer.Data;

public enum Faction
{
    Raga = 1,
    Sukma = 2,
    Wana = 3
}

public sealed class GameCharacter
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Faction Faction { get; set; }

    public int Level { get; set; } = 1;

    public long DharmaPoints { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
