    #region Wide View Definitions
    /// In line definition of a wide control.
    internal sealed class WideControlBody : ControlBody
        /// Number of columns to use for wide display.
        internal int columns = 0;
        /// Default wide entry definition
        internal WideControlEntryDefinition defaultEntryDefinition = null;
        internal List<WideControlEntryDefinition> optionalEntryList = new List<WideControlEntryDefinition>();
    internal sealed class WideControlEntryDefinition
    public sealed class WideControl : PSControl
        /// <summary>Entries in this wide control</summary>
        public List<WideControlEntryItem> Entries { get; internal set; }
        /// <summary>When true, widths are calculated based on more than the first object.</summary>
        /// <summary>Number of columns in the control</summary>
        public uint Columns { get; internal set; }
        /// <summary>Create a default WideControl</summary>
        public static WideControlBuilder Create(bool outOfBand = false, bool autoSize = false, uint columns = 0)
            var control = new WideControl { OutOfBand = false, AutoSize = autoSize, Columns = columns };
            return new WideControlBuilder(control);
            writer.WriteWideControl(this);
        /// Indicates if this control does not have
        /// any script blocks and is safe to export.
        /// <returns>True if exportable, false otherwise.</returns>
        /// <summary>Default constructor for WideControl</summary>
        public WideControl()
            Entries = new List<WideControlEntryItem>();
        internal WideControl(WideControlBody widecontrolbody, ViewDefinition viewDefinition) : this()
            AutoSize = widecontrolbody.autosize.GetValueOrDefault();
            Columns = (uint)widecontrolbody.columns;
            Entries.Add(new WideControlEntryItem(widecontrolbody.defaultEntryDefinition));
            foreach (WideControlEntryDefinition definition in widecontrolbody.optionalEntryList)
                Entries.Add(new WideControlEntryItem(definition));
        /// <summary>Public constructor for WideControl</summary>
        public WideControl(IEnumerable<WideControlEntryItem> wideEntries) : this()
            if (wideEntries == null)
                throw PSTraceSource.NewArgumentNullException(nameof(wideEntries));
            foreach (WideControlEntryItem entryItem in wideEntries)
                this.Entries.Add(entryItem);
        public WideControl(IEnumerable<WideControlEntryItem> wideEntries, uint columns) : this()
            this.Columns = columns;
        /// <summary>Construct an instance with columns</summary>
        public WideControl(uint columns) : this()
    /// Defines one item in a wide control entry.
    public sealed class WideControlEntryItem
        internal WideControlEntryItem()
        internal WideControlEntryItem(WideControlEntryDefinition definition) : this()
            if (definition.appliesTo != null)
                EntrySelectedBy = EntrySelectedBy.Get(definition.appliesTo.referenceList);
        /// Public constructor for WideControlEntryItem.
        public WideControlEntryItem(DisplayEntry entry) : this()
            if (entry == null)
                throw PSTraceSource.NewArgumentNullException(nameof(entry));
        public WideControlEntryItem(DisplayEntry entry, IEnumerable<string> selectedBy) : this()
            this.EntrySelectedBy = EntrySelectedBy.Get(selectedBy, null);
            return DisplayEntry.SafeForExport() && (EntrySelectedBy == null || EntrySelectedBy.SafeForExport());
            // Old versions of PowerShell don't know anything about FormatString or conditions in EntrySelectedBy.
            return FormatString == null &&
                   (EntrySelectedBy == null || EntrySelectedBy.CompatibleWithOldPowerShell());
    public sealed class WideControlBuilder
        private readonly WideControl _control;
        internal WideControlBuilder(WideControl control)
        public WideControlBuilder GroupByProperty(string property, CustomControl customControl = null, string label = null)
        public WideControlBuilder GroupByScriptBlock(string scriptBlock, CustomControl customControl = null, string label = null)
        public WideControlBuilder AddScriptBlockEntry(string scriptBlock, string format = null, IEnumerable<string> entrySelectedByType = null, IEnumerable<DisplayEntry> entrySelectedByCondition = null)
            var entry = new WideControlEntryItem(new DisplayEntry(scriptBlock, DisplayEntryValueType.ScriptBlock))
        public WideControlBuilder AddPropertyEntry(string propertyName, string format = null, IEnumerable<string> entrySelectedByType = null, IEnumerable<DisplayEntry> entrySelectedByCondition = null)
            var entry = new WideControlEntryItem(new DisplayEntry(propertyName, DisplayEntryValueType.Property))
        public WideControl EndWideControl()
