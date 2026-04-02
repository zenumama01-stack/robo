    /// The FilterRulePanelController is responsible managing the addition and removal of
    /// <see cref="FilterRulePanelItems" />s to a <see cref="FilterRulePanel"/>.
    public class FilterRulePanelController : IFilterExpressionProvider
        private ObservableCollection<FilterRulePanelItem> filterRulePanelItems;
        private ReadOnlyObservableCollection<FilterRulePanelItem> readOnlyFilterRulePanelItems;
            get { return this.readOnlyFilterRulePanelItems; }
                return this.CreateFilterExpression();
                return this.FilterExpression != null;
        /// Initializes a new instance of the FilterRulePanelController class.
        public FilterRulePanelController()
            this.filterRulePanelItems =
                new ObservableCollection<FilterRulePanelItem>();
            this.readOnlyFilterRulePanelItems =
                new ReadOnlyObservableCollection<FilterRulePanelItem>(this.filterRulePanelItems);
        /// Adds an item to the panel.
        /// The item to add.
        public void AddFilterRulePanelItem(FilterRulePanelItem item)
            ArgumentNullException.ThrowIfNull(item);
            int insertionIndex = this.GetInsertionIndex(item);
            this.filterRulePanelItems.Insert(insertionIndex, item);
            item.Rule.EvaluationResultInvalidated += this.Rule_EvaluationResultInvalidated;
            this.UpdateFilterRulePanelItemTypes();
        private void Rule_EvaluationResultInvalidated(object sender, EventArgs e)
        /// Removes an item from the panel.
        /// The item to remove.
        public void RemoveFilterRulePanelItem(FilterRulePanelItem item)
            item.Rule.EvaluationResultInvalidated -= this.Rule_EvaluationResultInvalidated;
            this.filterRulePanelItems.Remove(item);
        /// Removes all items from the panel.
        public void ClearFilterRulePanelItems()
            this.filterRulePanelItems.Clear();
        #region CreateFilterExpression
        private FilterExpressionNode CreateFilterExpression()
            List<FilterExpressionNode> groupNodes = new List<FilterExpressionNode>();
            for (int i = 0; i < this.filterRulePanelItems.Count;)
                int endIndex = this.GetExclusiveEndIndexForGroupStartingAt(i);
                FilterExpressionOrOperatorNode operatorOrNode = this.CreateFilterExpressionForGroup(i, endIndex);
                if (operatorOrNode.Children.Count > 0)
                    groupNodes.Add(operatorOrNode);
                i = endIndex;
            if (groupNodes.Count == 0)
            return new FilterExpressionAndOperatorNode(groupNodes);
        private int GetExclusiveEndIndexForGroupStartingAt(int startIndex)
            Debug.Assert(this.filterRulePanelItems.Count > 0, "greater than 0");
            Debug.Assert(startIndex >= 0, "greater than or equal to 0");
            int i = startIndex;
            for (; i < this.filterRulePanelItems.Count; i++)
                if (i == startIndex)
                string currentId = this.filterRulePanelItems[i].GroupId;
                string previousId = this.filterRulePanelItems[i - 1].GroupId;
                if (!currentId.Equals(previousId, StringComparison.Ordinal))
        private FilterExpressionOrOperatorNode CreateFilterExpressionForGroup(int startIndex, int endIndex)
            Debug.Assert(this.filterRulePanelItems.Count >= endIndex, "greater than or equal to endIndex");
            FilterExpressionOrOperatorNode groupNode = new FilterExpressionOrOperatorNode();
            for (int i = startIndex; i < endIndex; i++)
                FilterRule rule = this.filterRulePanelItems[i].Rule;
                if (rule.IsValid)
                    groupNode.Children.Add(new FilterExpressionOperandNode(rule.DeepCopy()));
            return groupNode;
        #endregion CreateFilterExpression
        #region Add/Remove Item Helpers
        private int GetInsertionIndex(FilterRulePanelItem item)
            for (int i = this.filterRulePanelItems.Count - 1; i >= 0; i--)
                string uniqueId = this.filterRulePanelItems[i].GroupId;
                if (uniqueId.Equals(item.GroupId, StringComparison.Ordinal))
            return this.filterRulePanelItems.Count;
        private void UpdateFilterRulePanelItemTypes()
            if (this.filterRulePanelItems.Count > 0)
                this.filterRulePanelItems[0].ItemType = FilterRulePanelItemType.FirstHeader;
            for (int i = 1; i < this.filterRulePanelItems.Count; i++)
                    this.filterRulePanelItems[i].ItemType = FilterRulePanelItemType.Header;
                    this.filterRulePanelItems[i].ItemType = FilterRulePanelItemType.Item;
        #endregion Add/Remove Item Helpers
