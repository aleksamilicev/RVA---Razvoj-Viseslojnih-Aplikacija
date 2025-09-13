using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.DTOs
{
    [DataContract]
    public class LocationDto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string River { get; set; } = string.Empty;

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }

        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public bool HasParking { get; set; }

        [DataMember]
        public bool HasFacilities { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
