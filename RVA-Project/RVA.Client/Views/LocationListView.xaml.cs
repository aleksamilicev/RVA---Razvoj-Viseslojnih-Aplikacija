using System.Windows.Controls;
using RVA.Client.ViewModels;

namespace RVA.Client.Views
{
    /// <summary>
    /// Interaction logic for LocationListView.xaml
    /// </summary>
    public partial class LocationListView : UserControl
    {
        public LocationListView()
        {
            InitializeComponent();
            // Opciono: možeš postaviti DataContext ovde ili iz parent kontrole
            // DataContext = new LocationListViewModel();
        }

        // Cleanup kada se control uništava
        private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is LocationListViewModel viewModel)
            {
                viewModel.Cleanup();
            }
        }
    }
}