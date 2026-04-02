    /// Cmdlet used to write a collection of formatting directives to an XML file.
    [Cmdlet(VerbsData.Export, "FormatData", DefaultParameterSetName = "ByPath", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096834")]
    public class ExportFormatDataCommand : PSCmdlet
        private ExtendedTypeDefinition[] _typeDefinition;
        /// Type definition to include in export.
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public ExtendedTypeDefinition[] InputObject
                return _typeDefinition;
                _typeDefinition = value;
        private string _filepath;
        /// Path of the XML file.
        [Parameter(ParameterSetName = "ByPath", Mandatory = true)]
        [Alias("FilePath")]
                return _filepath;
                _filepath = value;
        /// Literal path of the XML file.
        [Parameter(ParameterSetName = "ByLiteralPath", Mandatory = true)]
        private readonly List<ExtendedTypeDefinition> _typeDefinitions = new();
        /// Force writing a file.
        /// Do not overwrite file if exists.
        /// Include scriptblocks for export.
        public SwitchParameter IncludeScriptBlock
                return _includescriptblock;
                _includescriptblock = value;
        private bool _includescriptblock;
        /// Adds the type to the collection.
            foreach (ExtendedTypeDefinition typedef in _typeDefinition)
                _typeDefinitions.Add(typedef);
        /// Writes out the formatting directives from the
        /// collection to the specified XML file.
            FormatXmlWriter.WriteToPs1Xml(this, _typeDefinitions, _filepath, _force, _noclobber, _includescriptblock, _isLiteralPath);
