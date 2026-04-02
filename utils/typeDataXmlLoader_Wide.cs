        private WideControlBody LoadWideControl(XmlNode controlNode)
                bool wideViewEntriesFound = false;
                bool columnsNodeFound = false; // cardinality 0..1
                    if (MatchNodeName(n, XmlTags.AutoSizeNode))
                            this.ProcessDuplicateAlternateNode(n, XmlTags.AutoSizeNode, XmlTags.ColumnNumberNode);
                        wideBody.autosize = tempVal;
                    else if (MatchNodeName(n, XmlTags.ColumnNumberNode))
                        if (columnsNodeFound)
                        columnsNodeFound = true;
                        if (!ReadPositiveIntegerValue(n, out wideBody.columns))
                    else if (MatchNodeName(n, XmlTags.WideEntriesNode))
                        if (wideViewEntriesFound)
                        wideViewEntriesFound = true;
                        // now read the entries section
                        LoadWideControlEntries(n, wideBody);
                            // if we have an default entry, it means there was a failure
                if (autosizeNodeFound && columnsNodeFound)
                    this.ProcessDuplicateAlternateNode(XmlTags.AutoSizeNode, XmlTags.ColumnNumberNode);
                if (!wideViewEntriesFound)
                    this.ReportMissingNode(XmlTags.WideEntriesNode);
        private void LoadWideControlEntries(XmlNode wideControlEntriesNode, WideControlBody wideBody)
            using (this.StackFrame(wideControlEntriesNode))
                foreach (XmlNode n in wideControlEntriesNode.ChildNodes)
                    if (MatchNodeName(n, XmlTags.WideEntryNode))
                        WideControlEntryDefinition wved = LoadWideControlEntry(n, entryIndex++);
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNode, ComputeCurrentXPath(), FilePath, XmlTags.WideEntryNode));
                                this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.TooManyDefaultShapeEntry, ComputeCurrentXPath(), FilePath, XmlTags.WideEntryNode));
                    this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.NoDefaultShapeEntry, ComputeCurrentXPath(), FilePath, XmlTags.WideEntryNode));
        private WideControlEntryDefinition LoadWideControlEntry(XmlNode wideControlEntryNode, int index)
            using (this.StackFrame(wideControlEntryNode, index))
                bool appliesToNodeFound = false;     // cardinality 0..1
                bool propertyEntryNodeFound = false; // cardinality 1
                foreach (XmlNode n in wideControlEntryNode.ChildNodes)
                        wved.appliesTo = LoadAppliesToSection(n, true);
                    else if (MatchNodeName(n, XmlTags.WideItemNode))
                        if (propertyEntryNodeFound)
                        propertyEntryNodeFound = true;
                        wved.formatTokenList = LoadPropertyEntry(n);
                        if (wved.formatTokenList == null)
                            this.ReportError(StringUtil.Format(FormatAndOutXmlLoadingStrings.InvalidNode, ComputeCurrentXPath(), FilePath, XmlTags.WideItemNode));
                if (wved.formatTokenList.Count == 0)
                    // Error at XPath {0} in file {1}: Missing WideItem.
                    this.ReportMissingNode(XmlTags.WideItemNode);
        private List<FormatToken> LoadPropertyEntry(XmlNode propertyEntryNode)
                    formatTokenList.Add(match.TextToken);
                    formatTokenList.Add(fpt);
