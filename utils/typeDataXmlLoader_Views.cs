        private void LoadViewDefinitions(TypeInfoDataBase db, XmlNode viewDefinitionsNode)
            using (this.StackFrame(viewDefinitionsNode))
                foreach (XmlNode n in viewDefinitionsNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.ViewNode))
                        ViewDefinition view = LoadView(n, index++);
                                "{0} view {1} is loaded from file {2}",
                                view.loadingInfo.filePath));
        private ViewDefinition LoadView(XmlNode viewNode, int index)
            using (this.StackFrame(viewNode, index))
                // load the common data
                bool success = LoadCommonViewData(viewNode, view, unprocessedNodes);
                    // Error at XPath {0} in file {1}: View cannot be loaded.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ViewNotLoaded, ComputeCurrentXPath(), FilePath));
                // add the main control constituting the view
                // only one control can exist, and it can be
                // of the various types: Table, List, etc.
                string[] controlNodeTags = new string[]
                    XmlTags.TableControlNode,
                    XmlTags.ListControlNode,
                    XmlTags.WideControlNode,
                    XmlTags.ComplexControlNode
                List<XmlNode> secondPassUnprocessedNodes = new List<XmlNode>();
                bool mainControlFound = false; // cardinality 1
                    if (MatchNodeName(n, XmlTags.TableControlNode))
                        if (mainControlFound)
                        mainControlFound = true;
                        view.mainControl = LoadTableControl(n);
                    else if (MatchNodeName(n, XmlTags.ListControlNode))
                        view.mainControl = LoadListControl(n);
                    else if (MatchNodeName(n, XmlTags.WideControlNode))
                        view.mainControl = LoadWideControl(n);
                    else if (MatchNodeName(n, XmlTags.ComplexControlNode))
                        view.mainControl = LoadComplexControl(n);
                        secondPassUnprocessedNodes.Add(n);
                    this.ReportMissingNodes(controlNodeTags);
                if (!LoadMainControlDependentData(secondPassUnprocessedNodes, view))
                if (view.outOfBand && (view.groupBy != null))
                    // we cannot have grouping and out of band at the same time
                    // Error at XPath {0} in file {1}: An Out Of Band view cannot have GroupBy.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.OutOfBandGroupByConflict, ComputeCurrentXPath(), FilePath));
        private bool LoadMainControlDependentData(List<XmlNode> unprocessedNodes, ViewDefinition view)
                bool outOfBandNodeFound = false; // cardinality 0..1
                bool controlDefinitionsFound = false; // cardinality 0..1
                if (MatchNodeName(n, XmlTags.OutOfBandNode))
                    if (outOfBandNodeFound)
                    outOfBandNodeFound = true;
                    if (!this.ReadBooleanNode(n, out view.outOfBand))
                    if (view.mainControl is not ComplexControlBody && view.mainControl is not ListControlBody)
                        // Error at XPath {0} in file {1}: Out Of Band views can only have CustomControl or ListControl.
                        ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidControlForOutOfBandView, ComputeCurrentXPath(), FilePath));
                    LoadControlDefinitions(n, view.formatControlDefinitionHolder.controlDefinitionList);
        private bool LoadCommonViewData(XmlNode viewNode, ViewDefinition view, List<XmlNode> unprocessedNodes)
            if (viewNode == null)
                throw PSTraceSource.NewArgumentNullException(nameof(viewNode));
            if (view == null)
                throw PSTraceSource.NewArgumentNullException(nameof(view));
            // set loading information
            view.loadingInfo.xPath = this.ComputeCurrentXPath();
            // start the loading process
            bool nameNodeFound = false;             // cardinality 1
            bool appliesToNodeFound = false;        // cardinality 1
            bool groupByFound = false;              // cardinality 0..1
            foreach (XmlNode n in viewNode.ChildNodes)
                    view.name = GetMandatoryInnerText(n);
                    if (view.name == null)
                else if (MatchNodeName(n, XmlTags.ViewSelectedByNode))
                    // if null, we invalidate the view
                    view.appliesTo = LoadAppliesToSection(n, false);
                    if (view.appliesTo == null)
                else if (MatchNodeName(n, XmlTags.GroupByNode))
                    if (groupByFound)
                    groupByFound = true;
                    view.groupBy = LoadGroupBySection(n);
                    if (view.groupBy == null)
                    // save for further processing
            if (!appliesToNodeFound)
                this.ReportMissingNode(XmlTags.ViewSelectedByNode);
        private void LoadControlDefinitions(XmlNode definitionsNode, List<ControlDefinition> controlDefinitionList)
            using (this.StackFrame(definitionsNode))
                int controlDefinitionIndex = 0;
                foreach (XmlNode n in definitionsNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.ControlNode))
                        ControlDefinition def = LoadControlDefinition(n, controlDefinitionIndex++);
                        if (def != null)
                            controlDefinitionList.Add(def);
        private ControlDefinition LoadControlDefinition(XmlNode controlDefinitionNode, int index)
            using (this.StackFrame(controlDefinitionNode, index))
                bool nameNodeFound = false;         // cardinality 1
                bool controlNodeFound = false;         // cardinality 1
                ControlDefinition def = new ControlDefinition();
                foreach (XmlNode n in controlDefinitionNode.ChildNodes)
                        def.name = GetMandatoryInnerText(n);
                        if (def.name == null)
                            // Error at XPath {0} in file {1}: Control cannot have a null Name.
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NullControlName, ComputeCurrentXPath(), FilePath));
                        // NOTE: for the time being we allow only complex control definitions to be loaded
                        if (controlNodeFound)
                        controlNodeFound = true;
                        def.controlBody = LoadComplexControl(n);
                        if (def.controlBody == null)
                    this.ReportMissingNode(XmlTags.ComplexControlNode);
                return def;
