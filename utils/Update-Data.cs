    /// This is the base class for update-typedata and update-formatdata.
    public class UpdateData : PSCmdlet
        /// File parameter set name.
        protected const string FileParameterSet = "FileSet";
        /// Files to append to the existing set.
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true,
            ParameterSetName = FileParameterSet)]
        public string[] AppendPath { get; set; } = Array.Empty<string>();
        /// Files to prepend to the existing set.
        [Parameter(ParameterSetName = FileParameterSet)]
        public string[] PrependPath { get; set; } = Array.Empty<string>();
        private static void ReportWrongExtension(string file, string errorId, PSCmdlet cmdlet)
                PSTraceSource.NewInvalidOperationException(UpdateDataStrings.UpdateData_WrongExtension, file, "ps1xml"),
        private static void ReportWrongProviderType(string providerId, string errorId, PSCmdlet cmdlet)
                PSTraceSource.NewInvalidOperationException(UpdateDataStrings.UpdateData_WrongProviderError, providerId),
        /// <param name="files"></param>
        internal static Collection<string> Glob(string[] files, string errorId, PSCmdlet cmdlet)
            Collection<string> retValue = new();
            foreach (string file in files)
                Collection<string> providerPaths;
                    providerPaths = cmdlet.SessionState.Path.GetResolvedProviderPathFromPSPath(file, out provider);
                catch (SessionStateException e)
                    cmdlet.WriteError(new ErrorRecord(e, errorId, ErrorCategory.InvalidOperation, file));
                if (!provider.NameEquals(cmdlet.Context.ProviderNames.FileSystem))
                    ReportWrongProviderType(provider.FullName, errorId, cmdlet);
                foreach (string providerPath in providerPaths)
                    if (!providerPath.EndsWith(".ps1xml", StringComparison.OrdinalIgnoreCase))
                        ReportWrongExtension(providerPath, "WrongExtension", cmdlet);
                    retValue.Add(providerPath);
            return retValue;
