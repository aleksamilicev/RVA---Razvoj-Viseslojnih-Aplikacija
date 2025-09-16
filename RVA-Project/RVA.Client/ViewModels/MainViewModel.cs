using GalaSoft.MvvmLight.Command;
using MvvmHelpers;
using RVA.Client.Helpers;
using RVA.Client.Services;
using System;
using System.IO;
using System.Windows.Input;

namespace RVA.Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly WcfServiceClient _serviceClient;

        private string _connectionStatus;
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                _connectionStatus = value;
                OnPropertyChanged(); // iz BaseViewModel
            }
        }

        // Command za testiranje konekcije
        public ICommand TestConnectionCommand { get; }

        public MainViewModel()
        {
            // 1. Generiši testne podatke
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataFiles");
            DataSeeder.SeedRaftingData(dataDir, 10); // generiše 10 rafting aktivnosti

            // 2. Inicijalizuj WCF servis
            _serviceClient = new WcfServiceClient();

            // 3. Inicijalizuj komandu
            TestConnectionCommand = new RelayCommand(TestConnection);
        }

        private void TestConnection()
        {
            try
            {
                bool result = _serviceClient.TestConnection();
                ConnectionStatus = result ? "Connection successful!" : "Connection failed!";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Error: {ex.Message}";
            }
        }

        // Po potrebi dispose servisa
        public void Cleanup()
        {
            _serviceClient.Dispose();
        }
    }
}
