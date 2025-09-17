using RVA.Shared.Interfaces;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace RVA.Client.Services
{
    /// <summary>
    /// Centralized WCF client for all services with better error handling
    /// </summary>
    public class WcfServiceClient : IDisposable
    {
        #region Private Fields
        private ChannelFactory<IRaftingService> _raftingChannelFactory;
        private ChannelFactory<ILocationService> _locationChannelFactory;
        private ChannelFactory<IClothingService> _clothingChannelFactory;

        private IRaftingService _raftingService;
        private ILocationService _locationService;
        private IClothingService _clothingService;

        private readonly object _lock = new object();
        private bool _disposed = false;
        #endregion

        #region Properties with Better Error Handling
        public IRaftingService RaftingService
        {
            get
            {
                if (_raftingService == null)
                {
                    InitializeRaftingService();
                }
                return _raftingService;
            }
        }

        public ILocationService LocationService
        {
            get
            {
                if (_locationService == null)
                {
                    InitializeLocationService();
                }
                return _locationService;
            }
        }

        public IClothingService ClothingService
        {
            get
            {
                if (_clothingService == null)
                {
                    InitializeClothingService();
                }
                return _clothingService;
            }
        }
        #endregion

        #region Initialization Methods
        private void InitializeRaftingService()
        {
            lock (_lock)
            {
                if (_raftingService != null) return;

                try
                {
                    _raftingChannelFactory = new ChannelFactory<IRaftingService>("RaftingServiceEndpoint");
                    _raftingService = _raftingChannelFactory.CreateChannel();

                    // Test da li kanal radi
                    ((ICommunicationObject)_raftingService).Open();
                }
                catch (Exception ex)
                {
                    throw new ServiceException($"Failed to initialize RaftingService. Check if server is running on correct port and endpoint exists. Details: {ex.Message}", ex);
                }
            }
        }

        private void InitializeLocationService()
        {
            lock (_lock)
            {
                if (_locationService != null) return;

                try
                {
                    _locationChannelFactory = new ChannelFactory<ILocationService>("LocationServiceEndpoint");
                    _locationService = _locationChannelFactory.CreateChannel();
                    ((ICommunicationObject)_locationService).Open();
                }
                catch (Exception ex)
                {
                    throw new ServiceException($"Failed to initialize LocationService. Details: {ex.Message}", ex);
                }
            }
        }

        private void InitializeClothingService()
        {
            lock (_lock)
            {
                if (_clothingService != null) return;

                try
                {
                    _clothingChannelFactory = new ChannelFactory<IClothingService>("ClothingServiceEndpoint");
                    _clothingService = _clothingChannelFactory.CreateChannel();
                    ((ICommunicationObject)_clothingService).Open();
                }
                catch (Exception ex)
                {
                    throw new ServiceException($"Failed to initialize ClothingService. Details: {ex.Message}", ex);
                }
            }
        }
        #endregion

        #region Connection Management with Detailed Diagnostics
        public string TestConnectionWithDetails()
        {
            var results = new List<string>();

            // Test RaftingService
            try
            {
                var raftings = RaftingService.GetAll();
                results.Add("✓ RaftingService: Connected successfully");
            }
            catch (EndpointNotFoundException ex)
            {
                results.Add($"✗ RaftingService: Endpoint not found - {ex.Message}");
            }
            catch (CommunicationException ex)
            {
                results.Add($"✗ RaftingService: Communication error - {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                results.Add($"✗ RaftingService: Timeout - {ex.Message}");
            }
            catch (Exception ex)
            {
                results.Add($"✗ RaftingService: Unknown error - {ex.Message}");
            }

            // Test LocationService
            try
            {
                var locations = LocationService.GetAll();
                results.Add("✓ LocationService: Connected successfully");
            }
            catch (EndpointNotFoundException ex)
            {
                results.Add($"✗ LocationService: Endpoint not found - {ex.Message}");
            }
            catch (Exception ex)
            {
                results.Add($"✗ LocationService: Error - {ex.Message}");
            }

            // Test ClothingService
            try
            {
                var clothes = ClothingService.GetAll();
                results.Add("✓ ClothingService: Connected successfully");
            }
            catch (EndpointNotFoundException ex)
            {
                results.Add($"✗ ClothingService: Endpoint not found - {ex.Message}");
            }
            catch (Exception ex)
            {
                results.Add($"✗ ClothingService: Error - {ex.Message}");
            }

            return string.Join("\n", results);
        }

        public bool TestConnection()
        {
            try
            {
                RaftingService.GetAll();
                LocationService.GetAll();
                ClothingService.GetAll();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void RefreshConnections()
        {
            Dispose();
            _disposed = false;
            _raftingService = null;
            _locationService = null;
            _clothingService = null;
        }
        #endregion

        #region Safe Service Call Helpers
        public T Execute<T>(Func<T> call, string operation)
        {
            try
            {
                return call();
            }
            catch (CommunicationException ex)
            {
                throw new ServiceException($"Communication error during {operation}: {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServiceException($"Timeout during {operation}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error during {operation}: {ex.Message}", ex);
            }
        }

        public void Execute(Action call, string operation)
        {
            try
            {
                call();
            }
            catch (CommunicationException ex)
            {
                throw new ServiceException($"Communication error during {operation}: {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                throw new ServiceException($"Timeout during {operation}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Error during {operation}: {ex.Message}", ex);
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_disposed) return;

            CloseService(_raftingService, _raftingChannelFactory);
            CloseService(_locationService, _locationChannelFactory);
            CloseService(_clothingService, _clothingChannelFactory);

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void CloseService<T>(T service, ChannelFactory<T> factory) where T : class
        {
            if (service != null)
            {
                var commObj = (ICommunicationObject)service;
                try
                {
                    if (commObj.State == CommunicationState.Opened)
                        commObj.Close();
                    else if (commObj.State == CommunicationState.Faulted)
                        commObj.Abort();
                }
                catch
                {
                    commObj.Abort();
                }
            }

            factory?.Close();
        }

        ~WcfServiceClient() => Dispose();
        #endregion
    }

    #region Custom Exception
    public class ServiceException : Exception
    {
        public ServiceException(string message) : base(message) { }
        public ServiceException(string message, Exception inner) : base(message, inner) { }
    }
    #endregion
}