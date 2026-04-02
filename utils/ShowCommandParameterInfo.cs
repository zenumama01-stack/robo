    /// Implements a facade around ShowCommandParameterInfo and its deserialized counterpart.
    public class ShowCommandParameterInfo
        /// Initializes a new instance of the <see cref="ShowCommandParameterInfo"/> class
        /// with the specified <see cref="CommandParameterInfo"/>.
        public ShowCommandParameterInfo(CommandParameterInfo other)
            this.IsMandatory = other.IsMandatory;
            this.ValueFromPipeline = other.ValueFromPipeline;
            this.ParameterType = new ShowCommandParameterType(other.ParameterType);
            this.Position = other.Position;
            var validateSetAttribute = other.Attributes.Where(static x => typeof(ValidateSetAttribute).IsAssignableFrom(x.GetType())).Cast<ValidateSetAttribute>().LastOrDefault();
            if (validateSetAttribute != null)
                this.HasParameterSet = true;
                this.ValidParamSetValues = validateSetAttribute.ValidValues;
        /// Initializes a new instance of the <see cref="ShowCommandParameterInfo"/> class.
        /// Creates an instance of the ShowCommandParameterInfo class based on a PSObject object.
        public ShowCommandParameterInfo(PSObject other)
            this.IsMandatory = (bool)(other.Members["IsMandatory"].Value);
            this.ValueFromPipeline = (bool)(other.Members["ValueFromPipeline"].Value);
            this.HasParameterSet = (bool)(other.Members["HasParameterSet"].Value);
            this.ParameterType = new ShowCommandParameterType(other.Members["ParameterType"].Value as PSObject);
            this.Position = (int)(other.Members["Position"].Value);
            if (this.HasParameterSet)
                this.ValidParamSetValues = ShowCommandCommandInfo.GetObjectEnumerable((other.Members["ValidParamSetValues"].Value as PSObject).BaseObject as System.Collections.ArrayList).Cast<string>().ToList();
        /// Gets the name of the parameter.
        /// True if the parameter is dynamic, or false otherwise.
        public bool IsMandatory { get; }
        /// Gets whether the parameter can take values from the incoming pipeline object.
        public bool ValueFromPipeline { get; }
        /// Gets the type of the parameter.
        public ShowCommandParameterType ParameterType { get; }
        /// The possible values of this parameter.
        public IList<string> ValidParamSetValues { get; }
        /// Gets whether the parameter has a parameter set.
        public bool HasParameterSet { get; }
        /// Gets the position in which the parameter can be specified on the command line
        /// if not named. If the returned value is int.MinValue then the parameter must be named.
        public int Position { get; }
