using RVA.Server.Data;
using RVA.Server.Logging;
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
            var storage = StorageFactory.CreateStorage("xml", logger); // ili csv/json
            return new RaftingRepository((Shared.Interfaces.IDataStorage)storage, logger);
        }

        public static LocationRepository CreateLocationRepository()
        {
            var logger = new ServerLogger();
            var storage = StorageFactory.CreateStorage("xml", logger);
            return new LocationRepository((Shared.Interfaces.IDataStorage)storage, logger);
        }
    }

}