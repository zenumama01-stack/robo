    /// Partial class implementation for PopupControlButton control.
    public partial class PopupControlButton : ExpanderButton
        private bool isClickInProgress = false;
        /// Tooltip to show to expand.
        protected override string ExpandToolTip
            get { return XamlLocalizableResources.AutoResXGen_ManagementList2_ToolTip_132; }
        /// Constructs an instance of PopupControlButton.
        public PopupControlButton()
        /// Called when the IsChecked property becomes true.
        /// <param name="e">The event data for the Checked event.</param>
        protected override void OnChecked(RoutedEventArgs e)
            base.OnChecked(e);
            this.UpdateIsPopupOpen();
        /// Called when the IsChecked property becomes false.
        /// <param name="e">The event data for the Unchecked event.</param>
        protected override void OnUnchecked(RoutedEventArgs e)
            base.OnUnchecked(e);
        private void UpdateIsPopupOpen()
            this.IsPopupOpen = this.IsChecked.GetValueOrDefault();
        /// Invoked when an unhandled PreviewMouseLeftButtonUp routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// <param name="e">The MouseButtonEventArgs that contains the event data. The event data reports that the left mouse button was released.</param>
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
            // If the mouse is captured then we need to finish updating state after the current event it processed.
            if (this.IsMouseCaptured && this.isClickInProgress)
                this.isClickInProgress = false;
                this.ReleaseMouseCapture();
                                         new UpdateIsCheckedDelegate(this.UpdateIsChecked),
                                         DispatcherPriority.Input,
            base.OnPreviewMouseLeftButtonUp(e);
        private delegate void UpdateIsCheckedDelegate();
        partial void OnIsPopupOpenChangedImplementation(PropertyChangedEventArgs<bool> e)
            // If it looks like the button is in the act of being pressed,
            // then we don't want to update the IsChecked since the button
            // push will do it.
            // However we do need to handle the case where the mouse down is on the
            // button, but mouse up isn't.
            if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed && this.IsPopupOpen == false)
                if (this.GetIsMouseReallyOver())
                    this.isClickInProgress = true;
                    this.CaptureMouse();
            if (this.isClickInProgress == false)
                this.UpdateIsChecked();
        private bool GetIsMouseReallyOver()
            Point pos = Mouse.PrimaryDevice.GetPosition(this);
            if ((pos.X >= 0) && (pos.X <= ActualWidth) && (pos.Y >= 0) && (pos.Y <= ActualHeight))
        private void UpdateIsChecked()
            this.IsChecked = this.IsPopupOpen;
