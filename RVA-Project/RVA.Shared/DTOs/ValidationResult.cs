using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.DTOs
{
    [DataContract]
    public class ValidationResult
    {
        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public List<string> Errors { get; set; } = new List<string>();

        [DataMember]
        public List<string> Warnings { get; set; } = new List<string>();

        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
                Errors.Add(error);
        }

        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
                Warnings.Add(warning);
        }

        public string GetErrorsAsString()
        {
            return string.Join(Environment.NewLine, Errors);
        }

        public string GetWarningsAsString()
        {
            return string.Join(Environment.NewLine, Warnings);
        }
    }
}
