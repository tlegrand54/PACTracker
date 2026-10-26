using PACTracker.ViewModels;

namespace PACTracker.Views;

public partial class EnergyPage : ContentPage
{
    private readonly EnergyViewModel _viewModel;

    public EnergyPage(EnergyViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // Reloads each time the page is shown, so new entries appear after returning from the form
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}