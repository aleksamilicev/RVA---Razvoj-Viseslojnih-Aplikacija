using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Models
{
    public class Clothing
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ClothingType Type { get; set; }
        public Material Material { get; set; }
        public bool IsWaterproof { get; set; }
        public int Size { get; set; }

        // Dodatna polja
        public string Brand { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public DateTime LastCleaned { get; set; } = DateTime.Now;
        public string Condition { get; set; } = "Good";

        // Navigation properties
        public List<Rafting> Raftings { get; set; } = new List<Rafting>();
    }
}
