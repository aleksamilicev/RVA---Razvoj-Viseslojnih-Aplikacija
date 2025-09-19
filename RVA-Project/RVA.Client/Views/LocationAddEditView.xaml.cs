using RVA.Client.ViewModels;
using System.Windows;

namespace RVA.Client.Views
{
    public partial class LocationAddEditView : Window
    {
        private LocationAddEditViewModel ViewModel => DataContext as LocationAddEditViewModel;

        public LocationAddEditView()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // Coordinate helper methods
        private void SetTaraCoordinates(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Latitude = 43.2889;
                ViewModel.Longitude = 19.4658;
            }
        }

        private void SetDrinaCoordinates(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Latitude = 43.9159;
                ViewModel.Longitude = 19.3155;
            }
        }

        private void SetUnaCoordinates(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Latitude = 44.5589;
                ViewModel.Longitude = 16.1647;
            }
        }

        private void SetIbarCoordinates(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Latitude = 43.2341;
                ViewModel.Longitude = 20.9216;
            }
        }

        private void SetBelgradeCoordinates(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Latitude = 44.0165;
                ViewModel.Longitude = 21.0059;
            }
        }
    }
}