namespace Rtl.Core.Application.Seeding;

public sealed class SeedingOptions
{
    public const string SectionName = "Seeding";
    public bool Enabled { get; set; } = true;
    public int Seed { get; set; } = 12345;
    public List<string> Environments { get; set; } = ["Development"];
}
