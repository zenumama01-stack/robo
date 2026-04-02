using System.Diagnostics.Contracts;
    /// Provides enumerated values to use to set wildcard pattern
    /// matching options.
    public enum WildcardOptions
        /// Indicates that no special processing is required.
        /// Specifies that the wildcard pattern is compiled to an assembly.
        /// This yields faster execution but increases startup time.
        Compiled = 1,
        /// Specifies case-insensitive matching.
        IgnoreCase = 2,
        /// Specifies culture-invariant matching.
        CultureInvariant = 4
    /// Represents a wildcard pattern.
    public sealed class WildcardPattern
        // char that escapes special chars
        private const char escapeChar = '`';
        // Threshold for stack allocation.
        // The size is less than MaxShortPath = 260.
        private const int StackAllocThreshold = 256;
        // chars that are considered special in a wildcard pattern
        private const string SpecialChars = "*?[]`";
        // we convert a wildcard pattern to a predicate
        private Predicate<string> _isMatch;
        // static match-all delegate that is shared by all WildcardPattern instances
        private static readonly Predicate<string> s_matchAll = _ => true;
        // wildcard pattern
        internal string Pattern { get; }
        // Options that control match behavior.
        // Default is WildcardOptions.None.
        internal WildcardOptions Options { get; }
        /// Initializes and instance of the WildcardPattern class
        /// for the specified wildcard pattern.
        /// <param name="pattern">The wildcard pattern to match.</param>
        /// <returns>The constructed WildcardPattern object.</returns>
        public WildcardPattern(string pattern) : this(pattern, WildcardOptions.None)
        /// Initializes an instance of the WildcardPattern class for
        /// the specified wildcard pattern expression, with options
        /// that modify the pattern.
        /// <param name="options">Wildcard options.</param>
        public WildcardPattern(string pattern, WildcardOptions options)
            Pattern = pattern;
        private static readonly WildcardPattern s_matchAllIgnoreCasePattern = new WildcardPattern("*", WildcardOptions.None);
        /// Create a new WildcardPattern, or return an already created one.
        /// <param name="pattern">The pattern.</param>
        public static WildcardPattern Get(string pattern, WildcardOptions options)
            if (pattern.Length == 1 && pattern[0] == '*')
                return s_matchAllIgnoreCasePattern;
            return new WildcardPattern(pattern, options);
        /// Instantiate internal regex member if not already done.
            StringComparison GetStringComparison()
                StringComparison stringComparison;
                if (Options.HasFlag(WildcardOptions.IgnoreCase))
                    stringComparison = Options.HasFlag(WildcardOptions.CultureInvariant)
                        ? StringComparison.InvariantCultureIgnoreCase
                        : CultureInfo.CurrentCulture.Name.Equals("en-US-POSIX", StringComparison.OrdinalIgnoreCase)
                            // The collation behavior of the POSIX locale (also known as the C locale) is case sensitive.
                            // For this specific locale, we use 'OrdinalIgnoreCase'.
                            ? StringComparison.OrdinalIgnoreCase
                            : StringComparison.CurrentCultureIgnoreCase;
                        ? StringComparison.InvariantCulture
                        : StringComparison.CurrentCulture;
                return stringComparison;
            if (_isMatch != null)
            if (Pattern.Length == 1 && Pattern[0] == '*')
                _isMatch = s_matchAll;
            int index = Pattern.AsSpan().IndexOfAny(SpecialChars);
                // No special characters present in the pattern, so we can just do a string comparison.
                _isMatch = str => string.Equals(str, Pattern, GetStringComparison());
            if (index == Pattern.Length - 1 && Pattern[index] == '*')
                // No special characters present in the pattern before last position and last character is asterisk.
                var patternWithoutAsterisk = Pattern.AsMemory(0, index);
                _isMatch = str => str.AsSpan().StartsWith(patternWithoutAsterisk.Span, GetStringComparison());
            var matcher = new WildcardPatternMatcher(this);
            _isMatch = matcher.IsMatch;
        /// Indicates whether the wildcard pattern specified in the WildcardPattern
        /// constructor finds a match in the input string.
        /// <param name="input">The string to search for a match.</param>
        /// <returns>True if the wildcard pattern finds a match; otherwise, false.</returns>
        public bool IsMatch(string input)
            return input != null && _isMatch(input);
        /// Converts the wildcard pattern to its regular expression equivalent.
        /// A <see cref="Regex"/> object that represents the regular expression equivalent of the wildcard pattern.
        /// The regex is configured with options matching the wildcard pattern's options.
        /// This method converts a wildcard pattern to a regular expression.
        /// The conversion follows these rules:
        /// <list type="bullet">
        /// <item><description>* (asterisk) converts to .* (matches any string)</description></item>
        /// <item><description>? (question mark) converts to . (matches any single character)</description></item>
        /// <item><description>[abc] (bracket expression) converts to [abc] (matches any character in the set)</description></item>
        /// <item><description>Literal characters are escaped as needed for regex</description></item>
        /// </list>
        /// var pattern = new WildcardPattern("*.txt");
        /// Regex regex = pattern.ToRegex();
        /// // regex.ToString() returns: "\.txt$"
        public Regex ToRegex()
            return WildcardPatternToRegexParser.Parse(this);
        /// Escape special chars, except for those specified in <paramref name="charsNotToEscape"/>, in a string by replacing them with their escape codes.
        /// <param name="pattern">The input string containing the text to convert.</param>
        /// <param name="charsNotToEscape">Array of characters that not to escape.</param>
        /// A string of characters with any metacharacters, except for those specified in <paramref name="charsNotToEscape"/>, converted to their escaped form.
        internal static string Escape(string pattern, char[] charsNotToEscape)
            if (charsNotToEscape == null)
                throw PSTraceSource.NewArgumentNullException(nameof(charsNotToEscape));
            if (pattern == string.Empty)
                return pattern;
            Span<char> temp = pattern.Length < StackAllocThreshold ? stackalloc char[pattern.Length * 2 + 1] : new char[pattern.Length * 2 + 1];
            int tempIndex = 0;
            for (int i = 0; i < pattern.Length; i++)
                char ch = pattern[i];
                // if it is a special char, escape it
                if (SpecialChars.Contains(ch) && !charsNotToEscape.Contains(ch))
                    temp[tempIndex++] = escapeChar;
                temp[tempIndex++] = ch;
            if (tempIndex == pattern.Length)
                s = pattern;
                s = new string(temp.Slice(0, tempIndex));
        /// Escape special chars in a string by replacing them with their escape codes.
        /// A string of characters with any metacharacters converted to their escaped form.
        public static string Escape(string pattern)
            return Escape(pattern, Array.Empty<char>());
        /// Checks to see if the given string has any wild card characters in it.
        /// String which needs to be checked for the presence of wildcard chars
        /// <returns>True if the string has wild card chars, false otherwise..</returns>
        /// Currently { '*', '?', '[' } are considered wild card chars and
        /// '`' is the escape character.
        public static bool ContainsWildcardCharacters(string pattern)
            if (string.IsNullOrEmpty(pattern))
            for (int index = 0; index < pattern.Length; ++index)
                if (IsWildcardChar(pattern[index]))
                // If it is an escape character then advance past
                // the next character
                if (pattern[index] == escapeChar)
        /// Checks if the string contains a left bracket "[" followed by a right bracket "]" after any number of characters.
        /// <param name="pattern"> The string to check.</param>
        /// <returns>Returns true if the string contains both a left and right bracket "[" "]" and if the right bracket comes after the left bracket.</returns>
        internal static bool ContainsRangeWildcard(string pattern)
            bool foundStart = false;
                if (pattern[index] is '[')
                    foundStart = true;
                if (foundStart && pattern[index] is ']')
        /// Unescapes any escaped characters in the input string.
        /// The input string containing the text to convert.
        /// A string of characters with any escaped characters
        /// converted to their unescaped form.
        /// If <paramref name="pattern"/> is null.
        public static string Unescape(string pattern)
            Span<char> temp = pattern.Length < StackAllocThreshold ? stackalloc char[pattern.Length] : new char[pattern.Length];
            bool prevCharWasEscapeChar = false;
                if (ch == escapeChar)
                    if (prevCharWasEscapeChar)
                        prevCharWasEscapeChar = false;
                        prevCharWasEscapeChar = true;
                    if (!IsWildcardChar(ch))
            // Need to account for a trailing escape character as a real
            // character
        private static bool IsWildcardChar(char ch)
            return (ch == '*') || (ch == '?') || (ch == '[') || (ch == ']');
        /// Converts this wildcard to a string that can be used as a right-hand-side operand of the LIKE operator of WQL.
        /// For example: "a*" will be converted to "a%".
        public string ToWql()
            bool needsClientSideFiltering;
            string likeOperand = Microsoft.PowerShell.Cmdletization.Cim.WildcardPatternToCimQueryParser.Parse(this, out needsClientSideFiltering);
            if (!needsClientSideFiltering)
                return likeOperand;
                    "UnsupportedWildcardToWqlConversion",
                    this.Pattern,
                    this.GetType().FullName,
                    "WQL");
    /// Thrown when a wildcard pattern is invalid.
    public class WildcardPatternException : RuntimeException
        /// Constructor for class WildcardPatternException that takes
        /// an ErrorRecord to use in constructing this exception.
        /// <remarks>This is the recommended constructor to use for this exception.</remarks>
        /// ErrorRecord object containing additional information about the error condition.
        internal WildcardPatternException(ErrorRecord errorRecord)
            : base(RetrieveMessage(errorRecord))
        /// Constructs an instance of the WildcardPatternException object.
        public WildcardPatternException()
        /// Constructs an instance of the WildcardPatternException object taking
        /// a message parameter to use in constructing the exception.
        /// <param name="message">The string to use as the exception message.</param>
        public WildcardPatternException(string message) : base(message)
        /// Constructor for class WildcardPatternException that takes both a message to use
        /// and an inner exception to include in this object.
        /// <param name="message">The exception message to use.</param>
        /// <param name="innerException">The innerException object to encapsulate.</param>
        public WildcardPatternException(string message,
        /// Constructor for class WildcardPatternException for serialization.
        protected WildcardPatternException(SerializationInfo info,
    /// A base class for parsers of <see cref="WildcardPattern"/> patterns.
    internal abstract class WildcardPatternParser
        /// Called from <see cref="Parse"/> method to indicate
        /// the beginning of the wildcard pattern.
        /// Default implementation simply returns.
        /// <see cref="WildcardPattern"/> object that includes both
        /// the text of the pattern (<see cref="WildcardPattern.Pattern"/>)
        /// and the pattern options (<see cref="WildcardPattern.Options"/>)
        protected virtual void BeginWildcardPattern(WildcardPattern pattern)
        /// Called from <see cref="Parse"/> method to indicate that the next
        /// part of the pattern should match
        /// a literal character <paramref name="c"/>.
        protected abstract void AppendLiteralCharacter(char c);
        /// any string, including an empty string.
        protected abstract void AppendAsterix();
        /// any single character.
        protected abstract void AppendQuestionMark();
        /// Called from <see cref="Parse"/> method to indicate the end of the wildcard pattern.
        protected virtual void EndWildcardPattern()
        /// the beginning of a bracket expression.
        /// Bracket expressions of <see cref="WildcardPattern"/> are
        /// a greatly simplified version of bracket expressions of POSIX wildcards
        /// (https://www.opengroup.org/onlinepubs/9699919799/functions/fnmatch.html).
        /// Only literal characters and character ranges are supported.
        /// Negation (with either '!' or '^' characters),
        /// character classes ([:alpha:])
        /// and other advanced features are not supported.
        protected abstract void BeginBracketExpression();
        /// Called from <see cref="Parse"/> method to indicate that the bracket expression
        /// should include a literal character <paramref name="c"/>.
        protected abstract void AppendLiteralCharacterToBracketExpression(char c);
        /// should include all characters from character range
        /// starting at <paramref name="startOfCharacterRange"/>
        /// and ending at <paramref name="endOfCharacterRange"/>
        protected abstract void AppendCharacterRangeToBracketExpression(
                        char startOfCharacterRange,
                        char endOfCharacterRange);
        /// Called from <see cref="Parse"/> method to indicate the end of a bracket expression.
        protected abstract void EndBracketExpression();
        /// PowerShell v1 and v2 treats all characters inside
        /// <paramref name="brackedExpressionContents"/> as literal characters,
        /// except '-' sign which denotes a range.  In particular it means that
        /// '^', '[', ']' are escaped within the bracket expression and don't
        /// have their regex-y meaning.
        /// <param name="brackedExpressionContents"></param>
        /// <param name="bracketExpressionOperators"></param>
        /// <param name="pattern"></param>
        /// This method should be kept "internal"
        internal void AppendBracketExpression(string brackedExpressionContents, string bracketExpressionOperators, string pattern)
            while (i < brackedExpressionContents.Length)
                if (((i + 2) < brackedExpressionContents.Length) &&
                                (bracketExpressionOperators[i + 1] == '-'))
                    char lowerBound = brackedExpressionContents[i];
                    char upperBound = brackedExpressionContents[i + 2];
                    i += 3;
                    if (lowerBound > upperBound)
                        throw NewWildcardPatternException(pattern);
                    this.AppendCharacterRangeToBracketExpression(lowerBound, upperBound);
                    this.AppendLiteralCharacterToBracketExpression(brackedExpressionContents[i]);
        /// Parses <paramref name="pattern"/>, calling appropriate overloads
        /// in <paramref name="parser"/>
        /// <param name="pattern">Pattern to parse.</param>
        /// <param name="parser">Parser to call back.</param>
        public static void Parse(WildcardPattern pattern, WildcardPatternParser parser)
            parser.BeginWildcardPattern(pattern);
            bool previousCharacterIsAnEscape = false;
            bool previousCharacterStartedBracketExpression = false;
            bool insideCharacterRange = false;
            StringBuilder characterRangeContents = null;
            StringBuilder characterRangeOperators = null;
            foreach (char c in pattern.Pattern)
                if (insideCharacterRange)
                    if (c == ']' && !previousCharacterStartedBracketExpression && !previousCharacterIsAnEscape)
                        // An unescaped closing square bracket closes the character set.  In other
                        // words, there are no nested square bracket expressions
                        // This is different than the POSIX spec
                        // (at https://www.opengroup.org/onlinepubs/9699919799/functions/fnmatch.html),
                        // but we are keeping this behavior for back-compatibility.
                        insideCharacterRange = false;
                        parser.AppendBracketExpression(characterRangeContents.ToString(), characterRangeOperators.ToString(), pattern.Pattern);
                        characterRangeContents = null;
                        characterRangeOperators = null;
                    else if (c != '`' || previousCharacterIsAnEscape)
                        characterRangeContents.Append(c);
                        characterRangeOperators.Append((c == '-') && !previousCharacterIsAnEscape ? '-' : ' ');
                    previousCharacterStartedBracketExpression = false;
                    if (c == '*' && !previousCharacterIsAnEscape)
                        parser.AppendAsterix();
                    else if (c == '?' && !previousCharacterIsAnEscape)
                        parser.AppendQuestionMark();
                    else if (c == '[' && !previousCharacterIsAnEscape)
                        insideCharacterRange = true;
                        characterRangeContents = new StringBuilder();
                        characterRangeOperators = new StringBuilder();
                        previousCharacterStartedBracketExpression = true;
                        parser.AppendLiteralCharacter(c);
                previousCharacterIsAnEscape = (c == '`') && (!previousCharacterIsAnEscape);
                throw NewWildcardPatternException(pattern.Pattern);
            if (previousCharacterIsAnEscape)
                if (!pattern.Pattern.Equals("`", StringComparison.Ordinal)) // Win7 backcompatibility requires treating '`' pattern as '' pattern
                    parser.AppendLiteralCharacter(pattern.Pattern[pattern.Pattern.Length - 1]);
            parser.EndWildcardPattern();
        internal static WildcardPatternException NewWildcardPatternException(string invalidPattern)
                StringUtil.Format(WildcardPatternStrings.InvalidPattern,
                    invalidPattern
            ParentContainsErrorRecordException pce =
                new ParentContainsErrorRecordException(message);
                new ErrorRecord(pce,
                                 "WildcardPattern_Invalid",
            WildcardPatternException e =
                new WildcardPatternException(er);
    /// Convert a string with wild cards into its equivalent regex.
    /// A list of glob patterns and their equivalent regexes
    ///  glob pattern      regex
    /// -------------     -------
    /// *foo*              foo
    /// foo                ^foo$
    /// foo*bar            ^foo.*bar$
    /// foo`*bar           ^foo\*bar$
    /// for a more cases see the unit-test file RegexTest.cs
    internal class WildcardPatternToRegexParser : WildcardPatternParser
        private StringBuilder _regexPattern;
        private RegexOptions _regexOptions;
        private const string regexChars = "()[.?*{}^$+|\\"; // ']' is missing on purpose
        private static bool IsRegexChar(char ch)
            for (int i = 0; i < regexChars.Length; i++)
                if (ch == regexChars[i])
        internal static RegexOptions TranslateWildcardOptionsIntoRegexOptions(WildcardOptions options)
            RegexOptions regexOptions = RegexOptions.Singleline;
            if ((options & WildcardOptions.Compiled) != 0)
                regexOptions |= RegexOptions.Compiled;
            if ((options & WildcardOptions.IgnoreCase) != 0)
                regexOptions |= RegexOptions.IgnoreCase;
            if ((options & WildcardOptions.CultureInvariant) == WildcardOptions.CultureInvariant)
                regexOptions |= RegexOptions.CultureInvariant;
            return regexOptions;
        protected override void BeginWildcardPattern(WildcardPattern pattern)
            _regexPattern = new StringBuilder(pattern.Pattern.Length * 2 + 2);
            _regexPattern.Append('^');
            _regexOptions = TranslateWildcardOptionsIntoRegexOptions(pattern.Options);
        internal static void AppendLiteralCharacter(StringBuilder regexPattern, char c)
            if (IsRegexChar(c))
                regexPattern.Append('\\');
            regexPattern.Append(c);
            AppendLiteralCharacter(_regexPattern, c);
            _regexPattern.Append(".*");
            _regexPattern.Append('.');
        protected override void EndWildcardPattern()
            _regexPattern.Append('$');
            // lines below are not strictly necessary and are included to preserve
            // wildcard->regex conversion from PS v1 (i.e. not to break unit tests
            // and not to break backcompatibility).
            string regexPatternString = _regexPattern.ToString();
            if (regexPatternString.Equals("^.*$", StringComparison.Ordinal))
                _regexPattern.Remove(0, 4);
                if (regexPatternString.StartsWith("^.*", StringComparison.Ordinal))
                    _regexPattern.Remove(0, 3);
                if (regexPatternString.EndsWith(".*$", StringComparison.Ordinal))
                    _regexPattern.Remove(_regexPattern.Length - 3, 3);
            _regexPattern.Append('[');
        internal static void AppendLiteralCharacterToBracketExpression(StringBuilder regexPattern, char c)
            if (c == '[')
                regexPattern.Append('[');
            else if (c == ']')
                regexPattern.Append(@"\]");
            else if (c == '-')
                regexPattern.Append(@"\x2d");
                AppendLiteralCharacter(regexPattern, c);
            AppendLiteralCharacterToBracketExpression(_regexPattern, c);
        internal static void AppendCharacterRangeToBracketExpression(
                        StringBuilder regexPattern,
                        char endOfCharacterRange)
            AppendLiteralCharacterToBracketExpression(regexPattern, startOfCharacterRange);
            regexPattern.Append('-');
            AppendLiteralCharacterToBracketExpression(regexPattern, endOfCharacterRange);
        protected override void AppendCharacterRangeToBracketExpression(
            AppendCharacterRangeToBracketExpression(_regexPattern, startOfCharacterRange, endOfCharacterRange);
            _regexPattern.Append(']');
        /// Parses a <paramref name="wildcardPattern"/> into a <see cref="Regex"/>
        /// <param name="wildcardPattern">Wildcard pattern to parse.</param>
        /// <returns>Regular expression equivalent to <paramref name="wildcardPattern"/></returns>
        public static Regex Parse(WildcardPattern wildcardPattern)
            WildcardPatternToRegexParser parser = new WildcardPatternToRegexParser();
                return ParserOps.NewRegex(parser._regexPattern.ToString(), parser._regexOptions);
                throw WildcardPatternParser.NewWildcardPatternException(wildcardPattern.Pattern);
    internal class WildcardPatternMatcher
        private readonly PatternElement[] _patternElements;
        private readonly CharacterNormalizer _characterNormalizer;
        internal WildcardPatternMatcher(WildcardPattern wildcardPattern)
            _characterNormalizer = new CharacterNormalizer(wildcardPattern.Options);
            _patternElements = MyWildcardPatternParser.Parse(
                            wildcardPattern,
                            _characterNormalizer);
        internal bool IsMatch(string str)
            // - each state of NFA is represented by (patternPosition, stringPosition) tuple
            //     - state transitions are documented in
            //       ProcessStringCharacter and ProcessEndOfString methods
            // - the algorithm below tries to see if there is a path
            //   from (0, 0) to (lengthOfPattern, lengthOfString)
            //    - this is a regular graph traversal
            //    - there are O(1) edges per node (at most 2 edges)
            //      so the whole graph traversal takes O(number of nodes in the graph) =
            //      = O(lengthOfPattern * lengthOfString) time
            //    - for efficient remembering which states have already been visited,
            //      the traversal goes methodically from beginning to end of the string
            //      therefore requiring only O(lengthOfPattern) memory for remembering
            //      which states have been already visited
            //  - Wikipedia calls this algorithm the "NFA" algorithm at
            //    https://en.wikipedia.org/wiki/Regular_expression#Implementations_and_running_times
            var patternPositionsForCurrentStringPosition =
                    new PatternPositionsVisitor(_patternElements.Length);
            patternPositionsForCurrentStringPosition.Add(0);
            var patternPositionsForNextStringPosition =
                for (int currentStringPosition = 0;
                    currentStringPosition < str.Length;
                    currentStringPosition++)
                    char currentStringCharacter = _characterNormalizer.Normalize(str[currentStringPosition]);
                    patternPositionsForCurrentStringPosition.StringPosition = currentStringPosition;
                    patternPositionsForNextStringPosition.StringPosition = currentStringPosition + 1;
                    int patternPosition;
                    while (patternPositionsForCurrentStringPosition.MoveNext(out patternPosition))
                        _patternElements[patternPosition].ProcessStringCharacter(
                            currentStringCharacter,
                            patternPosition,
                            patternPositionsForCurrentStringPosition,
                            patternPositionsForNextStringPosition);
                    // swap patternPositionsForCurrentStringPosition
                    // with patternPositionsForNextStringPosition
                    var tmp = patternPositionsForCurrentStringPosition;
                    patternPositionsForCurrentStringPosition = patternPositionsForNextStringPosition;
                    patternPositionsForNextStringPosition = tmp;
                int patternPosition2;
                while (patternPositionsForCurrentStringPosition.MoveNext(out patternPosition2))
                    _patternElements[patternPosition2].ProcessEndOfString(
                        patternPosition2,
                        patternPositionsForCurrentStringPosition);
                return patternPositionsForCurrentStringPosition.ReachedEndOfPattern;
                patternPositionsForCurrentStringPosition.Dispose();
                patternPositionsForNextStringPosition.Dispose();
        private sealed class PatternPositionsVisitor : IDisposable
            private readonly int _lengthOfPattern;
            private readonly int[] _isPatternPositionVisitedMarker;
            private readonly int[] _patternPositionsForFurtherProcessing;
            private int _patternPositionsForFurtherProcessingCount;
            public PatternPositionsVisitor(int lengthOfPattern)
                Dbg.Assert(lengthOfPattern >= 0, "Caller should verify lengthOfPattern >= 0");
                _lengthOfPattern = lengthOfPattern;
                _isPatternPositionVisitedMarker = ArrayPool<int>.Shared.Rent(_lengthOfPattern + 1);
                for (int i = 0; i <= _lengthOfPattern; i++)
                    _isPatternPositionVisitedMarker[i] = -1;
                _patternPositionsForFurtherProcessing = ArrayPool<int>.Shared.Rent(_lengthOfPattern);
                _patternPositionsForFurtherProcessingCount = 0;
                ArrayPool<int>.Shared.Return(_isPatternPositionVisitedMarker, clearArray: true);
                ArrayPool<int>.Shared.Return(_patternPositionsForFurtherProcessing, clearArray: true);
            public int StringPosition { private get; set; }
            public void Add(int patternPosition)
                Dbg.Assert(patternPosition >= 0, "Caller should verify patternPosition >= 0");
                        patternPosition <= _lengthOfPattern,
                        "Caller should verify patternPosition <= this._lengthOfPattern");
                // is patternPosition already visited?
                if (_isPatternPositionVisitedMarker[patternPosition] == this.StringPosition)
                // mark patternPosition as visited
                _isPatternPositionVisitedMarker[patternPosition] = this.StringPosition;
                // add patternPosition to the queue for further processing
                if (patternPosition < _lengthOfPattern)
                    _patternPositionsForFurtherProcessing[_patternPositionsForFurtherProcessingCount] = patternPosition;
                    _patternPositionsForFurtherProcessingCount++;
                            _patternPositionsForFurtherProcessingCount <= _lengthOfPattern,
                            "There should never be more elements in the queue than the length of the pattern");
            public bool ReachedEndOfPattern
                    return _isPatternPositionVisitedMarker[_lengthOfPattern] >= this.StringPosition;
            // non-virtual MoveNext is more performant
            // than implementing IEnumerable / virtual MoveNext
            public bool MoveNext(out int patternPosition)
                        _patternPositionsForFurtherProcessingCount >= 0,
                if (_patternPositionsForFurtherProcessingCount == 0)
                    patternPosition = -1;
                _patternPositionsForFurtherProcessingCount--;
                patternPosition = _patternPositionsForFurtherProcessing[_patternPositionsForFurtherProcessingCount];
        private abstract class PatternElement
            public abstract void ProcessStringCharacter(
                            char currentStringCharacter,
                            int currentPatternPosition,
                            PatternPositionsVisitor patternPositionsForCurrentStringPosition,
                            PatternPositionsVisitor patternPositionsForNextStringPosition);
            public abstract void ProcessEndOfString(
                            PatternPositionsVisitor patternPositionsForEndOfStringPosition);
        private class QuestionMarkElement : PatternElement
            public override void ProcessStringCharacter(
                            PatternPositionsVisitor patternPositionsForNextStringPosition)
                // '?' : (patternPosition, stringPosition) => (patternPosition + 1, stringPosition + 1)
                patternPositionsForNextStringPosition.Add(currentPatternPosition + 1);
            public override void ProcessEndOfString(
                            PatternPositionsVisitor patternPositionsForEndOfStringPosition)
                // '?' : (patternPosition, endOfString) => <no transitions out of this state - cannot move beyond end of string>
        private sealed class LiteralCharacterElement : QuestionMarkElement
            private readonly char _literalCharacter;
            public LiteralCharacterElement(char literalCharacter)
                _literalCharacter = literalCharacter;
                if (_literalCharacter == currentStringCharacter)
                    base.ProcessStringCharacter(
                            currentPatternPosition,
        private sealed class BracketExpressionElement : QuestionMarkElement
            private readonly Regex _regex;
            public BracketExpressionElement(Regex regex)
                Dbg.Assert(regex != null, "Caller should verify regex != null");
                _regex = regex;
                if (_regex.IsMatch(new string(currentStringCharacter, 1)))
                    base.ProcessStringCharacter(currentStringCharacter, currentPatternPosition,
        private sealed class AsterixElement : PatternElement
                // '*' : (patternPosition, stringPosition) => (patternPosition + 1, stringPosition)
                patternPositionsForCurrentStringPosition.Add(currentPatternPosition + 1);
                // '*' : (patternPosition, stringPosition) => (patternPosition, stringPosition + 1)
                patternPositionsForNextStringPosition.Add(currentPatternPosition);
                // '*' : (patternPosition, endOfString) => (patternPosition + 1, endOfString)
                patternPositionsForEndOfStringPosition.Add(currentPatternPosition + 1);
        private sealed class MyWildcardPatternParser : WildcardPatternParser
            private readonly List<PatternElement> _patternElements = new List<PatternElement>();
            private CharacterNormalizer _characterNormalizer;
            private StringBuilder _bracketExpressionBuilder;
            public static PatternElement[] Parse(
                            WildcardPattern pattern,
                            CharacterNormalizer characterNormalizer)
                var parser = new MyWildcardPatternParser
                    _characterNormalizer = characterNormalizer,
                    _regexOptions = WildcardPatternToRegexParser.TranslateWildcardOptionsIntoRegexOptions(pattern.Options),
                WildcardPatternParser.Parse(pattern, parser);
                return parser._patternElements.ToArray();
                c = _characterNormalizer.Normalize(c);
                _patternElements.Add(new LiteralCharacterElement(c));
                _patternElements.Add(new AsterixElement());
                _patternElements.Add(new QuestionMarkElement());
                _bracketExpressionBuilder = new StringBuilder();
                _bracketExpressionBuilder.Append('[');
                WildcardPatternToRegexParser.AppendLiteralCharacterToBracketExpression(
                    _bracketExpressionBuilder,
                    c);
                WildcardPatternToRegexParser.AppendCharacterRangeToBracketExpression(
                    startOfCharacterRange,
                    endOfCharacterRange);
                _bracketExpressionBuilder.Append(']');
                Regex regex = ParserOps.NewRegex(_bracketExpressionBuilder.ToString(), _regexOptions);
                _patternElements.Add(new BracketExpressionElement(regex));
        private readonly struct CharacterNormalizer
            private readonly CultureInfo _cultureInfo;
            private readonly bool _caseInsensitive;
            public CharacterNormalizer(WildcardOptions options)
                _caseInsensitive = (options & WildcardOptions.IgnoreCase) != 0;
                if (_caseInsensitive)
                    _cultureInfo = (options & WildcardOptions.CultureInvariant) != 0
                        ? CultureInfo.InvariantCulture
                        : CultureInfo.CurrentCulture;
                    // Don't bother saving the culture if we won't use it
            [Pure]
            public char Normalize(char x)
                    return _cultureInfo.TextInfo.ToLower(x);
    /// Translates a <see cref="WildcardPattern"/> into a DOS wildcard.
    internal class WildcardPatternToDosWildcardParser : WildcardPatternParser
        private readonly StringBuilder _result = new StringBuilder();
            _result.Append('*');
            _result.Append('?');
        /// Converts <paramref name="wildcardPattern"/> into a DOS wildcard.
        internal static string Parse(WildcardPattern wildcardPattern)
            var parser = new WildcardPatternToDosWildcardParser();
