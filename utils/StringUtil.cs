    /// Contains utility functions for formatting localizable strings.
    internal class StringUtil
        /// Private constructor to present auto-generation of a default constructor with greater accessibility.
        private StringUtil()
        internal static string Format(string str)
            return string.Format(CultureInfo.CurrentCulture, str);
        internal static string Format(string fmt, string p0)
            return string.Format(CultureInfo.CurrentCulture, fmt, p0);
        internal static string Format(string fmt, string p0, string p1)
            return string.Format(CultureInfo.CurrentCulture, fmt, p0, p1);
        internal static string Format(string fmt, uint p0)
        internal static string Format(string fmt, int p0)
        internal static string FormatMessage(uint messageId, string[] args)
            var message = new System.Text.StringBuilder(256);
            UInt32 flags = Win32.FORMAT_MESSAGE_FROM_SYSTEM;
                flags |= Win32.FORMAT_MESSAGE_IGNORE_INSERTS;
                flags |= Win32.FORMAT_MESSAGE_ARGUMENT_ARRAY;
            var length = Win32.FormatMessage(flags, IntPtr.Zero, messageId, 0, message, 256, args);
                return message.ToString();
        internal static string GetSystemMessage(uint messageId)
            return FormatMessage(messageId, null);
    internal static class StringUtil
        internal static string Format(string format, object arg0)
            => string.Format(CultureInfo.CurrentCulture, format, arg0);
        internal static string Format(string format, object arg0, object arg1)
            => string.Format(CultureInfo.CurrentCulture, format, arg0, arg1);
        internal static string Format(string format, object arg0, object arg1, object arg2)
            => string.Format(CultureInfo.CurrentCulture, format, arg0, arg1, arg2);
        internal static string Format(string format, params object[] args)
            => string.Format(CultureInfo.CurrentCulture, format, args);
        internal static string TruncateToBufferCellWidth(PSHostRawUserInterface rawUI, string toTruncate, int maxWidthInBufferCells)
            Dbg.Assert(rawUI != null, "need a reference");
            Dbg.Assert(maxWidthInBufferCells >= 0, "maxWidthInBufferCells must be positive");
            int i = Math.Min(toTruncate.Length, maxWidthInBufferCells);
                result = toTruncate.Substring(0, i);
                int cellCount = rawUI.LengthInBufferCells(result);
                if (cellCount <= maxWidthInBufferCells)
                    // We need to back off 1 by 1 because there could theoretically
                    // be characters taking more 2 buffer cells
        // Typical padding is at most a screen's width, any more than that and we won't bother caching.
        private const int IndentCacheMax = 120;
        private static readonly string[] s_indentCache = new string[IndentCacheMax];
        internal static string Padding(int countOfSpaces)
            if (countOfSpaces >= IndentCacheMax)
                return new string(' ', countOfSpaces);
            var result = s_indentCache[countOfSpaces];
                Interlocked.CompareExchange(ref s_indentCache[countOfSpaces], new string(' ', countOfSpaces), null);
                result = s_indentCache[countOfSpaces];
        private const int DashCacheMax = 120;
        private static readonly string[] s_dashCache = new string[DashCacheMax];
        internal static string DashPadding(int count)
            if (count >= DashCacheMax)
                return new string('-', count);
            var result = s_dashCache[count];
                Interlocked.CompareExchange(ref s_dashCache[count], new string('-', count), null);
                result = s_dashCache[count];
        /// Substring implementation that takes into account the VT escape sequences.
        /// <param name="startOffset">
        /// When the string contains VT sequences, it means starting from the 'n-th' char that doesn't belong to a escape sequence.
        /// <returns>The requested substring.</returns>
        internal static string VtSubstring(this string str, int startOffset)
            return VtSubstring(str, startOffset, int.MaxValue, prependStr: null, appendStr: null);
        /// <param name="length">Number of non-escape-sequence characters to be included in the substring.</param>
        internal static string VtSubstring(this string str, int startOffset, int length)
            return VtSubstring(str, startOffset, length, prependStr: null, appendStr: null);
        /// <param name="prependStr">The string to be prepended to the substring.</param>
        /// <param name="appendStr">The string to be appended to the substring.</param>
        internal static string VtSubstring(this string str, int startOffset, string prependStr, string appendStr)
            return VtSubstring(str, startOffset, int.MaxValue, prependStr, appendStr);
        internal static string VtSubstring(this string str, int startOffset, int length, string prependStr, string appendStr)
                // Handle strings with VT sequences.
                bool copyStarted = startOffset == 0;
                bool firstNonEscChar = true;
                StringBuilder sb = new(capacity: str.Length);
                Dictionary<int, int> vtRanges = valueStrDec.EscapeSequenceRanges;
                for (int i = 0, offset = 0; i < str.Length; i++)
                    // Keep all leading ANSI escape sequences.
                    if (vtRanges.TryGetValue(i, out int len))
                        sb.Append(str.AsSpan(i, len));
                    // OK, now we get a non-escape-sequence character.
                    if (copyStarted)
                        if (firstNonEscChar)
                            // Prepend the string before we copy the first non-escape-sequence character.
                            sb.Append(prependStr);
                            firstNonEscChar = false;
                        // Copy this character if we've started the copy.
                        sb.Append(str[i]);
                        // Increment 'offset' to keep track of number of non-escape-sequence characters we've copied.
                    else if (++offset == startOffset)
                        // We've skipped enough non-escape-sequence characters, and will be copying the next one.
                        copyStarted = true;
                        // Reset 'offset' and from now on use it to track the number of copied non-escape-sequence characters.
                    // If the number of copied non-escape-sequence characters has reached the specified length, done copying.
                    if (copyStarted && offset == length)
                if (hasEscSeqs)
                    bool endsWithReset = sb.EndsWith(resetStr);
                    if (endsWithReset)
                        // Append the given string before the reset VT sequence.
                        sb.Insert(sb.Length - resetStr.Length, appendStr);
                        // Append the given string and add the reset VT sequence.
                        sb.Append(appendStr).Append(resetStr);
                    sb.Append(appendStr);
            // Handle strings without VT sequences.
            if (length == int.MaxValue)
                length = str.Length - startOffset;
            if (prependStr is null && appendStr is null)
                return str.Substring(startOffset, length);
                int capacity = length + prependStr?.Length ?? 0 + appendStr?.Length ?? 0;
                return new StringBuilder(prependStr, capacity)
                    .Append(str, startOffset, length)
                    .Append(appendStr)
                    .ToString();
        internal static bool EndsWith(this StringBuilder sb, string value)
            if (sb.Length < value.Length)
            int offset = sb.Length - value.Length;
                if (sb[offset + i] != value[i])
