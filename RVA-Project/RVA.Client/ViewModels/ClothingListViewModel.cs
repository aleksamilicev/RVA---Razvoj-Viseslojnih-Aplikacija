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
    public class ClothingListViewModel : BaseViewModel
    {
        #region Private Fields
        private readonly WcfServiceClient _serviceClient;
        private ObservableCollection<ClothingDto> _clothing;
        private ClothingDto _selectedClothing;
        private string _searchText;
        private ClothingType? _filterType;
        private bool? _filterIsAvailable;
        private bool? _filterIsWaterproof;
        private bool _isLoading;
        private string _statusMessage;
        private ICollectionView _clothingView;
        #endregion

        #region Properties
        public ObservableCollection<ClothingDto> Clothing
        {
            get => _clothing;
            set => SetProperty(ref _clothing, value);
        }

        public ICollectionView ClothingView
        {
            get => _clothingView;
            set => SetProperty(ref _clothingView, value);
        }

        public ClothingDto SelectedClothing
        {
            get => _selectedClothing;
            set
            {
                SetProperty(ref _selectedClothing, value);
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ViewDetailsCommand).RaiseCanExecuteChanged();
                ((RelayCommand)MarkAsUsedCommand).RaiseCanExecuteChanged();
                ((RelayCommand)MarkAsAvailableCommand).RaiseCanExecuteChanged();
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

        public ClothingType? FilterType
        {
            get => _filterType;
            set
            {
                SetProperty(ref _filterType, value);
                ApplyFilters();
            }
        }

        public bool? FilterIsAvailable
        {
            get => _filterIsAvailable;
            set
            {
                SetProperty(ref _filterIsAvailable, value);
                ApplyFilters();
            }
        }

        public bool? FilterIsWaterproof
        {
            get => _filterIsWaterproof;
            set
            {
                SetProperty(ref _filterIsWaterproof, value);
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
        public Array ClothingTypes => Enum.GetValues(typeof(ClothingType));
        #endregion

        #region Commands
        public ICommand LoadClothingCommand { get; }
        public ICommand AddNewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand MarkAsUsedCommand { get; }
        public ICommand MarkAsAvailableCommand { get; }
        #endregion

        #region Constructor

        public ClothingListViewModel() : this(null)
        {
            // prazno, koristi postojeći konstruktor
        }

        public ClothingListViewModel(WcfServiceClient serviceClient = null)
        {
            _serviceClient = serviceClient ?? new WcfServiceClient();
            Clothing = new ObservableCollection<ClothingDto>();

            // Initialize commands
            LoadClothingCommand = new RelayCommand(_ => LoadClothing());
            AddNewCommand = new RelayCommand(_ => AddNewClothing());
            EditCommand = new RelayCommand(_ => EditClothing(), _ => SelectedClothing != null);
            DeleteCommand = new RelayCommand(_ => DeleteClothing(), _ => SelectedClothing != null);
            ViewDetailsCommand = new RelayCommand(_ => ViewDetails(), _ => SelectedClothing != null);
            RefreshCommand = new RelayCommand(_ => LoadClothing());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            MarkAsUsedCommand = new RelayCommand(_ => MarkAsUsed(), _ => SelectedClothing != null && SelectedClothing.IsAvailable);
            MarkAsAvailableCommand = new RelayCommand(_ => MarkAsAvailable(), _ => SelectedClothing != null && !SelectedClothing.IsAvailable);

            // Setup collection view for filtering
            ClothingView = CollectionViewSource.GetDefaultView(Clothing);

            // Auto-load data
            LoadClothing();
        }
        #endregion

        #region Methods
        private async void LoadClothing()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading clothing items...";

                var clothing = _serviceClient.Execute(() => _serviceClient.ClothingService.GetAll(), "Load clothing");

                Clothing.Clear();
                foreach (var item in clothing ?? Enumerable.Empty<ClothingDto>())
                {
                    Clothing.Add(item);
                }

                StatusMessage = $"Loaded {Clothing.Count} clothing items successfully.";
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error loading clothing: {ex.Message}";
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

        private void AddNewClothing()
        {
            // TODO: Navigate to Add/Edit view with new clothing
            StatusMessage = "Add new clothing functionality - to be implemented";
        }

        private void EditClothing()
        {
            if (SelectedClothing == null) return;

            // TODO: Navigate to Add/Edit view with selected clothing
            StatusMessage = $"Edit clothing: {SelectedClothing.Name} - to be implemented";
        }

        private async void DeleteClothing()
        {
            if (SelectedClothing == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Deleting clothing: {SelectedClothing.Name}...";

                var success = _serviceClient.Execute(() =>
                    _serviceClient.ClothingService.Delete(SelectedClothing.Id),
                    "Delete clothing");

                if (success)
                {
                    Clothing.Remove(SelectedClothing);
                    SelectedClothing = null;
                    StatusMessage = "Clothing deleted successfully.";
                }
                else
                {
                    StatusMessage = "Failed to delete clothing.";
                }
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error deleting clothing: {ex.Message}";
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
            if (SelectedClothing == null) return;

            // TODO: Navigate to details view
            StatusMessage = $"View details for: {SelectedClothing.Name} - to be implemented";
        }

        private async void MarkAsUsed()
        {
            if (SelectedClothing == null || !SelectedClothing.IsAvailable) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Marking {SelectedClothing.Name} as used...";

                var success = _serviceClient.Execute(() =>
                    _serviceClient.ClothingService.MarkAsUsed(SelectedClothing.Id),
                    "Mark as used");

                if (success)
                {
                    SelectedClothing.IsAvailable = false;
                    SelectedClothing.LastCleaned = DateTime.Now.AddDays(-1);
                    StatusMessage = $"{SelectedClothing.Name} marked as used successfully.";

                    // Refresh the view to show updated data
                    ClothingView.Refresh();
                }
                else
                {
                    StatusMessage = "Failed to mark clothing as used.";
                }
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error marking clothing as used: {ex.Message}";
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

        private async void MarkAsAvailable()
        {
            if (SelectedClothing == null || SelectedClothing.IsAvailable) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Marking {SelectedClothing.Name} as available...";

                var success = _serviceClient.Execute(() =>
                    _serviceClient.ClothingService.MarkAsAvailable(SelectedClothing.Id),
                    "Mark as available");

                if (success)
                {
                    SelectedClothing.IsAvailable = true;
                    SelectedClothing.LastCleaned = DateTime.Now;
                    StatusMessage = $"{SelectedClothing.Name} marked as available successfully.";

                    // Refresh the view to show updated data
                    ClothingView.Refresh();
                }
                else
                {
                    StatusMessage = "Failed to mark clothing as available.";
                }
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error marking clothing as available: {ex.Message}";
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

        private void ApplyFilters()
        {
            if (ClothingView == null) return;

            ClothingView.Filter = item =>
            {
                var clothing = item as ClothingDto;
                if (clothing == null) return false;

                // Text filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLower();
                    if (!clothing.Name.ToLower().Contains(searchLower) &&
                        !clothing.Brand.ToLower().Contains(searchLower) &&
                        !clothing.Color.ToLower().Contains(searchLower) &&
                        !clothing.Condition.ToLower().Contains(searchLower))
                    {
                        return false;
                    }
                }

                // Type filter
                if (FilterType.HasValue && clothing.Type != FilterType.Value)
                {
                    return false;
                }

                // Availability filter
                if (FilterIsAvailable.HasValue && clothing.IsAvailable != FilterIsAvailable.Value)
                {
                    return false;
                }

                // Waterproof filter
                if (FilterIsWaterproof.HasValue && clothing.IsWaterproof != FilterIsWaterproof.Value)
                {
                    return false;
                }

                return true;
            };

            StatusMessage = $"Showing {ClothingView.Cast<object>().Count()} of {Clothing.Count} clothing items";
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            FilterType = null;
            FilterIsAvailable = null;
            FilterIsWaterproof = null;
            StatusMessage = $"Showing all {Clothing.Count} clothing items";
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