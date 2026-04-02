using Microsoft.PowerShell.DesiredStateConfiguration.Internal;
    internal sealed partial class SemanticChecks : AstVisitor2, IAstPostVisitHandler
        private static readonly IsConstantValueVisitor s_isConstantAttributeArgVisitor = new IsConstantValueVisitor
            CheckingAttributeArgument = true,
        private static readonly IsConstantValueVisitor s_isConstantAttributeArgForClassVisitor = new IsConstantValueVisitor
            CheckingClassAttributeArguments = true
        private readonly Stack<MemberAst> _memberScopeStack;
        private readonly Stack<ScriptBlockAst> _scopeStack;
        internal static void CheckAst(Parser parser, ScriptBlockAst ast)
            SemanticChecks semanticChecker = new SemanticChecks(parser);
            semanticChecker._scopeStack.Push(ast);
            ast.InternalVisit(semanticChecker);
            semanticChecker._scopeStack.Pop();
            Diagnostics.Assert(semanticChecker._memberScopeStack.Count == 0, "Unbalanced push/pop of member scope stack");
            Diagnostics.Assert(semanticChecker._scopeStack.Count == 0, "Unbalanced push/pop of scope stack");
        private SemanticChecks(Parser parser)
            _memberScopeStack = new Stack<MemberAst>();
            _scopeStack = new Stack<ScriptBlockAst>();
        private bool AnalyzingStaticMember()
            MemberAst currentMember;
            if (_memberScopeStack.Count == 0 || (currentMember = _memberScopeStack.Peek()) == null)
            var fnMemberAst = currentMember as FunctionMemberAst;
            return fnMemberAst != null ? fnMemberAst.IsStatic : ((PropertyMemberAst)currentMember).IsStatic;
        private static bool IsValidAttributeArgument(Ast ast, IsConstantValueVisitor visitor)
            return (bool)ast.Accept(visitor);
        private static (string id, string msg) GetNonConstantAttributeArgErrorExpr(IsConstantValueVisitor visitor)
            if (visitor.CheckingClassAttributeArguments)
                return (nameof(ParserStrings.ParameterAttributeArgumentNeedsToBeConstant),
                    ParserStrings.ParameterAttributeArgumentNeedsToBeConstant);
            return (nameof(ParserStrings.ParameterAttributeArgumentNeedsToBeConstantOrScriptBlock),
                ParserStrings.ParameterAttributeArgumentNeedsToBeConstantOrScriptBlock);
        private void CheckForDuplicateParameters(ReadOnlyCollection<ParameterAst> parameters)
                HashSet<string> parametersSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    string parameterName = parameter.Name.VariablePath.UserPath;
                    if (!parametersSet.Add(parameterName))
                        _parser.ReportError(parameter.Name.Extent,
                            nameof(ParserStrings.DuplicateFormalParameter),
                            ParserStrings.DuplicateFormalParameter,
                    var voidConstraint =
                        parameter.Attributes.OfType<TypeConstraintAst>().FirstOrDefault(static t => typeof(void) == t.TypeName.GetReflectionType());
                    if (voidConstraint != null)
                        _parser.ReportError(voidConstraint.Extent,
                            nameof(ParserStrings.VoidTypeConstraintNotAllowed),
                            ParserStrings.VoidTypeConstraintNotAllowed);
        public override AstVisitAction VisitParamBlock(ParamBlockAst paramBlockAst)
            CheckForDuplicateParameters(paramBlockAst.Parameters);
        public override AstVisitAction VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
            CheckArrayTypeNameDepth(typeConstraintAst.TypeName, typeConstraintAst.Extent, _parser);
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool checkingAttributeOnClass = false;
            AttributeTargets attributeTargets = default(AttributeTargets);
            var parent = attributeAst.Parent;
            TypeDefinitionAst typeDefinitionAst = parent as TypeDefinitionAst;
                checkingAttributeOnClass = true;
                attributeTargets = typeDefinitionAst.IsClass
                    ? AttributeTargets.Class
                    : typeDefinitionAst.IsEnum
                        ? AttributeTargets.Enum
                        : AttributeTargets.Interface;
            else if (parent is PropertyMemberAst)
                attributeTargets = AttributeTargets.Property | AttributeTargets.Field;
                var functionMemberAst = parent as FunctionMemberAst;
                    attributeTargets = functionMemberAst.IsConstructor
                        ? AttributeTargets.Constructor
                        : AttributeTargets.Method;
                else if (parent is ParameterAst && _memberScopeStack.Peek() is FunctionMemberAst)
                    // TODO: we aren't actually generating any attributes in the class metadata
                    attributeTargets = AttributeTargets.Parameter;
            var constantValueVisitor = checkingAttributeOnClass
                ? s_isConstantAttributeArgForClassVisitor
                : s_isConstantAttributeArgVisitor;
            if (checkingAttributeOnClass)
                    Diagnostics.Assert(_parser.ErrorList.Count > 0, "Symbol resolve should have reported error already");
                    var usage = attributeType.GetCustomAttribute<AttributeUsageAttribute>(true);
                    if (usage != null && (usage.ValidOn & attributeTargets) == 0)
                        _parser.ReportError(attributeAst.Extent,
                            nameof(ParserStrings.AttributeNotAllowedOnDeclaration),
                            ParserStrings.AttributeNotAllowedOnDeclaration,
                            ToStringCodeMethods.Type(attributeType),
                            usage.ValidOn);
                            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance |
                            BindingFlags.FlattenHierarchy);
                        if (members.Length != 1
                            || (members[0] is not PropertyInfo && members[0] is not FieldInfo))
                            _parser.ReportError(namedArg.Extent,
                                nameof(ParserStrings.PropertyNotFoundForAttribute),
                                ParserStrings.PropertyNotFoundForAttribute,
                                GetValidNamedAttributeProperties(attributeType));
                        if (fieldInfo.IsInitOnly || fieldInfo.IsLiteral)
                string name = namedArg.ArgumentName;
                if (!names.Add(name))
                    if (!namedArg.ExpressionOmitted && !IsValidAttributeArgument(namedArg.Argument, constantValueVisitor))
                        var error = GetNonConstantAttributeArgErrorExpr(constantValueVisitor);
                        _parser.ReportError(namedArg.Argument.Extent, error.id, error.msg);
            foreach (var posArg in attributeAst.PositionalArguments)
                if (!IsValidAttributeArgument(posArg, constantValueVisitor))
                    _parser.ReportError(posArg.Extent, error.id, error.msg);
        private static string GetValidNamedAttributeProperties(Type attributeType)
            var propertyNames = new List<string>();
            PropertyInfo[] properties = attributeType.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            for (int i = 0; i < properties.Length; i++)
                PropertyInfo propertyInfo = properties[i];
                if (propertyInfo.GetSetMethod() != null)
                    propertyNames.Add(propertyInfo.Name);
            FieldInfo[] fields = attributeType.GetFields(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                FieldInfo fieldInfo = fields[i];
                if (!fieldInfo.IsInitOnly && !fieldInfo.IsLiteral)
                    propertyNames.Add(fieldInfo.Name);
            return string.Join(", ", propertyNames);
            bool isClassMethod = parameterAst.Parent.Parent is FunctionMemberAst;
            bool isParamTypeDefined = false;
            foreach (AttributeBaseAst attribute in parameterAst.Attributes)
                if (attribute is TypeConstraintAst)
                    if (attribute.TypeName.FullName.Equals(LanguagePrimitives.OrderedAttribute, StringComparison.OrdinalIgnoreCase))
                        _parser.ReportError(attribute.Extent,
                            nameof(ParserStrings.OrderedAttributeOnlyOnHashLiteralNode),
                            ParserStrings.OrderedAttributeOnlyOnHashLiteralNode,
                            attribute.TypeName.FullName);
                        if (isClassMethod)
                            // attribute represent parameter type.
                            if (isParamTypeDefined)
                                    nameof(ParserStrings.MultipleTypeConstraintsOnMethodParam),
                                    ParserStrings.MultipleTypeConstraintsOnMethodParam);
                            isParamTypeDefined = true;
        public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
            CheckArrayTypeNameDepth(typeExpressionAst.TypeName, typeExpressionAst.Extent, _parser);
            // If this is access to the [Type] class, it may be suspicious
            if (typeof(Type) == typeExpressionAst.TypeName.GetReflectionType())
                MarkAstParentsAsSuspicious(typeExpressionAst);
        internal static void CheckArrayTypeNameDepth(ITypeName typeName, IScriptExtent extent, Parser parser)
            ITypeName type = typeName;
            while (type is not TypeName)
                if (count > 200)
                    parser.ReportError(extent,
                if (type is ArrayTypeName)
                    type = ((ArrayTypeName)type).ElementType;
            AttributeAst dscResourceAttributeAst = null;
            for (int i = 0; i < typeDefinitionAst.Attributes.Count; i++)
                var attr = typeDefinitionAst.Attributes[i];
                    dscResourceAttributeAst = attr;
            if (dscResourceAttributeAst != null)
                DscResourceChecker.CheckType(_parser, typeDefinitionAst, dscResourceAttributeAst);
        public override AstVisitAction VisitFunctionMember(FunctionMemberAst functionMemberAst)
            _memberScopeStack.Push(functionMemberAst);
            var body = functionMemberAst.Body;
            if (body.ParamBlock != null)
                _parser.ReportError(body.ParamBlock.Extent,
                    nameof(ParserStrings.ParamBlockNotAllowedInMethod),
                    ParserStrings.ParamBlockNotAllowedInMethod);
            if (body.BeginBlock != null
                || body.ProcessBlock != null
                || body.CleanBlock != null
                || body.DynamicParamBlock != null
                || !body.EndBlock.Unnamed)
                _parser.ReportError(Parser.ExtentFromFirstOf(body.DynamicParamBlock, body.BeginBlock, body.ProcessBlock, body.EndBlock),
                    nameof(ParserStrings.NamedBlockNotAllowedInMethod),
                    ParserStrings.NamedBlockNotAllowedInMethod);
            if (functionMemberAst.IsConstructor && functionMemberAst.ReturnType != null)
                    nameof(ParserStrings.ConstructorCantHaveReturnType),
                    ParserStrings.ConstructorCantHaveReturnType);
            // Analysis determines if all paths return and do data flow for variables.
            var allCodePathsReturned = VariableAnalysis.AnalyzeMemberFunction(functionMemberAst);
            if (!allCodePathsReturned && !functionMemberAst.IsReturnTypeVoid())
                    nameof(ParserStrings.MethodHasCodePathNotReturn),
                    ParserStrings.MethodHasCodePathNotReturn);
            if (functionDefinitionAst.Parameters != null
                && functionDefinitionAst.Body.ParamBlock != null)
                _parser.ReportError(functionDefinitionAst.Body.ParamBlock.Extent,
                    nameof(ParserStrings.OnlyOneParameterListAllowed),
                    ParserStrings.OnlyOneParameterListAllowed);
            else if (functionDefinitionAst.Parameters != null)
                CheckForDuplicateParameters(functionDefinitionAst.Parameters);
            if (functionDefinitionAst.IsWorkflow)
                _parser.ReportError(functionDefinitionAst.Extent,
                    nameof(ParserStrings.WorkflowNotSupportedInPowerShellCore),
                    ParserStrings.WorkflowNotSupportedInPowerShellCore);
        public override AstVisitAction VisitSwitchStatement(SwitchStatementAst switchStatementAst)
            // Parallel flag not allowed
            if ((switchStatementAst.Flags & SwitchFlags.Parallel) == SwitchFlags.Parallel)
                    switchStatementAst.Extent,
                    nameof(ParserStrings.KeywordParameterReservedForFutureUse),
                    ParserStrings.KeywordParameterReservedForFutureUse,
                    "switch",
                    "parallel");
        private static IEnumerable<string> GetConstantDataStatementAllowedCommands(DataStatementAst dataStatementAst)
            yield return "ConvertFrom-StringData";
            foreach (var allowed in dataStatementAst.CommandsAllowed)
                yield return ((StringConstantExpressionAst)allowed).Value;
            IEnumerable<string> allowedCommands =
                dataStatementAst.HasNonConstantAllowedCommand ? null : GetConstantDataStatementAllowedCommands(dataStatementAst);
            RestrictedLanguageChecker checker = new RestrictedLanguageChecker(_parser, allowedCommands, null, false);
            dataStatementAst.Body.InternalVisit(checker);
            if ((forEachStatementAst.Flags & ForEachFlags.Parallel) == ForEachFlags.Parallel)
                    forEachStatementAst.Extent,
            if (forEachStatementAst.ThrottleLimit != null)
                    "throttlelimit");
            // Throttle limit must be combined with Parallel flag
            if ((forEachStatementAst.ThrottleLimit != null) &&
                ((forEachStatementAst.Flags & ForEachFlags.Parallel) != ForEachFlags.Parallel))
                    nameof(ParserStrings.ThrottleLimitRequiresParallelFlag),
                    ParserStrings.ThrottleLimitRequiresParallelFlag);
        public override AstVisitAction VisitTryStatement(TryStatementAst tryStatementAst)
            if (tryStatementAst.CatchClauses.Count <= 1)
            for (int i = 0; i < tryStatementAst.CatchClauses.Count - 1; ++i)
                CatchClauseAst block1 = tryStatementAst.CatchClauses[i];
                for (int j = i + 1; j < tryStatementAst.CatchClauses.Count; ++j)
                    CatchClauseAst block2 = tryStatementAst.CatchClauses[j];
                    if (block1.IsCatchAll)
                        _parser.ReportError(Parser.Before(block2.Extent),
                            nameof(ParserStrings.EmptyCatchNotLast),
                            ParserStrings.EmptyCatchNotLast);
                    if (block2.IsCatchAll)
                    foreach (TypeConstraintAst typeLiteral1 in block1.CatchTypes)
                        Type type1 = typeLiteral1.TypeName.GetReflectionType();
                        // If the type can't be resolved yet, there isn't much we can do, so skip it.
                        if (type1 == null)
                        foreach (TypeConstraintAst typeLiteral2 in block2.CatchTypes)
                            Type type2 = typeLiteral2.TypeName.GetReflectionType();
                            if (type2 == null)
                            if (type1 == type2 || type2.IsSubclassOf(type1))
                                _parser.ReportError(typeLiteral2.Extent,
                                    nameof(ParserStrings.ExceptionTypeAlreadyCaught),
                                    ParserStrings.ExceptionTypeAlreadyCaught,
                                    type2.FullName);
        /// Check that label exists inside the method.
        /// Only call it, when label is present and can be calculated in compile time.
        /// <param name="ast">BreakStatementAst or ContinueStatementAst.</param>
        /// <param name="label">Label name. Can be null.</param>
        private void CheckLabelExists(StatementAst ast, string label)
            Ast parent;
            for (parent = ast.Parent; parent != null; parent = parent.Parent)
                if (parent is FunctionDefinitionAst)
                    if (parent.Parent is FunctionMemberAst)
                        _parser.ReportError(ast.Extent,
                            nameof(ParserStrings.LabelNotFound),
                            ParserStrings.LabelNotFound,
                            label);
                var loop = parent as LoopStatementAst;
                if (loop != null)
                    if (LoopFlowException.MatchLoopLabel(label, loop.Label ?? string.Empty))
        /// Check that flow doesn't leave finally.
        /// <param name="label">If label is null, either it's a break/continue to an unknown label
        /// (and unknown does not mean not specified, it means it's an expression we can't evaluate) or we have a return statement.
        private void CheckForFlowOutOfFinally(Ast ast, string label)
                if (parent is NamedBlockAst || parent is TrapStatementAst || parent is ScriptBlockAst)
                    // Script blocks, traps, and named blocks are all top level asts for a complete method,
                    // so if we didn't find a try/catch, the control flow is just leaving the method,
                    // it is not leaving some finally, even if the script lock/trap is nested in the finally.
                // If label is not null, we have a break/continue where we know the loop label at compile
                // time. If we can match the label before finding the finally, then we're not flowing out
                // of the finally.
                if (label != null && parent is LabeledStatementAst)
                    if (LoopFlowException.MatchLoopLabel(label, ((LabeledStatementAst)parent).Label ?? string.Empty))
                var stmtBlock = parent as StatementBlockAst;
                if (stmtBlock != null)
                    var tryStatementAst = stmtBlock.Parent as TryStatementAst;
                    if (tryStatementAst != null && tryStatementAst.Finally == stmtBlock)
                            nameof(ParserStrings.ControlLeavingFinally),
                            ParserStrings.ControlLeavingFinally);
        private static string GetLabel(ExpressionAst expr)
            // We only return null from this method if the label is unknown.  If no label is specified,
            // we just use the empty string.
            var str = expr as StringConstantExpressionAst;
            return str?.Value;
        public override AstVisitAction VisitBreakStatement(BreakStatementAst breakStatementAst)
            string label = GetLabel(breakStatementAst.Label);
            CheckForFlowOutOfFinally(breakStatementAst, label);
            CheckLabelExists(breakStatementAst, label);
        public override AstVisitAction VisitContinueStatement(ContinueStatementAst continueStatementAst)
            string label = GetLabel(continueStatementAst.Label);
            CheckForFlowOutOfFinally(continueStatementAst, label);
            CheckLabelExists(continueStatementAst, label);
        private void CheckForReturnStatement(ReturnStatementAst ast)
            if (!(_memberScopeStack.Peek() is FunctionMemberAst functionMemberAst))
            if (ast.Pipeline != null)
                if (functionMemberAst.IsReturnTypeVoid())
                        nameof(ParserStrings.VoidMethodHasReturn),
                        ParserStrings.VoidMethodHasReturn);
                if (!functionMemberAst.IsReturnTypeVoid())
                        nameof(ParserStrings.NonVoidMethodMissingReturnValue),
                        ParserStrings.NonVoidMethodMissingReturnValue);
        public override AstVisitAction VisitReturnStatement(ReturnStatementAst returnStatementAst)
            CheckForFlowOutOfFinally(returnStatementAst, null);
            CheckForReturnStatement(returnStatementAst);
        /// Check if the ast is a valid target for assignment.  If not, the action reportError is called.
        /// <param name="ast">The target of an assignment.</param>
        /// <param name="simpleAssignment">True if the operator '=' is used, false otherwise (e.g. false on '+=' or '++'.).</param>
        /// <param name="reportError">The action called to report any errors.</param>
        private void CheckAssignmentTarget(ExpressionAst ast, bool simpleAssignment, Action<Ast> reportError)
            ArrayLiteralAst arrayLiteralAst = ast as ArrayLiteralAst;
            Ast errorAst = null;
            if (arrayLiteralAst != null)
                if (simpleAssignment)
                    CheckArrayLiteralAssignment(arrayLiteralAst, reportError);
                    errorAst = arrayLiteralAst;
                ParenExpressionAst parenExpressionAst = ast as ParenExpressionAst;
                    ExpressionAst expr = parenExpressionAst.Pipeline.GetPureExpression();
                        errorAst = parenExpressionAst.Pipeline;
                        CheckAssignmentTarget(expr, simpleAssignment, reportError);
                else if (ast is not ISupportsAssignment)
                    errorAst = ast;
                else if (ast is MemberExpressionAst memberExprAst && memberExprAst.NullConditional)
                else if (ast is IndexExpressionAst indexExprAst && indexExprAst.NullConditional)
                else if (ast is AttributedExpressionAst)
                    // Check for multiple types combined with [ref].
                    ExpressionAst expr = ast;
                    int converts = 0;
                    IScriptExtent errorPosition = null;
                    Type lastConvertType = null;
                        var convertExpr = expr as ConvertExpressionAst;
                        if (convertExpr != null)
                            converts += 1;
                            lastConvertType = convertExpr.Type.TypeName.GetReflectionType();
                            if (typeof(PSReference) == lastConvertType)
                                errorPosition = convertExpr.Type.Extent;
                            else if (typeof(void) == lastConvertType)
                                _parser.ReportError(convertExpr.Type.Extent,
                    if ((errorPosition != null) && converts > 1)
                        _parser.ReportError(errorPosition,
                            nameof(ParserStrings.ReferenceNeedsToBeByItselfInTypeConstraint),
                            ParserStrings.ReferenceNeedsToBeByItselfInTypeConstraint);
                        var varExprAst = expr as VariableExpressionAst;
                        if (varExprAst != null)
                            var varPath = varExprAst.VariablePath;
                            if (varPath.IsVariable && varPath.IsAnyLocal())
                                var specialIndex = 0;
                                while (specialIndex < (int)AutomaticVariable.NumberOfAutomaticVariables)
                                    if (varPath.UnqualifiedPath.Equals(SpecialVariables.AutomaticVariables[specialIndex], StringComparison.OrdinalIgnoreCase))
                                        var expectedType = SpecialVariables.AutomaticVariableTypes[specialIndex];
                                        if (expectedType != lastConvertType)
                                                nameof(ParserStrings.AssignmentStatementToAutomaticNotSupported),
                                                ParserStrings.AssignmentStatementToAutomaticNotSupported,
                                                varPath.UnqualifiedPath,
                                                expectedType);
                                    specialIndex += 1;
            if (errorAst != null)
                reportError(errorAst);
        private void CheckArrayLiteralAssignment(ArrayLiteralAst ast, Action<Ast> reportError)
            foreach (var element in ast.Elements)
                CheckAssignmentTarget(element, true, reportError);
            // Make sure LHS is something that can be assigned to.
            CheckAssignmentTarget(assignmentStatementAst.Left, assignmentStatementAst.Operator == TokenKind.Equals,
                ast => _parser.ReportError(ast.Extent,
                    nameof(ParserStrings.InvalidLeftHandSide),
                    ParserStrings.InvalidLeftHandSide));
        public override AstVisitAction VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
            if (binaryExpressionAst.Operator == TokenKind.AndAnd
                || binaryExpressionAst.Operator == TokenKind.OrOr)
                _parser.ReportError(binaryExpressionAst.ErrorPosition,
                    nameof(ParserStrings.InvalidEndOfLine),
                    ParserStrings.InvalidEndOfLine,
                    binaryExpressionAst.Operator.Text());
        public override AstVisitAction VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
                    CheckAssignmentTarget(unaryExpressionAst.Child, false,
                            nameof(ParserStrings.OperatorRequiresVariableOrProperty),
                            ParserStrings.OperatorRequiresVariableOrProperty,
                            unaryExpressionAst.TokenKind.Text()));
        public override AstVisitAction VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
            if (convertExpressionAst.Type.TypeName.FullName.Equals(LanguagePrimitives.OrderedAttribute, StringComparison.OrdinalIgnoreCase))
                if (convertExpressionAst.Child is not HashtableAst)
                    // We allow the ordered attribute only on hashliteral node.
                    // This check covers the following scenario
                    //   $a = [ordered]10
                    _parser.ReportError(convertExpressionAst.Extent,
                        convertExpressionAst.Type.TypeName.FullName);
                // Currently, the type name '[ordered]' is handled specially in PowerShell.
                // When used in a conversion expression, it's only allowed on a hashliteral node, and it's
                // always interpreted as an initializer for a case-insensitive
                // 'System.Collections.Specialized.OrderedDictionary' by the compiler.
                // So, we can return early from here.
            if (typeof(PSReference) == convertExpressionAst.Type.TypeName.GetReflectionType())
                // Check for [ref][ref]
                ExpressionAst child = convertExpressionAst.Child;
                bool multipleRefs = false;
                    var childAttrExpr = child as AttributedExpressionAst;
                    if (childAttrExpr != null)
                        var childConvert = childAttrExpr as ConvertExpressionAst;
                        if (childConvert != null && typeof(PSReference) == childConvert.Type.TypeName.GetReflectionType())
                            multipleRefs = true;
                            _parser.ReportError(childConvert.Type.Extent,
                                                nameof(ParserStrings.ReferenceNeedsToBeByItselfInTypeSequence),
                                                ParserStrings.ReferenceNeedsToBeByItselfInTypeSequence);
                        child = childAttrExpr.Child;
                // Check for [int][ref], but don't add an extra error for [ref][ref].
                var parent = convertExpressionAst.Parent as AttributedExpressionAst;
                    var parentConvert = parent as ConvertExpressionAst;
                    if (parentConvert != null && !multipleRefs)
                        if (typeof(PSReference) == parentConvert.Type.TypeName.GetReflectionType())
                        // Don't complain if on the lhs of an assign, there is a different error message, and
                        // that is checked as part of assignment.
                        var ast = parent.Parent;
                        bool skipError = false;
                        while (ast != null)
                            var statementAst = ast as AssignmentStatementAst;
                            if (statementAst != null)
                                skipError = statementAst.Left.Find(ast1 => ast1 == convertExpressionAst, searchNestedScriptBlocks: true) != null;
                            if (ast is CommandExpressionAst)
                        if (!skipError)
                            _parser.ReportError(convertExpressionAst.Type.Extent,
                                                nameof(ParserStrings.ReferenceNeedsToBeLastTypeInTypeConversion),
                                                ParserStrings.ReferenceNeedsToBeLastTypeInTypeConversion);
                    parent = parent.Child as AttributedExpressionAst;
            // Converting to Type is suspicious
            if (typeof(Type) == convertExpressionAst.Type.TypeName.GetReflectionType())
                MarkAstParentsAsSuspicious(convertExpressionAst);
        public override AstVisitAction VisitUsingExpression(UsingExpressionAst usingExpressionAst)
            // The parser will parse anything that could start with a variable when
            // creating a UsingExpressionAst, but we will only support "simple"
            // property and array references with no side effects.
            var exprAst = usingExpressionAst.SubExpression;
            var badExpr = CheckUsingExpression(exprAst);
            if (badExpr != null)
                _parser.ReportError(badExpr.Extent,
                    nameof(ParserStrings.InvalidUsingExpression),
                    ParserStrings.InvalidUsingExpression);
        private static ExpressionAst CheckUsingExpression(ExpressionAst exprAst)
            if (exprAst is VariableExpressionAst)
            var memberExpr = exprAst as MemberExpressionAst;
            if (memberExpr != null
                && memberExpr is not InvokeMemberExpressionAst
                && memberExpr.Member is StringConstantExpressionAst)
                return CheckUsingExpression(memberExpr.Expression);
            var indexExpr = exprAst as IndexExpressionAst;
            if (indexExpr != null)
                if (!IsValidAttributeArgument(indexExpr.Index, s_isConstantAttributeArgVisitor))
                    return indexExpr.Index;
                return CheckUsingExpression(indexExpr.Target);
        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
            if (variableExpressionAst.Splatted
                && variableExpressionAst.Parent is not CommandAst
                && variableExpressionAst.Parent is not UsingExpressionAst)
                if (variableExpressionAst.Parent is ArrayLiteralAst && variableExpressionAst.Parent.Parent is CommandAst)
                    _parser.ReportError(variableExpressionAst.Extent,
                        nameof(ParserStrings.SplattingNotPermittedInArgumentList),
                        ParserStrings.SplattingNotPermittedInArgumentList,
                        variableExpressionAst.VariablePath.UserPath);
                        nameof(ParserStrings.SplattingNotPermitted),
                        ParserStrings.SplattingNotPermitted,
            if (variableExpressionAst.VariablePath.IsVariable)
                if (variableExpressionAst.TupleIndex == VariableAnalysis.ForceDynamic
                    && !variableExpressionAst.Assigned
                    && !variableExpressionAst.VariablePath.IsGlobal
                    && !variableExpressionAst.VariablePath.IsScript
                    && !variableExpressionAst.IsConstantVariable()
                    && !SpecialVariables.IsImplicitVariableAccessibleInClassMethod(variableExpressionAst.VariablePath))
                        nameof(ParserStrings.VariableNotLocal),
                        ParserStrings.VariableNotLocal);
            if (variableExpressionAst.VariablePath.UserPath.Equals(SpecialVariables.This, StringComparison.OrdinalIgnoreCase))
                if (AnalyzingStaticMember())
                        nameof(ParserStrings.NonStaticMemberAccessInStaticMember),
                        ParserStrings.NonStaticMemberAccessInStaticMember,
        public override AstVisitAction VisitHashtable(HashtableAst hashtableAst)
            HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in hashtableAst.KeyValuePairs)
                var keyStrAst = entry.Item1 as ConstantExpressionAst;
                if (keyStrAst != null)
                    var keyStr = keyStrAst.Value.ToString();
                    if (!keys.Add(keyStr))
                        if (hashtableAst.IsSchemaElement)
                            errorId = nameof(ParserStrings.DuplicatePropertyInInstanceDefinition);
                            errorMsg = ParserStrings.DuplicatePropertyInInstanceDefinition;
                            errorId = nameof(ParserStrings.DuplicateKeyInHashLiteral);
                            errorMsg = ParserStrings.DuplicateKeyInHashLiteral;
                        _parser.ReportError(entry.Item1.Extent, errorId, errorMsg, keyStr);
        public override AstVisitAction VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
            // The attribute (and not the entire expression) is used for the error extent.
            var errorAst = attributedExpressionAst.Attribute;
            while (attributedExpressionAst != null)
                if (attributedExpressionAst.Child is VariableExpressionAst)
                attributedExpressionAst = attributedExpressionAst.Child as AttributedExpressionAst;
            _parser.ReportError(errorAst.Extent,
                errorAst.TypeName.FullName);
        public override AstVisitAction VisitBlockStatement(BlockStatementAst blockStatementAst)
            if (blockStatementAst.IsInWorkflow())
            _parser.ReportError(blockStatementAst.Kind.Extent,
                nameof(ParserStrings.UnexpectedKeyword),
                ParserStrings.UnexpectedKeyword,
                blockStatementAst.Kind.Text);
        public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
            CheckMemberAccess(memberExpressionAst);
        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst memberExpressionAst)
        private static void CheckMemberAccess(MemberExpressionAst ast)
            // If the member access is not constant, it may be considered suspicious
            if (ast.Member is not ConstantExpressionAst)
                MarkAstParentsAsSuspicious(ast);
            TypeExpressionAst typeExpression = ast.Expression as TypeExpressionAst;
            // If this is static access on a variable, it may be suspicious
            if (ast.Static && (typeExpression == null))
        // Mark all of the parents of an AST as suspicious
        private static void MarkAstParentsAsSuspicious(Ast ast)
            Ast targetAst = ast;
            var parent = ast;
                targetAst = parent;
                targetAst.HasSuspiciousContent = true;
        public override AstVisitAction VisitScriptBlock(ScriptBlockAst scriptBlockAst)
            _scopeStack.Push(scriptBlockAst);
            if (scriptBlockAst.Parent == null
                || scriptBlockAst.Parent is ScriptBlockExpressionAst
                || scriptBlockAst.Parent.Parent is not FunctionMemberAst)
                _memberScopeStack.Push(null);
            ConfigurationDefinitionAst configAst = Ast.GetAncestorAst<ConfigurationDefinitionAst>(scriptBlockExpressionAst);
            // Check within a configuration statement, if there is undefined DSC resources (DynamicKeyword) was used
            // for example,
            //      Configuration TestConfig
            //          SomeDSCResource bla
            //          {
            //              Property = "value"
            //          }
            // SomeDSCResource is not default DSC Resource, in this case, a parse error should be generated to
            // indicate that SomeDSCResource is not defined. Parser generates a command call (CommandAst) to function
            // "SomeDSCResource" followed by a ScriptBlockExpressionAst. Following code check if there are patterns
            // that one ScriptBlockExpressionAst follows a CommandAst, and report Parser error(s) if true.
                var ast = scriptBlockExpressionAst.Parent; // Traverse all ancestor ast to find NamedBlockAst
                PipelineAst statementAst = null; // The nearest ancestor PipelineAst of the 'scriptBlockExpressionAst'
                int ancestorNodeLevel = 0; // The nearest ancestor PipelineAst should be two level above the 'scriptBlockExpressionAst'
                while ((ast != null) && (ancestorNodeLevel <= 2))
                    // Find nearest ancestor NamedBlockAst
                    var namedBlockedAst = ast as NamedBlockAst;
                    if ((namedBlockedAst != null) && (statementAst != null) && (ancestorNodeLevel == 2))
                        int index = namedBlockedAst.Statements.IndexOf(statementAst);
                            // Check if previous Statement is CommandAst
                            var pipelineAst = namedBlockedAst.Statements[index - 1] as PipelineAst;
                            if (pipelineAst != null && pipelineAst.PipelineElements.Count == 1)
                                var commandAst = pipelineAst.PipelineElements[0] as CommandAst;
                                if (commandAst != null &&
                                    commandAst.CommandElements.Count <= 2 &&
                                    commandAst.DefiningKeyword == null)
                                    // Here indicates a CommandAst followed by a ScriptBlockExpression,
                                    // which is invalid if the DSC resource is not defined
                                    var commandNameAst = commandAst.CommandElements[0] as StringConstantExpressionAst;
                                    if (commandNameAst != null)
                                        _parser.ReportError(commandNameAst.Extent,
                                            nameof(ParserStrings.ResourceNotDefined),
                                            ParserStrings.ResourceNotDefined,
                                            commandNameAst.Extent.Text);
                    statementAst = ast as PipelineAst;
                    ancestorNodeLevel++;
        public override AstVisitAction VisitUsingStatement(UsingStatementAst usingStatementAst)
            UsingStatementKind kind = usingStatementAst.UsingStatementKind;
            bool usingKindSupported = kind is UsingStatementKind.Namespace or UsingStatementKind.Assembly or UsingStatementKind.Module;
            if (!usingKindSupported || usingStatementAst.Alias != null)
                    usingStatementAst.Extent,
                    nameof(ParserStrings.UsingStatementNotSupported),
                    ParserStrings.UsingStatementNotSupported);
            if (kind is UsingStatementKind.Namespace)
                Regex nsPattern = NamespacePattern();
                if (!nsPattern.IsMatch(usingStatementAst.Name.Value))
                        usingStatementAst.Name.Extent,
                        nameof(ParserStrings.InvalidNamespaceValue),
                        ParserStrings.InvalidNamespaceValue);
        /// This regular expression is for validating if a namespace string is valid.
        /// In C#, a legit namespace is defined as `identifier ('.' identifier)*` [see https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/namespaces#143-namespace-declarations].
        /// And `identifier` is defined in https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names#naming-rules, summarized below:
        ///   - Identifiers must start with a letter or underscore (_).
        ///   - Identifiers can contain
        ///     * Unicode letter characters (categories: Lu, Ll, Lt, Lm, Lo or Nl);
        ///     * decimal digit characters (category: Nd);
        ///     * Unicode connecting characters (category: Pc);
        ///     * Unicode combining characters (categories: Mn, Mc);
        ///     * Unicode formatting characters (category: Cf).
        /// For details about how Unicode categories are represented in regular expression, see the "Unicode Categories" section in the following article:
        ///   - https://www.regular-expressions.info/unicode.html
        [GeneratedRegex(@"^[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Nd}\p{Pc}\p{Mn}\p{Mc}\p{Cf}_]*(?:\.[\p{L}\p{Nl}_][\p{L}\p{Nl}\p{Nd}\p{Pc}\p{Mn}\p{Mc}\p{Cf}_]*)*$")]
        private static partial Regex NamespacePattern();
        public override AstVisitAction VisitConfigurationDefinition(ConfigurationDefinitionAst configurationDefinitionAst)
            // Check if the ScriptBlockAst contains NamedBlockAst other than the End block
            ScriptBlockAst configBody = configurationDefinitionAst.Body.ScriptBlock;
            if (configBody.BeginBlock != null || configBody.ProcessBlock != null || configBody.DynamicParamBlock != null)
                var unsupportedNamedBlocks = new NamedBlockAst[] { configBody.BeginBlock, configBody.ProcessBlock, configBody.DynamicParamBlock };
                foreach (NamedBlockAst namedBlock in unsupportedNamedBlocks)
                    if (namedBlock != null)
                        _parser.ReportError(namedBlock.OpenCurlyExtent,
                            nameof(ParserStrings.UnsupportedNamedBlockInConfiguration),
                            ParserStrings.UnsupportedNamedBlockInConfiguration);
                // ToRemove: No need to continue the parsing if there is no EndBlock, check if (configBody.EndBlock == null)
        public override AstVisitAction VisitDynamicKeywordStatement(DynamicKeywordStatementAst dynamicKeywordStatementAst)
            if (dynamicKeywordStatementAst.Keyword.SemanticCheck != null)
                    ParseError[] errors = dynamicKeywordStatementAst.Keyword.SemanticCheck(dynamicKeywordStatementAst);
                        errors.ToList().ForEach(e => _parser.ReportError(e));
                    _parser.ReportError(dynamicKeywordStatementAst.Extent,
                        nameof(ParserStrings.DynamicKeywordSemanticCheckException),
                        ParserStrings.DynamicKeywordSemanticCheckException,
                        dynamicKeywordStatementAst.Keyword.ResourceName,
            DynamicKeyword keyword = dynamicKeywordStatementAst.Keyword;
            HashtableAst hashtable = dynamicKeywordStatementAst.BodyExpression as HashtableAst;
                // If it's a hash table, validate that only valid members have been specified.
                foreach (var keyValueTuple in hashtable.KeyValuePairs)
                    if (propName == null)
                        _parser.ReportError(keyValueTuple.Item1.Extent,
                            nameof(ParserStrings.ConfigurationInvalidPropertyName),
                            ParserStrings.ConfigurationInvalidPropertyName,
                            dynamicKeywordStatementAst.FunctionName.Extent,
                            keyValueTuple.Item1.Extent);
                    else if (!keyword.Properties.ContainsKey(propName.Value))
                        IOrderedEnumerable<string> tableKeys = keyword.Properties.Keys
                            .Order(StringComparer.OrdinalIgnoreCase);
                        _parser.ReportError(propName.Extent,
                            nameof(ParserStrings.InvalidInstanceProperty),
                            ParserStrings.InvalidInstanceProperty,
                            propName.Value,
                            string.Join("', '", tableKeys));
            // Check compatibility between DSC Resource and Configuration
            ConfigurationDefinitionAst configAst = Ast.GetAncestorAst<ConfigurationDefinitionAst>(dynamicKeywordStatementAst);
                StringConstantExpressionAst nameAst = dynamicKeywordStatementAst.CommandElements[0] as StringConstantExpressionAst;
                Diagnostics.Assert(nameAst != null, "nameAst should never be null");
                var extentText = nameAst.Extent.Text.Trim();
                var extentTextIsASystemResourceName = (dscSubsystem != null) ? dscSubsystem.IsSystemResourceName(extentText) : DscClassCache.SystemResourceNames.Contains(extentText);
                if (!extentTextIsASystemResourceName)
                    if (configAst.ConfigurationType == ConfigurationType.Meta && !dynamicKeywordStatementAst.Keyword.IsMetaDSCResource())
                        _parser.ReportError(nameAst.Extent,
                            nameof(ParserStrings.RegularResourceUsedInMetaConfig),
                            ParserStrings.RegularResourceUsedInMetaConfig,
                            nameAst.Extent.Text);
                    else if (configAst.ConfigurationType != ConfigurationType.Meta && dynamicKeywordStatementAst.Keyword.IsMetaDSCResource())
                            nameof(ParserStrings.MetaConfigurationUsedInRegularConfig),
                            ParserStrings.MetaConfigurationUsedInRegularConfig,
        public override AstVisitAction VisitPropertyMember(PropertyMemberAst propertyMemberAst)
            if (propertyMemberAst.PropertyType != null)
                // Type must be resolved, but if it's not, the error was reported by the symbol resolver.
                var type = propertyMemberAst.PropertyType.TypeName.GetReflectionType();
                if (type != null && (type == typeof(void) || type.IsGenericTypeDefinition))
                    _parser.ReportError(propertyMemberAst.PropertyType.Extent,
                        nameof(ParserStrings.TypeNotAllowedForProperty),
                        ParserStrings.TypeNotAllowedForProperty,
                        propertyMemberAst.PropertyType.TypeName.FullName);
            _memberScopeStack.Push(propertyMemberAst);
        public void PostVisit(Ast ast)
            var scriptBlockAst = ast as ScriptBlockAst;
            if (scriptBlockAst != null)
                    _memberScopeStack.Pop();
                _scopeStack.Pop();
                scriptBlockAst.PostParseChecksPerformed = true;
                // at this moment, we could use different parser for the initial syntax check
                // and this _parser for semantic check.
                // that's why '|=' instead of just '='.
                scriptBlockAst.HadErrors |= _parser.ErrorList.Count > 0;
            else if (ast is MemberAst)
    internal static class DscResourceChecker
        /// Check if it is a qualified DSC resource type.
        /// <param name="dscResourceAttributeAst"></param>
        internal static void CheckType(Parser parser, TypeDefinitionAst typeDefinitionAst, AttributeAst dscResourceAttributeAst)
            bool hasSet = false;
            bool hasTest = false;
            bool hasGet = false;
            bool hasDefaultCtor = false;
            bool hasNonDefaultCtor = false;
            bool hasKey = false;
            Diagnostics.Assert(dscResourceAttributeAst != null, "CheckType called only for DSC resources. dscResourceAttributeAst must be non-null.");
                    CheckSet(functionMemberAst, ref hasSet);
                    CheckGet(parser, functionMemberAst, ref hasGet);
                    CheckTest(functionMemberAst, ref hasTest);
                    if (functionMemberAst.IsConstructor && !functionMemberAst.IsStatic)
                        if (functionMemberAst.Parameters.Count == 0)
                            hasDefaultCtor = true;
                            hasNonDefaultCtor = true;
                    var propertyMemberAst = (PropertyMemberAst)member;
                    CheckKey(parser, propertyMemberAst, ref hasKey);
            if (typeDefinitionAst.BaseTypes != null && (!hasSet || !hasGet || !hasTest || !hasKey))
                LookupRequiredMembers(parser, typeDefinitionAst, ref hasSet, ref hasGet, ref hasTest, ref hasKey);
            var name = typeDefinitionAst.Name;
            if (!hasSet)
                parser.ReportError(dscResourceAttributeAst.Extent,
                    nameof(ParserStrings.DscResourceMissingSetMethod),
                    ParserStrings.DscResourceMissingSetMethod,
            if (!hasGet)
                    nameof(ParserStrings.DscResourceMissingGetMethod),
                    ParserStrings.DscResourceMissingGetMethod,
            if (!hasTest)
                    nameof(ParserStrings.DscResourceMissingTestMethod),
                    ParserStrings.DscResourceMissingTestMethod,
            if (!hasDefaultCtor && hasNonDefaultCtor)
                    nameof(ParserStrings.DscResourceMissingDefaultConstructor),
            if (!hasKey)
                    nameof(ParserStrings.DscResourceMissingKeyProperty),
                    ParserStrings.DscResourceMissingKeyProperty,
        /// Look up all the way up until find all the required members.
        /// <param name="typeDefinitionAst">The type definition ast of the DSC resource type.</param>
        /// <param name="hasSet">Flag to indicate if the class contains Set method.</param>
        /// <param name="hasGet">Flag to indicate if the class contains Get method.</param>
        /// <param name="hasTest">Flag to indicate if the class contains Test method.</param>
        /// <param name="hasKey">Flag to indicate if the class contains Key property.</param>
        private static void LookupRequiredMembers(Parser parser, TypeDefinitionAst typeDefinitionAst, ref bool hasSet, ref bool hasGet, ref bool hasTest, ref bool hasKey)
            if (hasSet && hasGet && hasTest && hasKey)
            foreach (var baseType in typeDefinitionAst.BaseTypes)
                if (!(baseType.TypeName is TypeName baseTypeName))
                TypeDefinitionAst baseTypeDefinitionAst = baseTypeName._typeDefinitionAst;
                if (baseTypeDefinitionAst == null || !baseTypeDefinitionAst.IsClass)
                foreach (var member in baseTypeDefinitionAst.Members)
                if (baseTypeDefinitionAst.BaseTypes != null && (!hasSet || !hasGet || !hasTest || !hasKey))
                    LookupRequiredMembers(parser, baseTypeDefinitionAst, ref hasSet, ref hasGet, ref hasTest, ref hasKey);
        /// Check if it is a Get method with correct return type and signature.
        /// <param name="functionMemberAst">The function member AST.</param>
        /// <param name="hasGet">True if it is a Get method with qualified return type and signature; otherwise, false.</param>
        private static void CheckGet(Parser parser, FunctionMemberAst functionMemberAst, ref bool hasGet)
            if (hasGet)
            if (functionMemberAst.Name.Equals("Get", StringComparison.OrdinalIgnoreCase) &&
                functionMemberAst.Parameters.Count == 0)
                if (functionMemberAst.ReturnType != null)
                    // Return type is of the class we're defined in
                    // it must return the class type, or array of the class type.
                    var arrayTypeName = functionMemberAst.ReturnType.TypeName as ArrayTypeName;
                    var typeName =
                        (arrayTypeName != null ? arrayTypeName.ElementType : functionMemberAst.ReturnType.TypeName) as
                            TypeName;
                    if (typeName == null || typeName._typeDefinitionAst != functionMemberAst.Parent)
                        parser.ReportError(functionMemberAst.Extent,
                            nameof(ParserStrings.DscResourceInvalidGetMethod),
                            ParserStrings.DscResourceInvalidGetMethod,
                            ((TypeDefinitionAst)functionMemberAst.Parent).Name);
                // Set hasGet to true to stop look up; it may have invalid get
                hasGet = true;
        /// Check if it is a Test method with correct return type and signature.
        /// <param name="hasTest">True if it is a Test method with qualified return type and signature; otherwise, false.</param>
        private static void CheckTest(FunctionMemberAst functionMemberAst, ref bool hasTest)
            if (hasTest)
            hasTest = (functionMemberAst.Name.Equals("Test", StringComparison.OrdinalIgnoreCase) &&
                    functionMemberAst.Parameters.Count == 0 &&
                    functionMemberAst.ReturnType != null &&
                    functionMemberAst.ReturnType.TypeName.GetReflectionType() == typeof(bool));
        /// Check if it is a Set method with correct return type and signature.
        /// <param name="hasSet">True if it is a Set method with qualified return type and signature; otherwise, false.</param>
        private static void CheckSet(FunctionMemberAst functionMemberAst, ref bool hasSet)
            if (hasSet)
            hasSet = (functionMemberAst.Name.Equals("Set", StringComparison.OrdinalIgnoreCase) &&
                    functionMemberAst.IsReturnTypeVoid());
        /// True if it is a key property.
        /// <param name="propertyMemberAst">The property member AST.</param>
        /// <param name="hasKey">True if it is a key property; otherwise, false.</param>
        private static void CheckKey(Parser parser, PropertyMemberAst propertyMemberAst, ref bool hasKey)
            foreach (var attr in propertyMemberAst.Attributes)
                if (attr.TypeName.GetReflectionAttributeType() == typeof(DscPropertyAttribute))
                        if (na.ArgumentName.Equals("Key", StringComparison.OrdinalIgnoreCase))
                            object attrArgValue;
                            if (IsConstantValueVisitor.IsConstant(na.Argument, out attrArgValue, forAttribute: true, forRequires: false)
                                && LanguagePrimitives.IsTrue(attrArgValue))
                                hasKey = true;
                                bool keyPropertyTypeAllowed = false;
                                var propertyType = propertyMemberAst.PropertyType;
                                if (propertyType != null)
                                    TypeName typeName = propertyType.TypeName as TypeName;
                                        var type = typeName.GetReflectionType();
                                            keyPropertyTypeAllowed = type == typeof(string) || type.IsInteger();
                                            var typeDefinitionAst = typeName._typeDefinitionAst;
                                                keyPropertyTypeAllowed = typeDefinitionAst.IsEnum;
                                if (!keyPropertyTypeAllowed)
                                    parser.ReportError(propertyMemberAst.Extent,
                                        nameof(ParserStrings.DscResourceInvalidKeyProperty),
                                        ParserStrings.DscResourceInvalidKeyProperty);
    internal class RestrictedLanguageChecker : AstVisitor
        private readonly IEnumerable<string> _allowedCommands;
        private readonly IEnumerable<string> _allowedVariables;
        private readonly bool _allVariablesAreAllowed;
        private readonly bool _allowEnvironmentVariables;
        internal RestrictedLanguageChecker(Parser parser, IEnumerable<string> allowedCommands, IEnumerable<string> allowedVariables, bool allowEnvironmentVariables)
            _allowedCommands = allowedCommands;
            if (allowedVariables != null)
                // A single '*' allows any variable to be used. The use of a single '*' aligns with the
                // way SessionState.Applications and SessionState.Scripts lists work.
                var allowedVariablesList = allowedVariables as IList<string> ?? allowedVariables.ToList();
                if (allowedVariablesList.Count == 1 && allowedVariablesList.Contains("*"))
                    _allVariablesAreAllowed = true;
                    // Allowed variables are the union of the default variables plus any the caller has passed in.
                    _allowedVariables = new HashSet<string>(s_defaultAllowedVariables).Union(allowedVariablesList);
                _allowedVariables = s_defaultAllowedVariables;
            _allowEnvironmentVariables = allowEnvironmentVariables;
        private bool FoundError
        internal static void CheckDataStatementLanguageModeAtRuntime(DataStatementAst dataStatementAst, ExecutionContext executionContext)
            // If we get here, we have already determined the data statement invokes commands, so
            // we only need to check the language mode.
            if (executionContext.LanguageMode == PSLanguageMode.ConstrainedLanguage)
                    parser.ReportError(dataStatementAst.CommandsAllowed[0].Extent,
                        nameof(ParserStrings.DataSectionAllowedCommandDisallowed),
                        ParserStrings.DataSectionAllowedCommandDisallowed);
                    title: ParserStrings.WDACParserDSSupportedCommandLogTitle,
                    message: ParserStrings.WDACParserDSSupportedCommandLogMessage,
                    fqid: "SupportedCommandInDataSectionNotSupported",
        internal static void CheckDataStatementAstAtRuntime(DataStatementAst dataStatementAst, string[] allowedCommands)
            var rlc = new RestrictedLanguageChecker(parser, allowedCommands, null, false);
            dataStatementAst.Body.InternalVisit(rlc);
        internal static void EnsureUtilityModuleLoaded(ExecutionContext context)
            Utils.EnsureModuleLoaded("Microsoft.PowerShell.Utility", context);
        private void ReportError(Ast ast, string errorId, string errorMsg, params object[] args)
            ReportError(ast.Extent, errorId, errorMsg, args);
            FoundError = true;
        private void ReportError(IScriptExtent extent, string errorId, string errorMsg, params object[] args)
            _parser.ReportError(extent, errorId, errorMsg, args);
            ReportError(scriptBlockAst,
                nameof(ParserStrings.ScriptBlockNotSupportedInDataSection),
                ParserStrings.ScriptBlockNotSupportedInDataSection);
            ReportError(paramBlockAst,
                nameof(ParserStrings.ParameterDeclarationNotSupportedInDataSection),
                ParserStrings.ParameterDeclarationNotSupportedInDataSection);
        public override AstVisitAction VisitNamedBlock(NamedBlockAst namedBlockAst)
            Diagnostics.Assert(_allowEnvironmentVariables || FoundError, "VisitScriptBlock should have already reported an error");
        private void CheckTypeName(Ast ast, ITypeName typename)
            Type type = typename.GetReflectionType();
            // Only allow simple types of arrays of simple types as defined by System.Typecode
            // The permitted types are: Empty, Object, DBNull, Boolean, Char, SByte, Byte,
            // Int16, UInt16, Int32, UInt32, Int64, UInt64, Single, Double, Decimal, DateTime, String
            // We reject anything with a typecode or element typecode of object...
            // If we couldn't resolve the type, then it's definitely an error.
            if (type == null || ((type.IsArray ? type.GetElementType() : type).GetTypeCode() == TypeCode.Object))
                ReportError(ast,
                    nameof(ParserStrings.TypeNotAllowedInDataSection),
                    ParserStrings.TypeNotAllowedInDataSection,
                    typename.FullName);
            CheckTypeName(typeConstraintAst, typeConstraintAst.TypeName);
            Diagnostics.Assert(FoundError, "an error should have been reported elsewhere, making this redunant");
            ReportError(attributeAst,
                nameof(ParserStrings.AttributeNotSupportedInDataSection),
                ParserStrings.AttributeNotSupportedInDataSection);
            Diagnostics.Assert(FoundError, "VisitParamBlock or VisitFunctionDeclaration should have already reported an error");
            CheckTypeName(typeExpressionAst, typeExpressionAst.TypeName);
            ReportError(functionDefinitionAst,
                nameof(ParserStrings.FunctionDeclarationNotSupportedInDataSection),
                ParserStrings.FunctionDeclarationNotSupportedInDataSection);
        public override AstVisitAction VisitStatementBlock(StatementBlockAst statementBlockAst)
            // Accepted if no traps and each statement is accepted.
        public override AstVisitAction VisitIfStatement(IfStatementAst ifStmtAst)
            // If statements are accepted if their conditions and bodies are accepted.
        public override AstVisitAction VisitTrap(TrapStatementAst trapStatementAst)
            ReportError(trapStatementAst,
                nameof(ParserStrings.TrapStatementNotSupportedInDataSection),
                ParserStrings.TrapStatementNotSupportedInDataSection);
            ReportError(switchStatementAst,
                nameof(ParserStrings.SwitchStatementNotSupportedInDataSection),
                ParserStrings.SwitchStatementNotSupportedInDataSection);
            ReportError(dataStatementAst,
                nameof(ParserStrings.DataSectionStatementNotSupportedInDataSection),
                ParserStrings.DataSectionStatementNotSupportedInDataSection);
            ReportError(forEachStatementAst,
                nameof(ParserStrings.ForeachStatementNotSupportedInDataSection),
                ParserStrings.ForeachStatementNotSupportedInDataSection);
        public override AstVisitAction VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
            ReportError(doWhileStatementAst,
                nameof(ParserStrings.DoWhileStatementNotSupportedInDataSection),
                ParserStrings.DoWhileStatementNotSupportedInDataSection);
        public override AstVisitAction VisitForStatement(ForStatementAst forStatementAst)
            ReportError(forStatementAst,
                nameof(ParserStrings.ForWhileStatementNotSupportedInDataSection),
                ParserStrings.ForWhileStatementNotSupportedInDataSection);
        public override AstVisitAction VisitWhileStatement(WhileStatementAst whileStatementAst)
            ReportError(whileStatementAst,
        public override AstVisitAction VisitCatchClause(CatchClauseAst catchClauseAst)
            Diagnostics.Assert(FoundError, "VisitTryStatement should have reported an error");
            ReportError(tryStatementAst,
                nameof(ParserStrings.TryStatementNotSupportedInDataSection),
                ParserStrings.TryStatementNotSupportedInDataSection);
            ReportError(breakStatementAst,
                nameof(ParserStrings.FlowControlStatementNotSupportedInDataSection),
                ParserStrings.FlowControlStatementNotSupportedInDataSection);
            ReportError(continueStatementAst,
            ReportError(returnStatementAst,
        public override AstVisitAction VisitExitStatement(ExitStatementAst exitStatementAst)
            ReportError(exitStatementAst,
        public override AstVisitAction VisitThrowStatement(ThrowStatementAst throwStatementAst)
            ReportError(throwStatementAst,
        public override AstVisitAction VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
            ReportError(doUntilStatementAst,
            // Assignments are never allowed.
            ReportError(assignmentStatementAst,
                nameof(ParserStrings.AssignmentStatementNotSupportedInDataSection),
                ParserStrings.AssignmentStatementNotSupportedInDataSection);
        public override AstVisitAction VisitPipeline(PipelineAst pipelineAst)
            // A pipeline is accepted if every command in the pipeline is accepted.
            // Commands are allowed if arguments are allowed, no redirection, no dotting
            // If _allowedCommands is null, any command is allowed, otherwise
            // we must check the command name.
                ReportError(commandAst,
                    nameof(ParserStrings.DotSourcingNotSupportedInDataSection),
                    ParserStrings.DotSourcingNotSupportedInDataSection);
            if (_allowedCommands == null)
                if (commandAst.InvocationOperator == TokenKind.Ampersand)
                        nameof(ParserStrings.OperatorNotSupportedInDataSection),
                        ParserStrings.OperatorNotSupportedInDataSection,
                        TokenKind.Ampersand.Text());
                        nameof(ParserStrings.CmdletNotInAllowedListForDataSection),
                        ParserStrings.CmdletNotInAllowedListForDataSection,
                        commandAst.Extent.Text);
            if (_allowedCommands.Any(allowedCommand => allowedCommand.Equals(commandName, StringComparison.OrdinalIgnoreCase)))
        public override AstVisitAction VisitCommandExpression(CommandExpressionAst commandExpressionAst)
            // allowed if the child expression is allowed
        public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
            // allowed if there is no argument, or if the optional argument is allowed
        public override AstVisitAction VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
            ReportError(mergingRedirectionAst,
                nameof(ParserStrings.RedirectionNotSupportedInDataSection),
                ParserStrings.RedirectionNotSupportedInDataSection);
        public override AstVisitAction VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
            ReportError(fileRedirectionAst,
            // match and notmatch disallowed because they can set $matches
            // -join/-split disallowed because they can be used in tandem to build very large strings, like:
            //    -split (-split ( ('a','a') -join " a a a a a ") -join " a a a a a ")
            // -replace disallowed because it too can be used to build very large strings, like:
            //    ('x' -replace '.','xx') -replace '.','xx'
            // -as disallowed because it can invoke arbitrary code (similar to why some casts are disallowed,
            //    but -as is completely disallowed because in some cases, we don't know the type at parse time.
            // Format disallowed because of unbounded results, like ("{0," + 1MB + ":0}") -f 1
            // Range disallowed because it has iteration semantics.
            if (binaryExpressionAst.Operator.HasTrait(TokenFlags.DisallowedInRestrictedMode))
                ReportError(binaryExpressionAst.ErrorPosition,
            if (unaryExpressionAst.TokenKind.HasTrait(TokenFlags.DisallowedInRestrictedMode))
                ReportError(unaryExpressionAst,
                    unaryExpressionAst.TokenKind.Text());
            // Convert is allowed if the type and child is allowed.
        public override AstVisitAction VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
            // Constants are allowed.
        public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        public override AstVisitAction VisitSubExpression(SubExpressionAst subExpressionAst)
            // Sub expression is allowed if the statement block is allowed.
            // Using expression is allowed if the sub-expression is allowed.
        private static readonly HashSet<string> s_defaultAllowedVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                                                    {"PSCulture", "PSUICulture", "true", "false", "null"};
            VariablePath varPath = variableExpressionAst.VariablePath;
            // If the variable is in the list, or the _allVariablesAreAllowed field is set, continue processing
            if (_allVariablesAreAllowed || _allowedVariables.Contains(varPath.UserPath, StringComparer.OrdinalIgnoreCase))
            if (_allowEnvironmentVariables)
                // Allow access to environment when processing module manifests
                if (varPath.IsDriveQualified && varPath.DriveName.Equals("env", StringComparison.OrdinalIgnoreCase))
            string resourceArg;
            if (ReferenceEquals(_allowedVariables, s_defaultAllowedVariables))
                resourceArg = ParserStrings.DefaultAllowedVariablesInDataSection;
                StringBuilder argBuilder = new StringBuilder();
                foreach (string varName in _allowedVariables)
                        argBuilder.Append(", ");
                    argBuilder.Append('$');
                    argBuilder.Append(varName);
                resourceArg = argBuilder.ToString();
            ReportError(variableExpressionAst,
                nameof(ParserStrings.VariableReferenceNotSupportedInDataSection),
                resourceArg);
            ReportError(memberExpressionAst,
                nameof(ParserStrings.PropertyReferenceNotSupportedInDataSection),
                ParserStrings.PropertyReferenceNotSupportedInDataSection);
        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
            ReportError(methodCallAst,
                nameof(ParserStrings.MethodCallNotSupportedInDataSection),
                ParserStrings.MethodCallNotSupportedInDataSection);
        public override AstVisitAction VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
            // Safe (if children are allowed)
        public override AstVisitAction VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
            // Allowed if elements are allowed.
            // Hash literals are accepted if the keys and values are accepted.
            ReportError(scriptBlockExpressionAst,
        public override AstVisitAction VisitParenExpression(ParenExpressionAst parenExpressionAst)
            // allowed if the child pipeline is allowed.
        public override AstVisitAction VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
            // REVIEW: it should be OK to allow these, since the ast now would visit the nested expressions and catch the errors.
            // Not allowed since most variables are not allowed
            // ReportError(expandableStringExpressionAst, () => ParserStrings.ExpandableStringNotSupportedInDataSection);
        public override AstVisitAction VisitIndexExpression(IndexExpressionAst indexExpressionAst)
            // Array references are never allowed.  They could turn into function calls.
            ReportError(indexExpressionAst,
                nameof(ParserStrings.ArrayReferenceNotSupportedInDataSection),
                ParserStrings.ArrayReferenceNotSupportedInDataSection);
            // Attributes are not allowed, they may code in attribute constructors.
            ReportError(attributedExpressionAst,
            // Keyword blocks are not allowed
            ReportError(blockStatementAst,
                nameof(ParserStrings.ParallelAndSequenceBlockNotSupportedInDataSection),
                ParserStrings.ParallelAndSequenceBlockNotSupportedInDataSection);
        public override AstVisitAction VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
            Diagnostics.Assert(FoundError, "VisitAttributedExpression or VisitParameter or VisitParamBlock should have issued an error.");
