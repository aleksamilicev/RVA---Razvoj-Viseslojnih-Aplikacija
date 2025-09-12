using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Models
{
    public abstract class WaterActivity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public double Distance { get; set; }

        // Dodatna polja
        public int ParticipantCount { get; set; }
        public int MaxParticipants { get; set; }



        /* Prebaciti u RVA.Server/Services
         * 
         * 
         * 
        // Abstract metode
        public abstract bool ValidateActivity();
        public abstract TimeSpan CalculateDuration();

        // Virtualne metode
        public virtual void AddParticipant()
        {
            if (ParticipantCount < MaxParticipants)
                ParticipantCount++;
        }

        public virtual void RemoveParticipant()
        {
            if (ParticipantCount > 0)
                ParticipantCount--;
        }

        public virtual int GetRemainingCapacity()
        {
            return MaxParticipants - ParticipantCount;
        }
        */
    }
}
