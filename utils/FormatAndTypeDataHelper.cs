    internal class PSSnapInTypeAndFormatErrors
        public string psSnapinName;
        // only one of fullPath or formatTable or typeData or typeDefinition should be specified..
        // typeData and isRemove should be used together
        internal PSSnapInTypeAndFormatErrors(string psSnapinName, string fullPath)
            FullPath = fullPath;
            Errors = new ConcurrentBag<string>();
        internal PSSnapInTypeAndFormatErrors(string psSnapinName, FormatTable formatTable)
            FormatTable = formatTable;
        internal PSSnapInTypeAndFormatErrors(string psSnapinName, TypeData typeData, bool isRemove)
        internal PSSnapInTypeAndFormatErrors(string psSnapinName, ExtendedTypeDefinition typeDefinition)
        internal ExtendedTypeDefinition FormatData { get; }
        internal TypeData TypeData { get; }
        internal bool IsRemove { get; }
        internal string FullPath { get; }
        internal FormatTable FormatTable { get; }
        internal ConcurrentBag<string> Errors { get; set; }
        internal string PSSnapinName { get { return psSnapinName; } }
        internal bool FailToLoadFile;
    internal static class FormatAndTypeDataHelper
        private const string FileNotFound = "FileNotFound";
        private const string CannotFindRegistryKey = "CannotFindRegistryKey";
        private const string CannotFindRegistryKeyPath = "CannotFindRegistryKeyPath";
        private const string EntryShouldBeMshXml = "EntryShouldBeMshXml";
        private const string DuplicateFile = "DuplicateFile";
        internal const string ValidationException = "ValidationException";
        private static string GetBaseFolder(Collection<string> independentErrors)
            return Path.GetDirectoryName(Environment.ProcessPath);
        private static string GetAndCheckFullFileName(
            string psSnapinName,
            HashSet<string> fullFileNameSet,
            string baseFolder,
            string baseFileName,
            Collection<string> independentErrors,
            ref bool needToRemoveEntry,
            bool checkFileExists)
            string retValue = Path.IsPathRooted(baseFileName) ? baseFileName : Path.Combine(baseFolder, baseFileName);
            if (checkFileExists && !File.Exists(retValue))
                string error = StringUtil.Format(TypesXmlStrings.FileNotFound, psSnapinName, retValue);
                independentErrors.Add(error);
            if (fullFileNameSet.Contains(retValue))
                // Do not add Errors as we want loading of type/format files to be idempotent.
                // Just mark as Duplicate so the duplicate entry gets removed
                needToRemoveEntry = true;
            if (!retValue.EndsWith(".ps1xml", StringComparison.OrdinalIgnoreCase))
                string error = StringUtil.Format(TypesXmlStrings.EntryShouldBeMshXml, psSnapinName, retValue);
            fullFileNameSet.Add(retValue);
        internal static void ThrowExceptionOnError(
            Collection<PSSnapInTypeAndFormatErrors> PSSnapinFilesCollection,
            Category category)
            Collection<string> errors = new Collection<string>();
            if (independentErrors != null)
                foreach (string error in independentErrors)
                    errors.Add(error);
            foreach (PSSnapInTypeAndFormatErrors PSSnapinFiles in PSSnapinFilesCollection)
                foreach (string error in PSSnapinFiles.Errors)
            if (errors.Count == 0)
            StringBuilder allErrors = new StringBuilder();
            if (category == Category.Types)
                message =
                    StringUtil.Format(ExtendedTypeSystem.TypesXmlError, allErrors.ToString());
            else if (category == Category.Formats)
                message = StringUtil.Format(FormatAndOutXmlLoadingStrings.FormatLoadingErrors, allErrors.ToString());
            RuntimeException ex = new RuntimeException(message);
            if (errors.IsEmpty)
        internal enum Category
            Formats,
