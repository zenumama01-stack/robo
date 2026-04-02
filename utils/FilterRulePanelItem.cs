    /// The FilterRulePanelItem class maintains the state for a row item within a <see cref="FilterRulePanel"/>.
    public class FilterRulePanelItem : INotifyPropertyChanged
        /// Gets a FilterRule that is stored in this FilterRulePanelItem.
        /// Gets a string that identifies which group this
        /// item belongs to.
        public string GroupId
        private FilterRulePanelItemType itemType = FilterRulePanelItemType.Header;
        /// Gets the type of FilterRulePanelItemType.
        public FilterRulePanelItemType ItemType
                return this.itemType;
            protected internal set
                if (value == this.itemType)
                this.itemType = value;
                this.NotifyPropertyChanged("ItemType");
        /// The FilterRule to store in this FilterRulePanelItem.
        /// <param name="groupId">
        /// A string which identifies which group this
        public FilterRulePanelItem(FilterRule rule, string groupId)
            ArgumentException.ThrowIfNullOrEmpty(groupId);
            this.GroupId = groupId;
        /// The name of a property that has changed.
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "not null");
