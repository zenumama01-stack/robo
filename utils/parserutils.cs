    #region Flow Control Exceptions
    /// FlowControlException, base class for flow control exceptions.
    public abstract class FlowControlException : SystemException
        internal FlowControlException() { }
    /// LoopFlowException, base class for loop control exceptions.
    public abstract class LoopFlowException : FlowControlException
        internal LoopFlowException(string label)
            this.Label = label ?? string.Empty;
        internal LoopFlowException() { }
        /// Label, indicates nested loop level affected by exception.
        /// No label means most nested loop is affected.
        public string Label
        internal bool MatchLabel(string loopLabel)
            return MatchLoopLabel(Label, loopLabel);
        internal static bool MatchLoopLabel(string flowLabel, string loopLabel)
            // If the flow statement has no label, it always matches (because it just means, break or continue from
            // the most nested loop.)  Otherwise, compare the labels.
            return string.IsNullOrEmpty(flowLabel) || flowLabel.Equals(loopLabel, StringComparison.OrdinalIgnoreCase);
    /// Flow control BreakException.
    public sealed class BreakException : LoopFlowException
        [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "This exception should only be thrown from SMA.dll")]
        internal BreakException(string label)
            : base(label)
        internal BreakException() { }
        internal BreakException(string label, Exception innerException)
    /// Flow control ContinueException.
    public sealed class ContinueException : LoopFlowException
        internal ContinueException(string label)
        internal ContinueException() { }
        internal ContinueException(string label, Exception innerException)
    internal class ReturnException : FlowControlException
        internal ReturnException(object argument)
            this.Argument = argument;
        internal object Argument { get; set; }
    /// Implements the exit keyword.
    public class ExitException : FlowControlException
        internal ExitException(object argument)
        /// Argument.
        public object Argument { get; internal set; }
        internal ExitException() { }
    /// Used by InternalHost.ExitNestedPrompt() to pop out of an interpreter level...
    internal class ExitNestedPromptException : FlowControlException
    /// Used by the debugger to terminate the execution of the current command.
    public sealed class TerminateException : FlowControlException
    /// Used by Select-Object cmdlet to stop all the upstream cmdlets, but continue
    /// executing downstream cmdlets.  The semantics of stopping is intended to mimic
    /// a user pressing Ctrl-C [but which only affects upstream cmdlets].
    internal class StopUpstreamCommandsException : FlowControlException
        public StopUpstreamCommandsException(InternalCommand requestingCommand)
            this.RequestingCommandProcessor = requestingCommand.Context.CurrentCommandProcessor;
        public CommandProcessorBase RequestingCommandProcessor { get; }
    #endregion Flow Control Exceptions
    /// A enum corresponding to the options on the -split operator.
    public enum SplitOptions
        /// Use simple string comparison when evaluating the delimiter.
        /// Cannot be used with RegexMatch.
        SimpleMatch = 0x01,
        /// Use regular expression matching to evaluate the delimiter.
        /// This is the default behavior. Cannot be used with SimpleMatch.
        RegexMatch = 0x02,
        /// CultureInvariant: Ignores cultural differences in language when evaluating the delimiter.
        /// Valid only with RegexMatch.
        CultureInvariant = 0x04,
        /// Ignores unescaped whitespace and comments marked with #.
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Whitespace")]
        IgnorePatternWhitespace = 0x08,
        /// Regex multiline mode, which recognizes the start and end of lines,
        /// as well as the start and end of strings.
        /// Singleline is the default.
        Multiline = 0x10,
        /// Regex Singleline mode, which recognizes only the start and end of strings.
        Singleline = 0x20,
        /// Forces case-insensitive matching, even if -cSplit is specified.
        IgnoreCase = 0x40,
        /// Ignores non-named match groups, so that only explicit capture groups
        /// are returned in the result list.
        ExplicitCapture = 0x80,
    #region ParserOps
    internal delegate object PowerShellBinaryOperator(ExecutionContext context, IScriptExtent errorPosition, object lval, object rval);
    /// A static class holding various operations specific to the PowerShell interpreter such as
    /// various math operations, ToString() and a routine to extract the base object from an
    /// PSObject in a canonical fashion.
    internal static class ParserOps
        internal const string MethodNotFoundErrorId = "MethodNotFound";
        /// Construct the various caching structures used by the runtime routines...
        static ParserOps()
            // Cache for ints and chars to avoid overhead of boxing every time...
            for (int i = 0; i < (_MaxCache - _MinCache); i++)
                s_integerCache[i] = (object)(i + _MinCache);
            for (char ch = (char)0; ch < 255; ch++)
                s_chars[ch] = new string(ch, 1);
        private const int _MinCache = -100;
        private const int _MaxCache = 1000;
        private static readonly object[] s_integerCache = new object[_MaxCache - _MinCache];
        private static readonly string[] s_chars = new string[255];
        internal static readonly object _TrueObject = (object)true;
        internal static readonly object _FalseObject = (object)false;
        internal static string CharToString(char ch)
            if (ch < 255) return s_chars[ch];
            return new string(ch, 1);
        internal static object BoolToObject(bool value)
            return value ? _TrueObject : _FalseObject;
        /// Convert an object into an int, avoiding boxing small integers...
        /// <param name="value">The int to convert.</param>
        /// <returns>The reference equivalent.</returns>
        internal static object IntToObject(int value)
            if (value < _MaxCache && value >= _MinCache)
                return s_integerCache[value - _MinCache];
            return (object)value;
        internal static PSObject WrappedNumber(object data, string text)
            PSObject wrapped = new PSObject(data);
            wrapped.TokenText = text;
            return wrapped;
        /// A helper routine that turns the argument object into an
        /// integer. It handles PSObject and conversions.
        /// <param name="errorPosition">The position to use for error reporting.</param>
        /// <exception cref="RuntimeException">The result could not be represented as an integer.</exception>
        internal static int FixNum(object obj, IScriptExtent errorPosition)
            if (obj is int)
                return (int)obj;
            int result = ConvertTo<int>(obj, errorPosition);
        /// This is a helper function for converting an object to a particular type.
        /// It will throw exception with information about token representing the object.
        internal static T ConvertTo<T>(object obj, IScriptExtent errorPosition)
                result = (T)LanguagePrimitives.ConvertTo(obj, typeof(T), CultureInfo.InvariantCulture);
            catch (PSInvalidCastException mice)
                RuntimeException re = new RuntimeException(mice.Message, mice);
                re.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errorPosition));
                throw re;
        /// Private method used to call the op_* operations for the math operators.
        /// <param name="lval">Left operand.</param>
        /// <param name="rval">Right operand.</param>
        /// <param name="op">Name of the operation method to perform.</param>
        /// <param name="errorOp">The string to use in error messages representing the op.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="RuntimeException">An error occurred performing the operation, see inner exception.</exception>
        internal static object ImplicitOp(object lval, object rval, string op, IScriptExtent errorPosition, string errorOp)
            // Get the base object. At somepoint, we may allow users to dynamically extend
            // the implicit operators at which point, we'll need to change this to find the
            // extension method...
            lval = PSObject.Base(lval);
            rval = PSObject.Base(rval);
            Type lvalType = lval?.GetType();
            Type rvalType = rval?.GetType();
            Type opType;
            if (lvalType == null || (lvalType.IsPrimitive))
                // Prefer the LHS type when looking for the operator, but attempt the right
                // the lhs can't have an operator.
                // This logic is overly simplified and doesn't match other languages which
                // would look for overloads in both types, but this logic covers the most common
                opType = (rvalType == null || rvalType.IsPrimitive) ? null : rvalType;
                opType = lvalType;
            if (opType == null)
                throw InterpreterError.NewInterpreterException(lval, typeof(RuntimeException), errorPosition,
                    "NotADefinedOperationForType", ParserStrings.NotADefinedOperationForType,
                    lvalType == null ? "$null" : lvalType.FullName,
                    errorOp,
                    rvalType == null ? "$null" : rvalType.FullName);
            // None of the explicit conversions worked so try and invoke a method instead...
            object[] parms = new object[2];
            parms[0] = lval;
            parms[1] = rval;
            return CallMethod(
                errorPosition,
                opType,
                op, /* methodName */
                null, /* invocationConstraints */
                parms,
                AutomationNull.Value);
        private enum SplitImplOptions
            TrimContent = 0x01,
        private static object[] unfoldTuple(ExecutionContext context, IScriptExtent errorPosition, object tuple)
            List<object> result = new List<object>();
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(tuple);
                while (ParserOps.MoveNext(context, errorPosition, enumerator))
                    object element = ParserOps.Current(errorPosition, enumerator);
                // Not a tuple at all, just a single item. Treat it
                // as a 1-tuple.
                result.Add(tuple);
        // uses "yield" from C# 2.0, which automatically creates
        // an enumerable out of the loop code. See
        // https://msdn.microsoft.com/msdnmag/issues/04/05/C20/ for
        private static IEnumerable<string> enumerateContent(ExecutionContext context, IScriptExtent errorPosition, SplitImplOptions implOptions, object tuple)
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(tuple) ?? new object[] { tuple }.GetEnumerator();
                string strValue = PSObject.ToStringParser(context, enumerator.Current);
                if ((implOptions & SplitImplOptions.TrimContent) != 0)
                    strValue = strValue.Trim();
                yield return strValue;
        private static RegexOptions parseRegexOptions(SplitOptions options)
            RegexOptions result = RegexOptions.None;
            if ((options & SplitOptions.CultureInvariant) != 0)
                result |= RegexOptions.CultureInvariant;
            if ((options & SplitOptions.IgnorePatternWhitespace) != 0)
                result |= RegexOptions.IgnorePatternWhitespace;
            if ((options & SplitOptions.Multiline) != 0)
                result |= RegexOptions.Multiline;
            if ((options & SplitOptions.Singleline) != 0)
                result |= RegexOptions.Singleline;
            if ((options & SplitOptions.IgnoreCase) != 0)
                result |= RegexOptions.IgnoreCase;
            if ((options & SplitOptions.ExplicitCapture) != 0)
                result |= RegexOptions.ExplicitCapture;
        internal static object UnarySplitOperator(ExecutionContext context, IScriptExtent errorPosition, object lval)
            // unary split does a little extra processing to make
            // whitespace processing more convenient. Specifically,
            // it will ignore leading/trailing whitespace.
            return SplitOperatorImpl(context, errorPosition, lval, new object[] { @"\s+" }, SplitImplOptions.TrimContent, false);
        internal static object SplitOperator(ExecutionContext context, IScriptExtent errorPosition, object lval, object rval, bool ignoreCase)
            return SplitOperatorImpl(context, errorPosition, lval, rval, SplitImplOptions.None, ignoreCase);
        private static IReadOnlyList<string> SplitOperatorImpl(ExecutionContext context, IScriptExtent errorPosition, object lval, object rval, SplitImplOptions implOptions, bool ignoreCase)
            IEnumerable<string> content = enumerateContent(context, errorPosition, implOptions, lval);
            ScriptBlock predicate = null;
            string separatorPattern = null;
            int limit = 0;
            SplitOptions options = 0;
            object[] args = unfoldTuple(context, errorPosition, rval);
            if (args.Length >= 1)
                predicate = args[0] as ScriptBlock;
                    separatorPattern = PSObject.ToStringParser(context, args[0]);
                // The first argument to split is always required.
                throw InterpreterError.NewInterpreterException(rval, typeof(RuntimeException), errorPosition,
                    "BadOperatorArgument", ParserStrings.BadOperatorArgument, "-split", rval);
            if (args.Length >= 2)
                limit = FixNum(args[1], errorPosition);
            if (args.Length >= 3 && args[2] != null)
                string args2asString = args[2] as string;
                if (args2asString == null || !string.IsNullOrEmpty(args2asString))
                    options = ConvertTo<SplitOptions>(args[2], errorPosition);
                    if (predicate != null)
                        throw InterpreterError.NewInterpreterException(null, typeof(ParseException),
                            errorPosition, "InvalidSplitOptionWithPredicate", ParserStrings.InvalidSplitOptionWithPredicate);
                    if (ignoreCase && (options & SplitOptions.IgnoreCase) == 0)
                        options |= SplitOptions.IgnoreCase;
            else if (ignoreCase)
                return SplitWithPattern(context, errorPosition, content, separatorPattern, limit, options);
            else if (limit >= 0)
                return SplitWithPredicate(context, errorPosition, content, predicate, limit);
                return NegativeSplitWithPredicate(context, errorPosition, content, predicate, limit);
        private static IReadOnlyList<string> NegativeSplitWithPredicate(ExecutionContext context, IScriptExtent errorPosition, IEnumerable<string> content, ScriptBlock predicate, int limit)
            var results = new List<string>();
            if (limit == -1)
                // If the user just wants 1 string
                // then just return the content
                return new List<string>(content);
            foreach (string item in content)
                var split = new List<string>();
                // Used to traverse through the item
                int cursor = item.Length - 1;
                int subStringLength = 0;
                for (int charCount = 0; charCount < item.Length; charCount++)
                    // Evaluate the predicate using the character at cursor.
                    object predicateResult = predicate.DoInvokeReturnAsIs(
                        dollarUnder: CharToString(item[cursor]),
                        args: new object[] { item, cursor });
                    if (!LanguagePrimitives.IsTrue(predicateResult))
                        subStringLength++;
                        cursor -= 1;
                    split.Add(item.Substring(cursor + 1, subStringLength));
                    subStringLength = 0;
                    if (System.Math.Abs(limit) == (split.Count + 1))
                if (cursor == -1)
                    // Used when the limit is negative
                    // and the cursor was allowed to go
                    // all the way to the start of the
                    // string.
                    split.Add(item.Substring(0, subStringLength));
                    // Used to get the rest of the string
                    // when using a negative limit and
                    // the cursor doesn't reach the end
                    // of the string.
                    split.Add(item.Substring(0, cursor + 1));
                split.Reverse();
                results.AddRange(split);
        private static IReadOnlyList<string> SplitWithPredicate(ExecutionContext context, IScriptExtent errorPosition, IEnumerable<string> content, ScriptBlock predicate, int limit)
            if (limit == 1)
                // This is used to calculate how much to split from item.
                    // If the current character is not a delimiter
                    // then it must be included into a substring.
                        cursor += 1;
                    // Else, if the character is a delimiter
                    // then add a substring to the split list.
                    split.Add(item.Substring(cursor - subStringLength, subStringLength));
                    if (limit == (split.Count + 1))
                if (cursor == item.Length)
                    // when the limit is not negative and
                    // the cursor is allowed to make it to
                    // the end of the string.
                    // the cursor is not at the end of the
                    split.Add(item.Substring(cursor, item.Length - cursor));
        private static IReadOnlyList<string> SplitWithPattern(ExecutionContext context, IScriptExtent errorPosition, IEnumerable<string> content, string separatorPattern, int limit, SplitOptions options)
            // Default to Regex matching if no match specified.
            if ((options & SplitOptions.SimpleMatch) == 0 &&
                (options & SplitOptions.RegexMatch) == 0)
                options |= SplitOptions.RegexMatch;
            if ((options & SplitOptions.SimpleMatch) != 0)
                if ((options & ~(SplitOptions.SimpleMatch | SplitOptions.IgnoreCase)) != 0)
                        errorPosition, "InvalidSplitOptionCombination", ParserStrings.InvalidSplitOptionCombination);
                separatorPattern = Regex.Escape(separatorPattern);
            RegexOptions regexOptions = parseRegexOptions(options);
            int calculatedLimit = limit;
            // If the limit is negative then set Regex to read from right to left
            if (calculatedLimit < 0)
                regexOptions |= RegexOptions.RightToLeft;
                calculatedLimit *= -1;
            Regex regex = NewRegex(separatorPattern, regexOptions);
                string[] split = regex.Split(item, calculatedLimit);
        /// Implementation of the PowerShell unary -join operator...
        /// <param name="context">The execution context to use.</param>
        /// <returns>The result of the operator.</returns>
        internal static object UnaryJoinOperator(ExecutionContext context, IScriptExtent errorPosition, object lval)
            return JoinOperator(context, errorPosition, lval, string.Empty);
        /// Implementation of the PowerShell binary -join operator.
        internal static object JoinOperator(ExecutionContext context, IScriptExtent errorPosition, object lval, object rval)
            string separator = PSObject.ToStringParser(context, rval);
            // PSObject already has join functionality; just expose it
            // as an operator.
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(lval);
                return PSObject.ToStringEnumerable(context, enumerable, separator, null, null);
                return PSObject.ToStringParser(context, lval);
        /// The implementation of the PowerShell range operator.
        /// <param name="lval">The object on which to start.</param>
        /// <param name="rval">The object on which to stop.</param>
        /// <returns>The array of objects.</returns>
        internal static object RangeOperator(object lval, object rval)
            var lbase = PSObject.Base(lval);
            var rbase = PSObject.Base(rval);
            // If both arguments is [char] type or [string] type with length==1
            // return objects of [char] type.
            // In special case "0".."9" return objects of [int] type.
            if (AsChar(lbase) is char lc && AsChar(rbase) is char rc)
                return CharOps.Range(lc, rc);
            // As a last resort, the range operator tries to return objects of [int] type.
            //    1..10
            //    "1".."10"
            //    [int]"1"..[int]"10"
            var l = Convert.ToInt32(lbase);
            var r = Convert.ToInt32(rbase);
            return IntOps.Range(l, r);
        /// The implementation of an enumerator for the PowerShell range operator.
        internal static IEnumerator GetRangeEnumerator(object lval, object rval)
                return new CharRangeEnumerator(lc, rc);
            return new RangeEnumerator(l, r);
        // Help function for Range operator.
        // In common case:
        //      for [char] type
        //      for [string] type and length == 1
        // return objects of [char] type:
        //     [char]'A'..[char]'Z'
        //     [char]'A'..'Z'
        //     [char]'A'.."Z"
        //     'A'..[char]'Z'
        //     "A"..[char]'Z'
        //     "A".."Z"
        //     [char]"A"..[string]"Z"
        //     "A"..[char]"Z"
        //     [string]"A".."Z"
        // and so on.
        // In special case:
        //     "0".."9"
        // return objects of [int] type.
        private static object AsChar(object obj)
            if (obj is char) return obj;
            if (obj is string str && str.Length == 1 && !char.IsDigit(str, 0)) return str[0];
        /// The implementation of the PowerShell -replace operator....
        /// <param name="context">The execution context in which to evaluate the expression.</param>
        /// <param name="lval">The object on which to replace the values.</param>
        /// <param name="rval">The replacement description.</param>
        /// <param name="ignoreCase">True for -ireplace/-replace, false for -creplace.</param>
        internal static object ReplaceOperator(ExecutionContext context, IScriptExtent errorPosition, object lval, object rval, bool ignoreCase)
            object pattern = string.Empty;
            object substitute = string.Empty;
            IList rList = rval as IList;
            if (rList != null)
                if (rList.Count > 2)
                    // only allow 1 or 2 arguments to -replace
                        "BadReplaceArgument", ParserStrings.BadReplaceArgument, errorPosition.Text, rList.Count);
                if (rList.Count > 0)
                    pattern = rList[0];
                    if (rList.Count > 1)
                        substitute = PSObject.Base(rList[1]);
                pattern = rval;
            RegexOptions rreOptions = RegexOptions.None;
                rreOptions = RegexOptions.IgnoreCase;
            Regex rr = pattern as Regex;
            if (rr == null)
                    rr = NewRegex((string)PSObject.ToStringParser(context, pattern), rreOptions);
                    throw InterpreterError.NewInterpreterExceptionWithInnerException(pattern, typeof(RuntimeException),
                        null, "InvalidRegularExpression", ParserStrings.InvalidRegularExpression, ae, pattern);
            var replacer = ReplaceOperatorImpl.Create(context, rr, substitute);
            IEnumerator list = LanguagePrimitives.GetEnumerator(lval);
                string lvalString = PSObject.ToStringParser(context, lval) ?? string.Empty;
                return replacer.Replace(lvalString);
                while (ParserOps.MoveNext(context, errorPosition, list))
                    string lvalString = PSObject.ToStringParser(context, ParserOps.Current(errorPosition, list));
                    resultList.Add(replacer.Replace(lvalString));
                return resultList.ToArray();
        private struct ReplaceOperatorImpl
            public static ReplaceOperatorImpl Create(ExecutionContext context, Regex regex, object substitute)
                return new ReplaceOperatorImpl(context, regex, substitute);
            private readonly string _cachedReplacementString;
            private readonly MatchEvaluator _cachedMatchEvaluator;
            private ReplaceOperatorImpl(
                Regex regex,
                object substitute)
                _cachedReplacementString = null;
                _cachedMatchEvaluator = null;
                switch (substitute)
                    case string replacement:
                        _cachedReplacementString = replacement;
                    case ScriptBlock sb:
                        _cachedMatchEvaluator = GetMatchEvaluator(context, sb);
                    case object val when LanguagePrimitives.TryConvertTo(val, out _cachedMatchEvaluator):
                        _cachedReplacementString = PSObject.ToStringParser(context, substitute);
            // every time 'ReplaceOperatorImpl' is invoked.
            private static MatchEvaluator GetMatchEvaluator(ExecutionContext context, ScriptBlock sb)
                return match =>
                    var result = sb.DoInvokeReturnAsIs(
                        useLocalScope: false, /* Use current scope to be consistent with 'ForEach/Where-Object {}' and 'collection.ForEach{}/Where{}' */
                        dollarUnder: match,
                    return PSObject.ToStringParser(context, result);
            /// ReplaceOperator implementation.
            /// Abstracts away conversion of the optional substitute parameter to either a string or a MatchEvaluator delegate
            /// and finally returns the result of the final Regex.Replace operation.
            public object Replace(string input)
                if (_cachedReplacementString is not null)
                    return _regex.Replace(input, _cachedReplacementString);
                Dbg.Assert(_cachedMatchEvaluator is not null, "_cachedMatchEvaluator should be not null when code reach here.");
                return _regex.Replace(input, _cachedMatchEvaluator);
        /// Implementation of the PowerShell type operators...
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        internal static object IsOperator(ExecutionContext context, IScriptExtent errorPosition, object left, object right)
            object lval = PSObject.Base(left);
            object rval = PSObject.Base(right);
            Type rType = rval as Type;
            if (rType == null)
                rType = ConvertTo<Type>(rval, errorPosition);
                    // "the right operand of '-is' must be a type"
                    throw InterpreterError.NewInterpreterException(rval, typeof(RuntimeException),
                        errorPosition, "IsOperatorRequiresType", ParserStrings.IsOperatorRequiresType);
            if (rType == typeof(PSCustomObject) && lval is PSObject)
                Diagnostics.Assert(rType.IsInstanceOfType(((PSObject)lval).ImmediateBaseObject), "Unexpect PSObject");
                return _TrueObject;
            if (rType.Equals(typeof(PSObject)) && left is PSObject)
            return BoolToObject(rType.IsInstanceOfType(lval));
        internal static object IsNotOperator(ExecutionContext context, IScriptExtent errorPosition, object left, object right)
                return _FalseObject;
            return BoolToObject(!rType.IsInstanceOfType(lval));
        /// Implementation of the PowerShell -like operator.
        /// <param name="operator">The operator.</param>
        internal static object LikeOperator(ExecutionContext context, IScriptExtent errorPosition, object lval, object rval, TokenKind @operator)
            var wcp = rval as WildcardPattern;
            if (wcp == null)
                var ignoreCase = @operator == TokenKind.Ilike || @operator == TokenKind.Inotlike;
                wcp = WildcardPattern.Get(PSObject.ToStringParser(context, rval),
            bool notLike = @operator == TokenKind.Inotlike || @operator == TokenKind.Cnotlike;
                string lvalString = lval == null ? string.Empty : PSObject.ToStringParser(context, lval);
                return BoolToObject(wcp.IsMatch(lvalString) ^ notLike);
                object val = ParserOps.Current(errorPosition, list);
                string lvalString = val == null ? string.Empty : PSObject.ToStringParser(context, val);
                if (wcp.IsMatch(lvalString) ^ notLike)
                    resultList.Add(lvalString);
        /// Implementation of the PowerShell -match operator.
        /// <param name="ignoreCase">Ignore case?</param>
        /// <param name="notMatch">True for -notmatch, false for -match.</param>
        internal static object MatchOperator(ExecutionContext context, IScriptExtent errorPosition, object lval, object rval, bool notMatch, bool ignoreCase)
            RegexOptions reOptions = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            // if passed an explicit regex, just use it
            // otherwise compile the expression.
            // In this situation, creation of Regex should not fail. We are not
            // processing ArgumentException in this case.
            Regex r = PSObject.Base(rval) as Regex
                ?? NewRegex(PSObject.ToStringParser(context, rval), reOptions);
                // Find a match in the string.
                Match m = r.Match(lvalString);
                if (m.Success)
                    GroupCollection groups = m.Groups;
                        Hashtable h = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                        foreach (string groupName in r.GetGroupNames())
                            Group g = groups[groupName];
                            if (g.Success)
                                int keyInt;
                                if (Int32.TryParse(groupName, out keyInt))
                                    h.Add(keyInt, g.ToString());
                                    h.Add(groupName, g.ToString());
                        context.SetVariable(SpecialVariables.MatchesVarPath, h);
                return BoolToObject(m.Success ^ notMatch);
                int check = 0;
                        // Find a single match in the string.
                        if (m.Success ^ notMatch)
                            resultList.Add(val);
                        if (check++ > 1000)
                            // Check to see if we're stopping every one in a while...
                            if (context != null && context.CurrentPipelineStopping)
                            check = 0;
                catch (ScriptCallDepthException)
                    throw InterpreterError.NewInterpreterExceptionWithInnerException(list, typeof(RuntimeException),
                        errorPosition, "BadEnumeration", ParserStrings.BadEnumeration, e, e.Message);
        // Implementation of the -contains/-in operators and case insensitive variants.
        internal static bool ContainsOperatorCompiled(ExecutionContext context,
                                                      CallSite<Func<CallSite, object, IEnumerator>> getEnumeratorSite,
                                                      CallSite<Func<CallSite, object, object, object>> comparerSite,
                                                      object left,
                                                      object right)
            IEnumerator list = getEnumeratorSite.Target.Invoke(getEnumeratorSite, left);
            if (list == null || list is EnumerableOps.NonEnumerableObjectEnumerator)
                return (bool)comparerSite.Target.Invoke(comparerSite, left, right);
            while (EnumerableOps.MoveNext(context, list))
                object val = EnumerableOps.Current(list);
                if ((bool)comparerSite.Target.Invoke(comparerSite, val, right))
        /// Implementation of the PowerShell -contains/-notcontains operators (and case sensitive variants)
        /// <param name="contains">True for -contains, false for -notcontains.</param>
        internal static object ContainsOperator(ExecutionContext context, IScriptExtent errorPosition, object left, object right, bool contains, bool ignoreCase)
            IEnumerator list = LanguagePrimitives.GetEnumerator(left);
                    BoolToObject(contains ==
                                 LanguagePrimitives.Equals(left, right, ignoreCase, CultureInfo.InvariantCulture));
                if (LanguagePrimitives.Equals(val, right, ignoreCase, CultureInfo.InvariantCulture))
                    return BoolToObject(contains);
            return BoolToObject(!contains);
        internal delegate bool CompareDelegate(object lhs, object rhs, bool ignoreCase);
        internal static object CompareOperators(ExecutionContext context, IScriptExtent errorPosition, object left, object right, CompareDelegate compareDelegate, bool ignoreCase)
                return BoolToObject(compareDelegate(left, right, ignoreCase));
                if (compareDelegate(val, right, ignoreCase))
        /// Cache regular expressions.
        /// <param name="patternString">The string to find the pattern for.</param>
        /// <param name="options">The options used to create the regex.</param>
        /// <returns>New or cached Regex.</returns>
        internal static Regex NewRegex(string patternString, RegexOptions options)
            var subordinateRegexCache = s_regexCache.GetOrAdd(options, s_subordinateRegexCacheCreationDelegate);
            if (subordinateRegexCache.TryGetValue(patternString, out Regex result))
                if (subordinateRegexCache.Count > MaxRegexCache)
                    // TODO: it would be useful to get a notice (in telemetry?) if the cache is full.
                    subordinateRegexCache.Clear();
                var regex = new Regex(patternString, options);
                return subordinateRegexCache.GetOrAdd(patternString, regex);
        private static readonly ConcurrentDictionary<RegexOptions, ConcurrentDictionary<string, Regex>> s_regexCache =
            new ConcurrentDictionary<RegexOptions, ConcurrentDictionary<string, Regex>>();
        private static readonly Func<RegexOptions, ConcurrentDictionary<string, Regex>> s_subordinateRegexCacheCreationDelegate =
            key => new ConcurrentDictionary<string, Regex>(StringComparer.Ordinal);
        private const int MaxRegexCache = 1000;
        /// A routine used to advance an enumerator and catch errors that might occur
        /// performing the operation.
        /// <param name="context">The execution context used to see if the pipeline is stopping.</param>
        /// <param name="enumerator">THe enumerator to advance.</param>
        /// <exception cref="RuntimeException">An error occurred moving to the next element in the enumeration.</exception>
        /// <returns>True if the move succeeded.</returns>
        internal static bool MoveNext(ExecutionContext context, IScriptExtent errorPosition, IEnumerator enumerator)
                // Check to see if we're stopping...
                return enumerator.MoveNext();
                throw InterpreterError.NewInterpreterExceptionWithInnerException(enumerator, typeof(RuntimeException),
        /// Wrapper caller for enumerator.MoveNext - handles and republishes errors...
        /// <param name="enumerator">The enumerator to read from.</param>
        internal static object Current(IScriptExtent errorPosition, IEnumerator enumerator)
        /// Retrieves the obj's type full name.
        /// <param name="obj">The object we want to retrieve the type's full name from.</param>
        /// <returns>The obj's type full name.</returns>
        internal static string GetTypeFullName(object obj)
                return obj.GetType().FullName;
            if (mshObj.InternalTypeNames.Count == 0)
                return typeof(PSObject).FullName;
            return mshObj.InternalTypeNames[0];
        /// Launch a method on an object. This will handle .NET native methods, COM
        /// methods and ScriptBlock notes. Native methods currently take precedence over notes...
        /// <param name="target">The object to call the method on. It shouldn't be a PSObject.</param>
        /// <param name="methodName">The name of the method to call.</param>
        /// <param name="paramArray">The arguments to pass to the method.</param>
        /// <param name="callStatic">Set to true if you want to call a static method.</param>
        /// <param name="valueToSet">If not automation null, then this must be a settable property.</param>
        /// <exception cref="RuntimeException">Wraps the exception returned from the method call.</exception>
        /// <exception cref="FlowControlException">Internal exception from a flow control statement.</exception>
        internal static object CallMethod(
            IScriptExtent errorPosition,
            object[] paramArray,
            bool callStatic,
            object valueToSet)
            Dbg.Assert(methodName != null, "methodName was null");
            PSMethodInfo targetMethod = null;
            object targetBase = null;
            PSObject targetAsPSObject = null;
                if (LanguagePrimitives.IsNull(target))
                    // "you can't call a method on null"
                    throw InterpreterError.NewInterpreterException(methodName, typeof(RuntimeException), errorPosition, "InvokeMethodOnNull", ParserStrings.InvokeMethodOnNull);
                targetBase = PSObject.Base(target);
                targetAsPSObject = PSObject.AsPSObject(target);
                Type targetType;
                if (callStatic)
                    targetType = (Type)targetBase;
                    targetType = targetBase.GetType();
                    targetMethod = PSObject.GetStaticCLRMember(target, methodName) as PSMethod;
                    targetMethod = targetAsPSObject.Members[methodName] as PSMethodInfo;
                if (targetMethod == null)
                    string typeFullName = null;
                        typeFullName = targetType.FullName;
                        typeFullName = GetTypeFullName(target);
                    if (valueToSet == AutomationNull.Value)
                        // "[{0}] doesn't contain a method named '{1}'"
                        throw InterpreterError.NewInterpreterException(methodName, typeof(RuntimeException), errorPosition,
                            MethodNotFoundErrorId, ParserStrings.MethodNotFound, typeFullName, methodName);
                            "ParameterizedPropertyAssignmentFailed", ParserStrings.ParameterizedPropertyAssignmentFailed, typeFullName, methodName);
                // If there is a property to set, then this is a multi-parameter property assignment
                // not really a method call.
                if (valueToSet != AutomationNull.Value)
                    if (targetMethod is not PSParameterizedProperty propertyToSet)
                                                                       "ParameterizedPropertyAssignmentFailed", ParserStrings.ParameterizedPropertyAssignmentFailed, GetTypeFullName(target), methodName);
                    propertyToSet.InvokeSet(valueToSet, paramArray);
                    return valueToSet;
                    PSMethod adaptedMethod = targetMethod as PSMethod;
                    if (adaptedMethod != null)
                        return adaptedMethod.Invoke(invocationConstraints, paramArray);
                        return targetMethod.Invoke(paramArray);
            catch (MethodInvocationException mie)
                if (mie.ErrorRecord.InvocationInfo == null)
                    mie.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errorPosition));
                if (rte.ErrorRecord.InvocationInfo == null)
                    rte.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errorPosition));
                // Note - we are catching all methods thrown from a method call and wrap them
                // unless they are already RuntimeException. This is ok.
                throw InterpreterError.NewInterpreterExceptionByMessage(typeof(RuntimeException), errorPosition,
                    e.Message, "MethodInvocationException", e);
    #endregion ParserOps
    #region RangeEnumerator
    /// This is a simple enumerator class that just enumerates of a range of numbers. It's used in enumerating
    /// elements when the range operator .. is used.
    internal class RangeEnumerator : IEnumerator
        private readonly int _lowerBound;
        internal int LowerBound
            get { return _lowerBound; }
        private readonly int _upperBound;
        internal int UpperBound
            get { return _upperBound; }
        private int _current;
            get { return Current; }
        public virtual int Current
            get { return _current; }
        internal int CurrentValue
        private readonly int _increment = 1;
        private bool _firstElement = true;
        public RangeEnumerator(int lowerBound, int upperBound)
            _lowerBound = lowerBound;
            _current = _lowerBound;
            _upperBound = upperBound;
                _increment = -1;
            _firstElement = true;
            if (_firstElement)
                _firstElement = false;
            if (_current == _upperBound)
            _current += _increment;
    /// The simple enumerator class is used for the range operator '..'
    /// in expressions like 'A'..'B' | ForEach-Object { $_ }
    internal class CharRangeEnumerator : IEnumerator
        public CharRangeEnumerator(char lowerBound, char upperBound)
            LowerBound = lowerBound;
            Current = lowerBound;
            UpperBound = upperBound;
        internal char LowerBound { get; }
        internal char UpperBound { get; }
        public char Current
            if (Current == UpperBound)
            Current = (char)(Current + _increment);
            Current = LowerBound;
    #endregion RangeEnumerator
    #region InterpreterError
    internal static class InterpreterError
        /// Create a new instance of an interpreter exception.
        /// <param name="targetObject">The target object for this exception.</param>
        /// <param name="exceptionType">Type of exception to build.</param>
        /// <param name="resourceIdAndErrorId">
        /// ResourceID to look up template message, and also ErrorID
        /// Resource string that holds the error message
        /// <param name="args">Insertion parameters to message.</param>
        /// <returns>A new instance of the specified exception type.</returns>
        internal static RuntimeException NewInterpreterException(object targetObject,
            Type exceptionType, IScriptExtent errorPosition, string resourceIdAndErrorId, string resourceString, params object[] args)
            return NewInterpreterExceptionWithInnerException(targetObject, exceptionType, errorPosition, resourceIdAndErrorId, resourceString, null, args);
        /// <param name="targetObject">The object associated with the problem.</param>
        /// Resource string which holds the error message
        /// <param name="innerException">Inner exception.</param>
        /// <returns>New instance of an interpreter exception.</returns>
        internal static RuntimeException NewInterpreterExceptionWithInnerException(object targetObject,
            Type exceptionType, IScriptExtent errorPosition, string resourceIdAndErrorId, string resourceString, Exception innerException, params object[] args)
            // errToken may be null
            if (string.IsNullOrEmpty(resourceIdAndErrorId))
                throw PSTraceSource.NewArgumentException(nameof(resourceIdAndErrorId));
            // innerException may be null
            // args may be null or empty
            RuntimeException rte = null;
                    Dbg.Assert(false,
                        "Could not load text for parser exception '"
                        + resourceIdAndErrorId + "'");
                    rte = NewBackupInterpreterException(exceptionType, errorPosition, resourceIdAndErrorId, null);
                    rte = NewInterpreterExceptionByMessage(exceptionType, errorPosition, message, resourceIdAndErrorId, innerException);
                    + resourceIdAndErrorId
                    + "' due to InvalidOperationException " + e.Message);
                rte = NewBackupInterpreterException(exceptionType, errorPosition, resourceIdAndErrorId, e);
            catch (System.Resources.MissingManifestResourceException e)
                    + "' due to MissingManifestResourceException " + e.Message);
                    + "' due to FormatException " + e.Message);
            rte.SetTargetObject(targetObject);
            return rte;
        /// <param name="message">Message.</param>
        /// <param name="errorId">ErrorID.</param>
        /// <returns>New instance of ParseException.</returns>
        internal static RuntimeException NewInterpreterExceptionByMessage(
            Type exceptionType, IScriptExtent errorPosition, string message, string errorId, Exception innerException)
            // only assert -- be permissive at runtime
            Dbg.Assert(!string.IsNullOrEmpty(message), "message was null or empty");
            Dbg.Assert(!string.IsNullOrEmpty(errorId), "errorId was null or empty");
            RuntimeException e;
            // Create an instance of the right exception type...
            if (exceptionType == typeof(ParseException))
                e = new ParseException(message, errorId, innerException);
            else if (exceptionType == typeof(IncompleteParseException))
                e = new IncompleteParseException(message, errorId, innerException);
                e = new RuntimeException(message, innerException);
                e.SetErrorId(errorId);
                e.SetErrorCategory(ErrorCategory.InvalidOperation);
            // Don't trash the existing InvocationInfo.
            if (errorPosition != null)
                e.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errorPosition));
        private static RuntimeException NewBackupInterpreterException(
            Type exceptionType,
            if (innerException == null)
                // there is no reason this string lookup should fail
                message = StringUtil.Format(ParserStrings.BackupParserMessage, errorId);
                message = StringUtil.Format(ParserStrings.BackupParserMessageWithException, errorId, innerException.Message);
            return NewInterpreterExceptionByMessage(exceptionType, errorPosition, message, errorId, innerException);
        internal static void UpdateExceptionErrorRecordPosition(Exception exception, IScriptExtent extent)
            if (extent == null || extent == PositionUtilities.EmptyExtent)
            var icer = exception as IContainsErrorRecord;
            if (icer != null)
                var errorRecord = icer.ErrorRecord;
                var invocationInfo = errorRecord.InvocationInfo;
                    errorRecord.SetInvocationInfo(new InvocationInfo(null, extent));
                else if (invocationInfo.ScriptPosition == null || invocationInfo.ScriptPosition == PositionUtilities.EmptyExtent)
                    invocationInfo.ScriptPosition = extent;
                    errorRecord.LockScriptStackTrace();
        internal static void UpdateExceptionErrorRecordHistoryId(RuntimeException exception, ExecutionContext context)
            InvocationInfo invInfo = exception.ErrorRecord.InvocationInfo;
            if (invInfo is not { HistoryId: -1 })
            if (context?.CurrentCommandProcessor is null)
            invInfo.HistoryId = context.CurrentCommandProcessor.Command.MyInvocation.HistoryId;
    #endregion InterpreterError
    #region ScriptTrace
    internal static class ScriptTrace
        internal static void Trace(int level, string messageId, string resourceString, params object[] args)
            // Need access to the execution context to see if we should trace. If we
            // can't get it, then just return...
            Trace(context, level, messageId, resourceString, args);
        internal static void Trace(ExecutionContext context, int level, string messageId, string resourceString, params object[] args)
            if (context.PSDebugTraceLevel > level)
                    Dbg.Assert(false, message);
                ((InternalHostUserInterface)context.EngineHostInterface.UI).WriteDebugLine(message, ref pref);
    #endregion ScriptTrace
