    /// Class containing miscellaneous helpers to deal with
    /// PSObject manipulation.
    internal static class PSObjectHelper
        [TraceSource("PSObjectHelper", "PSObjectHelper")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("PSObjectHelper", "PSObjectHelper");
        internal const char Ellipsis = '\u2026';
        internal const string EllipsisStr = "\u2026";
        internal static string PSObjectIsOfExactType(Collection<string> typeNames)
        internal static bool PSObjectIsEnum(Collection<string> typeNames)
            if (typeNames.Count < 2 || string.IsNullOrEmpty(typeNames[1]))
            return string.Equals(typeNames[1], "System.Enum", StringComparison.Ordinal);
        /// Retrieve the display name. It looks for a well known property and,
        /// if not found, it uses some heuristics to get a "close" match.
        /// <param name="target">Shell object to process.</param>
        /// <param name="expressionFactory">Expression factory to create PSPropertyExpression.</param>
        /// <returns>Resolved PSPropertyExpression; null if no match was found.</returns>
        internal static PSPropertyExpression GetDisplayNameExpression(PSObject target, PSPropertyExpressionFactory expressionFactory)
            // first try to get the expression from the object (types.ps1xml data)
            PSPropertyExpression expressionFromObject = GetDefaultNameExpression(target);
            if (expressionFromObject != null)
                return expressionFromObject;
            // we failed the default display name, let's try some well known names
            // trying to get something potentially useful
            string[] knownPatterns = new string[] {
                "name", "id", "key", "*key", "*name", "*id",
            // go over the patterns, looking for the first match
            foreach (string pattern in knownPatterns)
                PSPropertyExpression ex = new PSPropertyExpression(pattern);
                List<PSPropertyExpression> exprList = ex.ResolveNames(target);
                while ((exprList.Count > 0) && (
                    exprList[0].ToString().Equals(RemotingConstants.ComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase) ||
                    exprList[0].ToString().Equals(RemotingConstants.ShowComputerNameNoteProperty, StringComparison.OrdinalIgnoreCase) ||
                    exprList[0].ToString().Equals(RemotingConstants.RunspaceIdNoteProperty, StringComparison.OrdinalIgnoreCase) ||
                    exprList[0].ToString().Equals(RemotingConstants.SourceJobInstanceId, StringComparison.OrdinalIgnoreCase)))
                    exprList.RemoveAt(0);
                if (exprList.Count == 0)
                // if more than one match, just return the first one
                return exprList[0];
            // we did not find anything
        /// It gets the display name value.
        /// <returns>PSPropertyExpressionResult if successful; null otherwise.</returns>
        internal static PSPropertyExpressionResult GetDisplayName(PSObject target, PSPropertyExpressionFactory expressionFactory)
            // get the expression to evaluate
            PSPropertyExpression ex = GetDisplayNameExpression(target, expressionFactory);
            // evaluate the expression
            List<PSPropertyExpressionResult> resList = ex.GetValues(target);
            if (resList.Count == 0 || resList[0].Exception != null)
                // no results or the retrieval on the first one failed
            // return something only if the first match was successful
            return resList[0];
        /// This is necessary only to consider IDictionaries as IEnumerables, since LanguagePrimitives.GetEnumerable does not.
        /// <param name="obj">Object to extract the IEnumerable from.</param>
        internal static IEnumerable GetEnumerable(object obj)
            if (obj is PSObject mshObj)
                obj = mshObj.BaseObject;
            if (obj is IDictionary)
                return (IEnumerable)obj;
            return LanguagePrimitives.GetEnumerable(obj);
        private static string GetSmartToStringDisplayName(object x, PSPropertyExpressionFactory expressionFactory)
            PSPropertyExpressionResult r = PSObjectHelper.GetDisplayName(PSObjectHelper.AsPSObject(x), expressionFactory);
            if ((r != null) && (r.Exception == null))
                return PSObjectHelper.AsPSObject(r.Result).ToString();
                return PSObjectHelper.AsPSObject(x).ToString();
        private static string GetObjectName(object x, PSPropertyExpressionFactory expressionFactory)
            string objName;
            // check if the underlying object is of primitive type
            // if so just return its value
            if (x is PSObject &&
                (LanguagePrimitives.IsBoolOrSwitchParameterType((((PSObject)x).BaseObject).GetType()) ||
                LanguagePrimitives.IsNumeric(((((PSObject)x).BaseObject).GetType()).GetTypeCode()) ||
                LanguagePrimitives.IsNull(x)))
                objName = x.ToString();
                // use PowerShell's $null variable to indicate that the value is null...
                objName = "$null";
                MethodInfo toStringMethod = x.GetType().GetMethod("ToString", Type.EmptyTypes);
                // TODO:CORECLR double check with CORE CLR that x.GetType() == toStringMethod.ReflectedType
                // Check if the given object "x" implements "toString" method. Do that by comparing "DeclaringType" which 'Gets the class that declares this member' and the object type
                if (toStringMethod.DeclaringType == x.GetType())
                    objName = PSObjectHelper.AsPSObject(x).ToString();
                        objName = PSObjectHelper.AsPSObject(r.Result).ToString();
                        if (objName == string.Empty)
                            var baseObj = PSObject.Base(x);
                            if (baseObj != null)
                                objName = baseObj.ToString();
            return objName;
        /// Helper to convert an PSObject into a string
        /// It takes into account enumerations (use display name)
        /// <param name="so">Shell object to process.</param>
        /// <param name="enumerationLimit">Limit on IEnumerable enumeration.</param>
        /// <param name="formatErrorObject">Stores errors during string conversion.</param>
        /// <param name="formatFloat">Determine if to format floating point numbers using current culture.</param>
        /// <returns>String representation.</returns>
        internal static string SmartToString(PSObject so, PSPropertyExpressionFactory expressionFactory, int enumerationLimit, StringFormatError formatErrorObject, bool formatFloat = false)
                    IEnumerator enumerator = e.GetEnumerator();
                        if (enumerator is IBlockingEnumerator<object> be)
                            while (be.MoveNext(false))
                                if (enumerationLimit >= 0)
                                    if (enumCount == enumerationLimit)
                                        sb.Append(Ellipsis);
                                if (!first)
                                    sb.Append(", ");
                                sb.Append(GetObjectName(be.Current, expressionFactory));
                                sb.Append(GetObjectName(x, expressionFactory));
                if (formatFloat && so.BaseObject is not null)
                    // format numbers using the current culture
                    if (so.BaseObject is double dbl)
                        return dbl.ToString("F");
                    else if (so.BaseObject is float f)
                        return f.ToString("F");
                    else if (so.BaseObject is decimal d)
                        return d.ToString("F");
                return so.ToString();
            catch (Exception e) when (e is ExtendedTypeSystemException || e is InvalidOperationException)
                // These exceptions are being caught and handled by returning an empty string when
                // the object cannot be stringified due to ETS or an instance in the collection has been modified
                s_tracer.TraceWarning($"SmartToString method: Exception during conversion to string, emitting empty string: {e.Message}");
                if (formatErrorObject != null)
                    formatErrorObject.sourceObject = so;
                    formatErrorObject.exception = e;
        private static readonly PSObject s_emptyPSObject = new PSObject(string.Empty);
        internal static PSObject AsPSObject(object obj)
            return (obj == null) ? s_emptyPSObject : PSObject.AsPSObject(obj);
        /// Format an object using a provided format string directive.
        /// <param name="directive">Format directive object to use.</param>
        /// <param name="val">Object to format.</param>
        /// <param name="formatErrorObject">Formatting error object, if present.</param>
        internal static string FormatField(FieldFormattingDirective directive, object val, int enumerationLimit,
            StringFormatError formatErrorObject, PSPropertyExpressionFactory expressionFactory)
            PSObject so = PSObjectHelper.AsPSObject(val);
            bool isTable = false;
            if (directive is not null)
                isTable = directive.isTable;
                if (!string.IsNullOrEmpty(directive.formatString))
                    // we have a formatting directive, apply it
                    // NOTE: with a format directive, we do not make any attempt
                    // to deal with IEnumerable
                        // use some heuristics to determine if we have "composite formatting"
                        // 2004/11/16-JonN This is heuristic but should be safe enough
                        if (directive.formatString.Contains("{0") || directive.formatString.Contains('}'))
                            // we do have it, just use it
                            return string.Format(CultureInfo.CurrentCulture, directive.formatString, so);
                        // we fall back to the PSObject's IFormattable.ToString()
                        // pass a null IFormatProvider
                        return so.ToString(directive.formatString, formatProvider: null);
                    catch (Exception e) // 2004/11/17-JonN This covers exceptions thrown in
                                        // string.Format and PSObject.ToString().
                                        // I think we can swallow these.
                        // NOTE: we catch all the exceptions, since we do not know
                        // what the underlying object access would throw
                        if (formatErrorObject is not null)
                            formatErrorObject.formatString = directive.formatString;
            // we do not have a formatting directive or we failed the formatting (fallback)
            // but we did not report as an error;
            // this call would deal with IEnumerable if the object implements it
            return PSObjectHelper.SmartToString(so, expressionFactory, enumerationLimit, formatErrorObject, isTable);
        private static PSMemberSet MaskDeserializedAndGetStandardMembers(PSObject so)
            Diagnostics.Assert(so != null, "Shell object to process cannot be null");
            Collection<string> typeNamesWithoutDeserializedPrefix = Deserializer.MaskDeserializationPrefix(typeNames);
            if (typeNamesWithoutDeserializedPrefix == null)
            TypeTable typeTable = so.GetTypeTable();
            if (typeTable == null)
            PSMemberInfoInternalCollection<PSMemberInfo> members =
                typeTable.GetMembers<PSMemberInfo>(new ConsolidatedString(typeNamesWithoutDeserializedPrefix));
            return members[TypeTable.PSStandardMembers] as PSMemberSet;
        private static List<PSPropertyExpression> GetDefaultPropertySet(PSMemberSet standardMembersSet)
            if (standardMembersSet != null && standardMembersSet.Members[TypeTable.DefaultDisplayPropertySet] is PSPropertySet defaultDisplayPropertySet)
                List<PSPropertyExpression> retVal = new List<PSPropertyExpression>();
                foreach (string prop in defaultDisplayPropertySet.ReferencedPropertyNames)
                    if (!string.IsNullOrEmpty(prop))
                        retVal.Add(new PSPropertyExpression(prop));
            return new List<PSPropertyExpression>();
        /// Helper to retrieve the default property set of a shell object.
        /// <returns>Resolved expression; empty list if not found.</returns>
        internal static List<PSPropertyExpression> GetDefaultPropertySet(PSObject so)
            List<PSPropertyExpression> retVal = GetDefaultPropertySet(so.PSStandardMembers);
            if (retVal.Count == 0)
                retVal = GetDefaultPropertySet(MaskDeserializedAndGetStandardMembers(so));
        private static PSPropertyExpression GetDefaultNameExpression(PSMemberSet standardMembersSet)
            if (standardMembersSet != null && standardMembersSet.Members[TypeTable.DefaultDisplayProperty] is PSNoteProperty defaultDisplayProperty)
                string expressionString = defaultDisplayProperty.Value.ToString();
                if (string.IsNullOrEmpty(expressionString))
                    // invalid data, the PSObject is empty
                    return new PSPropertyExpression(expressionString);
        private static PSPropertyExpression GetDefaultNameExpression(PSObject so)
            PSPropertyExpression retVal = GetDefaultNameExpression(so.PSStandardMembers) ??
                                   GetDefaultNameExpression(MaskDeserializedAndGetStandardMembers(so));
        /// Helper to retrieve the value of an PSPropertyExpression and to format it.
        /// <param name="ex">Expression to use for retrieval.</param>
        /// <param name="directive">Format directive to use for formatting.</param>
        /// <param name="formatErrorObject"></param>
        /// <param name="result">Not null if an error condition arose.</param>
        /// <returns>Formatted string.</returns>
        internal static string GetExpressionDisplayValue(
            PSPropertyExpression ex,
            FieldFormattingDirective directive,
            StringFormatError formatErrorObject,
            out PSPropertyExpressionResult result)
            List<PSPropertyExpressionResult> resList = ex.GetValues(so);
            result = resList[0];
            return PSObjectHelper.FormatField(directive, result.Result, enumerationLimit, formatErrorObject, expressionFactory);
        /// Queries PSObject and determines if ComputerName property should be shown.
        internal static bool ShouldShowComputerNameProperty(PSObject so)
                    PSPropertyInfo computerNameProperty = so.Properties[RemotingConstants.ComputerNameNoteProperty];
                    PSPropertyInfo showComputerNameProperty = so.Properties[RemotingConstants.ShowComputerNameNoteProperty];
                    // if computer name property exists then this must be a remote object. see
                    // if it can be displayed.
                    if ((computerNameProperty != null) && (showComputerNameProperty != null))
                        LanguagePrimitives.TryConvertTo<bool>(showComputerNameProperty.Value, out result);
                    // ignore any exceptions thrown retrieving the *ComputerName properties
                    // from the object
    internal abstract class FormattingError
        internal object sourceObject;
    internal sealed class PSPropertyExpressionError : FormattingError
        internal PSPropertyExpressionResult result;
    internal sealed class StringFormatError : FormattingError
        internal string formatString;
        internal Exception exception;
    internal delegate ScriptBlock CreateScriptBlockFromString(string scriptBlockString);
    /// Helper class to create PSPropertyExpression's from format.ps1xml data structures.
    internal sealed class PSPropertyExpressionFactory
        internal void VerifyScriptBlockText(string scriptText)
            ScriptBlock.Create(scriptText);
        /// Create an expression from an expression token.
        /// <param name="et">Expression token to use.</param>
        /// <returns>Constructed expression.</returns>
        internal PSPropertyExpression CreateFromExpressionToken(ExpressionToken et)
            return CreateFromExpressionToken(et, null);
        /// <param name="loadingInfo">The context from which the file was loaded.</param>
        internal PSPropertyExpression CreateFromExpressionToken(ExpressionToken et, DatabaseLoadingInfo loadingInfo)
            if (et.isScriptBlock)
                // we cache script blocks from expression tokens
                if (_expressionCache != null)
                    PSPropertyExpression value;
                    if (_expressionCache.TryGetValue(et, out value))
                        // got a hit on the cache, just return
                    _expressionCache = new Dictionary<ExpressionToken, PSPropertyExpression>();
                bool isProductCode = false;
                if (loadingInfo != null)
                    isFullyTrusted = loadingInfo.isFullyTrusted;
                    isProductCode = loadingInfo.isProductCode;
                // no hit, we build one and we cache
                ScriptBlock sb = ScriptBlock.CreateDelayParsedScriptBlock(et.expressionValue, isProductCode: isProductCode);
                sb.DebuggerStepThrough = true;
                if (isFullyTrusted)
                    sb.LanguageMode = PSLanguageMode.FullLanguage;
                _expressionCache.Add(et, ex);
            // we do not cache if it is just a property name
            return new PSPropertyExpression(et.expressionValue);
        private Dictionary<ExpressionToken, PSPropertyExpression> _expressionCache;
