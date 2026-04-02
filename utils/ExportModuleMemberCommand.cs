// Now define the set of commands for manipulating modules.
    #region Export-ModuleMember
    /// Implements a cmdlet that loads a module.
    [Cmdlet(VerbsData.Export, "ModuleMember", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096578")]
    public sealed class ExportModuleMemberCommand : PSCmdlet
        /// This parameter specifies the functions to import from the module...
        public string[] Function
                return _functionList;
                _functionList = value;
                // Create the list of patterns to match at parameter bind time
                // so errors will be reported before loading the module...
                _functionPatterns = new List<WildcardPattern>();
                if (_functionList != null)
                    foreach (string pattern in _functionList)
                        _functionPatterns.Add(WildcardPattern.Get(pattern, WildcardOptions.IgnoreCase));
        private string[] _functionList;
        private List<WildcardPattern> _functionPatterns;
        public string[] Cmdlet
                return _cmdletList;
                _cmdletList = value;
                _cmdletPatterns = new List<WildcardPattern>();
                if (_cmdletList != null)
                    foreach (string pattern in _cmdletList)
                        _cmdletPatterns.Add(WildcardPattern.Get(pattern, WildcardOptions.IgnoreCase));
        private string[] _cmdletList;
        private List<WildcardPattern> _cmdletPatterns;
        /// This parameter specifies the variables to import from the module...
        public string[] Variable
                return _variableExportList;
                _variableExportList = value;
                _variablePatterns = new List<WildcardPattern>();
                if (_variableExportList != null)
                    foreach (string pattern in _variableExportList)
                        _variablePatterns.Add(WildcardPattern.Get(pattern, WildcardOptions.IgnoreCase));
        private string[] _variableExportList;
        private List<WildcardPattern> _variablePatterns;
        /// This parameter specifies the aliases to import from the module...
        public string[] Alias
                return _aliasExportList;
                _aliasExportList = value;
                _aliasPatterns = new List<WildcardPattern>();
                if (_aliasExportList != null)
                    foreach (string pattern in _aliasExportList)
                        _aliasPatterns.Add(WildcardPattern.Get(pattern, WildcardOptions.IgnoreCase));
        private string[] _aliasExportList;
        private List<WildcardPattern> _aliasPatterns;
        /// Export the specified functions...
            if (Context.EngineSessionState == Context.TopLevelSessionState)
                string message = StringUtil.Format(Modules.CanOnlyBeUsedFromWithinAModule);
                InvalidOperationException invalidOp = new InvalidOperationException(message);
                ErrorRecord er = new ErrorRecord(invalidOp, "Modules_CanOnlyExecuteExportModuleMemberInsideAModule",
                    ErrorCategory.PermissionDenied, null);
            // Prevent script injection attack by disallowing ExportModuleMemberCommand to export module members across
            // language boundaries. This will prevent injected untrusted script from exporting private trusted module functions.
            if (Context.EngineSessionState.Module?.LanguageMode != null &&
                Context.LanguageMode != Context.EngineSessionState.Module.LanguageMode)
                    var se = new PSSecurityException(Modules.CannotExportMembersAccrossLanguageBoundaries);
                    var er = new ErrorRecord(se, "Modules_CannotExportMembersAccrossLanguageBoundaries", ErrorCategory.SecurityError, this);
                    title: Modules.WDACExportModuleCommandLogTitle,
                    message: StringUtil.Format(Modules.WDACExportModuleCommandLogMessage, Context.EngineSessionState.Module.Name, Context.EngineSessionState.Module.LanguageMode, Context.LanguageMode),
                    fqid: "ExportModuleMemberCmdletNotAllowed",
            ModuleIntrinsics.ExportModuleMembers(this,
                this.Context.EngineSessionState,
                _functionPatterns, _cmdletPatterns, _aliasPatterns, _variablePatterns, null);
    #endregion Export-ModuleMember
