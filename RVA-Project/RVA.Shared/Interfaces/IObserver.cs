using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Interfaces
{
    /// Observer Pattern - Interfejs za komponente koje se pretplaćuju na promene
    public interface IObserver<in T>
    {
        void Update(T data);
        void OnError(Exception error);
        void OnCompleted();
    }

    /// Subject interfejs za komponente koje objavljuju 
    public interface ISubject<T>
    {
        void Subscribe(IObserver<T> observer);
        void Unsubscribe(IObserver<T> observer);
        void NotifyObservers(T data);
        void NotifyError(Exception error);
        void NotifyCompleted();
    }
}
