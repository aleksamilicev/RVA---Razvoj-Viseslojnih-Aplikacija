using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVA.Server.Storage
{
    /// Factory za kreiranje storage instanci
    public class StorageFactory : IFactory<IDataStorage>
    {
        private readonly ILogger _logger;

        public StorageFactory(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDataStorage Create()
        {
            // Default je XML storage
            return Create("xml");
        }

        public IDataStorage Create(params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return Create();

            var storageType = parameters[0]?.ToString()?.ToLower() ?? "xml";
            return CreateStorage(storageType);
        }

        /// <summary>
        /// Kreira storage na osnovu tipa
        /// </summary>
        /// <param name="storageType">Tip storage-a (xml, json, csv)</param>
        public IDataStorage CreateStorage(string storageType)
        {
            _logger.Debug($"Creating storage of type: {storageType}");

            return storageType?.ToLower() switch
            {
                "xml" => new XmlStorage(_logger),
                "json" => new JsonStorage(_logger),
                "csv" => new CsvStorage(_logger),
                _ => throw new NotSupportedException($"Storage type '{storageType}' is not supported. Supported types: xml, json, csv")
            };
        }

        /// <summary>
        /// Vraća sve podržane tipove storage-a
        /// </summary>
        public IEnumerable<string> GetSupportedStorageTypes()
        {
            return new[] { "xml", "json", "csv" };
        }

        /// <summary>
        /// Proverava da li je tip storage-a podržan
        /// </summary>
        public bool IsStorageTypeSupported(string storageType)
        {
            return GetSupportedStorageTypes().Contains(storageType?.ToLower());
        }
    }
}