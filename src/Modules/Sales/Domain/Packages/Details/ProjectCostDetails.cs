using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.Details;

// ItemId references cdc.project_cost_items via natural key JOIN to cdc.project_cost_categories.
public sealed class ProjectCostDetails : IVersionedDetails
{
    private ProjectCostDetails() { }

    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public int CategoryId { get; private set; }
    public int ItemId { get; private set; }
    public string? ItemDescription { get; private set; }

    public static ProjectCostDetails Create(int categoryId, int itemId, string? itemDescription = null)
    {
        return new ProjectCostDetails
        {
            CategoryId = categoryId,
            ItemId = itemId,
            ItemDescription = itemDescription
        };
    }
}
