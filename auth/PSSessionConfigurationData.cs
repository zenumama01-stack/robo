    public sealed class PSSessionConfigurationData
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static bool IsServerManager;
#pragma warning restore CA2211 // Non-constant fields should not be visible
        public List<string> ModulesToImport
        internal List<object> ModulesToImportInternal
                return _modulesToImportInternal;
        public string PrivateData
        private PSSessionConfigurationData()
        internal static string Unescape(string s)
            StringBuilder sb = new StringBuilder(s);
            sb.Replace("&lt;", "<");
            sb.Replace("&gt;", ">");
            sb.Replace("&quot;", "\"");
            sb.Replace("&apos;", "'");
            sb.Replace("&amp;", "&");
        internal static PSSessionConfigurationData Create(string configurationData)
            PSSessionConfigurationData configuration = new PSSessionConfigurationData();
            if (string.IsNullOrEmpty(configurationData))
                return configuration;
            configurationData = Unescape(configurationData);
            XmlReaderSettings readerSettings = new XmlReaderSettings
                MaxCharactersInDocument = 10000,
                XmlResolver = null,
                ConformanceLevel = ConformanceLevel.Fragment
            using (XmlReader reader = XmlReader.Create(new StringReader(configurationData), readerSettings))
                // read the header <SessionConfigurationData>
                if (reader.ReadToFollowing(SessionConfigToken))
                    bool isParamFound = reader.ReadToDescendant(ParamToken);
                        if (!reader.MoveToAttribute(NameToken))
                            throw PSTraceSource.NewArgumentException(configurationData,
                                NameToken, ValueToken, ParamToken);
                        if (string.Equals(optionName, PrivateDataToken, StringComparison.OrdinalIgnoreCase))
                            // this is a PrivateData element which we
                            // need to process
                            if (reader.ReadToFollowing(PrivateDataToken))
                                string privateData = reader.ReadOuterXml();
                                AssertValueNotAssigned(PrivateDataToken, configuration._privateData);
                                configuration._privateData = privateData;
                            if (!reader.MoveToAttribute(ValueToken))
                            configuration.Update(optionName, optionValue);
                        isParamFound = reader.ReadToFollowing(ParamToken);
            configuration.CreateCollectionIfNecessary();
        private List<string> _modulesToImport;
        private List<object> _modulesToImportInternal;
        private string _privateData;
                    RemotingErrorIdStrings.DuplicateInitializationParameterFound, optionName, SessionConfigToken);
                case ModulesToImportToken:
                        AssertValueNotAssigned(ModulesToImportToken, _modulesToImport);
                        _modulesToImport = new List<string>();
                        _modulesToImportInternal = new List<object>();
                        object[] modulesToImport = optionValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var module in modulesToImport)
                            var s = module as string;
                            if (s != null)
                                _modulesToImport.Add(s.Trim());
                                if (ModuleSpecification.TryParse(s, out moduleSpec))
                                    _modulesToImportInternal.Add(moduleSpec);
                                    _modulesToImportInternal.Add(s.Trim());
                        Dbg.Assert(false, "Unknown option specified");
        private void CreateCollectionIfNecessary()
            _modulesToImport ??= new List<string>();
            _modulesToImportInternal ??= new List<object>();
        private const string SessionConfigToken = "SessionConfigurationData";
        internal const string ModulesToImportToken = "modulestoimport";
        internal const string PrivateDataToken = "PrivateData";
        internal const string InProcActivityToken = "InProcActivity";
        private const string ParamToken = "Param";
        private const string NameToken = "Name";
        private const string ValueToken = "Value";
