    internal sealed class TableViewGenerator : ViewGenerator
        private TableControlBody _tableBody;
        private List<MshResolvedExpressionParameterAssociation> _activeAssociationList;
                _tableBody = (TableControlBody)this.dataBaseInfo.view.mainControl;
                                        PSObject so, TypeInfoDataBase db,
            FormattingCommandLineParameters parameters)
            // Build the active association list (with ExcludeProperty filter applied)
            _activeAssociationList = BuildActiveAssociationList(so);
        /// Builds the raw association list for table formatting.
            // check if we received properties from the command line
            if (propertyList is not null && propertyList.Count > 0)
                return AssociationManager.ExpandTableParameters(propertyList, so);
            // we did not get any properties:
            // try to get properties from the default property set of the object
            var list = AssociationManager.ExpandDefaultPropertySet(so, this.expressionFactory);
            if (list.Count > 0)
                if (PSObjectHelper.ShouldShowComputerNameProperty(so))
                    list.Add(new MshResolvedExpressionParameterAssociation(null,
            // we failed to get anything from the default property set
            list = AssociationManager.ExpandAll(so);
                AssociationManager.HandleComputerNameProperties(so, list);
                return LimitAssociationListSize(list);
            // we were unable to retrieve any properties, so we leave an empty list
            return new List<MshResolvedExpressionParameterAssociation>();
                // dont change the original format definition in the database..just make a copy and work
                // with the copy
                _tableBody = (TableControlBody)this.dataBaseInfo.view.mainControl.Copy();
                TableRowItemDefinition cnRowDefinition = new TableRowItemDefinition();
                PropertyTokenBase propToken = new FieldPropertyToken();
                propToken.expression = new ExpressionToken(RemotingConstants.ComputerNameNoteProperty, false);
                cnRowDefinition.formatTokenList.Add(propToken);
                _tableBody.defaultDefinition.rowItemDefinitionList.Add(cnRowDefinition);
                // add header only if there are other header definitions
                if (_tableBody.header.columnHeaderDefinitionList.Count > 0)
                    TableColumnHeaderDefinition cnHeaderDefinition = new TableColumnHeaderDefinition();
                    cnHeaderDefinition.label = new TextToken();
                    cnHeaderDefinition.label.text = RemotingConstants.ComputerNameNoteProperty;
                    _tableBody.header.columnHeaderDefinitionList.Add(cnHeaderDefinition);
                startFormat.shapeInfo = GenerateTableHeaderInfoFromDataBaseInfo(so);
                startFormat.shapeInfo = GenerateTableHeaderInfoFromProperties(so);
        /// Limits the association list size for table view.
        /// <param name="list">The list to limit.</param>
        /// <returns>The limited list.</returns>
        private static List<MshResolvedExpressionParameterAssociation> LimitAssociationListSize(
            List<MshResolvedExpressionParameterAssociation> list)
            const int maxCount = 10;
            if (list.Count <= maxCount)
            var result = new List<MshResolvedExpressionParameterAssociation>(maxCount);
            for (int k = 0; k < maxCount; k++)
                result.Add(list[k]);
        private TableHeaderInfo GenerateTableHeaderInfoFromDataBaseInfo(PSObject so)
            TableHeaderInfo thi = new TableHeaderInfo();
            bool dummy;
            List<TableRowItemDefinition> activeRowItemDefinitionList = GetActiveTableRowDefinition(_tableBody, so, out dummy);
            thi.hideHeader = this.HideHeaders;
            thi.repeatHeader = this.RepeatHeader;
                TableColumnInfo ci = new TableColumnInfo();
                    colHeader = _tableBody.header.columnHeaderDefinitionList[col];
                if (colHeader != null)
                    ci.width = colHeader.width;
                    ci.alignment = colHeader.alignment;
                    if (colHeader.label != null)
                        if (colHeader.label.text != string.Empty)
                            ci.HeaderMatchesProperty = so.Properties[colHeader.label.text] is not null;
                        ci.label = this.dataBaseInfo.db.displayResourceManagerCache.GetTextTokenString(colHeader.label);
                if (ci.alignment == TextAlignment.Undefined)
                    ci.alignment = rowItem.alignment;
                if (ci.label == null)
                            ci.label = fpt.expression.expressionValue;
                            ci.label = this.dataBaseInfo.db.displayResourceManagerCache.GetTextTokenString(tt);
                        ci.label = string.Empty;
                thi.tableColumnInfoList.Add(ci);
            return thi;
        private TableHeaderInfo GenerateTableHeaderInfoFromProperties(PSObject so)
            for (int k = 0; k < _activeAssociationList.Count; k++)
                MshResolvedExpressionParameterAssociation a = _activeAssociationList[k];
                        ci.propertyName = (string)key;
                ci.propertyName ??= _activeAssociationList[k].ResolvedExpression.ToString();
                // set the width of the table
                    object key = a.OriginatingParameter.GetEntry(FormatParameterDefinitionKeys.WidthEntryKey);
                        ci.width = (int)key;
                        ci.width = 0; // let Column Width Manager decide the width
                // set the alignment
                    object key = a.OriginatingParameter.GetEntry(FormatParameterDefinitionKeys.AlignmentEntryKey);
                        ci.alignment = (int)key;
                        ci.alignment = ComputeDefaultAlignment(so, a.ResolvedExpression);
        private bool HideHeaders
                // first check command line, it takes the precedence
                if (this.parameters != null && this.parameters.shapeParameters != null)
                    TableSpecificParameters tableSpecific = (TableSpecificParameters)this.parameters.shapeParameters;
                    if (tableSpecific != null && tableSpecific.hideHeaders.HasValue)
                        return tableSpecific.hideHeaders.Value;
                // if we have a view, get the value out of it
                    return _tableBody.header.hideHeader;
        private bool RepeatHeaders
                if (this.parameters != null)
                    return this.parameters.repeatHeader;
        private static int ComputeDefaultAlignment(PSObject so, PSPropertyExpression ex)
            List<PSPropertyExpressionResult> rList = ex.GetValues(so);
            if ((rList.Count == 0) || (rList[0].Exception != null))
            object val = rList[0].Result;
            PSObject soVal = PSObject.AsPSObject(val);
            var typeNames = soVal.InternalTypeNames;
            if (string.Equals(PSObjectHelper.PSObjectIsOfExactType(typeNames),
                                "System.String", StringComparison.OrdinalIgnoreCase))
            if (DefaultScalarTypes.IsTypeInList(typeNames))
            TableRowEntry tre;
                tre = GenerateTableRowEntryFromDataBaseInfo(so, enumerationLimit);
                tre = GenerateTableRowEntryFromFromProperties(so, enumerationLimit);
                // get the global setting for multiline
                tre.multiLine = this.dataBaseInfo.db.defaultSettingsSection.MultilineTables;
            fed.formatEntryInfo = tre;
            // override from command line, if there
                if (tableSpecific != null && tableSpecific.multiLine.HasValue)
                    tre.multiLine = tableSpecific.multiLine.Value;
        private List<TableRowItemDefinition> GetActiveTableRowDefinition(TableControlBody tableBody, PSObject so,
                                                out bool multiLine)
            multiLine = tableBody.defaultDefinition.multiLine;
            // the overriding row definition takes the precedence
            if (matchingRowDefinition.multiLine)
                multiLine = matchingRowDefinition.multiLine;
            List<TableRowItemDefinition> activeRowItemDefinitionList = new List<TableRowItemDefinition>();
                // check if the row is an override or not
                    // it's a place holder, use the default
                    // use the override
        private TableRowEntry GenerateTableRowEntryFromDataBaseInfo(PSObject so, int enumerationLimit)
            TableRowEntry tre = new TableRowEntry();
            List<TableRowItemDefinition> activeRowItemDefinitionList = GetActiveTableRowDefinition(_tableBody, so, out tre.multiLine);
                FormatPropertyField fpf = GenerateFormatPropertyField(rowItem.formatTokenList, so, enumerationLimit);
                // get the alignment from the row entry
                // NOTE: if it's not set, the alignment sent with the header will prevail
                fpf.alignment = rowItem.alignment;
                tre.formatPropertyFieldList.Add(fpf);
            return tre;
        private TableRowEntry GenerateTableRowEntryFromFromProperties(PSObject so, int enumerationLimit)
                if (_activeAssociationList[k].OriginatingParameter != null)
                    directive = _activeAssociationList[k].OriginatingParameter.GetEntry(FormatParameterDefinitionKeys.FormatStringEntryKey) as FieldFormattingDirective;
                if (directive is null)
                    directive = new FieldFormattingDirective();
                    directive.isTable = true;
                fpf.propertyValue = this.GetExpressionDisplayValue(so, enumerationLimit, _activeAssociationList[k].ResolvedExpression, directive);
