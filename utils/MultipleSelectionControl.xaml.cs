    /// Interaction logic for MultipleSelectionControl.xaml.
    public partial class MultipleSelectionControl : UserControl
        /// Initializes a new instance of the MultipleSelectionControl class.
        public MultipleSelectionControl()
        /// Show more items in new dialog.
        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
            MultipleSelectionDialog multipleSelectionDialog = new MultipleSelectionDialog();
            multipleSelectionDialog.Title = this.multipleValueButton.ToolTip.ToString();
            multipleSelectionDialog.listboxParameter.ItemsSource = comboxParameter.ItemsSource;
            multipleSelectionDialog.ShowDialog();
            if (multipleSelectionDialog.DialogResult != true)
            StringBuilder newComboText = new StringBuilder();
            foreach (object selectedItem in multipleSelectionDialog.listboxParameter.SelectedItems)
                newComboText.Append(CultureInfo.InvariantCulture, $"{selectedItem},");
            if (newComboText.Length > 1)
                newComboText.Remove(newComboText.Length - 1, 1);
            comboxParameter.Text = newComboText.ToString();
