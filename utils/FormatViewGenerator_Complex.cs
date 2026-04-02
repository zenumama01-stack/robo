    internal sealed class ComplexViewGenerator : ViewGenerator
        internal override void Initialize(TerminatingErrorContext errorContext, PSPropertyExpressionFactory expressionFactory,
            PSObject so, TypeInfoDataBase db, FormattingCommandLineParameters parameters)
            base.Initialize(errorContext, expressionFactory, so, db, parameters);
        internal override FormatStartData GenerateStartData(PSObject so)
            FormatStartData startFormat = base.GenerateStartData(so);
            startFormat.shapeInfo = new ComplexViewHeaderInfo();
        internal override FormatEntryData GeneratePayload(PSObject so, int enumerationLimit)
                fed.formatEntryInfo = GenerateComplexViewEntryFromDataBaseInfo(so, enumerationLimit);
                fed.formatEntryInfo = GenerateComplexViewEntryFromProperties(so, enumerationLimit);
            return fed;
        private ComplexViewEntry GenerateComplexViewEntryFromProperties(PSObject so, int enumerationLimit)
            ComplexViewObjectBrowser browser = new ComplexViewObjectBrowser(this.ErrorManager, this.expressionFactory, enumerationLimit);
            return browser.GenerateView(so, this.parameters);
        private ComplexViewEntry GenerateComplexViewEntryFromDataBaseInfo(PSObject so, int enumerationLimit)
            // execute on the format directive
                this.dataBaseInfo.view.mainControl, so, cve.formatValueList);
            return cve;
    /// Class to process a complex control directive and generate
    /// the corresponding formatting tokens.
    internal sealed class ComplexControlGenerator
        internal ComplexControlGenerator(TypeInfoDataBase dataBase,
                                            DatabaseLoadingInfo loadingInfo,
                                            PSPropertyExpressionFactory expressionFactory,
                                            List<ControlDefinition> controlDefinitionList,
                                            FormatErrorManager resultErrorManager,
                                            int enumerationLimit,
                                            TerminatingErrorContext errorContext)
            _db = dataBase;
            _loadingInfo = loadingInfo;
            _controlDefinitionList = controlDefinitionList;
            _errorManager = resultErrorManager;
            _enumerationLimit = enumerationLimit;
            _errorContext = errorContext;
        internal void GenerateFormatEntries(int maxTreeDepth, ControlBase control,
                PSObject so, List<FormatValue> formatValueList)
                throw PSTraceSource.NewArgumentNullException(nameof(control));
            ExecuteFormatControl(new TraversalInfo(0, maxTreeDepth), control,
                                    so, formatValueList);
        private bool ExecuteFormatControl(TraversalInfo level, ControlBase control,
            // we are looking for a complex control to execute
            ComplexControlBody complexBody = null;
            // we might have a reference
            if (control is ControlReference controlReference && controlReference.controlType == typeof(ComplexControlBody))
                // retrieve the reference
                complexBody = DisplayDataQuery.ResolveControlReference(
                                        _db,
                                        _controlDefinitionList,
                                        controlReference) as ComplexControlBody;
                // try as an in line control
                complexBody = control as ComplexControlBody;
            // finally, execute the control body
            if (complexBody != null)
                // we have an inline control, just execute it
                ExecuteFormatControlBody(level, so, complexBody, formatValueList);
        private void ExecuteFormatControlBody(TraversalInfo level,
                PSObject so, ComplexControlBody complexBody, List<FormatValue> formatValueList)
            ComplexControlEntryDefinition activeControlEntryDefinition =
                    GetActiveComplexControlEntryDefinition(complexBody, so);
            ExecuteFormatTokenList(level,
                                 so, activeControlEntryDefinition.itemDefinition.formatTokenList, formatValueList);
        private ComplexControlEntryDefinition GetActiveComplexControlEntryDefinition(ComplexControlBody complexBody, PSObject so)
            TypeMatch match = new TypeMatch(_expressionFactory, _db, typeNames);
            foreach (ComplexControlEntryDefinition x in complexBody.optionalEntryList)
                if (match.PerfectMatch(new TypeMatchItem(x, x.appliesTo, so)))
                    return x;
            if (match.BestMatch != null)
                return match.BestMatch as ComplexControlEntryDefinition;
                    match = new TypeMatch(_expressionFactory, _db, typesWithoutPrefix);
                return complexBody.defaultEntry;
        private void ExecuteFormatTokenList(TraversalInfo level,
                PSObject so, List<FormatToken> formatTokenList, List<FormatValue> formatValueList)
            if (so == null)
                throw PSTraceSource.NewArgumentNullException(nameof(so));
            // guard against infinite loop
            if (level.Level == level.MaxDepth)
            formatValueList.Add(fe);
            #region foreach loop
            foreach (FormatToken t in formatTokenList)
                if (t is TextToken tt)
                    ftf.text = _db.displayResourceManagerCache.GetTextTokenString(tt);
                if (t is NewLineToken newline)
                    for (int i = 0; i < newline.count; i++)
                if (t is FrameToken ft)
                    // instantiate a new entry and attach a frame info object
                    FormatEntry feFrame = new FormatEntry();
                    feFrame.frameInfo = new FrameInfo();
                    // add the frame info
                    feFrame.frameInfo.firstLine = ft.frameInfoDefinition.firstLine;
                    feFrame.frameInfo.leftIndentation = ft.frameInfoDefinition.leftIndentation;
                    feFrame.frameInfo.rightIndentation = ft.frameInfoDefinition.rightIndentation;
                    // execute the list inside the frame
                    ExecuteFormatTokenList(level, so, ft.itemDefinition.formatTokenList, feFrame.formatValueList);
                    // add the frame computation results to the current format entry
                    fe.formatValueList.Add(feFrame);
                #region CompoundPropertyToken
                if (t is CompoundPropertyToken cpt)
                    if (!EvaluateDisplayCondition(so, cpt.conditionToken))
                        // token not active, skip it
                    // get the property from the object
                    object val = null;
                    // if no expression was specified, just use the
                    // object itself
                    if (cpt.expression == null || string.IsNullOrEmpty(cpt.expression.expressionValue))
                        val = so;
                        PSPropertyExpression ex = _expressionFactory.CreateFromExpressionToken(cpt.expression, _loadingInfo);
                        List<PSPropertyExpressionResult> resultList = ex.GetValues(so);
                        if (resultList.Count > 0)
                            val = resultList[0].Result;
                            if (resultList[0].Exception != null)
                                _errorManager.LogPSPropertyExpressionFailedResult(resultList[0], so);
                    // if the token is has a formatting string, it's a leaf node,
                    // do the formatting and we will be done
                    if (cpt.control == null || cpt.control is FieldControlBody)
                        // Since it is a leaf node we just consider it an empty string and go
                        // on with formatting
                        val ??= string.Empty;
                        FieldFormattingDirective fieldFormattingDirective = null;
                        if (cpt.control != null)
                            fieldFormattingDirective = ((FieldControlBody)cpt.control).fieldFormattingDirective;
                            if (fieldFormattingDirective != null && _errorManager.DisplayFormatErrorString)
                        IEnumerable e = PSObjectHelper.GetEnumerable(val);
                        if (cpt.enumerateCollection && e != null)
                            foreach (object x in e)
                                if (x == null)
                                    // nothing to process
                                fpf = new FormatPropertyField();
                                fpf.propertyValue = PSObjectHelper.FormatField(fieldFormattingDirective, x, _enumerationLimit, formatErrorObject, _expressionFactory);
                            fpf.propertyValue = PSObjectHelper.FormatField(fieldFormattingDirective, val, _enumerationLimit, formatErrorObject, _expressionFactory);
                            fpf.propertyValue = _errorManager.FormatErrorString;
                        // An empty result that is not a leaf node should not be expanded
                                // proceed with the recursion
                                ExecuteFormatControl(level.NextLevel, cpt.control, PSObject.AsPSObject(x), fe.formatValueList);
                            ExecuteFormatControl(level.NextLevel, cpt.control, PSObjectHelper.AsPSObject(val), fe.formatValueList);
                #endregion CompoundPropertyToken
            #endregion foreach loop
        private bool EvaluateDisplayCondition(PSObject so, ExpressionToken conditionToken)
            PSPropertyExpression ex = _expressionFactory.CreateFromExpressionToken(conditionToken, _loadingInfo);
        private readonly TypeInfoDataBase _db;
        private readonly DatabaseLoadingInfo _loadingInfo;
        private readonly PSPropertyExpressionFactory _expressionFactory;
        private readonly List<ControlDefinition> _controlDefinitionList;
        private readonly FormatErrorManager _errorManager;
        private readonly TerminatingErrorContext _errorContext;
        private readonly int _enumerationLimit;
    internal class TraversalInfo
        internal TraversalInfo(int level, int maxDepth)
            _maxDepth = maxDepth;
        internal int Level { get { return _level; } }
        internal int MaxDepth { get { return _maxDepth; } }
        internal TraversalInfo NextLevel
                return new TraversalInfo(_level + 1, _maxDepth);
        private readonly int _level;
        private readonly int _maxDepth;
    /// Class to generate a complex view from properties.
    internal sealed class ComplexViewObjectBrowser
        internal ComplexViewObjectBrowser(FormatErrorManager resultErrorManager, PSPropertyExpressionFactory mshExpressionFactory, int enumerationLimit)
            _expressionFactory = mshExpressionFactory;
        /// Given an object, generate a tree-like view
        /// <param name="parameters">Parameters from the command line.</param>
        /// <returns>Complex view entry to send to the output command.</returns>
        internal ComplexViewEntry GenerateView(PSObject so, FormattingCommandLineParameters parameters)
            _parameters = parameters;
            _complexSpecificParameters = (ComplexSpecificParameters)parameters.shapeParameters;
            int maxDepth = _complexSpecificParameters.maxDepth;
            TraversalInfo level = new TraversalInfo(0, maxDepth);
            List<MshParameter> mshParameterList = null;
            mshParameterList = parameters.mshParameterList;
            // create a top level entry as root of the tree
            if (TreatAsScalarType(typeNames))
                DisplayRawObject(so, fe.formatValueList);
                // check if the top level object is an enumeration
                    // let's start the traversal with an enumeration
                    DisplayEnumeration(e, level, fe.formatValueList);
                    // let's start the traversal with a traversal on properties
                    DisplayObject(so, level, mshParameterList, cve.formatValueList);
        private void DisplayRawObject(PSObject so, List<FormatValue> formatValueList)
            fpf.propertyValue = PSObjectHelper.SmartToString(so, _expressionFactory, _enumerationLimit, formatErrorObject);
            formatValueList.Add(fpf);
            formatValueList.Add(new FormatNewLine());
        /// Recursive call to display an object.
        /// <param name="so">Object to display.</param>
        /// <param name="currentLevel">Current level in the traversal.</param>
        /// <param name="parameterList">List of parameters from the command line.</param>
        /// <param name="formatValueList">List of format tokens to add to.</param>
        private void DisplayObject(PSObject so, TraversalInfo currentLevel, List<MshParameter> parameterList,
                                        List<FormatValue> formatValueList)
            // resolve the names of the properties
            List<MshResolvedExpressionParameterAssociation> activeAssociationList =
                        AssociationManager.SetupActiveProperties(parameterList, so, _expressionFactory);
            // Apply ExcludeProperty filter using the centralized method
            activeAssociationList = ViewGenerator.ApplyExcludeFilter(activeAssociationList, _parameters?.excludePropertyFilter);
            // create a format entry
            // add the display name of the object
            string objectDisplayName = GetObjectDisplayName(so);
            if (objectDisplayName != null)
                objectDisplayName = "class " + objectDisplayName;
            AddPrologue(fe.formatValueList, "{", objectDisplayName);
            ProcessActiveAssociationList(so, currentLevel, activeAssociationList, AddIndentationLevel(fe.formatValueList));
            AddEpilogue(fe.formatValueList, "}");
        private void ProcessActiveAssociationList(PSObject so,
                                TraversalInfo currentLevel,
                                List<MshResolvedExpressionParameterAssociation> activeAssociationList,
            foreach (MshResolvedExpressionParameterAssociation a in activeAssociationList)
                ftf.text = a.ResolvedExpression.ToString() + " = ";
                formatValueList.Add(ftf);
                // compute the value of the entry
                List<PSPropertyExpressionResult> resList = a.ResolvedExpression.GetValues(so);
                if (resList.Count >= 1)
                        _errorManager.LogPSPropertyExpressionFailedResult(result, so);
                            val = _errorManager.ErrorString;
                            val = string.Empty;
                        val = result.Result;
                // extract the optional max depth
                TraversalInfo level = currentLevel;
                if (a.OriginatingParameter != null)
                    object maxDepthKey = a.OriginatingParameter.GetEntry(FormatParameterDefinitionKeys.DepthEntryKey);
                    if (maxDepthKey != AutomationNull.Value)
                        int parameterMaxDept = (int)maxDepthKey;
                        level = new TraversalInfo(currentLevel.Level, parameterMaxDept);
                IEnumerable e = null;
                if (val != null || (level.Level >= level.MaxDepth))
                    e = PSObjectHelper.GetEnumerable(val);
                    DisplayEnumeration(e, level.NextLevel, AddIndentationLevel(formatValueList));
                else if (val == null || TreatAsLeafNode(val, level))
                    DisplayLeaf(val, formatValueList);
                    // we need to go one more level down
                    DisplayObject(PSObject.AsPSObject(val), level.NextLevel, null,
                        AddIndentationLevel(formatValueList));
        /// <param name="e">Enumeration to display.</param>
        /// <param name="level">Current level in the traversal.</param>
        private void DisplayEnumeration(IEnumerable e, TraversalInfo level, List<FormatValue> formatValueList)
            AddPrologue(formatValueList, "[", null);
            DisplayEnumerationInner(e, level, AddIndentationLevel(formatValueList));
            AddEpilogue(formatValueList, "]");
        private void DisplayEnumerationInner(IEnumerable e, TraversalInfo level, List<FormatValue> formatValueList)
            int enumCount = 0;
                if (LocalPipeline.GetExecutionContextFromTLS().CurrentPipelineStopping)
                if (_enumerationLimit >= 0)
                    if (_enumerationLimit == enumCount)
                        DisplayLeaf(PSObjectHelper.Ellipsis, formatValueList);
                    enumCount++;
                if (TreatAsLeafNode(x, level))
                    DisplayLeaf(x, formatValueList);
                    IEnumerable e1 = PSObjectHelper.GetEnumerable(x);
                        DisplayEnumeration(e1, level.NextLevel, AddIndentationLevel(formatValueList));
                        DisplayObject(PSObjectHelper.AsPSObject(x), level.NextLevel, null, formatValueList);
        /// Display a leaf value.
        /// <param name="val">Object to display.</param>
        private void DisplayLeaf(object val, List<FormatValue> formatValueList)
            fpf.propertyValue = PSObjectHelper.FormatField(null, PSObjectHelper.AsPSObject(val), _enumerationLimit, null, _expressionFactory);
        /// Determine if we have to stop the expansion.
        /// <param name="val">Object to verify.</param>
        /// <param name="level">Current level of recursion.</param>
        private static bool TreatAsLeafNode(object val, TraversalInfo level)
            if (level.Level >= level.MaxDepth || val == null)
            return TreatAsScalarType(PSObject.GetTypeNames(val));
        /// Treat as scalar check.
        /// <param name="typeNames">Name of the type to check.</param>
        /// <returns>True if it has to be treated as a scalar.</returns>
        private static bool TreatAsScalarType(Collection<string> typeNames)
            return DefaultScalarTypes.IsTypeInList(typeNames);
        private string GetObjectDisplayName(PSObject so)
            if (_complexSpecificParameters.classDisplay == ComplexSpecificParameters.ClassInfoDisplay.none)
                return "PSObject";
            if (_complexSpecificParameters.classDisplay == ComplexSpecificParameters.ClassInfoDisplay.shortName)
                // get the last token in the full name
                string[] arr = typeNames[0].Split('.');
                if (arr.Length > 0)
                    return arr[arr.Length - 1];
            return typeNames[0];
        private static void AddPrologue(List<FormatValue> formatValueList, string openTag, string label)
                FormatTextField ftfLabel = new FormatTextField();
                ftfLabel.text = label;
                formatValueList.Add(ftfLabel);
            ftf.text = openTag;
        private static void AddEpilogue(List<FormatValue> formatValueList, string closeTag)
            ftf.text = closeTag;
        private List<FormatValue> AddIndentationLevel(List<FormatValue> formatValueList)
            feFrame.frameInfo.firstLine = 0;
            feFrame.frameInfo.leftIndentation = _indentationStep;
            feFrame.frameInfo.rightIndentation = 0;
            formatValueList.Add(feFrame);
            return feFrame.formatValueList;
        private FormattingCommandLineParameters _parameters;
        private ComplexSpecificParameters _complexSpecificParameters;
        /// Indentation added to each level in the recursion.
        private readonly int _indentationStep = 2;
