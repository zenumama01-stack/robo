    /// Partial class implementation for MessageTextBox control.
    public partial class MessageTextBox : TextBox
        static partial void StaticConstructorImplementation()
            TextProperty.OverrideMetadata(
                                          typeof(MessageTextBox),
                                          new FrameworkPropertyMetadata(
                                                                        new CoerceValueCallback(OnTextBoxTextCoerce)));
        #region Non-Public Methods
        private void UpdateIsBackgroundTextShown(string text)
            if (string.IsNullOrEmpty(text) == false && this.IsBackgroundTextShown)
                this.IsBackgroundTextShown = false;
            else if (string.IsNullOrEmpty(text) && this.IsBackgroundTextShown == false)
                this.IsBackgroundTextShown = true;
        private static object OnTextBoxTextCoerce(DependencyObject o, object baseValue)
            MessageTextBox mtb = (MessageTextBox)o;
            mtb.UpdateIsBackgroundTextShown((string)baseValue);
            if (baseValue == null)
            return baseValue;
        #endregion Non-Public Methods
