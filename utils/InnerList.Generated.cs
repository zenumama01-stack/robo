    /// List control for Inner Applications.  This Control supports grouping, sorting, filtering and GUI Virtualization through DataBinding.
    partial class InnerList
        // Copy routed command
        static private void CopyCommand_CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
            InnerList obj = (InnerList) sender;
            obj.OnCopyCanExecute( e );
        static private void CopyCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            obj.OnCopyExecuted( e );
        /// Called to determine if Copy can execute.
        protected virtual void OnCopyCanExecute(CanExecuteRoutedEventArgs e)
            OnCopyCanExecuteImplementation(e);
        partial void OnCopyCanExecuteImplementation(CanExecuteRoutedEventArgs e);
        /// Called when Copy executes.
        /// When executed, the currently selected items are copied to the clipboard.
        protected virtual void OnCopyExecuted(ExecutedRoutedEventArgs e)
            OnCopyExecutedImplementation(e);
        partial void OnCopyExecutedImplementation(ExecutedRoutedEventArgs e);
        // AutoGenerateColumns dependency property
        /// Identifies the AutoGenerateColumns dependency property.
        public static readonly DependencyProperty AutoGenerateColumnsProperty = DependencyProperty.Register( "AutoGenerateColumns", typeof(bool), typeof(InnerList), new PropertyMetadata( BooleanBoxes.FalseBox, AutoGenerateColumnsProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether this list&apos;s columns should be automatically generated based on its data.
        [Description("Gets or sets a value indicating whether this list's columns should be automatically generated based on its data.")]
        public bool AutoGenerateColumns
                return (bool) GetValue(AutoGenerateColumnsProperty);
                SetValue(AutoGenerateColumnsProperty,BooleanBoxes.Box(value));
        static private void AutoGenerateColumnsProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            InnerList obj = (InnerList) o;
            obj.OnAutoGenerateColumnsChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when AutoGenerateColumns property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> AutoGenerateColumnsChanged;
        /// Called when AutoGenerateColumns property changes.
        protected virtual void OnAutoGenerateColumnsChanged(PropertyChangedEventArgs<bool> e)
            OnAutoGenerateColumnsChangedImplementation(e);
            RaisePropertyChangedEvent(AutoGenerateColumnsChanged, e);
        partial void OnAutoGenerateColumnsChangedImplementation(PropertyChangedEventArgs<bool> e);
        // IsGroupsExpanded dependency property
        /// Identifies the IsGroupsExpanded dependency property.
        public static readonly DependencyProperty IsGroupsExpandedProperty = DependencyProperty.Register( "IsGroupsExpanded", typeof(bool), typeof(InnerList), new PropertyMetadata( BooleanBoxes.FalseBox, IsGroupsExpandedProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether is groups expanded or not.
        [Description("Gets or sets a value indicating whether is groups expanded or not.")]
        public bool IsGroupsExpanded
                return (bool) GetValue(IsGroupsExpandedProperty);
                SetValue(IsGroupsExpandedProperty,BooleanBoxes.Box(value));
        static private void IsGroupsExpandedProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnIsGroupsExpandedChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when IsGroupsExpanded property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> IsGroupsExpandedChanged;
        /// Called when IsGroupsExpanded property changes.
        protected virtual void OnIsGroupsExpandedChanged(PropertyChangedEventArgs<bool> e)
            OnIsGroupsExpandedChangedImplementation(e);
            RaisePropertyChangedEvent(IsGroupsExpandedChanged, e);
        partial void OnIsGroupsExpandedChangedImplementation(PropertyChangedEventArgs<bool> e);
        // IsPrimarySortColumn dependency property
        /// Identifies the IsPrimarySortColumn dependency property key.
        private static readonly DependencyPropertyKey IsPrimarySortColumnPropertyKey = DependencyProperty.RegisterAttachedReadOnly( "IsPrimarySortColumn", typeof(bool), typeof(InnerList), new PropertyMetadata( BooleanBoxes.FalseBox, IsPrimarySortColumnProperty_PropertyChanged) );
        /// Identifies the IsPrimarySortColumn dependency property.
        public static readonly DependencyProperty IsPrimarySortColumnProperty = IsPrimarySortColumnPropertyKey.DependencyProperty;
        /// Gets whether a column is the primary sort in a list.
        /// The value of IsPrimarySortColumn that is attached to element.
        static public bool GetIsPrimarySortColumn(DependencyObject element)
            return (bool) element.GetValue(IsPrimarySortColumnProperty);
        /// Sets whether a column is the primary sort in a list.
        static private void SetIsPrimarySortColumn(DependencyObject element, bool value)
            element.SetValue(IsPrimarySortColumnPropertyKey,BooleanBoxes.Box(value));
        static private void IsPrimarySortColumnProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            IsPrimarySortColumnProperty_PropertyChangedImplementation(o, e);
        static partial void IsPrimarySortColumnProperty_PropertyChangedImplementation(DependencyObject o, DependencyPropertyChangedEventArgs e);
        static InnerList()
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InnerList), new FrameworkPropertyMetadata(typeof(InnerList)));
            CommandManager.RegisterClassCommandBinding( typeof(InnerList), new CommandBinding( ApplicationCommands.Copy, CopyCommand_CommandExecuted, CopyCommand_CommandCanExecute ));
