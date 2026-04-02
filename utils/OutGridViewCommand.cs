    /// Enum for SelectionMode parameter.
    public enum OutputModeOption
        /// None is the default and it means OK and Cancel will not be present
        /// and no objects will be written to the pipeline.
        /// The selectionMode of the actual list will still be multiple.
        /// Allow selection of one single item to be written to the pipeline.
        Single,
        ///Allow select of multiple items to be written to the pipeline.
        Multiple
    /// Implementation for the Out-GridView command.
    [Cmdlet(VerbsData.Out, "GridView", DefaultParameterSetName = "PassThru", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2109378")]
    public class OutGridViewCommand : PSCmdlet, IDisposable
        private const string DataNotQualifiedForGridView = "DataNotQualifiedForGridView";
        private const string RemotingNotSupported = "RemotingNotSupported";
        private TypeInfoDataBase _typeInfoDataBase;
        private PSPropertyExpressionFactory _expressionFactory;
        private OutWindowProxy _windowProxy;
        private GridHeader _gridHeader;
        /// Initializes a new instance of the <see cref="OutGridViewCommand"/> class.
        public OutGridViewCommand()
        #region Input Parameters
        /// Gets/sets the title of the Out-GridView window.
        public string Title { get; set; }
        /// Get or sets a value indicating whether the cmdlet should wait for the window to be closed.
        [Parameter(ParameterSetName = "Wait")]
        /// Get or sets a value indicating whether the selected items should be written to the pipeline
        /// and if it should be possible to select multiple or single list items.
        [Parameter(ParameterSetName = "OutputMode")]
        public OutputModeOption OutputMode { get; set; }
        /// Gets or sets a value indicating whether the selected items should be written to the pipeline.
        /// Setting this to true is the same as setting the OutputMode to Multiple.
        [Parameter(ParameterSetName = "PassThru")]
            get { return OutputMode == OutputModeOption.Multiple ? new SwitchParameter(true) : new SwitchParameter(false); }
            set { this.OutputMode = value.IsPresent ? OutputModeOption.Multiple : OutputModeOption.None; }
        #endregion Input Parameters
        /// Provides a one-time, pre-processing functionality for the cmdlet.
            // Set up the ExpressionFactory
            _expressionFactory = new PSPropertyExpressionFactory();
            // If the value of the Title parameter is valid, use it as a window's title.
            if (this.Title != null)
                _windowProxy = new OutWindowProxy(this.Title, OutputMode, this);
                // Using the command line as a title.
                _windowProxy = new OutWindowProxy(this.MyInvocation.Line, OutputMode, this);
            // Load the Type info database.
            _typeInfoDataBase = this.Context.FormatDBManager.GetTypeInfoDataBase();
        /// Blocks depending on the wait and selected.
            if (_windowProxy == null)
            // If -Wait is used or outputMode is not None we have to wait for the window to be closed
            // The pipeline will be blocked while we don't return
            if (this.Wait || this.OutputMode != OutputModeOption.None)
                _windowProxy.BlockUntilClosed();
            // Output selected items to pipeline.
            List<PSObject> selectedItems = _windowProxy.GetSelectedItems();
            if (this.OutputMode != OutputModeOption.None && selectedItems != null)
                foreach (PSObject selectedItem in selectedItems)
                    if (selectedItem == null)
                    PSPropertyInfo originalObjectProperty = selectedItem.Properties[OutWindowProxy.OriginalObjectPropertyName];
                    if (originalObjectProperty == null)
                    this.WriteObject(originalObjectProperty.Value, false);
        /// Provides a record-by-record processing functionality for the cmdlet.
            if (InputObject.BaseObject is IDictionary dictionary)
                // Dictionaries should be enumerated through because the pipeline does not enumerate through them.
                foreach (DictionaryEntry entry in dictionary)
                    ProcessObject(PSObjectHelper.AsPSObject(entry));
                ProcessObject(InputObject);
        /// StopProcessing is called close the window when Ctrl+C in the command prompt.
                _windowProxy.CloseWindow();
        /// Converts the provided PSObject to a string preserving PowerShell formatting.
        /// <param name="liveObject">PSObject to be converted to a string.</param>
        internal string ConvertToString(PSObject liveObject)
            StringFormatError formatErrorObject = new();
            string smartToString = PSObjectHelper.SmartToString(liveObject,
                                                                _expressionFactory,
                                                                InnerFormatShapeCommand.FormatEnumerationLimit(),
                                                                formatErrorObject);
            if (formatErrorObject.exception != null)
                // There was a formatting error that should be sent to the console.
                this.WriteError(
                        formatErrorObject.exception,
                        "ErrorFormattingType",
                        liveObject)
            return smartToString;
        /// Execute formatting on a single object.
        /// <param name="input">Object to process.</param>
        private void ProcessObject(PSObject input)
            // Make sure the OGV window is not closed.
            if (_windowProxy.IsWindowClosed())
                LocalPipeline pipeline = (LocalPipeline)this.Context.CurrentRunspace.GetCurrentlyRunningPipeline();
                if (pipeline != null && !pipeline.IsStopping)
                    // Stop the pipeline cleanly.
                    pipeline.StopAsync();
            object baseObject = input.BaseObject;
            // Throw a terminating error for types that are not supported.
            if (baseObject is ScriptBlock ||
                baseObject is SwitchParameter ||
                baseObject is PSReference ||
                baseObject is FormatInfoData ||
                baseObject is PSObject)
                    new FormatException(StringUtil.Format(FormatAndOut_out_gridview.DataNotQualifiedForGridView)),
                    DataNotQualifiedForGridView,
            if (_gridHeader == null)
                // Columns have not been added yet; Start the main window and add columns.
                _windowProxy.ShowWindow();
                _gridHeader = GridHeader.ConstructGridHeader(input, this);
                _gridHeader.ProcessInputObject(input);
            // Some thread synchronization needed.
            Exception exception = _windowProxy.GetLastException();
                    "ManagementListInvocationException",
                    ErrorCategory.OperationStopped,
        internal abstract class GridHeader
            protected OutGridViewCommand parentCmd;
            internal GridHeader(OutGridViewCommand parentCmd)
                this.parentCmd = parentCmd;
            internal static GridHeader ConstructGridHeader(PSObject input, OutGridViewCommand parentCmd)
                if (DefaultScalarTypes.IsTypeInList(input.TypeNames) ||
                    !OutOfBandFormatViewManager.HasNonRemotingProperties(input))
                    return new ScalarTypeHeader(parentCmd, input);
                return new NonscalarTypeHeader(parentCmd, input);
            internal abstract void ProcessInputObject(PSObject input);
        internal sealed class ScalarTypeHeader : GridHeader
            private readonly Type _originalScalarType;
            internal ScalarTypeHeader(OutGridViewCommand parentCmd, PSObject input) : base(parentCmd)
                _originalScalarType = input.BaseObject.GetType();
                // On scalar types the type name is used as a column name.
                this.parentCmd._windowProxy.AddColumnsAndItem(input);
            internal override void ProcessInputObject(PSObject input)
                if (!_originalScalarType.Equals(input.BaseObject.GetType()))
                    parentCmd._gridHeader = new HeteroTypeHeader(base.parentCmd, input);
                    // Columns are already added; Add the input PSObject as an item to the underlying Management List.
                    base.parentCmd._windowProxy.AddItem(input);
        internal sealed class NonscalarTypeHeader : GridHeader
            private readonly AppliesTo _appliesTo = null;
            internal NonscalarTypeHeader(OutGridViewCommand parentCmd, PSObject input) : base(parentCmd)
                // Prepare a table view.
                TableView tableView = new();
                tableView.Initialize(parentCmd._expressionFactory, parentCmd._typeInfoDataBase);
                // Request a view definition from the type database.
                ViewDefinition viewDefinition = DisplayDataQuery.GetViewByShapeAndType(parentCmd._expressionFactory, parentCmd._typeInfoDataBase, FormatShape.Table, input.TypeNames, null);
                if (viewDefinition != null)
                    // Create a header using a view definition provided by the types database.
                    parentCmd._windowProxy.AddColumnsAndItem(input, tableView, (TableControlBody)viewDefinition.mainControl);
                    // Remember all type names and type groups the current view applies to.
                    _appliesTo = viewDefinition.appliesTo;
                    // Create a header using only the input object's properties.
                    parentCmd._windowProxy.AddColumnsAndItem(input, tableView);
                    _appliesTo = new AppliesTo();
                    // Add all type names except for Object and MarshalByRefObject types because they are too generic.
                    // Leave the Object type name if it is the only type name.
                    foreach (string typeName in input.TypeNames)
                        if (index > 0 && (typeName.Equals(typeof(object).FullName, StringComparison.OrdinalIgnoreCase) ||
                            typeName.Equals(typeof(MarshalByRefObject).FullName, StringComparison.OrdinalIgnoreCase)))
                        _appliesTo.AddAppliesToType(typeName);
                // Find out if the input has matching types in the this.appliesTo collection.
                foreach (TypeOrGroupReference typeOrGroupRef in _appliesTo.referenceList)
                    if (typeOrGroupRef is TypeReference)
                        // Add deserialization prefix.
                        string deserializedTypeName = typeOrGroupRef.name;
                        Deserializer.AddDeserializationPrefix(ref deserializedTypeName);
                        for (int i = 0; i < input.TypeNames.Count; i++)
                            if (typeOrGroupRef.name.Equals(input.TypeNames[i], StringComparison.OrdinalIgnoreCase)
                                || deserializedTypeName.Equals(input.TypeNames[i], StringComparison.OrdinalIgnoreCase))
                                // Current view supports the input's Type;
                                // Add the input PSObject as an item to the underlying Management List.
                        // Find out if the input's Type belongs to the current TypeGroup.
                        // TypeGroupReference has only a group's name, so use the database to get through all actual TypeGroup's.
                        List<TypeGroupDefinition> typeGroupList = base.parentCmd._typeInfoDataBase.typeGroupSection.typeGroupDefinitionList;
                        foreach (TypeGroupDefinition typeGroup in typeGroupList)
                            if (typeGroup.name.Equals(typeOrGroupRef.name, StringComparison.OrdinalIgnoreCase))
                                // A matching TypeGroup is found in the database.
                                // Find out if the input's Type belongs to this TypeGroup.
                                foreach (TypeReference typeRef in typeGroup.typeReferenceList)
                                    string deserializedTypeName = typeRef.name;
                                    if (input.TypeNames.Count > 0
                                        && (typeRef.name.Equals(input.TypeNames[0], StringComparison.OrdinalIgnoreCase)
                                            || deserializedTypeName.Equals(input.TypeNames[0], StringComparison.OrdinalIgnoreCase)))
                // The input's Type is not supported by the current view;
                // Switch to the Hetero Type view.
        internal sealed class HeteroTypeHeader : GridHeader
            internal HeteroTypeHeader(OutGridViewCommand parentCmd, PSObject input) : base(parentCmd)
                // Clear all existed columns and add Type and Value columns.
                this.parentCmd._windowProxy.AddHeteroViewColumnsAndItem(input);
                this.parentCmd._windowProxy.AddHeteroViewItem(input);
                if (_windowProxy != null)
                    _windowProxy.Dispose();
                    _windowProxy = null;
