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
            var storage = StorageFactory.CreateStorage("xml"); // ili csv/json
            var logger = new ServerLogger();
            return new RaftingRepository((Shared.Interfaces.IDataStorage)storage, logger);
        }

        public static LocationRepository CreateLocationRepository()
        {
            var storage = StorageFactory.CreateStorage("xml");
            var logger = new ServerLogger();
            return new LocationRepository((Shared.Interfaces.IDataStorage)storage, logger);
        }
    }

}