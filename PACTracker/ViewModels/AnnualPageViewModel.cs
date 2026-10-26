using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PACTracker.Core.Energy.Services;
using System.Collections.ObjectModel;

namespace PACTracker.ViewModels
{
    public partial class AnnualPageViewModel(IEnergyService energyService) : ObservableObject
    {
        public ObservableCollection<YearCardViewModel> Cards { get; } = [];

        // Generates LoadCommand
        [RelayCommand]
        private async Task LoadAsync()
        {
            var currentYear = DateTime.Today.Year;
            var cards = new List<YearCardViewModel>();

            // Current year first, then the two previous ones
            for (var year = currentYear; year >= currentYear - 2; year--)
            {
                var summary = await energyService.GetAllRecordsByYearAsync(year);

                // The current year is always shown; older years only when they have data
                if (year != currentYear && !summary.HasData)
                    continue;

                cards.Add(new YearCardViewModel
                {
                    Year = year,
                    Production = summary.YearProduction,
                    Consumption = summary.YearConsumption,
                    Cop = summary.YearCop
                });
            }

            Cards.Clear();
            foreach (var card in cards)
                Cards.Add(card);
        }
    }
}
