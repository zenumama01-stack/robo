    internal static class DisplayCondition
        internal static bool Evaluate(PSObject obj, PSPropertyExpression ex, out PSPropertyExpressionResult expressionResult)
            expressionResult = null;
            List<PSPropertyExpressionResult> res = ex.GetValues(obj);
            if (res.Count == 0)
            if (res[0].Exception != null)
                expressionResult = res[0];
            return LanguagePrimitives.IsTrue(res[0].Result);
    /// Helper object holding a generic object and the related
    /// "applies to" object.
    /// It is used in by the inheritance based type match algorithm.
    internal sealed class TypeMatchItem
        internal TypeMatchItem(object obj, AppliesTo a)
            Item = obj;
            AppliesTo = a;
        internal TypeMatchItem(object obj, AppliesTo a, PSObject currentObject)
            CurrentObject = currentObject;
        internal object Item { get; }
        internal AppliesTo AppliesTo { get; }
        internal PSObject CurrentObject { get; }
    /// Algorithm to execute a type match on a list of entities
    /// having an "applies to" associated object.
    internal sealed class TypeMatch
        [TraceSource("TypeMatch", "F&O TypeMatch")]
        private static readonly PSTraceSource s_classTracer =
            PSTraceSource.GetTracer("TypeMatch", "F&O TypeMatch");
        private static PSTraceSource s_activeTracer = null;
        private static PSTraceSource ActiveTracer
                return s_activeTracer ?? s_classTracer;
        internal static void SetTracer(PSTraceSource t)
            s_activeTracer = t;
        internal static void ResetTracer()
            s_activeTracer = s_classTracer;
        internal TypeMatch(PSPropertyExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames)
            _db = db;
            _typeNameHierarchy = typeNames;
            _useInheritance = true;
        internal TypeMatch(PSPropertyExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames, bool useInheritance)
            _useInheritance = useInheritance;
        internal bool PerfectMatch(TypeMatchItem item)
            int match = ComputeBestMatch(item.AppliesTo, item.CurrentObject);
            if (match == BestMatchIndexUndefined)
            if (_bestMatchIndex == BestMatchIndexUndefined ||
                match < _bestMatchIndex)
                _bestMatchIndex = match;
                _bestMatchItem = item;
            return _bestMatchIndex == BestMatchIndexPerfect;
        internal object BestMatch
                if (_bestMatchItem == null)
                return _bestMatchItem.Item;
        private int ComputeBestMatch(AppliesTo appliesTo, PSObject currentObject)
            int best = BestMatchIndexUndefined;
            foreach (TypeOrGroupReference r in appliesTo.referenceList)
                PSPropertyExpression ex = null;
                if (r.conditionToken != null)
                    ex = _expressionFactory.CreateFromExpressionToken(r.conditionToken);
                int currentMatch = BestMatchIndexUndefined;
                if (r is TypeReference tr)
                    // we have a type
                    currentMatch = MatchTypeIndex(tr.name, currentObject, ex);
                    // we have a type group reference
                    TypeGroupReference tgr = r as TypeGroupReference;
                    // find the type group definition the reference points to
                    TypeGroupDefinition tgd = DisplayDataQuery.FindGroupDefinition(_db, tgr.name);
                    if (tgd != null)
                        // we found the group, see if the group has the type
                        currentMatch = ComputeBestMatchInGroup(tgd, currentObject, ex);
                if (currentMatch == BestMatchIndexPerfect)
                    return currentMatch;
                if (best == BestMatchIndexUndefined || best < currentMatch)
                    best = currentMatch;
            return best;
        private int ComputeBestMatchInGroup(TypeGroupDefinition tgd, PSObject currentObject, PSPropertyExpression ex)
            foreach (TypeReference tr in tgd.typeReferenceList)
                int currentMatch = MatchTypeIndex(tr.name, currentObject, ex);
        private int MatchTypeIndex(string typeName, PSObject currentObject, PSPropertyExpression ex)
                return BestMatchIndexUndefined;
            foreach (string name in _typeNameHierarchy)
                if (string.Equals(name, typeName, StringComparison.OrdinalIgnoreCase)
                            && MatchCondition(currentObject, ex))
                    return k;
                if (k == 0 && !_useInheritance)
        private static bool MatchCondition(PSObject currentObject, PSPropertyExpression ex)
            if (ex == null)
            bool retVal = DisplayCondition.Evaluate(currentObject, ex, out expressionResult);
        private readonly Collection<string> _typeNameHierarchy;
        private readonly bool _useInheritance;
        private int _bestMatchIndex = BestMatchIndexUndefined;
        private TypeMatchItem _bestMatchItem;
        private const int BestMatchIndexUndefined = -1;
        private const int BestMatchIndexPerfect = 0;
    internal static class DisplayDataQuery
        [TraceSource("DisplayDataQuery", "DisplayDataQuery")]
            PSTraceSource.GetTracer("DisplayDataQuery", "DisplayDataQuery");
        internal static EnumerableExpansion GetEnumerableExpansionFromType(PSPropertyExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames)
            TypeMatch match = new TypeMatch(expressionFactory, db, typeNames);
            foreach (EnumerableExpansionDirective expansionDirective in db.defaultSettingsSection.enumerableExpansionDirectiveList)
                if (match.PerfectMatch(new TypeMatchItem(expansionDirective, expansionDirective.appliesTo)))
                    return expansionDirective.enumerableExpansion;
                return ((EnumerableExpansionDirective)(match.BestMatch)).enumerableExpansion;
                    EnumerableExpansion result = GetEnumerableExpansionFromType(expressionFactory, db, typesWithoutPrefix);
                // return a default value if no matches were found
                return EnumerableExpansion.EnumOnly;
        internal static FormatShape GetShapeFromType(PSPropertyExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames)
            ShapeSelectionDirectives shapeDirectives = db.defaultSettingsSection.shapeSelectionDirectives;
            foreach (FormatShapeSelectionOnType shapeSelOnType in shapeDirectives.formatShapeSelectionOnTypeList)
                if (match.PerfectMatch(new TypeMatchItem(shapeSelOnType, shapeSelOnType.appliesTo)))
                    return shapeSelOnType.formatShape;
                return ((FormatShapeSelectionOnType)(match.BestMatch)).formatShape;
                    FormatShape result = GetShapeFromType(expressionFactory, db, typesWithoutPrefix);
                return FormatShape.Undefined;
        internal static FormatShape GetShapeFromPropertyCount(TypeInfoDataBase db, int propertyCount)
            if (propertyCount <= db.defaultSettingsSection.shapeSelectionDirectives.PropertyCountForTable)
        internal static ViewDefinition GetViewByShapeAndType(PSPropertyExpressionFactory expressionFactory, TypeInfoDataBase db,
                FormatShape shape, Collection<string> typeNames, string viewName)
                return GetDefaultView(expressionFactory, db, typeNames);
            // map the FormatShape to a type derived from ViewDefinition
            System.Type t = null;
                t = typeof(TableControlBody);
                t = typeof(ListControlBody);
                t = typeof(WideControlBody);
                t = typeof(ComplexControlBody);
                Diagnostics.Assert(false, "unknown shape: this should never happen unless a new shape is added");
            return GetView(expressionFactory, db, t, typeNames, viewName);
        internal static ViewDefinition GetOutOfBandView(PSPropertyExpressionFactory expressionFactory,
                                                        TypeInfoDataBase db, Collection<string> typeNames)
            foreach (ViewDefinition vd in db.viewDefinitionsSection.viewDefinitionList)
                if (!IsOutOfBandView(vd))
                if (match.PerfectMatch(new TypeMatchItem(vd, vd.appliesTo)))
                    return vd;
            // this is the best match we had
            ViewDefinition result = match.BestMatch as ViewDefinition;
                    result = GetOutOfBandView(expressionFactory, db, typesWithoutPrefix);
        private static ViewDefinition GetView(PSPropertyExpressionFactory expressionFactory, TypeInfoDataBase db, System.Type mainControlType, Collection<string> typeNames, string viewName)
                if (vd == null || mainControlType != vd.mainControl.GetType())
                    ActiveTracer.WriteLine(
                        "NOT MATCH {0}  NAME: {1}",
                        ControlBase.GetControlShapeName(vd.mainControl), (vd != null ? vd.name : string.Empty));
                if (IsOutOfBandView(vd))
                        "NOT MATCH OutOfBand {0}  NAME: {1}",
                        ControlBase.GetControlShapeName(vd.mainControl), vd.name);
                if (vd.appliesTo == null)
                        "NOT MATCH {0}  NAME: {1}  No applicable types",
                // first make sure we match on name:
                // if not, we do not try a match at all
                if (viewName != null && !string.Equals(vd.name, viewName, StringComparison.OrdinalIgnoreCase))
                // check if we have a perfect match
                    TypeMatch.SetTracer(ActiveTracer);
                        TraceHelper(vd, true);
                    TypeMatch.ResetTracer();
                TraceHelper(vd, false);
            ViewDefinition result = GetBestMatch(match);
                    result = GetView(expressionFactory, db, mainControlType, typesWithoutPrefix, viewName);
        private static void TraceHelper(ViewDefinition vd, bool isMatched)
            if ((ActiveTracer.Options & PSTraceSourceOptions.WriteLine) != 0)
                foreach (TypeOrGroupReference togr in vd.appliesTo.referenceList)
                    sb.Append(isMatched ? "MATCH FOUND" : "NOT MATCH");
                    if (togr is TypeReference tr)
                        sb.AppendFormat(
                            " {0} NAME: {1}  TYPE: {2}",
                            ControlBase.GetControlShapeName(vd.mainControl),
                            vd.name,
                            tr.name);
                        TypeGroupReference tgr = togr as TypeGroupReference;
                            " {0} NAME: {1}  GROUP: {2}",
                            tgr.name);
                    ActiveTracer.WriteLine(sb.ToString());
        private static ViewDefinition GetBestMatch(TypeMatch match)
            ViewDefinition bestMatchedVD = match.BestMatch as ViewDefinition;
            if (bestMatchedVD != null)
                TraceHelper(bestMatchedVD, true);
            return bestMatchedVD;
        private static ViewDefinition GetDefaultView(PSPropertyExpressionFactory expressionFactory, TypeInfoDataBase db, Collection<string> typeNames)
                if (vd == null)
                    result = GetDefaultView(expressionFactory, db, typesWithoutPrefix);
        private static bool IsOutOfBandView(ViewDefinition vd)
            return (vd.mainControl is ComplexControlBody || vd.mainControl is ListControlBody) && vd.outOfBand;
        /// Given an appliesTo list, it finds all the types that are contained (following type
        /// group references)
        /// <param name="db">Database to use.</param>
        /// <param name="appliesTo">Object to lookup.</param>
        internal static AppliesTo GetAllApplicableTypes(TypeInfoDataBase db, AppliesTo appliesTo)
            var allTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                // if it is a type reference, just add the type name
                    allTypes.Add(tr.name);
                    // check if we have a type group reference
                    if (r is not TypeGroupReference tgr)
                    TypeGroupDefinition tgd = FindGroupDefinition(db, tgr.name);
                    if (tgd == null)
                    // we found the group, go over it
                    foreach (TypeReference x in tgd.typeReferenceList)
                        allTypes.Add(x.name);
            AppliesTo retVal = new AppliesTo();
            foreach (string x in allTypes)
                retVal.AddAppliesToType(x);
        internal static TypeGroupDefinition FindGroupDefinition(TypeInfoDataBase db, string groupName)
            foreach (TypeGroupDefinition tgd in db.typeGroupSection.typeGroupDefinitionList)
                if (string.Equals(tgd.name, groupName, StringComparison.OrdinalIgnoreCase))
                    return tgd;
        internal static ControlBody ResolveControlReference(TypeInfoDataBase db, List<ControlDefinition> viewControlDefinitionList,
                                                            ControlReference controlReference)
            // first tri to resolve the reference at the view level
            ControlBody controlBody = ResolveControlReferenceInList(controlReference,
                viewControlDefinitionList);
            if (controlBody != null)
                return controlBody;
            // fall back to the global definitions
            return ResolveControlReferenceInList(controlReference, db.formatControlDefinitionHolder.controlDefinitionList);
        private static ControlBody ResolveControlReferenceInList(ControlReference controlReference,
                                        List<ControlDefinition> controlDefinitionList)
            foreach (ControlDefinition x in controlDefinitionList)
                if (x.controlBody.GetType() != controlReference.controlType)
                if (string.Equals(controlReference.name, x.name, StringComparison.OrdinalIgnoreCase))
                    return x.controlBody;
