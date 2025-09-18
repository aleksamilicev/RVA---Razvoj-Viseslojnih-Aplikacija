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
    public class LocationListViewModel : BaseViewModel
    {
        #region Private Fields
        private readonly WcfServiceClient _serviceClient;
        private ObservableCollection<LocationDto> _locations;
        private LocationDto _selectedLocation;
        private string _searchText;
        private bool? _filterHasParking;
        private bool _isLoading;
        private string _statusMessage;
        private ICollectionView _locationsView;
        #endregion

        #region Properties
        public ObservableCollection<LocationDto> Locations
        {
            get => _locations;
            set => SetProperty(ref _locations, value);
        }

        public ICollectionView LocationsView
        {
            get => _locationsView;
            set => SetProperty(ref _locationsView, value);
        }

        public LocationDto SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                SetProperty(ref _selectedLocation, value);
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ViewDetailsCommand).RaiseCanExecuteChanged();
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

        public bool? FilterHasParking
        {
            get => _filterHasParking;
            set
            {
                SetProperty(ref _filterHasParking, value);
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


        #endregion

        #region Commands
        public ICommand LoadLocationsCommand { get; }
        public ICommand AddNewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        #endregion

        #region Constructor

        public LocationListViewModel() : this(null)
        {
            // prazno, koristi postojeći konstruktor
        }

        public LocationListViewModel(WcfServiceClient serviceClient = null)
        {
            _serviceClient = serviceClient ?? new WcfServiceClient();
            Locations = new ObservableCollection<LocationDto>();

            // Initialize commands
            LoadLocationsCommand = new RelayCommand(_ => LoadLocations());
            AddNewCommand = new RelayCommand(_ => AddNewLocation());
            EditCommand = new RelayCommand(_ => EditLocation(), _ => SelectedLocation != null);
            DeleteCommand = new RelayCommand(_ => DeleteLocation(), _ => SelectedLocation != null);
            ViewDetailsCommand = new RelayCommand(_ => ViewDetails(), _ => SelectedLocation != null);
            RefreshCommand = new RelayCommand(_ => LoadLocations());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());

            // Setup collection view for filtering
            LocationsView = CollectionViewSource.GetDefaultView(Locations);

            // Auto-load data
            LoadLocations();
        }
        #endregion

        #region Methods
        private async void LoadLocations()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading locations...";

                var locations = _serviceClient.Execute(() => _serviceClient.LocationService.GetAll(), "Load locations");

                Locations.Clear();
                foreach (var location in locations ?? Enumerable.Empty<LocationDto>())
                {
                    Locations.Add(location);
                }

                StatusMessage = $"Loaded {Locations.Count} locations successfully.";
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error loading locations: {ex.Message}";
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

        private void AddNewLocation()
        {
            // TODO: Navigate to Add/Edit view with new location
            StatusMessage = "Add new location functionality - to be implemented";
        }

        private void EditLocation()
        {
            if (SelectedLocation == null) return;

            // TODO: Navigate to Add/Edit view with selected location
            StatusMessage = $"Edit location: {SelectedLocation.Name} - to be implemented";
        }

        private async void DeleteLocation()
        {
            if (SelectedLocation == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Deleting location: {SelectedLocation.Name}...";

                var success = _serviceClient.Execute(() =>
                    _serviceClient.LocationService.Delete(SelectedLocation.Id),
                    "Delete location");

                if (success)
                {
                    Locations.Remove(SelectedLocation);
                    SelectedLocation = null;
                    StatusMessage = "Location deleted successfully.";
                }
                else
                {
                    StatusMessage = "Failed to delete location.";
                }
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error deleting location: {ex.Message}";
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
            if (SelectedLocation == null) return;

            // TODO: Navigate to details view
            StatusMessage = $"View details for: {SelectedLocation.Name} - to be implemented";
        }

        private void ApplyFilters()
        {
            if (LocationsView == null) return;

            LocationsView.Filter = item =>
            {
                var location = item as LocationDto;
                if (location == null) return false;

                // Text filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLower();
                    if (!location.Name.ToLower().Contains(searchLower) &&
                        !location.Description.ToLower().Contains(searchLower) &&
                        !location.River.ToLower().Contains(searchLower))
                    {
                        return false;
                    }
                }

                // Parking filter
                if (FilterHasParking.HasValue && location.HasParking != FilterHasParking.Value)
                {
                    return false;
                }

                return true;
            };

            StatusMessage = $"Showing {LocationsView.Cast<object>().Count()} of {Locations.Count} locations";
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            FilterHasParking = null;
            StatusMessage = $"Showing all {Locations.Count} locations";
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