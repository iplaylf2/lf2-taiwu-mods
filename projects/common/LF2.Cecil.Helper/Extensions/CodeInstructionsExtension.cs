using System.Reflection.Emit;
using HarmonyLib;

namespace LF2.Cecil.Helper.Extensions;

public static class CodeInstructionsExtension
{
    public static bool TryGetLoc(this IEnumerable<CodeInstruction> instructions, int index, out LocalBuilder? loc)
    {
        var matcher = new CodeMatcher(instructions)
        .MatchStartForward
        (
            new CodeMatch
            (
                x => x.opcode == OpCodes.Ldloc_S
                    && x.operand is LocalBuilder loc
                    && loc.LocalIndex == index
            )
        );

        if (matcher.IsInvalid)
        {
            loc = null;

            return false;
        }

        loc = matcher.Operand as LocalBuilder;

        return true;
    }
}
