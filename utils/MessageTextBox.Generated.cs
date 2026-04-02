    /// A TextBox which shows a user provided text when its empty.
    partial class MessageTextBox
        // BackgroundText dependency property
        /// Identifies the BackgroundText dependency property.
        public static readonly DependencyProperty BackgroundTextProperty = DependencyProperty.Register( "BackgroundText", typeof(string), typeof(MessageTextBox), new PropertyMetadata( string.Empty, BackgroundTextProperty_PropertyChanged) );
        /// Gets or sets a value for text presented to user when TextBox is empty.
        [Description("Gets or sets a value for text presented to user when TextBox is empty.")]
        public string BackgroundText
                return (string) GetValue(BackgroundTextProperty);
                SetValue(BackgroundTextProperty,value);
        static private void BackgroundTextProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            MessageTextBox obj = (MessageTextBox) o;
            obj.OnBackgroundTextChanged( new PropertyChangedEventArgs<string>((string)e.OldValue, (string)e.NewValue) );
        /// Occurs when BackgroundText property changes.
        public event EventHandler<PropertyChangedEventArgs<string>> BackgroundTextChanged;
        /// Called when BackgroundText property changes.
        protected virtual void OnBackgroundTextChanged(PropertyChangedEventArgs<string> e)
            OnBackgroundTextChangedImplementation(e);
            RaisePropertyChangedEvent(BackgroundTextChanged, e);
        partial void OnBackgroundTextChangedImplementation(PropertyChangedEventArgs<string> e);
        // IsBackgroundTextShown dependency property
        /// Identifies the IsBackgroundTextShown dependency property key.
        private static readonly DependencyPropertyKey IsBackgroundTextShownPropertyKey = DependencyProperty.RegisterReadOnly( "IsBackgroundTextShown", typeof(bool), typeof(MessageTextBox), new PropertyMetadata( BooleanBoxes.TrueBox, IsBackgroundTextShownProperty_PropertyChanged) );
        /// Identifies the IsBackgroundTextShown dependency property.
        public static readonly DependencyProperty IsBackgroundTextShownProperty = IsBackgroundTextShownPropertyKey.DependencyProperty;
        /// Gets a value indicating if the background text is being shown.
        [Description("Gets a value indicating if the background text is being shown.")]
        public bool IsBackgroundTextShown
                return (bool) GetValue(IsBackgroundTextShownProperty);
            private set
                SetValue(IsBackgroundTextShownPropertyKey,BooleanBoxes.Box(value));
        static private void IsBackgroundTextShownProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnIsBackgroundTextShownChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when IsBackgroundTextShown property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> IsBackgroundTextShownChanged;
        /// Called when IsBackgroundTextShown property changes.
        protected virtual void OnIsBackgroundTextShownChanged(PropertyChangedEventArgs<bool> e)
            OnIsBackgroundTextShownChangedImplementation(e);
            RaisePropertyChangedEvent(IsBackgroundTextShownChanged, e);
        partial void OnIsBackgroundTextShownChangedImplementation(PropertyChangedEventArgs<bool> e);
        static MessageTextBox()
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MessageTextBox), new FrameworkPropertyMetadata(typeof(MessageTextBox)));
