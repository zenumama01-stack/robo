    /// The FilterRulePanelItemType enum is used to classify a <see cref="FilterRulePanelItem"/>'s
    /// hierarchical relationship within a <see cref="FilterRulePanel"/>.
    public enum FilterRulePanelItemType
        /// A FilterRulePanelItemType of FirstHeader indicates that a FilterRulePanelItem
        /// is the first item in the FilterRulePanel.
        FirstHeader = 0,
        /// A FilterRulePanelItemType of Header indicates that a FilterRulePanelItem with
        /// some GroupId is the first item in the FilterRulePanel with that GroupId.
        Header = 1,
        /// A FilterRulePanelItemType of Item indicates that a FilterRulePanelItem with
        /// some GroupId is not the first item in the FilterRulePanel with that GroupId.
        Item = 2
