        private ListControlBody LoadListControl(XmlNode controlNode)
                bool listViewEntriesFound = false;
                    if (MatchNodeName(n, XmlTags.ListEntriesNode))
                        if (listViewEntriesFound)
                        listViewEntriesFound = true;
                        LoadListControlEntries(n, listBody);
                if (!listViewEntriesFound)
                    this.ReportMissingNode(XmlTags.ListEntriesNode);
        private void LoadListControlEntries(XmlNode listViewEntriesNode, ListControlBody listBody)
            using (this.StackFrame(listViewEntriesNode))
                foreach (XmlNode n in listViewEntriesNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.ListEntryNode))
                        ListControlEntryDefinition lved = LoadListControlEntryDefinition(n, entryIndex++);
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.LoadTagFailed, ComputeCurrentXPath(), FilePath, XmlTags.ListEntryNode));
                                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntry, ComputeCurrentXPath(), FilePath, XmlTags.ListEntryNode));
                if (listBody.optionalEntryList == null)
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntry, ComputeCurrentXPath(), FilePath, XmlTags.ListEntryNode));
        private ListControlEntryDefinition LoadListControlEntryDefinition(XmlNode listViewEntryNode, int index)
            using (this.StackFrame(listViewEntryNode, index))
                foreach (XmlNode n in listViewEntryNode.ChildNodes)
                        lved.appliesTo = LoadAppliesToSection(n, true);
                    else if (MatchNodeName(n, XmlTags.ListItemsNode))
                        LoadListControlItemDefinitions(lved, n);
                    // Error at XPath {0} in file {1}: Missing definition list.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefinitionList, ComputeCurrentXPath(), FilePath));
        private void LoadListControlItemDefinitions(ListControlEntryDefinition lved, XmlNode bodyNode)
                    if (MatchNodeName(n, XmlTags.ListItemNode))
                        ListControlItemDefinition lvid = LoadListControlItemDefinition(n);
                        if (lvid == null)
                            // Error at XPath {0} in file {1}: Invalid property entry.
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidPropertyEntry, ComputeCurrentXPath(), FilePath));
                    // Error at XPath {0} in file {1}: At least one list view item must be specified.
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoListViewItem, ComputeCurrentXPath(), FilePath));
        private ListControlItemDefinition LoadListControlItemDefinition(XmlNode propertyEntryNode)
            using (this.StackFrame(propertyEntryNode))
                // process Mshexpression, format string and text token
                ViewEntryNodeMatch match = new ViewEntryNodeMatch(this);
                if (!match.ProcessExpressionDirectives(propertyEntryNode, unprocessedNodes))
                // process the remaining nodes
                TextToken labelToken = null;
                bool labelNodeFound = false; // cardinality 0..1
                    if (MatchNodeName(n, XmlTags.ItemSelectionConditionNode))
                        if (labelNodeFound)
                        labelNodeFound = true;
                        labelToken = LoadLabel(n);
                        if (labelToken == null)
                // finally build the item to return
                // add the label
                lvid.label = labelToken;
                // add condition
                lvid.conditionToken = condition;
                // add either the text token or the PSPropertyExpression with optional format string
                if (match.TextToken != null)
                    lvid.formatTokenList.Add(match.TextToken);
                    fpt.expression = match.Expression;
                    fpt.fieldFormattingDirective.formatString = match.FormatString;
                return lvid;
        private ExpressionToken LoadItemSelectionCondition(XmlNode itemNode)
            using (this.StackFrame(itemNode))
                bool expressionNodeFound = false;     // cardinality 1
                foreach (XmlNode n in itemNode.ChildNodes)
                return expressionMatch.GenerateExpressionToken();
