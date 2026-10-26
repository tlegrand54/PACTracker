using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using PACTracker.Converters;
using PACTracker.Core.Energy.Services;
using PACTracker.Enums;
using PACTracker.Models;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Globalization;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

namespace PACTracker.ViewModels;

public partial class ComparatifGraphViewModel(IEnergyService energyService) : ObservableObject
{
    public double[] Values1 { get; set; } =
        [2, 1, 3, 5, 3, 4, 6];

    public int[] Values2 { get; set; } =
        [4, 2, 5, 2, 4, 5, 3];

    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");

    // Single source for the bar colors and the legend dots (current year first)
    private static readonly string[] YearColors = ["#4C2BD9", "#3B82F6", "#B7A5F2"];

    private static readonly PeriodOption[] AllPeriods =
    [
        new() { Label = "Janvier – Décembre", FirstMonth = 1, LastMonth = 12 },
        new() { Label = "Janvier – Juin", FirstMonth = 1, LastMonth = 6 },
        new() { Label = "Juillet – Décembre", FirstMonth = 7, LastMonth = 12 },
        new() { Label = "Janvier – Mars", FirstMonth = 1, LastMonth = 3 },
        new() { Label = "Avril – Juin", FirstMonth = 4, LastMonth = 6 },
        new() { Label = "Juillet – Septembre", FirstMonth = 7, LastMonth = 9 },
        new() { Label = "Octobre – Décembre", FirstMonth = 10, LastMonth = 12 }
    ];

    // The three displayed years, loaded by LoadAsync (12 months each)
    private YearSummaryModel _currentYearSummary = new();
    private YearSummaryModel _previousYearSummary = new();
    private YearSummaryModel _previousPreviousYearSummary = new();

    public int CurrentYear => DateTime.Today.Year;
    public int PreviousYear => CurrentYear - 1;
    public int PreviousPreviousYear => CurrentYear - 2;

    public Color CurrentYearColor => Color.FromArgb(YearColors[0]);
    public Color PreviousYearColor => Color.FromArgb(YearColors[1]);
    public Color PreviousPreviousYearColor => Color.FromArgb(YearColors[2]);

    public IReadOnlyList<PeriodOption> PeriodOptions => AllPeriods;

    [ObservableProperty] private PeriodOption selectedPeriod = AllPeriods[0];
    [ObservableProperty] private EnergyMetric selectedMetric = EnergyMetric.Consumption;

    // Texts that depend on the metric and the period
    [ObservableProperty] private string chartTitle = string.Empty;
    [ObservableProperty] private string chartSubtitle = string.Empty;
    [ObservableProperty] private string unitCaption = string.Empty;
    [ObservableProperty] private string periodNote = string.Empty;
    [ObservableProperty] private string totalLabel = "Total annuel";

    // Chart
    [ObservableProperty] private ISeries[] series = [];
    [ObservableProperty] private Axis[] xAxes = [];
    public Axis[] YAxes { get; } = [new Axis { MinLimit = 0 }];

    // Table
    public ObservableCollection<MonthComparisonRowViewModel> Rows { get; } = [];
    [ObservableProperty] private MonthComparisonRowViewModel totalRow = new();

    // Generates SelectMetricCommand (parameter: EnergyMetric)
    [RelayCommand]
    private void SelectMetric(EnergyMetric metric)
    {
        SelectedMetric = metric;
        Rebuild();
    }

    // Called by the toolkit whenever the Picker changes SelectedPeriod
    partial void OnSelectedPeriodChanged(PeriodOption value) => Rebuild();

    // Generates LoadCommand
    [RelayCommand]
    private async Task LoadAsync()
    {
        _currentYearSummary = (await energyService.GetAllRecordsByYearAsync(CurrentYear)).ConvertDtoToModel();
        _previousYearSummary = (await energyService.GetAllRecordsByYearAsync(PreviousYear)).ConvertDtoToModel();
        _previousPreviousYearSummary = (await energyService.GetAllRecordsByYearAsync(PreviousPreviousYear)).ConvertDtoToModel();

        Rebuild();
    }

    private void Rebuild()
    {
        YearSummaryModel[] years = [_currentYearSummary, _previousYearSummary, _previousPreviousYearSummary];
        var first = SelectedPeriod.FirstMonth;
        var last = SelectedPeriod.LastMonth;

        BuildTexts(first, last);
        BuildChart(years, first, last);
        BuildTable(years, first, last);
    }

    private void BuildTexts(int first, int last)
    {
        var isCop = SelectedMetric == EnergyMetric.Cop;
        var monthCount = last - first + 1;

        ChartTitle = SelectedMetric switch
        {
            EnergyMetric.Consumption => "Consommation électrique",
            EnergyMetric.Production => "Production de chaleur",
            _ => "Coefficient de performance"
        };

        ChartSubtitle = $"{(isCop ? "Sans unité" : "En kWh")} • Comparaison sur {monthCount} mois";
        UnitCaption = isCop ? "(sans unité)" : "(kWh)";
        TotalLabel = isCop ? "COP annuel" : "Total annuel";

        var noun = isCop ? "COP annuel" : "total annuel";
        PeriodNote = $"Le {noun} {CurrentYear} est calculé sur la période renseignée " +
                     $"({MonthName(first)} à {MonthName(last)}).";
    }

    private void BuildChart(YearSummaryModel[] years, int first, int last)
    {
        var months = Enumerable.Range(first, last - first + 1).ToArray();

        // One group of bars per month, one bar per year (current year first)
        Series = years
            .Select((year, index) => (ISeries)new ColumnSeries<double?>
            {
                Name = (CurrentYear - index).ToString(),
                Values = months.Select(m => GetValue(FindMonth(year, m))).ToArray(),
                Fill = new SolidColorPaint(SKColor.Parse(YearColors[index])),
                MaxBarWidth = 16
            })
            .ToArray();

        XAxes = [new Axis { Labels = months.Select(ShortMonthName).ToArray() }];
    }

    private void BuildTable(YearSummaryModel[] years, int first, int last)
    {
        Rows.Clear();

        for (var month = first; month <= last; month++)
        {
            Rows.Add(CreateRow(
                French.TextInfo.ToTitleCase(MonthName(month)),
                GetValue(FindMonth(years[0], month)),
                GetValue(FindMonth(years[1], month)),
                GetValue(FindMonth(years[2], month))));
        }

        TotalRow = CreateRow(
            TotalLabel,
            GetPeriodTotal(years[0], first, last),
            GetPeriodTotal(years[1], first, last),
            GetPeriodTotal(years[2], first, last));
    }

    private MonthComparisonRowViewModel CreateRow(
        string label, double? current, double? previous, double? previousPrevious)
    {
        var row = new MonthComparisonRowViewModel
        {
            Label = label,
            CurrentText = Format(current),
            PreviousText = Format(previous),
            PreviousPreviousText = Format(previousPrevious)
        };

        ApplyVariation(row, current, previous);
        return row;
    }

    // Variation of the current year against the previous one, in percent
    private void ApplyVariation(MonthComparisonRowViewModel row, double? current, double? previous)
    {
        if (current is null || previous is null || previous == 0)
            return; // keeps the defaults: "-", no arrow, gray

        var percent = (current.Value - previous.Value) / previous.Value * 100;

        // Lower is better for consumption; higher is better for production and COP
        var isImprovement = SelectedMetric == EnergyMetric.Consumption ? percent < 0 : percent > 0;

        row.VariationText = percent.ToString("+0.0;-0.0;0.0", French) + " %";
        row.VariationArrow = percent < 0 ? "↓" : percent > 0 ? "↑" : string.Empty;
        row.VariationCaption = $"vs {PreviousYear}";
        row.VariationColor = percent == 0
            ? Colors.Gray
            : Color.FromArgb(isImprovement ? "#16A34A" : "#DC2626");
    }

    private static MonthSummaryModel? FindMonth(YearSummaryModel year, int month)
        => year.MonthSummaries.FirstOrDefault(m => m.Month == month);

    private double? GetValue(MonthSummaryModel? month) => month is null ? null : SelectedMetric switch
    {
        EnergyMetric.Consumption => month.TotalConsumption,
        EnergyMetric.Production => month.TotalProduction,
        _ => month.MonthCop
    };

    // Total over the period; for COP it is total production / total consumption
    private double? GetPeriodTotal(YearSummaryModel year, int first, int last)
    {
        var months = year.MonthSummaries.Where(m => m.Month >= first && m.Month <= last).ToList();

        if (!months.Any(m => m.TotalProduction != 0 || m.TotalConsumption != 0))
            return null; // no data at all on this period

        var production = months.Sum(m => m.TotalProduction);
        var consumption = months.Sum(m => m.TotalConsumption);

        return SelectedMetric switch
        {
            EnergyMetric.Consumption => consumption,
            EnergyMetric.Production => production,
            _ => consumption > 0 ? production / consumption : (double?)null
        };
    }

    private string Format(double? value) => value is null
        ? "-"
        : SelectedMetric == EnergyMetric.Cop
            ? value.Value.ToString("F2", French)
            : value.Value.ToString("N0", French);

    private static string MonthName(int month) => French.DateTimeFormat.GetMonthName(month);

    private static string ShortMonthName(int month)
        => French.TextInfo.ToTitleCase(French.DateTimeFormat.GetAbbreviatedMonthName(month).TrimEnd('.'));
}