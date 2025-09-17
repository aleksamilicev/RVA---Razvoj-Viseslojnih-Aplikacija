using RVA.Server.Factories;
using RVA.Server.Logging;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace RVA.Server.Services
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class ClothingService : IClothingService
    {
        /*
        ClothingService koristi generički repozitorijum (IRepository<ClothingDto>) jer za odeću nema puno specijalizovanih metoda
        većinom su to standardni upiti koje možeš rešiti preko Find(predicate) metode iz IRepository<T>. Dakle, dovoljan je generički pristup.
         */
        private readonly Shared.Interfaces.IRepository<ClothingDto> _repository;
        private readonly ILogger _logger;

        public ClothingService()
        {
            // Inicijalizuj dependencies kroz factory
            _repository = RepositoryFactory.CreateClothingRepository();
            _logger = new ServerLogger();
        }

        public ClothingService(Shared.Interfaces.IRepository<ClothingDto> repository, ILogger logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<ClothingDto> GetAll()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.Info("Getting all clothing items");
                return _repository.GetAll();
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetAll clothing", ex);
                throw new FaultException("Failed to retrieve clothing");
            }
            finally
            {
                _logger.Debug($"GetAll clothing completed in {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Stop();
            }
        }

        public ClothingDto GetById(int id)
        {
            try
            {
                _logger.Info($"Getting clothing by ID: {id}");
                var result = _repository.GetById(id);
                if (result == null)
                {
                    _logger.Warn($"Clothing with ID {id} not found");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting clothing by ID {id}", ex);
                throw new FaultException("Failed to retrieve clothing");
            }
        }

        public int Create(ClothingDto clothing)
        {
            try
            {
                _logger.Info("Creating new clothing item");

                if (clothing == null)
                    throw new ArgumentNullException(nameof(clothing));

                var validationResult = Validate(clothing);
                if (!validationResult.IsValid)
                {
                    _logger.Warn($"Clothing validation failed: {validationResult.GetErrorsAsString()}");
                    throw new FaultException($"Validation failed: {validationResult.GetErrorsAsString()}");
                }

                _repository.Add(clothing);
                _repository.SaveChanges();

                _logger.Info($"Clothing created with ID: {clothing.Id}");
                return clothing.Id;
            }
            catch (Exception ex)
            {
                _logger.Error("Error creating clothing", ex);
                throw new FaultException("Failed to create clothing");
            }
        }

        public bool Update(ClothingDto clothing)
        {
            try
            {
                _logger.Info($"Updating clothing ID: {clothing?.Id}");

                if (clothing == null)
                    throw new ArgumentNullException(nameof(clothing));

                if (!_repository.Exists(clothing.Id))
                {
                    _logger.Warn($"Clothing with ID {clothing.Id} not found for update");
                    return false;
                }

                var validationResult = Validate(clothing);
                if (!validationResult.IsValid)
                {
                    _logger.Warn($"Clothing update validation failed: {validationResult.GetErrorsAsString()}");
                    throw new FaultException($"Validation failed: {validationResult.GetErrorsAsString()}");
                }

                _repository.Update(clothing);
                _repository.SaveChanges();

                _logger.Info($"Clothing ID {clothing.Id} updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating clothing ID {clothing?.Id}", ex);
                throw new FaultException("Failed to update clothing");
            }
        }

        public bool Delete(int id)
        {
            try
            {
                _logger.Info($"Deleting clothing ID: {id}");

                if (!_repository.Exists(id))
                {
                    _logger.Warn($"Clothing with ID {id} not found for deletion");
                    return false;
                }

                _repository.Delete(id);
                _repository.SaveChanges();

                _logger.Info($"Clothing ID {id} deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting clothing ID {id}", ex);
                throw new FaultException("Failed to delete clothing");
            }
        }

        public IEnumerable<ClothingDto> GetAvailable()
        {
            try
            {
                _logger.Info("Getting available clothing");
                return _repository.Find(c => c.IsAvailable);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting available clothing", ex);
                throw new FaultException("Failed to retrieve available clothing");
            }
        }

        public IEnumerable<ClothingDto> GetByType(ClothingType type)
        {
            try
            {
                _logger.Info($"Getting clothing by type: {type}");
                return _repository.Find(c => c.Type == type);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting clothing by type {type}", ex);
                throw new FaultException("Failed to retrieve clothing by type");
            }
        }

        public IEnumerable<ClothingDto> GetByMaterial(Material material)
        {
            try
            {
                _logger.Info($"Getting clothing by material: {material}");
                return _repository.Find(c => c.Material == material);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting clothing by material {material}", ex);
                throw new FaultException("Failed to retrieve clothing by material");
            }
        }

        public IEnumerable<ClothingDto> GetWaterproof()
        {
            try
            {
                _logger.Info("Getting waterproof clothing");
                return _repository.Find(c => c.IsWaterproof);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting waterproof clothing", ex);
                throw new FaultException("Failed to retrieve waterproof clothing");
            }
        }

        public IEnumerable<ClothingDto> GetBySize(int size)
        {
            try
            {
                _logger.Info($"Getting clothing by size: {size}");
                return _repository.Find(c => c.Size == size);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting clothing by size {size}", ex);
                throw new FaultException("Failed to retrieve clothing by size");
            }
        }

        public bool MarkAsUsed(int id)
        {
            try
            {
                _logger.Info($"Marking clothing {id} as used");

                var clothing = _repository.GetById(id);
                if (clothing == null)
                {
                    _logger.Warn($"Clothing with ID {id} not found");
                    return false;
                }

                clothing.IsAvailable = false;
                clothing.LastCleaned = DateTime.Now.AddDays(-1); // Označava da treba pranje

                _repository.Update(clothing);
                _repository.SaveChanges();

                _logger.Info($"Clothing {id} marked as used");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error marking clothing {id} as used", ex);
                throw new FaultException("Failed to mark clothing as used");
            }
        }

        public bool MarkAsAvailable(int id)
        {
            try
            {
                _logger.Info($"Marking clothing {id} as available");

                var clothing = _repository.GetById(id);
                if (clothing == null)
                {
                    _logger.Warn($"Clothing with ID {id} not found");
                    return false;
                }

                clothing.IsAvailable = true;
                clothing.LastCleaned = DateTime.Now;

                _repository.Update(clothing);
                _repository.SaveChanges();

                _logger.Info($"Clothing {id} marked as available");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error marking clothing {id} as available", ex);
                throw new FaultException("Failed to mark clothing as available");
            }
        }

        public ValidationResult Validate(ClothingDto clothing)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                if (clothing == null)
                {
                    result.AddError("Clothing cannot be null");
                    result.IsValid = false;
                    return result;
                }

                if (string.IsNullOrWhiteSpace(clothing.Name))
                    result.AddError("Name is required");

                if (clothing.Size <= 0 || clothing.Size > 60)
                    result.AddError("Size must be between 1 and 60");

                if (string.IsNullOrWhiteSpace(clothing.Brand))
                    result.AddWarning("Brand is not specified");

                if (string.IsNullOrWhiteSpace(clothing.Color))
                    result.AddWarning("Color is not specified");

                if (string.IsNullOrWhiteSpace(clothing.Condition))
                    result.AddWarning("Condition is not specified");

                // Validacija enum vrednosti
                if (!Enum.IsDefined(typeof(ClothingType), clothing.Type))
                    result.AddError("Invalid clothing type");

                if (!Enum.IsDefined(typeof(Material), clothing.Material))
                    result.AddError("Invalid material type");

                result.IsValid = !result.Errors.Any();
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Error validating clothing", ex);
                result.AddError("Validation error occurred");
                result.IsValid = false;
                return result;
            }
        }
    }
}