using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Interfaces
{
    /// Generic Repository Pattern interfejs za data access
    /// <typeparam name="T">Tip entiteta</typeparam>
    public interface IRepository<T> where T : class
    {
        // CRUD operacije
        IEnumerable<T> GetAll();
        T GetById(int id);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        void Delete(T entity);

        // Bulk operacije
        void AddRange(IEnumerable<T> entities);
        void RemoveRange(IEnumerable<T> entities);

        // Persistence
        void SaveChanges();

        // Count i provera postojanja
        int Count();
        bool Exists(int id);
    }
}
