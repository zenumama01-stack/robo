    [Cmdlet(VerbsCommon.Clear, "Content", DefaultParameterSetName = "Path", SupportsShouldProcess = true, SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096807")]
    public class ClearContentCommand : ContentCommandBase
        #region Command code
        /// Clears the contents from the item at the specified path.
            // Default to the CmdletProviderContext that will direct output to
            // the pipeline.
            CmdletProviderContext currentCommandContext = CmdletProviderContext;
            currentCommandContext.PassThru = false;
            foreach (string path in Path)
                    InvokeProvider.Content.Clear(path, currentCommandContext);
                    WriteError(
                            notSupported.ErrorRecord,
                            notSupported));
                catch (DriveNotFoundException driveNotFound)
                            driveNotFound.ErrorRecord,
                            driveNotFound));
                            providerNotFound.ErrorRecord,
                            providerNotFound));
                            pathNotFound.ErrorRecord,
                            pathNotFound));
        #endregion Command code
        /// Determines if the provider for the specified path supports ShouldProcess.
        /// <value></value>
        protected override bool ProviderSupportsShouldProcess
                return base.DoesProviderSupportShouldProcess(base.Path);
        /// A virtual method for retrieving the dynamic parameters for a cmdlet. Derived cmdlets
        /// that require dynamic parameters should override this method and return the
        /// dynamic parameter object.
        /// <param name="context">
        /// The context under which the command is running.
        /// An object representing the dynamic parameters for the cmdlet or null if there
        /// are none.
        internal override object GetDynamicParameters(CmdletProviderContext context)
            if (Path != null && Path.Length > 0)
                // Go ahead an let any exceptions terminate the pipeline.
                return InvokeProvider.Content.ClearContentDynamicParameters(Path[0], context);
            return InvokeProvider.Content.ClearContentDynamicParameters(".", context);
