    /// Toggle button with images to represent enabled and disabled states.
    public partial class ImageToggleButton : ImageButtonBase
        /// Value indicating the button is checked.
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ImageToggleButton));
        /// Initializes a new instance of the ImageToggleButton class.
        public ImageToggleButton()
        /// Gets or sets a value indicating whether the button is checked.
            get { return (bool)GetValue(ImageToggleButton.IsCheckedProperty); }
            set { SetValue(ImageToggleButton.IsCheckedProperty, value); }
                this.toggleInnerButton.SetValue(AutomationProperties.AutomationIdProperty, thisAutomationId);
                this.toggleInnerButton.SetValue(AutomationProperties.NameProperty, thisAutomationName);
