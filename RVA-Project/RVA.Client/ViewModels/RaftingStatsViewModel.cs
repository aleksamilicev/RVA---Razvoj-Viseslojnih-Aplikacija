using RVA.Client.Services;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using RVA.Client.Commands;
using System.Windows.Input;

namespace RVA.Client.ViewModels
{
    public class RaftingStatsViewModel : BaseViewModel
    {
        private readonly WcfServiceClient _serviceClient;
        // Change the declaration of _updateTimer from readonly to non-readonly
        private DispatcherTimer _updateTimer;
        private string _statusMessage;
        private bool _isLoading;
        private int _plannedCount;
        private int _boardingCount;
        private int _paddlingCount;
        private int _restingCount;
        private int _finishedCount;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public int PlannedCount
        {
            get => _plannedCount;
            set => SetProperty(ref _plannedCount, value);
        }

        public int BoardingCount
        {
            get => _boardingCount;
            set => SetProperty(ref _boardingCount, value);
        }

        public int PaddlingCount
        {
            get => _paddlingCount;
            set => SetProperty(ref _paddlingCount, value);
        }

        public int RestingCount
        {
            get => _restingCount;
            set => SetProperty(ref _restingCount, value);
        }

        public int FinishedCount
        {
            get => _finishedCount;
            set => SetProperty(ref _finishedCount, value);
        }

        public ObservableCollection<string> HistoryLog { get; set; }

        public ICommand StartMonitoringCommand { get; private set; }
        public ICommand StopMonitoringCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        public RaftingStatsViewModel() : this(null)
        {
        }

        public RaftingStatsViewModel(WcfServiceClient serviceClient = null)
        {
            _serviceClient = serviceClient ?? new WcfServiceClient();
            HistoryLog = new ObservableCollection<string>();

            InitializeCommands();
            InitializeTimer();

            // Start monitoring by default
            StartMonitoring();

            StatusMessage = "Statistics initialized. Monitoring started.";
        }

        private void InitializeCommands()
        {
            StartMonitoringCommand = new RelayCommand(() => StartMonitoring(), () => !_updateTimer.IsEnabled);
            StopMonitoringCommand = new RelayCommand(() => StopMonitoring(), () => _updateTimer.IsEnabled);
            RefreshCommand = new RelayCommand(() => UpdateStatistics());
        }

        private void InitializeTimer()
        {
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2) // Update every 2 seconds
            };
            _updateTimer.Tick += UpdateTimer_Tick;
        }

        private void StartMonitoring()
        {
            if (!_updateTimer.IsEnabled)
            {
                _updateTimer.Start();
                StatusMessage = "Real-time monitoring started";
                AddToHistory("Monitoring started");

                ((RelayCommand)StartMonitoringCommand).RaiseCanExecuteChanged();
                ((RelayCommand)StopMonitoringCommand).RaiseCanExecuteChanged();
            }
        }

        private void StopMonitoring()
        {
            if (_updateTimer.IsEnabled)
            {
                _updateTimer.Stop();
                StatusMessage = "Real-time monitoring stopped";
                AddToHistory("Monitoring stopped");

                ((RelayCommand)StartMonitoringCommand).RaiseCanExecuteChanged();
                ((RelayCommand)StopMonitoringCommand).RaiseCanExecuteChanged();
            }
        }

        private async void UpdateTimer_Tick(object sender, EventArgs e)
        {
            await UpdateStatistics();
        }

        private async System.Threading.Tasks.Task UpdateStatistics()
        {
            try
            {
                IsLoading = true;

                var raftings = _serviceClient.Execute(() => _serviceClient.RaftingService.GetAll(), "Get raftings for statistics");

                if (raftings != null)
                {
                    var statistics = raftings.GroupBy(r => r.CurrentState)
                                           .ToDictionary(g => g.Key, g => g.Count());

                    var currentTime = DateTime.Now;

                    // Update counts
                    var newPlannedCount = statistics.ContainsKey(RaftingState.Planned) ? statistics[RaftingState.Planned] : 0;
                    var newBoardingCount = statistics.ContainsKey(RaftingState.Boarding) ? statistics[RaftingState.Boarding] : 0;
                    var newPaddlingCount = statistics.ContainsKey(RaftingState.Paddling) ? statistics[RaftingState.Paddling] : 0;
                    var newRestingCount = statistics.ContainsKey(RaftingState.Resting) ? statistics[RaftingState.Resting] : 0;
                    var newFinishedCount = statistics.ContainsKey(RaftingState.Finished) ? statistics[RaftingState.Finished] : 0;

                    // Check for changes
                    bool hasChanges = newPlannedCount != PlannedCount ||
                                    newBoardingCount != BoardingCount ||
                                    newPaddlingCount != PaddlingCount ||
                                    newRestingCount != RestingCount ||
                                    newFinishedCount != FinishedCount;

                    // Update properties
                    PlannedCount = newPlannedCount;
                    BoardingCount = newBoardingCount;
                    PaddlingCount = newPaddlingCount;
                    RestingCount = newRestingCount;
                    FinishedCount = newFinishedCount;

                    var totalRaftings = raftings.Count();
                    StatusMessage = $"Updated: {currentTime:HH:mm:ss} | Total: {totalRaftings} | P:{PlannedCount} B:{BoardingCount} Pd:{PaddlingCount} R:{RestingCount} F:{FinishedCount}";

                    // Add to history if there are changes or it's the first update
                    if (hasChanges || HistoryLog.Count == 0)
                    {
                        AddToHistory($"{currentTime:HH:mm:ss} - Total:{totalRaftings} P:{PlannedCount} B:{BoardingCount} Pd:{PaddlingCount} R:{RestingCount} F:{FinishedCount}");
                    }
                }
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Service error: {ex.Message}";
                AddToHistory($"{DateTime.Now:HH:mm:ss} - ERROR: {ex.Message}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Update error: {ex.Message}";
                AddToHistory($"{DateTime.Now:HH:mm:ss} - ERROR: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddToHistory(string message)
        {
            HistoryLog.Insert(0, message);

            // Keep only last 50 entries
            while (HistoryLog.Count > 50)
            {
                HistoryLog.RemoveAt(HistoryLog.Count - 1);
            }
        }

        public void Cleanup()
        {
            _updateTimer?.Stop();
            _serviceClient?.Dispose();
        }
    }
}