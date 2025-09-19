using RVA.Client.Commands;
using RVA.Client.Services;
using RVA.Shared.DTOs;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Windows.Input;

namespace RVA.Client.ViewModels
{
    public class LocationAddEditViewModel : BaseViewModel
    {
        #region Private Fields
        private readonly WcfServiceClient _serviceClient;
        private readonly LocationDto _originalLocation;
        private bool _isEditMode;
        private bool _isLoading;
        private string _statusMessage;

        // Form fields
        private string _name;
        private string _river;
        private double _latitude;
        private double _longitude;
        private string _description;
        private bool _hasParking;
        private bool _hasFacilities;

        // Validation
        private string _nameError;
        private string _riverError;
        private string _latitudeError;
        private string _longitudeError;
        private string _descriptionError;
        #endregion

        #region Events
        public event EventHandler<LocationDto> LocationSaved;
        #endregion

        #region Properties
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
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

        public string WindowTitle => IsEditMode ? "Edit Location" : "Add New Location";

        // Form Fields
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                ValidateName();
                UpdateSaveCommand();
            }
        }

        [Required(ErrorMessage = "River name is required")]
        [StringLength(100, ErrorMessage = "River name cannot exceed 100 characters")]
        public string River
        {
            get => _river;
            set
            {
                SetProperty(ref _river, value);
                ValidateRiver();
                UpdateSaveCommand();
            }
        }

        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude
        {
            get => _latitude;
            set
            {
                SetProperty(ref _latitude, value);
                ValidateLatitude();
                UpdateSaveCommand();
            }
        }

        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude
        {
            get => _longitude;
            set
            {
                SetProperty(ref _longitude, value);
                ValidateLongitude();
                UpdateSaveCommand();
            }
        }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description
        {
            get => _description;
            set
            {
                SetProperty(ref _description, value);
                ValidateDescription();
            }
        }

        public bool HasParking
        {
            get => _hasParking;
            set => SetProperty(ref _hasParking, value);
        }

        public bool HasFacilities
        {
            get => _hasFacilities;
            set => SetProperty(ref _hasFacilities, value);
        }

        // Validation Error Properties
        public string NameError
        {
            get => _nameError;
            set => SetProperty(ref _nameError, value);
        }

        public string RiverError
        {
            get => _riverError;
            set => SetProperty(ref _riverError, value);
        }

        public string LatitudeError
        {
            get => _latitudeError;
            set => SetProperty(ref _latitudeError, value);
        }

        public string LongitudeError
        {
            get => _longitudeError;
            set => SetProperty(ref _longitudeError, value);
        }

        public string DescriptionError
        {
            get => _descriptionError;
            set => SetProperty(ref _descriptionError, value);
        }

        public bool IsFormValid => string.IsNullOrEmpty(NameError) &&
                                  string.IsNullOrEmpty(RiverError) &&
                                  string.IsNullOrEmpty(LatitudeError) &&
                                  string.IsNullOrEmpty(LongitudeError) &&
                                  string.IsNullOrEmpty(DescriptionError) &&
                                  !string.IsNullOrWhiteSpace(Name) &&
                                  !string.IsNullOrWhiteSpace(River);
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }
        #endregion

        #region Constructor
        public LocationAddEditViewModel(WcfServiceClient serviceClient, LocationDto locationToEdit = null)
        {
            // Postavi kulturu na Invariant za ceo view model
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
            _originalLocation = locationToEdit;
            IsEditMode = locationToEdit != null;

            // Initialize commands
            SaveCommand = new RelayCommand(_ => SaveLocation(), _ => IsFormValid && !IsLoading);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            ResetCommand = new RelayCommand(_ => ResetForm());

            // Load data if editing
            if (IsEditMode)
            {
                LoadLocationData();
            }
            else
            {
                // Set default values for new location
                _latitude = 44.0165;  // Default to Serbia coordinates
                _longitude = 21.0059;
                _hasParking = false;
                _hasFacilities = false;
            }

            StatusMessage = IsEditMode ? "Editing existing location" : "Creating new location";
        }
        #endregion

        #region Methods
        private void LoadLocationData()
        {
            if (_originalLocation == null) return;

            Name = _originalLocation.Name;
            River = _originalLocation.River;
            Latitude = _originalLocation.Latitude;
            Longitude = _originalLocation.Longitude;
            Description = _originalLocation.Description;
            HasParking = _originalLocation.HasParking;
            HasFacilities = _originalLocation.HasFacilities;
        }

        private async void SaveLocation()
        {
            try
            {
                IsLoading = true;
                StatusMessage = IsEditMode ? "Updating location..." : "Creating location...";

                var location = CreateLocationDto();

                if (IsEditMode)
                {
                    location.Id = _originalLocation.Id;
                    location.CreatedDate = _originalLocation.CreatedDate;

                    var success = _serviceClient.Execute(() =>
                        _serviceClient.LocationService.Update(location),
                        "Update location");

                    if (success)
                    {
                        StatusMessage = "Location updated successfully!";
                        LocationSaved?.Invoke(this, location);
                    }
                    else
                    {
                        StatusMessage = "Failed to update location.";
                    }
                }
                else
                {
                    location.CreatedDate = DateTime.Now;

                    var newId = _serviceClient.Execute(() =>
                        _serviceClient.LocationService.Create(location),
                        "Create location");

                    if (newId > 0)
                    {
                        location.Id = newId;
                        StatusMessage = "Location created successfully!";
                        LocationSaved?.Invoke(this, location);
                    }
                    else
                    {
                        StatusMessage = "Failed to create location.";
                    }
                }
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error saving location: {ex.Message}";
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

        private LocationDto CreateLocationDto()
        {
            return new LocationDto
            {
                Name = Name?.Trim(),
                River = River?.Trim(),
                Latitude = Latitude,
                Longitude = Longitude,
                Description = Description?.Trim(),
                HasParking = HasParking,
                HasFacilities = HasFacilities
            };
        }

        private void CancelEdit()
        {
            StatusMessage = "Operation cancelled.";
            // Close dialog logic will be handled by the view
        }

        private void ResetForm()
        {
            if (IsEditMode)
            {
                LoadLocationData();
            }
            else
            {
                // Reset to default values
                Name = string.Empty;
                River = string.Empty;
                Latitude = 44.0165;
                Longitude = 21.0059;
                Description = string.Empty;
                HasParking = false;
                HasFacilities = false;
            }

            StatusMessage = "Form reset.";
        }

        private void UpdateSaveCommand()
        {
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        // Validation Methods
        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(Name))
                NameError = "Name is required";
            else if (Name.Length > 100)
                NameError = "Name cannot exceed 100 characters";
            else
                NameError = string.Empty;
        }

        private void ValidateRiver()
        {
            if (string.IsNullOrWhiteSpace(River))
                RiverError = "River name is required";
            else if (River.Length > 100)
                RiverError = "River name cannot exceed 100 characters";
            else
                RiverError = string.Empty;
        }

        private void ValidateLatitude()
        {
            if (Latitude < -90.0 || Latitude > 90.0)
                LatitudeError = "Latitude must be between -90 and 90";
            else
                LatitudeError = string.Empty;
        }

        private void ValidateLongitude()
        {
            if (Longitude < -180.0 || Longitude > 180.0)
                LongitudeError = "Longitude must be between -180 and 180";
            else
                LongitudeError = string.Empty;
        }

        private void ValidateDescription()
        {
            if (!string.IsNullOrEmpty(Description) && Description.Length > 500)
                DescriptionError = "Description cannot exceed 500 characters";
            else
                DescriptionError = string.Empty;
        }
        #endregion
    }
}