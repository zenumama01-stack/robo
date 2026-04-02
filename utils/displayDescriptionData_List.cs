    #region List View Definitions
    /// In line definition of a list control.
    internal sealed class ListControlBody : ControlBody
        internal ListControlEntryDefinition defaultEntryDefinition = null;
        internal List<ListControlEntryDefinition> optionalEntryList = new List<ListControlEntryDefinition>();
        internal override ControlBase Copy()
            ListControlBody result = new ListControlBody();
            result.autosize = this.autosize;
            if (defaultEntryDefinition != null)
                result.defaultEntryDefinition = this.defaultEntryDefinition.Copy();
            foreach (ListControlEntryDefinition lced in this.optionalEntryList)
                result.optionalEntryList.Add(lced);
    /// Definition of the data to be displayed in a list entry.
    internal sealed class ListControlEntryDefinition
        /// Mandatory list of list view items.
        /// It cannot be empty.
        internal List<ListControlItemDefinition> itemDefinitionList = new List<ListControlItemDefinition>();
        internal ListControlEntryDefinition Copy()
            ListControlEntryDefinition result = new ListControlEntryDefinition();
            result.appliesTo = this.appliesTo;
            foreach (ListControlItemDefinition lcid in this.itemDefinitionList)
                result.itemDefinitionList.Add(lcid);
    /// Cell definition inside a row.
    internal sealed class ListControlItemDefinition
        internal ExpressionToken conditionToken;
        /// Optional label
        /// If not present, use the name of the property from the matching
        /// mandatory item description.
        internal TextToken label = null;
        /// Format directive body telling how to format the cell
        /// RULE: the body can only contain
        ///     * TextToken
        ///     * PropertyToken
        ///     * NOTHING (provide an empty cell)
    /// Defines a list control.
    public sealed class ListControl : PSControl
        /// <summary>Entries in this list control</summary>
        public List<ListControlEntry> Entries { get; internal set; }
        /// <summary></summary>
        public static ListControlBuilder Create(bool outOfBand = false)
            var list = new ListControl { OutOfBand = false };
            return new ListControlBuilder(list);
            writer.WriteListControl(this);
        /// <summary>Indicates if this control does not have any script blocks and is safe to export</summary>
        /// <summary>Initiate an instance of ListControl</summary>
        public ListControl()
            Entries = new List<ListControlEntry>();
        /// <summary>To go from internal representation to external - for Get-FormatData</summary>
        internal ListControl(ListControlBody listcontrolbody, ViewDefinition viewDefinition)
            this.GroupBy = PSControlGroupBy.Get(viewDefinition.groupBy);
            this.OutOfBand = viewDefinition.outOfBand;
            Entries.Add(new ListControlEntry(listcontrolbody.defaultEntryDefinition));
            foreach (ListControlEntryDefinition lced in listcontrolbody.optionalEntryList)
                Entries.Add(new ListControlEntry(lced));
        /// <summary>Public constructor for ListControl</summary>
        public ListControl(IEnumerable<ListControlEntry> entries)
            if (entries == null)
                throw PSTraceSource.NewArgumentNullException(nameof(entries));
            foreach (ListControlEntry entry in entries)
                this.Entries.Add(entry);
            if (!base.CompatibleWithOldPowerShell())
                if (!entry.CompatibleWithOldPowerShell())
    /// Defines one entry in a list control.
    public sealed class ListControlEntry
        /// <summary>List of items in the entry</summary>
        public List<ListControlEntryItem> Items { get; internal set; }
        /// <summary>List of typenames which select this entry, deprecated, use EntrySelectedBy</summary>
        public List<string> SelectedBy
                EntrySelectedBy ??= new EntrySelectedBy { TypeNames = new List<string>() };
                return EntrySelectedBy.TypeNames;
        /// <summary>List of typenames and/or a script block which select this entry.</summary>
        public EntrySelectedBy EntrySelectedBy { get; internal set; }
        /// <summary>Initiate an instance of ListControlEntry</summary>
        public ListControlEntry()
            Items = new List<ListControlEntryItem>();
        internal ListControlEntry(ListControlEntryDefinition entrydefn)
            if (entrydefn.appliesTo != null)
                EntrySelectedBy = EntrySelectedBy.Get(entrydefn.appliesTo.referenceList);
            foreach (ListControlItemDefinition itemdefn in entrydefn.itemDefinitionList)
                Items.Add(new ListControlEntryItem(itemdefn));
        /// <summary>Public constructor for ListControlEntry</summary>
        public ListControlEntry(IEnumerable<ListControlEntryItem> listItems)
            if (listItems == null)
                throw PSTraceSource.NewArgumentNullException(nameof(listItems));
            foreach (ListControlEntryItem item in listItems)
                this.Items.Add(item);
        public ListControlEntry(IEnumerable<ListControlEntryItem> listItems, IEnumerable<string> selectedBy)
            if (selectedBy == null)
                throw PSTraceSource.NewArgumentNullException(nameof(selectedBy));
            EntrySelectedBy = new EntrySelectedBy { TypeNames = new List<string>(selectedBy) };
            foreach (var item in Items)
            return EntrySelectedBy == null || EntrySelectedBy.SafeForExport();
                if (!item.CompatibleWithOldPowerShell())
            return EntrySelectedBy == null || EntrySelectedBy.CompatibleWithOldPowerShell();
    /// Defines one row in a list control entry.
    public sealed class ListControlEntryItem
        /// Gets the label for this List Control Entry Item
        /// If nothing is specified, then it uses the
        /// property name.
        public string Label { get; internal set; }
        /// <summary>Display entry</summary>
        public DisplayEntry DisplayEntry { get; internal set; }
        public DisplayEntry ItemSelectionCondition { get; internal set; }
        /// <summary>Format string to apply</summary>
        public string FormatString { get; internal set; }
        internal ListControlEntryItem()
        internal ListControlEntryItem(ListControlItemDefinition definition)
            if (definition.label != null)
                Label = definition.label.text;
            if (definition.formatTokenList[0] is FieldPropertyToken fpt)
                if (fpt.fieldFormattingDirective.formatString != null)
                    FormatString = fpt.fieldFormattingDirective.formatString;
                DisplayEntry = new DisplayEntry(fpt.expression);
                if (definition.conditionToken != null)
                    ItemSelectionCondition = new DisplayEntry(definition.conditionToken);
        /// Public constructor for ListControlEntryItem
        /// Label and Entry could be null.
        /// <param name="label"></param>
        /// <param name="entry"></param>
        public ListControlEntryItem(string label, DisplayEntry entry)
            this.Label = label;
            this.DisplayEntry = entry;
            return DisplayEntry.SafeForExport() &&
                   (ItemSelectionCondition == null || ItemSelectionCondition.SafeForExport());
            // Old versions of PowerShell know nothing about ItemSelectionCondition.
            return ItemSelectionCondition == null;
    public class ListEntryBuilder
        private readonly ListControlBuilder _listBuilder;
        internal ListControlEntry _listEntry;
        internal ListEntryBuilder(ListControlBuilder listBuilder, ListControlEntry listEntry)
            _listBuilder = listBuilder;
            _listEntry = listEntry;
        private ListEntryBuilder AddItem(string value, string label, DisplayEntryValueType kind, string format)
                throw PSTraceSource.NewArgumentNullException("property");
            _listEntry.Items.Add(new ListControlEntryItem
                DisplayEntry = new DisplayEntry(value, kind),
                Label = label,
                FormatString = format
        public ListEntryBuilder AddItemScriptBlock(string scriptBlock, string label = null, string format = null)
            return AddItem(scriptBlock, label, DisplayEntryValueType.ScriptBlock, format);
        public ListEntryBuilder AddItemProperty(string property, string label = null, string format = null)
            return AddItem(property, label, DisplayEntryValueType.Property, format);
        public ListControlBuilder EndEntry()
            return _listBuilder;
    public class ListControlBuilder
        internal ListControl _list;
        internal ListControlBuilder(ListControl list)
            _list = list;
        public ListControlBuilder GroupByProperty(string property, CustomControl customControl = null, string label = null)
            _list.GroupBy = new PSControlGroupBy
        public ListControlBuilder GroupByScriptBlock(string scriptBlock, CustomControl customControl = null, string label = null)
        public ListEntryBuilder StartEntry(IEnumerable<string> entrySelectedByType = null, IEnumerable<DisplayEntry> entrySelectedByCondition = null)
            var listEntry = new ListControlEntry
                EntrySelectedBy = EntrySelectedBy.Get(entrySelectedByType, entrySelectedByCondition)
            _list.Entries.Add(listEntry);
            return new ListEntryBuilder(this, listEntry);
        public ListControl EndList()
            return _list;
