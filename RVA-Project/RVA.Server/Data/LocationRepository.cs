using RVA.Shared.DTOs;
using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVA.Server.Data
{
    /// Repository za Location entitete
    public class LocationRepository : BaseRepository<LocationDto>
    {
        public LocationRepository(IDataStorage dataStorage, ILogger logger)
            : base(dataStorage, logger, "location_data")
        {
        }

        protected override int GetEntityId(LocationDto entity)
        {
            return entity.Id;
        }

        protected override void SetEntityId(LocationDto entity, int id)
        {
            entity.Id = id;
            if (entity.CreatedDate == default(DateTime))
            {
                entity.CreatedDate = DateTime.Now;
            }
        }


        /// Pronalazi lokacije po reci
        public IEnumerable<LocationDto> GetByRiver(string river)
        {
            if (string.IsNullOrWhiteSpace(river))
                return Enumerable.Empty<LocationDto>();

            return Find(l => l.River.ToLower().Contains(river.ToLower()));
        }


        /// Pronalazi lokacije koje imaju parking
        public IEnumerable<LocationDto> GetWithParking()
        {
            return Find(l => l.HasParking);
        }


        /// Pronalazi lokacije koje imaju objekte/sadržaje
        public IEnumerable<LocationDto> GetWithFacilities()
        {
            return Find(l => l.HasFacilities);
        }

        public IEnumerable<LocationDto> GetWithStarted()
        {
            return Find(l => l.HasStarted);
        }


        /// Pronalazi lokacije koje imaju objekte/sadržaje
        public IEnumerable<LocationDto> GetWithEnded()
        {
            return Find(l => l.HasEnded);
        }


        /// Pronalazi lokacije u određenom radiusu od zadatih koordinata
        public IEnumerable<LocationDto> GetWithinRadius(double latitude, double longitude, double radiusKm)
        {
            return Find(l => CalculateDistance(latitude, longitude, l.Latitude, l.Longitude) <= radiusKm);
        }


        /// Pretražuje lokacije po imenu, opisu ili reci
        public IEnumerable<LocationDto> SearchByText(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return GetAll();

            var lowerSearch = searchText.ToLower();
            return Find(l => l.Name.ToLower().Contains(lowerSearch) ||
                           l.Description.ToLower().Contains(lowerSearch) ||
                           l.River.ToLower().Contains(lowerSearch));
        }


        /// Pronalazi najbližu lokaciju datim koordinatama
        public LocationDto GetNearest(double latitude, double longitude)
        {
            var allLocations = GetAll();
            if (!allLocations.Any())
                return null;

            return allLocations.OrderBy(l => CalculateDistance(latitude, longitude, l.Latitude, l.Longitude))
                              .FirstOrDefault();
        }


        /// Pronalazi lokacije sortirane po udaljenosti od datih koordinata
        public IEnumerable<LocationDto> GetOrderedByDistance(double latitude, double longitude)
        {
            return GetAll().OrderBy(l => CalculateDistance(latitude, longitude, l.Latitude, l.Longitude));
        }


        /// Pronalazi unikatne reke
        public IEnumerable<string> GetUniqueRivers()
        {
            return GetAll().Where(l => !string.IsNullOrWhiteSpace(l.River))
                          .Select(l => l.River)
                          .Distinct()
                          .OrderBy(r => r);
        }


        /// Haversine formula za izračunavanje distance između GPS koordinata
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in kilometers
            var lat1Rad = lat1 * Math.PI / 180;
            var lat2Rad = lat2 * Math.PI / 180;
            var deltaLat = (lat2 - lat1) * Math.PI / 180;
            var deltaLng = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }


        /// Validira GPS koordinate
        public bool ValidateCoordinates(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 &&
                   longitude >= -180 && longitude <= 180;
        }


        /// Pronalazi lokacije u određenom geografskom regionu
        public IEnumerable<LocationDto> GetInRegion(double minLat, double maxLat, double minLng, double maxLng)
        {
            return Find(l => l.Latitude >= minLat && l.Latitude <= maxLat &&
                           l.Longitude >= minLng && l.Longitude <= maxLng);
        }
    }
}