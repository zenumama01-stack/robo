        private ComplexControlBody LoadComplexControl(XmlNode controlNode)
            using (this.StackFrame(controlNode))
                ComplexControlBody complexBody = new ComplexControlBody();
                bool entriesFound = false;
                foreach (XmlNode n in controlNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.ComplexEntriesNode))
                        if (entriesFound)
                        entriesFound = true;
                        // now read the columns section
                        LoadComplexControlEntries(n, complexBody);
                        if (complexBody.defaultEntry == null)
                if (!entriesFound)
                    this.ReportMissingNode(XmlTags.ComplexEntriesNode);
                return complexBody;
        private void LoadComplexControlEntries(XmlNode complexControlEntriesNode, ComplexControlBody complexBody)
            using (this.StackFrame(complexControlEntriesNode))
                int entryIndex = 0;
                foreach (XmlNode n in complexControlEntriesNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.ComplexEntryNode))
                        ComplexControlEntryDefinition cced = LoadComplexControlEntryDefinition(n, entryIndex++);
                        if (cced == null)
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, ComputeCurrentXPath(), FilePath, XmlTags.ComplexEntryNode));
                            complexBody.defaultEntry = null;
                                complexBody.defaultEntry = cced;
                                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntry, ComputeCurrentXPath(), FilePath, XmlTags.ComplexEntryNode));
                            complexBody.optionalEntryList.Add(cced);
                    // Error at XPath {0} in file {1}: There must be at least one default {2}.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntry, ComputeCurrentXPath(), FilePath, XmlTags.ComplexEntryNode));
        private ComplexControlEntryDefinition LoadComplexControlEntryDefinition(XmlNode complexControlEntryNode, int index)
            using (this.StackFrame(complexControlEntryNode, index))
                bool appliesToNodeFound = false;    // cardinality 0..1
                bool bodyNodeFound = false;         // cardinality 1
                ComplexControlEntryDefinition cced = new ComplexControlEntryDefinition();
                foreach (XmlNode n in complexControlEntryNode.ChildNodes)
                        // optional section
                        cced.appliesTo = LoadAppliesToSection(n, true);
                    else if (MatchNodeName(n, XmlTags.ComplexItemNode))
                        if (bodyNodeFound)
                        bodyNodeFound = true;
                        cced.itemDefinition.formatTokenList = LoadComplexControlTokenListDefinitions(n);
                if (cced.itemDefinition.formatTokenList == null)
                    // MissingNode=Error at XPath {0} in file {1}: Missing Node {2}.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingNode, ComputeCurrentXPath(), FilePath, XmlTags.ComplexItemNode));
        private List<FormatToken> LoadComplexControlTokenListDefinitions(XmlNode bodyNode)
            using (this.StackFrame(bodyNode))
                List<FormatToken> formatTokenList = new List<FormatToken>();
                int compoundPropertyIndex = 0;
                int newLineIndex = 0;
                int textIndex = 0;
                int frameIndex = 0;
                foreach (XmlNode n in bodyNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.ExpressionBindingNode))
                        CompoundPropertyToken cpt = LoadCompoundProperty(n, compoundPropertyIndex++);
                        if (cpt == null)
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, ComputeCurrentXPath(), FilePath, XmlTags.ExpressionBindingNode));
                        formatTokenList.Add(cpt);
                    else if (MatchNodeName(n, XmlTags.NewLineNode))
                        NewLineToken nlt = LoadNewLine(n, newLineIndex++);
                        if (nlt == null)
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, ComputeCurrentXPath(), FilePath, XmlTags.NewLineNode));
                        formatTokenList.Add(nlt);
                    else if (MatchNodeNameWithAttributes(n, XmlTags.TextNode))
                        TextToken tt = LoadText(n, textIndex++);
                        if (tt == null)
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, ComputeCurrentXPath(), FilePath, XmlTags.TextNode));
                        formatTokenList.Add(tt);
                    else if (MatchNodeName(n, XmlTags.FrameNode))
                        FrameToken frame = LoadFrameDefinition(n, frameIndex++);
                        if (frame == null)
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, ComputeCurrentXPath(), FilePath, XmlTags.FrameNode));
                        formatTokenList.Add(frame);
                if (formatTokenList.Count == 0)
                    // Error at XPath {0} in file {1}: Empty custom control token list.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.EmptyCustomControlList, ComputeCurrentXPath(), FilePath));
                return formatTokenList;
        private bool LoadPropertyBaseHelper(XmlNode propertyBaseNode, PropertyTokenBase ptb, List<XmlNode> unprocessedNodes)
            bool expressionNodeFound = false;     // cardinality 0..1
            bool collectionNodeFound = false;       // cardinality 0..1
            bool itemSelectionConditionNodeFound = false; // cardinality 0..1
            ExpressionToken condition = null;
            foreach (XmlNode n in propertyBaseNode.ChildNodes)
                else if (MatchNodeName(n, XmlTags.EnumerateCollectionNode))
                    if (collectionNodeFound)
                    collectionNodeFound = true;
                    if (!ReadBooleanNode(n, out ptb.enumerateCollection))
                else if (MatchNodeName(n, XmlTags.ItemSelectionConditionNode))
                    if (itemSelectionConditionNodeFound)
                    itemSelectionConditionNodeFound = true;
                    condition = LoadItemSelectionCondition(n);
                    if (condition == null)
                    if (!IsFilteredOutNode(n))
                ptb.expression = expression;
                ptb.conditionToken = condition;
        // No used currently
        private FieldPropertyToken LoadFieldProperty (XmlNode fieldPropertyNode, int index)
            using (this.StackFrame (fieldPropertyNode, index))
                FieldPropertyToken fpt = new FieldPropertyToken ();
                List<XmlNode> unprocessedNodes = new List<XmlNode> ();
                bool success = LoadPropertyBaseHelper (fieldPropertyNode, fpt, unprocessedNodes);
                foreach (XmlNode n in unprocessedNodes)
                    this.ProcessUnknownNode (n);
                if (success && unprocessedNodes.Count == 0)
                    return fpt; // success
                return null; // failure
        private CompoundPropertyToken LoadCompoundProperty(XmlNode compoundPropertyNode, int index)
            using (this.StackFrame(compoundPropertyNode, index))
                CompoundPropertyToken cpt = new CompoundPropertyToken();
                List<XmlNode> unprocessedNodes = new List<XmlNode>();
                bool success = LoadPropertyBaseHelper(compoundPropertyNode, cpt, unprocessedNodes);
                cpt.control = null;
                // mutually exclusive
                bool complexControlFound = false;  // cardinality 0..1
                bool fieldControlFound = false;  // cardinality 0..1
                FieldControlBody fieldControlBody = null;
                    if (controlMatch.MatchNode(n))
                        if (complexControlFound)
                        complexControlFound = true;
                    else if (MatchNodeName(n, XmlTags.FieldControlNode))
                        if (fieldControlFound)
                        fieldControlFound = true;
                        fieldControlBody = new FieldControlBody();
                        fieldControlBody.fieldFormattingDirective.formatString = GetMandatoryInnerText(n);
                        if (fieldControlBody.fieldFormattingDirective.formatString == null)
                if (fieldControlFound && complexControlFound)
                    this.ProcessDuplicateAlternateNode(XmlTags.ComplexControlNode, XmlTags.ComplexControlNameNode);
                    cpt.control = fieldControlBody;
                    cpt.control = controlMatch.Control;
                if (cpt.control == null)
                    this.ReportMissingNodes (
                            new string[] { XmlTags.FieldControlNode, XmlTags.ComplexControlNode, XmlTags.ComplexControlNameNode });
        private NewLineToken LoadNewLine(XmlNode newLineNode, int index)
            using (this.StackFrame(newLineNode, index))
                if (!VerifyNodeHasNoChildren(newLineNode))
                NewLineToken nlt = new NewLineToken();
                return nlt;
        private TextToken LoadText(XmlNode textNode, int index)
            using (this.StackFrame(textNode, index))
        internal TextToken LoadText(XmlNode textNode)
        private int LoadIntegerValue(XmlNode node, out bool success)
            using (this.StackFrame(node))
                int retVal = 0;
                if (!VerifyNodeHasNoChildren(node))
                string val = this.GetMandatoryInnerText(node);
                    // Error at XPath {0} in file {1}: Missing inner text value.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.MissingInnerText, ComputeCurrentXPath(), FilePath));
                if (!int.TryParse(val, out retVal))
                    // Error at XPath {0} in file {1}: An integer is expected.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectInteger, ComputeCurrentXPath(), FilePath));
        private int LoadPositiveOrZeroIntegerValue(XmlNode node, out bool success)
            int val = LoadIntegerValue(node, out success);
                if (val < 0)
                    // Error at XPath {0} in file {1}: A non-negative integer is expected.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectNaturalNumber, ComputeCurrentXPath(), FilePath));
        private FrameToken LoadFrameDefinition(XmlNode frameNode, int index)
            using (this.StackFrame(frameNode, index))
                bool itemNodeFound = false; // cardinality 1
                bool leftIndentFound = false;   // cardinality 0..1
                bool rightIndentFound = false;  // cardinality 0..1
                bool firstLineIndentFound = false;  // cardinality 0..1
                bool firstLineHangingFound = false;  // cardinality 0..1
                bool success; // scratch variable
                FrameToken frame = new FrameToken();
                foreach (XmlNode n in frameNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.LeftIndentNode))
                        if (leftIndentFound)
                        leftIndentFound = true;
                        frame.frameInfoDefinition.leftIndentation = LoadPositiveOrZeroIntegerValue(n, out success);
                    else if (MatchNodeName(n, XmlTags.RightIndentNode))
                        if (rightIndentFound)
                        rightIndentFound = true;
                        frame.frameInfoDefinition.rightIndentation = LoadPositiveOrZeroIntegerValue(n, out success);
                    else if (MatchNodeName(n, XmlTags.FirstLineIndentNode))
                        if (firstLineIndentFound)
                            this.ProcessDuplicateAlternateNode(n, XmlTags.FirstLineIndentNode, XmlTags.FirstLineHangingNode);
                        firstLineIndentFound = true;
                        frame.frameInfoDefinition.firstLine = LoadPositiveOrZeroIntegerValue(n, out success);
                    else if (MatchNodeName(n, XmlTags.FirstLineHangingNode))
                        if (firstLineHangingFound)
                        firstLineHangingFound = true;
                        // hanging is codified as negative
                        frame.frameInfoDefinition.firstLine = -frame.frameInfoDefinition.firstLine;
                        if (itemNodeFound)
                        itemNodeFound = true;
                        frame.itemDefinition.formatTokenList = LoadComplexControlTokenListDefinitions(n);
                if (firstLineHangingFound && firstLineIndentFound)
                    this.ProcessDuplicateAlternateNode(XmlTags.FirstLineIndentNode, XmlTags.FirstLineHangingNode);
                if (frame.itemDefinition.formatTokenList == null)
        private bool ReadBooleanNode(XmlNode collectionElement, out bool val)
            val = false;
            if (!VerifyNodeHasNoChildren(collectionElement))
            string s = collectionElement.InnerText;
                val = true;
            if (string.Equals(s, XMLStringValues.False, StringComparison.OrdinalIgnoreCase))
            else if (string.Equals(s, XMLStringValues.True, StringComparison.OrdinalIgnoreCase))
            // Error at XPath {0} in file {1}: A Boolean value is expected.
            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectBoolean, ComputeCurrentXPath(), FilePath));
