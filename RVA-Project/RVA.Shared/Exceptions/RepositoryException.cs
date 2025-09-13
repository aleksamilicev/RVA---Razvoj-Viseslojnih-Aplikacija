using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Exceptions
{
    /// Izuzetak za probleme sa repository operacijama
    [Serializable]
    public class RepositoryException : Exception
    {
        public RepositoryException() : base()
        {
        }

        public RepositoryException(string message) : base(message)
        {
        }

        public RepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RepositoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string RepositoryName { get; set; }
        public string Operation { get; set; }
        public int? EntityId { get; set; }
        public string AdditionalInfo { get; set; }

        public override string ToString()
        {
            var info = $"{base.ToString()}";

            if (!string.IsNullOrEmpty(RepositoryName))
                info += $"\nRepository: {RepositoryName}";

            if (!string.IsNullOrEmpty(Operation))
                info += $"\nOperation: {Operation}";

            if (EntityId.HasValue)
                info += $"\nEntity ID: {EntityId}";

            if (!string.IsNullOrEmpty(AdditionalInfo))
                info += $"\nAdditional Info: {AdditionalInfo}";

            return info;
        }
    }
}
