#nullable enable

using System.Globalization;
using System.Resources;

namespace PACTracker.Resources.Translation;

public static class AppResources
{
    private static readonly ResourceManager ResourceManager = new(
        "PACTracker.Resources.Translation.AppResources",
        typeof(AppResources).Assembly);

    private static CultureInfo? resourceCulture;

    public static CultureInfo? Culture
    {
        get => resourceCulture;
        set => resourceCulture = value;
    }

    public static string HomeTabTitle => GetString(nameof(HomeTabTitle));
    public static string AnnualTabTitle => GetString(nameof(AnnualTabTitle));
    public static string MonthlyTabTitle => GetString(nameof(MonthlyTabTitle));
    public static string SettingsTabTitle => GetString(nameof(SettingsTabTitle));
    public static string MainPageTitle => GetString(nameof(MainPageTitle));
    public static string MainEnergyButton => GetString(nameof(MainEnergyButton));
    public static string MainComparisonButton => GetString(nameof(MainComparisonButton));
    public static string EnergyPageTitle => GetString(nameof(EnergyPageTitle));
    public static string AddEnergyButton => GetString(nameof(AddEnergyButton));
    public static string DateLabel => GetString(nameof(DateLabel));
    public static string ProductionLabel => GetString(nameof(ProductionLabel));
    public static string ConsumptionLabel => GetString(nameof(ConsumptionLabel));
    public static string ConsumptionShortLabel => GetString(nameof(ConsumptionShortLabel));
    public static string CopLabel => GetString(nameof(CopLabel));
    public static string MonthlyCopLabel => GetString(nameof(MonthlyCopLabel));
    public static string AddEnergyPageTitle => GetString(nameof(AddEnergyPageTitle));
    public static string ElectricityConsumptionLabel => GetString(nameof(ElectricityConsumptionLabel));
    public static string ConsumptionPlaceholder => GetString(nameof(ConsumptionPlaceholder));
    public static string HeatProductionLabel => GetString(nameof(HeatProductionLabel));
    public static string ProductionPlaceholder => GetString(nameof(ProductionPlaceholder));
    public static string CancelButton => GetString(nameof(CancelButton));
    public static string SaveButton => GetString(nameof(SaveButton));
    public static string InvalidReadingValueError => GetString(nameof(InvalidReadingValueError));
    public static string InvalidReadingOrderError => GetString(nameof(InvalidReadingOrderError));
    public static string AnnualPageTitle => GetString(nameof(AnnualPageTitle));
    public static string MonthlyComparisonPageTitle => GetString(nameof(MonthlyComparisonPageTitle));
    public static string ConsumptionMetricLabel => GetString(nameof(ConsumptionMetricLabel));
    public static string ProductionMetricLabel => GetString(nameof(ProductionMetricLabel));
    public static string CopMetricLabel => GetString(nameof(CopMetricLabel));
    public static string PeriodPickerTitle => GetString(nameof(PeriodPickerTitle));
    public static string MonthsColumnHeader => GetString(nameof(MonthsColumnHeader));
    public static string ChartTitleConsumption => GetString(nameof(ChartTitleConsumption));
    public static string ChartTitleProduction => GetString(nameof(ChartTitleProduction));
    public static string ChartTitleCop => GetString(nameof(ChartTitleCop));
    public static string ChartSubtitleUnitless => GetString(nameof(ChartSubtitleUnitless));
    public static string ChartSubtitleKwh => GetString(nameof(ChartSubtitleKwh));
    public static string UnitCaptionUnitless => GetString(nameof(UnitCaptionUnitless));
    public static string UnitCaptionKwh => GetString(nameof(UnitCaptionKwh));
    public static string AnnualCopTotal => GetString(nameof(AnnualCopTotal));
    public static string AnnualTotal => GetString(nameof(AnnualTotal));
    public static string PeriodNoteCop => GetString(nameof(PeriodNoteCop));
    public static string PeriodNoteTotal => GetString(nameof(PeriodNoteTotal));
    public static string VariationCaption => GetString(nameof(VariationCaption));
    public static string PeriodJanuaryToDecember => GetString(nameof(PeriodJanuaryToDecember));
    public static string PeriodJanuaryToJune => GetString(nameof(PeriodJanuaryToJune));
    public static string PeriodJulyToDecember => GetString(nameof(PeriodJulyToDecember));
    public static string PeriodJanuaryToMarch => GetString(nameof(PeriodJanuaryToMarch));
    public static string PeriodAprilToJune => GetString(nameof(PeriodAprilToJune));
    public static string PeriodJulyToSeptember => GetString(nameof(PeriodJulyToSeptember));
    public static string PeriodOctoberToDecember => GetString(nameof(PeriodOctoberToDecember));
    public static string SettingsPageTitle => GetString(nameof(SettingsPageTitle));
    public static string DataTransferHeading => GetString(nameof(DataTransferHeading));
    public static string DataTransferDescription => GetString(nameof(DataTransferDescription));
    public static string ExportButton => GetString(nameof(ExportButton));
    public static string ImportButton => GetString(nameof(ImportButton));
    public static string ImportPolicyDescription => GetString(nameof(ImportPolicyDescription));
    public static string ExportShareTitle => GetString(nameof(ExportShareTitle));
    public static string ExportReadyStatus => GetString(nameof(ExportReadyStatus));
    public static string ExportFailedStatus => GetString(nameof(ExportFailedStatus));
    public static string ExportAccessDeniedStatus => GetString(nameof(ExportAccessDeniedStatus));
    public static string ExportNotSupportedStatus => GetString(nameof(ExportNotSupportedStatus));
    public static string ImportPickerTitle => GetString(nameof(ImportPickerTitle));
    public static string ImportSuccessStatus => GetString(nameof(ImportSuccessStatus));
    public static string ImportFailedStatus => GetString(nameof(ImportFailedStatus));
    public static string ImportInvalidStatus => GetString(nameof(ImportInvalidStatus));
    public static string ImportAccessDeniedStatus => GetString(nameof(ImportAccessDeniedStatus));
    public static string ImportNotSupportedStatus => GetString(nameof(ImportNotSupportedStatus));

    private static string GetString(string name)
        => ResourceManager.GetString(name, resourceCulture) ?? name;
}
