using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RVA.Shared.Enums;

namespace RVA.Shared.DTOs
{
    [DataContract]
    public class RaftingDto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public double Distance { get; set; }

        [DataMember]
        public Intensity CurrentIntensity { get; set; }

        [DataMember]
        public double CurrentSpeedKmh { get; set; }

        [DataMember]
        public int Capacity { get; set; }

        [DataMember]
        public RaftingState CurrentState { get; set; }

        [DataMember]
        public int GuideId { get; set; }

        [DataMember]
        public decimal PricePerPerson { get; set; }

        [DataMember]
        public string WeatherConditions { get; set; } = string.Empty;

        [DataMember]
        public int ParticipantCount { get; set; }

        [DataMember]
        public int MaxParticipants { get; set; }

        [DataMember]
        public int StartLocationId { get; set; }

        [DataMember]
        public int EndLocationId { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public DateTime ModifiedDate { get; set; }

        // Lists za WCF transfer
        [DataMember]
        public List<int> UsedClothingIds { get; set; } = new List<int>();

        [DataMember]
        public List<int> UsedEquipmentIds { get; set; } = new List<int>();
    }
}
