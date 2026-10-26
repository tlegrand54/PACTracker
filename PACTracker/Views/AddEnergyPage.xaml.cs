using PACTracker.ViewModels;

namespace PACTracker.Views;

public partial class AddEnergyPage : ContentPage
{
    public AddEnergyPage(AddEnergyViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;

    }
}