    /// Helper class to hold a resolved expression and its
    /// originating parameter.
    internal sealed class MshResolvedExpressionParameterAssociation
        [TraceSource("MshResolvedExpressionParameterAssociation", "MshResolvedExpressionParameterAssociation")]
        internal static readonly PSTraceSource tracer = PSTraceSource.GetTracer("MshResolvedExpressionParameterAssociation",
                                                "MshResolvedExpressionParameterAssociation");
        internal MshResolvedExpressionParameterAssociation(MshParameter parameter, PSPropertyExpression expression)
                throw PSTraceSource.NewArgumentNullException(nameof(expression));
            OriginatingParameter = parameter;
            ResolvedExpression = expression;
        internal PSPropertyExpression ResolvedExpression { get; }
        internal MshParameter OriginatingParameter { get; }
    internal static class AssociationManager
        internal static List<MshResolvedExpressionParameterAssociation> SetupActiveProperties(List<MshParameter> rawMshParameterList,
                                                   PSObject target, PSPropertyExpressionFactory expressionFactory)
            if (rawMshParameterList != null && rawMshParameterList.Count > 0)
                return AssociationManager.ExpandParameters(rawMshParameterList, target);
            List<MshResolvedExpressionParameterAssociation> activeAssociationList = AssociationManager.ExpandDefaultPropertySet(target, expressionFactory);
                if (PSObjectHelper.ShouldShowComputerNameProperty(target))
                return activeAssociationList;
            activeAssociationList = AssociationManager.ExpandAll(target);
            AssociationManager.HandleComputerNameProperties(target, activeAssociationList);
        internal static List<MshResolvedExpressionParameterAssociation> ExpandTableParameters(List<MshParameter> parameters, PSObject target)
            List<MshResolvedExpressionParameterAssociation> retVal = new List<MshResolvedExpressionParameterAssociation>();
            foreach (MshParameter par in parameters)
                PSPropertyExpression expression = par.GetEntry(FormatParameterDefinitionKeys.ExpressionEntryKey) as PSPropertyExpression;
                List<PSPropertyExpression> expandedExpressionList = expression.ResolveNames(target);
                if (!expression.HasWildCardCharacters && expandedExpressionList.Count == 0)
                    // we did not find anything, mark as unresolved
                    retVal.Add(new MshResolvedExpressionParameterAssociation(par, expression));
                foreach (PSPropertyExpression ex in expandedExpressionList)
                    retVal.Add(new MshResolvedExpressionParameterAssociation(par, ex));
        internal static List<MshResolvedExpressionParameterAssociation> ExpandParameters(List<MshParameter> parameters, PSObject target)
        internal static List<MshResolvedExpressionParameterAssociation> ExpandDefaultPropertySet(PSObject target, PSPropertyExpressionFactory expressionFactory)
            List<PSPropertyExpression> expandedExpressionList = PSObjectHelper.GetDefaultPropertySet(target);
                retVal.Add(new MshResolvedExpressionParameterAssociation(null, ex));
        private static List<string> GetPropertyNamesFromView(PSObject source, PSMemberViewTypes viewType)
            Collection<CollectionEntry<PSMemberInfo>> memberCollection =
                PSObject.GetMemberCollection(viewType);
            PSMemberInfoIntegratingCollection<PSMemberInfo> membersToSearch =
                new PSMemberInfoIntegratingCollection<PSMemberInfo>(source, memberCollection);
            ReadOnlyPSMemberInfoCollection<PSMemberInfo> matchedMembers =
                membersToSearch.Match("*", PSMemberTypes.Properties);
            List<string> retVal = new List<string>();
            foreach (PSMemberInfo member in matchedMembers)
                retVal.Add(member.Name);
        internal static List<MshResolvedExpressionParameterAssociation> ExpandAll(PSObject target)
            List<string> adaptedProperties = GetPropertyNamesFromView(target, PSMemberViewTypes.Adapted);
            List<string> baseProperties = GetPropertyNamesFromView(target, PSMemberViewTypes.Base);
            List<string> extendedProperties = GetPropertyNamesFromView(target, PSMemberViewTypes.Extended);
            var displayedProperties = adaptedProperties.Count != 0 ? adaptedProperties : baseProperties;
            displayedProperties.AddRange(extendedProperties);
            Dictionary<string, object> duplicatesFinder = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (string property in displayedProperties)
                if (duplicatesFinder.TryAdd(property, null))
                    PSPropertyExpression expr = new PSPropertyExpression(property, true);
                    retVal.Add(new MshResolvedExpressionParameterAssociation(null, expr));
        /// Helper method to handle PSComputerName and PSShowComputerName properties from
        /// the formating objects. If PSShowComputerName exists and is false, removes
        /// PSComputerName from the display.
        /// PSShowComputerName is an internal property..so this property is always
        /// removed from the display.
        /// <param name="activeAssociationList"></param>
        internal static void HandleComputerNameProperties(PSObject so, List<MshResolvedExpressionParameterAssociation> activeAssociationList)
            if (so.Properties[RemotingConstants.ShowComputerNameNoteProperty] != null)
                // always remove PSShowComputerName for the display. This is an internal property
                // that should never be visible to the user.
                Collection<MshResolvedExpressionParameterAssociation> itemsToRemove = new Collection<MshResolvedExpressionParameterAssociation>();
                foreach (MshResolvedExpressionParameterAssociation cpProp in activeAssociationList)
                    if (cpProp.ResolvedExpression.ToString().Equals(RemotingConstants.ShowComputerNameNoteProperty,
                        itemsToRemove.Add(cpProp);
                // remove computername for remoteobjects..only if PSShowComputerName property exists
                // otherwise the PSComputerName property does not belong to a remote object:
                // Ex: icm $s { gps } | select pscomputername --> In this case we want to show
                // PSComputerName
                if ((so.Properties[RemotingConstants.ComputerNameNoteProperty] != null) &&
                    (!PSObjectHelper.ShouldShowComputerNameProperty(so)))
                        if (cpProp.ResolvedExpression.ToString().Equals(RemotingConstants.ComputerNameNoteProperty,
                if (itemsToRemove.Count > 0)
                    foreach (MshResolvedExpressionParameterAssociation itemToRemove in itemsToRemove)
                        activeAssociationList.Remove(itemToRemove);
