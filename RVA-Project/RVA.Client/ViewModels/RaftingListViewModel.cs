using RVA.Client.Commands;
using RVA.Client.Services;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;

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
        #endregion

        #region Properties
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
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilters();
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

        // Enum values for ComboBox binding
        public Array RaftingStates => Enum.GetValues(typeof(RaftingState));
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

            // Initialize commands
            LoadRaftingsCommand = new RelayCommand(_ => LoadRaftings());
            AddNewCommand = new RelayCommand(_ => AddNewRafting());
            EditCommand = new RelayCommand(_ => EditRafting(), _ => SelectedRafting != null);
            DeleteCommand = new RelayCommand(_ => DeleteRafting(), _ => SelectedRafting != null);
            ViewDetailsCommand = new RelayCommand(_ => ViewDetails(), _ => SelectedRafting != null);
            RefreshCommand = new RelayCommand(_ => LoadRaftings());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            ChangeStateCommand = new RelayCommand(param => ChangeRaftingState(param), _ => SelectedRafting != null);

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

                var raftings = _serviceClient.Execute(() => _serviceClient.RaftingService.GetAll(), "Load raftings");

                Raftings.Clear();
                foreach (var rafting in raftings ?? Enumerable.Empty<RaftingDto>())
                {
                    Raftings.Add(rafting);
                }

                StatusMessage = $"Loaded {Raftings.Count} raftings successfully.";
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error loading raftings: {ex.Message}";
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

        private void AddNewRafting()
        {
            // TODO: Navigate to Add/Edit view with new rafting
            StatusMessage = "Add new rafting functionality - to be implemented";
        }

        private void EditRafting()
        {
            if (SelectedRafting == null) return;

            // TODO: Navigate to Add/Edit view with selected rafting
            StatusMessage = $"Edit rafting: {SelectedRafting.Name} - to be implemented";
        }

        private async void DeleteRafting()
        {
            if (SelectedRafting == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Deleting rafting: {SelectedRafting.Name}...";

                var success = _serviceClient.Execute(() =>
                    _serviceClient.RaftingService.Delete(SelectedRafting.Id),
                    "Delete rafting");

                if (success)
                {
                    Raftings.Remove(SelectedRafting);
                    SelectedRafting = null;
                    StatusMessage = "Rafting deleted successfully.";
                }
                else
                {
                    StatusMessage = "Failed to delete rafting.";
                }
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error deleting rafting: {ex.Message}";
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

        private async void ChangeRaftingState(object parameter)
        {
            if (SelectedRafting == null || parameter == null) return;

            if (Enum.TryParse(parameter.ToString(), out RaftingState newState))
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = $"Changing state to {newState}...";

                    var success = _serviceClient.Execute(() =>
                        _serviceClient.RaftingService.ChangeState(SelectedRafting.Id, newState),
                        "Change rafting state");

                    if (success)
                    {
                        SelectedRafting.CurrentState = newState;
                        SelectedRafting.ModifiedDate = DateTime.Now;
                        StatusMessage = $"State changed to {newState} successfully.";

                        // Refresh the view to show updated data
                        RaftingsView.Refresh();
                    }
                    else
                    {
                        StatusMessage = "Failed to change state.";
                    }
                }
                catch (ServiceException ex)
                {
                    StatusMessage = $"Error changing state: {ex.Message}";
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

        private void ApplyFilters()
        {
            if (RaftingsView == null) return;

            RaftingsView.Filter = item =>
            {
                var rafting = item as RaftingDto;
                if (rafting == null) return false;

                // Text filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLower();
                    if (!rafting.Name.ToLower().Contains(searchLower) &&
                        !rafting.Description.ToLower().Contains(searchLower) &&
                        !rafting.WeatherConditions.ToLower().Contains(searchLower))
                    {
                        return false;
                    }
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
            _serviceClient?.Dispose();
        }
        #endregion
    }
}