namespace LF2.Kit.Random;

public static class RollHelper
{
    public static T RetryAndCompare<T>(Func<T> generator, IComparer<T> comparer, int retryCount)
    {
        return Enumerable
        .Range(0, retryCount + 1)
        .Select(_ => generator())
        .Aggregate((best, next) => comparer.Compare(next, best) > 0 ? next : best);
    }
}