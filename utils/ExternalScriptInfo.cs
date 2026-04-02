    /// Provides information for scripts that are directly executable by PowerShell
    /// but are not built into the runspace configuration.
    public class ExternalScriptInfo : CommandInfo, IScriptCommandInfo
        /// Creates an instance of the ExternalScriptInfo class with the specified name, and path.
        /// The name of the script.
        /// The path to the script
        /// The context of the currently running engine.
        /// If <paramref name="path"/> is null or empty.
        internal ExternalScriptInfo(string name, string path, ExecutionContext context)
            : base(name, CommandTypes.ExternalScript, context)
            Diagnostics.Assert(IO.Path.IsPathRooted(path), "Caller makes sure that 'path' is already resolved.");
            // Path might contain short-name syntax such as 'DOCUME~1'. Use Path.GetFullPath to expand the short name
            _path = IO.Path.GetFullPath(path);
            CommonInitialization();
        /// Creates an instance of ExternalScriptInfo that has no ExecutionContext.
        /// This is used exclusively to pass it to the AuthorizationManager that just uses the path parameter.
        internal ExternalScriptInfo(string name, string path) : base(name, CommandTypes.ExternalScript)
        internal ExternalScriptInfo(ExternalScriptInfo other)
            _path = other._path;
        /// Common initialization for all constructors.
        private void CommonInitialization()
            // Assume external scripts are untrusted by default (for Get-Command, etc)
            // until we've actually parsed their script block.
            if (SystemPolicy.GetSystemLockdownPolicy() != SystemEnforcementMode.None)
                // Get the lock down policy with no handle. This only impacts command discovery,
                // as the real language mode assignment will be done when we read the script
                // contents.
                switch (SystemPolicy.GetLockdownPolicy(_path, null))
                    case SystemEnforcementMode.None:
                        DefiningLanguageMode = PSLanguageMode.FullLanguage;
                    case SystemEnforcementMode.Audit:
                        // For policy audit mode, language mode is set to CL but audit messages are emitted to log
                        // instead of applying restrictions.
                        DefiningLanguageMode = PSLanguageMode.ConstrainedLanguage;
                    case SystemEnforcementMode.Enforce:
        internal override CommandInfo CreateGetCommandCopy(object[] argumentList)
            ExternalScriptInfo copy = new ExternalScriptInfo(this) { IsGetCommandCopy = true, Arguments = argumentList };
            get { return HelpCategory.ExternalScript; }
        /// Gets the path to the script file.
        private readonly string _path = string.Empty;
        internal override string Syntax
                            Globalization.CultureInfo.CurrentCulture,
                            "{0} {1}",
                            parameterSet));
                    return SessionStateEntryVisibility.Public;
                return Context.EngineSessionState.CheckScriptVisibility(_path);
        /// The script block that represents the external script.
        public ScriptBlock ScriptBlock
                if (_scriptBlock == null)
                    // Skip ShouldRun check for .psd1 files.
                    // Use ValidateScriptInfo() for explicitly validating the checkpolicy for psd1 file.
                    if (!_path.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase))
                        ValidateScriptInfo(null);
                    // parse the script into an expression tree...
                    ScriptBlock newScriptBlock = ParseScriptContents(new Parser(), _path, ScriptContents, DefiningLanguageMode);
                    this.ScriptBlock = newScriptBlock;
                    _scriptBlock.LanguageMode = this.DefiningLanguageMode;
        private ScriptBlockAst _scriptBlockAst;
        private static ScriptBlock ParseScriptContents(Parser parser, string fileName, string fileContents, PSLanguageMode? definingLanguageMode)
            // If we are in ConstrainedLanguage mode but the defining language mode is FullLanguage, then we need
            // to parse the script contents in FullLanguage mode context.  Otherwise we will get bogus parsing errors
            // such as "Configuration keyword not allowed".
            if (definingLanguageMode.HasValue && (definingLanguageMode == PSLanguageMode.FullLanguage))
                if ((context != null) && (context.LanguageMode == PSLanguageMode.ConstrainedLanguage))
                    context.LanguageMode = PSLanguageMode.FullLanguage;
                        return ScriptBlock.Create(parser, fileName, fileContents);
                        context.LanguageMode = PSLanguageMode.ConstrainedLanguage;
        internal ScriptBlockAst GetScriptBlockAst()
            var scriptContents = ScriptContents;
                this.ScriptBlock = ScriptBlock.TryGetCachedScriptBlock(_path, scriptContents);
                return (ScriptBlockAst)_scriptBlock.Ast;
            if (_scriptBlockAst == null)
                Parser parser = new Parser();
                // such as "Configuration or Class keyword not allowed".
                if (context != null && context.LanguageMode == PSLanguageMode.ConstrainedLanguage &&
                    DefiningLanguageMode == PSLanguageMode.FullLanguage)
                        _scriptBlockAst = parser.Parse(_path, ScriptContents, null, out errors, ParseMode.Default);
                    this.ScriptBlock = new ScriptBlock(_scriptBlockAst, isFilter: false);
                    ScriptBlock.CacheScriptBlock(_scriptBlock.Clone(), _path, scriptContents);
            return _scriptBlockAst;
        /// Validates the external script info.
        public void ValidateScriptInfo(Host.PSHost host)
            if (!_signatureChecked)
                ExecutionContext context = Context ?? LocalPipeline.GetExecutionContextFromTLS();
                ReadScriptContents();
                // We have no way to check the signature w/o context because we don't have
                // an AuthorizationManager.  This can happen during initialization when trying
                // to get the CommandMetadata for a script (either to prepopulate the metadata
                // or creating a proxy).  If context can be null under any other circumstances,
                // we need to make sure it's acceptable if the parser is invoked on unsigned scripts.
                    CommandDiscovery.ShouldRun(context, host, this, CommandOrigin.Internal);
                    _signatureChecked = true;
        /// The output type(s) is specified in the script block.
            get { return ScriptBlock.OutputType; }
        internal bool SignatureChecked
            set { _signatureChecked = value; }
        private bool _signatureChecked;
        /// The command metadata for the script.
                return _commandMetadata ??=
                    new CommandMetadata(this.ScriptBlock, this.Name, LocalPipeline.GetExecutionContextFromTLS());
        private CommandMetadata _commandMetadata;
                    return ScriptBlock.HasDynamicParameters;
                catch (ParseException) { }
                catch (ScriptRequiresException) { }
                // If we got here, there was some sort of parsing exception.  We'll just
                // ignore it and assume the script does not implement dynamic parameters.
                // Furthermore, we'll clear out the fields so that the next attempt to
                // access ScriptBlock will result in an exception that doesn't get ignored.
                _scriptBlock = null;
                _scriptContents = null;
        private ScriptRequirements GetRequiresData()
            return GetScriptBlockAst().ScriptRequirements;
        internal string RequiresApplicationID
                var data = GetRequiresData();
                return data?.RequiredApplicationId;
        internal uint ApplicationIDLineNumber
        internal Version RequiresPSVersion
                return data?.RequiredPSVersion;
        internal IEnumerable<string> RequiresPSEditions
                return data?.RequiredPSEditions;
        internal IEnumerable<ModuleSpecification> RequiresModules
                return data?.RequiredModules;
        internal bool RequiresElevation
                return data != null && data.IsElevationRequired;
        internal uint PSVersionLineNumber
        /// Gets the original contents of the script.
        public string ScriptContents
                if (_scriptContents == null)
                return _scriptContents;
        private string _scriptContents;
        /// Gets the original encoding of the script.
        public Encoding OriginalEncoding
                return _originalEncoding;
        private Encoding _originalEncoding;
        private void ReadScriptContents()
                // make sure we can actually load the script and that it's non-empty
                // before we call it.
                // Note, although we are passing ASCII as the encoding, the StreamReader
                // class still obeys the byte order marks at the beginning of the file
                // if present. If not present, then ASCII is used as the default encoding.
                    using (FileStream readerStream = new FileStream(_path, FileMode.Open, FileAccess.Read))
                        using (StreamReader scriptReader = new StreamReader(readerStream, Encoding.Default))
                            _scriptContents = scriptReader.ReadToEnd();
                            _originalEncoding = scriptReader.CurrentEncoding;
                            // Check this file against any system wide enforcement policies.
                            SystemScriptFileEnforcement filePolicyEnforcement = SystemPolicy.GetFilePolicyEnforcement(_path, readerStream);
                            switch (filePolicyEnforcement)
                                case SystemScriptFileEnforcement.None:
                                        DefiningLanguageMode = Context.LanguageMode;
                                case SystemScriptFileEnforcement.Allow:
                                case SystemScriptFileEnforcement.AllowConstrained:
                                case SystemScriptFileEnforcement.AllowConstrainedAudit:
                                        title: SecuritySupportStrings.ExternalScriptWDACLogTitle,
                                        message: string.Format(Globalization.CultureInfo.CurrentUICulture, SecuritySupportStrings.ExternalScriptWDACLogMessage, _path),
                                        fqid: "ScriptFileNotTrustedByPolicy");
                                    // We set the language mode to Constrained Language, even though in policy audit mode no restrictions are applied
                                    // and instead an audit log message is generated wherever a restriction would be applied.
                                case SystemScriptFileEnforcement.Block:
                                    throw new PSSecurityException(
                                            Globalization.CultureInfo.CurrentUICulture,
                                            SecuritySupportStrings.ScriptFileBlockedBySystemPolicy,
                                            _path));
                                            SecuritySupportStrings.UnknownSystemScriptFileEnforcement,
                                            filePolicyEnforcement));
                    // This catches PSArgumentException as well.
                    ThrowCommandNotFoundException(e);
                    // this is unadvertised exception thrown by the StreamReader ctor when
                    // no permission to read the script file
        private static void ThrowCommandNotFoundException(Exception innerException)
            CommandNotFoundException cmdE = new CommandNotFoundException(innerException.Message, innerException);
            throw cmdE;
    /// Thrown when fail to parse #requires statements. Caught by CommandDiscovery.
    internal class ScriptRequiresSyntaxException : ScriptRequiresException
        internal ScriptRequiresSyntaxException(string message)
    /// Defines the name and version tuple of a PSSnapin.
    public class PSSnapInSpecification
        internal PSSnapInSpecification(string psSnapinName)
            PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(psSnapinName);
            Name = psSnapinName;
            Version = null;
        /// The name of the snapin.
        /// The version of the snapin.
        public Version Version { get; internal set; }
