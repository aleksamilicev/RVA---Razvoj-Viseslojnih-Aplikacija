using RVA.Server.Data;
using RVA.Server.Logging;
using RVA.Shared.DTOs;
using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVA.Server.Factories
{
    public static class RepositoryFactory
    {
        public static RaftingRepository CreateRaftingRepository()
        {
            var logger = new ServerLogger();
            var storage = StorageFactory.CreateStorage("csv", logger);
            return new RaftingRepository(storage, logger);
        }

        public static LocationRepository CreateLocationRepository()
        {
            var logger = new ServerLogger();
            var storage = StorageFactory.CreateStorage("csv", logger);
            return new LocationRepository(storage, logger);
        }

        public static ClothingRepository CreateClothingRepository()
        {
            var logger = new ServerLogger();
            var storage = StorageFactory.CreateStorage("csv", logger);
            return new ClothingRepository(storage, logger);
        }
    }
}