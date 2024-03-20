using System.Reactive.Subjects;

namespace Turnbind.Helper;

public class BehaviorObservable<T>(BehaviorSubject<T> subject) : IObservable<T>
{
    public T Value => subject.Value;

    public IDisposable Subscribe(IObserver<T> observer) => subject.Subscribe(observer);
}
