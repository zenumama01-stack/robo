    /// Interaction logic for ManagementList.
    [TemplatePart(Name="PART_ViewManager", Type=typeof(ListOrganizer))]
    [TemplatePart(Name="PART_ViewSaver", Type=typeof(PickerBase))]
    partial class ManagementList
        private ListOrganizer viewManager;
        private PickerBase viewSaver;
        // ViewsChanged RoutedEvent
        /// Identifies the ViewsChanged RoutedEvent.
        public static readonly RoutedEvent ViewsChangedEvent = EventManager.RegisterRoutedEvent("ViewsChanged",RoutingStrategy.Bubble,typeof(RoutedEventHandler),typeof(ManagementList));
        /// Occurs when any of this instance's views change.
        public event RoutedEventHandler ViewsChanged
                AddHandler(ViewsChangedEvent,value);
                RemoveHandler(ViewsChangedEvent,value);
        // ClearFilter routed command
        /// Informs the ManagementList that it should clear the filter that is applied.
        public static readonly RoutedCommand ClearFilterCommand = new RoutedCommand("ClearFilter",typeof(ManagementList));
        static private void ClearFilterCommand_CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
            ManagementList obj = (ManagementList) sender;
            obj.OnClearFilterCanExecute( e );
        static private void ClearFilterCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            obj.OnClearFilterExecuted( e );
        /// Called to determine if ClearFilter can execute.
        protected virtual void OnClearFilterCanExecute(CanExecuteRoutedEventArgs e)
            OnClearFilterCanExecuteImplementation(e);
        partial void OnClearFilterCanExecuteImplementation(CanExecuteRoutedEventArgs e);
        /// Called when ClearFilter executes.
        protected virtual void OnClearFilterExecuted(ExecutedRoutedEventArgs e)
            OnClearFilterExecutedImplementation(e);
        partial void OnClearFilterExecutedImplementation(ExecutedRoutedEventArgs e);
        // SaveView routed command
        public static readonly RoutedCommand SaveViewCommand = new RoutedCommand("SaveView",typeof(ManagementList));
        static private void SaveViewCommand_CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
            obj.OnSaveViewCanExecute( e );
        static private void SaveViewCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            obj.OnSaveViewExecuted( e );
        /// Called to determine if SaveView can execute.
        protected virtual void OnSaveViewCanExecute(CanExecuteRoutedEventArgs e)
            OnSaveViewCanExecuteImplementation(e);
        partial void OnSaveViewCanExecuteImplementation(CanExecuteRoutedEventArgs e);
        /// Called when SaveView executes.
        protected virtual void OnSaveViewExecuted(ExecutedRoutedEventArgs e)
            OnSaveViewExecutedImplementation(e);
        partial void OnSaveViewExecutedImplementation(ExecutedRoutedEventArgs e);
        // StartFilter routed command
        /// Informs the ManagementList that it should apply the filter.
        public static readonly RoutedCommand StartFilterCommand = new RoutedCommand("StartFilter",typeof(ManagementList));
        static private void StartFilterCommand_CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
            obj.OnStartFilterCanExecute( e );
        static private void StartFilterCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            obj.OnStartFilterExecuted( e );
        /// Called to determine if StartFilter can execute.
        protected virtual void OnStartFilterCanExecute(CanExecuteRoutedEventArgs e)
            OnStartFilterCanExecuteImplementation(e);
        partial void OnStartFilterCanExecuteImplementation(CanExecuteRoutedEventArgs e);
        /// Called when StartFilter executes.
        protected virtual void OnStartFilterExecuted(ExecutedRoutedEventArgs e)
            OnStartFilterExecutedImplementation(e);
        partial void OnStartFilterExecutedImplementation(ExecutedRoutedEventArgs e);
        // StopFilter routed command
        /// Informs the ManagementList that it should stop filtering that is in progress.
        public static readonly RoutedCommand StopFilterCommand = new RoutedCommand("StopFilter",typeof(ManagementList));
        static private void StopFilterCommand_CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
            obj.OnStopFilterCanExecute( e );
        static private void StopFilterCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            obj.OnStopFilterExecuted( e );
        /// Called to determine if StopFilter can execute.
        protected virtual void OnStopFilterCanExecute(CanExecuteRoutedEventArgs e)
            OnStopFilterCanExecuteImplementation(e);
        partial void OnStopFilterCanExecuteImplementation(CanExecuteRoutedEventArgs e);
        /// Called when StopFilter executes.
        protected virtual void OnStopFilterExecuted(ExecutedRoutedEventArgs e)
            OnStopFilterExecutedImplementation(e);
        partial void OnStopFilterExecutedImplementation(ExecutedRoutedEventArgs e);
        // AddFilterRulePicker dependency property
        /// Identifies the AddFilterRulePicker dependency property key.
        private static readonly DependencyPropertyKey AddFilterRulePickerPropertyKey = DependencyProperty.RegisterReadOnly( "AddFilterRulePicker", typeof(AddFilterRulePicker), typeof(ManagementList), new PropertyMetadata( null, AddFilterRulePickerProperty_PropertyChanged) );
        /// Identifies the AddFilterRulePicker dependency property.
        public static readonly DependencyProperty AddFilterRulePickerProperty = AddFilterRulePickerPropertyKey.DependencyProperty;
        /// Gets the filter rule picker.
        [Description("Gets the filter rule picker.")]
        public AddFilterRulePicker AddFilterRulePicker
                return (AddFilterRulePicker) GetValue(AddFilterRulePickerProperty);
                SetValue(AddFilterRulePickerPropertyKey,value);
        static private void AddFilterRulePickerProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            ManagementList obj = (ManagementList) o;
            obj.OnAddFilterRulePickerChanged( new PropertyChangedEventArgs<AddFilterRulePicker>((AddFilterRulePicker)e.OldValue, (AddFilterRulePicker)e.NewValue) );
        /// Occurs when AddFilterRulePicker property changes.
        public event EventHandler<PropertyChangedEventArgs<AddFilterRulePicker>> AddFilterRulePickerChanged;
        /// Called when AddFilterRulePicker property changes.
        protected virtual void OnAddFilterRulePickerChanged(PropertyChangedEventArgs<AddFilterRulePicker> e)
            OnAddFilterRulePickerChangedImplementation(e);
            RaisePropertyChangedEvent(AddFilterRulePickerChanged, e);
        partial void OnAddFilterRulePickerChangedImplementation(PropertyChangedEventArgs<AddFilterRulePicker> e);
        // CurrentView dependency property
        /// Identifies the CurrentView dependency property key.
        private static readonly DependencyPropertyKey CurrentViewPropertyKey = DependencyProperty.RegisterReadOnly( "CurrentView", typeof(StateDescriptor<ManagementList>), typeof(ManagementList), new PropertyMetadata( null, CurrentViewProperty_PropertyChanged) );
        /// Identifies the CurrentView dependency property.
        public static readonly DependencyProperty CurrentViewProperty = CurrentViewPropertyKey.DependencyProperty;
        /// Gets or sets current view.
        [Description("Gets or sets current view.")]
        public StateDescriptor<ManagementList> CurrentView
                return (StateDescriptor<ManagementList>) GetValue(CurrentViewProperty);
                SetValue(CurrentViewPropertyKey,value);
        static private void CurrentViewProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnCurrentViewChanged( new PropertyChangedEventArgs<StateDescriptor<ManagementList>>((StateDescriptor<ManagementList>)e.OldValue, (StateDescriptor<ManagementList>)e.NewValue) );
        /// Occurs when CurrentView property changes.
        public event EventHandler<PropertyChangedEventArgs<StateDescriptor<ManagementList>>> CurrentViewChanged;
        /// Called when CurrentView property changes.
        protected virtual void OnCurrentViewChanged(PropertyChangedEventArgs<StateDescriptor<ManagementList>> e)
            OnCurrentViewChangedImplementation(e);
            RaisePropertyChangedEvent(CurrentViewChanged, e);
        partial void OnCurrentViewChangedImplementation(PropertyChangedEventArgs<StateDescriptor<ManagementList>> e);
        // Evaluator dependency property
        /// Identifies the Evaluator dependency property.
        public static readonly DependencyProperty EvaluatorProperty = DependencyProperty.Register( "Evaluator", typeof(ItemsControlFilterEvaluator), typeof(ManagementList), new PropertyMetadata( null, EvaluatorProperty_PropertyChanged) );
        /// Gets or sets the FilterEvaluator.
        [Description("Gets or sets the FilterEvaluator.")]
        public ItemsControlFilterEvaluator Evaluator
                return (ItemsControlFilterEvaluator) GetValue(EvaluatorProperty);
                SetValue(EvaluatorProperty,value);
        static private void EvaluatorProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnEvaluatorChanged( new PropertyChangedEventArgs<ItemsControlFilterEvaluator>((ItemsControlFilterEvaluator)e.OldValue, (ItemsControlFilterEvaluator)e.NewValue) );
        /// Occurs when Evaluator property changes.
        public event EventHandler<PropertyChangedEventArgs<ItemsControlFilterEvaluator>> EvaluatorChanged;
        /// Called when Evaluator property changes.
        protected virtual void OnEvaluatorChanged(PropertyChangedEventArgs<ItemsControlFilterEvaluator> e)
            OnEvaluatorChangedImplementation(e);
            RaisePropertyChangedEvent(EvaluatorChanged, e);
        partial void OnEvaluatorChangedImplementation(PropertyChangedEventArgs<ItemsControlFilterEvaluator> e);
        // FilterRulePanel dependency property
        /// Identifies the FilterRulePanel dependency property key.
        private static readonly DependencyPropertyKey FilterRulePanelPropertyKey = DependencyProperty.RegisterReadOnly( "FilterRulePanel", typeof(FilterRulePanel), typeof(ManagementList), new PropertyMetadata( null, FilterRulePanelProperty_PropertyChanged) );
        /// Identifies the FilterRulePanel dependency property.
        public static readonly DependencyProperty FilterRulePanelProperty = FilterRulePanelPropertyKey.DependencyProperty;
        /// Gets the filter rule panel.
        [Description("Gets the filter rule panel.")]
        public FilterRulePanel FilterRulePanel
                return (FilterRulePanel) GetValue(FilterRulePanelProperty);
                SetValue(FilterRulePanelPropertyKey,value);
        static private void FilterRulePanelProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnFilterRulePanelChanged( new PropertyChangedEventArgs<FilterRulePanel>((FilterRulePanel)e.OldValue, (FilterRulePanel)e.NewValue) );
        /// Occurs when FilterRulePanel property changes.
        public event EventHandler<PropertyChangedEventArgs<FilterRulePanel>> FilterRulePanelChanged;
        /// Called when FilterRulePanel property changes.
        protected virtual void OnFilterRulePanelChanged(PropertyChangedEventArgs<FilterRulePanel> e)
            OnFilterRulePanelChangedImplementation(e);
            RaisePropertyChangedEvent(FilterRulePanelChanged, e);
        partial void OnFilterRulePanelChangedImplementation(PropertyChangedEventArgs<FilterRulePanel> e);
        // IsFilterShown dependency property
        /// Identifies the IsFilterShown dependency property.
        public static readonly DependencyProperty IsFilterShownProperty = DependencyProperty.Register( "IsFilterShown", typeof(bool), typeof(ManagementList), new PropertyMetadata( BooleanBoxes.TrueBox, IsFilterShownProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether the filter is shown.
        [Description("Gets or sets a value indicating whether the filter is shown.")]
        public bool IsFilterShown
                return (bool) GetValue(IsFilterShownProperty);
                SetValue(IsFilterShownProperty,BooleanBoxes.Box(value));
        static private void IsFilterShownProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnIsFilterShownChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when IsFilterShown property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> IsFilterShownChanged;
        /// Called when IsFilterShown property changes.
        protected virtual void OnIsFilterShownChanged(PropertyChangedEventArgs<bool> e)
            OnIsFilterShownChangedImplementation(e);
            RaisePropertyChangedEvent(IsFilterShownChanged, e);
        partial void OnIsFilterShownChangedImplementation(PropertyChangedEventArgs<bool> e);
        // IsLoadingItems dependency property
        /// Identifies the IsLoadingItems dependency property.
        public static readonly DependencyProperty IsLoadingItemsProperty = DependencyProperty.Register( "IsLoadingItems", typeof(bool), typeof(ManagementList), new PropertyMetadata( BooleanBoxes.FalseBox, IsLoadingItemsProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether items are loading.
        [Description("Gets or sets a value indicating whether items are loading.")]
        public bool IsLoadingItems
                return (bool) GetValue(IsLoadingItemsProperty);
                SetValue(IsLoadingItemsProperty,BooleanBoxes.Box(value));
        static private void IsLoadingItemsProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnIsLoadingItemsChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when IsLoadingItems property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> IsLoadingItemsChanged;
        /// Called when IsLoadingItems property changes.
        protected virtual void OnIsLoadingItemsChanged(PropertyChangedEventArgs<bool> e)
            OnIsLoadingItemsChangedImplementation(e);
            RaisePropertyChangedEvent(IsLoadingItemsChanged, e);
        partial void OnIsLoadingItemsChangedImplementation(PropertyChangedEventArgs<bool> e);
        // IsSearchShown dependency property
        /// Identifies the IsSearchShown dependency property.
        public static readonly DependencyProperty IsSearchShownProperty = DependencyProperty.Register( "IsSearchShown", typeof(bool), typeof(ManagementList), new PropertyMetadata( BooleanBoxes.TrueBox, IsSearchShownProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether the search box is shown.
        [Description("Gets or sets a value indicating whether the search box is shown.")]
        public bool IsSearchShown
                return (bool) GetValue(IsSearchShownProperty);
                SetValue(IsSearchShownProperty,BooleanBoxes.Box(value));
        static private void IsSearchShownProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnIsSearchShownChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when IsSearchShown property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> IsSearchShownChanged;
        /// Called when IsSearchShown property changes.
        protected virtual void OnIsSearchShownChanged(PropertyChangedEventArgs<bool> e)
            OnIsSearchShownChangedImplementation(e);
            RaisePropertyChangedEvent(IsSearchShownChanged, e);
        partial void OnIsSearchShownChangedImplementation(PropertyChangedEventArgs<bool> e);
        // List dependency property
        /// Identifies the List dependency property key.
        private static readonly DependencyPropertyKey ListPropertyKey = DependencyProperty.RegisterReadOnly( "List", typeof(InnerList), typeof(ManagementList), new PropertyMetadata( null, ListProperty_PropertyChanged) );
        /// Identifies the List dependency property.
        public static readonly DependencyProperty ListProperty = ListPropertyKey.DependencyProperty;
        /// Gets the list.
        [Description("Gets the list.")]
        public InnerList List
                return (InnerList) GetValue(ListProperty);
                SetValue(ListPropertyKey,value);
        static private void ListProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnListChanged( new PropertyChangedEventArgs<InnerList>((InnerList)e.OldValue, (InnerList)e.NewValue) );
        /// Occurs when List property changes.
        public event EventHandler<PropertyChangedEventArgs<InnerList>> ListChanged;
        /// Called when List property changes.
        protected virtual void OnListChanged(PropertyChangedEventArgs<InnerList> e)
            OnListChangedImplementation(e);
            RaisePropertyChangedEvent(ListChanged, e);
        partial void OnListChangedImplementation(PropertyChangedEventArgs<InnerList> e);
        // SearchBox dependency property
        /// Identifies the SearchBox dependency property key.
        private static readonly DependencyPropertyKey SearchBoxPropertyKey = DependencyProperty.RegisterReadOnly( "SearchBox", typeof(SearchBox), typeof(ManagementList), new PropertyMetadata( null, SearchBoxProperty_PropertyChanged) );
        /// Identifies the SearchBox dependency property.
        public static readonly DependencyProperty SearchBoxProperty = SearchBoxPropertyKey.DependencyProperty;
        /// Gets the search box.
        [Description("Gets the search box.")]
        public SearchBox SearchBox
                return (SearchBox) GetValue(SearchBoxProperty);
                SetValue(SearchBoxPropertyKey,value);
        static private void SearchBoxProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnSearchBoxChanged( new PropertyChangedEventArgs<SearchBox>((SearchBox)e.OldValue, (SearchBox)e.NewValue) );
        /// Occurs when SearchBox property changes.
        public event EventHandler<PropertyChangedEventArgs<SearchBox>> SearchBoxChanged;
        /// Called when SearchBox property changes.
        protected virtual void OnSearchBoxChanged(PropertyChangedEventArgs<SearchBox> e)
            OnSearchBoxChangedImplementation(e);
            RaisePropertyChangedEvent(SearchBoxChanged, e);
        partial void OnSearchBoxChangedImplementation(PropertyChangedEventArgs<SearchBox> e);
        // ViewManagerUserActionState dependency property
        /// Identifies the ViewManagerUserActionState dependency property.
        public static readonly DependencyProperty ViewManagerUserActionStateProperty = DependencyProperty.Register( "ViewManagerUserActionState", typeof(UserActionState), typeof(ManagementList), new PropertyMetadata( UserActionState.Enabled, ViewManagerUserActionStateProperty_PropertyChanged) );
        /// Gets or sets the user interaction state of the view manager.
        [Description("Gets or sets the user interaction state of the view manager.")]
        public UserActionState ViewManagerUserActionState
                return (UserActionState) GetValue(ViewManagerUserActionStateProperty);
                SetValue(ViewManagerUserActionStateProperty,value);
        static private void ViewManagerUserActionStateProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnViewManagerUserActionStateChanged( new PropertyChangedEventArgs<UserActionState>((UserActionState)e.OldValue, (UserActionState)e.NewValue) );
        /// Occurs when ViewManagerUserActionState property changes.
        public event EventHandler<PropertyChangedEventArgs<UserActionState>> ViewManagerUserActionStateChanged;
        /// Called when ViewManagerUserActionState property changes.
        protected virtual void OnViewManagerUserActionStateChanged(PropertyChangedEventArgs<UserActionState> e)
            OnViewManagerUserActionStateChangedImplementation(e);
            RaisePropertyChangedEvent(ViewManagerUserActionStateChanged, e);
        partial void OnViewManagerUserActionStateChangedImplementation(PropertyChangedEventArgs<UserActionState> e);
        // ViewSaverUserActionState dependency property
        /// Identifies the ViewSaverUserActionState dependency property.
        public static readonly DependencyProperty ViewSaverUserActionStateProperty = DependencyProperty.Register( "ViewSaverUserActionState", typeof(UserActionState), typeof(ManagementList), new PropertyMetadata( UserActionState.Enabled, ViewSaverUserActionStateProperty_PropertyChanged) );
        /// Gets or sets the user interaction state of the view saver.
        [Description("Gets or sets the user interaction state of the view saver.")]
        public UserActionState ViewSaverUserActionState
                return (UserActionState) GetValue(ViewSaverUserActionStateProperty);
                SetValue(ViewSaverUserActionStateProperty,value);
        static private void ViewSaverUserActionStateProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnViewSaverUserActionStateChanged( new PropertyChangedEventArgs<UserActionState>((UserActionState)e.OldValue, (UserActionState)e.NewValue) );
        /// Occurs when ViewSaverUserActionState property changes.
        public event EventHandler<PropertyChangedEventArgs<UserActionState>> ViewSaverUserActionStateChanged;
        /// Called when ViewSaverUserActionState property changes.
        protected virtual void OnViewSaverUserActionStateChanged(PropertyChangedEventArgs<UserActionState> e)
            OnViewSaverUserActionStateChangedImplementation(e);
            RaisePropertyChangedEvent(ViewSaverUserActionStateChanged, e);
        partial void OnViewSaverUserActionStateChangedImplementation(PropertyChangedEventArgs<UserActionState> e);
            this.viewManager = WpfHelp.GetTemplateChild<ListOrganizer>(this,"PART_ViewManager");
            this.viewSaver = WpfHelp.GetTemplateChild<PickerBase>(this,"PART_ViewSaver");
        static ManagementList()
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ManagementList), new FrameworkPropertyMetadata(typeof(ManagementList)));
            CommandManager.RegisterClassCommandBinding( typeof(ManagementList), new CommandBinding( ManagementList.ClearFilterCommand, ClearFilterCommand_CommandExecuted, ClearFilterCommand_CommandCanExecute ));
            CommandManager.RegisterClassCommandBinding( typeof(ManagementList), new CommandBinding( ManagementList.SaveViewCommand, SaveViewCommand_CommandExecuted, SaveViewCommand_CommandCanExecute ));
            CommandManager.RegisterClassCommandBinding( typeof(ManagementList), new CommandBinding( ManagementList.StartFilterCommand, StartFilterCommand_CommandExecuted, StartFilterCommand_CommandCanExecute ));
            CommandManager.RegisterClassCommandBinding( typeof(ManagementList), new CommandBinding( ManagementList.StopFilterCommand, StopFilterCommand_CommandExecuted, StopFilterCommand_CommandCanExecute ));
            return new ExtendedFrameworkElementAutomationPeer(this,AutomationControlType.Pane);
