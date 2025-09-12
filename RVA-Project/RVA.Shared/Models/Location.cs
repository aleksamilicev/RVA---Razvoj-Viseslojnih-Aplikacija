using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string River { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Dodatna polja
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool HasParking { get; set; }
        public bool HasFacilities { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public List<Rafting> StartingRaftings { get; set; } = new List<Rafting>();
        public List<Rafting> EndingRaftings { get; set; } = new List<Rafting>();



        /* Prebaciti u RVA.Server/Services
         * 
         * 
         * 
        // Metode
        public double CalculateDistance(Location otherLocation)
        {
            if (otherLocation == null) return 0;

            // Haversine formula za izračunavanje distance između GPS koordinata
            var R = 6371; // Earth's radius in kilometers
            var lat1Rad = Latitude * Math.PI / 180;
            var lat2Rad = otherLocation.Latitude * Math.PI / 180;
            var deltaLat = (otherLocation.Latitude - Latitude) * Math.PI / 180;
            var deltaLng = (otherLocation.Longitude - Longitude) * Math.PI / 180;

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        public bool ValidateCoordinates()
        {
            return Latitude >= -90 && Latitude <= 90 &&
                   Longitude >= -180 && Longitude <= 180;
        }

        public Location Clone()
        {
            return new Location
            {
                Id = this.Id,
                River = this.River,
                Latitude = this.Latitude,
                Longitude = this.Longitude,
                Name = this.Name,
                Description = this.Description,
                HasParking = this.HasParking,
                HasFacilities = this.HasFacilities,
                CreatedDate = this.CreatedDate
            };
        }
        */
    }
}
