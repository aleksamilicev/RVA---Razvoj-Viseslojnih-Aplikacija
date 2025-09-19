using RVA.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RVA.Client.Services
{
    public class NavigationService : INavigationService
    {
        private readonly ContentControl _mainContent;

        public NavigationService(ContentControl mainContent)
        {
            _mainContent = mainContent;
        }

        public void NavigateTo(object viewModel)
        {
            // Ovde pretpostavljamo da postoji DataTemplate za svaki ViewModel
            _mainContent.Content = viewModel;
        }
    }

}
