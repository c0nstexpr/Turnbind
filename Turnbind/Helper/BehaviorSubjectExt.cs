using System.Reactive.Subjects;

namespace Turnbind.Helper;

public static class BehaviorSubjectExt
{
    public static BehaviorObservable<T> AsObservable<T>(this BehaviorSubject<T> subject) => new(subject);
}
