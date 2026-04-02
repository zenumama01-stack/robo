    /// Implements a facade around CommandParameterSetInfo and its deserialized counterpart.
    public class ShowCommandParameterSetInfo
        /// Initializes a new instance of the <see cref="ShowCommandParameterSetInfo"/> class
        /// with the specified <see cref="CommandParameterSetInfo"/>.
        public ShowCommandParameterSetInfo(CommandParameterSetInfo other)
            this.IsDefault = other.IsDefault;
            this.Parameters = other.Parameters.Select(static x => new ShowCommandParameterInfo(x)).ToArray();
        public ShowCommandParameterSetInfo(PSObject other)
            this.IsDefault = (bool)(other.Members["IsDefault"].Value);
            var parameters = (other.Members["Parameters"].Value as PSObject).BaseObject as System.Collections.ArrayList;
            this.Parameters = ShowCommandCommandInfo.GetObjectEnumerable(parameters).Cast<PSObject>().Select(static x => new ShowCommandParameterInfo(x)).ToArray();
        /// Gets the name of the parameter set.
        /// Gets whether the parameter set is the default parameter set.
        public bool IsDefault { get; }
        /// Gets the parameter information for the parameters in this parameter set.
        public ICollection<ShowCommandParameterInfo> Parameters { get; }
