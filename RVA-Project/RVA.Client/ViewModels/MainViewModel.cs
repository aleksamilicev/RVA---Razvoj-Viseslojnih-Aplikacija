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
                OnPropertyChanged();
            }
        }

        // Komande
        public ICommand TestConnectionCommand { get; }
        public ICommand DetailedTestCommand { get; }

        public MainViewModel()
        {
            /* Bio je neki bug, pa sam rucno prebacio kreirane fajlove u DataFiles folder
             *
            // 1. Generiši testne podatke
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataFiles");
            try
            {
                DataSeeder.SeedRaftingData(dataDir, 10);
                ConnectionStatus = "Test data generated successfully. Ready to test connection.";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Error generating test data: {ex.Message}";
            }
            */

            // 2. Inicijalizuj WCF servis
            _serviceClient = new WcfServiceClient();

            // 3. Inicijalizuj komande
            TestConnectionCommand = new RelayCommand(TestConnection);
            DetailedTestCommand = new RelayCommand(DetailedTest);
        }

        private void TestConnection()
        {
            try
            {
                ConnectionStatus = "Testing connection...";
                bool result = _serviceClient.TestConnection();
                ConnectionStatus = result ? "Connection successful!" : "Connection failed!";
            }
            catch (ServiceException ex)
            {
                ConnectionStatus = $"Service Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Unexpected Error: {ex.Message}";
            }
        }

        private void DetailedTest()
        {
            try
            {
                ConnectionStatus = "Running detailed connection test...\n";
                string details = _serviceClient.TestConnectionWithDetails();
                ConnectionStatus = $"Detailed Test Results:\n{details}";
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Detailed Test Error: {ex.Message}";
            }
        }

        public void Cleanup()
        {
            _serviceClient?.Dispose();
        }
    }
}