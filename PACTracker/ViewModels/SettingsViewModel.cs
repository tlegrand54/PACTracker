using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PACTracker.Core.Energy.Services;
using System.Text;

namespace PACTracker.ViewModels;

public partial class SettingsViewModel(IEnergyService energyService) : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    [NotifyCanExecuteChangedFor(nameof(ImportCommand))]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    private bool CanStartOperation() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanStartOperation))]
    private async Task ExportAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            var json = await energyService.ExportDataAsync();
            var filePath = Path.Combine(
                FileSystem.CacheDirectory,
                $"pactracker-export-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            await File.WriteAllTextAsync(filePath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Exporter les données PACTracker",
                File = new ShareFile(filePath, "application/json")
            });

            StatusMessage = "Export prêt. Choisissez où enregistrer ou partager le fichier.";
        }
        catch (IOException)
        {
            StatusMessage = "Impossible de créer ou partager le fichier d’export.";
        }
        catch (UnauthorizedAccessException)
        {
            StatusMessage = "L’accès au fichier d’export a été refusé.";
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "Le partage de fichiers n’est pas disponible sur cet appareil.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStartOperation))]
    private async Task ImportAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Sélectionner un export PACTracker"
            });

            if (file is null)
            {
                return;
            }

            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var importedCount = await energyService.ImportDataAsync(json);

            StatusMessage = $"{importedCount} mesure(s) importée(s). Les mesures des mêmes dates ont été remplacées.";
        }
        catch (InvalidDataException exception)
        {
            StatusMessage = exception.Message;
        }
        catch (IOException)
        {
            StatusMessage = "Impossible de lire le fichier sélectionné.";
        }
        catch (UnauthorizedAccessException)
        {
            StatusMessage = "L’accès au fichier sélectionné a été refusé.";
        }
        catch (FeatureNotSupportedException)
        {
            StatusMessage = "La sélection de fichiers n’est pas disponible sur cet appareil.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
