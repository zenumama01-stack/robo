    /// Allows the state of the ManagementList to be saved and restored.
    public class ManagementListStateDescriptor : StateDescriptor<ManagementList>
        private Dictionary<string, ColumnStateDescriptor> columns = new Dictionary<string, ColumnStateDescriptor>();
        private string searchBoxText;
        private List<RuleStateDescriptor> rulesSelected = new List<RuleStateDescriptor>();
        private string sortOrderPropertyName;
        #endregion Fields
        /// Constructs a new instance of the ManagementListStateDescriptor class.
        public ManagementListStateDescriptor()
        /// <param name="name">The name that will be displayed to users.</param>
        public ManagementListStateDescriptor(string name)
            : base(name)
        #region Save/Restore
        /// Saves a snapshot of the ManagementList state.
        /// <param name="subject">
        /// The ManagementList instance whose state should be preserved.
        /// Columns will not be saved if not supported per
        /// <see cref="VerifyColumnsSavable"/>.
        /// <exception cref="InvalidOperationException">
        /// ManagementList.AutoGenerateColumns not supported.
        public override void SaveState(ManagementList subject)
            ArgumentNullException.ThrowIfNull(subject);
            this.SaveColumns(subject);
            this.SaveSortOrder(subject);
            this.SaveRulesSelected(subject);
        /// Restores the state of the passed in ManagementList and applies the restored filter.
        /// <param name="subject">The ManagementList instance whose state should be restored.</param>
        public override void RestoreState(ManagementList subject)
            this.RestoreState(subject, true);
        /// Restores the state of the passed in ManagementList.
        /// The ManagementList instance whose state should be restored.
        /// <param name="applyRestoredFilter">
        /// Whether the restored filter should be automatically applied.
        /// Columns will not be restored if not supported per
        /// <see cref="VerifyColumnsRestorable"/>.
        public void RestoreState(ManagementList subject, bool applyRestoredFilter)
            // Clear the sort, otherwise restoring columns and filters may trigger extra sorting \\
            subject.List.ClearSort();
            this.RestoreColumns(subject);
            this.RestoreRulesSelected(subject);
            if (applyRestoredFilter)
                subject.Evaluator.StartFilter();
            // Apply sorting after everything else has been set up \\
            this.RestoreSortOrder(subject);
        #region Verify State Helpers
        private static bool VerifyColumnsSavable(ManagementList subject, RetryActionCallback<ManagementList> callback)
            if (!VerifyColumnsRestorable(subject, callback))
            if (subject.List.InnerGrid == null)
        /// Checks whether columns can be restored.
        /// <param name="subject">Target ManagementList.</param>
        /// <param name="callback">RetryActionAfterLoaded callback method.</param>
        /// <returns>True if-and-only-if columns are restorable.</returns>
        private static bool VerifyColumnsRestorable(ManagementList subject, RetryActionCallback<ManagementList> callback)
            if (WpfHelp.RetryActionAfterLoaded<ManagementList>(subject, callback, subject))
            if (WpfHelp.RetryActionAfterLoaded<ManagementList>(subject.List, callback, subject))
            if (subject.List == null)
            // Columns are not savable/restorable if AutoGenerateColumns is true.
            if (subject.List.AutoGenerateColumns)
                throw new InvalidOperationException("View Manager is not supported when AutoGenerateColumns is set.");
        private static bool VerifyRulesSavableAndRestorable(ManagementList subject, RetryActionCallback<ManagementList> callback)
            if (subject.AddFilterRulePicker == null)
            if (subject.FilterRulePanel == null)
            if (subject.SearchBox == null)
        #endregion Verify State Helpers
        #region Save/Restore Helpers
        #region Columns
        private void SaveColumns(ManagementList subject)
            if (!VerifyColumnsSavable(subject, this.SaveColumns))
            this.columns.Clear();
            foreach (InnerListColumn ilc in subject.List.InnerGrid.Columns)
                ColumnStateDescriptor csd = CreateColumnStateDescriptor(ilc, true);
                csd.Index = i++;
                this.columns.Add(ilc.DataDescription.PropertyName, csd);
            foreach (InnerListColumn ilc in subject.List.InnerGrid.AvailableColumns)
                if (subject.List.InnerGrid.Columns.Contains(ilc))
                ColumnStateDescriptor csd = CreateColumnStateDescriptor(ilc, false);
        private void RestoreColumns(ManagementList subject)
            if (!VerifyColumnsRestorable(subject, this.RestoreColumns))
            this.RestoreColumnsState(subject);
            this.RestoreColumnsOrder(subject);
            subject.List.RefreshColumns();
        /// Set column state for target <see cref="ManagementList"/> to
        /// previously persisted state.
        /// Target <see cref="ManagementList"/> whose column state
        /// is to be restored.
        /// Required columns are always visible regardless of persisted state.
        private void RestoreColumnsState(ManagementList subject)
            ColumnStateDescriptor csd;
            foreach (InnerListColumn ilc in subject.List.Columns)
                if (this.columns.TryGetValue(ilc.DataDescription.PropertyName, out csd))
                    SetColumnSortDirection(ilc, csd.SortDirection);
                    SetColumnIsInUse(ilc, csd.IsInUse || ilc.Required);
                    SetColumnWidth(ilc, csd.Width);
                    SetColumnIsInUse(ilc, ilc.Required);
        private void RestoreColumnsOrder(ManagementList subject)
            // Restore the order of Columns
            // Use the sorted copy to determine what values to swap
            List<InnerListColumn> columnsCopy = new List<InnerListColumn>(subject.List.Columns);
            InnerListColumnOrderComparer ilcc = new InnerListColumnOrderComparer(this.columns);
            columnsCopy.Sort(ilcc);
            Debug.Assert(columnsCopy.Count == subject.List.Columns.Count, "match count");
            Utilities.ResortObservableCollection<InnerListColumn>(
                subject.List.Columns,
                columnsCopy);
            // Restore the order of InnerGrid.Columns
            columnsCopy.Clear();
            foreach (GridViewColumn gvc in subject.List.InnerGrid.Columns)
                columnsCopy.Add((InnerListColumn)gvc);
            Debug.Assert(columnsCopy.Count == subject.List.InnerGrid.Columns.Count, "match count");
            Utilities.ResortObservableCollection<GridViewColumn>(
                subject.List.InnerGrid.Columns,
        #endregion Columns
        #region Rules
        private void SaveRulesSelected(ManagementList subject)
            if (!VerifyRulesSavableAndRestorable(subject, this.SaveRulesSelected))
            this.rulesSelected.Clear();
            this.searchBoxText = subject.SearchBox.Text;
            foreach (FilterRulePanelItem item in subject.FilterRulePanel.FilterRulePanelItems)
                RuleStateDescriptor rsd = new RuleStateDescriptor();
                rsd.UniqueName = item.GroupId;
                rsd.Rule = item.Rule.DeepCopy();
                this.rulesSelected.Add(rsd);
        private void RestoreRulesSelected(ManagementList subject)
            if (!VerifyRulesSavableAndRestorable(subject, this.RestoreRulesSelected))
            subject.Evaluator.StopFilter();
            subject.SearchBox.Text = this.searchBoxText;
            this.AddSelectedRules(subject);
        private void AddSelectedRules(ManagementList subject)
            // Cache values
            Dictionary<string, FilterRulePanelItem> rulesCache = new Dictionary<string, FilterRulePanelItem>();
            foreach (AddFilterRulePickerItem pickerItem in subject.AddFilterRulePicker.ShortcutFilterRules)
                rulesCache.Add(pickerItem.FilterRule.GroupId, pickerItem.FilterRule);
            foreach (AddFilterRulePickerItem pickerItem in subject.AddFilterRulePicker.ColumnFilterRules)
            subject.FilterRulePanel.Controller.ClearFilterRulePanelItems();
            foreach (RuleStateDescriptor rsd in this.rulesSelected)
                AddSelectedRule(subject, rsd, rulesCache);
        private static void AddSelectedRule(ManagementList subject, RuleStateDescriptor rsd, Dictionary<string, FilterRulePanelItem> rulesCache)
            FilterRulePanelItem item;
            if (rulesCache.TryGetValue(rsd.UniqueName, out item))
                subject.FilterRulePanel.Controller.AddFilterRulePanelItem(new FilterRulePanelItem(rsd.Rule.DeepCopy(), item.GroupId));
        #endregion Rules
        #region Sort Order
        private void SaveSortOrder(ManagementList subject)
            if (!VerifyColumnsRestorable(subject, this.SaveSortOrder))
            // NOTE : We only support sorting on one property.
            if (subject.List.SortedColumn != null)
                this.sortOrderPropertyName = subject.List.SortedColumn.DataDescription.PropertyName;
                this.sortOrderPropertyName = string.Empty;
        private void RestoreSortOrder(ManagementList subject)
            if (!VerifyColumnsRestorable(subject, this.RestoreSortOrder))
            if (!string.IsNullOrEmpty(this.sortOrderPropertyName))
                foreach (InnerListColumn column in subject.List.Columns)
                    if (column.DataDescription.PropertyName == this.sortOrderPropertyName)
                        subject.List.ApplySort(column, false);
        #endregion Sort Order
        #endregion Save/Restore Helpers
        #region Column Helpers
        private static ColumnStateDescriptor CreateColumnStateDescriptor(InnerListColumn ilc, bool isInUse)
            ColumnStateDescriptor csd = new ColumnStateDescriptor();
            csd.IsInUse = isInUse;
            csd.Width = GetColumnWidth(ilc);
            csd.SortDirection = ilc.DataDescription.SortDirection;
            return csd;
        #region SortDirection
        private static void SetColumnSortDirection(InnerListColumn ilc, ListSortDirection sortDirection)
            ilc.DataDescription.SortDirection = sortDirection;
        #endregion IsInUse
        #region IsInUse
        private static void SetColumnIsInUse(InnerListColumn ilc, bool isInUse)
            ilc.Visible = isInUse;
        #region Width
        private static double GetColumnWidth(InnerListColumn ilc)
            return ilc.Visible ? ilc.ActualWidth : ilc.Width;
        private static void SetColumnWidth(GridViewColumn ilc, double width)
            if (!double.IsNaN(width))
                ilc.Width = width;
        #endregion Width
        #endregion Column Helpers
        #endregion Save/Restore
        #region Helper Classes
        internal class ColumnStateDescriptor
            private int index;
            private bool isInUse;
            private ListSortDirection sortDirection;
            private double width;
            /// Gets or sets the location of the column.
            public int Index
                get { return this.index; }
                set { this.index = value; }
            /// Gets or sets a value indicating whether the column should be shown.
            public bool IsInUse
                get { return this.isInUse; }
                set { this.isInUse = value; }
            /// Gets or sets the sort direction of the column.
            public ListSortDirection SortDirection
                get { return this.sortDirection; }
                set { this.sortDirection = value; }
            /// Gets or sets a value indicating the width of a column.
            public double Width
                get { return this.width; }
                set { this.width = value; }
        internal class RuleStateDescriptor
            /// Gets or sets the UniqueName associated with the rule.
            public string UniqueName
            /// Gets the FilterRule associated with the rule.
        internal class InnerListColumnOrderComparer : IComparer<InnerListColumn>
            private Dictionary<string, ColumnStateDescriptor> columns;
            /// Constructor that takes a lookup dictionary of column information.
            /// <param name="columns">The lookup dictionary.</param>
            public InnerListColumnOrderComparer(Dictionary<string, ColumnStateDescriptor> columns)
                this.columns = columns;
            /// Compares two InnerListColumn objects and determines their relative
            /// ordering.
            /// <param name="x">The first object.</param>
            /// <param name="y">The second object.</param>
            /// Returns 1 if x should ordered after y in the list, returns -1 if
            /// x should be order before y, and returns 0 if the ordering should not
            /// be changed.
            public int Compare(InnerListColumn x, InnerListColumn y)
                if (ReferenceEquals(x, y))
                else if (x == null)
                else if (y == null)
                ColumnStateDescriptor csdX;
                ColumnStateDescriptor csdY;
                this.columns.TryGetValue(x.DataDescription.PropertyName, out csdX);
                this.columns.TryGetValue(y.DataDescription.PropertyName, out csdY);
                if (csdX == null || csdY == null || (csdX.IsInUse && csdX.IsInUse) == false)
                return (csdX.Index > csdY.Index) ? 1 : -1;
        #endregion Helper Classes
        #region ToString
        /// Displayable string identifying this class instance.
        /// <returns>A string to represent the instance of this class.</returns>
        public override string ToString()
            return this.Name;
        #endregion ToString
