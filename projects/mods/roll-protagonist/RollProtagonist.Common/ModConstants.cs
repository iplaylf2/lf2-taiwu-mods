using System.Diagnostics.CodeAnalysis;

namespace RollProtagonist.Common;

[SuppressMessage
("Design", "CA1034:Nested types should not be visible")
]
public static class ModConstants
{
    public static class Method
    {
        public static class ExecuteInitial
        {
            public static class Parameters
            {
                public const string creationInfo = "creationInfo";
            }
        }

        public static class ExecuteRoll
        {
            public static class ReturnValue
            {
                public const string character = "character";
            }
        }
    }
}