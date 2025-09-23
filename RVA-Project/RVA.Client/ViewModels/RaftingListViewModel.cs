using RVA.Client.Commands;
using RVA.Client.Services;
using RVA.Client.Views;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CommandManager = RVA.Client.Commands.CommandManager;

namespace RVA.Client.ViewModels
{
    public class RaftingListViewModel : BaseViewModel
    {
        #region Private Fields
        private readonly WcfServiceClient _serviceClient;
        private ObservableCollection<RaftingDto> _raftings;
        private RaftingDto _selectedRafting;
        private string _searchText;
        private RaftingState? _filterState;
        private bool _isLoading;
        private string _statusMessage;
        private ICollectionView _raftingsView;
        private DispatcherTimer _searchTimer; // Za debounce funkcionalnost
        private CommandManager _commandManager;
        private readonly Dictionary<int, SimulateRaftingCommand> _runningSimulations = new Dictionary<int, SimulateRaftingCommand>();
        #endregion

        #region Properties

        public CommandManager CommandManager
        {
            get => _commandManager;
            set => SetProperty(ref _commandManager, value);
        }

        

        public ObservableCollection<RaftingDto> Raftings
        {
            get => _raftings;
            set => SetProperty(ref _raftings, value);
        }

        public ICollectionView RaftingsView
        {
            get => _raftingsView;
            set => SetProperty(ref _raftingsView, value);
        }

        public RaftingDto SelectedRafting
        {
            get => _selectedRafting;
            set
            {
                SetProperty(ref _selectedRafting, value);
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ViewDetailsCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ChangeStateCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SimulateCommand).RaiseCanExecuteChanged();
                ((RelayCommand)StopSimulationCommand).RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                // Koristi debounce za bolje performanse
                _searchTimer?.Stop();
                _searchTimer?.Start();
            }
        }

        public RaftingState? FilterState
        {
            get => _filterState;
            set
            {
                SetProperty(ref _filterState, value);
                ApplyFilters();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Enum values for ComboBox binding - dodaj null opciju za "All States"
        public object[] RaftingStates
        {
            get
            {
                var states = new object[] { null }.Concat(Enum.GetValues(typeof(RaftingState)).Cast<object>()).ToArray();
                return states;
            }
        }
        #endregion

        #region Commands
        public ICommand LoadRaftingsCommand { get; }
        public ICommand AddNewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ChangeStateCommand { get; }

        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand ClearHistoryCommand { get; }
        public ICommand SimulateCommand { get; }
        public ICommand StopSimulationCommand { get; }
        public bool IsAnySimulationRunning => _runningSimulations.Any();
        #endregion

        #region Constructor

        public RaftingListViewModel() : this(null)
        {
            // prazno, koristi postojeći konstruktor
        }

        public RaftingListViewModel(WcfServiceClient serviceClient = null)
        {
            _serviceClient = serviceClient ?? new WcfServiceClient();
            Raftings = new ObservableCollection<RaftingDto>();
            _commandManager = new CommandManager(maxHistorySize: 100);

            // Initialize search timer for debounce
            _searchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300) // 300ms delay
            };
            _searchTimer.Tick += (s, e) =>
            {
                _searchTimer.Stop();
                ApplyFilters();
            };

            // Initialize commands
            LoadRaftingsCommand = new RelayCommand(_ => LoadRaftings());
            AddNewCommand = new RelayCommand(_ => AddNewRafting());
            EditCommand = new RelayCommand(_ => EditRafting(), _ => SelectedRafting != null);
            DeleteCommand = new RelayCommand(_ => DeleteRafting(), _ => SelectedRafting != null);
            ViewDetailsCommand = new RelayCommand(_ => ViewDetails(), _ => SelectedRafting != null);
            RefreshCommand = new RelayCommand(_ => LoadRaftings());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            ChangeStateCommand = new RelayCommand(param => ChangeRaftingState(param), _ => SelectedRafting != null);

            UndoCommand = new RelayCommand(_ => UndoLastCommand(), _ => CommandManager.CanUndo);
            RedoCommand = new RelayCommand(_ => RedoLastCommand(), _ => CommandManager.CanRedo);
            ClearHistoryCommand = new RelayCommand(_ => ClearCommandHistory());
            SimulateCommand = new RelayCommand(_ => StartSimulation(), _ => SelectedRafting != null && !IsSimulationRunning(SelectedRafting));
            StopSimulationCommand = new RelayCommand(_ => StopSimulation(), _ => SelectedRafting != null && IsSimulationRunning(SelectedRafting));

            _commandManager.PropertyChanged += (s, e) =>
            {
                ((RelayCommand)UndoCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RedoCommand).RaiseCanExecuteChanged();
            };


            // Setup collection view for filtering
            RaftingsView = CollectionViewSource.GetDefaultView(Raftings);

            // Auto-load data
            LoadRaftings();
        }
        #endregion

        #region Methods
        private async void LoadRaftings()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading raftings...";
                ClientLogger.Info("Loading raftings initiated");

                var raftings = _serviceClient.Execute(() => _serviceClient.RaftingService.GetAll(), "Load raftings");

                Raftings.Clear();
                foreach (var rafting in raftings ?? Enumerable.Empty<RaftingDto>())
                {
                    Raftings.Add(rafting);
                }

                StatusMessage = $"Loaded {Raftings.Count} raftings successfully.";
                ClientLogger.Info($"Successfully loaded {Raftings.Count} raftings");
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error loading raftings: {ex.Message}";
                ClientLogger.Error($"Failed to load raftings: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unexpected error: {ex.Message}";
                ClientLogger.Error($"Failed to load raftings: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddNewRafting()
        {
            OpenAddEditDialog();
        }

        private void StartSimulation()
        {
            if (SelectedRafting == null || IsSimulationRunning(SelectedRafting)) return;

            try
            {
                var simulateCommand = new SimulateRaftingCommand(_serviceClient, SelectedRafting, UpdateSimulationStatus);

                if (CommandManager.ExecuteCommand(simulateCommand))
                {
                    _runningSimulations[SelectedRafting.Id] = simulateCommand;
                    StatusMessage = $"Starting simulation for '{SelectedRafting.Name}'";
                    ClientLogger.Info($"Starting simulation for rafting ID: {SelectedRafting.Id}");

                    // Update command states
                    ((RelayCommand)SimulateCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopSimulationCommand).RaiseCanExecuteChanged();
                }
                else
                {
                    StatusMessage = "Failed to start simulation.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error starting simulation: {ex.Message}";
            }
        }

        private void StopSimulation()
        {
            if (SelectedRafting == null || !IsSimulationRunning(SelectedRafting)) return;

            try
            {
                if (_runningSimulations.TryGetValue(SelectedRafting.Id, out var simulateCommand))
                {
                    simulateCommand.Dispose();
                    _runningSimulations.Remove(SelectedRafting.Id);
                    StatusMessage = $"Simulation stopped for '{SelectedRafting.Name}'";

                    // Update command states
                    ((RelayCommand)SimulateCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopSimulationCommand).RaiseCanExecuteChanged();
                    RaftingsView.Refresh();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error stopping simulation: {ex.Message}";
            }
        }

        private bool IsSimulationRunning(RaftingDto rafting)
        {
            return rafting != null && _runningSimulations.ContainsKey(rafting.Id);
        }

        private void UpdateSimulationStatus(string message)
        {
            StatusMessage = message;

            // Refresh view to show state changes
            RaftingsView?.Refresh();

            // Update command states
            ((RelayCommand)SimulateCommand).RaiseCanExecuteChanged();
            ((RelayCommand)StopSimulationCommand).RaiseCanExecuteChanged();
        }

        private void OpenAddEditDialog(RaftingDto raftingToEdit = null)
        {
            try
            {
                var viewModel = new RaftingAddEditViewModel(_serviceClient, raftingToEdit);
                var dialog = new RaftingAddEditView { DataContext = viewModel };

                // Subscribe to save event
                viewModel.RaftingSaved += (sender, savedRafting) =>
                {
                    if (raftingToEdit == null)
                    {
                        // Adding new - use undoable command
                        var addCommand = new AddRaftingCommand(_serviceClient, Raftings, savedRafting);
                        if (CommandManager.ExecuteCommand(addCommand))
                        {
                            StatusMessage = $"Rafting '{savedRafting.Name}' added successfully.";
                            dialog.DialogResult = true;
                        }
                        else
                        {
                            StatusMessage = "Failed to add rafting.";
                        }
                    }
                    else
                    {
                        // Editing existing - use undoable command
                        var updateCommand = new UpdateRaftingCommand(_serviceClient, Raftings, raftingToEdit, savedRafting);
                        if (CommandManager.ExecuteCommand(updateCommand))
                        {
                            StatusMessage = $"Rafting '{savedRafting.Name}' updated successfully.";
                            dialog.DialogResult = true;
                        }
                        else
                        {
                            StatusMessage = "Failed to update rafting.";
                        }
                    }
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening add/edit dialog: {ex.Message}";
            }
        }

        private void EditRafting()
        {
            if (SelectedRafting == null) return;
            OpenAddEditDialog(SelectedRafting);
        }

        private void DeleteRafting()
        {
            if (SelectedRafting == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Deleting rafting: {SelectedRafting.Name}...";

                var deleteCommand = new DeleteRaftingCommand(_serviceClient, Raftings, SelectedRafting);
                if (CommandManager.ExecuteCommand(deleteCommand))
                {
                    var deletedName = SelectedRafting.Name;
                    SelectedRafting = null;
                    StatusMessage = $"Rafting '{deletedName}' deleted successfully.";
                    ClientLogger.Info($"Rafting deleted successfully: {deletedName}");
                }
                else
                {
                    StatusMessage = "Failed to delete rafting.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Unexpected error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ViewDetails()
        {
            if (SelectedRafting == null) return;

            // TODO: Navigate to details view
            StatusMessage = $"View details for: {SelectedRafting.Name} - to be implemented";
        }

        private void ChangeRaftingState(object parameter)
        {
            if (SelectedRafting == null || parameter == null) return;

            if (Enum.TryParse(parameter.ToString(), out RaftingState newState))
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = $"Changing state to {newState}...";

                    var stateCommand = new ChangeRaftingStateCommand(_serviceClient, SelectedRafting, newState);
                    if (CommandManager.ExecuteCommand(stateCommand))
                    {
                        StatusMessage = $"State changed to {newState} successfully.";
                        RaftingsView.Refresh();
                    }
                    else
                    {
                        StatusMessage = "Failed to change state.";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Unexpected error: {ex.Message}";
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void UndoLastCommand()
        {
            if (CommandManager.Undo())
            {
                StatusMessage = "Command undone successfully.";
                RaftingsView.Refresh();
            }
            else
            {
                StatusMessage = "Failed to undo command.";
            }
        }

        private void RedoLastCommand()
        {
            if (CommandManager.Redo())
            {
                StatusMessage = "Command redone successfully.";
                RaftingsView.Refresh();
            }
            else
            {
                StatusMessage = "Failed to redo command.";
            }
        }

        private void ClearCommandHistory()
        {
            CommandManager.ClearHistory();
            StatusMessage = "Command history cleared.";
        }

        private void ApplyFilters()
        {
            if (RaftingsView == null) return;

            RaftingsView.Filter = item =>
            {
                var rafting = item as RaftingDto;
                if (rafting == null) return false;

                // Text filter - proverava sva relevantna polja
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLower();

                    // Proveri glavna tekstualna polja
                    bool matchFound = false;

                    // Name
                    if (!string.IsNullOrEmpty(rafting.Name) &&
                        rafting.Name.ToLower().Contains(searchLower))
                        matchFound = true;

                    // Description
                    if (!matchFound && !string.IsNullOrEmpty(rafting.Description) &&
                        rafting.Description.ToLower().Contains(searchLower))
                        matchFound = true;

                    // Weather Conditions
                    if (!matchFound && !string.IsNullOrEmpty(rafting.WeatherConditions) &&
                        rafting.WeatherConditions.ToLower().Contains(searchLower))
                        matchFound = true;

                    // Current State
                    if (!matchFound && rafting.CurrentState.ToString().ToLower().Contains(searchLower))
                        matchFound = true;

                    // Distance (konvertuj u string za pretragu)
                    if (!matchFound && rafting.Distance.ToString().Contains(SearchText))
                        matchFound = true;

                    // Duration
                    if (!matchFound && rafting.Duration.ToString(@"hh\:mm").ToLower().Contains(searchLower))
                        matchFound = true;


                    // Current Intensity
                    if (!matchFound && rafting.CurrentIntensity.ToString().Contains(SearchText))
                        matchFound = true;

                    // Current Speed
                    if (!matchFound && rafting.CurrentSpeedKmh.ToString().Contains(SearchText))
                        matchFound = true;

                    // Price Per Person
                    if (!matchFound && rafting.PricePerPerson.ToString().Contains(SearchText))
                        matchFound = true;

                    // Participant Count
                    if (!matchFound && rafting.ParticipantCount.ToString().Contains(SearchText))
                        matchFound = true;

                    // Max Participants
                    if (!matchFound && rafting.MaxParticipants.ToString().Contains(SearchText))
                        matchFound = true;

                    // Capacity
                    if (!matchFound && rafting.Capacity.ToString().Contains(SearchText))
                        matchFound = true;

                    // Guide ID
                    if (!matchFound && rafting.GuideId.ToString().Contains(SearchText))
                        matchFound = true;

                    // Start and End Location IDs
                    if (!matchFound && rafting.StartLocationId.ToString().Contains(SearchText))
                        matchFound = true;

                    if (!matchFound && rafting.EndLocationId.ToString().Contains(SearchText))
                        matchFound = true;

                    // Start Time i End Time (formatirao kao string)
                    if (!matchFound && rafting.StartTime.ToString().ToLower().Contains(searchLower))
                        matchFound = true;

                    if (!matchFound && rafting.EndTime.ToString().ToLower().Contains(searchLower))
                        matchFound = true;

                    if (!matchFound)
                        return false;
                }

                // State filter
                if (FilterState.HasValue && rafting.CurrentState != FilterState.Value)
                {
                    return false;
                }

                return true;
            };

            StatusMessage = $"Showing {RaftingsView.Cast<object>().Count()} of {Raftings.Count} raftings";
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            FilterState = null;
            StatusMessage = $"Showing all {Raftings.Count} raftings";
        }
        #endregion

        #region Cleanup
        public void Cleanup()
        {
            // Stop all running simulations
            foreach (var simulation in _runningSimulations.Values)
            {
                simulation.Dispose();
            }
            _runningSimulations.Clear();

            _searchTimer?.Stop();
            _commandManager?.ClearHistory();
            _serviceClient?.Dispose();
        }
        #endregion
    }
}