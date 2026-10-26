using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PACTracker.Core.Energy.Services;
using PACTracker.Converters;
using PACTracker.Models;
using PACTracker.Views;
using System.Collections.ObjectModel;

namespace PACTracker.ViewModels;

public partial class EnergyViewModel(IEnergyService energyService) : ObservableObject
{
    // Records of the current month, shown in the table
    public ObservableCollection<EnergyModel> Records { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthCop))]
    private double totalConsumption;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthCop))]
    private double totalProduction;

    // COP computed on monthly totals, not averaged from daily values
    public double? MonthCop => TotalConsumption > 0 ? TotalProduction / TotalConsumption : null;

    // Generates LoadCommand
    [RelayCommand]
    private async Task LoadAsync()
    {
        var today = DateTime.Today;
        var items = await energyService.GetAllRecordsByMonthAsync(today.Year, today.Month);

        Records.Clear();
        foreach (var item in items)
            Records.Add(item.ConvertEntityToModel());

        TotalConsumption = items.OrderByDescending(r => r.Date).FirstOrDefault()?.Consumption ?? 0;
        TotalProduction = items.OrderByDescending(r => r.Date).FirstOrDefault()?.Production ?? 0;
    }

    // Generates GoBackCommand
    [RelayCommand]
    private Task GoBackAsync() => Shell.Current.GoToAsync("..");

    // Generates AddEntryCommand
    [RelayCommand]
    private Task AddEntryAsync() => Shell.Current.GoToAsync(nameof(AddEnergyPage));
}