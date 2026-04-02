    /// Button with images to represent enabled and disabled states.
    [SuppressMessage("Microsoft.MSInternal", "CA903:InternalNamespaceShouldNotContainPublicTypes", Justification = "Required by XAML")]
    public partial class ImageButton : ImageButtonBase
        /// Initializes a new instance of the ImageButton class.
        public ImageButton()
            this.Loaded += this.ImageButton_Loaded;
        /// Copies the automation id and name from the parent control to the inner button.
        private void ImageButton_Loaded(object sender, System.Windows.RoutedEventArgs e)
            object thisAutomationId = this.GetValue(AutomationProperties.AutomationIdProperty);
            if (thisAutomationId != null)
                this.innerButton.SetValue(AutomationProperties.AutomationIdProperty, thisAutomationId);
            object thisAutomationName = this.GetValue(AutomationProperties.NameProperty);
            if (thisAutomationName != null)
                this.innerButton.SetValue(AutomationProperties.NameProperty, thisAutomationName);
