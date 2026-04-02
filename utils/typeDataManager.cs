    /// Class to manage the database instances, do the reloading, etc.
    internal sealed class TypeInfoDataBaseManager
        /// Instance of the object holding the format.ps1xml in memory database.
        internal TypeInfoDataBase Database { get; private set; }
        // for locking the F&O database
        internal object databaseLock = new object();
        // for locking the update from XMLs
        internal object updateDatabaseLock = new object();
        // this is used to throw errors when updating a shared TypeTable.
        internal bool isShared;
        private readonly List<string> _formatFileList;
        internal bool DisableFormatTableUpdates { get; set; }
        internal TypeInfoDataBaseManager()
            isShared = false;
            _formatFileList = new List<string>();
        /// <param name="formatFiles"></param>
        /// <param name="isShared"></param>
        /// <exception cref="ArgumentNullException"/>
        /// 1. FormatFile is not rooted.
        /// 1. There were errors loading Formattable. Look in the Errors property to get
        /// detailed error messages.
        internal TypeInfoDataBaseManager(
            IEnumerable<string> formatFiles,
            bool isShared,
            AuthorizationManager authorizationManager,
            PSHost host)
            Collection<PSSnapInTypeAndFormatErrors> filesToLoad = new Collection<PSSnapInTypeAndFormatErrors>();
            ConcurrentBag<string> errors = new ConcurrentBag<string>();
            foreach (string formatFile in formatFiles)
                if (string.IsNullOrEmpty(formatFile) || (!Path.IsPathRooted(formatFile)))
                    throw PSTraceSource.NewArgumentException(nameof(formatFiles), FormatAndOutXmlLoadingStrings.FormatFileNotRooted, formatFile);
                PSSnapInTypeAndFormatErrors fileToLoad = new PSSnapInTypeAndFormatErrors(string.Empty, formatFile);
                fileToLoad.Errors = errors;
                filesToLoad.Add(fileToLoad);
                _formatFileList.Add(formatFile);
            PSPropertyExpressionFactory expressionFactory = new PSPropertyExpressionFactory();
            List<XmlLoaderLoggerEntry> logEntries = null;
            // load the files
            LoadFromFile(filesToLoad, expressionFactory, true, authorizationManager, host, false, out logEntries);
            this.isShared = isShared;
            // check to see if there are any errors loading the format files
                throw new FormatTableLoadException(errors);
        internal TypeInfoDataBase GetTypeInfoDataBase()
            return Database;
                throw PSTraceSource.NewArgumentException(nameof(formatFile), FormatAndOutXmlLoadingStrings.FormatFileNotRooted, formatFile);
            lock (_formatFileList)
                if (shouldPrepend)
                    _formatFileList.Insert(0, formatFile);
                _formatFileList.Remove(formatFile);
        /// Update a shared formatting database with formatData of 'ExtendedTypeDefinition' type.
        /// This method should only be called from the FormatTable, where are shared formatting
        /// database is created.
        /// The format data to update the database
        /// Specify the order in which the format data will be loaded
        internal void AddFormatData(IEnumerable<ExtendedTypeDefinition> formatData, bool shouldPrepend)
            Diagnostics.Assert(isShared, "this method should only be called from FormatTable to update a shared database");
                foreach (ExtendedTypeDefinition typeDefinition in formatData)
                    PSSnapInTypeAndFormatErrors entryToLoad = new PSSnapInTypeAndFormatErrors(string.Empty, typeDefinition);
                    entryToLoad.Errors = errors;
                    filesToLoad.Add(entryToLoad);
                // check if the passed in formatData is empty
                if (filesToLoad.Count == 0)
                foreach (string formatFile in _formatFileList)
            if (!shouldPrepend)
                if (filesToLoad.Count == _formatFileList.Count)
            // load the formatting data
            LoadFromFile(filesToLoad, expressionFactory, false, null, null, false, out logEntries);
        /// Update the current formattable with the existing formatFileList.
        /// New files might have been added using Add() or Files might
        /// have been removed using Remove.
        internal void Update(AuthorizationManager authorizationManager, PSHost host)
            if (DisableFormatTableUpdates)
            if (isShared)
                throw PSTraceSource.NewInvalidOperationException(FormatAndOutXmlLoadingStrings.SharedFormatTableCannotBeUpdated);
            UpdateDataBase(filesToLoad, authorizationManager, host, false);
        /// Update the format data database. If there is any error in loading the format xml files,
        /// the old database is unchanged.
        /// The reference returned should NOT be modified by any means by the caller.
        /// <param name="mshsnapins">Files to be loaded and errors to be updated.</param>
        /// <param name="preValidated">
        /// True if the format data has been pre-validated (build time, manual testing, etc) so that validation can be
        /// skipped at runtime.
        /// <returns>Database instance.</returns>
        internal void UpdateDataBase(
            Collection<PSSnapInTypeAndFormatErrors> mshsnapins,
            PSHost host,
            bool preValidated
            LoadFromFile(mshsnapins, expressionFactory, false, authorizationManager, host, preValidated, out logEntries);
        /// Load the database
        /// NOTE: need to be protected by lock since not thread safe per se.
        /// <param name="files">*.formal.xml files to be loaded.</param>
        /// <param name="expressionFactory">Expression factory to validate script blocks.</param>
        /// <param name="acceptLoadingErrors">If true, load the database even if there are loading errors.</param>
        /// <param name="logEntries">Trace and error logs from loading the format Xml files.</param>
        /// <returns>True if we had a successful load.</returns>
        internal bool LoadFromFile(
            Collection<PSSnapInTypeAndFormatErrors> files,
            bool acceptLoadingErrors,
            bool preValidated,
            out List<XmlLoaderLoggerEntry> logEntries)
                TypeInfoDataBase newDataBase = null;
                lock (updateDatabaseLock)
                    newDataBase = LoadFromFileHelper(files, expressionFactory, authorizationManager, host, preValidated, out logEntries, out success);
                // if we have a valid database, assign it to the
                // current database
                lock (databaseLock)
                    if (acceptLoadingErrors || success)
                        Database = newDataBase;
                // if, for any reason, we failed the load, we initialize the
                // data base to an empty instance
                    if (Database == null)
                        TypeInfoDataBase tempDataBase = new TypeInfoDataBase();
                        AddPreLoadIntrinsics(tempDataBase);
                        AddPostLoadIntrinsics(tempDataBase);
                        Database = tempDataBase;
        /// It loads a database from file(s).
        /// <param name="logEntries">List of logger entries (errors, etc.) to return to the caller.</param>
        /// <param name="success">True if no error occurred.</param>
        /// <returns>A database instance loaded from file(s).</returns>
        private static TypeInfoDataBase LoadFromFileHelper(
            out List<XmlLoaderLoggerEntry> logEntries,
            out bool success)
            // Holds the aggregated log entries for all files...
            logEntries = new List<XmlLoaderLoggerEntry>();
            // fresh instance of the database
            TypeInfoDataBase db = new TypeInfoDataBase();
            // prepopulate the database with any necessary overriding data
            AddPreLoadIntrinsics(db);
            var etwEnabled = RunspaceEventSource.Log.IsEnabled();
            // load the XML document into a copy of the
            // in memory database
            foreach (PSSnapInTypeAndFormatErrors file in files)
                // Loads formatting data from ExtendedTypeDefinition instance
                if (file.FormatData != null)
                    LoadFormatDataHelper(file.FormatData, expressionFactory, logEntries, ref success, file, db, isBuiltInFormatData: false, isForHelp: false);
                if (etwEnabled)
                    RunspaceEventSource.Log.ProcessFormatFileStart(file.FullPath);
                if (!ProcessBuiltin(file, db, expressionFactory, logEntries, ref success))
                    // Loads formatting data from formatting data XML file
                    XmlFileLoadInfo info =
                        new XmlFileLoadInfo(Path.GetPathRoot(file.FullPath), file.FullPath, file.Errors, file.PSSnapinName);
                    using (TypeInfoDataBaseLoader loader = new TypeInfoDataBaseLoader())
                        if (!loader.LoadXmlFile(info, db, expressionFactory, authorizationManager, host, preValidated))
                        foreach (XmlLoaderLoggerEntry entry in loader.LogEntries)
                            // filter in only errors from the current file...
                                string mshsnapinMessage = StringUtil.Format(FormatAndOutXmlLoadingStrings.MshSnapinQualifiedError, info.psSnapinName, entry.message);
                                info.errors.Add(mshsnapinMessage);
                                if (entry.failToLoadFile)
                                    file.FailToLoadFile = true;
                        // now aggregate the entries...
                        logEntries.AddRange(loader.LogEntries);
                    RunspaceEventSource.Log.ProcessFormatFileStop(file.FullPath);
            // add any sensible defaults to the database
            AddPostLoadIntrinsics(db);
            return db;
        private static void LoadFormatDataHelper(
            ExtendedTypeDefinition formatData,
            PSPropertyExpressionFactory expressionFactory, List<XmlLoaderLoggerEntry> logEntries, ref bool success,
            PSSnapInTypeAndFormatErrors file, TypeInfoDataBase db,
            bool isBuiltInFormatData,
            bool isForHelp)
                if (!loader.LoadFormattingData(formatData, db, expressionFactory, isBuiltInFormatData, isForHelp))
                        string mshsnapinMessage = StringUtil.Format(FormatAndOutXmlLoadingStrings.MshSnapinQualifiedError,
                            file.PSSnapinName, entry.message);
                        file.Errors.Add(mshsnapinMessage);
        private delegate IEnumerable<ExtendedTypeDefinition> TypeGenerator();
        private static Dictionary<string, Tuple<bool, TypeGenerator>> s_builtinGenerators;
        private static Tuple<bool, TypeGenerator> GetBuiltin(bool isForHelp, TypeGenerator generator)
            return new Tuple<bool, TypeGenerator>(isForHelp, generator);
        private static bool ProcessBuiltin(
            PSSnapInTypeAndFormatErrors file,
            List<XmlLoaderLoggerEntry> logEntries,
            ref bool success)
            if (s_builtinGenerators == null)
                var builtInGenerators = new Dictionary<string, Tuple<bool, TypeGenerator>>(StringComparer.OrdinalIgnoreCase);
                var psHome = Utils.DefaultPowerShellAppBase;
                builtInGenerators.Add(Path.Combine(psHome, "Certificate.format.ps1xml"), GetBuiltin(false, Certificate_Format_Ps1Xml.GetFormatData));
                builtInGenerators.Add(Path.Combine(psHome, "Diagnostics.Format.ps1xml"), GetBuiltin(false, Diagnostics_Format_Ps1Xml.GetFormatData));
                builtInGenerators.Add(Path.Combine(psHome, "DotNetTypes.format.ps1xml"), GetBuiltin(false, DotNetTypes_Format_Ps1Xml.GetFormatData));
                builtInGenerators.Add(Path.Combine(psHome, "Event.Format.ps1xml"), GetBuiltin(false, Event_Format_Ps1Xml.GetFormatData));
                builtInGenerators.Add(Path.Combine(psHome, "FileSystem.format.ps1xml"), GetBuiltin(false, FileSystem_Format_Ps1Xml.GetFormatData));
                builtInGenerators.Add(Path.Combine(psHome, "Help.format.ps1xml"), GetBuiltin(true, Help_Format_Ps1Xml.GetFormatData));
                builtInGenerators.Add(Path.Combine(psHome, "HelpV3.format.ps1xml"), GetBuiltin(true, HelpV3_Format_Ps1Xml.GetFormatData));
                builtInGenerators.Add(Path.Combine(psHome, "PowerShellCore.format.ps1xml"), GetBuiltin(false, PowerShellCore_Format_Ps1Xml.GetFormatData));
                builtInGenerators.Add(Path.Combine(psHome, "PowerShellTrace.format.ps1xml"), GetBuiltin(false, PowerShellTrace_Format_Ps1Xml.GetFormatData));
                builtInGenerators.Add(Path.Combine(psHome, "Registry.format.ps1xml"), GetBuiltin(false, Registry_Format_Ps1Xml.GetFormatData));
                builtInGenerators.Add(Path.Combine(psHome, "WSMan.Format.ps1xml"), GetBuiltin(false, WSMan_Format_Ps1Xml.GetFormatData));
                Interlocked.CompareExchange(ref s_builtinGenerators, builtInGenerators, null);
            Tuple<bool, TypeGenerator> generator;
            if (!s_builtinGenerators.TryGetValue(file.FullPath, out generator))
            ProcessBuiltinFormatViewDefinitions(generator.Item2(), db, expressionFactory, file, logEntries, generator.Item1, ref success);
        private static void ProcessBuiltinFormatViewDefinitions(
            IEnumerable<ExtendedTypeDefinition> views,
            bool isForHelp,
            foreach (var v in views)
                LoadFormatDataHelper(v, expressionFactory, logEntries, ref success, file, db, isBuiltInFormatData: true, isForHelp: isForHelp);
        /// Helper to add any pre-load intrinsics to the db.
        /// <param name="db">Db being initialized.</param>
        private static void AddPreLoadIntrinsics(TypeInfoDataBase db)
            // NOTE: nothing to add for the time being. Add here if needed.
        /// Helper to add any post-load intrinsics to the db.
        private static void AddPostLoadIntrinsics(TypeInfoDataBase db)
            // add entry for the output of update-formatdata
            // we want to be able to display this as a list, unless overridden
            // by an entry loaded from file
            FormatShapeSelectionOnType sel = new FormatShapeSelectionOnType();
            sel.appliesTo = new AppliesTo();
            sel.appliesTo.AddAppliesToType("Microsoft.PowerShell.Commands.FormatDataLoadingInfo");
            sel.formatShape = FormatShape.List;
            db.defaultSettingsSection.shapeSelectionDirectives.formatShapeSelectionOnTypeList.Add(sel);
