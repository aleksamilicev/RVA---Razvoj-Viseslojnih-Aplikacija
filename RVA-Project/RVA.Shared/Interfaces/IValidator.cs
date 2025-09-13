using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationResult = RVA.Shared.DTOs.ValidationResult;

namespace RVA.Shared.Interfaces
{
    /// Validator interfejs za validaciju entiteta
    public interface IValidator<in T>
    {
        ValidationResult Validate(T entity);
        ValidationResult ValidateProperty(T entity, string propertyName);
    }
}
