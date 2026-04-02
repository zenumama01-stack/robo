    /// The AddFilterRulePicker class is responsible for allowing users to
    /// add rules to an FilterRulePanel.
    public partial class AddFilterRulePicker : Control
        private ObservableCollection<AddFilterRulePickerItem> shortcutFilterRules = new ObservableCollection<AddFilterRulePickerItem>();
        /// Gets the collection of shortcut rules available for addition to the FilterRulePanel.
        public ObservableCollection<AddFilterRulePickerItem> ShortcutFilterRules
                return this.shortcutFilterRules;
        private ObservableCollection<AddFilterRulePickerItem> columnFilterRules = new ObservableCollection<AddFilterRulePickerItem>();
        /// Gets the collection of column rules available for addition to the FilterRulePanel.
        public ObservableCollection<AddFilterRulePickerItem> ColumnFilterRules
                return this.columnFilterRules;
        partial void OnOkAddFilterRulesCanExecuteImplementation(System.Windows.Input.CanExecuteRoutedEventArgs e)
            e.CanExecute = (this.AddFilterRulesCommand != null)
                ? CommandHelper.CanExecuteCommand(this.AddFilterRulesCommand, null, this.AddFilterRulesCommandTarget)
                : false;
        partial void OnOkAddFilterRulesExecutedImplementation(System.Windows.Input.ExecutedRoutedEventArgs e)
            Collection<FilterRulePanelItem> addedRules = new Collection<FilterRulePanelItem>();
            foreach (AddFilterRulePickerItem item in this.shortcutFilterRules)
                if (item.IsChecked)
                    addedRules.Add(item.FilterRule);
                    item.IsChecked = false;
            foreach (AddFilterRulePickerItem item in this.columnFilterRules)
            if (addedRules.Count > 0)
                CommandHelper.ExecuteCommand(this.AddFilterRulesCommand, addedRules, this.AddFilterRulesCommandTarget);
        partial void OnCancelAddFilterRulesExecutedImplementation(System.Windows.Input.ExecutedRoutedEventArgs e)
