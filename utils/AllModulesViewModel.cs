    /// Contains all Commands, Parameters, ParameterSet and Common Parameter.
    public class AllModulesViewModel : INotifyPropertyChanged
        #region Private Fields
        /// Flag indicating a wait message is being displayed.
        private bool waitMessageDisplayed;
        /// True if this ViewModel is not supposed to show common parameters.
        private bool noCommonParameter;
        /// the filterName of command.
        private string commandNameFilter;
        /// Field used for the Modules property.
        private List<ModuleViewModel> modules;
        /// true if a command can be run.
        private bool canRun;
        /// true if a command can be copied.
        private bool canCopy;
        /// the selected module being displayed in the GUI.
        private ModuleViewModel selectedModule;
        /// the visibility of the refresh button.
        private Visibility refreshVisibility = Visibility.Collapsed;
        /// Provides an extra viewModel object that allows callers to control certain aspects of the GUI.
        private object extraViewModel;
        /// private property for ZoomLevel.
        private double zoomLevel = 1.0;
        /// Initializes a new instance of the AllModulesViewModel class.
        /// <param name="importedModules">The loaded modules.</param>
        /// <param name="commands">Commands to show.</param>
        public AllModulesViewModel(Dictionary<string, ShowCommandModuleInfo> importedModules, IEnumerable<ShowCommandCommandInfo> commands)
            ArgumentNullException.ThrowIfNull(commands);
            if (!commands.GetEnumerator().MoveNext())
                throw new ArgumentNullException("commands");
            this.Initialization(importedModules, commands, true);
        /// <param name="commands">All PowerShell commands.</param>
        /// <param name="noCommonParameter">True not to show common parameters.</param>
        public AllModulesViewModel(Dictionary<string, ShowCommandModuleInfo> importedModules, IEnumerable<ShowCommandCommandInfo> commands, bool noCommonParameter)
            this.Initialization(importedModules, commands, noCommonParameter);
        /// PropertyChanged Event.
        /// Indicates the selected command in the selected module needs to display the help for a command.
        public event EventHandler<HelpNeededEventArgs> SelectedCommandInSelectedModuleNeedsHelp;
        /// Indicates the selected command in the selected module needs to import a module for a command.
        public event EventHandler<ImportModuleEventArgs> SelectedCommandInSelectedModuleNeedsImportModule;
        /// Indicates the selected command in the selected module should be run.
        public event EventHandler<CommandEventArgs> RunSelectedCommandInSelectedModule;
        /// Indicates we want to refresh the viewModel.
        public event EventHandler<EventArgs> Refresh;
        #region Public Properties
        /// Get or Sets Zoom level.
                return this.zoomLevel;
                if (value > 0)
                    this.zoomLevel = value / 100.0;
        /// Gets the tooltip for the refresh button.
        public static string RefreshTooltip
            get { return string.Format(CultureInfo.CurrentUICulture, ShowCommandResources.RefreshShowCommandTooltipFormat, "import-module"); }
        /// Gets or sets the visibility of the refresh button.
        public Visibility RefreshVisibility
                return this.refreshVisibility;
                if (this.refreshVisibility == value)
                this.refreshVisibility = value;
                this.OnNotifyPropertyChanged("RefreshVisibility");
        /// Gets a value indicating whether common parameters are displayed.
        public bool NoCommonParameter
            get { return this.noCommonParameter; }
        /// Gets or sets the filterName of command.
        public string CommandNameFilter
                return this.commandNameFilter;
                if (this.CommandNameFilter == value)
                this.commandNameFilter = value;
                if (this.selectedModule != null)
                    this.selectedModule.RefreshFilteredCommands(this.CommandNameFilter);
                    this.selectedModule.SelectedCommand = null;
                this.OnNotifyPropertyChanged("CommandNameFilter");
        /// Gets or sets the selected module being displayed in the GUI.
        public ModuleViewModel SelectedModule
                return this.selectedModule;
                if (this.selectedModule == value)
                    this.selectedModule.SelectedCommandNeedsImportModule -= this.SelectedModule_SelectedCommandNeedsImportModule;
                    this.selectedModule.SelectedCommandNeedsHelp -= this.SelectedModule_SelectedCommandNeedsHelp;
                    this.selectedModule.RunSelectedCommand -= this.SelectedModule_RunSelectedCommand;
                    this.selectedModule.PropertyChanged -= this.SelectedModule_PropertyChanged;
                this.selectedModule = value;
                this.SetCanRun();
                this.SetCanCopy();
                    this.selectedModule.SelectedCommandNeedsImportModule += this.SelectedModule_SelectedCommandNeedsImportModule;
                    this.selectedModule.SelectedCommandNeedsHelp += this.SelectedModule_SelectedCommandNeedsHelp;
                    this.selectedModule.RunSelectedCommand += this.SelectedModule_RunSelectedCommand;
                    this.selectedModule.PropertyChanged += this.SelectedModule_PropertyChanged;
                this.OnNotifyPropertyChanged("SelectedModule");
        /// Gets a value indicating whether we can run a command.
        public bool CanRun
                return this.canRun;
        /// Gets a value indicating whether we can copy a command.
        public bool CanCopy
                return this.canCopy;
        /// Gets the Modules parameter.
        public List<ModuleViewModel> Modules
            get { return this.modules; }
        /// Gets the visibility of the wait message.
        public Visibility WaitMessageVisibility
                return this.waitMessageDisplayed ? Visibility.Visible : Visibility.Hidden;
        /// Gets the visibility of the main grid.
        public Visibility MainGridVisibility
                return this.waitMessageDisplayed ? Visibility.Hidden : Visibility.Visible;
        /// Gets a value indicating whether the main grid is displayed.
        public bool MainGridDisplayed
                return !this.waitMessageDisplayed;
        /// Gets or sets a value indicating whether the wait message is displayed.
        public bool WaitMessageDisplayed
                return this.waitMessageDisplayed;
                if (this.waitMessageDisplayed == value)
                this.waitMessageDisplayed = value;
                this.OnNotifyPropertyChanged("WaitMessageDisplayed");
                this.OnNotifyPropertyChanged("WaitMessageVisibility");
                this.OnNotifyPropertyChanged("MainGridDisplayed");
                this.OnNotifyPropertyChanged("MainGridVisibility");
        /// Gets or sets an extra viewModel object that allows callers to control certain aspects of the GUI.
        public object ExtraViewModel
                return this.extraViewModel;
                if (this.extraViewModel == value)
                this.extraViewModel = value;
                this.OnNotifyPropertyChanged("ExtraViewModel");
        /// Returns the selected script.
        /// <returns>The selected script.</returns>
        public string GetScript()
            if (this.SelectedModule == null)
            if (this.SelectedModule.SelectedCommand == null)
            return this.SelectedModule.SelectedCommand.GetScript();
        /// Triggers Refresh.
        internal void OnRefresh()
            EventHandler<EventArgs> handler = this.Refresh;
                handler(this, new EventArgs());
        /// If current modules name is ALL, then return true.
        /// <param name="name">The modules name.</param>
        /// <returns>Return true is the module name is ALLModulesViewModel.</returns>
        private static bool IsAll(string name)
            return name.Equals(ShowCommandResources.All, StringComparison.Ordinal);
        /// Monitors property changes in the selected module to call:
        ///     SetCanRun for IsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues
        ///     SetCanCopy for SetCanCopy
        private void SelectedModule_PropertyChanged(object sender, PropertyChangedEventArgs e)
            if (e.PropertyName == "IsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues")
            else if (e.PropertyName == "IsThereASelectedCommand")
        /// Called to set this.CanRun when:
        ///     The SelectedModule changes, since there will be no selected command in the new module, and CanRun should be false
        ///     WaitMessageDisplayedMessage changes since this being true will cause this.MainGridDisplayed to be false and CanRun should be false
        ///     IsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues changes in the selected module
        private void SetCanRun()
            bool newValue = this.selectedModule != null && this.MainGridDisplayed &&
                this.selectedModule.IsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues;
            if (this.canRun == newValue)
            this.canRun = newValue;
            this.OnNotifyPropertyChanged("CanRun");
        /// Called to set this.CanCopy when:
        ///     The SelectedModule changes, since there will be no selected command in the new module, and CanCopy should be false
        ///     WaitMessageDisplayedMessage changes since this being true will cause this.MainGridDisplayed to be false and CanCopy should be false
        ///     IsThereASelectedCommand changes in the selected module
        private void SetCanCopy()
            bool newValue = this.selectedModule != null && this.MainGridDisplayed && this.selectedModule.IsThereASelectedCommand;
            if (this.canCopy == newValue)
            this.canCopy = newValue;
            this.OnNotifyPropertyChanged("CanCopy");
        /// Initialize AllModulesViewModel.
        /// <param name="importedModules">All loaded modules.</param>
        /// <param name="commands">List of commands in all modules.</param>
        /// <param name="noCommonParameterInModel">Whether showing common parameter.</param>
        private void Initialization(Dictionary<string, ShowCommandModuleInfo> importedModules, IEnumerable<ShowCommandCommandInfo> commands, bool noCommonParameterInModel)
            if (commands == null)
            Dictionary<string, ModuleViewModel> rawModuleViewModels = new Dictionary<string, ModuleViewModel>();
            this.noCommonParameter = noCommonParameterInModel;
            // separates commands in their Modules
            foreach (ShowCommandCommandInfo command in commands)
                ModuleViewModel moduleViewModel;
                if (!rawModuleViewModels.TryGetValue(command.ModuleName, out moduleViewModel))
                    moduleViewModel = new ModuleViewModel(command.ModuleName, importedModules);
                    rawModuleViewModels.Add(command.ModuleName, moduleViewModel);
                CommandViewModel commandViewModel;
                    commandViewModel = CommandViewModel.GetCommandViewModel(moduleViewModel, command, noCommonParameterInModel);
                catch (RuntimeException)
                moduleViewModel.Commands.Add(commandViewModel);
                moduleViewModel.SetAllModules(this);
            // populates this.modules
            this.modules = new List<ModuleViewModel>();
            // if there is just one module then use only it
            if (rawModuleViewModels.Values.Count == 1)
                this.modules.Add(rawModuleViewModels.Values.First());
                this.modules[0].SortCommands(false);
                this.SelectedModule = this.modules[0];
            // If there are more modules, create an additional module to aggregate all commands
            ModuleViewModel allCommandsModule = new ModuleViewModel(ShowCommandResources.All, null);
            this.modules.Add(allCommandsModule);
            allCommandsModule.SetAllModules(this);
            if (rawModuleViewModels.Values.Count > 0)
                foreach (ModuleViewModel module in rawModuleViewModels.Values)
                    module.SortCommands(false);
                    this.modules.Add(module);
                    allCommandsModule.Commands.AddRange(module.Commands);
            allCommandsModule.SortCommands(true);
            this.modules.Sort(this.Compare);
            this.SelectedModule = this.modules.Count == 0 ? null : this.modules[0];
        /// Compare two ModuleViewModel target and source.
        /// <param name="source">The source ModuleViewModel.</param>
        /// <param name="target">The target ModuleViewModel.</param>
        /// <returns>Compare result.</returns>
        private int Compare(ModuleViewModel source, ModuleViewModel target)
            if (AllModulesViewModel.IsAll(source.Name) && !AllModulesViewModel.IsAll(target.Name))
            if (!AllModulesViewModel.IsAll(source.Name) && AllModulesViewModel.IsAll(target.Name))
            return string.Compare(source.Name, target.Name, StringComparison.OrdinalIgnoreCase);
        /// Called when the SelectedCommandNeedsHelp event is triggered in the Selected Module.
        private void SelectedModule_SelectedCommandNeedsHelp(object sender, HelpNeededEventArgs e)
            this.OnSelectedCommandInSelectedModuleNeedsHelp(e);
        /// Called when the SelectedCommandNeedsImportModule event is triggered in the Selected Module.
        private void SelectedModule_SelectedCommandNeedsImportModule(object sender, ImportModuleEventArgs e)
            this.OnSelectedCommandInSelectedModuleNeedsImportModule(e);
        /// Triggers SelectedCommandInSelectedModuleNeedsHelp.
        private void OnSelectedCommandInSelectedModuleNeedsHelp(HelpNeededEventArgs e)
            EventHandler<HelpNeededEventArgs> handler = this.SelectedCommandInSelectedModuleNeedsHelp;
                handler(this, e);
        /// Triggers SelectedCommandInSelectedModuleNeedsImportModule.
        private void OnSelectedCommandInSelectedModuleNeedsImportModule(ImportModuleEventArgs e)
            EventHandler<ImportModuleEventArgs> handler = this.SelectedCommandInSelectedModuleNeedsImportModule;
        /// Called when the RunSelectedCommand is triggered in the selected module.
        private void SelectedModule_RunSelectedCommand(object sender, CommandEventArgs e)
            this.OnRunSelectedCommandInSelectedModule(e);
        /// Triggers RunSelectedCommandInSelectedModule.
        private void OnRunSelectedCommandInSelectedModule(CommandEventArgs e)
            EventHandler<CommandEventArgs> handler = this.RunSelectedCommandInSelectedModule;
        /// If property changed will be notify.
        /// <param name="propertyName">The changed property.</param>
