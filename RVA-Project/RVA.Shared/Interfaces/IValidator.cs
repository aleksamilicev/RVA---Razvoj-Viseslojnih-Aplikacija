using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Interfaces
{
    /// Validator interfejs za validaciju entiteta
    public interface IValidator<in T>
    {
        ValidationResult Validate(T entity);
        ValidationResult ValidateProperty(T entity, string propertyName);
    }

    /// Rezultat validacije
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
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
