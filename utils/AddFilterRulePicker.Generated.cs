    partial class AddFilterRulePicker
        // CancelAddFilterRules routed command
        /// Closes the picker and unchecks all items in the panel.
        public static readonly RoutedCommand CancelAddFilterRulesCommand = new RoutedCommand("CancelAddFilterRules",typeof(AddFilterRulePicker));
        static private void CancelAddFilterRulesCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            AddFilterRulePicker obj = (AddFilterRulePicker) sender;
            obj.OnCancelAddFilterRulesExecuted( e );
        /// Called when CancelAddFilterRules executes.
        protected virtual void OnCancelAddFilterRulesExecuted(ExecutedRoutedEventArgs e)
            OnCancelAddFilterRulesExecutedImplementation(e);
        partial void OnCancelAddFilterRulesExecutedImplementation(ExecutedRoutedEventArgs e);
        // OkAddFilterRules routed command
        /// Closes the picker and calls AddFilterRulesCommand with the collection of checked items from the picker.
        public static readonly RoutedCommand OkAddFilterRulesCommand = new RoutedCommand("OkAddFilterRules",typeof(AddFilterRulePicker));
        static private void OkAddFilterRulesCommand_CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
            obj.OnOkAddFilterRulesCanExecute( e );
        static private void OkAddFilterRulesCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            obj.OnOkAddFilterRulesExecuted( e );
        /// Called to determine if OkAddFilterRules can execute.
        protected virtual void OnOkAddFilterRulesCanExecute(CanExecuteRoutedEventArgs e)
            OnOkAddFilterRulesCanExecuteImplementation(e);
        partial void OnOkAddFilterRulesCanExecuteImplementation(CanExecuteRoutedEventArgs e);
        /// Called when OkAddFilterRules executes.
        protected virtual void OnOkAddFilterRulesExecuted(ExecutedRoutedEventArgs e)
            OnOkAddFilterRulesExecutedImplementation(e);
        partial void OnOkAddFilterRulesExecutedImplementation(ExecutedRoutedEventArgs e);
        // AddFilterRulesCommand dependency property
        /// Identifies the AddFilterRulesCommand dependency property.
        public static readonly DependencyProperty AddFilterRulesCommandProperty = DependencyProperty.Register( "AddFilterRulesCommand", typeof(ICommand), typeof(AddFilterRulePicker), new PropertyMetadata( null, AddFilterRulesCommandProperty_PropertyChanged) );
        /// Gets or sets the command used to communicate that the action has occurred.
        [Description("Gets or sets the command used to communicate that the action has occurred.")]
        public ICommand AddFilterRulesCommand
                return (ICommand) GetValue(AddFilterRulesCommandProperty);
                SetValue(AddFilterRulesCommandProperty,value);
        static private void AddFilterRulesCommandProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            AddFilterRulePicker obj = (AddFilterRulePicker) o;
            obj.OnAddFilterRulesCommandChanged( new PropertyChangedEventArgs<ICommand>((ICommand)e.OldValue, (ICommand)e.NewValue) );
        /// Occurs when AddFilterRulesCommand property changes.
        public event EventHandler<PropertyChangedEventArgs<ICommand>> AddFilterRulesCommandChanged;
        /// Called when AddFilterRulesCommand property changes.
        protected virtual void OnAddFilterRulesCommandChanged(PropertyChangedEventArgs<ICommand> e)
            OnAddFilterRulesCommandChangedImplementation(e);
            RaisePropertyChangedEvent(AddFilterRulesCommandChanged, e);
        partial void OnAddFilterRulesCommandChangedImplementation(PropertyChangedEventArgs<ICommand> e);
        // AddFilterRulesCommandTarget dependency property
        /// Identifies the AddFilterRulesCommandTarget dependency property.
        public static readonly DependencyProperty AddFilterRulesCommandTargetProperty = DependencyProperty.Register( "AddFilterRulesCommandTarget", typeof(IInputElement), typeof(AddFilterRulePicker), new PropertyMetadata( null, AddFilterRulesCommandTargetProperty_PropertyChanged) );
        /// Gets or sets a target of the Command.
        [Description("Gets or sets a target of the Command.")]
        public IInputElement AddFilterRulesCommandTarget
                return (IInputElement) GetValue(AddFilterRulesCommandTargetProperty);
                SetValue(AddFilterRulesCommandTargetProperty,value);
        static private void AddFilterRulesCommandTargetProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnAddFilterRulesCommandTargetChanged( new PropertyChangedEventArgs<IInputElement>((IInputElement)e.OldValue, (IInputElement)e.NewValue) );
        /// Occurs when AddFilterRulesCommandTarget property changes.
        public event EventHandler<PropertyChangedEventArgs<IInputElement>> AddFilterRulesCommandTargetChanged;
        /// Called when AddFilterRulesCommandTarget property changes.
        protected virtual void OnAddFilterRulesCommandTargetChanged(PropertyChangedEventArgs<IInputElement> e)
            OnAddFilterRulesCommandTargetChangedImplementation(e);
            RaisePropertyChangedEvent(AddFilterRulesCommandTargetChanged, e);
        partial void OnAddFilterRulesCommandTargetChangedImplementation(PropertyChangedEventArgs<IInputElement> e);
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register( "IsOpen", typeof(bool), typeof(AddFilterRulePicker), new PropertyMetadata( BooleanBoxes.FalseBox, IsOpenProperty_PropertyChanged) );
        static AddFilterRulePicker()
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AddFilterRulePicker), new FrameworkPropertyMetadata(typeof(AddFilterRulePicker)));
            CommandManager.RegisterClassCommandBinding( typeof(AddFilterRulePicker), new CommandBinding( AddFilterRulePicker.CancelAddFilterRulesCommand, CancelAddFilterRulesCommand_CommandExecuted ));
            CommandManager.RegisterClassCommandBinding( typeof(AddFilterRulePicker), new CommandBinding( AddFilterRulePicker.OkAddFilterRulesCommand, OkAddFilterRulesCommand_CommandExecuted, OkAddFilterRulesCommand_CommandCanExecute ));
