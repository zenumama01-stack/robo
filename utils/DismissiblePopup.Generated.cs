#region StyleCop Suppression - generated code
    /// A popup which child controls can signal to be dismissed.
    /// If a control wants to dismiss the popup then they should execute the DismissPopupCommand on a target in the popup window.
    [Localizability(LocalizationCategory.None)]
    partial class DismissiblePopup
        // DismissPopup routed command
        /// A command which child controls can use to tell the popup to close.
        public static readonly RoutedCommand DismissPopupCommand = new RoutedCommand("DismissPopup",typeof(DismissiblePopup));
        static private void DismissPopupCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            DismissiblePopup obj = (DismissiblePopup) sender;
            obj.OnDismissPopupExecuted( e );
        /// Called when DismissPopup executes.
        protected virtual void OnDismissPopupExecuted(ExecutedRoutedEventArgs e)
            OnDismissPopupExecutedImplementation(e);
        partial void OnDismissPopupExecutedImplementation(ExecutedRoutedEventArgs e);
        // CloseOnEscape dependency property
        /// Identifies the CloseOnEscape dependency property.
        public static readonly DependencyProperty CloseOnEscapeProperty = DependencyProperty.Register( "CloseOnEscape", typeof(bool), typeof(DismissiblePopup), new PropertyMetadata( BooleanBoxes.TrueBox, CloseOnEscapeProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether the popup closes when ESC is pressed.
        [Bindable(true)]
        [Category("Common Properties")]
        [Description("Gets or sets a value indicating whether the popup closes when ESC is pressed.")]
        public bool CloseOnEscape
                return (bool) GetValue(CloseOnEscapeProperty);
                SetValue(CloseOnEscapeProperty,BooleanBoxes.Box(value));
        static private void CloseOnEscapeProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            DismissiblePopup obj = (DismissiblePopup) o;
            obj.OnCloseOnEscapeChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when CloseOnEscape property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> CloseOnEscapeChanged;
        /// Called when CloseOnEscape property changes.
        protected virtual void OnCloseOnEscapeChanged(PropertyChangedEventArgs<bool> e)
            OnCloseOnEscapeChangedImplementation(e);
            RaisePropertyChangedEvent(CloseOnEscapeChanged, e);
        partial void OnCloseOnEscapeChangedImplementation(PropertyChangedEventArgs<bool> e);
        // FocusChildOnOpen dependency property
        /// Identifies the FocusChildOnOpen dependency property.
        public static readonly DependencyProperty FocusChildOnOpenProperty = DependencyProperty.Register( "FocusChildOnOpen", typeof(bool), typeof(DismissiblePopup), new PropertyMetadata( BooleanBoxes.TrueBox, FocusChildOnOpenProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether focus should be set on the child when the popup opens.
        [Description("Gets or sets a value indicating whether focus should be set on the child when the popup opens.")]
        public bool FocusChildOnOpen
                return (bool) GetValue(FocusChildOnOpenProperty);
                SetValue(FocusChildOnOpenProperty,BooleanBoxes.Box(value));
        static private void FocusChildOnOpenProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnFocusChildOnOpenChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when FocusChildOnOpen property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> FocusChildOnOpenChanged;
        /// Called when FocusChildOnOpen property changes.
        protected virtual void OnFocusChildOnOpenChanged(PropertyChangedEventArgs<bool> e)
            OnFocusChildOnOpenChangedImplementation(e);
            RaisePropertyChangedEvent(FocusChildOnOpenChanged, e);
        partial void OnFocusChildOnOpenChangedImplementation(PropertyChangedEventArgs<bool> e);
        // SetFocusOnClose dependency property
        /// Identifies the SetFocusOnClose dependency property.
        public static readonly DependencyProperty SetFocusOnCloseProperty = DependencyProperty.Register( "SetFocusOnClose", typeof(bool), typeof(DismissiblePopup), new PropertyMetadata( BooleanBoxes.FalseBox, SetFocusOnCloseProperty_PropertyChanged) );
        /// Indicates whether the focus returns to either a defined by the FocusOnCloseTarget dependency property UIElement or PlacementTarget or not.
        [Description("Indicates whether the focus returns to either a defined by the FocusOnCloseTarget dependency property UIElement or PlacementTarget or not.")]
        public bool SetFocusOnClose
                return (bool) GetValue(SetFocusOnCloseProperty);
                SetValue(SetFocusOnCloseProperty,BooleanBoxes.Box(value));
        static private void SetFocusOnCloseProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnSetFocusOnCloseChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when SetFocusOnClose property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> SetFocusOnCloseChanged;
        /// Called when SetFocusOnClose property changes.
        protected virtual void OnSetFocusOnCloseChanged(PropertyChangedEventArgs<bool> e)
            OnSetFocusOnCloseChangedImplementation(e);
            RaisePropertyChangedEvent(SetFocusOnCloseChanged, e);
        partial void OnSetFocusOnCloseChangedImplementation(PropertyChangedEventArgs<bool> e);
        // SetFocusOnCloseElement dependency property
        /// Identifies the SetFocusOnCloseElement dependency property.
        public static readonly DependencyProperty SetFocusOnCloseElementProperty = DependencyProperty.Register( "SetFocusOnCloseElement", typeof(UIElement), typeof(DismissiblePopup), new PropertyMetadata( null, SetFocusOnCloseElementProperty_PropertyChanged) );
        /// If the SetFocusOnClose property is set True and this property is set to a valid UIElement, focus returns to this UIElement after the DismissiblePopup is closed.
        [Description("If the SetFocusOnClose property is set True and this property is set to a valid UIElement, focus returns to this UIElement after the DismissiblePopup is closed.")]
        public UIElement SetFocusOnCloseElement
                return (UIElement) GetValue(SetFocusOnCloseElementProperty);
                SetValue(SetFocusOnCloseElementProperty,value);
        static private void SetFocusOnCloseElementProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnSetFocusOnCloseElementChanged( new PropertyChangedEventArgs<UIElement>((UIElement)e.OldValue, (UIElement)e.NewValue) );
        /// Occurs when SetFocusOnCloseElement property changes.
        public event EventHandler<PropertyChangedEventArgs<UIElement>> SetFocusOnCloseElementChanged;
        /// Called when SetFocusOnCloseElement property changes.
        protected virtual void OnSetFocusOnCloseElementChanged(PropertyChangedEventArgs<UIElement> e)
            OnSetFocusOnCloseElementChangedImplementation(e);
            RaisePropertyChangedEvent(SetFocusOnCloseElementChanged, e);
        partial void OnSetFocusOnCloseElementChangedImplementation(PropertyChangedEventArgs<UIElement> e);
        /// Called when a property changes.
        private void RaisePropertyChangedEvent<T>(EventHandler<PropertyChangedEventArgs<T>> eh, PropertyChangedEventArgs<T> e)
            if (eh != null)
                eh(this,e);
        // Static constructor
        /// Called when the type is initialized.
        static DismissiblePopup()
            CommandManager.RegisterClassCommandBinding( typeof(DismissiblePopup), new CommandBinding( DismissiblePopup.DismissPopupCommand, DismissPopupCommand_CommandExecuted ));
            StaticConstructorImplementation();
        static partial void StaticConstructorImplementation();
