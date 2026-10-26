using PACTracker.ViewModels;

namespace PACTracker.Views;

public partial class AnnualPage : ContentPage
{
    private readonly AnnualPageViewModel _viewModel;
        
    public AnnualPage(AnnualPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // Reloads each time the tab is selected, so new entries are taken into account
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}