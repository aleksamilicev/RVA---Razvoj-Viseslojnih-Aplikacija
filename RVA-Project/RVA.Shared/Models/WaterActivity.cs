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
    }
}
