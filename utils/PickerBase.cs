using System.Windows.Threading;
    /// Implements a re-usable base component useful for showing
    /// Picker-like controls.
    public partial class PickerBase : HeaderedContentControl
        /// Creates a new instance of the PickerBase class.
        public PickerBase()
        partial void OnCloseDropDownExecutedImplementation(ExecutedRoutedEventArgs e)
        #region DropDownButtonTemplate Changed
        partial void OnDropDownButtonTemplateChangedImplementation(PropertyChangedEventArgs<ControlTemplate> e)
            this.ApplyDropDownButtonTemplate();
        private void ApplyDropDownButtonTemplate()
                this.Loaded += this.PickerBase_Loaded_ApplyDropDownButtonTemplate;
            if (this.DropDownButtonTemplate != null && !ReferenceEquals(this.dropDownButton.Template, this.DropDownButtonTemplate))
                this.dropDownButton.Template = this.DropDownButtonTemplate;
        private void PickerBase_Loaded_ApplyDropDownButtonTemplate(object sender, RoutedEventArgs e)
            this.Loaded -= this.PickerBase_Loaded_ApplyDropDownButtonTemplate;
        #endregion DropDownButtonTemplate Changed
        #region DropDown IsOpen Handlers
        private void DropDown_Opened(object sender, EventArgs e)
            this.FocusDropDown();
        private void FocusDropDown()
            if (!this.dropDown.IsLoaded)
                this.dropDown.Loaded += this.DropDown_Loaded_FocusDropDown;
            if (this.dropDown.Child != null && !this.dropDown.IsAncestorOf((DependencyObject)Keyboard.FocusedElement))
                this.dropDown.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        private void DropDown_Loaded_FocusDropDown(object sender, RoutedEventArgs e)
            this.Loaded -= this.DropDown_Loaded_FocusDropDown;
        private void DropDown_Closed(object sender, EventArgs e)
            if (this.dropDown.IsKeyboardFocusWithin || Keyboard.FocusedElement == null)
                this.dropDownButton.Focus();
        #endregion DropDown IsOpen Handlers
        #region Apply Template
            this.dropDown.Opened += this.DropDown_Opened;
            this.dropDown.Closed += this.DropDown_Closed;
            if (this.dropDown != null)
                this.dropDown.Opened -= this.DropDown_Opened;
                this.dropDown.Closed -= this.DropDown_Closed;
        #endregion Apply Template
