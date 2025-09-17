using System.Windows.Controls;
using RVA.Client.ViewModels;

namespace RVA.Client.Views
{
    /// <summary>
    /// Interaction logic for RaftingListView.xaml
    /// </summary>
    public partial class RaftingListView : UserControl
    {
        public RaftingListView()
        {
            InitializeComponent();

            // Opciono: možeš postaviti DataContext ovde ili iz parent kontrole
            // DataContext = new RaftingListViewModel();
        }

        // Cleanup kada se control uništava
        private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is RaftingListViewModel viewModel)
            {
                viewModel.Cleanup();
            }
        }
    }
}