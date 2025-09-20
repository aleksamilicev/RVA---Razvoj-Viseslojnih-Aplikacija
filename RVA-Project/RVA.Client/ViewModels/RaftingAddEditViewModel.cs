using RVA.Client.Commands;
using RVA.Client.Services;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;

namespace RVA.Client.ViewModels
{
    public class RaftingAddEditViewModel : BaseViewModel
    {
        #region Private Fields
        private readonly WcfServiceClient _serviceClient;
        private readonly RaftingDto _originalRafting;
        private bool _isEditMode;
        private bool _isLoading;
        private string _statusMessage;

        // Form fields
        private string _name;
        private string _description;
        private DateTime _startTime = DateTime.Now;
        private DateTime _endTime = DateTime.Now.AddHours(3);
        private double _distance = 10.0;
        private Intensity _currentIntensity = Intensity.Medium;
        private double _currentSpeedKmh = 15.0;
        private int _capacity = 8;
        private RaftingState _currentState = RaftingState.Planned;
        private int _guideId = 1;
        private decimal _pricePerPerson = 50.0m;
        private string _weatherConditions = "Sunny";
        private int _maxParticipants = 8;
        private int _startLocationId = 1;
        private int _endLocationId = 2;

        // Validation
        private string _nameError;
        private string _descriptionError;
        private string _timeError;
        private string _distanceError;
        private string _capacityError;
        private string _priceError;
        private string _locationError;
        #endregion

        #region Events
        public event EventHandler<RaftingDto> RaftingSaved;
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

        public string WindowTitle => IsEditMode ? "Edit Rafting" : "Add New Rafting";

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

        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                SetProperty(ref _startTime, value);
                UpdateDuration();
                ValidateTime();
                UpdateSaveCommand();
            }
        }

        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                SetProperty(ref _endTime, value);
                UpdateDuration();
                ValidateTime();
                UpdateSaveCommand();
            }
        }

        public TimeSpan Duration => EndTime - StartTime;

        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km")]
        public double Distance
        {
            get => _distance;
            set
            {
                SetProperty(ref _distance, value);
                ValidateDistance();
                UpdateSaveCommand();
            }
        }

        public Intensity CurrentIntensity
        {
            get => _currentIntensity;
            set => SetProperty(ref _currentIntensity, value);
        }

        [Range(1, 50, ErrorMessage = "Current speed must be between 1 and 50 km/h")]
        public double CurrentSpeedKmh
        {
            get => _currentSpeedKmh;
            set => SetProperty(ref _currentSpeedKmh, value);
        }

        [Range(1, 50, ErrorMessage = "Capacity must be between 1 and 50")]
        public int Capacity
        {
            get => _capacity;
            set
            {
                SetProperty(ref _capacity, value);
                ValidateCapacity();
                UpdateSaveCommand();
            }
        }

        public RaftingState CurrentState
        {
            get => _currentState;
            set => SetProperty(ref _currentState, value);
        }

        [Range(1, int.MaxValue, ErrorMessage = "Guide ID is required")]
        public int GuideId
        {
            get => _guideId;
            set => SetProperty(ref _guideId, value);
        }

        [Range(0, 10000, ErrorMessage = "Price must be between 0 and 10000")]
        public decimal PricePerPerson
        {
            get => _pricePerPerson;
            set
            {
                SetProperty(ref _pricePerPerson, value);
                ValidatePrice();
                UpdateSaveCommand();
            }
        }

        public string WeatherConditions
        {
            get => _weatherConditions;
            set => SetProperty(ref _weatherConditions, value);
        }

        [Range(1, 50, ErrorMessage = "Max participants must be between 1 and 50")]
        public int MaxParticipants
        {
            get => _maxParticipants;
            set => SetProperty(ref _maxParticipants, value);
        }

        [Range(1, int.MaxValue, ErrorMessage = "Start location is required")]
        public int StartLocationId
        {
            get => _startLocationId;
            set
            {
                SetProperty(ref _startLocationId, value);
                ValidateLocation();
                UpdateSaveCommand();
            }
        }

        [Range(1, int.MaxValue, ErrorMessage = "End location is required")]
        public int EndLocationId
        {
            get => _endLocationId;
            set
            {
                SetProperty(ref _endLocationId, value);
                ValidateLocation();
                UpdateSaveCommand();
            }
        }

        // Validation Error Properties
        public string NameError
        {
            get => _nameError;
            set => SetProperty(ref _nameError, value);
        }

        public string DescriptionError
        {
            get => _descriptionError;
            set => SetProperty(ref _descriptionError, value);
        }

        public string TimeError
        {
            get => _timeError;
            set => SetProperty(ref _timeError, value);
        }

        public string DistanceError
        {
            get => _distanceError;
            set => SetProperty(ref _distanceError, value);
        }

        public string CapacityError
        {
            get => _capacityError;
            set => SetProperty(ref _capacityError, value);
        }

        public string PriceError
        {
            get => _priceError;
            set => SetProperty(ref _priceError, value);
        }

        public string LocationError
        {
            get => _locationError;
            set => SetProperty(ref _locationError, value);
        }

        public bool IsFormValid => string.IsNullOrEmpty(NameError) &&
                                  string.IsNullOrEmpty(TimeError) &&
                                  string.IsNullOrEmpty(DistanceError) &&
                                  string.IsNullOrEmpty(CapacityError) &&
                                  string.IsNullOrEmpty(PriceError) &&
                                  string.IsNullOrEmpty(LocationError) &&
                                  !string.IsNullOrWhiteSpace(Name);

        // Enums for ComboBox binding
        public Array IntensityValues => Enum.GetValues(typeof(Intensity));
        public Array RaftingStateValues => Enum.GetValues(typeof(RaftingState));
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }
        #endregion

        #region Constructor
        public RaftingAddEditViewModel(WcfServiceClient serviceClient, RaftingDto raftingToEdit = null)
        {
            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
            _originalRafting = raftingToEdit;
            IsEditMode = raftingToEdit != null;

            // Initialize commands
            SaveCommand = new RelayCommand(_ => SaveRafting(), _ => IsFormValid && !IsLoading);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            ResetCommand = new RelayCommand(_ => ResetForm());

            // Load data if editing
            if (IsEditMode)
            {
                LoadRaftingData();
            }

            StatusMessage = IsEditMode ? "Editing existing rafting" : "Creating new rafting";
        }
        #endregion

        #region Methods
        private void LoadRaftingData()
        {
            if (_originalRafting == null) return;

            Name = _originalRafting.Name;
            Description = _originalRafting.Description;
            StartTime = _originalRafting.StartTime;
            EndTime = _originalRafting.EndTime;
            Distance = _originalRafting.Distance;
            CurrentIntensity = _originalRafting.CurrentIntensity;
            CurrentSpeedKmh = _originalRafting.CurrentSpeedKmh;
            Capacity = _originalRafting.Capacity;
            CurrentState = _originalRafting.CurrentState;
            GuideId = _originalRafting.GuideId;
            PricePerPerson = _originalRafting.PricePerPerson;
            WeatherConditions = _originalRafting.WeatherConditions;
            MaxParticipants = _originalRafting.MaxParticipants;
            StartLocationId = _originalRafting.StartLocationId;
            EndLocationId = _originalRafting.EndLocationId;
        }

        // U RaftingAddEditViewModel, izmeni SaveRafting metodu:
        private async void SaveRafting()
        {
            try
            {
                IsLoading = true;
                StatusMessage = IsEditMode ? "Updating rafting..." : "Creating rafting...";

                var rafting = CreateRaftingDto();

                if (IsEditMode)
                {
                    rafting.Id = _originalRafting.Id;
                    rafting.CreatedDate = _originalRafting.CreatedDate;
                    rafting.ModifiedDate = DateTime.Now;
                }
                else
                {
                    rafting.CreatedDate = DateTime.Now;
                    rafting.ModifiedDate = DateTime.Now;
                }

                // Samo pozovi event sa DTO - ne radi direktno sa servisom
                StatusMessage = IsEditMode ? "Rafting updated successfully!" : "Rafting created successfully!";
                RaftingSaved?.Invoke(this, rafting);
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

        private RaftingDto CreateRaftingDto()
        {
            return new RaftingDto
            {
                Name = Name?.Trim(),
                Description = Description?.Trim(),
                StartTime = StartTime,
                EndTime = EndTime,
                Duration = Duration,
                Distance = Distance,
                CurrentIntensity = CurrentIntensity,
                CurrentSpeedKmh = CurrentSpeedKmh,
                Capacity = Capacity,
                CurrentState = CurrentState,
                GuideId = GuideId,
                PricePerPerson = PricePerPerson,
                WeatherConditions = WeatherConditions?.Trim(),
                MaxParticipants = MaxParticipants,
                ParticipantCount = 0, // Default for new rafting
                StartLocationId = StartLocationId,
                EndLocationId = EndLocationId,
                UsedClothingIds = new List<int>(),
                UsedEquipmentIds = new List<int>()
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
                LoadRaftingData();
            }
            else
            {
                // Reset to default values
                Name = string.Empty;
                Description = string.Empty;
                StartTime = DateTime.Now;
                EndTime = DateTime.Now.AddHours(3);
                Distance = 10.0;
                CurrentIntensity = Intensity.Medium;
                CurrentSpeedKmh = 15.0;
                Capacity = 8;
                CurrentState = RaftingState.Planned;
                GuideId = 1;
                PricePerPerson = 50.0m;
                WeatherConditions = "Sunny";
                MaxParticipants = 8;
                StartLocationId = 1;
                EndLocationId = 2;
            }

            StatusMessage = "Form reset.";
        }

        private void UpdateDuration()
        {
            OnPropertyChanged(nameof(Duration));
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

        private void ValidateDescription()
        {
            if (!string.IsNullOrEmpty(Description) && Description.Length > 500)
                DescriptionError = "Description cannot exceed 500 characters";
            else
                DescriptionError = string.Empty;
        }

        private void ValidateTime()
        {
            if (StartTime >= EndTime)
                TimeError = "Start time must be before end time";
            else if (Duration.TotalMinutes < 30)
                TimeError = "Duration must be at least 30 minutes";
            else if (Duration.TotalHours > 12)
                TimeError = "Duration cannot exceed 12 hours";
            else
                TimeError = string.Empty;
        }

        private void ValidateDistance()
        {
            if (Distance <= 0)
                DistanceError = "Distance must be greater than 0";
            else if (Distance > 1000)
                DistanceError = "Distance cannot exceed 1000 km";
            else
                DistanceError = string.Empty;
        }

        private void ValidateCapacity()
        {
            if (Capacity <= 0)
                CapacityError = "Capacity must be greater than 0";
            else if (Capacity > 50)
                CapacityError = "Capacity cannot exceed 50";
            else
                CapacityError = string.Empty;
        }

        private void ValidatePrice()
        {
            if (PricePerPerson < 0)
                PriceError = "Price cannot be negative";
            else if (PricePerPerson > 10000)
                PriceError = "Price cannot exceed 10,000";
            else
                PriceError = string.Empty;
        }

        private void ValidateLocation()
        {
            if (StartLocationId == EndLocationId)
                LocationError = "Start and end locations must be different";
            else
                LocationError = string.Empty;
        }
        #endregion
    }
}