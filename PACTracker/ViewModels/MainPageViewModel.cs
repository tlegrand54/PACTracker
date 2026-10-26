using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PACTracker.Views;

namespace PACTracker.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {

        // Generates GoToEnergyCommand
        [RelayCommand]
        private Task GoToEnergyAsync()
            => Shell.Current.GoToAsync(nameof(EnergyPage));

        // Generates GoToComparisonCommand
        [RelayCommand]
        private Task GoToComparisonAsync()
            => Shell.Current.GoToAsync(nameof(ComparatifGraphPage));

    }
}
