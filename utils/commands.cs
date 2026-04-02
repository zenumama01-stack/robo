    internal static class EnumerableExpansionConversion
        internal const string CoreOnlyString = "CoreOnly";
        internal const string EnumOnlyString = "EnumOnly";
        internal const string BothString = "Both";
        internal static bool Convert(string expansionString, out EnumerableExpansion expansion)
            expansion = EnumerableExpansion.EnumOnly;
            if (string.Equals(expansionString, CoreOnlyString, StringComparison.OrdinalIgnoreCase))
                expansion = EnumerableExpansion.CoreOnly;
            if (string.Equals(expansionString, EnumOnlyString, StringComparison.OrdinalIgnoreCase))
            if (string.Equals(expansionString, BothString, StringComparison.OrdinalIgnoreCase))
                expansion = EnumerableExpansion.Both;
