    /// Represents a control that parses search text to return a filter expression.
    partial class SearchBox
        // ClearText routed command
        /// Clears the search text.
        public static readonly RoutedCommand ClearTextCommand = new RoutedCommand("ClearText",typeof(SearchBox));
        static private void ClearTextCommand_CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
            SearchBox obj = (SearchBox) sender;
            obj.OnClearTextCanExecute( e );
        static private void ClearTextCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            obj.OnClearTextExecuted( e );
        /// Called to determine if ClearText can execute.
        protected virtual void OnClearTextCanExecute(CanExecuteRoutedEventArgs e)
            OnClearTextCanExecuteImplementation(e);
        partial void OnClearTextCanExecuteImplementation(CanExecuteRoutedEventArgs e);
        /// Called when ClearText executes.
        protected virtual void OnClearTextExecuted(ExecutedRoutedEventArgs e)
            OnClearTextExecutedImplementation(e);
        partial void OnClearTextExecutedImplementation(ExecutedRoutedEventArgs e);
        public static readonly DependencyProperty BackgroundTextProperty = DependencyProperty.Register( "BackgroundText", typeof(string), typeof(SearchBox), new PropertyMetadata( UICultureResources.SearchBox_BackgroundText, BackgroundTextProperty_PropertyChanged) );
        /// Gets or sets the background text of the search box.
        [Description("Gets or sets the background text of the search box.")]
            SearchBox obj = (SearchBox) o;
        // Text dependency property
        /// Identifies the Text dependency property.
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof(string), typeof(SearchBox), new PropertyMetadata( string.Empty, TextProperty_PropertyChanged) );
        /// Gets or sets the text contents of the search box.
        [Description("Gets or sets the text contents of the search box.")]
        public string Text
                return (string) GetValue(TextProperty);
                SetValue(TextProperty,value);
        static private void TextProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnTextChanged( new PropertyChangedEventArgs<string>((string)e.OldValue, (string)e.NewValue) );
        /// Occurs when Text property changes.
        public event EventHandler<PropertyChangedEventArgs<string>> TextChanged;
        /// Called when Text property changes.
        protected virtual void OnTextChanged(PropertyChangedEventArgs<string> e)
            OnTextChangedImplementation(e);
            RaisePropertyChangedEvent(TextChanged, e);
        partial void OnTextChangedImplementation(PropertyChangedEventArgs<string> e);
        static SearchBox()
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
            CommandManager.RegisterClassCommandBinding( typeof(SearchBox), new CommandBinding( SearchBox.ClearTextCommand, ClearTextCommand_CommandExecuted, ClearTextCommand_CommandCanExecute ));
