using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StackExchange.Redis;
using WorldServer.Data;
using WorldServer.Health;

var builder = WebApplication.CreateBuilder(args);

var postgresConnectionString =
    builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Postgres is not configured.");

var redisConnectionString =
    builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Redis is not configured.");

builder.Services.AddSingleton(
    NpgsqlDataSource.Create(postgresConnectionString));

builder.Services.AddDbContext<WorldServerDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services
    .AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgres")
    .AddCheck<RedisHealthCheck>("redis");

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    error = entry.Value.Exception?.Message
                })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(payload));
    }
});

app.MapGet("/api/handshake", () => Results.Ok(new
{
    service = "WorldServer",
    protocolVersion = 1,
    serverVersion = "M0",
    status = "ready"
}));

app.MapGet("/api/characters", async (
    WorldServerDbContext db,
    CancellationToken cancellationToken) =>
{
    var characters = await db.Characters
        .AsNoTracking()
        .OrderBy(x => x.CreatedAtUtc)
        .Select(x => new
        {
            x.Id,
            x.Name,
            faction = x.Faction.ToString(),
            x.Level,
            x.DharmaPoints,
            x.CreatedAtUtc
        })
        .ToListAsync(cancellationToken);

    return Results.Ok(characters);
});

app.MapGet("/api/characters/{id:guid}", async (
    Guid id,
    WorldServerDbContext db,
    CancellationToken cancellationToken) =>
{
    var character = await db.Characters
        .AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new
        {
            x.Id,
            x.Name,
            faction = x.Faction.ToString(),
            x.Level,
            x.DharmaPoints,
            x.CreatedAtUtc
        })
        .SingleOrDefaultAsync(cancellationToken);

    return character is null
        ? Results.NotFound(new { error = "Character not found." })
        : Results.Ok(character);
});


app.MapGet("/api/characters/{id:guid}/equipment", async (
    Guid id,
    WorldServerDbContext db,
    CancellationToken cancellationToken) =>
{
    var characterExists = await db.Characters
        .AsNoTracking()
        .AnyAsync(x => x.Id == id, cancellationToken);

    if (!characterExists)
    {
        return Results.NotFound(new { error = "Character not found." });
    }

    var equipment = await db.CharacterEquipment
        .AsNoTracking()
        .Where(x => x.CharacterId == id)
        .OrderBy(x => x.Slot)
        .Select(x => new
        {
            x.Id,
            slot = x.Slot.ToString(),
            x.ItemCode,
            x.RefinementLevel,
            x.IsTradable,
            x.EquippedAtUtc
        })
        .ToListAsync(cancellationToken);

    return Results.Ok(equipment);
});


app.MapPut("/api/characters/{id:guid}/equipment/{slot}", async (
    Guid id,
    string slot,
    EquipCharacterRequest request,
    WorldServerDbContext db,
    CancellationToken cancellationToken) =>
{
    if (!Enum.TryParse<EquipmentSlot>(slot, true, out var equipmentSlot) ||
        !Enum.IsDefined(equipmentSlot))
    {
        return Results.BadRequest(new
        {
            error = "Invalid equipment slot."
        });
    }

    var itemCode = request.ItemCode.Trim();

    if (itemCode.Length < 1 || itemCode.Length > 64)
    {
        return Results.BadRequest(new
        {
            error = "ItemCode must be 1-64 characters."
        });
    }

    if (request.RefinementLevel < 0 || request.RefinementLevel > 8)
    {
        return Results.BadRequest(new
        {
            error = "RefinementLevel must be between 0 and 8."
        });
    }

    var characterExists = await db.Characters
        .AnyAsync(x => x.Id == id, cancellationToken);

    if (!characterExists)
    {
        return Results.NotFound(new { error = "Character not found." });
    }

    var equipment = await db.CharacterEquipment
        .SingleOrDefaultAsync(
            x => x.CharacterId == id && x.Slot == equipmentSlot,
            cancellationToken);

    if (equipment is null)
    {
        equipment = new CharacterEquipment
        {
            Id = Guid.NewGuid(),
            CharacterId = id,
            Slot = equipmentSlot
        };

        db.CharacterEquipment.Add(equipment);
    }

    equipment.ItemCode = itemCode;
    equipment.RefinementLevel = request.RefinementLevel;
    equipment.IsTradable = request.IsTradable;
    equipment.EquippedAtUtc = DateTime.UtcNow;

    await db.SaveChangesAsync(cancellationToken);

    return Results.Ok(new
    {
        equipment.Id,
        slot = equipment.Slot.ToString(),
        equipment.ItemCode,
        equipment.RefinementLevel,
        equipment.IsTradable,
        equipment.EquippedAtUtc
    });
});

app.MapPost("/api/characters", async (
    CreateCharacterRequest request,
    WorldServerDbContext db,
    CancellationToken cancellationToken) =>
{
    var name = request.Name.Trim();

    if (name.Length < 3 || name.Length > 64)
    {
        return Results.BadRequest(new
        {
            error = "Character name must be 3-64 characters."
        });
    }

    if (!Enum.IsDefined(typeof(Faction), request.Faction))
    {
        return Results.BadRequest(new
        {
            error = "Faction must be 1 (Raga), 2 (Sukma), or 3 (Wana)."
        });
    }

    var exists = await db.Characters
        .AnyAsync(x => x.Name == name, cancellationToken);

    if (exists)
    {
        return Results.Conflict(new
        {
            error = "Character name is already in use."
        });
    }

    var character = new GameCharacter
    {
        Id = Guid.NewGuid(),
        Name = name,
        Faction = (Faction)request.Faction,
        Level = 1,
        DharmaPoints = 0,
        CreatedAtUtc = DateTime.UtcNow
    };

    db.Characters.Add(character);
    await db.SaveChangesAsync(cancellationToken);

    return Results.Created(
        $"/api/characters/{character.Id}",
        new
        {
            character.Id,
            character.Name,
            faction = character.Faction.ToString(),
            character.Level,
            character.DharmaPoints,
            character.CreatedAtUtc
        });
});

app.Run();


public sealed record CreateCharacterRequest(
    string Name,
    int Faction);


public sealed record EquipCharacterRequest(
    string ItemCode,
    int RefinementLevel,
    bool IsTradable);
