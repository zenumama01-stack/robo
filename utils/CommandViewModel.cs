using Microsoft.Management.UI;
using SMAI = System.Management.Automation.Internal;
    /// Contains information about a cmdlet's Shard ParameterSet,
    /// ParameterSets, Parameters, Common Parameters and error message.
    public class CommandViewModel : INotifyPropertyChanged
        /// The name of the AllParameterSets.
        private const string SharedParameterSetName = "__AllParameterSets";
        /// Grid length constant.
        private static readonly GridLength star = new GridLength(1, GridUnitType.Star);
        /// The module containing this cmdlet in the gui.
        private ModuleViewModel parentModule;
        /// The name of the default ParameterSet.
        private string defaultParameterSetName;
        /// Field used for the AreCommonParametersExpanded parameter.
        private bool areCommonParametersExpanded;
        /// Field used for the SelectedParameterSet parameter.
        private ParameterSetViewModel selectedParameterSet;
        /// Field used for the ParameterSets parameter.
        private List<ParameterSetViewModel> parameterSets = new List<ParameterSetViewModel>();
        /// Field used for the ParameterSetTabControlVisibility parameter.
        private bool noCommonParameters;
        /// Field used for the CommonParameters parameter.
        private ParameterSetViewModel comonParameters;
        /// The ShowCommandCommandInfo this model is based on.
        private ShowCommandCommandInfo commandInfo;
        ///  value indicating whether the selected parameter set has all mandatory parameters valid.
        private bool selectedParameterSetAllMandatoryParametersHaveValues;
        /// value indicating whether the command name should be qualified by the module in GetScript.
        private bool moduleQualifyCommandName;
        /// The height for common parameters that will depend on CommonParameterVisibility.
        private GridLength commonParametersHeight;
        /// Prevents a default instance of the CommandViewModel class from being created.
        private CommandViewModel()
        /// Indicates the command needs to display the help for a command.
        public event EventHandler<HelpNeededEventArgs> HelpNeeded;
        /// Indicates a module needs to be imported.
        public event EventHandler<EventArgs> ImportModule;
        /// Gets or sets a value indicating whether the command name should be qualified by the module in GetScript.
        public bool ModuleQualifyCommandName
            get { return this.moduleQualifyCommandName; }
            set { this.moduleQualifyCommandName = value; }
        /// Gets or sets a value indicating whether the common parameters are expanded.
        public bool AreCommonParametersExpanded
                return this.areCommonParametersExpanded;
                if (this.areCommonParametersExpanded == value)
                this.areCommonParametersExpanded = value;
                this.OnNotifyPropertyChanged("AreCommonParametersExpanded");
                this.SetCommonParametersHeight();
        /// Gets or sets the SelectedParameterSet parameter.
        public ParameterSetViewModel SelectedParameterSet
                return this.selectedParameterSet;
                if (this.selectedParameterSet != value)
                    if (this.selectedParameterSet != null)
                        this.selectedParameterSet.PropertyChanged -= this.SelectedParameterSet_PropertyChanged;
                    this.selectedParameterSet = value;
                        this.selectedParameterSet.PropertyChanged += this.SelectedParameterSet_PropertyChanged;
                        this.SelectedParameterSetAllMandatoryParametersHaveValues = this.SelectedParameterSet.AllMandatoryParametersHaveValues;
                        this.SelectedParameterSetAllMandatoryParametersHaveValues = true;
                    this.OnNotifyPropertyChanged("SelectedParameterSet");
        /// Gets or sets a value indicating whether the selected parameter set has all mandatory parameters valid.
        /// If there is no selected parameter set this value is true
        public bool SelectedParameterSetAllMandatoryParametersHaveValues
                return this.selectedParameterSetAllMandatoryParametersHaveValues;
                if (this.selectedParameterSetAllMandatoryParametersHaveValues == value)
                this.selectedParameterSetAllMandatoryParametersHaveValues = value;
                this.OnNotifyPropertyChanged("SelectedParameterSetAllMandatoryParametersHaveValues");
        /// Gets the ParameterSets parameter.
        public List<ParameterSetViewModel> ParameterSets
            get { return this.parameterSets; }
        /// Gets the visibility for the tab control displaying several ParameterSetControl. This is displayed when there are more than 1 parameter sets.
        public Visibility ParameterSetTabControlVisibility
            get { return (this.ParameterSets.Count > 1) && this.IsImported ? Visibility.Visible : Visibility.Collapsed; }
        /// Gets the visibility for the single ParameterSetControl displayed when there is only 1 parameter set.
        public Visibility SingleParameterSetControlVisibility
            get { return (this.ParameterSets.Count == 1) ? Visibility.Visible : Visibility.Collapsed; }
        /// Gets the CommonParameters parameter.
        public ParameterSetViewModel CommonParameters
            get { return this.comonParameters; }
        /// Gets the CommonParameterVisibility parameter.
        public Visibility CommonParameterVisibility
            get { return this.noCommonParameters || (this.CommonParameters.Parameters.Count == 0) ? Visibility.Collapsed : Visibility.Visible; }
        /// Gets or sets the height for common parameters that will depend on CommonParameterVisibility.
        public GridLength CommonParametersHeight
                return this.commonParametersHeight;
                if (this.commonParametersHeight == value)
                this.commonParametersHeight = value;
                this.OnNotifyPropertyChanged("CommonParametersHeight");
        /// Gets the visibility for the control displayed when the module is not imported.
        public Visibility NotImportedVisibility
                return this.IsImported ? Visibility.Collapsed : Visibility.Visible;
        /// Gets the visibility for the control displayed when there are no parameters.
        public Visibility NoParameterVisibility
                bool hasNoParameters = this.ParameterSets.Count == 0 || (this.ParameterSets.Count == 1 && this.ParameterSets[0].Parameters.Count == 0);
                return this.IsImported && hasNoParameters ? Visibility.Visible : Visibility.Collapsed;
        /// Gets a value indicating whether the cmdlet comes from a module which is imported.
        public bool IsImported
                return this.commandInfo.Module == null || this.ParentModule.IsModuleImported;
        /// Gets the Name parameter.
                if (this.commandInfo != null)
                    return this.commandInfo.Name;
        /// Gets the module path if it is not null or empty, or the name otherwise.
        public string ModuleName
                if (this.commandInfo != null && this.commandInfo.ModuleName != null)
                    return this.commandInfo.ModuleName;
        /// Gets the module containing this cmdlet in the GUI.
        public ModuleViewModel ParentModule
                return this.parentModule;
        /// Gets Tooltip string for the cmdlet.
        public string ToolTip
                return string.Format(
                     ShowCommandResources.CmdletTooltipFormat,
                     this.Name,
                     this.ParentModule.DisplayName,
                     this.IsImported ? ShowCommandResources.Imported : ShowCommandResources.NotImported);
        /// Gets the message to be displayed when the cmdlet belongs to a module that is not imported.
        public string ImportModuleMessage
                     ShowCommandResources.NotImportedFormat,
                     this.ModuleName,
                     ShowCommandResources.ImportModuleButtonText);
        /// Gets the title for the cmdlet details.
        public string DetailsTitle
                if (this.IsImported)
                         ShowCommandResources.DetailsParameterTitleFormat,
                         this.Name);
                         ShowCommandResources.NameLabelFormat,
        /// Gets a Grid length constant.
        internal static GridLength Star
            get { return CommandViewModel.star; }
        /// Gets the builded PowerShell script.
        /// <returns>Return script as string.</returns>
            StringBuilder builder = new StringBuilder();
            string commandName = this.commandInfo.CommandType == CommandTypes.ExternalScript ? this.commandInfo.Definition : this.Name;
            if (this.ModuleQualifyCommandName && !string.IsNullOrEmpty(this.ModuleName))
                commandName = this.ModuleName + "\\" + commandName;
            if (commandName.Contains(' '))
                builder.Append($"& \"{commandName}\"");
                builder.Append(commandName);
            builder.Append(' ');
            if (this.SelectedParameterSet != null)
                builder.Append(this.SelectedParameterSet.GetScript());
            if (this.CommonParameters != null)
                builder.Append(this.CommonParameters.GetScript());
            string script = builder.ToString();
            return script.Trim();
        /// Showing help information for current active cmdlet.
        public void OpenHelpWindow()
            this.OnHelpNeeded();
        /// Determines whether current command name and a specified ParameterSetName have same name.
        /// <param name="name">The name of ShareParameterSet.</param>
        /// <returns>Return true is ShareParameterSet. Else return false.</returns>
        internal static bool IsSharedParameterSetName(string name)
            return name.Equals(CommandViewModel.SharedParameterSetName, StringComparison.OrdinalIgnoreCase);
        /// Creates a new CommandViewModel out the <paramref name="commandInfo"/>.
        /// <param name="module">Module to which the CommandViewModel will belong to.</param>
        /// <param name="commandInfo">Will showing command.</param>
        /// <param name="noCommonParameters">True to ommit displaying common parameter.</param>
        /// <exception cref="ArgumentNullException">If commandInfo is null</exception>
        /// <exception cref="RuntimeException">
        /// If could not create the CommandViewModel. For instance the ShowCommandCommandInfo corresponding to
        /// the following function will throw a RuntimeException when the ShowCommandCommandInfo Parameters
        /// are retrieved:
        /// function CrashMe ([I.Am.A.Type.That.Does.Not.Exist]$name) {}
        /// <returns>The CommandViewModel corresponding to commandInfo.</returns>
        internal static CommandViewModel GetCommandViewModel(ModuleViewModel module, ShowCommandCommandInfo commandInfo, bool noCommonParameters)
            ArgumentNullException.ThrowIfNull(commandInfo);
            CommandViewModel returnValue = new CommandViewModel();
            returnValue.commandInfo = commandInfo;
            returnValue.noCommonParameters = noCommonParameters;
            returnValue.parentModule = module;
            Dictionary<string, ParameterViewModel> commonParametersTable = new Dictionary<string, ParameterViewModel>();
            foreach (ShowCommandParameterSetInfo parameterSetInfo in commandInfo.ParameterSets)
                if (parameterSetInfo.IsDefault)
                    returnValue.defaultParameterSetName = parameterSetInfo.Name;
                List<ParameterViewModel> parametersForParameterSet = new List<ParameterViewModel>();
                foreach (ShowCommandParameterInfo parameterInfo in parameterSetInfo.Parameters)
                    bool isCommon = Cmdlet.CommonParameters.Contains(parameterInfo.Name);
                    if (isCommon)
                        if (!commonParametersTable.ContainsKey(parameterInfo.Name))
                            commonParametersTable.Add(parameterInfo.Name, new ParameterViewModel(parameterInfo, parameterSetInfo.Name));
                    parametersForParameterSet.Add(new ParameterViewModel(parameterInfo, parameterSetInfo.Name));
                if (parametersForParameterSet.Count != 0)
                    returnValue.ParameterSets.Add(new ParameterSetViewModel(parameterSetInfo.Name, parametersForParameterSet));
            List<ParameterViewModel> commonParametersList = commonParametersTable.Values.ToList<ParameterViewModel>();
            returnValue.comonParameters = new ParameterSetViewModel(string.Empty, commonParametersList);
            returnValue.parameterSets.Sort(returnValue.Compare);
            if (returnValue.parameterSets.Count > 0)
                // Setting SelectedParameterSet will also set SelectedParameterSetAllMandatoryParametersHaveValues
                returnValue.SelectedParameterSet = returnValue.ParameterSets[0];
                returnValue.SelectedParameterSetAllMandatoryParametersHaveValues = true;
            returnValue.SetCommonParametersHeight();
        /// Called to trigger the event fired when help is needed for the command.
        internal void OnHelpNeeded()
            EventHandler<HelpNeededEventArgs> handler = this.HelpNeeded;
                handler(this, new HelpNeededEventArgs(this.Name));
        /// Called to trigger the event fired when a module needs to be imported.
        internal void OnImportModule()
            EventHandler<EventArgs> handler = this.ImportModule;
        /// Called to set the height for common parameters initially or when the AreCommonParametersExpanded changes.
        private void SetCommonParametersHeight()
            this.CommonParametersHeight = this.AreCommonParametersExpanded ? CommandViewModel.Star : GridLength.Auto;
        /// Compares source and target by being the default parameter set and then by name.
        /// <param name="source">Source paremeterset.</param>
        /// <param name="target">Target parameterset.</param>
        /// <returns>0 if they are the same, -1 if source is smaller, 1 if source is larger.</returns>
        private int Compare(ParameterSetViewModel source, ParameterSetViewModel target)
            if (this.defaultParameterSetName != null)
                if (source.Name.Equals(this.defaultParameterSetName) && target.Name.Equals(this.defaultParameterSetName))
                if (source.Name.Equals(this.defaultParameterSetName, StringComparison.Ordinal))
                if (target.Name.Equals(this.defaultParameterSetName, StringComparison.Ordinal))
            return string.CompareOrdinal(source.Name, target.Name);
        /// Called when the PropertyChanged event is triggered on the SelectedParameterSet.
        private void SelectedParameterSet_PropertyChanged(object sender, PropertyChangedEventArgs e)
            if (!e.PropertyName.Equals("AllMandatoryParametersHaveValues"))
