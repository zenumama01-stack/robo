    #region Table View Definitions
    /// Alignment values
    /// NOTE: we do not use an enum because this will have to be
    /// serialized and ERS/serialization do not support enumerations.
    internal static class TextAlignment
        internal const int Undefined = 0;
        internal const int Left = 1;
        internal const int Center = 2;
        internal const int Right = 3;
    /// Definition of a table control.
    internal sealed class TableControlBody : ControlBody
        /// Optional, if not present, use data off the default table row definition.
        internal TableHeaderDefinition header = new TableHeaderDefinition();
        /// Default row definition
        internal TableRowDefinition defaultDefinition;
        /// Optional list of row definition overrides. It can be empty if there are no overrides.
        internal List<TableRowDefinition> optionalDefinitionList = new List<TableRowDefinition>();
            TableControlBody result = new TableControlBody
                autosize = this.autosize,
                header = this.header.Copy()
            if (defaultDefinition != null)
                result.defaultDefinition = this.defaultDefinition.Copy();
            foreach (TableRowDefinition trd in this.optionalDefinitionList)
                result.optionalDefinitionList.Add(trd);
    /// Information about the table header
    /// NOTE: if an instance of this class is present, the list must not be empty.
    internal sealed class TableHeaderDefinition
        /// If true, direct the outputter to suppress table header printing.
        internal bool hideHeader;
        /// Mandatory list of column header definitions.
        internal List<TableColumnHeaderDefinition> columnHeaderDefinitionList =
                            new List<TableColumnHeaderDefinition>();
        internal TableHeaderDefinition Copy()
            TableHeaderDefinition result = new TableHeaderDefinition { hideHeader = this.hideHeader };
            foreach (TableColumnHeaderDefinition tchd in this.columnHeaderDefinitionList)
                result.columnHeaderDefinitionList.Add(tchd);
    internal sealed class TableColumnHeaderDefinition
        /// mandatory row description.
        /// General alignment for the column
        /// If not present, either use the one from the row definition
        /// or the data driven heuristics.
        internal int alignment = TextAlignment.Undefined;
        /// Width of the column.
        internal int width = 0; // undefined
    /// Definition of the data to be displayed in a table row.
    internal sealed class TableRowDefinition
        /// If true, the current table row should be allowed
        /// to wrap to multiple lines, else truncated.
        internal bool multiLine;
        /// Mandatory list of column items.
        internal List<TableRowItemDefinition> rowItemDefinitionList = new List<TableRowItemDefinition>();
        internal TableRowDefinition Copy()
            TableRowDefinition result = new TableRowDefinition
                appliesTo = this.appliesTo,
                multiLine = this.multiLine
            foreach (TableRowItemDefinition trid in this.rowItemDefinitionList)
                result.rowItemDefinitionList.Add(trid);
    internal sealed class TableRowItemDefinition
        /// Optional alignment to override the default one at the header level.
    /// Defines a table control.
    public sealed class TableControl : PSControl
        /// <summary>Collection of column header definitions for this table control</summary>
        public List<TableControlColumnHeader> Headers { get; set; }
        /// <summary>Collection of row definitions for this table control</summary>
        public List<TableControlRow> Rows { get; set; }
        /// <summary>When true, column widths are calculated based on more than the first object.</summary>
        public bool AutoSize { get; set; }
        /// <summary>When true, table headers are not displayed</summary>
        public bool HideTableHeaders { get; set; }
        /// <summary>Create a default TableControl</summary>
        public static TableControlBuilder Create(bool outOfBand = false, bool autoSize = false, bool hideTableHeaders = false)
            var table = new TableControl { OutOfBand = outOfBand, AutoSize = autoSize, HideTableHeaders = hideTableHeaders };
            return new TableControlBuilder(table);
        /// <summary>Public default constructor for TableControl</summary>
        public TableControl()
            Headers = new List<TableControlColumnHeader>();
            Rows = new List<TableControlRow>();
            writer.WriteTableControl(this);
        /// Determines if this object is safe to be written.
        /// <returns>True if safe, false otherwise.</returns>
            foreach (var row in Rows)
                if (!row.SafeForExport())
                if (!row.CompatibleWithOldPowerShell())
        internal TableControl(TableControlBody tcb, ViewDefinition viewDefinition) : this()
            this.AutoSize = tcb.autosize.GetValueOrDefault();
            this.HideTableHeaders = tcb.header.hideHeader;
            TableControlRow row = new TableControlRow(tcb.defaultDefinition);
            Rows.Add(row);
            foreach (TableRowDefinition rd in tcb.optionalDefinitionList)
                row = new TableControlRow(rd);
            foreach (TableColumnHeaderDefinition hd in tcb.header.columnHeaderDefinitionList)
                TableControlColumnHeader header = new TableControlColumnHeader(hd);
                Headers.Add(header);
        /// Public constructor for TableControl that only takes 'tableControlRows'.
        /// <param name="tableControlRow"></param>
        public TableControl(TableControlRow tableControlRow) : this()
            if (tableControlRow == null)
                throw PSTraceSource.NewArgumentNullException("tableControlRows");
            this.Rows.Add(tableControlRow);
        /// Public constructor for TableControl that takes both 'tableControlRows' and 'tableControlColumnHeaders'.
        /// <param name="tableControlColumnHeaders"></param>
        public TableControl(TableControlRow tableControlRow, IEnumerable<TableControlColumnHeader> tableControlColumnHeaders) : this()
            if (tableControlColumnHeaders == null)
                throw PSTraceSource.NewArgumentNullException(nameof(tableControlColumnHeaders));
            foreach (TableControlColumnHeader header in tableControlColumnHeaders)
                this.Headers.Add(header);
    /// Defines the header for a particular column in a table control.
    public sealed class TableControlColumnHeader
        /// <summary>Label for the column</summary>
        /// <summary>Alignment of the string within the column</summary>
        public Alignment Alignment { get; set; }
        /// <summary>Width of the column - in number of display cells</summary>
        public int Width { get; set; }
        internal TableControlColumnHeader(TableColumnHeaderDefinition colheaderdefinition)
            if (colheaderdefinition.label != null)
                Label = colheaderdefinition.label.text;
            Alignment = (Alignment)colheaderdefinition.alignment;
            Width = colheaderdefinition.width;
        /// <summary>Default constructor</summary>
        public TableControlColumnHeader()
        /// Public constructor for TableControlColumnHeader.
        /// <param name="label">Could be null if no label to specify.</param>
        /// <param name="width">The Value should be non-negative.</param>
        /// <param name="alignment">The default value is Alignment.Undefined.</param>
        public TableControlColumnHeader(string label, int width, Alignment alignment)
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(width), width);
            this.Width = width;
            this.Alignment = alignment;
    /// Defines a particular column within a row
    /// in a table control.
    public sealed class TableControlColumn
        /// <summary>Alignment of the particular column</summary>
        /// <summary>Display Entry</summary>
        public DisplayEntry DisplayEntry { get; set; }
        /// Returns the value of the entry.
            return DisplayEntry.Value;
        public TableControlColumn()
        internal TableControlColumn(string text, int alignment, bool isscriptblock, string formatString)
            Alignment = (Alignment)alignment;
            DisplayEntry = new DisplayEntry(text, isscriptblock ? DisplayEntryValueType.ScriptBlock : DisplayEntryValueType.Property);
            FormatString = formatString;
        /// Public constructor for TableControlColumn.
        /// <param name="alignment"></param>
        public TableControlColumn(Alignment alignment, DisplayEntry entry)
            return DisplayEntry.SafeForExport();
    /// Defines a single row in a table control.
    public sealed class TableControlRow
        /// <summary>Collection of column definitions for this row</summary>
        public List<TableControlColumn> Columns { get; set; }
        /// <summary>List of typenames which select this entry</summary>
        public EntrySelectedBy SelectedBy { get; internal set; }
        /// <summary>When true, instead of truncating to the column width, use multiple lines.</summary>
        public bool Wrap { get; set; }
        /// <summary>Public constructor for TableControlRow</summary>
        public TableControlRow()
            Columns = new List<TableControlColumn>();
        internal TableControlRow(TableRowDefinition rowdefinition) : this()
            Wrap = rowdefinition.multiLine;
            if (rowdefinition.appliesTo != null)
                SelectedBy = EntrySelectedBy.Get(rowdefinition.appliesTo.referenceList);
            foreach (TableRowItemDefinition itemdef in rowdefinition.rowItemDefinitionList)
                TableControlColumn column;
                if (itemdef.formatTokenList[0] is FieldPropertyToken fpt)
                    column = new TableControlColumn(fpt.expression.expressionValue, itemdef.alignment,
                                    fpt.expression.isScriptBlock, fpt.fieldFormattingDirective.formatString);
                    column = new TableControlColumn();
                Columns.Add(column);
        /// <summary>Public constructor for TableControlRow.</summary>
        public TableControlRow(IEnumerable<TableControlColumn> columns) : this()
            if (columns == null)
                throw PSTraceSource.NewArgumentNullException(nameof(columns));
            foreach (TableControlColumn column in columns)
            foreach (var column in Columns)
                if (!column.SafeForExport())
            return SelectedBy != null && SelectedBy.SafeForExport();
            // Old versions of PowerShell don't support multiple row definitions.
            return SelectedBy == null;
    /// <summary>A helper class for defining table controls</summary>
    public sealed class TableRowDefinitionBuilder
        internal readonly TableControlBuilder _tcb;
        internal readonly TableControlRow _tcr;
        internal TableRowDefinitionBuilder(TableControlBuilder tcb, TableControlRow tcr)
            _tcb = tcb;
            _tcr = tcr;
        private TableRowDefinitionBuilder AddItem(string value, DisplayEntryValueType entryType, Alignment alignment, string format)
                throw PSTraceSource.NewArgumentException(nameof(value));
            var tableControlColumn = new TableControlColumn(alignment, new DisplayEntry(value, entryType))
            _tcr.Columns.Add(tableControlColumn);
        /// Add a column to the current row definition that calls a script block.
        public TableRowDefinitionBuilder AddScriptBlockColumn(string scriptBlock, Alignment alignment = Alignment.Undefined, string format = null)
            return AddItem(scriptBlock, DisplayEntryValueType.ScriptBlock, alignment, format);
        /// Add a column to the current row definition that references a property.
        public TableRowDefinitionBuilder AddPropertyColumn(string propertyName, Alignment alignment = Alignment.Undefined, string format = null)
            return AddItem(propertyName, DisplayEntryValueType.Property, alignment, format);
        /// Complete a row definition.
        public TableControlBuilder EndRowDefinition()
            return _tcb;
    public sealed class TableControlBuilder
        internal readonly TableControl _table;
        internal TableControlBuilder(TableControl table)
            _table = table;
        public TableControlBuilder GroupByProperty(string property, CustomControl customControl = null, string label = null)
            _table.GroupBy = new PSControlGroupBy
        public TableControlBuilder GroupByScriptBlock(string scriptBlock, CustomControl customControl = null, string label = null)
        /// <summary>Add a header</summary>
        public TableControlBuilder AddHeader(Alignment alignment = Alignment.Undefined, int width = 0, string label = null)
            _table.Headers.Add(new TableControlColumnHeader(label, width, alignment));
        public TableRowDefinitionBuilder StartRowDefinition(bool wrap = false, IEnumerable<string> entrySelectedByType = null, IEnumerable<DisplayEntry> entrySelectedByCondition = null)
            var row = new TableControlRow { Wrap = wrap };
                row.SelectedBy = new EntrySelectedBy();
                    row.SelectedBy.TypeNames = new List<string>(entrySelectedByType);
                    row.SelectedBy.SelectionCondition = new List<DisplayEntry>(entrySelectedByCondition);
            _table.Rows.Add(row);
            return new TableRowDefinitionBuilder(this, row);
        /// <summary>Complete a table definition</summary>
        public TableControl EndTable()
            return _table;
