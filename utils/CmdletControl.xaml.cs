    /// Interaction logic for CmdletControl.xaml.
    public partial class CmdletControl : UserControl
        /// Field used for the CurrentCommandViewModel parameter.
        private CommandViewModel currentCommandViewModel;
        /// Initializes a new instance of the CmdletControl class.
        public CmdletControl()
            this.NotImportedControl.ImportModuleButton.Click += ImportModuleButton_Click;
            this.ParameterSetTabControl.DataContextChanged += new DependencyPropertyChangedEventHandler(this.ParameterSetTabControl_DataContextChanged);
            this.KeyDown += this.CmdletControl_KeyDown;
            this.helpButton.innerButton.Click += this.HelpButton_Click;
        /// Gets the owner of the ViewModel.
        private CommandViewModel CurrentCommandViewModel
            get { return this.currentCommandViewModel; }
        #region Private Events
        /// DataContextChanged event.
        private void ParameterSetTabControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            if (this.DataContext == null)
            CommandViewModel viewModel = (CommandViewModel)this.DataContext;
            this.currentCommandViewModel = viewModel;
            if (viewModel.ParameterSets.Count == 0)
            this.ParameterSetTabControl.SelectedItem = viewModel.ParameterSets[0];
        /// Key down event for user press F1 button.
        private void CmdletControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
            if (e.Key == System.Windows.Input.Key.F1)
                this.CurrentCommandViewModel.OpenHelpWindow();
        /// Help button event.
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        /// Import Module Button event.
        private void ImportModuleButton_Click(object sender, RoutedEventArgs e)
            this.CurrentCommandViewModel.OnImportModule();
