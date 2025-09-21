using RVA.Server.Data;
using RVA.Server.Factories;
using RVA.Server.Logging;
using RVA.Shared.DTOs;
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
    public class LocationService : ILocationService
    {
        private readonly LocationRepository _repository;
        private readonly ILogger _logger;

        public LocationService()
        {
            // Inicijalizuj dependencies kroz factory
            _repository = RepositoryFactory.CreateLocationRepository();
            _logger = new ServerLogger();
        }

        /*
        public LocationService()
        {
            // Inicijalizuj dependencies kroz factory
            _repository = RepositoryFactory.CreateLocationRepository();
            //_locationRepository = RepositoryFactory.CreateLocationRepository();
            _logger = new ServerLogger();
        }*/

        public LocationService(LocationRepository repository, ILogger logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<LocationDto> GetAll()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.Info("Getting all locations");
                return _repository.GetAll();
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetAll locations", ex);
                throw new FaultException("Failed to retrieve locations");
            }
            finally
            {
                _logger.Debug($"GetAll locations completed in {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Stop();
            }
        }

        public LocationDto GetById(int id)
        {
            try
            {
                _logger.Info($"Getting location by ID: {id}");
                var result = _repository.GetById(id);
                if (result == null)
                {
                    _logger.Warn($"Location with ID {id} not found");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting location by ID {id}", ex);
                throw new FaultException("Failed to retrieve location");
            }
        }

        public int Create(LocationDto location)
        {
            try
            {
                _logger.Info("Creating new location");

                if (location == null)
                    throw new ArgumentNullException(nameof(location));

                var validationResult = Validate(location);
                if (!validationResult.IsValid)
                {
                    _logger.Warn($"Location validation failed: {validationResult.GetErrorsAsString()}");
                    throw new FaultException($"Validation failed: {validationResult.GetErrorsAsString()}");
                }

                _repository.Add(location);
                _repository.SaveChanges();

                _logger.Info($"Location created with ID: {location.Id}");
                return location.Id;
            }
            catch (Exception ex)
            {
                _logger.Error("Error creating location", ex);
                throw new FaultException("Failed to create location");
            }
        }

        public bool Update(LocationDto location)
        {
            try
            {
                _logger.Info($"Updating location ID: {location?.Id}");

                if (location == null)
                    throw new ArgumentNullException(nameof(location));

                if (!_repository.Exists(location.Id))
                {
                    _logger.Warn($"Location with ID {location.Id} not found for update");
                    return false;
                }

                var validationResult = Validate(location);
                if (!validationResult.IsValid)
                {
                    _logger.Warn($"Location update validation failed: {validationResult.GetErrorsAsString()}");
                    throw new FaultException($"Validation failed: {validationResult.GetErrorsAsString()}");
                }

                _repository.Update(location);
                _repository.SaveChanges();

                _logger.Info($"Location ID {location.Id} updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating location ID {location?.Id}", ex);
                throw new FaultException("Failed to update location");
            }
        }

        public bool Delete(int id)
        {
            try
            {
                _logger.Info($"Deleting location ID: {id}");

                if (!_repository.Exists(id))
                {
                    _logger.Warn($"Location with ID {id} not found for deletion");
                    return false;
                }

                _repository.Delete(id);
                _repository.SaveChanges();

                _logger.Info($"Location ID {id} deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting location ID {id}", ex);
                throw new FaultException("Failed to delete location");
            }
        }

        public IEnumerable<LocationDto> GetByRiver(string river)
        {
            try
            {
                _logger.Info($"Getting locations by river: {river}");
                return _repository.GetByRiver(river);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting locations by river {river}", ex);
                throw new FaultException("Failed to retrieve locations by river");
            }
        }

        public IEnumerable<LocationDto> GetWithParking()
        {
            try
            {
                _logger.Info("Getting locations with parking");
                return _repository.GetWithParking();
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting locations with parking", ex);
                throw new FaultException("Failed to retrieve locations with parking");
            }
        }

        public IEnumerable<LocationDto> GetWithFacilities()
        {
            try
            {
                _logger.Info("Getting locations with facilities");
                return _repository.GetWithFacilities();
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting locations with facilities", ex);
                throw new FaultException("Failed to retrieve locations with facilities");
            }
        }

        public IEnumerable<LocationDto> GetWithStarted()
        {
            try
            {
                _logger.Info("Getting locations with parking");
                return _repository.GetWithStarted();
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting locations with started", ex);
                throw new FaultException("Failed to retrieve locations with started");
            }
        }

        public IEnumerable<LocationDto> GetWithEnded()
        {
            try
            {
                _logger.Info("Getting locations with ended");
                return _repository.GetWithEnded();
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting locations with ended", ex);
                throw new FaultException("Failed to retrieve locations with ended");
            }
        }

        public IEnumerable<LocationDto> GetWithinRadius(double latitude, double longitude, double radiusKm)
        {
            try
            {
                _logger.Info($"Getting locations within radius {radiusKm}km of {latitude},{longitude}");
                return _repository.GetWithinRadius(latitude, longitude, radiusKm);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting locations within radius", ex);
                throw new FaultException("Failed to retrieve locations within radius");
            }
        }

        public IEnumerable<LocationDto> SearchByText(string searchText)
        {
            try
            {
                _logger.Info($"Searching locations by text: {searchText}");
                return _repository.SearchByText(searchText);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error searching locations by text '{searchText}'", ex);
                throw new FaultException("Failed to search locations");
            }
        }

        public LocationDto GetNearest(double latitude, double longitude)
        {
            try
            {
                _logger.Info($"Getting nearest location to {latitude},{longitude}");
                return _repository.GetNearest(latitude, longitude);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting nearest location", ex);
                throw new FaultException("Failed to get nearest location");
            }
        }

        public IEnumerable<string> GetUniqueRivers()
        {
            try
            {
                _logger.Info("Getting unique rivers");
                return _repository.GetUniqueRivers();
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting unique rivers", ex);
                throw new FaultException("Failed to retrieve unique rivers");
            }
        }

        public ValidationResult Validate(LocationDto location)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                if (location == null)
                {
                    result.AddError("Location cannot be null");
                    result.IsValid = false;
                    return result;
                }

                if (string.IsNullOrWhiteSpace(location.Name))
                    result.AddError("Name is required");

                if (string.IsNullOrWhiteSpace(location.River))
                    result.AddError("River is required");

                if (location.Latitude < -90 || location.Latitude > 90)
                    result.AddError("Latitude must be between -90 and 90");

                if (location.Longitude < -180 || location.Longitude > 180)
                    result.AddError("Longitude must be between -180 and 180");

                result.IsValid = !result.Errors.Any();
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Error validating location", ex);
                result.AddError("Validation error occurred");
                result.IsValid = false;
                return result;
            }
        }
    }
}