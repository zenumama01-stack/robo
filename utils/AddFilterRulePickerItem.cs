    /// The AddFilterRulePicker class is responsible for holding state
    /// information needed by the AddFilterRulePicker class.
    public class AddFilterRulePickerItem : INotifyPropertyChanged
        private bool isChecked;
        /// Gets or sets a value indicating whether this item should
        /// be added to the FilterRulePanel.
        public bool IsChecked
                return this.isChecked;
                if (value != this.isChecked)
                    this.isChecked = value;
                    this.NotifyPropertyChanged("IsChecked");
        /// Gets the FilterRulePanelItem that will be added to the FilterRulePanel.
        public FilterRulePanelItem FilterRule
        /// Initializes a new instance of the FilterRulePanelItem class.
        /// <param name="filterRule">
        /// The FilterRulePanelItem that will be added to the FilterRulePanel.
        public AddFilterRulePickerItem(FilterRulePanelItem filterRule)
            this.FilterRule = filterRule;
