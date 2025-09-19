using System.Windows;

namespace RVA.Client.Views
{
    public partial class ClothingAddEditView : Window
    {
        public ClothingAddEditView()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}