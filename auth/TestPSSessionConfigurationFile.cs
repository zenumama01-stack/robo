    /// Test-PSSessionConfigurationFile command implementation
    /// See Declarative Initial Session Config (DISC)
    [Cmdlet(VerbsDiagnostic.Test, "PSSessionConfigurationFile", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096797")]
    public class TestPSSessionConfigurationFileCommand : PSCmdlet
                if (this.Context.EngineSessionState.IsProviderLoaded(Context.ProviderNames.FileSystem))
                    filePaths = SessionState.Path.GetResolvedProviderPathFromPSPath(_path, out provider);
                string message = StringUtil.Format(RemotingErrorIdStrings.PSSessionConfigurationFileNotFound, _path);
                ErrorRecord er = new ErrorRecord(fnf, "PSSessionConfigurationFileNotFound",
            if (ext.Equals(StringLiterals.PowerShellDISCFileExtension, StringComparison.OrdinalIgnoreCase))
                    WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.DISCErrorParsingConfigFile, filePath, e.Message));
                DISCUtils.ExecutionPolicyType = typeof(ExecutionPolicy);
                WriteObject(DISCUtils.VerifyConfigTable(configTable, this, filePath));
