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



        /* Prebaciti u RVA.Server/Services
         * 
         * 
         * 
        // Metode
        public bool ValidateSize()
        {
            return Size > 0 && Size <= 60; // Veličine od 1 do 60
        }

        public void MarkAsUsed()
        {
            IsAvailable = false;
            LastCleaned = DateTime.Now.AddDays(-1); // Označava da treba pranje
        }

        public void MarkAsAvailable()
        {
            IsAvailable = true;
            LastCleaned = DateTime.Now;
        }

        public void UpdateCondition(string newCondition)
        {
            Condition = newCondition;
        }

        public Clothing Clone()
        {
            return new Clothing
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                Material = this.Material,
                IsWaterproof = this.IsWaterproof,
                Size = this.Size,
                Brand = this.Brand,
                Color = this.Color,
                IsAvailable = this.IsAvailable,
                LastCleaned = this.LastCleaned,
                Condition = this.Condition
            };
        }
        */
    }
}
