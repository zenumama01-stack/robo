namespace Microsoft.PowerShell.Commands.ShowCommandInternal
    /// Interaction logic for AllModulesControl.xaml.
    public partial class AllModulesControl : UserControl
        #region Construction and Destructor
        /// Initializes a new instance of the AllModulesControl class.
        public AllModulesControl()
            this.Loaded += (obj, args) =>
                this.ModulesCombo.Focus();
        /// Gets current control of the ShowModuleControl.
        internal ShowModuleControl CurrentShowModuleControl
            get { return this.ShowModuleControl; }
        private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
            AllModulesViewModel viewModel = this.DataContext as AllModulesViewModel;
            if (viewModel == null)
            viewModel.OnRefresh();
