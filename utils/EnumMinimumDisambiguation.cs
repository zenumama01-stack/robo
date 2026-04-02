    /// Performs enum minimum disambiguation.
    internal static class EnumMinimumDisambiguation
        /// Initialize the dictionary for special cases of minimum disambiguation.
        static EnumMinimumDisambiguation()
            // Add special minimum disambiguation cases here for certain enum types.
            // The current implementation assumes that special names in each type can be
            // differentiated by their first letter.
            s_specialDisambiguateCases.Add(
                typeof(System.IO.FileAttributes),
                new string[] { "Directory", "ReadOnly", "System" });
        /// Perform disambiguation on enum names.
        /// <returns>Complete enum name after disambiguation.</returns>
        internal static string EnumDisambiguate(string text, Type enumType)
            // Get all enum names in the given enum type
            string[] enumNames = Enum.GetNames(enumType);
            // Get all names that matches the given prefix.
            List<string> namesWithMatchingPrefix = new List<string>();
            foreach (string name in enumNames)
                if (name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                    namesWithMatchingPrefix.Add(name);
            // Throw error when no match is found.
            if (namesWithMatchingPrefix.Count == 0)
                    null, "NoEnumNameMatch", EnumExpressionEvaluatorStrings.NoEnumNameMatch, text, EnumAllValues(enumType));
            // Return the result if there is only one match.
            else if (namesWithMatchingPrefix.Count == 1)
                return namesWithMatchingPrefix[0];
            // multiple matches situation
                // test for exact match
                foreach (string matchName in namesWithMatchingPrefix)
                    if (matchName.Equals(text, StringComparison.OrdinalIgnoreCase))
                        return matchName;
                // test for special cases match
                string[] minDisambiguateNames;
                if (s_specialDisambiguateCases.TryGetValue(enumType, out minDisambiguateNames))
                    foreach (string tName in minDisambiguateNames)
                        if (tName.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                            return tName;
                // No special cases match, throw error for multiple matches.
                StringBuilder matchListSB = new StringBuilder(namesWithMatchingPrefix[0]);
                for (int i = 1; i < namesWithMatchingPrefix.Count; i++)
                    matchListSB.Append(separator);
                    matchListSB.Append(namesWithMatchingPrefix[i]);
                    null, "MultipleEnumNameMatch", EnumExpressionEvaluatorStrings.MultipleEnumNameMatch,
                    text, matchListSB.ToString());
        /// Produces a string that contains all the enumerator names in an enum type.
        /// <param name="enumType"></param>
        internal static string EnumAllValues(Type enumType)
            string[] names = Enum.GetNames(enumType);
            if (names.Length != 0)
                for (int i = 0; i < names.Length; i++)
                    returnValue.Append(names[i]);
                    returnValue.Append(separator);
                returnValue.Remove(returnValue.Length - separator.Length, separator.Length);
        private static readonly Dictionary<Type, string[]> s_specialDisambiguateCases = new Dictionary<Type, string[]>();
