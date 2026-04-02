    /// This provider is the data accessor for shell functions. It uses
    [CmdletProvider(FunctionProvider.ProviderName, ProviderCapabilities.ShouldProcess)]
    [OutputType(typeof(FunctionInfo), ProviderCmdlet = ProviderCmdlet.SetItem)]
    [OutputType(typeof(FunctionInfo), ProviderCmdlet = ProviderCmdlet.RenameItem)]
    [OutputType(typeof(FunctionInfo), ProviderCmdlet = ProviderCmdlet.CopyItem)]
    [OutputType(typeof(FunctionInfo), ProviderCmdlet = ProviderCmdlet.GetChildItem)]
    [OutputType(typeof(FunctionInfo), ProviderCmdlet = ProviderCmdlet.GetItem)]
    [OutputType(typeof(FunctionInfo), ProviderCmdlet = ProviderCmdlet.NewItem)]
    public sealed class FunctionProvider : SessionStateProviderBase
        public const string ProviderName = "Function";
        public FunctionProvider()
        /// Initializes the function drive.
        /// An array of a single PSDriveInfo object representing the functions drive.
            string description = SessionStateStrings.FunctionDriveDescription;
            PSDriveInfo functionDrive =
                    DriveNames.FunctionDrive,
            drives.Add(functionDrive);
        /// An instance of FunctionProviderDynamicParameters which is the dynamic parameters for
            return new FunctionProviderDynamicParameters();
        /// Gets a function from session state.
        /// A ScriptBlock that represents the function.
            CommandInfo function = SessionState.Internal.GetFunction(name, Context.Origin);
        /// Sets the function of the specified name to the specified value.
        /// The new value for the function.
            FunctionProviderDynamicParameters dynamicParameters =
                DynamicParameters as FunctionProviderDynamicParameters;
            CommandInfo modifiedItem = null;
                // If the value wasn't specified but the options were, just set the
                // options on the existing function.
                // If the options weren't specified, then remove the function
                    modifiedItem = (CommandInfo)GetSessionStateItem(name);
                    if (modifiedItem != null)
                        SetOptions(modifiedItem, dynamicParameters.Options);
                    // Unwrap the PSObject before binding it as a scriptblock...
                    PSObject pso = value as PSObject;
                        value = pso.BaseObject;
                    ScriptBlock scriptBlockValue = value as ScriptBlock;
                    if (scriptBlockValue != null)
                            modifiedItem = SessionState.Internal.SetFunction(name, scriptBlockValue,
                                null, dynamicParameters.Options, Force, Context.Origin);
                            modifiedItem = SessionState.Internal.SetFunction(name, scriptBlockValue, null, Force, Context.Origin);
                    FunctionInfo function = value as FunctionInfo;
                    if (function != null)
                        ScopedItemOptions options = function.Options;
                            options = dynamicParameters.Options;
                        modifiedItem = SessionState.Internal.SetFunction(name, function.ScriptBlock, function, options, Force, Context.Origin);
                        ScriptBlock scriptBlock = ScriptBlock.Create(Context.ExecutionContext, stringValue);
                            modifiedItem = SessionState.Internal.SetFunction(name, scriptBlock, null, dynamicParameters.Options, Force, Context.Origin);
                            modifiedItem = SessionState.Internal.SetFunction(name, scriptBlock, null, Force, Context.Origin);
                if (writeItem && modifiedItem != null)
                    WriteItemObject(modifiedItem, modifiedItem.Name, false);
        private static void SetOptions(CommandInfo function, ScopedItemOptions options)
            ((FunctionInfo)function).Options = options;
        /// Removes the specified function from session state.
        /// The name of the function to remove from session state.
            SessionState.Internal.RemoveFunction(name, Force);
            FunctionInfo function = item as FunctionInfo;
                value = function.ScriptBlock;
        /// Gets a flattened view of the functions in session state.
        /// An IDictionary representing the flattened view of the functions in
            return (IDictionary)SessionState.Internal.GetFunctionTable();
            FunctionInfo functionInfo = item as FunctionInfo;
                if ((functionInfo.Options & ScopedItemOptions.Constant) != 0 ||
                    ((functionInfo.Options & ScopedItemOptions.ReadOnly) != 0 && !Force))
                            functionInfo.Name,
                            "CannotRenameFunction",
                            SessionStateStrings.CannotRenameFunction);
    /// The dynamic parameter object for the FunctionProvider SetItem and NewItem commands.
    public class FunctionProviderDynamicParameters
        /// Gets or sets the option parameter for the function.
        private bool _optionsSet;
