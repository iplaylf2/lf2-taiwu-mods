using GameData.Serializer;
using GameData.Utilities;

namespace LF2.Game.Helper;

internal static class StringSerializer
{
    private const int DefaultInitialCapacity = 1024;

    public static string Serialize<T>(T item, int initialCapacity = DefaultInitialCapacity)
    {
        if (initialCapacity <= 0)
        {
            initialCapacity = DefaultInitialCapacity;
        }

        using var dataPool = new RawDataPool(initialCapacity);
        SerializerHolder<T>.Serialize(item, dataPool);

        var dataSize = dataPool.RawDataSize;
        if (dataSize == 0)
        {
            return string.Empty;
        }

        var buffer = new byte[dataSize];
        dataPool.SetStreamReadingOffset(0);
        dataPool.Read(buffer, 0, dataSize);

        return Convert.ToBase64String(buffer);
    }

    public static T? Deserialize<T>(string encodedString)
    {
        if (string.IsNullOrEmpty(encodedString))
        {
            return default;
        }

        var bytes = Convert.FromBase64String(encodedString);

        using var dataPool = new RawDataPool(bytes.Length);
        dataPool.Write(bytes, 0, bytes.Length);

        T? item = default;
        if (item is null)
        {
            return item;
        }

        SerializerHolder<T>.Deserialize(dataPool, 0, ref item);

        return item;
    }
}