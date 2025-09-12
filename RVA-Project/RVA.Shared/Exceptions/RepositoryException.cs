using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Exceptions
{
    /// Izuzetak za probleme sa repository operacijama
    public class RepositoryException : Exception
    {
        public RepositoryException() : base() { }

        public RepositoryException(string message) : base(message) { }

        public RepositoryException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
