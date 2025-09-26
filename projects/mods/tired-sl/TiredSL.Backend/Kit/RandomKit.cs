namespace TiredSL.Backend.Kit;

internal static class RandomKit
{
    public static T NiceRetry<T>(Func<T> generator, IComparer<T> comparer, int retryCount)
    {
        return Enumerable
            .Range(0, retryCount + 1)
            .Select(_ => generator())
            .Aggregate((best, next) => comparer.Compare(next, best) > 0 ? next : best);
    }
}