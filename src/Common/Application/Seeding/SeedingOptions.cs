namespace Rtl.Core.Application.Seeding;

public sealed class SeedingOptions
{
    public const string SectionName = "Seeding";
    public bool Enabled { get; set; }
    public int Seed { get; set; } = 12345; //Controls Bodgus random number generator
}
