// RVA.Client.Views/RaftingAddEditView.xaml.cs
using System.Windows;

namespace RVA.Client.Views
{
    public partial class RaftingAddEditView : Window
    {
        public RaftingAddEditView()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}