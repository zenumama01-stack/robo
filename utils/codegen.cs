    /// Contains utility methods for use in applications that generate PowerShell code.
    public static class CodeGeneration
        /// Escapes content so that it is safe for inclusion in a single-quoted string.
        /// For example: "'" + EscapeSingleQuotedStringContent(userContent) + "'"
        /// <param name="value">The content to be included in a single-quoted string.</param>
        /// <returns>Content with all single-quotes escaped.</returns>
        public static string EscapeSingleQuotedStringContent(string value)
            StringBuilder sb = new StringBuilder(value.Length);
            foreach (char c in value)
                if (CharExtensions.IsSingleQuote(c))
                    // double-up quotes to escape them
        /// Escapes content so that it is safe for inclusion in a block comment.
        /// For example: "&lt;#" + EscapeBlockCommentContent(userContent) + "#&gt;"
        /// <param name="value">The content to be included in a block comment.</param>
        /// <returns>Content with all block comment characters escaped.</returns>
        public static string EscapeBlockCommentContent(string value)
            return value
                .Replace("<#", "<`#")
                .Replace("#>", "#`>");
        /// Escapes content so that it is safe for inclusion in a string that will later be used as a
        /// format string. If this is to be embedded inside of a single-quoted string, be sure to also
        /// call EscapeSingleQuotedStringContent.
        /// For example: "'" + EscapeSingleQuotedStringContent(EscapeFormatStringContent(userContent)) + "'" -f $args.
        /// <param name="value">The content to be included in a format string.</param>
        /// <returns>Content with all curly braces escaped.</returns>
        public static string EscapeFormatStringContent(string value)
                if (CharExtensions.IsCurlyBracket(c))
                    // double-up curly brackets to escape them
        /// Escapes content so that it is safe for inclusion in a string that will later be used in a variable
        /// name reference. This is only valid when used within PowerShell's curly brace naming syntax.
        /// For example: '${' + EscapeVariableName('value') + '}'
        /// <param name="value">The content to be included as a variable name.</param>
        /// <returns>Content with all curly braces and back-ticks escaped.</returns>
        public static string EscapeVariableName(string value)
                .Replace("`", "``")
                .Replace("}", "`}")
                .Replace("{", "`{");
