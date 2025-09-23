using RVA.Client.Services;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using RVA.Client.Commands;
using System.Windows.Input;

namespace RVA.Client.ViewModels
{
    public class RaftingStatsViewModel : BaseViewModel
    {
        private readonly WcfServiceClient _serviceClient;
        private DispatcherTimer _updateTimer;
        private string _statusMessage;
        private bool _isLoading;
        private int _plannedCount;
        private int _boardingCount;
        private int _paddlingCount;
        private int _restingCount;
        private int _finishedCount;

        private DateTime _startTime;

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

        // Kolekcije tačaka za Canvas
        public ObservableCollection<Point> PlannedPoints { get; } = new ObservableCollection<Point>();
        public ObservableCollection<Point> BoardingPoints { get; } = new ObservableCollection<Point>();
        public ObservableCollection<Point> PaddlingPoints { get; } = new ObservableCollection<Point>();
        public ObservableCollection<Point> RestingPoints { get; } = new ObservableCollection<Point>();
        public ObservableCollection<Point> FinishedPoints { get; } = new ObservableCollection<Point>();

        public ICommand StartMonitoringCommand { get; private set; }
        public ICommand StopMonitoringCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        public RaftingStatsViewModel() : this(null) { }

        public RaftingStatsViewModel(WcfServiceClient serviceClient = null)
        {
            _serviceClient = serviceClient ?? new WcfServiceClient();
            HistoryLog = new ObservableCollection<string>();
            _startTime = DateTime.Now;

            InitializeCommands();
            InitializeTimer();

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
                Interval = TimeSpan.FromSeconds(2)
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
            }
        }

        private void StopMonitoring()
        {
            if (_updateTimer.IsEnabled)
            {
                _updateTimer.Stop();
                StatusMessage = "Real-time monitoring stopped";
                AddToHistory("Monitoring stopped");
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
                    var elapsedSeconds = (currentTime - _startTime).TotalSeconds;

                    PlannedCount = statistics.ContainsKey(RaftingState.Planned) ? statistics[RaftingState.Planned] : 0;
                    BoardingCount = statistics.ContainsKey(RaftingState.Boarding) ? statistics[RaftingState.Boarding] : 0;
                    PaddlingCount = statistics.ContainsKey(RaftingState.Paddling) ? statistics[RaftingState.Paddling] : 0;
                    RestingCount = statistics.ContainsKey(RaftingState.Resting) ? statistics[RaftingState.Resting] : 0;
                    FinishedCount = statistics.ContainsKey(RaftingState.Finished) ? statistics[RaftingState.Finished] : 0;

                    // Dodavanje tačaka za iscrtavanje na Canvas-u
                    PlannedPoints.Add(new Point(elapsedSeconds, PlannedCount));
                    BoardingPoints.Add(new Point(elapsedSeconds, BoardingCount));
                    PaddlingPoints.Add(new Point(elapsedSeconds, PaddlingCount));
                    RestingPoints.Add(new Point(elapsedSeconds, RestingCount));
                    FinishedPoints.Add(new Point(elapsedSeconds, FinishedCount));

                    // Ograničavamo na poslednjih 50
                    TrimCollection(PlannedPoints);
                    TrimCollection(BoardingPoints);
                    TrimCollection(PaddlingPoints);
                    TrimCollection(RestingPoints);
                    TrimCollection(FinishedPoints);

                    var totalRaftings = raftings.Count();
                    StatusMessage = $"Updated: {currentTime:HH:mm:ss} | Total: {totalRaftings} | P:{PlannedCount} B:{BoardingCount} Pd:{PaddlingCount} R:{RestingCount} F:{FinishedCount}";
                    AddToHistory(StatusMessage);
                }
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
            OnDataUpdated();
        }

        private void TrimCollection(ObservableCollection<Point> collection)
        {
            while (collection.Count > 50)
                collection.RemoveAt(0);
        }

        private void AddToHistory(string message)
        {
            HistoryLog.Insert(0, message);
            while (HistoryLog.Count > 50)
                HistoryLog.RemoveAt(HistoryLog.Count - 1);
        }

        public event EventHandler DataUpdated;

        private void OnDataUpdated()
        {
            DataUpdated?.Invoke(this, EventArgs.Empty);
        }


        public void Cleanup()
        {
            _updateTimer?.Stop();
            //_serviceClient?.Dispose();
        }
    }
}
