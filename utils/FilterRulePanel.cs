    /// The FilterRulePanel allows users to construct and display a complex query built using <see cref="FilterRule"/>s.
    /// The FilterRulePanel manages two primary entities: <see cref="FilterRulePanelItem"/>s and DataTemplates.
    /// /// </para>
    /// <see cref="FilterRulePanelItem" />s are the data classes that store the state for each item in the panel.
    /// They are added and removed to/from the panel using the AddRulesCommand and the RemoveRuleCommand commands.
    /// For a FilterRule to display in the panel it must have a DataTemplate registered. To add and remove
    /// DataTemplates, use the AddFilterRulePanelItemContentTemplate and RemoveFilterRulePanelItemContentTemplate methods.
    public partial class FilterRulePanel : Control, IFilterExpressionProvider
        #region Filter Rule Panel Items
        /// Gets the collection of FilterRulePanelItems that are currently
        /// displayed in the panel.
        public ReadOnlyCollection<FilterRulePanelItem> FilterRulePanelItems
                return this.Controller.FilterRulePanelItems;
        #endregion Filter Rule Panel Items
        #region Filter Expression
                return this.Controller.FilterExpression;
        #endregion Filter Expression
        #region Controller
        private FilterRulePanelController controller = new FilterRulePanelController();
        /// Gets the FilterRulePanelController associated with this FilterRulePanel.
        public FilterRulePanelController Controller
                return this.controller;
        #endregion Controller
        #region Filter Rule Template Selector
        private FilterRuleTemplateSelector filterRuleTemplateSelector;
        /// Gets a FilterRuleTemplateSelector that stores
        /// the templates used for items in the panel.
        public DataTemplateSelector FilterRuleTemplateSelector
                return this.filterRuleTemplateSelector;
        #endregion Filter Rule Template Selector
                return this.Controller.HasFilterExpression;
        /// Raised when a FilterRulePanelItem has been added or removed.
        /// Initializes a new instance of the FilterRulePanel class.
        public FilterRulePanel()
            this.InitializeTemplates();
            this.Controller.FilterExpressionChanged += this.Controller_FilterExpressionChanged;
        #region Content Templates
        /// Associates a DataTemplate with a Type so that objects of that Type
        /// that are displayed in FilterRulePanel use the specified DataTemplate.
        /// <param name="type">
        /// The type to associate the DataTemplate with.
        /// <param name="dataTemplate">
        /// The DataTemplate to associate the type with.
        public void AddFilterRulePanelItemContentTemplate(Type type, DataTemplate dataTemplate)
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(dataTemplate);
            this.filterRuleTemplateSelector.TemplateDictionary.Add(new KeyValuePair<Type, DataTemplate>(type, dataTemplate));
        /// Removes the Type and associated DataTemplate from usage when displaying objects
        /// of that type in the FilterRulePanel.
        /// The type to remove.
        public void RemoveFilterRulePanelItemContentTemplate(Type type)
            this.filterRuleTemplateSelector.TemplateDictionary.Remove(type);
        /// Gets a DataTemplate associated with a type.
        /// <param name="type">A Type whose DataTemplate will be returned.</param>
        /// <param name="dataTemplate">A DataTemplate registered for type.</param>
        /// <returns>Returns true if there is a DataTemplate registered for type, false otherwise.</returns>
        public bool TryGetContentTemplate(Type type, out DataTemplate dataTemplate)
            dataTemplate = null;
            return this.filterRuleTemplateSelector.TemplateDictionary.TryGetValue(type, out dataTemplate);
        /// Removes all the registered content templates.
        public void ClearContentTemplates()
            this.filterRuleTemplateSelector.TemplateDictionary.Clear();
        #endregion Content Templates
        #region Notify Filter Expression Changed
        private void Controller_FilterExpressionChanged(object sender, EventArgs e)
        #endregion Notify Filter Expression Changed
        #region Add Rules Command Callback
        partial void OnAddRulesExecutedImplementation(ExecutedRoutedEventArgs e)
            Debug.Assert(e != null, "not null");
                throw new ArgumentException("e.Parameter is null.", "e");
            List<FilterRulePanelItem> itemsToAdd = new List<FilterRulePanelItem>();
            IList selectedItems = (IList)e.Parameter;
            foreach (object item in selectedItems)
                FilterRulePanelItem newItem = item as FilterRulePanelItem;
                if (newItem == null)
                    throw new ArgumentException(
                        "e.Parameter contains a value which is not a valid FilterRulePanelItem object.",
                        "e");
                itemsToAdd.Add(newItem);
            foreach (FilterRulePanelItem item in itemsToAdd)
                this.AddFilterRuleInternal(item);
        #endregion Add Rules Command Callback
        #region Remove Rule Command Callback
        partial void OnRemoveRuleExecutedImplementation(ExecutedRoutedEventArgs e)
            FilterRulePanelItem item = e.Parameter as FilterRulePanelItem;
                throw new ArgumentException("e.Parameter is not a valid FilterRulePanelItem object.", "e");
            this.RemoveFilterRuleInternal(item);
        #endregion Remove Rule Command Callback
        #region InitializeTemplates
        private void InitializeTemplates()
            this.filterRuleTemplateSelector = new FilterRuleTemplateSelector();
            this.InitializeTemplatesForInputTypes();
            List<KeyValuePair<Type, string>> defaultTemplates = new List<KeyValuePair<Type, string>>()
                new KeyValuePair<Type, string>(typeof(SelectorFilterRule), "CompositeRuleTemplate"),
                new KeyValuePair<Type, string>(typeof(SingleValueComparableValueFilterRule<>), "ComparableValueRuleTemplate"),
                new KeyValuePair<Type, string>(typeof(IsEmptyFilterRule), "NoInputTemplate"),
                new KeyValuePair<Type, string>(typeof(IsNotEmptyFilterRule), "NoInputTemplate"),
                new KeyValuePair<Type, string>(typeof(FilterRulePanelItemType), "FilterRulePanelGroupItemTypeTemplate"),
                new KeyValuePair<Type, string>(typeof(ValidatingValue<>), "ValidatingValueTemplate"),
                new KeyValuePair<Type, string>(typeof(ValidatingSelectorValue<>), "ValidatingSelectorValueTemplate"),
                new KeyValuePair<Type, string>(typeof(IsBetweenFilterRule<>), "IsBetweenRuleTemplate"),
                new KeyValuePair<Type, string>(typeof(object), "CatchAllTemplate")
            defaultTemplates.ForEach(templateInfo => this.AddFilterRulePanelItemContentTemplate(templateInfo.Key, templateInfo.Value));
        private void InitializeTemplatesForInputTypes()
            List<Type> inputTypes = new List<Type>()
                typeof(sbyte),
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
                typeof(char),
                typeof(Single),
                typeof(double),
                typeof(decimal),
                typeof(bool),
                typeof(Enum),
                typeof(DateTime),
                typeof(string)
            inputTypes.ForEach(type => this.AddFilterRulePanelItemContentTemplate(type, "InputValueTemplate"));
        private void AddFilterRulePanelItemContentTemplate(Type type, string resourceName)
            Debug.Assert(type != null, "not null");
            Debug.Assert(!string.IsNullOrEmpty(resourceName), "not null");
            var templateInfo = new ComponentResourceKey(typeof(FilterRulePanel), resourceName);
            DataTemplate template = (DataTemplate)this.TryFindResource(templateInfo);
            Debug.Assert(template != null, "not null");
            this.AddFilterRulePanelItemContentTemplate(type, template);
        #endregion InitializeTemplates
        #region Add/Remove FilterRules to Controller
        private void AddFilterRuleInternal(FilterRulePanelItem item)
            Debug.Assert(item != null, "not null");
            FilterRulePanelItem newItem = new FilterRulePanelItem(item.Rule.DeepCopy(), item.GroupId);
            this.Controller.AddFilterRulePanelItem(newItem);
        private void RemoveFilterRuleInternal(FilterRulePanelItem item)
            this.Controller.RemoveFilterRulePanelItem(item);
        #endregion Add/Remove FilterRules to Controller
