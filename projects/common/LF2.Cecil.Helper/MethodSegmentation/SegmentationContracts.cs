using MonoMod.Cil;
using System.Reflection;

namespace LF2.Cecil.Helper.MethodSegmentation;

public interface ISplitConfig<T> where T : Delegate
{
    MethodInfo Prototype { get; }
    IEnumerable<Type> InjectSplitPoint(ILCursor ilCursor);
}

public interface IContinuationConfig<T> where T : Delegate
{
    MethodInfo Prototype { get; }
    void InjectContinuationPoint(ILCursor ilCursor);
}
