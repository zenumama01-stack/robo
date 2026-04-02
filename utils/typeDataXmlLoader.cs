    internal sealed class XmlFileLoadInfo
        internal XmlFileLoadInfo() { }
        internal XmlFileLoadInfo(string dir, string path, ConcurrentBag<string> errors, string psSnapinName)
            fileDirectory = dir;
            filePath = path;
            this.errors = errors;
            this.psSnapinName = psSnapinName;
        internal ConcurrentBag<string> errors;
        internal string psSnapinName;
    /// Class to load the XML document into data structures.
    /// It encapsulates the file format specific code.
    internal sealed partial class TypeInfoDataBaseLoader : XmlLoaderBase
        private const string resBaseName = "TypeInfoDataBaseLoaderStrings";
        [TraceSource("TypeInfoDataBaseLoader", "TypeInfoDataBaseLoader")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("TypeInfoDataBaseLoader", "TypeInfoDataBaseLoader");
        /// Table of XML node tags used in the file format.
        private static class XmlTags
            // top level entries in the XML document
            internal const string DefaultSettingsNode = "DefaultSettings";
            internal const string ConfigurationNode = "Configuration";
            internal const string SelectionSetsNode = "SelectionSets";
            internal const string ViewDefinitionsNode = "ViewDefinitions";
            internal const string ControlsNode = "Controls";
            // default settings entries
            internal const string MultilineTablesNode = "WrapTables";
            internal const string PropertyCountForTableNode = "PropertyCountForTable";
            internal const string ShowErrorsAsMessagesNode = "ShowError";
            internal const string ShowErrorsInFormattedOutputNode = "DisplayError";
            internal const string EnumerableExpansionsNode = "EnumerableExpansions";
            internal const string EnumerableExpansionNode = "EnumerableExpansion";
            internal const string ExpandNode = "Expand";
            // entries identifying the various control types definitions
            internal const string ControlNode = "Control";
            internal const string ComplexControlNameNode = "CustomControlName";
            // selection sets (a.k.a. Type Groups)
            internal const string SelectionSetNode = "SelectionSet";
            internal const string SelectionSetNameNode = "SelectionSetName";
            internal const string SelectionConditionNode = "SelectionCondition";
            internal const string NameNode = "Name";
            internal const string TypesNode = "Types";
            internal const string TypeNameNode = "TypeName";
            internal const string ViewNode = "View";
            // entries identifying the various control types
            internal const string TableControlNode = "TableControl";
            internal const string ListControlNode = "ListControl";
            internal const string WideControlNode = "WideControl";
            internal const string ComplexControlNode = "CustomControl";
            internal const string FieldControlNode = "FieldControl";
            // view specific tags
            internal const string ViewSelectedByNode = "ViewSelectedBy";
            internal const string GroupByNode = "GroupBy";
            internal const string OutOfBandNode = "OutOfBand";
            // table specific tags
            internal const string HideTableHeadersNode = "HideTableHeaders";
            internal const string TableHeadersNode = "TableHeaders";
            internal const string TableColumnHeaderNode = "TableColumnHeader";
            internal const string TableRowEntriesNode = "TableRowEntries";
            internal const string TableRowEntryNode = "TableRowEntry";
            internal const string MultiLineNode = "Wrap";
            internal const string TableColumnItemsNode = "TableColumnItems";
            internal const string TableColumnItemNode = "TableColumnItem";
            internal const string WidthNode = "Width";
            // list specific tags
            internal const string ListEntriesNode = "ListEntries";
            internal const string ListEntryNode = "ListEntry";
            internal const string ListItemsNode = "ListItems";
            internal const string ListItemNode = "ListItem";
            // wide specific tags
            internal const string ColumnNumberNode = "ColumnNumber";
            internal const string WideEntriesNode = "WideEntries";
            internal const string WideEntryNode = "WideEntry";
            internal const string WideItemNode = "WideItem";
            // complex specific tags
            internal const string ComplexEntriesNode = "CustomEntries";
            internal const string ComplexEntryNode = "CustomEntry";
            internal const string ComplexItemNode = "CustomItem";
            internal const string ExpressionBindingNode = "ExpressionBinding";
            internal const string NewLineNode = "NewLine";
            internal const string TextNode = "Text";
            internal const string FrameNode = "Frame";
            internal const string LeftIndentNode = "LeftIndent";
            internal const string RightIndentNode = "RightIndent";
            internal const string FirstLineIndentNode = "FirstLineIndent";
            internal const string FirstLineHangingNode = "FirstLineHanging";
            internal const string EnumerateCollectionNode = "EnumerateCollection";
            // general purpose tags
            internal const string AutoSizeNode = "AutoSize"; // valid only for table and wide
            internal const string AlignmentNode = "Alignment";
            internal const string PropertyNameNode = "PropertyName";
            internal const string ScriptBlockNode = "ScriptBlock";
            internal const string FormatStringNode = "FormatString";
            internal const string LabelNode = "Label";
            internal const string EntrySelectedByNode = "EntrySelectedBy";
            internal const string ItemSelectionConditionNode = "ItemSelectionCondition";
            // attribute tags for resource strings
            internal const string AssemblyNameAttribute = "AssemblyName";
            internal const string BaseNameAttribute = "BaseName";
            internal const string ResourceIdAttribute = "ResourceId";
        /// Table of miscellanea string constant values for XML nodes.
        private static class XMLStringValues
            internal const string True = "TRUE";
            internal const string False = "FALSE";
            internal const string AlignmentLeft = "left";
            internal const string AlignmentCenter = "center";
            internal const string AlignmentRight = "right";
        // Flag that determines whether validation should be suppressed while
        // processing pre-validated type / formatting information.
        private bool _suppressValidation = false;
        /// Entry point for the loader algorithm.
        /// <param name="info">Information needed to load the file.</param>
        /// <param name="db">Database instance to load the file into.</param>
        /// <returns>True if successful.</returns>
        internal bool LoadXmlFile(
            XmlFileLoadInfo info,
            bool preValidated)
                throw PSTraceSource.NewArgumentNullException(nameof(info));
            if (info.filePath == null)
                throw PSTraceSource.NewArgumentNullException("info.filePath");
            if (db == null)
                throw PSTraceSource.NewArgumentNullException(nameof(db));
            if (expressionFactory == null)
                throw PSTraceSource.NewArgumentNullException(nameof(expressionFactory));
            if (SecuritySupport.IsProductBinary(info.filePath))
                this.SetLoadingInfoIsProductCode(true);
            this.displayResourceManagerCache = db.displayResourceManagerCache;
            this.expressionFactory = expressionFactory;
            this.SetDatabaseLoadingInfo(info);
            this.ReportTrace("loading file started");
            XmlDocument newDocument = null;
            bool isFullyTrusted = false;
            newDocument = LoadXmlDocumentFromFileLoadingInfo(authorizationManager, host, out isFullyTrusted);
            // If we're not in a locked-down environment, types and formatting are allowed based just on the authorization
            // manager. If we are in a locked-down environment, additionally check the system policy.
            if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce)
                SetLoadingInfoIsFullyTrusted(isFullyTrusted);
            if (newDocument == null)
            bool previousSuppressValidation = _suppressValidation;
                _suppressValidation = preValidated;
                    this.LoadData(newDocument, db);
                catch (TooManyErrorsException)
                    // already logged an error before throwing
                    // Error in file {0}: {1}
                if (this.HasErrors)
                _suppressValidation = previousSuppressValidation;
            this.ReportTrace("file loaded with no errors");
        /// Entry point for the loader algorithm to load formatting data from ExtendedTypeDefinition.
        /// <param name="typeDefinition">The ExtendedTypeDefinition instance to load formatting data from.</param>
        /// <param name="db">Database instance to load the formatting data into.</param>
        /// <param name="expressionFactory">Expression factory to validate the script block.</param>
        /// <param name="isBuiltInFormatData">Do we implicitly trust the script blocks (so they should run in full language mode)?</param>
        /// <param name="isForHelp">True when the view is for help output.</param>
        internal bool LoadFormattingData(
            ExtendedTypeDefinition typeDefinition,
            if (typeDefinition == null)
                throw PSTraceSource.NewArgumentNullException(nameof(typeDefinition));
            if (typeDefinition.TypeName == null)
                throw PSTraceSource.NewArgumentNullException("typeDefinition.TypeName");
            this.ReportTrace("loading ExtendedTypeDefinition started");
                this.SetLoadingInfoIsFullyTrusted(isBuiltInFormatData);
                this.SetLoadingInfoIsProductCode(isBuiltInFormatData);
                this.LoadData(typeDefinition, db, isForHelp);
                // Error in formatting data "{0}": {1}
                this.ReportErrorForLoadingFromObjectModel(
                    StringUtil.Format(FormatAndOutXmlLoadingStrings.ErrorInFormattingData, typeDefinition.TypeName, e.Message), typeDefinition.TypeName);
            this.ReportTrace("ExtendedTypeDefinition loaded with no errors");
        /// Load the content of the XML document into the data instance.
        /// It assumes that the XML document has been successfully loaded.
        /// <param name="doc">XML document to load from, cannot be null.</param>
        /// <param name="db">Instance of the databaseto load into.</param>
        private void LoadData(XmlDocument doc, TypeInfoDataBase db)
            if (doc == null)
                throw PSTraceSource.NewArgumentNullException(nameof(doc));
            // create a new instance of the database to be loaded
            XmlElement documentElement = doc.DocumentElement;
            bool defaultSettingsNodeFound = false;
            bool typeGroupsFound = false;
            bool viewDefinitionsFound = false;
            bool controlDefinitionsFound = false;
            if (MatchNodeNameWithAttributes(documentElement, XmlTags.ConfigurationNode))
                // load the various sections
                using (this.StackFrame(documentElement))
                    foreach (XmlNode n in documentElement.ChildNodes)
                        if (MatchNodeName(n, XmlTags.DefaultSettingsNode))
                            if (defaultSettingsNodeFound)
                                ProcessDuplicateNode(n);
                            defaultSettingsNodeFound = true;
                            LoadDefaultSettings(db, n);
                        else if (MatchNodeName(n, XmlTags.SelectionSetsNode))
                            if (typeGroupsFound)
                            typeGroupsFound = true;
                            LoadTypeGroups(db, n);
                        else if (MatchNodeName(n, XmlTags.ViewDefinitionsNode))
                            if (viewDefinitionsFound)
                            viewDefinitionsFound = true;
                            LoadViewDefinitions(db, n);
                        else if (MatchNodeName(n, XmlTags.ControlsNode))
                            if (controlDefinitionsFound)
                            controlDefinitionsFound = true;
                            LoadControlDefinitions(n, db.formatControlDefinitionHolder.controlDefinitionList);
                            ProcessUnknownNode(n);
                ProcessUnknownNode(documentElement);
        #region load formatting data from FormatViewDefinition
        /// Load the content of the ExtendedTypeDefinition instance into the db.
        /// Only support following view controls:
        ///     TableControl
        ///     ListControl
        ///     WideControl
        ///     CustomControl.
        /// <param name="typeDefinition">ExtendedTypeDefinition instances to load from, cannot be null.</param>
        /// <param name="db">Instance of the database to load into.</param>
        /// <param name="isForHelpOutput">True if the formatter is used for formatting help objects.</param>
        private void LoadData(ExtendedTypeDefinition typeDefinition, TypeInfoDataBase db, bool isForHelpOutput)
                throw PSTraceSource.NewArgumentNullException("viewDefinition");
            int viewIndex = 0;
            foreach (FormatViewDefinition formatView in typeDefinition.FormatViewDefinition)
                ViewDefinition view = LoadViewFromObjectModel(typeDefinition.TypeNames, formatView, viewIndex++);
                    ReportTrace(string.Format(
                        "{0} view {1} is loaded from the 'FormatViewDefinition' at index {2} in 'ExtendedTypeDefinition' with type name {3}",
                        ControlBase.GetControlShapeName(view.mainControl),
                        view.name,
                        viewIndex - 1,
                        typeDefinition.TypeName));
                    // we are fine, add the view to the list
                    db.viewDefinitionsSection.viewDefinitionList.Add(view);
                    view.loadingInfo = this.LoadingInfo;
                    view.isHelpFormatter = isForHelpOutput;
        /// Load the view into a ViewDefinition.
        /// <param name="typeNames">The TypeName tag under SelectedBy tag.</param>
        /// <param name="formatView"></param>
        /// <param name="viewIndex"></param>
        private ViewDefinition LoadViewFromObjectModel(List<string> typeNames, FormatViewDefinition formatView, int viewIndex)
            // Get AppliesTo information
            AppliesTo appliesTo = new AppliesTo();
            foreach (var typename in typeNames)
                TypeReference tr = new TypeReference { name = typename };
                appliesTo.referenceList.Add(tr);
            // Set AppliesTo and Name in the view
            ViewDefinition view = new ViewDefinition();
            view.appliesTo = appliesTo;
            view.name = formatView.Name;
            var firstTypeName = typeNames[0];
            PSControl control = formatView.Control;
            if (control is TableControl)
                var tableControl = control as TableControl;
                view.mainControl = LoadTableControlFromObjectModel(tableControl, viewIndex, firstTypeName);
            else if (control is ListControl)
                var listControl = control as ListControl;
                view.mainControl = LoadListControlFromObjectModel(listControl, viewIndex, firstTypeName);
            else if (control is WideControl)
                var wideControl = control as WideControl;
                view.mainControl = LoadWideControlFromObjectModel(wideControl, viewIndex, firstTypeName);
                view.mainControl = LoadCustomControlFromObjectModel((CustomControl)control, viewIndex, firstTypeName);
            // Check if the PSControl is successfully loaded
            if (view.mainControl == null)
            view.outOfBand = control.OutOfBand;
            if (control.GroupBy != null)
                view.groupBy = new GroupBy
                    startGroup = new StartGroup
                        expression = LoadExpressionFromObjectModel(control.GroupBy.Expression, viewIndex, firstTypeName)
                if (control.GroupBy.Label != null)
                    view.groupBy.startGroup.labelTextToken = new TextToken { text = control.GroupBy.Label };
                if (control.GroupBy.CustomControl != null)
                    view.groupBy.startGroup.control = LoadCustomControlFromObjectModel(control.GroupBy.CustomControl, viewIndex, firstTypeName);
            return view;
        #region Load TableControl
        /// Load the TableControl to ControlBase.
        /// <param name="table"></param>
        private ControlBase LoadTableControlFromObjectModel(TableControl table, int viewIndex, string typeName)
            TableControlBody tableBody = new TableControlBody { autosize = table.AutoSize };
            LoadHeadersSectionFromObjectModel(tableBody, table.Headers);
            // No 'SelectedBy' data supplied, so the rowEntry will only be set to
            // tableBody.defaultDefinition. There cannot be more than one 'defaultDefinition'
            // defined for the tableBody.
            if (table.Rows.Count > 1)
                    StringUtil.Format(FormatAndOutXmlLoadingStrings.MultipleRowEntriesFoundInFormattingData, typeName, viewIndex, XmlTags.TableRowEntryNode), typeName);
            LoadRowEntriesSectionFromObjectModel(tableBody, table.Rows, viewIndex, typeName);
            // When error occurs while loading rowEntry, the tableBody.defaultDefinition would be null
            if (tableBody.defaultDefinition == null)
            // CHECK: verify consistency of headers and row entries
            if (tableBody.header.columnHeaderDefinitionList.Count != 0)
                // CHECK: if there are headers in the list, their number has to match
                // the default row definition item count
                if (tableBody.header.columnHeaderDefinitionList.Count !=
                    tableBody.defaultDefinition.rowItemDefinitionList.Count)
                    // Error at XPath {0} in file {1}: Header item count = {2} does not match default row item count = {3}.
                        StringUtil.Format(FormatAndOutXmlLoadingStrings.IncorrectHeaderItemCountInFormattingData, typeName, viewIndex,
                        tableBody.header.columnHeaderDefinitionList.Count,
                        tableBody.defaultDefinition.rowItemDefinitionList.Count), typeName);
                    return null; // fatal error
            // CHECK: if there are alternative row definitions. There should be no alternative row definitions here.
            Diagnostics.Assert(tableBody.optionalDefinitionList.Count == 0,
                "there should be no alternative row definitions because no SelectedBy is defined for TableControlRow");
            return tableBody;
        /// Load the headers defined for columns.
        /// <param name="tableBody"></param>
        /// <param name="headers"></param>
        private static void LoadHeadersSectionFromObjectModel(TableControlBody tableBody, List<TableControlColumnHeader> headers)
            foreach (TableControlColumnHeader header in headers)
                TableColumnHeaderDefinition chd = new TableColumnHeaderDefinition();
                // Contains:
                //   Label     --- Label     cardinality 0..1
                //   Width     --- Width     cardinality 0..1
                //   Alignment --- Alignment cardinality 0..1
                if (!string.IsNullOrEmpty(header.Label))
                    TextToken tt = new TextToken();
                    tt.text = header.Label;
                    chd.label = tt;
                chd.width = header.Width;
                chd.alignment = (int)header.Alignment;
                tableBody.header.columnHeaderDefinitionList.Add(chd);
        /// Load row enties, set the defaultDefinition of the TableControlBody.
        /// <param name="rowEntries"></param>
        private void LoadRowEntriesSectionFromObjectModel(TableControlBody tableBody, List<TableControlRow> rowEntries, int viewIndex, string typeName)
            foreach (TableControlRow row in rowEntries)
                TableRowDefinition trd = new TableRowDefinition { multiLine = row.Wrap };
                //   Columns --- TableColumnItems cardinality: 0..1
                // No SelectedBy is supplied in the TableControlRow
                if (row.Columns.Count > 0)
                    LoadColumnEntriesFromObjectModel(trd, row.Columns, viewIndex, typeName);
                    // trd.rowItemDefinitionList is null, it means there was a failure
                    if (trd.rowItemDefinitionList == null)
                        tableBody.defaultDefinition = null;
                    trd.appliesTo = LoadAppliesToSectionFromObjectModel(row.SelectedBy.TypeNames, row.SelectedBy.SelectionCondition);
                    tableBody.optionalDefinitionList.Add(trd);
                    tableBody.defaultDefinition = trd;
            // rowEntries must not be empty
                    StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntryInFormattingData, typeName, viewIndex, XmlTags.TableRowEntryNode), typeName);
        /// Load the column items into the TableRowDefinition.
        /// <param name="trd"></param>
        /// <param name="columns"></param>
        private void LoadColumnEntriesFromObjectModel(TableRowDefinition trd, List<TableControlColumn> columns, int viewIndex, string typeName)
                TableRowItemDefinition rid = new TableRowItemDefinition();
                // Contain:
                //   DisplayEntry --- Expression cardinality: 0..1
                //   Alignment    --- Alignment  cardinality: 0..1
                if (column.DisplayEntry != null)
                    ExpressionToken expression = LoadExpressionFromObjectModel(column.DisplayEntry, viewIndex, typeName);
                    if (expression == null)
                        trd.rowItemDefinitionList = null;
                    fpt.expression = expression;
                    fpt.fieldFormattingDirective.formatString = column.FormatString;
                    rid.formatTokenList.Add(fpt);
                rid.alignment = (int)column.Alignment;
                trd.rowItemDefinitionList.Add(rid);
        #endregion Load TableControl
        /// Load the expression information from DisplayEntry.
        /// <param name="displayEntry"></param>
        private ExpressionToken LoadExpressionFromObjectModel(DisplayEntry displayEntry, int viewIndex, string typeName)
            ExpressionToken token = new ExpressionToken();
                token.expressionValue = displayEntry.Value;
                return token;
                token.isScriptBlock = true;
                    // For faster startup, we don't validate any of the built-in formatting script blocks, where isFullyTrusted == built-in.
                    if (!LoadingInfo.isFullyTrusted)
                        this.expressionFactory.VerifyScriptBlockText(token.expressionValue);
                catch (ParseException e)
                    // Error at
                        StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidScriptBlockInFormattingData, typeName, viewIndex, e.Message), typeName);
                    Diagnostics.Assert(false, "TypeInfoBaseLoader.VerifyScriptBlock unexpected exception " + e.GetType().FullName);
            // this should never happen if the API is used correctly
            PSTraceSource.NewInvalidOperationException();
        /// Load EntrySelectedBy (TypeName) into AppliesTo.
        private static AppliesTo LoadAppliesToSectionFromObjectModel(List<string> selectedBy, List<DisplayEntry> condition)
            if (selectedBy != null)
                foreach (string type in selectedBy)
                    if (string.IsNullOrEmpty(type))
                    TypeReference tr = new TypeReference { name = type };
            if (condition != null)
                foreach (var cond in condition)
                    // TODO
            return appliesTo;
        #region Load ListControl
        /// Load LoisControl into the ListControlBody.
        /// <param name="list"></param>
        private ListControlBody LoadListControlFromObjectModel(ListControl list, int viewIndex, string typeName)
            ListControlBody listBody = new ListControlBody();
            // load the list entries section
            LoadListControlEntriesFromObjectModel(listBody, list.Entries, viewIndex, typeName);
            if (listBody.defaultEntryDefinition == null)
            return listBody;
        private void LoadListControlEntriesFromObjectModel(ListControlBody listBody, List<ListControlEntry> entries, int viewIndex, string typeName)
            //   Entries --- ListEntries cardinality 1
            foreach (ListControlEntry listEntry in entries)
                ListControlEntryDefinition lved = LoadListControlEntryDefinitionFromObjectModel(listEntry, viewIndex, typeName);
                if (lved == null)
                        StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailedInFormattingData, typeName, viewIndex, XmlTags.ListEntryNode), typeName);
                    listBody.defaultEntryDefinition = null;
                // determine if we have a default entry and if it's already set
                if (lved.appliesTo == null)
                        listBody.defaultEntryDefinition = lved;
                        // Error at XPath {0} in file {1}: There cannot be more than one default {2}.
                            StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntryInFormattingData, typeName, viewIndex, XmlTags.ListEntryNode), typeName);
                        return; // fatal error
                    listBody.optionalEntryList.Add(lved);
            // list entries is empty
                // Error: there must be at least one default
                    StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntryInFormattingData, typeName, viewIndex, XmlTags.ListEntryNode), typeName);
        /// Load ListEntry into ListControlEntryDefinition.
        /// <param name="listEntry"></param>
        private ListControlEntryDefinition LoadListControlEntryDefinitionFromObjectModel(ListControlEntry listEntry, int viewIndex, string typeName)
            ListControlEntryDefinition lved = new ListControlEntryDefinition();
            //   SelectedBy ---  EntrySelectedBy(TypeName)  cardinality 0..1
            //   Items      ---  ListItems                  cardinality 1
            if (listEntry.EntrySelectedBy != null)
                lved.appliesTo = LoadAppliesToSectionFromObjectModel(listEntry.EntrySelectedBy.TypeNames, listEntry.EntrySelectedBy.SelectionCondition);
            LoadListControlItemDefinitionsFromObjectModel(lved, listEntry.Items, viewIndex, typeName);
            if (lved.itemDefinitionList == null)
            return lved;
        /// Load ListItems into ListControlItemDefinition.
        /// <param name="lved"></param>
        /// <param name="listItems"></param>
        private void LoadListControlItemDefinitionsFromObjectModel(ListControlEntryDefinition lved, List<ListControlEntryItem> listItems, int viewIndex, string typeName)
            foreach (ListControlEntryItem listItem in listItems)
                ListControlItemDefinition lvid = new ListControlItemDefinition();
                //   DisplayEntry --- Expression  cardinality 0..1
                //   Label        --- Label       cardinality 0..1
                if (listItem.DisplayEntry != null)
                    ExpressionToken expression = LoadExpressionFromObjectModel(listItem.DisplayEntry, viewIndex, typeName);
                        lved.itemDefinitionList = null;
                        return; // fatal
                    fpt.fieldFormattingDirective.formatString = listItem.FormatString;
                    lvid.formatTokenList.Add(fpt);
                if (!string.IsNullOrEmpty(listItem.Label))
                    tt.text = listItem.Label;
                    lvid.label = tt;
                lved.itemDefinitionList.Add(lvid);
            // we must have at least a definition in th elist
            if (lved.itemDefinitionList.Count == 0)
                // Error: At least one list view item must be specified.
                    StringUtil.Format(FormatAndOutXmlLoadingStrings.NoListViewItemInFormattingData, typeName, viewIndex), typeName);
        #endregion Load ListControl
        #region Load WideControl
        /// Load the WideControl into the WideControlBody.
        /// <param name="wide"></param>
        private WideControlBody LoadWideControlFromObjectModel(WideControl wide, int viewIndex, string typeName)
            WideControlBody wideBody = new WideControlBody();
            //   Columns --- ColumnNumbers  cardinality 0..1
            //   Entries --- WideEntries    cardinality 1
            wideBody.columns = (int)wide.Columns;
            if (wide.AutoSize)
                wideBody.autosize = true;
            LoadWideControlEntriesFromObjectModel(wideBody, wide.Entries, viewIndex, typeName);
            if (wideBody.defaultEntryDefinition == null)
                // if we have no default entry definition, it means there was a failure
            return wideBody;
        /// Load WideEntries.
        /// <param name="wideBody"></param>
        /// <param name="wideEntries"></param>
        private void LoadWideControlEntriesFromObjectModel(WideControlBody wideBody, List<WideControlEntryItem> wideEntries, int viewIndex, string typeName)
            foreach (WideControlEntryItem wideItem in wideEntries)
                WideControlEntryDefinition wved = LoadWideControlEntryFromObjectModel(wideItem, viewIndex, typeName);
                if (wved == null)
                        StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidFormattingData, typeName, viewIndex, XmlTags.WideEntryNode), typeName);
                    wideBody.defaultEntryDefinition = null;
                if (wved.appliesTo == null)
                        wideBody.defaultEntryDefinition = wved;
                            StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntryInFormattingData, typeName, viewIndex, XmlTags.WideEntryNode), typeName);
                    wideBody.optionalEntryList.Add(wved);
                    StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntryInFormattingData, typeName, viewIndex, XmlTags.WideEntryNode), typeName);
        /// Load WideEntry into WieControlEntryDefinition.
        /// <param name="wideItem"></param>
        private WideControlEntryDefinition LoadWideControlEntryFromObjectModel(WideControlEntryItem wideItem, int viewIndex, string typeName)
            WideControlEntryDefinition wved = new WideControlEntryDefinition();
            //   SelectedBy   --- EntrySelectedBy (TypeName)  cardinality 0..1
            //   DisplayEntry --- WideItem (Expression)       cardinality 1
            // process selectedBy property
            if (wideItem.EntrySelectedBy != null)
                wved.appliesTo = LoadAppliesToSectionFromObjectModel(wideItem.EntrySelectedBy.TypeNames, wideItem.EntrySelectedBy.SelectionCondition);
            // process displayEntry property
            ExpressionToken expression = LoadExpressionFromObjectModel(wideItem.DisplayEntry, viewIndex, typeName);
                return null; // fatal
            fpt.fieldFormattingDirective.formatString = wideItem.FormatString;
            wved.formatTokenList.Add(fpt);
            return wved;
        #endregion Load WideControl
        #region Load CustomControl
        private ComplexControlBody LoadCustomControlFromObjectModel(CustomControl custom, int viewIndex, string typeName)
            if (custom._cachedBody != null)
                return custom._cachedBody;
            var ccb = new ComplexControlBody();
            foreach (var entry in custom.Entries)
                var cced = LoadComplexControlEntryDefinitionFromObjectModel(entry, viewIndex, typeName);
                if (cced.appliesTo == null)
                    ccb.defaultEntry = cced;
                    ccb.optionalEntryList.Add(cced);
            Interlocked.CompareExchange(ref custom._cachedBody, ccb, null);
            return ccb;
        private ComplexControlEntryDefinition LoadComplexControlEntryDefinitionFromObjectModel(CustomControlEntry entry, int viewIndex, string typeName)
            var cced = new ComplexControlEntryDefinition();
            if (entry.SelectedBy != null)
                cced.appliesTo = LoadAppliesToSectionFromObjectModel(entry.SelectedBy.TypeNames, entry.SelectedBy.SelectionCondition);
                cced.itemDefinition.formatTokenList.Add(LoadFormatTokenFromObjectModel(item, viewIndex, typeName));
            return cced;
        private FormatToken LoadFormatTokenFromObjectModel(CustomItemBase item, int viewIndex, string typeName)
                return new NewLineToken { count = newline.Count };
                return new TextToken { text = text.Text };
                var cpt = new CompoundPropertyToken { enumerateCollection = expr.EnumerateCollection };
                    cpt.conditionToken = LoadExpressionFromObjectModel(expr.ItemSelectionCondition, viewIndex, typeName);
                    cpt.expression = LoadExpressionFromObjectModel(expr.Expression, viewIndex, typeName);
                    cpt.control = LoadCustomControlFromObjectModel(expr.CustomControl, viewIndex, typeName);
                return cpt;
            var frameToken = new FrameToken
                frameInfoDefinition =
                    leftIndentation = (int)frame.LeftIndent,
                    rightIndentation = (int)frame.RightIndent,
                    firstLine = frame.FirstLineHanging != 0 ? -(int)frame.FirstLineHanging : (int)frame.FirstLineIndent
            foreach (var i in frame.CustomItems)
                frameToken.itemDefinition.formatTokenList.Add(LoadFormatTokenFromObjectModel(i, viewIndex, typeName));
            return frameToken;
        #endregion Load Custom Control
        #endregion load formatting data from FormatViewDefinition
        #region Default Settings Loading
        private void LoadDefaultSettings(TypeInfoDataBase db, XmlNode defaultSettingsNode)
            // all these nodes are of [0..1] cardinality
            bool propertyCountForTableFound = false;
            bool showErrorsAsMessagesFound = false;
            bool showErrorsInFormattedOutputFound = false;
            bool enumerableExpansionsFound = false;
            bool multilineTablesFound = false;
            bool tempVal;
            using (this.StackFrame(defaultSettingsNode))
                foreach (XmlNode n in defaultSettingsNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.ShowErrorsAsMessagesNode))
                        if (showErrorsAsMessagesFound)
                        showErrorsAsMessagesFound = true;
                        if (ReadBooleanNode(n, out tempVal))
                            db.defaultSettingsSection.formatErrorPolicy.ShowErrorsAsMessages = tempVal;
                    else if (MatchNodeName(n, XmlTags.ShowErrorsInFormattedOutputNode))
                        if (showErrorsInFormattedOutputFound)
                        showErrorsInFormattedOutputFound = true;
                            db.defaultSettingsSection.formatErrorPolicy.ShowErrorsInFormattedOutput = tempVal;
                    else if (MatchNodeName(n, XmlTags.PropertyCountForTableNode))
                        if (propertyCountForTableFound)
                        propertyCountForTableFound = true;
                        int val;
                        if (ReadPositiveIntegerValue(n, out val))
                            db.defaultSettingsSection.shapeSelectionDirectives.PropertyCountForTable = val;
                            // Error at XPath {0} in file {1}: Invalid {2} value.
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNodeValue, ComputeCurrentXPath(), FilePath, XmlTags.PropertyCountForTableNode));
                    else if (MatchNodeName(n, XmlTags.MultilineTablesNode))
                        if (multilineTablesFound)
                        multilineTablesFound = true;
                            db.defaultSettingsSection.MultilineTables = tempVal;
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNodeValue, ComputeCurrentXPath(), FilePath, XmlTags.MultilineTablesNode));
                    else if (MatchNodeName(n, XmlTags.EnumerableExpansionsNode))
                        if (enumerableExpansionsFound)
                        enumerableExpansionsFound = true;
                        db.defaultSettingsSection.enumerableExpansionDirectiveList =
                            LoadEnumerableExpansionDirectiveList(n);
                        this.ProcessUnknownNode(n);
        private List<EnumerableExpansionDirective> LoadEnumerableExpansionDirectiveList(XmlNode expansionListNode)
            List<EnumerableExpansionDirective> retVal =
                                new List<EnumerableExpansionDirective>();
            using (this.StackFrame(expansionListNode))
                foreach (XmlNode n in expansionListNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.EnumerableExpansionNode))
                        EnumerableExpansionDirective eed = LoadEnumerableExpansionDirective(n, k++);
                        if (eed == null)
                            // Error at XPath {0} in file {1}: {2} failed to load.
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, ComputeCurrentXPath(), FilePath, XmlTags.EnumerableExpansionNode));
                        retVal.Add(eed);
        private EnumerableExpansionDirective LoadEnumerableExpansionDirective(XmlNode directive, int index)
            using (this.StackFrame(directive, index))
                EnumerableExpansionDirective eed = new EnumerableExpansionDirective();
                bool appliesToNodeFound = false;    // cardinality 1
                bool expandNodeFound = false;    // cardinality 1
                foreach (XmlNode n in directive.ChildNodes)
                    if (MatchNodeName(n, XmlTags.EntrySelectedByNode))
                        if (appliesToNodeFound)
                            this.ProcessDuplicateNode(n);
                        appliesToNodeFound = true;
                        eed.appliesTo = LoadAppliesToSection(n, true);
                    else if (MatchNodeName(n, XmlTags.ExpandNode))
                        if (expandNodeFound)
                        expandNodeFound = true;
                        string s = GetMandatoryInnerText(n);
                        bool success = EnumerableExpansionConversion.Convert(s, out eed.enumerableExpansion);
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNodeValue, ComputeCurrentXPath(), FilePath, XmlTags.ExpandNode));
                return eed;
        #region Type Groups Loading
        private void LoadTypeGroups(TypeInfoDataBase db, XmlNode typeGroupsNode)
            using (this.StackFrame(typeGroupsNode))
                int typeGroupCount = 0;
                foreach (XmlNode n in typeGroupsNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.SelectionSetNode))
                        LoadTypeGroup(db, n, typeGroupCount++);
        private void LoadTypeGroup(TypeInfoDataBase db, XmlNode typeGroupNode, int index)
            using (this.StackFrame(typeGroupNode, index))
                // create data structure
                TypeGroupDefinition typeGroupDefinition = new TypeGroupDefinition();
                bool nameNodeFound = false;
                foreach (XmlNode n in typeGroupNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.NameNode))
                        if (nameNodeFound)
                        nameNodeFound = true;
                        typeGroupDefinition.name = GetMandatoryInnerText(n);
                    else if (MatchNodeName(n, XmlTags.TypesNode))
                        LoadTypeGroupTypeRefs(n, typeGroupDefinition);
                if (!nameNodeFound)
                    this.ReportMissingNode(XmlTags.NameNode);
                // finally add to the list
                db.typeGroupSection.typeGroupDefinitionList.Add(typeGroupDefinition);
        private void LoadTypeGroupTypeRefs(XmlNode typesNode, TypeGroupDefinition typeGroupDefinition)
            using (this.StackFrame(typesNode))
                int typeRefCount = 0;
                foreach (XmlNode n in typesNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.TypeNameNode))
                        using (this.StackFrame(n, typeRefCount++))
                            tr.name = GetMandatoryInnerText(n);
                            typeGroupDefinition.typeReferenceList.Add(tr);
        #region AppliesTo Loading
        private AppliesTo LoadAppliesToSection(XmlNode appliesToNode, bool allowSelectionCondition)
            using (this.StackFrame(appliesToNode))
                // expect: type ref, group ref, or nothing
                foreach (XmlNode n in appliesToNode.ChildNodes)
                    using (this.StackFrame(n))
                        if (MatchNodeName(n, XmlTags.SelectionSetNameNode))
                            TypeGroupReference tgr = LoadTypeGroupReference(n);
                            if (tgr != null)
                                appliesTo.referenceList.Add(tgr);
                        else if (MatchNodeName(n, XmlTags.TypeNameNode))
                            TypeReference tr = LoadTypeReference(n);
                            if (tr != null)
                        else if (allowSelectionCondition && MatchNodeName(n, XmlTags.SelectionConditionNode))
                            TypeOrGroupReference tgr = LoadSelectionConditionNode(n);
                if (appliesTo.referenceList.Count == 0)
                    // we do not accept an empty list
                    // Error at XPath {0} in file {1}: No type or condition is specified for applying the view.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.EmptyAppliesTo, ComputeCurrentXPath(), FilePath));
        private TypeReference LoadTypeReference(XmlNode n)
            string val = GetMandatoryInnerText(n);
                tr.name = val;
                return tr;
        private TypeGroupReference LoadTypeGroupReference(XmlNode n)
                TypeGroupReference tgr = new TypeGroupReference();
                tgr.name = val;
                return tgr;
        private TypeOrGroupReference LoadSelectionConditionNode(XmlNode selectionConditionNode)
            using (this.StackFrame(selectionConditionNode))
                TypeOrGroupReference retVal = null;
                bool expressionNodeFound = false;       // cardinality 1
                // these two nodes are mutually exclusive
                bool typeFound = false;              // cardinality 0..1
                bool typeGroupFound = false;              // cardinality 0..1
                ExpressionNodeMatch expressionMatch = new ExpressionNodeMatch(this);
                foreach (XmlNode n in selectionConditionNode.ChildNodes)
                        if (typeGroupFound)
                            this.ProcessDuplicateAlternateNode(n, XmlTags.SelectionSetNameNode, XmlTags.TypeNameNode);
                        typeGroupFound = true;
                            retVal = tgr;
                        if (typeFound)
                        typeFound = true;
                            retVal = tr;
                    else if (expressionMatch.MatchNode(n))
                        if (expressionNodeFound)
                        expressionNodeFound = true;
                        if (!expressionMatch.ProcessNode(n))
                if (typeFound && typeGroupFound)
                    // Error at XPath {0} in file {1}: Cannot have SelectionSetName and TypeName at the same time.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.SelectionSetNameAndTypeName, ComputeCurrentXPath(), FilePath));
                if (retVal == null)
                    // missing mandatory node
                    this.ReportMissingNodes(new string[] { XmlTags.SelectionSetNameNode, XmlTags.TypeNameNode });
                    // mandatory node
                    retVal.conditionToken = expressionMatch.GenerateExpressionToken();
                    if (retVal.conditionToken == null)
                // failure: expression is mandatory
                // Error at XPath {0} in file {1}: An expression is expected.
                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectExpression, ComputeCurrentXPath(), FilePath));
        #region GroupBy Loading
        private GroupBy LoadGroupBySection(XmlNode groupByNode)
            using (this.StackFrame(groupByNode))
                ComplexControlMatch controlMatch = new ComplexControlMatch(this);
                bool expressionNodeFound = false;       // cardinality 0..1
                bool controlFound = false;              // cardinality 0..1
                bool labelFound = false;              // cardinality 0..1
                GroupBy groupBy = new GroupBy();
                foreach (XmlNode n in groupByNode)
                    if (expressionMatch.MatchNode(n))
                    else if (controlMatch.MatchNode(n))
                        if (controlFound)
                            this.ProcessDuplicateAlternateNode(n, XmlTags.ComplexControlNode, XmlTags.ComplexControlNameNode);
                        controlFound = true;
                        if (!controlMatch.ProcessNode(n))
                    else if (MatchNodeNameWithAttributes(n, XmlTags.LabelNode))
                        if (labelFound)
                        labelFound = true;
                        labelTextToken = LoadLabel(n);
                        if (labelTextToken == null)
                if (controlFound && labelFound)
                    // Error at XPath {0} in file {1}: Cannot have control and label at the same time.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ControlAndLabel, ComputeCurrentXPath(), FilePath));
                if (controlFound || labelFound)
                    if (!expressionNodeFound)
                        // Error at XPath {0} in file {1}: Cannot have control or label without an expression.
                        this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ControlLabelWithoutExpression, ComputeCurrentXPath(), FilePath));
                        groupBy.startGroup.control = controlMatch.Control;
                    else if (labelFound)
                        groupBy.startGroup.labelTextToken = labelTextToken;
                    // we add only if we encountered one, since it's not mandatory
                    ExpressionToken expression = expressionMatch.GenerateExpressionToken();
                    groupBy.startGroup.expression = expression;
                    return groupBy;
        private TextToken LoadLabel(XmlNode textNode)
            using (this.StackFrame(textNode))
                return LoadTextToken(textNode);
        private TextToken LoadTextToken(XmlNode n)
            if (!LoadStringResourceReference(n, out tt.resource))
                // inner text is optional
                tt.text = n.InnerText;
                return tt;
            // inner text is mandatory
            tt.text = this.GetMandatoryInnerText(n);
            if (tt.text == null)
        private bool LoadStringResourceReference(XmlNode n, out StringResourceReference resource)
            resource = null;
            if (n is not XmlElement e)
                // Error at XPath {0} in file {1}: Node should be an XmlElement.
                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NonXmlElementNode, ComputeCurrentXPath(), FilePath));
            if (e.Attributes.Count <= 0)
                // no resources to load
            // need to find mandatory attributes
            resource = LoadResourceAttributes(e.Attributes);
            // we committed to having resources, if not obtained, it's an error
            return resource != null;
        private StringResourceReference LoadResourceAttributes(XmlAttributeCollection attributes)
            StringResourceReference resource = new StringResourceReference();
            foreach (XmlAttribute a in attributes)
                if (MatchAttributeName(a, XmlTags.AssemblyNameAttribute))
                    resource.assemblyName = GetMandatoryAttributeValue(a);
                    if (resource.assemblyName == null)
                else if (MatchAttributeName(a, XmlTags.BaseNameAttribute))
                    resource.baseName = GetMandatoryAttributeValue(a);
                    if (resource.baseName == null)
                else if (MatchAttributeName(a, XmlTags.ResourceIdAttribute))
                    resource.resourceId = GetMandatoryAttributeValue(a);
                    if (resource.resourceId == null)
                    ProcessUnknownAttribute(a);
            // make sure we got all the attributes, since allof them are mandatory
                ReportMissingAttribute(XmlTags.AssemblyNameAttribute);
                ReportMissingAttribute(XmlTags.BaseNameAttribute);
                ReportMissingAttribute(XmlTags.ResourceIdAttribute);
            // success in loading
            resource.loadingInfo = this.LoadingInfo;
            // optional pre-load and binding verification
            if (this.VerifyStringResources)
                DisplayResourceManagerCache.LoadingResult result;
                DisplayResourceManagerCache.AssemblyBindingStatus bindingStatus;
                this.displayResourceManagerCache.VerifyResource(resource, out result, out bindingStatus);
                if (result != DisplayResourceManagerCache.LoadingResult.NoError)
                    ReportStringResourceFailure(resource, result, bindingStatus);
            return resource;
        private void ReportStringResourceFailure(StringResourceReference resource,
                                                    DisplayResourceManagerCache.LoadingResult result,
                                                    DisplayResourceManagerCache.AssemblyBindingStatus bindingStatus)
            string assemblyDisplayName;
            switch (bindingStatus)
                case DisplayResourceManagerCache.AssemblyBindingStatus.FoundInPath:
                        assemblyDisplayName = resource.assemblyLocation;
                case DisplayResourceManagerCache.AssemblyBindingStatus.FoundInGac:
                        // "(Global Assembly Cache) {0}"
                        assemblyDisplayName = StringUtil.Format(FormatAndOutXmlLoadingStrings.AssemblyInGAC, resource.assemblyName);
                        assemblyDisplayName = resource.assemblyName;
                case DisplayResourceManagerCache.LoadingResult.AssemblyNotFound:
                        // Error at XPath {0} in file {1}: Assembly {2} is not found.
                        msg = StringUtil.Format(FormatAndOutXmlLoadingStrings.AssemblyNotFound, ComputeCurrentXPath(), FilePath, assemblyDisplayName);
                case DisplayResourceManagerCache.LoadingResult.ResourceNotFound:
                        // Error at XPath {0} in file {1}: Resource {2} in assembly {3} is not found.
                        msg = StringUtil.Format(FormatAndOutXmlLoadingStrings.ResourceNotFound, ComputeCurrentXPath(), FilePath, resource.baseName, assemblyDisplayName);
                case DisplayResourceManagerCache.LoadingResult.StringNotFound:
                        // Error at XPath {0} in file {1}: String {2} from resource {3} in assembly {4} is not found.
                        msg = StringUtil.Format(FormatAndOutXmlLoadingStrings.StringResourceNotFound, ComputeCurrentXPath(), FilePath,
                            resource.resourceId, resource.baseName, assemblyDisplayName);
            this.ReportError(msg);
        #region Expression Loading
        /// Helper to verify the text of a string block and
        /// log an error if an exception is thrown.
        /// <param name="scriptBlockText">Script block string to verify.</param>
        /// <returns>True if parsed correctly, false if failed.</returns>
        internal bool VerifyScriptBlock(string scriptBlockText)
                this.expressionFactory.VerifyScriptBlockText(scriptBlockText);
                // Error at XPath {0} in file {1}: Invalid script block "{2}".
                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidScriptBlock, ComputeCurrentXPath(), FilePath, e.Message));
        /// Helper class to wrap the loading of a script block/property name alternative tag.
        private sealed class ExpressionNodeMatch
            internal ExpressionNodeMatch(TypeInfoDataBaseLoader loader)
            internal bool MatchNode(XmlNode n)
                return _loader.MatchNodeName(n, XmlTags.PropertyNameNode) || _loader.MatchNodeName(n, XmlTags.ScriptBlockNode);
            internal bool ProcessNode(XmlNode n)
                if (_loader.MatchNodeName(n, XmlTags.PropertyNameNode))
                    if (_token != null)
                        if (_token.isScriptBlock)
                            _loader.ProcessDuplicateAlternateNode(n, XmlTags.PropertyNameNode, XmlTags.ScriptBlockNode);
                            _loader.ProcessDuplicateNode(n);
                        return false; // fatal error
                    _token = new ExpressionToken();
                    _token.expressionValue = _loader.GetMandatoryInnerText(n);
                    if (_token.expressionValue == null)
                        // Error at XPath {0} in file {1}: Missing property.
                        _loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoProperty, _loader.ComputeCurrentXPath(), _loader.FilePath));
                        _fatalError = true;
                else if (_loader.MatchNodeName(n, XmlTags.ScriptBlockNode))
                        if (!_token.isScriptBlock)
                    _token.isScriptBlock = true;
                        // Error at XPath {0} in file {1}: Missing script block text.
                        _loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoScriptBlockText, _loader.ComputeCurrentXPath(), _loader.FilePath));
                    if ((!_loader._suppressValidation) && (!_loader.VerifyScriptBlock(_token.expressionValue)))
            internal ExpressionToken GenerateExpressionToken()
                if (_fatalError)
                    // we failed the loading already, just return
                if (_token == null)
                    // we do not have a token: we never got one
                    // the user should have specified either a property or a script block
                    _loader.ReportMissingNodes(new string[] { XmlTags.PropertyNameNode, XmlTags.ScriptBlockNode });
            private readonly TypeInfoDataBaseLoader _loader;
            private ExpressionToken _token;
            private bool _fatalError = false;
        /// Helper class to wrap the loading of an expression (using ExpressionNodeMatch)
        /// plus the formatting string and an alternative text node.
        private sealed class ViewEntryNodeMatch
            internal ViewEntryNodeMatch(TypeInfoDataBaseLoader loader)
            internal bool ProcessExpressionDirectives(XmlNode containerNode, List<XmlNode> unprocessedNodes)
                if (containerNode == null)
                    throw PSTraceSource.NewArgumentNullException(nameof(containerNode));
                string formatString = null;
                TextToken textToken = null;
                ExpressionNodeMatch expressionMatch = new ExpressionNodeMatch(_loader);
                bool formatStringNodeFound = false; // cardinality 0..1
                bool expressionNodeFound = false;   // cardinality 0..1
                bool textNodeFound = false;         // cardinality 0..1
                foreach (XmlNode n in containerNode.ChildNodes)
                    else if (_loader.MatchNodeName(n, XmlTags.FormatStringNode))
                        if (formatStringNodeFound)
                        formatStringNodeFound = true;
                        formatString = _loader.GetMandatoryInnerText(n);
                        if (formatString == null)
                            // Error at XPath {0} in file {1}: Missing a format string.
                            _loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoFormatString, _loader.ComputeCurrentXPath(), _loader.FilePath));
                    else if (_loader.MatchNodeNameWithAttributes(n, XmlTags.TextNode))
                        if (textNodeFound)
                        textNodeFound = true;
                        textToken = _loader.LoadText(n);
                        if (textToken == null)
                            // Error at XPath {0} in file {1}: Invalid {2}.
                            _loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNode, _loader.ComputeCurrentXPath(), _loader.FilePath, XmlTags.TextNode));
                        // for further processing by calling context
                        unprocessedNodes.Add(n);
                    // RULE: cannot have a text node and an expression at the same time
                        // Error at XPath {0} in file {1}: {2} cannot be specified with an expression.
                        _loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NodeWithExpression, _loader.ComputeCurrentXPath(),
                            _loader.FilePath, XmlTags.TextNode));
                    // set the output data
                    if (!string.IsNullOrEmpty(formatString))
                        _formatString = formatString;
                    // RULE: we cannot have a format string without an expression node
                        // Error at XPath {0} in file {1}: {2} cannot be specified without an expression.
                        _loader.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NodeWithoutExpression, _loader.ComputeCurrentXPath(),
                            _loader.FilePath, XmlTags.FormatStringNode));
                    // we might have a text node
                        _textToken = textToken;
            internal string FormatString { get { return _formatString; } }
            internal TextToken TextToken { get { return _textToken; } }
            internal ExpressionToken Expression { get { return _expression; } }
            private string _formatString;
            private TextToken _textToken;
            private ExpressionToken _expression;
        #region Complex Control Loading
        private sealed class ComplexControlMatch
            internal ComplexControlMatch(TypeInfoDataBaseLoader loader)
                return _loader.MatchNodeName(n, XmlTags.ComplexControlNode) ||
                        _loader.MatchNodeName(n, XmlTags.ComplexControlNameNode);
                if (_loader.MatchNodeName(n, XmlTags.ComplexControlNode))
                    // load an embedded complex control
                    _control = _loader.LoadComplexControl(n);
                else if (_loader.MatchNodeName(n, XmlTags.ComplexControlNameNode))
                    string name = _loader.GetMandatoryInnerText(n);
                    ControlReference controlRef = new ControlReference();
                    controlRef.name = name;
                    controlRef.controlType = typeof(ComplexControlBody);
                    _control = controlRef;
            internal ControlBase Control
                get { return _control; }
            private ControlBase _control;
