using RVA.Server.Interfaces;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace RVA.Client.Services
{
    /// <summary>
    /// Centralized WCF client for all services
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

        #region Properties
        public IRaftingService RaftingService
        {
            get
            {
                if (_raftingService == null)
                {
                    _raftingService = InitializeService(ref _raftingChannelFactory, ref _raftingService, "RaftingServiceEndpoint");
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
                    _locationService = InitializeService(ref _locationChannelFactory, ref _locationService, "LocationServiceEndpoint");
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
                    _clothingService = InitializeService(ref _clothingChannelFactory, ref _clothingService, "ClothingServiceEndpoint");
                }
                return _clothingService;
            }
        }
        #endregion

        #region Initialization
        private T InitializeService<T>(ref ChannelFactory<T> factory, ref T service, string endpointName) where T : class
        {
            lock (_lock)
            {
                if (service != null) return service;

                try
                {
                    factory = new ChannelFactory<T>(endpointName);
                    service = factory.CreateChannel();
                    ((ICommunicationObject)service).Open();
                    return service;
                }
                catch (Exception ex)
                {
                    throw new ServiceException($"Failed to initialize service {typeof(T).Name}: {ex.Message}", ex);
                }
            }
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

        #region Connection Management
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
