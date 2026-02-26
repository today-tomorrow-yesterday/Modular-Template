using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.ProjectCosts;

// ItemId references cdc.project_cost_items via natural key JOIN to cdc.project_cost_categories.
// Category/Item snapshot properties freeze CDC reference data at creation time (flat, like Home/Land).
// Snapshot properties are null for auto-generated PCs or legacy records.
public sealed class ProjectCostDetails : IVersionedDetails
{
    private ProjectCostDetails() { }

    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    // Keys
    public int CategoryId { get; private set; }
    public int ItemId { get; private set; }
    public string? ItemDescription { get; private set; }

    // Category snapshot (from cdc.project_cost_category at creation time)
    public string? CategoryDescription { get; private set; }
    public bool? CategoryIsCreditConsideration { get; private set; }
    public bool? CategoryIsLandDot { get; private set; }
    public bool? CategoryRestrictFha { get; private set; }
    public bool? CategoryRestrictCss { get; private set; }
    public bool? CategoryDisplayForCash { get; private set; }

    // Item snapshot (from cdc.project_cost_item at creation time)
    public string? ItemStatus { get; private set; }
    public bool? ItemIsFeeItem { get; private set; }
    public bool? ItemIsCssRestricted { get; private set; }
    public bool? ItemIsFhaRestricted { get; private set; }
    public bool? ItemIsDisplayForCash { get; private set; }
    public bool? ItemIsRestrictOptionPrice { get; private set; }
    public bool? ItemIsRestrictCssCost { get; private set; }
    public bool? ItemIsHopeRefundsIncluded { get; private set; }
    public decimal? ItemProfitPercentage { get; private set; }

    public static ProjectCostDetails Create(
        int categoryId,
        int itemId,
        string? itemDescription = null,
        // Category snapshot
        string? categoryDescription = null,
        bool? categoryIsCreditConsideration = null,
        bool? categoryIsLandDot = null,
        bool? categoryRestrictFha = null,
        bool? categoryRestrictCss = null,
        bool? categoryDisplayForCash = null,
        // Item snapshot
        string? itemStatus = null,
        bool? itemIsFeeItem = null,
        bool? itemIsCssRestricted = null,
        bool? itemIsFhaRestricted = null,
        bool? itemIsDisplayForCash = null,
        bool? itemIsRestrictOptionPrice = null,
        bool? itemIsRestrictCssCost = null,
        bool? itemIsHopeRefundsIncluded = null,
        decimal? itemProfitPercentage = null)
    {
        return new ProjectCostDetails
        {
            CategoryId = categoryId,
            ItemId = itemId,
            ItemDescription = itemDescription,
            CategoryDescription = categoryDescription,
            CategoryIsCreditConsideration = categoryIsCreditConsideration,
            CategoryIsLandDot = categoryIsLandDot,
            CategoryRestrictFha = categoryRestrictFha,
            CategoryRestrictCss = categoryRestrictCss,
            CategoryDisplayForCash = categoryDisplayForCash,
            ItemStatus = itemStatus,
            ItemIsFeeItem = itemIsFeeItem,
            ItemIsCssRestricted = itemIsCssRestricted,
            ItemIsFhaRestricted = itemIsFhaRestricted,
            ItemIsDisplayForCash = itemIsDisplayForCash,
            ItemIsRestrictOptionPrice = itemIsRestrictOptionPrice,
            ItemIsRestrictCssCost = itemIsRestrictCssCost,
            ItemIsHopeRefundsIncluded = itemIsHopeRefundsIncluded,
            ItemProfitPercentage = itemProfitPercentage
        };
    }
}
