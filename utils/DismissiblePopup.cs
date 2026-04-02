using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
    /// <content>
    /// Partial class implementation for DismissiblePopup control.
    /// </content>
    public partial class DismissiblePopup : Popup
        /// Constructs an instance of DismissablePopup.
        public DismissiblePopup() : base()
            // nothing
        private delegate void FocusChildDelegate();
        /// Responds to the condition in which the value of the IsOpen property changes from false to true.
        /// <param name="e">The event arguments.</param>
        protected override void OnOpened(EventArgs e)
            base.OnOpened(e);
            if (this.FocusChildOnOpen)
                this.Dispatcher.BeginInvoke(
                                            System.Windows.Threading.DispatcherPriority.Loaded,
                                            new FocusChildDelegate(this.FocusChild));
            this.SetupAutomationIdBinding();
        /// Responds when the value of the IsOpen property changes from to true to false.
        protected override void OnClosed(EventArgs e)
            base.OnClosed(e);
            if (this.SetFocusOnClose)
                // Find a control to set focus on.
                if (this.SetFocusOnCloseElement != null)
                    // The focus target is set explicitly.
                    this.SetFocus(this.SetFocusOnCloseElement);
                else if (this.PlacementTarget != null)
                    // Use PlacementTarget as a first chance option.
                    this.SetFocus(this.PlacementTarget);
                    // Use parent UIObject when neither FocusOnCloseTarget nor PlacementTarget is set.
                    UIElement parent = this.Parent as UIElement;
                    if (parent != null)
                        this.SetFocus(parent);
        private void SetFocus(UIElement element)
            if (element.Focusable)
                element.Focus();
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        private void SetupAutomationIdBinding()
            var popupRoot = this.FindPopupRoot();
            var binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath(AutomationProperties.AutomationIdProperty);
            popupRoot.SetBinding(AutomationProperties.AutomationIdProperty, binding);
        private FrameworkElement FindPopupRoot()
            DependencyObject element = this.Child;
            while (element.GetType().Name.Equals("PopupRoot", StringComparison.Ordinal) == false)
                element = VisualTreeHelper.GetParent(element);
            Debug.Assert(element != null, "element not null");
            return (FrameworkElement)element;
        /// Provides class handling for the KeyDown routed event that occurs when the user presses a key while this control has focus.
        /// <param name="e">The event data.</param>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
            ////
            // Close the popup if ESC is pressed
            if (e.Key == System.Windows.Input.Key.Escape && this.CloseOnEscape)
                this.IsOpen = false;
                base.OnKeyDown(e);
        partial void OnDismissPopupExecutedImplementation(ExecutedRoutedEventArgs e)
        private void FocusChild()
            if (this.Child != null)
                this.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
