using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Models
{
    public class Kayaking : WaterActivity
    {
        // Polja iz UML-a
        public Intensity Intensity { get; set; }
        public KayakType Type { get; set; }
        public bool IsSolo { get; set; }

        // Dodatna polja
        public string SkillLevel { get; set; } = "Beginner";
        public bool HasInstructor { get; set; }
        public int KayakId { get; set; }



        /* Prebaciti u RVA.Server/Services
         * 
         * 
         * 
        // Abstract metode implementacije
        public override TimeSpan CalculateDuration()
        {
            // Kayaking duration se računa drugačije - na osnovu distance i intensity
            var baseSpeed = Intensity switch
            {
                Intensity.Low => 5.0,
                Intensity.Medium => 8.0,
                Intensity.High => 12.0,
                Intensity.Extreme => 15.0,
                _ => 5.0
            };

            var hours = Distance / baseSpeed;
            return TimeSpan.FromHours(hours);
        }

        public override bool ValidateActivity()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   Distance > 0 &&
                   KayakId > 0 &&
                   MaxParticipants > 0 &&
                   (!IsSolo || MaxParticipants == 1); // Solo kayaking može imati samo 1 učesnika
        }

        // Specifične metode za Kayaking
        public void AssignKayak(int kayakId)
        {
            if (kayakId > 0)
                KayakId = kayakId;
        }

        public bool ValidateSkillLevel()
        {
            var validLevels = new[] { "Beginner", "Intermediate", "Advanced", "Expert" };
            return Array.Exists(validLevels, level => level == SkillLevel);
        }

        public Kayaking Clone()
        {
            return new Kayaking
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                StartTime = this.StartTime,
                Duration = this.Duration,
                Distance = this.Distance,
                Intensity = this.Intensity,
                Type = this.Type,
                IsSolo = this.IsSolo,
                SkillLevel = this.SkillLevel,
                HasInstructor = this.HasInstructor,
                KayakId = this.KayakId,
                ParticipantCount = this.ParticipantCount,
                MaxParticipants = this.MaxParticipants
            };
        }
        */
    }
}
