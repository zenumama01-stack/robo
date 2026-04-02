    internal sealed class ListViewGenerator : ViewGenerator
        // tableBody to use for this instance of the ViewGenerator;
        private ListControlBody _listBody;
        internal override void Initialize(TerminatingErrorContext terminatingErrorContext, PSPropertyExpressionFactory mshExpressionFactory, TypeInfoDataBase db, ViewDefinition view, FormattingCommandLineParameters formatParameters)
            base.Initialize(terminatingErrorContext, mshExpressionFactory, db, view, formatParameters);
            if ((this.dataBaseInfo != null) && (this.dataBaseInfo.view != null))
                _listBody = (ListControlBody)this.dataBaseInfo.view.mainControl;
        /// Builds the raw association list for list formatting.
        protected override List<MshResolvedExpressionParameterAssociation> BuildRawAssociationList(PSObject so, List<MshParameter> propertyList)
            return AssociationManager.SetupActiveProperties(propertyList, so, this.expressionFactory);
        /// Let the view prepare itself for RemoteObjects. This will add "ComputerName" to the
        /// table columns.
        internal override void PrepareForRemoteObjects(PSObject so)
            Diagnostics.Assert(so != null, "so cannot be null");
            // make sure computername property exists.
            Diagnostics.Assert(so.Properties[RemotingConstants.ComputerNameNoteProperty] != null,
                "PrepareForRemoteObjects cannot be called when the object does not contain ComputerName property.");
            if ((dataBaseInfo != null) && (dataBaseInfo.view != null) && (dataBaseInfo.view.mainControl != null))
                _listBody = (ListControlBody)this.dataBaseInfo.view.mainControl.Copy();
                // build up the definition for computer name.
                ListControlItemDefinition cnListItemDefinition = new ListControlItemDefinition();
                cnListItemDefinition.label = new TextToken();
                cnListItemDefinition.label.text = RemotingConstants.ComputerNameNoteProperty;
                FieldPropertyToken fpt = new FieldPropertyToken();
                fpt.expression = new ExpressionToken(RemotingConstants.ComputerNameNoteProperty, false);
                cnListItemDefinition.formatTokenList.Add(fpt);
                _listBody.defaultEntryDefinition.itemDefinitionList.Add(cnListItemDefinition);
            startFormat.shapeInfo = new ListViewHeaderInfo();
                fed.formatEntryInfo = GenerateListViewEntryFromDataBaseInfo(so, enumerationLimit);
                fed.formatEntryInfo = GenerateListViewEntryFromProperties(so, enumerationLimit);
        private ListViewEntry GenerateListViewEntryFromDataBaseInfo(PSObject so, int enumerationLimit)
            ListViewEntry lve = new ListViewEntry();
            ListControlEntryDefinition activeListControlEntryDefinition =
                GetActiveListControlEntryDefinition(_listBody, so);
            foreach (ListControlItemDefinition listItem in activeListControlEntryDefinition.itemDefinitionList)
                if (!EvaluateDisplayCondition(so, listItem.conditionToken))
                ListViewField lvf = new ListViewField();
                lvf.formatPropertyField = GenerateFormatPropertyField(listItem.formatTokenList, so, enumerationLimit, out result);
                // we need now to provide a label
                if (listItem.label != null)
                    // if the directive provides one, we use it
                    lvf.label = this.dataBaseInfo.db.displayResourceManagerCache.GetTextTokenString(listItem.label);
                else if (result != null)
                    // if we got a valid match from the Mshexpression, use it as a label
                    lvf.label = result.ResolvedExpression.ToString();
                    // we did fail getting a result (i.e. property does not exist on the object)
                    // we try to fall back and see if we have an un-resolved PSPropertyExpression
                    FormatToken token = listItem.formatTokenList[0];
                        // use the un-resolved PSPropertyExpression string as a label
                        lvf.label = ex.ToString();
                        // we had a text token, use it as a label (last resort...)
                        lvf.label = this.dataBaseInfo.db.displayResourceManagerCache.GetTextTokenString(tt);
                lve.listViewFieldList.Add(lvf);
            return lve;
        private ListControlEntryDefinition GetActiveListControlEntryDefinition(ListControlBody listBody, PSObject so)
            TypeMatch match = new TypeMatch(expressionFactory, this.dataBaseInfo.db, typeNames);
            foreach (ListControlEntryDefinition x in listBody.optionalEntryList)
                return match.BestMatch as ListControlEntryDefinition;
                    match = new TypeMatch(expressionFactory, this.dataBaseInfo.db, typesWithoutPrefix);
                return listBody.defaultEntryDefinition;
        private ListViewEntry GenerateListViewEntryFromProperties(PSObject so, int enumerationLimit)
            // Build active association list (with ExcludeProperty filter applied)
            var associationList = BuildActiveAssociationList(so);
            for (int k = 0; k < associationList.Count; k++)
                MshResolvedExpressionParameterAssociation a = associationList[k];
                    object key = a.OriginatingParameter.GetEntry(FormatParameterDefinitionKeys.LabelEntryKey);
                        lvf.propertyName = (string)key;
                        lvf.propertyName = a.ResolvedExpression.ToString();
                FieldFormattingDirective directive = null;
                    directive = a.OriginatingParameter.GetEntry(FormatParameterDefinitionKeys.FormatStringEntryKey) as FieldFormattingDirective;
                lvf.formatPropertyField.propertyValue = this.GetExpressionDisplayValue(so, enumerationLimit, a.ResolvedExpression, directive);
