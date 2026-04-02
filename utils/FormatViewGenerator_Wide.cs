    internal sealed class WideViewGenerator : ViewGenerator
        /// Builds the raw association list for wide formatting.
                return AssociationManager.ExpandParameters(propertyList, so);
            // try to get the display property of the object
            PSPropertyExpression displayNameExpression = PSObjectHelper.GetDisplayNameExpression(so, this.expressionFactory);
            if (displayNameExpression is not null)
                return new List<MshResolvedExpressionParameterAssociation>
                    new MshResolvedExpressionParameterAssociation(null, displayNameExpression)
            // try to get the default property set (we will use the first property)
            if (list.Count == 0)
                // just get all the properties
            WideViewHeaderInfo wideViewHeaderInfo = new WideViewHeaderInfo();
            startFormat.shapeInfo = wideViewHeaderInfo;
            if (!this.AutoSize)
                // autosize overrides columns
                wideViewHeaderInfo.columns = this.Columns;
                wideViewHeaderInfo.columns = 0;
        private int Columns
                // check command line first
                if (parameters != null && parameters.shapeParameters != null)
                    WideSpecificParameters wp = (WideSpecificParameters)parameters.shapeParameters;
                    if (wp.columns.HasValue)
                        return wp.columns.Value;
                // check if the view has info
                if (this.dataBaseInfo.view != null && this.dataBaseInfo.view.mainControl != null)
                    WideControlBody wideControl = (WideControlBody)this.dataBaseInfo.view.mainControl;
                    return wideControl.columns;
                // not specified
                fed.formatEntryInfo = GenerateWideViewEntryFromDataBaseInfo(so, enumerationLimit);
                fed.formatEntryInfo = GenerateWideViewEntryFromProperties(so, enumerationLimit);
        private WideViewEntry GenerateWideViewEntryFromDataBaseInfo(PSObject so, int enumerationLimit)
            WideControlBody wideBody = (WideControlBody)this.dataBaseInfo.view.mainControl;
            WideControlEntryDefinition activeWideControlEntryDefinition =
                    GetActiveWideControlEntryDefinition(wideBody, so);
            WideViewEntry wve = new WideViewEntry();
            wve.formatPropertyField = GenerateFormatPropertyField(activeWideControlEntryDefinition.formatTokenList, so, enumerationLimit);
            // wve.alignment = activeWideViewEntryDefinition.alignment;
            return wve;
        private WideControlEntryDefinition GetActiveWideControlEntryDefinition(WideControlBody wideBody, PSObject so)
            foreach (WideControlEntryDefinition x in wideBody.optionalEntryList)
                return match.BestMatch as WideControlEntryDefinition;
                return wideBody.defaultEntryDefinition;
        private WideViewEntry GenerateWideViewEntryFromProperties(PSObject so, int enumerationLimit)
            wve.formatPropertyField = fpf;
            if (associationList.Count > 0)
                // get the first one
                MshResolvedExpressionParameterAssociation a = associationList[0];
                fpf.propertyValue = this.GetExpressionDisplayValue(so, enumerationLimit, a.ResolvedExpression, directive);
