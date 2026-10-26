using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PACTracker.Core.Energy.Services;
using PACTracker.Models;

namespace PACTracker.ViewModels
{
    public partial class AddEnergyViewModel(IEnergyService energyService) : ObservableObject
    {
        public DateTime MaximumDate => DateTime.Today;

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        [ObservableProperty]
        private string consumptionText = string.Empty;

        [ObservableProperty]
        private string productionText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string errorMessage = string.Empty;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!TryParse(ConsumptionText, out var consumption) ||
                !TryParse(ProductionText, out var production))
            {
                ErrorMessage = "Veuillez saisir des nombres positifs valides.";
                return;
            }

            ErrorMessage = string.Empty;

            // An existing entry for the same day is replaced by the service
            await energyService.SaveRecordAsync( SelectedDate.Date, consumption, production);

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public Task CancelAsync() => Shell.Current.GoToAsync("..");

        private bool TryParse(string text, out double value)
        {
            value = 0;

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return double.TryParse(text.Replace(',','.'), System.Globalization.CultureInfo.InvariantCulture, out value);
        }
    }
}
