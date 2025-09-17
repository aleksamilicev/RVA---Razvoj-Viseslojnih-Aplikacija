using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RVA.Shared.Models
{
    public class Rafting : WaterActivity
    {
        // Osnovne properties iz UML-a
        public DateTime EndTime { get; set; }
        public Intensity CurrentIntensity { get; set; }
        public double CurrentSpeedKmh { get; set; }
        public int Capacity { get; set; }

        // Dodane properties
        public RaftingState CurrentState { get; set; } = RaftingState.Planned;
        public int GuideId { get; set; }
        public decimal PricePerPerson { get; set; }
        public string WeatherConditions { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // ID properties - ove će biti serijalizovane
        public int StartLocationId { get; set; }
        public int EndLocationId { get; set; }

        // Navigation properties - ove neće biti serijalizovane u XML
        [XmlIgnore]
        public Location StartLocation { get; set; } = new Location();

        [XmlIgnore]
        public Location EndLocation { get; set; } = new Location();

        [XmlIgnore]
        public List<Clothing> UsedClothing { get; set; } = new List<Clothing>();

        [XmlIgnore]
        public List<Equipment> UsedEquipment { get; set; } = new List<Equipment>();

        // Dodajte ove properties za ID-jeve clothing i equipment objekata
        // Ovi će biti serijalizovani umesto kompleksnih objekata
        public List<int> ClothingIds { get; set; } = new List<int>();
        public List<int> EquipmentIds { get; set; } = new List<int>();
    }
}