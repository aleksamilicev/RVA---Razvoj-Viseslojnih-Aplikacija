using log4net.Repository.Hierarchy;
using RVA.Server.Interfaces;
using RVA.Server.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVA.Server.Factories
{
    public static class StorageFactory
    {
        public static RVA.Shared.Interfaces.IDataStorage CreateStorage(string type, RVA.Shared.Interfaces.ILogger logger)
        {
            return type.ToLower() switch
            {
                "xml" => new XmlStorage(logger),
                "csv" => new CsvStorage(logger),
                "json" => new JsonStorage(logger),
                _ => throw new ArgumentException($"Unknown storage type: {type}")
            };
        }
    }

}