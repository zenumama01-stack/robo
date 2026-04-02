    /// A toggle button which controls is a popup is open or not.
    partial class PopupControlButton
        // IsPopupOpen dependency property
        /// Identifies the IsPopupOpen dependency property.
        public static readonly DependencyProperty IsPopupOpenProperty = DependencyProperty.Register( "IsPopupOpen", typeof(bool), typeof(PopupControlButton), new FrameworkPropertyMetadata( BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsPopupOpenProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether the popup is open or not.
        /// The Popup.IsOpen property should be two-way bound to this property.
        [Description("Gets or sets a value indicating whether the popup is open or not.")]
        public bool IsPopupOpen
                return (bool) GetValue(IsPopupOpenProperty);
                SetValue(IsPopupOpenProperty,BooleanBoxes.Box(value));
        static private void IsPopupOpenProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            PopupControlButton obj = (PopupControlButton) o;
            obj.OnIsPopupOpenChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when IsPopupOpen property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> IsPopupOpenChanged;
        /// Called when IsPopupOpen property changes.
        protected virtual void OnIsPopupOpenChanged(PropertyChangedEventArgs<bool> e)
            OnIsPopupOpenChangedImplementation(e);
            RaisePropertyChangedEvent(IsPopupOpenChanged, e);
        partial void OnIsPopupOpenChangedImplementation(PropertyChangedEventArgs<bool> e);
