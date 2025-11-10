using System.Collections.Concurrent;

namespace LF2.Kit.Service;

/// <summary>
/// Lightweight registry for mod-level services that enforces disposal symmetry.
/// </summary>
public static class ModServiceRegistry
{
    private static readonly ConcurrentDictionary<Type, IDisposable> services = new();

    public static TService Add<TService>(Func<TService> factory)
        where TService : IDisposable
    {
        return (TService)services.AddOrUpdate
        (
            typeof(TService),
            _ => factory(),
            (_, old) =>
            {
                old.Dispose();
                return factory();
            }
        );
    }

    public static bool TryGet<TService>(out TService? service)
        where TService : IDisposable
    {
        if (services.TryGetValue(typeof(TService), out var value))
        {
            service = (TService)value;
            return true;
        }

        service = default;
        return false;
    }

    public static bool Remove<TService>()
        where TService : class, IDisposable
    {
        return Remove(typeof(TService));
    }

    public static bool Remove(Type type)
    {
        if (services.TryRemove(type, out var removed))
        {
            removed.Dispose();
            return true;
        }

        return false;
    }

    public static void Clear()
    {
        foreach (var key in services.Keys)
        {
            _ = Remove(key);
        }
    }
}
