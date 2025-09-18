using System.Windows.Controls;
using RVA.Client.ViewModels;

namespace RVA.Client.Views
{
    /// <summary>
    /// Interaction logic for ClothingListView.xaml
    /// </summary>
    public partial class ClothingListView : UserControl
    {
        public ClothingListView()
        {
            InitializeComponent();
            // Opciono: možeš postaviti DataContext ovde ili iz parent kontrole
            // DataContext = new ClothingListViewModel();
        }

        // Cleanup kada se control uništava
        private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ClothingListViewModel viewModel)
            {
                viewModel.Cleanup();
            }
        }
    }
}