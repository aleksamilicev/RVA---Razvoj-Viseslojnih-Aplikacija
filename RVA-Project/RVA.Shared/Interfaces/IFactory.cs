using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Interfaces
{
    /// Factory Pattern - Generic interfejs za kreiranje objekata
    public interface IFactory<out T>
    {
        T Create();
        T Create(params object[] parameters);
    }

    /// Abstract Factory za kreiranje povezanih objekata
    public interface IAbstractFactory
    {
        IRepository<T> CreateRepository<T>() where T : class;
        IDataStorage CreateDataStorage(string storageType);
        IValidator<T> CreateValidator<T>() where T : class;
    }
}
