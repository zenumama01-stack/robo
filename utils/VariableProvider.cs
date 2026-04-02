    /// This provider is the data accessor for shell variables. It uses
    /// the HashtableProvider as the base class to get a hashtable as
    /// a data store.
    [CmdletProvider(VariableProvider.ProviderName, ProviderCapabilities.ShouldProcess)]
    [OutputType(typeof(PSVariable), ProviderCmdlet = ProviderCmdlet.SetItem)]
    [OutputType(typeof(PSVariable), ProviderCmdlet = ProviderCmdlet.RenameItem)]
    [OutputType(typeof(PSVariable), ProviderCmdlet = ProviderCmdlet.CopyItem)]
    [OutputType(typeof(PSVariable), ProviderCmdlet = ProviderCmdlet.GetItem)]
    [OutputType(typeof(PSVariable), ProviderCmdlet = ProviderCmdlet.NewItem)]
    public sealed class VariableProvider : SessionStateProviderBase
        public const string ProviderName = "Variable";
        public VariableProvider()
        /// Initializes the variables drive.
        /// An array of a single PSDriveInfo object representing the variables drive.
            string description = SessionStateStrings.VariableDriveDescription;
            PSDriveInfo variableDrive =
                    DriveNames.VariableDrive,
            drives.Add(variableDrive);
        /// Gets a variable from session state.
        /// A PSVariable that represents the variable.
            return (PSVariable)SessionState.Internal.GetVariable(name, Context.Origin);
        /// Sets the variable of the specified name to the specified value.
        /// The new value for the variable.
            PSVariable variable = null;
                variable = value as PSVariable;
                    variable = new PSVariable(name, value);
                    // ensure the name matches
                    if (!string.Equals(name, variable.Name, StringComparison.OrdinalIgnoreCase))
                        PSVariable newVar = new PSVariable(name, variable.Value, variable.Options, variable.Attributes);
                        newVar.Description = variable.Description;
                        variable = newVar;
                variable = new PSVariable(name, null);
            PSVariable item = SessionState.Internal.SetVariable(variable, Force, Context.Origin) as PSVariable;
        /// The name of the variable to remove from session state.
            SessionState.Internal.RemoveVariable(name, Force);
        /// Gets a flattened view of the variables in session state.
        /// An IDictionary representing the flattened view of the variables in
            return (IDictionary)SessionState.Internal.GetVariableTable();
        /// Gets the value of the item that is returned from GetItem by
        /// extracting the PSVariable value.
            // Call the base class to unwrap the DictionaryEntry
            // if necessary
            object value = base.GetValueOfItem(item);
            PSVariable var = item as PSVariable;
                value = var.Value;
            PSVariable variable = item as PSVariable;
                if ((variable.Options & ScopedItemOptions.Constant) != 0 ||
                    ((variable.Options & ScopedItemOptions.ReadOnly) != 0 && !Force))
                            "CannotRenameVariable",
                            SessionStateStrings.CannotRenameVariable);
