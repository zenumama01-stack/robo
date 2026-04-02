    /// Partial class implementation for ListOrganizerItem control.
    public partial class ListOrganizerItem : Control
        private string startingText;
        private FrameworkElement templatedParent;
        /// Creates a new instance of the ListOrganizerItem class.
        public ListOrganizerItem()
        /// Gets a value indicating whether the item is in edit mode.
        public bool IsInEditMode
                return (this.renameButton != null) ? this.renameButton.IsChecked.Value : false;
        /// Selects the current item.
        public void Select()
            if (!this.IsLoaded)
                this.Loaded += this.ListOrganizerItem_Loaded_SelectItem;
                this.ApplyTemplate();
            CommandHelper.ExecuteCommand(this.linkButton.Command, this.linkButton.CommandParameter, this.linkButton.CommandTarget);
        /// Allows modification of the item.
        public void Rename()
            this.renameButton.IsChecked = true;
        /// Deletes the item.
        public void Delete()
            CommandHelper.ExecuteCommand(this.deleteButton.Command, this.deleteButton.CommandParameter, this.deleteButton.CommandTarget);
        /// Provides class handling for the KeyDown routed event that
        /// occurs when the user presses a key while this control has focus.
            if (this.IsInEditMode)
            switch (e.Key)
                case Key.Insert:
                    this.Rename();
                case Key.Delete:
                    this.Delete();
                case Key.Enter:
                case Key.Space:
                    this.Select();
        private void TemplatedParent_OnKeyDown(object sender, KeyEventArgs e)
            this.OnKeyDown(e);
        private void ListOrganizerItem_Loaded_SelectItem(object sender, RoutedEventArgs e)
            this.Loaded -= this.ListOrganizerItem_Loaded_SelectItem;
        #region EditBox Event Handlers
        private void EditBox_LostFocus(object sender, RoutedEventArgs e)
            this.ChangeFromEditToDisplayMode();
        private void EditBox_KeyDown(object sender, KeyEventArgs e)
                case Key.Escape:
                    this.RevertTextAndChangeFromEditToDisplayMode();
        private void RevertTextAndChangeFromEditToDisplayMode()
            this.editBox.Text = this.startingText;
        private void ChangeFromEditToDisplayMode()
            // NOTE : This is to resolve a race condition where clicking
            // on the rename button causes the edit box to change and
            // then have re-toggle.
            DependencyObject d = Mouse.DirectlyOver as DependencyObject;
            if (d == null || !(this.renameButton.IsAncestorOf(d) && Mouse.LeftButton == MouseButtonState.Pressed))
                this.renameButton.IsChecked = false;
            if (!this.IsKeyboardFocusWithin)
                this.Focus();
        private void EditBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            if (this.editBox.IsVisible)
                this.startingText = this.editBox.Text;
                this.editBox.Focus();
                this.editBox.SelectAll();
        #endregion EditBox Event Handlers
        partial void OnTextContentPropertyNameChangedImplementation(PropertyChangedEventArgs<string> e)
            this.UpdateTextContentBindings();
        private void UpdateTextContentBindings()
                this.Loaded += this.ListOrganizerItem_Loaded_UpdateTextContentBindings;
            if (!string.IsNullOrEmpty(this.TextContentPropertyName))
                Binding b = new Binding(this.TextContentPropertyName);
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                this.linkButton.SetBinding(Button.ContentProperty, b);
                this.editBox.SetBinding(TextBox.TextProperty, b);
                BindingOperations.ClearBinding(this.linkButton, Button.ContentProperty);
                BindingOperations.ClearBinding(this.editBox, TextBox.TextProperty);
        private void ListOrganizerItem_Loaded_UpdateTextContentBindings(object sender, RoutedEventArgs e)
            this.Loaded -= this.ListOrganizerItem_Loaded_UpdateTextContentBindings;
        #region OnApplyTemplate Helpers
        partial void PreOnApplyTemplate()
            this.DetachFromVisualTree();
        partial void PostOnApplyTemplate()
            this.AttachToVisualTree();
        private void AttachToVisualTree()
            this.editBox.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.EditBox_IsVisibleChanged);
            this.editBox.KeyDown += this.EditBox_KeyDown;
            this.editBox.LostFocus += this.EditBox_LostFocus;
            this.templatedParent = this.TemplatedParent as FrameworkElement;
            if (this.templatedParent != null)
                this.templatedParent.KeyDown += this.TemplatedParent_OnKeyDown;
        private void DetachFromVisualTree()
            if (this.editBox != null)
                this.editBox.IsVisibleChanged -= this.EditBox_IsVisibleChanged;
                this.editBox.KeyDown -= this.EditBox_KeyDown;
                this.editBox.LostFocus -= this.EditBox_LostFocus;
                this.templatedParent.KeyDown -= this.TemplatedParent_OnKeyDown;
        #endregion OnApplyTemplate Helpers
