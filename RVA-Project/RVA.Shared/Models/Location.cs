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

        public bool HasStarted { get; set; }
        public bool HasEnded { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public List<Rafting> StartingRaftings { get; set; } = new List<Rafting>();
        public List<Rafting> EndingRaftings { get; set; } = new List<Rafting>();
    }
}
