    #region Complex View Definitions
    /// In line definition of a complex control.
    internal sealed class ComplexControlBody : ControlBody
        /// Default list entry definition
        /// It's mandatory.
        internal ComplexControlEntryDefinition defaultEntry;
        /// Optional list of list entry definition overrides. It can be empty if there are no overrides.
        internal List<ComplexControlEntryDefinition> optionalEntryList = new List<ComplexControlEntryDefinition>();
    internal sealed class ComplexControlEntryDefinition
        /// Applicability clause
        /// Only valid if not the default definition.
        internal AppliesTo appliesTo = null;
        /// Item associated with this entry definition.
    internal sealed class ComplexControlItemDefinition
        /// List of tokens the item can contain.
        internal List<FormatToken> formatTokenList = new List<FormatToken>();
    public sealed class CustomControl : PSControl
        public List<CustomControlEntry> Entries { get; set; }
        internal ComplexControlBody _cachedBody;
        internal CustomControl()
            Entries = new List<CustomControlEntry>();
        internal CustomControl(ComplexControlBody body, ViewDefinition viewDefinition)
            // viewDefinition can be null for nested controls
                OutOfBand = viewDefinition.outOfBand;
                GroupBy = PSControlGroupBy.Get(viewDefinition.groupBy);
            // Default entry
            var cce = new CustomControlEntry(body.defaultEntry);
            Entries.Add(cce);
            foreach (var entry in body.optionalEntryList)
                cce = new CustomControlEntry(entry);
        public static CustomControlBuilder Create(bool outOfBand = false)
            var customControl = new CustomControl { OutOfBand = outOfBand };
            return new CustomControlBuilder(customControl);
        internal override void WriteToXml(FormatXmlWriter writer)
            writer.WriteCustomControl(this);
        internal override bool SafeForExport()
            if (!base.SafeForExport())
            foreach (var entry in Entries)
                if (!entry.SafeForExport())
        internal override bool CompatibleWithOldPowerShell()
            // Old versions of PowerShell know nothing about CustomControl.
    public sealed class CustomControlEntry
        internal CustomControlEntry()
            CustomItems = new List<CustomItemBase>();
        internal CustomControlEntry(ComplexControlEntryDefinition entry)
            if (entry.appliesTo != null)
                SelectedBy = EntrySelectedBy.Get(entry.appliesTo.referenceList);
            foreach (var tok in entry.itemDefinition.formatTokenList)
                CustomItems.Add(CustomItemBase.Create(tok));
        public EntrySelectedBy SelectedBy { get; set; }
        public List<CustomItemBase> CustomItems { get; set; }
            foreach (var item in CustomItems)
                if (!item.SafeForExport())
            return SelectedBy == null || SelectedBy.SafeForExport();
    public abstract class CustomItemBase
        internal static CustomItemBase Create(FormatToken token)
            if (token is NewLineToken)
                return new CustomItemNewline();
            if (token is TextToken textToken)
                return new CustomItemText { Text = textToken.text };
            if (token is FrameToken frameToken)
                var frame = new CustomItemFrame
                    RightIndent = (uint)frameToken.frameInfoDefinition.rightIndentation,
                    LeftIndent = (uint)frameToken.frameInfoDefinition.leftIndentation
                var firstLine = frameToken.frameInfoDefinition.firstLine;
                if (firstLine > 0)
                    frame.FirstLineIndent = (uint)firstLine;
                else if (firstLine < 0)
                    frame.FirstLineHanging = (uint)-firstLine;
                foreach (var frameItemToken in frameToken.itemDefinition.formatTokenList)
                    frame.CustomItems.Add(CustomItemBase.Create(frameItemToken));
            if (token is CompoundPropertyToken cpt)
                var cie = new CustomItemExpression { EnumerateCollection = cpt.enumerateCollection };
                if (cpt.conditionToken != null)
                    cie.ItemSelectionCondition = new DisplayEntry(cpt.conditionToken);
                if (cpt.expression.expressionValue != null)
                    cie.Expression = new DisplayEntry(cpt.expression);
                if (cpt.control is ComplexControlBody complexControlBody)
                    cie.CustomControl = new CustomControl(complexControlBody, null);
                return cie;
            Diagnostics.Assert(false, "Unexpected formatting token kind");
    public sealed class CustomItemExpression : CustomItemBase
        internal CustomItemExpression() { }
        public DisplayEntry ItemSelectionCondition { get; set; }
        public bool EnumerateCollection { get; set; }
            return (ItemSelectionCondition == null || ItemSelectionCondition.SafeForExport()) &&
                   (Expression == null || Expression.SafeForExport()) &&
    public sealed class CustomItemFrame : CustomItemBase
        public uint LeftIndent { get; set; }
        public uint RightIndent { get; set; }
        public uint FirstLineHanging { get; set; }
        public uint FirstLineIndent { get; set; }
        internal CustomItemFrame()
            foreach (var frameItem in CustomItems)
                if (!frameItem.SafeForExport())
    public sealed class CustomItemNewline : CustomItemBase
        public CustomItemNewline()
            this.Count = 1;
    public sealed class CustomItemText : CustomItemBase
        public string Text { get; set; }
    public sealed class CustomEntryBuilder
        private readonly Stack<List<CustomItemBase>> _entryStack;
        private readonly CustomControlBuilder _controlBuilder;
        internal CustomEntryBuilder(CustomControlBuilder controlBuilder, CustomControlEntry entry)
            _entryStack = new Stack<List<CustomItemBase>>();
            _entryStack.Push(entry.CustomItems);
            _controlBuilder = controlBuilder;
        public CustomEntryBuilder AddNewline(int count = 1)
            _entryStack.Peek().Add(new CustomItemNewline { Count = count });
        public CustomEntryBuilder AddText(string text)
            _entryStack.Peek().Add(new CustomItemText { Text = text });
        private void AddDisplayExpressionBinding(
            string value,
            DisplayEntryValueType valueType,
            bool enumerateCollection = false,
            string selectedByType = null,
            string selectedByScript = null,
            CustomControl customControl = null)
            _entryStack.Peek().Add(new CustomItemExpression()
                ItemSelectionCondition = selectedByScript != null
                    ? new DisplayEntry(selectedByScript, DisplayEntryValueType.ScriptBlock)
                    : selectedByType != null
                        ? new DisplayEntry(selectedByType, DisplayEntryValueType.Property)
                        : null,
                EnumerateCollection = enumerateCollection,
                Expression = new DisplayEntry(value, valueType),
                CustomControl = customControl
        public CustomEntryBuilder AddPropertyExpressionBinding(
            AddDisplayExpressionBinding(property, DisplayEntryValueType.Property, enumerateCollection, selectedByType, selectedByScript, customControl);
        public CustomEntryBuilder AddScriptBlockExpressionBinding(
            string scriptBlock,
            AddDisplayExpressionBinding(scriptBlock, DisplayEntryValueType.ScriptBlock, enumerateCollection, selectedByType, selectedByScript, customControl);
        public CustomEntryBuilder AddCustomControlExpressionBinding(
            CustomControl customControl,
            string selectedByScript = null)
        public CustomEntryBuilder StartFrame(uint leftIndent = 0, uint rightIndent = 0, uint firstLineHanging = 0, uint firstLineIndent = 0)
            // Mutually exclusive
            if (leftIndent != 0 && rightIndent != 0)
                throw PSTraceSource.NewArgumentException(nameof(leftIndent));
            if (firstLineHanging != 0 && firstLineIndent != 0)
                throw PSTraceSource.NewArgumentException(nameof(firstLineHanging));
                LeftIndent = leftIndent,
                RightIndent = rightIndent,
                FirstLineHanging = firstLineHanging,
                FirstLineIndent = firstLineIndent
            _entryStack.Peek().Add(frame);
            _entryStack.Push(frame.CustomItems);
        public CustomEntryBuilder EndFrame()
            if (_entryStack.Count < 2)
            _entryStack.Pop();
        public CustomControlBuilder EndEntry()
            if (_entryStack.Count != 1)
            return _controlBuilder;
    public sealed class CustomControlBuilder
        internal CustomControl _control;
        internal CustomControlBuilder(CustomControl control)
            _control = control;
        /// <summary>Group instances by the property name with an optional label.</summary>
        public CustomControlBuilder GroupByProperty(string property, CustomControl customControl = null, string label = null)
            _control.GroupBy = new PSControlGroupBy
                Expression = new DisplayEntry(property, DisplayEntryValueType.Property),
                CustomControl = customControl,
                Label = label
        /// <summary>Group instances by the script block expression with an optional label.</summary>
        public CustomControlBuilder GroupByScriptBlock(string scriptBlock, CustomControl customControl = null, string label = null)
                Expression = new DisplayEntry(scriptBlock, DisplayEntryValueType.ScriptBlock),
        public CustomEntryBuilder StartEntry(IEnumerable<string> entrySelectedByType = null, IEnumerable<DisplayEntry> entrySelectedByCondition = null)
            var entry = new CustomControlEntry
                SelectedBy = EntrySelectedBy.Get(entrySelectedByType, entrySelectedByCondition)
            _control.Entries.Add(entry);
            return new CustomEntryBuilder(this, entry);
        public CustomControl EndControl()
            return _control;
