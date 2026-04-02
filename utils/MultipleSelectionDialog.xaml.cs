    /// Interaction logic for MultipleSelectionDialog.xaml.
    public partial class MultipleSelectionDialog : Window
        /// Initializes a new instance of the MultipleSelectionDialog class.
        public MultipleSelectionDialog()
        /// OK Click event function.
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        /// Cancel Click event function.
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
            this.DialogResult = false;
