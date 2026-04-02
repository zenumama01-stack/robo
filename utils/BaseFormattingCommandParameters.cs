    #region Formatting Command Line Parameters
    /// It holds the command line parameter values
    /// It unifies the data structures across the various
    /// formatting command (e.g. table, wide and list)
    internal sealed class FormattingCommandLineParameters
        /// MshParameter collection, as specified by metadata
        /// the list can be empty of no data is specified.
        internal List<MshParameter> mshParameterList = new List<MshParameter>();
        /// Name of the group by property, it can be null.
        internal MshParameter groupByParameter = null;
        /// Name of a view from format.ps1xml, it can be null.
        internal string viewName = null;
        /// Flag to force a shape even on out of band objects.
        internal bool forceFormattingAlsoOnOutOfBand = false;
        /// Autosize formatting flag. If true, the output command is instructed
        /// to get the "best fit" for the device screen.
        internal bool? autosize = null;
        /// If true, the header for a table is repeated after each screen full
        /// of content.
        internal bool repeatHeader = false;
        /// Errors are shown as out of band messages.
        /// Errors are shown in the formatted output.
        /// Expand IEnumerable flag.
        /// Extension mechanism for shape specific parameters.
        internal ShapeSpecificParameters shapeParameters = null;
        /// Filter for excluding properties from formatting.
        internal PSPropertyExpressionFilter excludePropertyFilter = null;
    /// Class to derive from to pass shepe specific data.
    internal abstract class ShapeSpecificParameters
    internal sealed class TableSpecificParameters : ShapeSpecificParameters
        internal bool? hideHeaders = null;
        internal bool? multiLine = null;
    internal sealed class WideSpecificParameters : ShapeSpecificParameters
        internal int? columns = null;
    internal sealed class ComplexSpecificParameters : ShapeSpecificParameters
        /// Options for class info display on objects.
        internal enum ClassInfoDisplay { none, fullName, shortName }
        internal ClassInfoDisplay classDisplay = ClassInfoDisplay.shortName;
        internal const int maxDepthAllowable = 5;
        /// Max depth of recursion on sub objects.
        internal int maxDepth = maxDepthAllowable;
    #region MshParameter metadata
    /// Specialized class for the "expression" property.
    internal class ExpressionEntryDefinition : HashtableEntryDefinition
        internal ExpressionEntryDefinition() : this(false)
        internal ExpressionEntryDefinition(bool noGlobbing) : base(FormatParameterDefinitionKeys.ExpressionEntryKey,
                                    new Type[] { typeof(string), typeof(ScriptBlock) }, true)
            _noGlobbing = noGlobbing;
        internal override Hashtable CreateHashtableFromSingleType(object val)
            Hashtable hash = new Hashtable();
            hash.Add(FormatParameterDefinitionKeys.ExpressionEntryKey, val);
#if false
        internal override Hashtable CreateHashtableFromSingleType (object val)
            Hashtable hash = new Hashtable ();
            // a simple type was specified, it could be a ScriptBlock
            if (val is ScriptBlock)
                hash.Add (FormatParameterDefinitionKeys.NameEntryKey, val);
            // it could be a string
            // build a hash with "hotrodded" entries if there
            string s = val as string;
            object width, align;
            string nameVal = UnpackString (s, out width, out align);
            hash.Add (FormatParameterDefinitionKeys.NameEntryKey, nameVal);
                hash.Add (FormatParameterDefinitionKeys.WidthEntryKey, width);
            if (align != null)
                hash.Add (FormatParameterDefinitionKeys.AlignmentEntryKey, align);
        internal override object Verify(object val,
                                        TerminatingErrorContext invocationContext,
                                        bool originalParameterWasHashTable)
            if (val == null)
                throw PSTraceSource.NewArgumentNullException(nameof(val));
            // need to check the type:
            // it can be a string or a script block
            if (val is ScriptBlock sb)
                PSPropertyExpression ex = new PSPropertyExpression(sb);
            if (val is string s)
                    ProcessEmptyStringError(originalParameterWasHashTable, invocationContext);
                PSPropertyExpression ex = new PSPropertyExpression(s);
                if (_noGlobbing)
                    if (ex.HasWildCardCharacters)
                        ProcessGlobbingCharactersError(originalParameterWasHashTable, s, invocationContext);
            PSTraceSource.NewArgumentException(nameof(val));
        #region Error Processing
        private void ProcessEmptyStringError(bool originalParameterWasHashTable,
                                                TerminatingErrorContext invocationContext)
            string errorID;
            if (originalParameterWasHashTable)
                msg = StringUtil.Format(FormatAndOut_MshParameter.MshExEmptyStringHashError,
                    this.KeyName);
                errorID = "ExpressionEmptyString1";
                msg = StringUtil.Format(FormatAndOut_MshParameter.MshExEmptyStringError);
                errorID = "ExpressionEmptyString2";
            ParameterProcessor.ThrowParameterBindingException(invocationContext, errorID, msg);
        private void ProcessGlobbingCharactersError(bool originalParameterWasHashTable, string expression, TerminatingErrorContext invocationContext)
                msg = StringUtil.Format(FormatAndOut_MshParameter.MshExGlobbingHashError,
                    this.KeyName, expression);
                errorID = "ExpressionGlobbing1";
                msg = StringUtil.Format(FormatAndOut_MshParameter.MshExGlobbingStringError,
                    expression);
                errorID = "ExpressionGlobbing2";
        private readonly bool _noGlobbing;
    internal class AlignmentEntryDefinition : HashtableEntryDefinition
        internal AlignmentEntryDefinition() : base(FormatParameterDefinitionKeys.AlignmentEntryKey,
                                    new Type[] { typeof(string) })
            if (!originalParameterWasHashTable)
                // this should never happen
            // it is a string, need to check for partial match in a case insensitive way
            // and normalize
            if (!string.IsNullOrEmpty(s))
                for (int k = 0; k < s_legalValues.Length; k++)
                    if (CommandParameterDefinition.FindPartialMatch(s, s_legalValues[k]))
                        if (k == 0)
                            return TextAlignment.Left;
                        if (k == 1)
                            return TextAlignment.Center;
                        return TextAlignment.Right;
            // nothing found, we have an illegal value
            ProcessIllegalValue(s, invocationContext);
        private void ProcessIllegalValue(string s, TerminatingErrorContext invocationContext)
            string msg = StringUtil.Format(FormatAndOut_MshParameter.IllegalAlignmentValueError,
                s,
                this.KeyName,
                ParameterProcessor.CatenateStringArray(s_legalValues)
            ParameterProcessor.ThrowParameterBindingException(invocationContext, "AlignmentIllegalValue", msg);
        private static readonly string[] s_legalValues = new string[] { LeftAlign, CenterAlign, RightAlign };
        private const string LeftAlign = "left";
        private const string CenterAlign = "center";
        private const string RightAlign = "right";
    internal class WidthEntryDefinition : HashtableEntryDefinition
        internal WidthEntryDefinition() : base(FormatParameterDefinitionKeys.WidthEntryKey,
                                    new Type[] { typeof(int) })
            // it's an int, just check range, no need to change it
            VerifyRange((int)val, invocationContext);
        private void VerifyRange(int width, TerminatingErrorContext invocationContext)
            if (width <= 0)
                string msg = StringUtil.Format(FormatAndOut_MshParameter.OutOfRangeWidthValueError,
                    width,
                    this.KeyName
                ParameterProcessor.ThrowParameterBindingException(invocationContext, "WidthOutOfRange", msg);
    internal class LabelEntryDefinition : HashtableEntryDefinition
        internal LabelEntryDefinition() : base(FormatParameterDefinitionKeys.LabelEntryKey, new string[] { NameEntryDefinition.NameEntryKey }, new Type[] { typeof(string) }, false)
    internal class FormatStringDefinition : HashtableEntryDefinition
        internal FormatStringDefinition() : base(FormatParameterDefinitionKeys.FormatStringEntryKey,
                string msg = StringUtil.Format(FormatAndOut_MshParameter.EmptyFormatStringValueError,
                ParameterProcessor.ThrowParameterBindingException(invocationContext, "FormatStringEmpty", msg);
            // we expect a string and we build a field formatting directive
            FieldFormattingDirective directive = new FieldFormattingDirective();
            directive.formatString = s;
            return directive;
    internal class BooleanEntryDefinition : HashtableEntryDefinition
        internal BooleanEntryDefinition(string entryKey) : base(entryKey, null)
            return LanguagePrimitives.IsTrue(val);
    internal static class FormatParameterDefinitionKeys
        // common entries
        internal const string ExpressionEntryKey = "expression";
        internal const string FormatStringEntryKey = "formatString";
        // specific to format-table
        // specific to format-table,list and wide
        // specific to format-wide
        // NONE
        // specific to format-custom (no format string for it, just the name)
        internal const string DepthEntryKey = "depth";
    internal class FormatGroupByParameterDefinition : CommandParameterDefinition
            this.hashEntries.Add(new FormatStringDefinition());
    internal class FormatParameterDefinitionBase : CommandParameterDefinition
    internal class FormatTableParameterDefinition : FormatParameterDefinitionBase
            base.SetEntries();
            this.hashEntries.Add(new WidthEntryDefinition());
            this.hashEntries.Add(new AlignmentEntryDefinition());
    internal class FormatListParameterDefinition : FormatParameterDefinitionBase
    internal class FormatWideParameterDefinition : FormatParameterDefinitionBase
        // no additional entries
    internal class FormatObjectParameterDefinition : CommandParameterDefinition
            this.hashEntries.Add(new HashtableEntryDefinition(FormatParameterDefinitionKeys.DepthEntryKey, new Type[] { typeof(int) }));
