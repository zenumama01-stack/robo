    /// Contains information about a single parameter inside a parameter set.
    /// If a parameter with the same name belongs to two (or more) parameter sets,
    /// there will be two (or more) ParameterViewModel objects for the parameters,
    /// each one inside its own ParameterSetViewModel.
    public class ParameterViewModel : INotifyPropertyChanged
        /// ParameterMetadata contains information that is the same throughout parameter sets
        /// like Name and Type.
        /// Note: It also happens to contain a list of all ParameterSetMetadata for the parametersets
        /// in this cmdlet, but this information is not used in this class since if a parameter is
        /// in multiple parametersets, there will be a ParameterViewModel for each time the parameter
        /// appears in a parameterset.
        private ShowCommandParameterInfo parameter;
        /// value entered in the GUI for the parameter.
        private object parameterValue;
        /// Name of the parameter set this parameter is in.
        private string parameterSetName;
        /// Initializes a new instance of the ParameterViewModel class.
        /// <param name="parameter">The parameter information for this parameter.</param>
        /// <param name="parameterSetName">The name of the parameter set this parameter is in.</param>
        public ParameterViewModel(ShowCommandParameterInfo parameter, string parameterSetName)
            ArgumentNullException.ThrowIfNull(parameterSetName);
            this.parameter = parameter;
            this.parameterSetName = parameterSetName;
            if (this.parameter.ParameterType.IsSwitch)
                this.parameterValue = false;
                this.parameterValue = string.Empty;
        /// Gets the ParameterMetadata that contains information that is the same throughout parameter sets
        public ShowCommandParameterInfo Parameter
            get { return this.parameter; }
        /// Gets or sets the value for this parameter from the GUI.
                return this.parameterValue;
                if (this.parameterValue != value)
                    this.parameterValue = value;
                    this.OnNotifyPropertyChanged("Value");
        /// Gets the parameter name.
            get { return this.Parameter.Name; }
        /// Gets the name of the parameter set this parameter is in.
        public string ParameterSetName
            get { return this.parameterSetName; }
        /// Gets a value indicating whether this parameter is in the shared parameterset.
        public bool IsInSharedParameterSet
            get { return CommandViewModel.IsSharedParameterSetName(this.parameterSetName); }
        /// Gets Name with an extra suffix to indicate if the parameter is mandatory to serve.
        public string NameTextLabel
                return this.Parameter.IsMandatory ?
                        ShowCommandResources.MandatoryNameLabelFormat,
                        ShowCommandResources.MandatoryLabelSegment) :
        /// Gets Label in the case this parameter is used in a combo box.
        public string NameCheckLabel
                string returnValue = this.Parameter.Name;
                if (this.Parameter.IsMandatory)
                    returnValue = string.Create(CultureInfo.CurrentUICulture, $"{returnValue}{ShowCommandResources.MandatoryLabelSegment}");
        /// Gets Tooltip string for the parameter.
                return ParameterViewModel.EvaluateTooltip(
                    this.Parameter.ParameterType.FullName,
                    this.Parameter.Position,
                    this.Parameter.IsMandatory,
                    this.IsInSharedParameterSet,
                    this.Parameter.ValueFromPipeline);
        /// Gets a value indicating whether the parameter is mandatory.
        public bool IsMandatory
            get { return this.Parameter.IsMandatory; }
        /// Gets a value indicating whether the parameter has a value.
        public bool HasValue
                if (this.Parameter.ParameterType.IsSwitch)
                    return ((bool?)this.Value) == true;
                return this.Value.ToString().Length != 0;
        /// Evaluates the tooltip based on the parameters.
        /// <param name="typeName">Parameter type name.</param>
        /// <param name="position">Parameter position.</param>
        /// <param name="mandatory">True if the parameter is mandatory.</param>
        /// <param name="shared">True if the parameter is shared by parameter sets.</param>
        /// <param name="valueFromPipeline">True if the parameter takes value from the pipeline.</param>
        /// <returns> the tooltip based on the parameters.</returns>
        internal static string EvaluateTooltip(string typeName, int position, bool mandatory, bool shared, bool valueFromPipeline)
            StringBuilder returnValue = new StringBuilder(string.Format(
                    ShowCommandResources.TypeFormat,
                    typeName));
            string newlineFormatString = Environment.NewLine + "{0}";
            if (position >= 0)
                string positionFormat = string.Format(
                    ShowCommandResources.PositionFormat,
                returnValue.AppendFormat(CultureInfo.InvariantCulture, newlineFormatString, positionFormat);
            string optionalOrMandatory = mandatory ? ShowCommandResources.Mandatory : ShowCommandResources.Optional;
            returnValue.AppendFormat(CultureInfo.InvariantCulture, newlineFormatString, optionalOrMandatory);
            if (shared)
                returnValue.AppendFormat(CultureInfo.InvariantCulture, newlineFormatString, ShowCommandResources.CommonToAllParameterSets);
            if (valueFromPipeline)
                returnValue.AppendFormat(CultureInfo.InvariantCulture, newlineFormatString, ShowCommandResources.CanReceiveValueFromPipeline);
