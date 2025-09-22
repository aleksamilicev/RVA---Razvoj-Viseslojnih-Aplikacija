using System.Windows.Controls;
using RVA.Client.ViewModels;

namespace RVA.Client.Views
{
    public partial class RaftingStatsView : UserControl
    {
        public RaftingStatsView()
        {
            InitializeComponent();
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Cleanup when view is unloaded
            if (DataContext is RaftingStatsViewModel viewModel)
            {
                viewModel.Cleanup();
            }
        }
    }
}