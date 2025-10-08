using System.Reflection.Emit;
using HarmonyLib;

namespace LF2.Cecil.Helper;

public static class CodeMatherDebugger
{
    public static string Display(CodeMatcher matcher)
    {
        var labelDict = new Dictionary<Label, int>
        (
            matcher.InstructionEnumeration()
            .SelectMany
            (
                (x, i) => x.labels.Select
                (
                    label => KeyValuePair.Create(label, i)
                )
            )
        );

        return string.Join
        (
            "\n",
            matcher.InstructionEnumeration()
            .Select
            (
                (x, index) => $"IL_{index:X4}: {x.opcode.Name}"
                    + x.operand switch
                    {
                        Label label => $" IL_{labelDict[label]:X4}",
                        { } normal => $" {normal}",
                        _ => ""
                    }
            )
        );
    }
}