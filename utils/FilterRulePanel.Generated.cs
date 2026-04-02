    partial class FilterRulePanel
        // AddRules routed command
        /// Adds a collection of FilterRules to the panel.
        public static readonly RoutedCommand AddRulesCommand = new RoutedCommand("AddRules",typeof(FilterRulePanel));
        static private void AddRulesCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            FilterRulePanel obj = (FilterRulePanel) sender;
            obj.OnAddRulesExecuted( e );
        /// Called when AddRules executes.
        protected virtual void OnAddRulesExecuted(ExecutedRoutedEventArgs e)
            OnAddRulesExecutedImplementation(e);
        partial void OnAddRulesExecutedImplementation(ExecutedRoutedEventArgs e);
        // RemoveRule routed command
        /// Removes a FilterRulePanelItem from the panel.
        public static readonly RoutedCommand RemoveRuleCommand = new RoutedCommand("RemoveRule",typeof(FilterRulePanel));
        static private void RemoveRuleCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            obj.OnRemoveRuleExecuted( e );
        /// Called when RemoveRule executes.
        protected virtual void OnRemoveRuleExecuted(ExecutedRoutedEventArgs e)
            OnRemoveRuleExecutedImplementation(e);
        partial void OnRemoveRuleExecutedImplementation(ExecutedRoutedEventArgs e);
        static FilterRulePanel()
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterRulePanel), new FrameworkPropertyMetadata(typeof(FilterRulePanel)));
            CommandManager.RegisterClassCommandBinding( typeof(FilterRulePanel), new CommandBinding( FilterRulePanel.AddRulesCommand, AddRulesCommand_CommandExecuted ));
            CommandManager.RegisterClassCommandBinding( typeof(FilterRulePanel), new CommandBinding( FilterRulePanel.RemoveRuleCommand, RemoveRuleCommand_CommandExecuted ));
            return new ExtendedFrameworkElementAutomationPeer(this,AutomationControlType.Group,true);
