    /// Dialog with settings for the help dialog.
    public partial class SettingsDialog : Window
        /// Initializes a new instance of the SettingsDialog class.
        public SettingsDialog()
            this.Description.IsChecked = HelpWindowSettings.Default.HelpDescriptionDisplayed;
            this.Examples.IsChecked = HelpWindowSettings.Default.HelpExamplesDisplayed;
            this.Inputs.IsChecked = HelpWindowSettings.Default.HelpInputsDisplayed;
            this.Notes.IsChecked = HelpWindowSettings.Default.HelpNotesDisplayed;
            this.Outputs.IsChecked = HelpWindowSettings.Default.HelpOutputsDisplayed;
            this.Parameters.IsChecked = HelpWindowSettings.Default.HelpParametersDisplayed;
            this.RelatedLinks.IsChecked = HelpWindowSettings.Default.HelpRelatedLinksDisplayed;
            this.Remarks.IsChecked = HelpWindowSettings.Default.HelpRemarksDisplayed;
            this.Synopsys.IsChecked = HelpWindowSettings.Default.HelpSynopsysDisplayed;
            this.Syntax.IsChecked = HelpWindowSettings.Default.HelpSyntaxDisplayed;
            this.CaseSensitive.IsChecked = HelpWindowSettings.Default.HelpSearchMatchCase;
            this.WholeWord.IsChecked = HelpWindowSettings.Default.HelpSearchWholeWord;
        /// Called when the OK button has been clicked.
        private void OK_Click(object sender, RoutedEventArgs e)
            HelpWindowSettings.Default.HelpDescriptionDisplayed = this.Description.IsChecked == true;
            HelpWindowSettings.Default.HelpExamplesDisplayed = this.Examples.IsChecked == true;
            HelpWindowSettings.Default.HelpInputsDisplayed = this.Inputs.IsChecked == true;
            HelpWindowSettings.Default.HelpOutputsDisplayed = this.Outputs.IsChecked == true;
            HelpWindowSettings.Default.HelpNotesDisplayed = this.Notes.IsChecked == true;
            HelpWindowSettings.Default.HelpParametersDisplayed = this.Parameters.IsChecked == true;
            HelpWindowSettings.Default.HelpRelatedLinksDisplayed = this.RelatedLinks.IsChecked == true;
            HelpWindowSettings.Default.HelpRemarksDisplayed = this.Remarks.IsChecked == true;
            HelpWindowSettings.Default.HelpSynopsysDisplayed = this.Synopsys.IsChecked == true;
            HelpWindowSettings.Default.HelpSyntaxDisplayed = this.Syntax.IsChecked == true;
            HelpWindowSettings.Default.HelpSearchMatchCase = this.CaseSensitive.IsChecked == true;
            HelpWindowSettings.Default.HelpSearchWholeWord = this.WholeWord.IsChecked == true;
            this.DialogResult = true;
            this.Close();
