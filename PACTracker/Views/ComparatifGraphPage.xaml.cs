using PACTracker.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace PACTracker.Views;

public partial class ComparatifGraphPage : ContentPage
{
	private readonly ComparatifGraphViewModel _viewModel;

    public ComparatifGraphPage(ComparatifGraphViewModel viewModel)
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