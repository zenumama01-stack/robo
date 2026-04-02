    /// ModuleViewModel Contains information about a PowerShell module.
    public class ModuleViewModel : INotifyPropertyChanged
        /// True if the module is imported.
        private bool isModuleImported;
        /// Field used for the Name parameter.
        ///  Filter commands property of this module.
        private ObservableCollection<CommandViewModel> filteredCommands;
        /// The selected command property of this module.
        private CommandViewModel selectedCommand;
        /// Field used for the Commands parameter.
        private List<CommandViewModel> commands;
        /// value indicating whether there is a selected command which belongs to an imported module,
        /// with no parameter sets or with a selected parameter set where all mandatory parameters have values
        private bool isThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues;
        /// value indicating whether there is a selected command.
        private bool isThereASelectedCommand;
        /// The AllModulesViewModel containing this, if any.
        private AllModulesViewModel allModules;
        /// Initializes a new instance of the ModuleViewModel class.
        /// <param name="name">Module name.</param>
        public ModuleViewModel(string name, Dictionary<string, ShowCommandModuleInfo> importedModules)
            ArgumentNullException.ThrowIfNull(name);
            this.name = name;
            this.commands = new List<CommandViewModel>();
            this.filteredCommands = new ObservableCollection<CommandViewModel>();
            // This check looks to see if the given module name shows up in
            // the set of modules that are known to be imported in the current
            // session.  In remote PowerShell sessions, the core cmdlet module
            // Microsoft.PowerShell.Core doesn't appear as being imported despite
            // always being loaded by default.  To make sure we don't incorrectly
            // mark this module as not imported, check for it by name.
            this.isModuleImported =
                importedModules == null ? true : name.Length == 0 ||
                importedModules.ContainsKey(name) ||
                string.Equals("Microsoft.PowerShell.Core", name, StringComparison.OrdinalIgnoreCase);
        /// Indicates the selected command in needs to display the help for a command.
        public event EventHandler<HelpNeededEventArgs> SelectedCommandNeedsHelp;
        /// Indicates the selected command needs to import a module.
        public event EventHandler<ImportModuleEventArgs> SelectedCommandNeedsImportModule;
        /// Indicates the selected command should be run.
        public event EventHandler<CommandEventArgs> RunSelectedCommand;
        #region Public Property
        /// Gets the name property of this ModuleView.
            get { return this.name; }
        /// Gets the GUI friendly module name.
                if (!string.IsNullOrEmpty(this.name))
                return ShowCommandResources.NoModuleName;
        /// Gets CommandControl is visibility or not.
        public Visibility CommandControlVisibility
            get { return this.selectedCommand == null ? Visibility.Collapsed : Visibility.Visible; }
        ///  Gets CommandControl Height.
        public GridLength CommandRowHeight
            get { return this.selectedCommand == null ? GridLength.Auto : CommandViewModel.Star; }
        /// Gets the commands under in this module.
        public List<CommandViewModel> Commands
            get { return this.commands; }
        ///  Gets the filter commands of this module.
        public ObservableCollection<CommandViewModel> FilteredCommands
            get { return this.filteredCommands; }
        /// Gets or sets the selected commands of this module.
        public CommandViewModel SelectedCommand
                return this.selectedCommand;
                if (value == this.selectedCommand)
                if (this.selectedCommand != null)
                    this.selectedCommand.PropertyChanged -= this.SelectedCommand_PropertyChanged;
                    this.selectedCommand.HelpNeeded -= this.SelectedCommand_HelpNeeded;
                    this.selectedCommand.ImportModule -= this.SelectedCommand_ImportModule;
                this.selectedCommand = value;
                this.SetIsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues();
                    this.selectedCommand.PropertyChanged += this.SelectedCommand_PropertyChanged;
                    this.selectedCommand.HelpNeeded += this.SelectedCommand_HelpNeeded;
                    this.selectedCommand.ImportModule += this.SelectedCommand_ImportModule;
                    this.IsThereASelectedCommand = true;
                    this.IsThereASelectedCommand = false;
                this.OnNotifyPropertyChanged("SelectedCommand");
                this.OnNotifyPropertyChanged("CommandControlVisibility");
                this.OnNotifyPropertyChanged("CommandRowHeight");
        /// Gets or sets a value indicating whether there is a selected command.
        public bool IsThereASelectedCommand
                return this.isThereASelectedCommand;
                if (value == this.isThereASelectedCommand)
                this.isThereASelectedCommand = value;
                this.OnNotifyPropertyChanged("IsThereASelectedCommand");
        /// Gets or sets a value indicating whether there is a selected command which belongs
        /// to an imported module, with no parameter sets or with a selected parameter set
        /// where all mandatory parameters have values
        public bool IsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues
                return this.isThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues;
                if (value == this.isThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues)
                this.isThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues = value;
                this.OnNotifyPropertyChanged("IsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues");
        /// Gets the AllModulesViewModel containing this, if any.
        public AllModulesViewModel AllModules
                return this.allModules;
        /// Gets a value indicating whether the module is imported.
        internal bool IsModuleImported
                return this.isModuleImported;
        /// Sets the AllModulesViewModel containing this.
        /// <param name="parentAllModules">The AllModulesViewModel containing this.</param>
        internal void SetAllModules(AllModulesViewModel parentAllModules)
            this.allModules = parentAllModules;
        /// Sorts commands and optionally sets ModuleQualifyCommandName.
        /// <param name="markRepeatedCmdlets">True to mark repeated commands with a flag that will produce a module qualified name in GetScript.</param>
        internal void SortCommands(bool markRepeatedCmdlets)
            this.commands.Sort(this.Compare);
            if (!markRepeatedCmdlets || this.commands.Count == 0)
            CommandViewModel reference = this.commands[0];
            for (int i = 1; i < this.commands.Count; i++)
                CommandViewModel command = this.commands[i];
                if (reference.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase))
                    reference.ModuleQualifyCommandName = true;
                    command.ModuleQualifyCommandName = true;
                    reference = command;
        /// According commandNameFilter to filter command,and added the filter commands into filteredCommands property.
        /// <param name="filter">Current filter.</param>
        internal void RefreshFilteredCommands(string filter)
            this.filteredCommands.Clear();
            if (string.IsNullOrEmpty(filter))
                foreach (CommandViewModel command in this.Commands)
                    this.filteredCommands.Add(command);
            WildcardPattern filterPattern = null;
            if (WildcardPattern.ContainsWildcardCharacters(filter))
                filterPattern = new WildcardPattern(filter, WildcardOptions.IgnoreCase);
                if (ModuleViewModel.Matches(filterPattern, command.Name, filter))
                if (filterPattern != null)
                string[] textSplit = filter.Split(' ');
                if (textSplit.Length != 2)
                if (ModuleViewModel.Matches(filterPattern, command.Name, textSplit[0] + "-" + textSplit[1]))
        /// Called in response to a GUI event that requires the command to be run.
        internal void OnRunSelectedCommand()
            EventHandler<CommandEventArgs> handler = this.RunSelectedCommand;
                handler(this, new CommandEventArgs(this.SelectedCommand));
        /// Triggers the SelectedCommandNeedsHelp event.
        internal void OnSelectedCommandNeedsHelp(HelpNeededEventArgs e)
            EventHandler<HelpNeededEventArgs> handler = this.SelectedCommandNeedsHelp;
        /// Triggers the SelectedCommandNeedsImportModule event.
        internal void OnSelectedCommandNeedsImportModule()
            EventHandler<ImportModuleEventArgs> handler = this.SelectedCommandNeedsImportModule;
                handler(this, new ImportModuleEventArgs(this.SelectedCommand.Name, this.SelectedCommand.ModuleName, this.Name));
        /// Uses pattern matching if pattern is not null or calls MatchesEvenIfInPlural otherwise.
        /// <param name="filterPattern">Pattern corresponding to filter.</param>
        /// <param name="commandName">Command name string.</param>
        /// <param name="filter">Filter string.</param>
        /// <returns>True if coparisonText matches str or pattern.</returns>
        private static bool Matches(WildcardPattern filterPattern, string commandName, string filter)
                return filterPattern.IsMatch(commandName);
            return ModuleViewModel.MatchesEvenIfInPlural(commandName, filter);
        /// Returns true if filter matches commandName, even when filter is in the plural.
        /// <returns>Return match result.</returns>
        private static bool MatchesEvenIfInPlural(string commandName, string filter)
            if (commandName.Contains(filter, StringComparison.OrdinalIgnoreCase))
            if (filter.Length > 5 && filter.EndsWith("es", StringComparison.OrdinalIgnoreCase))
                ReadOnlySpan<char> filterSpan = filter.AsSpan(0, filter.Length - 2);
                return commandName.AsSpan().Contains(filterSpan, StringComparison.OrdinalIgnoreCase);
            if (filter.Length > 4 && filter.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                ReadOnlySpan<char> filterSpan = filter.AsSpan(0, filter.Length - 1);
        /// Handles the HelpNeeded event in the selected command and triggers the SelectedCommandNeedsHelp event.
        /// <param name="sender">HelpNeeded event sender.</param>
        /// <param name="e">HelpNeeded event argument.</param>
        private void SelectedCommand_HelpNeeded(object sender, HelpNeededEventArgs e)
            this.OnSelectedCommandNeedsHelp(e);
        /// Handles the ImportModule event in the selected command and triggers the SelectedCommandNeedsImportModule event.
        private void SelectedCommand_ImportModule(object sender, EventArgs e)
            this.OnSelectedCommandNeedsImportModule();
        /// Called when the SelectedCommand property changes to update IsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues.
        private void SelectedCommand_PropertyChanged(object sender, PropertyChangedEventArgs e)
            if (!e.PropertyName.Equals("SelectedParameterSetAllMandatoryParametersHaveValues"))
        /// Called to set IsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues when
        /// SelectedParameterSetAllMandatoryParametersHaveValues changes in the SelectedCommand or
        /// when the SelectedCommand changes
        private void SetIsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues()
            this.IsThereASelectedImportedCommandWhereAllMandatoryParametersHaveValues =
                this.selectedCommand != null &&
                this.selectedCommand.IsImported &&
                this.selectedCommand.SelectedParameterSetAllMandatoryParametersHaveValues;
        /// Compare source commandmodule is equal like target commandmodule.
        /// <param name="source">Source commandmodule.</param>
        /// <param name="target">Target commandmodule.</param>
        /// <returns>Return compare result.</returns>
        private int Compare(CommandViewModel source, CommandViewModel target)
