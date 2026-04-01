namespace ModularTemplate.Application.Seeding;

public sealed class SeedingOptions
{
    public const string SectionName = "Seeding";
    public bool Enabled { get; set; }
    public int Seed { get; set; } = 12345;
}
