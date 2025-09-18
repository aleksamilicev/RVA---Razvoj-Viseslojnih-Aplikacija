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

    }
}
