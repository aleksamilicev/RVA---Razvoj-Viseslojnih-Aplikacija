using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        // Dodatna polja
        public string SerialNumber { get; set; } = string.Empty;
        public string Condition { get; set; } = "Good";
        public DateTime LastMaintenance { get; set; } = DateTime.Now;

        // Navigation properties
        public List<Rafting> Raftings { get; set; } = new List<Rafting>();



        /* Prebaciti u RVA.Server/Services
         * 
         * 
         * 
        // Metode
        public void MarkAsUsed()
        {
            IsAvailable = false;
        }

        public void MarkAsAvailable()
        {
            IsAvailable = true;
        }

        public void UpdateCondition(string newCondition)
        {
            Condition = newCondition;
        }

        public void ScheduleMaintenance()
        {
            LastMaintenance = DateTime.Now;
            IsAvailable = false;
        }

        public Equipment Clone()
        {
            return new Equipment
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                IsAvailable = this.IsAvailable,
                SerialNumber = this.SerialNumber,
                Condition = this.Condition,
                LastMaintenance = this.LastMaintenance
            };
        }
        */
    }
}
