using Microsoft.Maui.Graphics;

namespace PACTracker.ViewModels;

// One table row (a month, or the total): already formatted for display
public class MonthComparisonRowViewModel
{
    public string Label { get; set; } = string.Empty;
    public string CurrentText { get; set; } = "-";
    public string PreviousText { get; set; } = "-";
    public string PreviousPreviousText { get; set; } = "-";

    public string VariationArrow { get; set; } = string.Empty;
    public string VariationText { get; set; } = "-";
    public string VariationCaption { get; set; } = string.Empty;
    public Color VariationColor { get; set; } = Colors.Gray;
}