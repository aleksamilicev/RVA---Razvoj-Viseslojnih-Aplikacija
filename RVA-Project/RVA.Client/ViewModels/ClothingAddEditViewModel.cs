using RVA.Client.Commands;
using RVA.Client.Services;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace RVA.Client.ViewModels
{
    public class ClothingAddEditViewModel : BaseViewModel
    {
        #region Private Fields
        private readonly WcfServiceClient _serviceClient;
        private readonly ClothingDto _originalClothing;
        private bool _isEditMode;
        private bool _isLoading;
        private string _statusMessage;

        // Form fields
        private string _name;
        private ClothingType _type = ClothingType.Wetsuit;
        private Material _material = Material.Neoprene;
        private bool _isWaterproof;
        private int _size = 42;
        private string _brand;
        private string _color;
        private bool _isAvailable = true;
        private DateTime _lastCleaned = DateTime.Now;
        private string _condition = "Good";

        // Validation
        private string _nameError;
        private string _sizeError;
        private string _brandError;
        private string _colorError;
        private string _conditionError;
        #endregion

        #region Events
        public event EventHandler<ClothingDto> ClothingSaved;
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

        public string WindowTitle => IsEditMode ? "Edit Clothing" : "Add New Clothing";

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

        public ClothingType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public Material Material
        {
            get => _material;
            set => SetProperty(ref _material, value);
        }

        public bool IsWaterproof
        {
            get => _isWaterproof;
            set => SetProperty(ref _isWaterproof, value);
        }

        [Range(1, 60, ErrorMessage = "Size must be between 1 and 60")]
        public int Size
        {
            get => _size;
            set
            {
                SetProperty(ref _size, value);
                ValidateSize();
                UpdateSaveCommand();
            }
        }

        [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
        public string Brand
        {
            get => _brand;
            set
            {
                SetProperty(ref _brand, value);
                ValidateBrand();
            }
        }

        [StringLength(30, ErrorMessage = "Color cannot exceed 30 characters")]
        public string Color
        {
            get => _color;
            set
            {
                SetProperty(ref _color, value);
                ValidateColor();
            }
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set => SetProperty(ref _isAvailable, value);
        }

        public DateTime LastCleaned
        {
            get => _lastCleaned;
            set => SetProperty(ref _lastCleaned, value);
        }

        [Required(ErrorMessage = "Condition is required")]
        [StringLength(50, ErrorMessage = "Condition cannot exceed 50 characters")]
        public string Condition
        {
            get => _condition;
            set
            {
                SetProperty(ref _condition, value);
                ValidateCondition();
                UpdateSaveCommand();
            }
        }

        // Validation Error Properties
        public string NameError
        {
            get => _nameError;
            set => SetProperty(ref _nameError, value);
        }

        public string SizeError
        {
            get => _sizeError;
            set => SetProperty(ref _sizeError, value);
        }

        public string BrandError
        {
            get => _brandError;
            set => SetProperty(ref _brandError, value);
        }

        public string ColorError
        {
            get => _colorError;
            set => SetProperty(ref _colorError, value);
        }

        public string ConditionError
        {
            get => _conditionError;
            set => SetProperty(ref _conditionError, value);
        }

        public bool IsFormValid => string.IsNullOrEmpty(NameError) &&
                                  string.IsNullOrEmpty(SizeError) &&
                                  string.IsNullOrEmpty(BrandError) &&
                                  string.IsNullOrEmpty(ColorError) &&
                                  string.IsNullOrEmpty(ConditionError) &&
                                  !string.IsNullOrWhiteSpace(Name) &&
                                  !string.IsNullOrWhiteSpace(Condition);

        // Enums for ComboBox binding
        public Array ClothingTypes => Enum.GetValues(typeof(ClothingType));
        public Array MaterialTypes => Enum.GetValues(typeof(Material));

        // Predefined condition options
        public string[] ConditionOptions => new[] { "Excellent", "Good", "Fair", "Poor", "Needs Repair" };

        // Common color options
        public string[] ColorOptions => new[] { "Black", "Blue", "Red", "Green", "Yellow", "Orange", "Purple", "Pink", "White", "Gray", "Brown" };
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }
        #endregion

        #region Constructor
        public ClothingAddEditViewModel(WcfServiceClient serviceClient, ClothingDto clothingToEdit = null)
        {
            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
            _originalClothing = clothingToEdit;
            IsEditMode = clothingToEdit != null;

            // Initialize commands
            SaveCommand = new RelayCommand(_ => SaveClothing(), _ => IsFormValid && !IsLoading);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            ResetCommand = new RelayCommand(_ => ResetForm());

            // Load data if editing
            if (IsEditMode)
            {
                LoadClothingData();
            }
            else
            {
                // Set default values for new clothing
                _name = string.Empty;
                _brand = string.Empty;
                _color = "Black";
            }

            StatusMessage = IsEditMode ? "Editing existing clothing item" : "Creating new clothing item";
        }
        #endregion

        #region Methods
        private void LoadClothingData()
        {
            if (_originalClothing == null) return;

            Name = _originalClothing.Name;
            Type = _originalClothing.Type;
            Material = _originalClothing.Material;
            IsWaterproof = _originalClothing.IsWaterproof;
            Size = _originalClothing.Size;
            Brand = _originalClothing.Brand;
            Color = _originalClothing.Color;
            IsAvailable = _originalClothing.IsAvailable;
            LastCleaned = _originalClothing.LastCleaned;
            Condition = _originalClothing.Condition;
        }

        private async void SaveClothing()
        {
            try
            {
                IsLoading = true;
                StatusMessage = IsEditMode ? "Updating clothing..." : "Creating clothing...";

                var clothing = CreateClothingDto();

                if (IsEditMode)
                {
                    clothing.Id = _originalClothing.Id;

                    var success = _serviceClient.Execute(() =>
                        _serviceClient.ClothingService.Update(clothing),
                        "Update clothing");

                    if (success)
                    {
                        StatusMessage = "Clothing updated successfully!";
                        ClothingSaved?.Invoke(this, clothing);
                    }
                    else
                    {
                        StatusMessage = "Failed to update clothing.";
                    }
                }
                else
                {
                    var newId = _serviceClient.Execute(() =>
                        _serviceClient.ClothingService.Create(clothing),
                        "Create clothing");

                    if (newId > 0)
                    {
                        clothing.Id = newId;
                        StatusMessage = "Clothing created successfully!";
                        ClothingSaved?.Invoke(this, clothing);
                    }
                    else
                    {
                        StatusMessage = "Failed to create clothing.";
                    }
                }
            }
            catch (ServiceException ex)
            {
                StatusMessage = $"Error saving clothing: {ex.Message}";
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

        private ClothingDto CreateClothingDto()
        {
            return new ClothingDto
            {
                Name = Name?.Trim(),
                Type = Type,
                Material = Material,
                IsWaterproof = IsWaterproof,
                Size = Size,
                Brand = Brand?.Trim(),
                Color = Color?.Trim(),
                IsAvailable = IsAvailable,
                LastCleaned = LastCleaned,
                Condition = Condition?.Trim()
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
                LoadClothingData();
            }
            else
            {
                // Reset to default values
                Name = string.Empty;
                Type = ClothingType.Wetsuit;
                Material = Material.Neoprene;
                IsWaterproof = false;
                Size = 42;
                Brand = string.Empty;
                Color = "Black";
                IsAvailable = true;
                LastCleaned = DateTime.Now;
                Condition = "Good";
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

        private void ValidateSize()
        {
            if (Size < 1 || Size > 60)
                SizeError = "Size must be between 1 and 60";
            else
                SizeError = string.Empty;
        }

        private void ValidateBrand()
        {
            if (!string.IsNullOrEmpty(Brand) && Brand.Length > 50)
                BrandError = "Brand cannot exceed 50 characters";
            else
                BrandError = string.Empty;
        }

        private void ValidateColor()
        {
            if (!string.IsNullOrEmpty(Color) && Color.Length > 30)
                ColorError = "Color cannot exceed 30 characters";
            else
                ColorError = string.Empty;
        }

        private void ValidateCondition()
        {
            if (string.IsNullOrWhiteSpace(Condition))
                ConditionError = "Condition is required";
            else if (Condition.Length > 50)
                ConditionError = "Condition cannot exceed 50 characters";
            else
                ConditionError = string.Empty;
        }
        #endregion
    }
}