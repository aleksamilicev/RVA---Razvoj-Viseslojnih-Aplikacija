using log4net.Repository.Hierarchy;
using RVA.Server.Data;
using RVA.Server.Interfaces;
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
    public class RaftingService : IRaftingService
    {
        private readonly RaftingRepository _repository;
        private readonly LocationRepository _locationRepository;
        private readonly ILogger _logger;

        // WCF zahteva parameterless konstruktor
        public RaftingService()
        {
            // Inicijalizuj dependencies kroz factory
            _repository = RepositoryFactory.CreateRaftingRepository();
            _locationRepository = RepositoryFactory.CreateLocationRepository();
            _logger = new ServerLogger(); // ili ako RepositoryFactory već prosleđuje logger, možeš koristiti isti
        }



        // Konstruktor za testiranje/DI (opciono)
        public RaftingService(RaftingRepository repository, LocationRepository locationRepository, ILogger logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<RaftingDto> GetAll()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger?.Info("Getting all raftings");
                var result = _repository.GetAll();
                return result ?? new List<RaftingDto>();
            }
            catch (Exception ex)
            {
                _logger?.Error("Error in GetAll raftings", ex);
                throw new FaultException("Failed to retrieve raftings");
            }
            finally
            {
                _logger?.Debug($"GetAll raftings completed in {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Stop();
            }
        }

        public RaftingDto GetById(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger?.Info($"Getting rafting by ID: {id}");
                var result = _repository.GetById(id);
                if (result == null)
                {
                    _logger?.Warn($"Rafting with ID {id} not found");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error getting rafting by ID {id}", ex);
                throw new FaultException("Failed to retrieve rafting");
            }
            finally
            {
                _logger?.Debug($"GetById rafting completed in {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Stop();
            }
        }

        public int Create(RaftingDto rafting)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger?.Info("Creating new rafting");

                if (rafting == null)
                    throw new FaultException("Rafting cannot be null");

                var validationResult = Validate(rafting);
                if (!validationResult.IsValid)
                {
                    _logger?.Warn($"Rafting validation failed: {validationResult.GetErrorsAsString()}");
                    throw new FaultException($"Validation failed: {validationResult.GetErrorsAsString()}");
                }

                _repository.Add(rafting);
                _repository.SaveChanges();

                _logger?.Info($"Rafting created with ID: {rafting.Id}");
                return rafting.Id;
            }
            catch (FaultException)
            {
                throw; // Re-throw WCF faults
            }
            catch (Exception ex)
            {
                _logger?.Error("Error creating rafting", ex);
                throw new FaultException("Failed to create rafting");
            }
            finally
            {
                _logger?.Debug($"Create rafting completed in {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Stop();
            }
        }

        public bool Update(RaftingDto rafting)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger?.Info($"Updating rafting ID: {rafting?.Id}");

                if (rafting == null)
                    throw new FaultException("Rafting cannot be null");

                if (!_repository.Exists(rafting.Id))
                {
                    _logger?.Warn($"Rafting with ID {rafting.Id} not found for update");
                    return false;
                }

                var validationResult = Validate(rafting);
                if (!validationResult.IsValid)
                {
                    _logger?.Warn($"Rafting update validation failed: {validationResult.GetErrorsAsString()}");
                    throw new FaultException($"Validation failed: {validationResult.GetErrorsAsString()}");
                }

                _repository.Update(rafting);
                _repository.SaveChanges();

                _logger?.Info($"Rafting ID {rafting.Id} updated successfully");
                return true;
            }
            catch (FaultException)
            {
                throw; // Re-throw WCF faults
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error updating rafting ID {rafting?.Id}", ex);
                throw new FaultException("Failed to update rafting");
            }
            finally
            {
                _logger?.Debug($"Update rafting completed in {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Stop();
            }
        }

        public bool Delete(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger?.Info($"Deleting rafting ID: {id}");

                if (!_repository.Exists(id))
                {
                    _logger?.Warn($"Rafting with ID {id} not found for deletion");
                    return false;
                }

                _repository.Delete(id);
                _repository.SaveChanges();

                _logger?.Info($"Rafting ID {id} deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error deleting rafting ID {id}", ex);
                throw new FaultException("Failed to delete rafting");
            }
            finally
            {
                _logger?.Debug($"Delete rafting completed in {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Stop();
            }
        }

        public IEnumerable<RaftingDto> GetByState(RaftingState state)
        {
            try
            {
                _logger?.Info($"Getting raftings by state: {state}");
                return _repository.GetByState(state) ?? new List<RaftingDto>();
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error getting raftings by state {state}", ex);
                throw new FaultException("Failed to retrieve raftings by state");
            }
        }

        public IEnumerable<RaftingDto> GetActive()
        {
            try
            {
                _logger?.Info("Getting active raftings");
                return _repository.GetActive() ?? new List<RaftingDto>();
            }
            catch (Exception ex)
            {
                _logger?.Error("Error getting active raftings", ex);
                throw new FaultException("Failed to retrieve active raftings");
            }
        }

        public IEnumerable<RaftingDto> GetAvailableForBooking()
        {
            try
            {
                _logger?.Info("Getting raftings available for booking");
                return _repository.GetAvailableForBooking() ?? new List<RaftingDto>();
            }
            catch (Exception ex)
            {
                _logger?.Error("Error getting available raftings", ex);
                throw new FaultException("Failed to retrieve available raftings");
            }
        }

        public bool ChangeState(int raftingId, RaftingState newState)
        {
            try
            {
                _logger?.Info($"Changing rafting {raftingId} state to {newState}");

                var rafting = _repository.GetById(raftingId);
                if (rafting == null)
                {
                    _logger?.Warn($"Rafting with ID {raftingId} not found for state change");
                    return false;
                }

                var oldState = rafting.CurrentState;
                rafting.CurrentState = newState;
                rafting.ModifiedDate = DateTime.Now;

                _repository.Update(rafting);
                _repository.SaveChanges();

                _logger?.Info($"Rafting {raftingId} state changed from {oldState} to {newState}");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error changing rafting {raftingId} state", ex);
                throw new FaultException("Failed to change rafting state");
            }
        }

        public Dictionary<RaftingState, int> GetStateStatistics()
        {
            try
            {
                _logger?.Info("Getting state statistics");
                return _repository.GetStateStatistics() ?? new Dictionary<RaftingState, int>();
            }
            catch (Exception ex)
            {
                _logger?.Error("Error getting state statistics", ex);
                throw new FaultException("Failed to retrieve state statistics");
            }
        }

        public IEnumerable<RaftingDto> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger?.Info($"Getting raftings by date range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                return _repository.GetByDateRange(startDate, endDate) ?? new List<RaftingDto>();
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error getting raftings by date range", ex);
                throw new FaultException("Failed to retrieve raftings by date range");
            }
        }

        public IEnumerable<RaftingDto> GetByLocation(int locationId)
        {
            try
            {
                _logger?.Info($"Getting raftings by location: {locationId}");
                return _repository.GetByLocation(locationId) ?? new List<RaftingDto>();
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error getting raftings by location {locationId}", ex);
                throw new FaultException("Failed to retrieve raftings by location");
            }
        }

        public IEnumerable<RaftingDto> GetByPriceRange(decimal minPrice, decimal maxPrice)
        {
            try
            {
                _logger?.Info($"Getting raftings by price range: {minPrice} - {maxPrice}");
                return _repository.GetByPriceRange(minPrice, maxPrice) ?? new List<RaftingDto>();
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error getting raftings by price range", ex);
                throw new FaultException("Failed to retrieve raftings by price range");
            }
        }

        public ValidationResult Validate(RaftingDto rafting)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                if (rafting == null)
                {
                    result.AddError("Rafting cannot be null");
                    result.IsValid = false;
                    return result;
                }

                if (string.IsNullOrWhiteSpace(rafting.Name))
                    result.AddError("Name is required");

                if (rafting.StartTime >= rafting.EndTime)
                    result.AddError("Start time must be before end time");

                if (rafting.Distance <= 0)
                    result.AddError("Distance must be greater than 0");

                if (rafting.Capacity <= 0)
                    result.AddError("Capacity must be greater than 0");

                if (rafting.ParticipantCount > rafting.Capacity)
                    result.AddError("Participant count cannot exceed capacity");

                if (rafting.PricePerPerson < 0)
                    result.AddError("Price per person cannot be negative");

                if (_locationRepository != null)
                {
                    if (!_locationRepository.Exists(rafting.StartLocationId))
                        result.AddError($"Start location with ID {rafting.StartLocationId} does not exist");

                    if (!_locationRepository.Exists(rafting.EndLocationId))
                        result.AddError($"End location with ID {rafting.EndLocationId} does not exist");
                }

                result.IsValid = !result.Errors.Any();
                return result;
            }
            catch (Exception ex)
            {
                _logger?.Error("Error validating rafting", ex);
                result.AddError("Validation error occurred");
                result.IsValid = false;
                return result;
            }
        }
    }
}