using System.Management.Automation.Language;
    ///  Contains information about a single ParameterSet inside a cmdlet.
    public class ParameterSetViewModel : INotifyPropertyChanged
        /// value indicating all mandatory parameters have values.
        private bool allMandatoryParametersHaveValues;
        /// Field used for the Parameters parameter.
        private List<ParameterViewModel> parameters;
        /// Initializes a new instance of the ParameterSetViewModel class.
        /// <param name="name">The name of the parameterSet.</param>
        /// <param name="parameters">The array parameters of the parameterSet.</param>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "this type is internal, made public only for WPF Binding")]
        public ParameterSetViewModel(
            List<ParameterViewModel> parameters)
            ArgumentNullException.ThrowIfNull(parameters);
            parameters.Sort(Compare);
            this.parameters = parameters;
            foreach (ParameterViewModel parameter in this.parameters)
                if (!parameter.IsMandatory)
                parameter.PropertyChanged += this.MandatoryParameter_PropertyChanged;
            this.EvaluateAllMandatoryParametersHaveValues();
        /// Gets the ParameterSet Name.
        /// Gets the Parameters of this parameterset.
        public List<ParameterViewModel> Parameters
            get { return this.parameters; }
        /// Gets or sets a value indicating whether all mandatory parameters have values.
        public bool AllMandatoryParametersHaveValues
                return this.allMandatoryParametersHaveValues;
                if (this.allMandatoryParametersHaveValues != value)
                    this.allMandatoryParametersHaveValues = value;
                    this.OnNotifyPropertyChanged("AllMandatoryParametersHaveValues");
        #region Public Method
        /// Creates script according parameters of this parameterset.
        /// <returns>Return script of this parameterset parameters.</returns>
            if (this.Parameters == null || this.Parameters.Count == 0)
            foreach (ParameterViewModel parameter in this.Parameters)
                if (parameter.Value == null)
                    if (((bool?)parameter.Value) == true)
                        builder.Append($"-{parameter.Name} ");
                string parameterValueString = parameter.Value.ToString();
                if (parameterValueString.Length == 0)
                ShowCommandParameterType parameterType = parameter.Parameter.ParameterType;
                if (parameterType.IsEnum || parameterType.IsString || (parameterType.IsArray && parameterType.ElementType.IsString))
                    parameterValueString = ParameterSetViewModel.GetDelimitedParameter(parameterValueString, "\"", "\"");
                else if (parameterType.IsScriptBlock)
                    parameterValueString = ParameterSetViewModel.GetDelimitedParameter(parameterValueString, "{", "}");
                    parameterValueString = ParameterSetViewModel.GetDelimitedParameter(parameterValueString, "(", ")");
                builder.Append($"-{parameter.Name} {parameterValueString} ");
            return builder.ToString().Trim();
        /// Gets the individual parameter count of this parameterset.
        /// <returns>Return individual parameter count of this parameterset.</returns>
        public int GetIndividualParameterCount()
            foreach (ParameterViewModel p in this.Parameters)
                if (p.IsInSharedParameterSet)
        #region Internal Method
        /// Compare source parametermodel is equal like target parametermodel.
        /// <param name="source">The source of parametermodel.</param>
        /// <param name="target">The target of parametermodel.</param>
        internal static int Compare(ParameterViewModel source, ParameterViewModel target)
            if (source.Parameter.IsMandatory && !target.Parameter.IsMandatory)
            if (!source.Parameter.IsMandatory && target.Parameter.IsMandatory)
            return string.Compare(source.Parameter.Name, target.Parameter.Name);
        /// Gets the delimited parameter if it needs delimitation and is not delimited.
        /// <param name="parameterValue">Value needing delimitation.</param>
        /// <param name="openDelimiter">Open delimitation.</param>
        /// <param name="closeDelimiter">Close delimitation.</param>
        /// <returns>The delimited parameter if it needs delimitation and is not delimited.</returns>
        private static string GetDelimitedParameter(string parameterValue, string openDelimiter, string closeDelimiter)
            string parameterValueTrimmed = parameterValue.Trim();
            if (parameterValueTrimmed.Length == 0)
                return openDelimiter + parameterValue + closeDelimiter;
            char delimitationChar = ParameterSetViewModel.ParameterNeedsDelimitation(parameterValueTrimmed, openDelimiter.Length == 1 && openDelimiter[0] == '{');
            switch (delimitationChar)
                case '1':
                    return '\'' + parameterValue + '\'';
                case '\"':
                    return '\"' + parameterValue + '\"';
                    return parameterValueTrimmed;
        /// Returns '0' if the <paramref name="parameterValue"/> does not need delimitation, '1' if it does, and a quote character if it needs to be delimited with a quote.
        /// <param name="parameterValue">Parameter value to check.</param>
        /// <param name="requireScriptblock">True if the parameter value should be a scriptblock.</param>
        /// <returns>'0' if the parameter does not need delimitation, '1' if it needs, '\'' if it needs to be delimited with single quote and '\"' if it needs to be delimited with double quotes.</returns>
        private static char ParameterNeedsDelimitation(string parameterValue, bool requireScriptblock)
            Token[] tokens;
            ParseError[] errors;
            ScriptBlockAst values = Parser.ParseInput("commandName -parameterName " + parameterValue, out tokens, out errors);
            if (values == null || values.EndBlock == null || values.EndBlock.Statements.Count == 0)
                return '1';
            PipelineAst pipeline = values.EndBlock.Statements[0] as PipelineAst;
            if (pipeline == null || pipeline.PipelineElements.Count == 0)
            CommandAst commandAst = pipeline.PipelineElements[0] as CommandAst;
            if (commandAst == null || commandAst.CommandElements.Count == 0)
            // 3 is for CommandName, Parameter and its value
            if (commandAst.CommandElements.Count != 3)
            if (requireScriptblock)
                ScriptBlockExpressionAst scriptAst = commandAst.CommandElements[2] as ScriptBlockExpressionAst;
                return scriptAst == null ? '1' : '0';
            StringConstantExpressionAst stringValue = commandAst.CommandElements[2] as StringConstantExpressionAst;
            if (stringValue != null)
                if (errors.Length == 0)
                    return '0';
                char stringTerminationChar;
                if (stringValue.StringConstantType == StringConstantType.BareWord)
                    stringTerminationChar = parameterValue[0];
                else if (stringValue.StringConstantType == StringConstantType.DoubleQuoted || stringValue.StringConstantType == StringConstantType.DoubleQuotedHereString)
                    stringTerminationChar = '\"';
                    stringTerminationChar = '\'';
                char oppositeTerminationChar = stringTerminationChar == '\"' ? '\'' : '\"';
                // If the string is not terminated, it should be delimited by the opposite string termination character
                return oppositeTerminationChar;
            if (errors.Length != 0)
        /// Called to evaluate the value of AllMandatoryParametersHaveValues.
        private void EvaluateAllMandatoryParametersHaveValues()
            bool newCanRun = true;
                if (!parameter.HasValue)
                    newCanRun = false;
            this.AllMandatoryParametersHaveValues = newCanRun;
        /// Used to track changes to parameter values in order to verify the enabled state of buttons.
        /// <param name="sender">Event arguments.</param>
        /// <param name="e">Event sender.</param>
        private void MandatoryParameter_PropertyChanged(object sender, PropertyChangedEventArgs e)
            if (!e.PropertyName.Equals("Value", StringComparison.Ordinal))
