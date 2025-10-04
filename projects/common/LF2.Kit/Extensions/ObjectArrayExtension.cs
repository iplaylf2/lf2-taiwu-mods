using System.Runtime.CompilerServices;

namespace LF2.Kit.Extensions;

public static class ObjectArrayExtension
{
    public static ITuple AsTuple(this object[] objects)
    {
        return new ObjectArrayTuple(objects);
    }
}

file sealed class ObjectArrayTuple(object[] source) : ITuple
{
    public object this[int index] => source[index];

    public int Length => source.Length;
}