using System.Reflection.Emit;
using HarmonyLib;

namespace LF2.Cecil.Helper.Extensions;

public static class CodeMatcherExtensions
{
    public static bool TryGetLoc(this CodeMatcher codeMatcher, int index, out LocalBuilder? loc)
    {
        _ = codeMatcher
        .Start()
        .MatchStartForward
        (
            new CodeMatch
            (
                x => x.opcode == OpCodes.Ldloc_S
                    && x.operand is LocalBuilder loc
                    && loc.LocalIndex == index
            )
        );

        if (codeMatcher.IsInvalid)
        {
            loc = null;

            return false;
        }

        loc = codeMatcher.Operand as LocalBuilder;

        return true;
    }
}
