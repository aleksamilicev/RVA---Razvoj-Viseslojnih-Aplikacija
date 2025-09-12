using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.DTOs
{
    [DataContract]
    public class ClothingDto
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public ClothingType Type { get; set; }

        [DataMember]
        public Material Material { get; set; }

        [DataMember]
        public bool IsWaterproof { get; set; }

        [DataMember]
        public int Size { get; set; }

        [DataMember]
        public string Brand { get; set; } = string.Empty;

        [DataMember]
        public string Color { get; set; } = string.Empty;

        [DataMember]
        public bool IsAvailable { get; set; }

        [DataMember]
        public DateTime LastCleaned { get; set; }

        [DataMember]
        public string Condition { get; set; } = string.Empty;
    }
}
