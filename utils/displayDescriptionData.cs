// this file contains the data structures for the in memory database
// containing display and formatting information
    internal enum EnumerableExpansion
        /// Process core only, ignore IEumerable.
        CoreOnly,
        /// Process IEnumerable, ignore core.
        EnumOnly,
        /// Process both core and IEnumerable, core first.
        Both,
    #region Type Info Database
    internal sealed partial class TypeInfoDataBase
        // define the sections corresponding the XML file
        internal DefaultSettingsSection defaultSettingsSection = new DefaultSettingsSection();
        internal TypeGroupsSection typeGroupSection = new TypeGroupsSection();
        internal ViewDefinitionsSection viewDefinitionsSection = new ViewDefinitionsSection();
        internal FormatControlDefinitionHolder formatControlDefinitionHolder = new FormatControlDefinitionHolder();
        /// Cache for resource strings in format.ps1xml.
        internal DisplayResourceManagerCache displayResourceManagerCache = new DisplayResourceManagerCache();
    internal sealed class DatabaseLoadingInfo
        internal string fileDirectory = null;
        internal bool isFullyTrusted = false;
        internal bool isProductCode = false;
        internal DateTime loadTime = DateTime.Now;
    #region Default Settings
#if _LATER
    internal class SettableOnceValue<T>
        SettableOnceValue (T defaultValue)
            this._default = defaultValue;
        internal void f(T x)
            Nullable<T> y = x;
            this._value = y;
            // this._value = (Nullable<T>)x;
        internal T Value
                if (_value == null)
                    this._value = value;
                if (_value != null)
                    return this._value.Value;
                return _default;
        private Nullable<T> _value;
        private T _default;
    internal sealed class DefaultSettingsSection
        internal bool MultilineTables
                if (_multilineTables.HasValue)
                    return _multilineTables.Value;
                if (!_multilineTables.HasValue)
                    _multilineTables = value;
        private bool? _multilineTables;
        internal FormatErrorPolicy formatErrorPolicy = new FormatErrorPolicy();
        internal ShapeSelectionDirectives shapeSelectionDirectives = new ShapeSelectionDirectives();
        internal List<EnumerableExpansionDirective> enumerableExpansionDirectiveList = new List<EnumerableExpansionDirective>();
    internal sealed class FormatErrorPolicy
        /// If true, display error messages.
        internal bool ShowErrorsAsMessages
                if (_showErrorsAsMessages.HasValue)
                    return _showErrorsAsMessages.Value;
                if (!_showErrorsAsMessages.HasValue)
                    _showErrorsAsMessages = value;
        private bool? _showErrorsAsMessages;
        /// If true, display an error string in the formatted display
        /// (e.g. cell in a table)
        internal bool ShowErrorsInFormattedOutput
                if (_showErrorsInFormattedOutput.HasValue)
                    return _showErrorsInFormattedOutput.Value;
                if (!_showErrorsInFormattedOutput.HasValue)
                    _showErrorsInFormattedOutput = value;
        private bool? _showErrorsInFormattedOutput;
        /// String to display in the formatted display (e.g. cell in a table)
        /// when the evaluation of a PSPropertyExpression fails.
        internal string errorStringInFormattedOutput = "#ERR";
        /// when a format operation on a value fails.
        internal string formatErrorStringInFormattedOutput = "#FMTERR";
    internal sealed class ShapeSelectionDirectives
        internal int PropertyCountForTable
                if (_propertyCountForTable.HasValue)
                    return _propertyCountForTable.Value;
                return 4;
                if (!_propertyCountForTable.HasValue)
                    _propertyCountForTable = value;
        private int? _propertyCountForTable;
        internal List<FormatShapeSelectionOnType> formatShapeSelectionOnTypeList = new List<FormatShapeSelectionOnType>();
    internal enum FormatShape { Table, List, Wide, Complex, Undefined }
    internal abstract class FormatShapeSelectionBase
        internal FormatShape formatShape = FormatShape.Undefined;
    internal sealed class FormatShapeSelectionOnType : FormatShapeSelectionBase
        internal AppliesTo appliesTo;
    internal sealed class EnumerableExpansionDirective
        internal EnumerableExpansion enumerableExpansion = EnumerableExpansion.EnumOnly;
    #region Type Groups Definitions
    internal sealed class TypeGroupsSection
        internal List<TypeGroupDefinition> typeGroupDefinitionList = new List<TypeGroupDefinition>();
    internal sealed class TypeGroupDefinition
        internal string name;
        internal List<TypeReference> typeReferenceList = new List<TypeReference>();
    internal abstract class TypeOrGroupReference
        /// Optional expression for conditional binding.
        internal ExpressionToken conditionToken = null;
    internal sealed class TypeReference : TypeOrGroupReference
    internal sealed class TypeGroupReference : TypeOrGroupReference
    #region Elementary Tokens
    internal abstract class FormatToken
    internal sealed class TextToken : FormatToken
        internal string text;
        internal StringResourceReference resource;
    internal sealed class NewLineToken : FormatToken
        internal int count = 1;
    internal sealed class FrameToken : FormatToken
        /// Item associated with this frame definition.
        internal ComplexControlItemDefinition itemDefinition = new ComplexControlItemDefinition();
        /// Frame info associated with this frame definition.
        internal FrameInfoDefinition frameInfoDefinition = new FrameInfoDefinition();
    internal sealed class FrameInfoDefinition
        /// Left indentation for a frame is relative to the parent frame.
        /// it must be a value >=0.
        internal int leftIndentation = 0;
        /// Right indentation for a frame is relative to the parent frame.
        internal int rightIndentation = 0;
        /// It can have the following values:
        /// 0 : ignore
        /// greater than 0 : it represents the indentation for the first line (i.e. "first line indent").
        ///                  The first line will be indented by the indicated number of characters.
        /// less than 0    : it represents the hanging of the first line WRT the following ones
        ///                  (i.e. "first line hanging").
        internal int firstLine = 0;
    internal sealed class ExpressionToken
        internal ExpressionToken() { }
        internal ExpressionToken(string expressionValue, bool isScriptBlock)
            this.expressionValue = expressionValue;
            this.isScriptBlock = isScriptBlock;
        internal bool isScriptBlock;
        internal string expressionValue;
    internal abstract class PropertyTokenBase : FormatToken
        internal ExpressionToken expression = new ExpressionToken();
        internal bool enumerateCollection = false;
    internal sealed class CompoundPropertyToken : PropertyTokenBase
        /// An inline control or a reference to a control definition.
        internal ControlBase control = null;
    internal sealed class FieldPropertyToken : PropertyTokenBase
        internal FieldFormattingDirective fieldFormattingDirective = new FieldFormattingDirective();
    internal sealed class FieldFormattingDirective
        internal string formatString = null; // optional
        internal bool isTable = false;
    #endregion Elementary Tokens
    #region Control Definitions: common data
    /// Root class for all the control types.
    internal abstract class ControlBase
        internal static string GetControlShapeName(ControlBase control)
            if (control is TableControlBody)
                return nameof(FormatShape.Table);
            if (control is ListControlBody)
                return nameof(FormatShape.List);
            if (control is WideControlBody)
                return nameof(FormatShape.Wide);
            if (control is ComplexControlBody)
                return nameof(FormatShape.Complex);
        /// Returns a Shallow Copy of the current object.
        internal virtual ControlBase Copy()
            System.Management.Automation.Diagnostics.Assert(false,
                "This should never be called directly on the base. Let the derived class implement this method.");
    /// Reference to a control.
    internal sealed class ControlReference : ControlBase
        /// Name of the control we refer to, it cannot be null.
        internal string name = null;
        /// Type of the control we refer to, it cannot be null.
        internal Type controlType = null;
    /// Base class for all control definitions
    /// NOTE: this is an extensibility point, if a new control
    /// needs to be created, it has to be derived from this class.
    internal abstract class ControlBody : ControlBase
        /// RULE: valid only for table and wide only.
        /// RULE: only valid for table.
    /// Class to hold a definition of a control.
    internal sealed class ControlDefinition
        /// Name of the control we define, it cannot be null.
        /// Body of the control we define, it cannot be null.
        internal ControlBody controlBody = null;
    #region View Definitions: common data
    internal sealed class ViewDefinitionsSection
        internal List<ViewDefinition> viewDefinitionList = new List<ViewDefinition>();
    internal sealed partial class AppliesTo
        // it can contain either a type or type group reference
        internal List<TypeOrGroupReference> referenceList = new List<TypeOrGroupReference>();
    internal sealed class GroupBy
        internal StartGroup startGroup = new StartGroup();
        // NOTE: extension point for describing:
        // * end group statistics
        // * end group footer
        // This can be done with defining a new Type called EndGroup with fields
        // such as stat and footer.
    internal sealed class StartGroup
        /// Expression to be used to select the grouping.
        internal ExpressionToken expression = null;
        /// Alternative (and simplified) representation for the control
        /// RULE: if the control object is null, use this one.
        internal TextToken labelTextToken = null;
    /// Container for control definitions.
    internal sealed class FormatControlDefinitionHolder
        /// List of control definitions.
        internal List<ControlDefinition> controlDefinitionList = new List<ControlDefinition>();
    /// Definition of a view.
    internal sealed class ViewDefinition
        internal DatabaseLoadingInfo loadingInfo;
        /// The name of this view. Must not be null.
        /// Applicability of the view. Mandatory.
        internal AppliesTo appliesTo = new AppliesTo();
        /// Optional grouping directive.
        internal GroupBy groupBy;
        /// Container for optional local formatting directives.
        /// Main control for the view (e.g. reference to a control or a control body.
        internal ControlBase mainControl;
        /// RULE: only valid for list and complex.
        internal bool outOfBand;
        /// Set if the view is for help output, used so we can prune the view from Get-FormatData
        /// because those views are too complicated and big for remoting.
        internal bool isHelpFormatter;
        internal Guid InstanceId { get; private set; }
        internal ViewDefinition()
            InstanceId = Guid.NewGuid();
    /// Base class for all the "shape"-Directive classes.
    internal abstract class FormatDirective
    #region Localized Resources
    internal sealed class StringResourceReference
        internal DatabaseLoadingInfo loadingInfo = null;
        internal string assemblyName = null;
        internal string assemblyLocation = null;
        internal string baseName = null;
        internal string resourceId = null;
    /// Specifies additional type definitions for an object.
    public sealed class ExtendedTypeDefinition
        /// A format definition may apply to multiple types.  This api returns
        /// the first typename that this format definition applies to, but there
        /// may be other types that apply. <see cref="TypeNames"/> should be
        /// used instead.
            get { return TypeNames[0]; }
        /// The list of type names this set of format definitions applies to.
        public List<string> TypeNames { get; internal set; }
        /// The formatting view definition for
        /// the specified type.
        public List<FormatViewDefinition> FormatViewDefinition { get; internal set; }
        /// Overloaded to string method for
        /// better display.
            return TypeName;
        /// Constructor for the ExtendedTypeDefinition.
        /// <param name="viewDefinitions"></param>
        public ExtendedTypeDefinition(string typeName, IEnumerable<FormatViewDefinition> viewDefinitions) : this()
            if (viewDefinitions == null)
                throw PSTraceSource.NewArgumentNullException(nameof(viewDefinitions));
            TypeNames.Add(typeName);
            foreach (FormatViewDefinition definition in viewDefinitions)
                FormatViewDefinition.Add(definition);
        /// Initiate an instance of ExtendedTypeDefinition with the type name.
        public ExtendedTypeDefinition(string typeName) : this()
        internal ExtendedTypeDefinition()
            FormatViewDefinition = new List<FormatViewDefinition>();
            TypeNames = new List<string>();
    /// Defines a formatting view for a particular type.
    [DebuggerDisplay("{Name}")]
    public sealed class FormatViewDefinition
        /// <summary>Name of the formatting view as defined in the formatting file</summary>
        /// <summary>The control defined by this formatting view can be one of table, list, wide, or custom</summary>
        public PSControl Control { get; }
        /// <summary>instance id of the original view this will be used to distinguish two views with the same name and control types</summary>
        internal Guid InstanceId { get; set; }
        internal FormatViewDefinition(string name, PSControl control, Guid instanceid)
            Control = control;
            InstanceId = instanceid;
        /// <summary/>
        public FormatViewDefinition(string name, PSControl control)
    /// Defines a control for the formatting types defined by PowerShell.
    public abstract class PSControl
        /// Each control can group items and specify a header for the group.
        /// You can group by same property value, or result of evaluating a script block.
        public PSControlGroupBy GroupBy { get; set; }
        /// When the "shape" of formatting has been determined by previous objects,
        /// sometimes you want objects of different types to continue using that shape
        /// (table, list, or whatever) even if they specify their own views, and sometimes
        /// you want your view to take over. When OutOfBand is true, the view will apply
        /// regardless of previous objects that may have selected the shape.
        public bool OutOfBand { get; set; }
        internal abstract void WriteToXml(FormatXmlWriter writer);
        internal virtual bool SafeForExport()
            return GroupBy == null || GroupBy.IsSafeForExport();
        internal virtual bool CompatibleWithOldPowerShell()
            // This is too strict, the GroupBy would just be ignored by the remote
            // PowerShell, but that's still wrong.
            // OutOfBand is also ignored by old PowerShell, but it's of less importance.
            return GroupBy == null;
    /// Allows specifying a header for groups of related objects being formatted, can
    /// be specified on any type of PSControl.
    public sealed class PSControlGroupBy
        /// Specifies the property or expression (script block) that controls grouping.
        public DisplayEntry Expression { get; set; }
        /// Optional - used to specify a label for the header of a group.
        public string Label { get; set; }
        /// Optional - used to format the header of a group.
        public CustomControl CustomControl { get; set; }
        internal bool IsSafeForExport()
            return (Expression == null || Expression.SafeForExport()) &&
                   (CustomControl == null || CustomControl.SafeForExport());
        internal static PSControlGroupBy Get(GroupBy groupBy)
                // TODO - groupBy.startGroup.control
                var expressionToken = groupBy.startGroup.expression;
                return new PSControlGroupBy
                    Expression = new DisplayEntry(expressionToken),
                    Label = groupBy.startGroup.labelTextToken?.text
    /// One entry in a format display unit, script block or property name.
    public sealed class DisplayEntry
        /// <summary>Returns the type of this value</summary>
        public DisplayEntryValueType ValueType { get; internal set; }
        /// <summary>Returns the value as a string</summary>
        public string Value { get; internal set; }
        internal DisplayEntry() { }
        /// <summary>Public constructor for DisplayEntry</summary>
        public DisplayEntry(string value, DisplayEntryValueType type)
                if (value == null || type == DisplayEntryValueType.Property)
            Value = value;
            ValueType = type;
            return (ValueType == DisplayEntryValueType.Property ? "property: " : "script: ") + Value;
        internal DisplayEntry(ExpressionToken expression)
            Value = expression.expressionValue;
            ValueType = expression.isScriptBlock ? DisplayEntryValueType.ScriptBlock : DisplayEntryValueType.Property;
            if (string.IsNullOrEmpty(Value))
                if (Value == null || ValueType == DisplayEntryValueType.Property)
        internal bool SafeForExport()
            return ValueType != DisplayEntryValueType.ScriptBlock;
    /// Each control (table, list, wide, or custom) may have multiple entries. If there are multiple
    /// entries, there must be a default entry with no condition, all other entries must have EntrySelectedBy
    /// specified. This is useful when you need a single view for grouping or otherwise just selecting the
    /// shape of formatting, but need distinct formatting rules for each instance.  For example, when
    /// listing files, you may want to group based on the parent path, but select different entries
    /// depending on if the item is a file or directory.
    public sealed class EntrySelectedBy
        /// An optional list of typenames that apply to the entry.
        public List<string> TypeNames { get; set; }
        /// An optional condition that applies to the entry.
        public List<DisplayEntry> SelectionCondition { get; set; }
        internal static EntrySelectedBy Get(IEnumerable<string> entrySelectedByType, IEnumerable<DisplayEntry> entrySelectedByCondition)
            EntrySelectedBy result = null;
            if (entrySelectedByType != null || entrySelectedByCondition != null)
                result = new EntrySelectedBy();
                bool isEmpty = true;
                if (entrySelectedByType != null)
                    result.TypeNames = new List<string>(entrySelectedByType);
                    if (result.TypeNames.Count > 0)
                        isEmpty = false;
                if (entrySelectedByCondition != null)
                    result.SelectionCondition = new List<DisplayEntry>(entrySelectedByCondition);
                    if (result.SelectionCondition.Count > 0)
        internal static EntrySelectedBy Get(List<TypeOrGroupReference> references)
            if (references != null && references.Count > 0)
                foreach (TypeOrGroupReference tr in references)
                    if (tr.conditionToken != null)
                        result.SelectionCondition ??= new List<DisplayEntry>();
                        result.SelectionCondition.Add(new DisplayEntry(tr.conditionToken));
                    if (tr is TypeGroupReference)
                    result.TypeNames ??= new List<string>();
                    result.TypeNames.Add(tr.name);
            if (SelectionCondition == null)
            foreach (var cond in SelectionCondition)
                if (!cond.SafeForExport())
        internal bool CompatibleWithOldPowerShell()
            // Old versions of PowerShell know nothing about selection conditions.
            return SelectionCondition == null || SelectionCondition.Count == 0;
    /// Specifies possible alignment enumerations for display cells.
    public enum Alignment
        /// Not defined.
        /// Left of the cell, contents will trail with a ... if exceeded - ex "Display..."
        Left = 1,
        /// Center of the cell.
        Center = 2,
        /// Right of the cell, contents will lead with a ... if exceeded - ex "...456"
        Right = 3,
    /// Specifies the type of entry value.
    public enum DisplayEntryValueType
        /// The value is a property. Look for a property with the specified name.
        Property = 0,
        /// The value is a scriptblock. Evaluate the script block and fill the entry with the result.
        ScriptBlock = 1,
