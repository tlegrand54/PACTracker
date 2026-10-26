using PACTracker.Views;

namespace PACTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(EnergyPage), typeof(EnergyPage));
            Routing.RegisterRoute(nameof(AddEnergyPage), typeof(AddEnergyPage));
        }
    }
}
