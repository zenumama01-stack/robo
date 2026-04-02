        private ControlBase LoadTableControl(XmlNode controlNode)
                TableControlBody tableBody = new TableControlBody();
                bool headersNodeFound = false;      // cardinality 0..1
                bool rowEntriesNodeFound = false;   // cardinality 1
                bool hideHeadersNodeFound = false;   // cardinality 0..1
                bool autosizeNodeFound = false;   // cardinality 0..1
                    if (MatchNodeName(n, XmlTags.HideTableHeadersNode))
                        if (hideHeadersNodeFound)
                        hideHeadersNodeFound = true;
                        if (!this.ReadBooleanNode(n, out tableBody.header.hideHeader))
                    else if (MatchNodeName(n, XmlTags.AutoSizeNode))
                        if (autosizeNodeFound)
                        autosizeNodeFound = true;
                        if (!this.ReadBooleanNode(n, out tempVal))
                        tableBody.autosize = tempVal;
                    else if (MatchNodeName(n, XmlTags.TableHeadersNode))
                        if (headersNodeFound)
                        headersNodeFound = true;
                        // now read the columns header section
                        LoadHeadersSection(tableBody, n);
                        if (tableBody.header.columnHeaderDefinitionList == null)
                            // if we have an empty list, it means there was a failure
                    else if (MatchNodeName(n, XmlTags.TableRowEntriesNode))
                        if (rowEntriesNodeFound)
                        rowEntriesNodeFound = true;
                        LoadRowEntriesSection(tableBody, n);
                if (!rowEntriesNodeFound)
                    this.ReportMissingNode(XmlTags.TableRowEntriesNode);
                        this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.IncorrectHeaderItemCount, ComputeCurrentXPath(), FilePath,
                            tableBody.defaultDefinition.rowItemDefinitionList.Count));
                // CHECK: if there are alternative row definitions, they should have the same # of items
                if (tableBody.optionalDefinitionList.Count != 0)
                    foreach (TableRowDefinition trd in tableBody.optionalDefinitionList)
                        if (trd.rowItemDefinitionList.Count !=
                            // Error at XPath {0} in file {1}: Row item count = {2} on alternative set #{3} does not match default row item count = {4}.
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.IncorrectRowItemCount, ComputeCurrentXPath(), FilePath,
                                trd.rowItemDefinitionList.Count,
                                tableBody.defaultDefinition.rowItemDefinitionList.Count, k + 1));
        private void LoadHeadersSection(TableControlBody tableBody, XmlNode headersNode)
            using (this.StackFrame(headersNode))
                int columnIndex = 0;
                foreach (XmlNode n in headersNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.TableColumnHeaderNode))
                        TableColumnHeaderDefinition chd = LoadColumnHeaderDefinition(n, columnIndex++);
                        if (chd != null)
                            // Error at XPath {0} in file {1}: Column header definition is invalid; all headers are discarded.
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidColumnHeader, ComputeCurrentXPath(), FilePath));
                            tableBody.header.columnHeaderDefinitionList = null;
                // NOTICE: the list can be empty if no entries were found
        private TableColumnHeaderDefinition LoadColumnHeaderDefinition(XmlNode columnHeaderNode, int index)
            using (this.StackFrame(columnHeaderNode, index))
                bool widthNodeFound = false; // cardinality 0..1
                bool alignmentNodeFound = false; // cardinality 0..1
                foreach (XmlNode n in columnHeaderNode.ChildNodes)
                    if (MatchNodeNameWithAttributes(n, XmlTags.LabelNode))
                        chd.label = LoadLabel(n);
                        if (chd.label == null)
                    else if (MatchNodeName(n, XmlTags.WidthNode))
                        if (widthNodeFound)
                        widthNodeFound = true;
                        int wVal;
                        if (ReadPositiveIntegerValue(n, out wVal))
                            chd.width = wVal;
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNodeValue, ComputeCurrentXPath(), FilePath, XmlTags.WidthNode));
                    else if (MatchNodeName(n, XmlTags.AlignmentNode))
                        if (alignmentNodeFound)
                        alignmentNodeFound = true;
                        if (!LoadAlignmentValue(n, out chd.alignment))
                return chd;
        private bool ReadPositiveIntegerValue(XmlNode n, out int val)
            val = -1;
            string text = GetMandatoryInnerText(n);
            if (text == null)
            bool isInteger = int.TryParse(text, out val);
            if (!isInteger || val <= 0)
                // Error at XPath {0} in file {1}: A positive integer is expected.
                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.ExpectPositiveInteger, ComputeCurrentXPath(), FilePath));
        private bool LoadAlignmentValue(XmlNode n, out int alignmentValue)
            alignmentValue = TextAlignment.Undefined;
            string alignmentString = GetMandatoryInnerText(n);
            if (alignmentString == null)
            if (string.Equals(n.InnerText, XMLStringValues.AlignmentLeft, StringComparison.OrdinalIgnoreCase))
                alignmentValue = TextAlignment.Left;
            else if (string.Equals(n.InnerText, XMLStringValues.AlignmentRight, StringComparison.OrdinalIgnoreCase))
                alignmentValue = TextAlignment.Right;
            else if (string.Equals(n.InnerText, XMLStringValues.AlignmentCenter, StringComparison.OrdinalIgnoreCase))
                alignmentValue = TextAlignment.Center;
                // Error at XPath {0} in file {1}: "{2}" is not an valid alignment value.
                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidAlignmentValue, ComputeCurrentXPath(), FilePath, alignmentString));
        private void LoadRowEntriesSection(TableControlBody tableBody, XmlNode rowEntriesNode)
            using (this.StackFrame(rowEntriesNode))
                int rowEntryIndex = 0;
                foreach (XmlNode n in rowEntriesNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.TableRowEntryNode))
                        TableRowDefinition trd = LoadRowEntryDefinition(n, rowEntryIndex++);
                        if (trd == null)
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, ComputeCurrentXPath(), FilePath, XmlTags.TableRowEntryNode));
                        if (trd.appliesTo == null)
                                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntry, ComputeCurrentXPath(), FilePath, XmlTags.TableRowEntryNode));
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntry, ComputeCurrentXPath(), FilePath, XmlTags.TableRowEntryNode));
        private TableRowDefinition LoadRowEntryDefinition(XmlNode rowEntryNode, int index)
            using (this.StackFrame(rowEntryNode, index))
                bool columnEntriesNodeFound = false;         // cardinality 1
                bool multiLineFound = false;    // cardinality 0..1
                TableRowDefinition trd = new TableRowDefinition();
                foreach (XmlNode n in rowEntryNode.ChildNodes)
                        trd.appliesTo = LoadAppliesToSection(n, true);
                    else if (MatchNodeName(n, XmlTags.TableColumnItemsNode))
                        if (columnEntriesNodeFound)
                        LoadColumnEntries(n, trd);
                    else if (MatchNodeName(n, XmlTags.MultiLineNode))
                        if (multiLineFound)
                        multiLineFound = true;
                        if (!this.ReadBooleanNode(n, out trd.multiLine))
                return trd;
        private void LoadColumnEntries(XmlNode columnEntriesNode, TableRowDefinition trd)
            using (this.StackFrame(columnEntriesNode))
                int columnEntryIndex = 0;
                foreach (XmlNode n in columnEntriesNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.TableColumnItemNode))
                        TableRowItemDefinition rid = LoadColumnEntry(n, columnEntryIndex++);
                        if (rid != null)
                            // we failed one entry: fatal error to percolate up
                            // remove all the entries
        private TableRowItemDefinition LoadColumnEntry(XmlNode columnEntryNode, int index)
            using (this.StackFrame(columnEntryNode, index))
                if (!match.ProcessExpressionDirectives(columnEntryNode, unprocessedNodes))
                    if (MatchNodeName(n, XmlTags.AlignmentNode))
                        if (!LoadAlignmentValue(n, out rid.alignment))
                    rid.formatTokenList.Add(match.TextToken);
                else if (match.Expression != null)
                return rid;
