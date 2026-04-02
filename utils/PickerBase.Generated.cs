    /// This control provides basic functionality for Picker-like controls.
    ///     PART_DropDown - A required template part which must be of type DismissiblePopup.  The dropdown which hosts the picker.
    ///     PART_DropDownButton - A required template part which must be of type ToggleButton.  The ToggleButton which controls whether the dropdown is open.
    [TemplatePart(Name="PART_DropDown", Type=typeof(DismissiblePopup))]
    [TemplatePart(Name="PART_DropDownButton", Type=typeof(ToggleButton))]
    partial class PickerBase
        private DismissiblePopup dropDown;
        private ToggleButton dropDownButton;
        // CloseDropDown routed command
        /// Informs the PickerBase that it should close the dropdown.
        public static readonly RoutedCommand CloseDropDownCommand = new RoutedCommand("CloseDropDown",typeof(PickerBase));
        static private void CloseDropDownCommand_CommandExecuted(object sender, ExecutedRoutedEventArgs e)
            PickerBase obj = (PickerBase) sender;
            obj.OnCloseDropDownExecuted( e );
        /// Called when CloseDropDown executes.
        protected virtual void OnCloseDropDownExecuted(ExecutedRoutedEventArgs e)
            OnCloseDropDownExecutedImplementation(e);
        partial void OnCloseDropDownExecutedImplementation(ExecutedRoutedEventArgs e);
        public static readonly DependencyProperty DropDownButtonTemplateProperty = DependencyProperty.Register( "DropDownButtonTemplate", typeof(ControlTemplate), typeof(PickerBase), new PropertyMetadata( null, DropDownButtonTemplateProperty_PropertyChanged) );
            PickerBase obj = (PickerBase) o;
        public static readonly DependencyProperty DropDownStyleProperty = DependencyProperty.Register( "DropDownStyle", typeof(Style), typeof(PickerBase), new PropertyMetadata( null, DropDownStyleProperty_PropertyChanged) );
        // IsOpen dependency property
        /// Identifies the IsOpen dependency property.
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register( "IsOpen", typeof(bool), typeof(PickerBase), new PropertyMetadata( BooleanBoxes.FalseBox, IsOpenProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether the Popup is visible.
        [Description("Gets or sets a value indicating whether the Popup is visible.")]
        public bool IsOpen
                return (bool) GetValue(IsOpenProperty);
                SetValue(IsOpenProperty,BooleanBoxes.Box(value));
        static private void IsOpenProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnIsOpenChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when IsOpen property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> IsOpenChanged;
        /// Called when IsOpen property changes.
        protected virtual void OnIsOpenChanged(PropertyChangedEventArgs<bool> e)
            OnIsOpenChangedImplementation(e);
            RaisePropertyChangedEvent(IsOpenChanged, e);
        partial void OnIsOpenChangedImplementation(PropertyChangedEventArgs<bool> e);
            this.dropDown = WpfHelp.GetTemplateChild<DismissiblePopup>(this,"PART_DropDown");
            this.dropDownButton = WpfHelp.GetTemplateChild<ToggleButton>(this,"PART_DropDownButton");
        static PickerBase()
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PickerBase), new FrameworkPropertyMetadata(typeof(PickerBase)));
            CommandManager.RegisterClassCommandBinding( typeof(PickerBase), new CommandBinding( PickerBase.CloseDropDownCommand, CloseDropDownCommand_CommandExecuted ));
