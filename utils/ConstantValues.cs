     * The IsGetPowerShellSafeValueVisitor class in SafeValues.cs used this class as the basis for implementation.
     * There is a number of similarities between these two classes, and changes (fixes) in this code
     * may need to be reflected in that class and vice versa
    internal class IsConstantValueVisitor : ICustomAstVisitor2
        public static bool IsConstant(Ast ast, out object constantValue, bool forAttribute = false, bool forRequires = false)
                if ((bool)ast.Accept(new IsConstantValueVisitor { CheckingAttributeArgument = forAttribute, CheckingRequiresArgument = forRequires }))
                    Ast parent = ast.Parent;
                        if (parent is DataStatementAst)
                    if (parent == null)
                        constantValue = ast.Accept(new ConstantValueVisitor { AttributeArgument = forAttribute, RequiresArgument = forRequires });
                // If we get an exception, ignore it and assume the expression isn't constant.
                // This can happen, e.g. if a cast is invalid:
                //     [int]"zed"
            constantValue = null;
        internal bool CheckingAttributeArgument { get; set; }
        internal bool CheckingClassAttributeArguments { get; set; }
        internal bool CheckingRequiresArgument { get; set; }
        public object VisitIndexExpression(IndexExpressionAst indexExpressionAst) { return false; }
        public object VisitUsingStatement(UsingStatementAst usingStatement) { return false; }
        public object VisitConfigurationDefinition(ConfigurationDefinitionAst configurationDefinitionAst) { return false; }
        public object VisitDynamicKeywordStatement(DynamicKeywordStatementAst dynamicKeywordAst) { return false; }
            if (statementBlockAst.Traps != null) 
            if (statementBlockAst.Statements.Count > 1)
        private static bool IsNullDivisor(ExpressionAst operand)
            if (!(operand is VariableExpressionAst varExpr))
            var parent = operand.Parent as BinaryExpressionAst;
            if (parent == null || parent.Right != operand)
            switch (parent.Operator)
                case TokenKind.DivideEquals:
                case TokenKind.RemainderEquals:
                    string name = varExpr.VariablePath.UnqualifiedPath;
                    return (name.Equals(SpecialVariables.False, StringComparison.OrdinalIgnoreCase) ||
                            name.Equals(SpecialVariables.Null, StringComparison.OrdinalIgnoreCase));
            return binaryExpressionAst.Operator.HasTrait(TokenFlags.CanConstantFold) &&
                (bool)binaryExpressionAst.Left.Accept(this) && (bool)binaryExpressionAst.Right.Accept(this)
                && !IsNullDivisor(binaryExpressionAst.Right);
            return unaryExpressionAst.TokenKind.HasTrait(TokenFlags.CanConstantFold) &&
                (bool)unaryExpressionAst.Child.Accept(this);
            var type = convertExpressionAst.Type.TypeName.GetReflectionType();
            if (!type.IsSafePrimitive())
                // Only do conversions to built-in types - other conversions might not
                // be safe to optimize.
        public object VisitUsingExpression(UsingExpressionAst usingExpressionAst)
            // $using:true should be constant - it's silly to write that, but not harmful.
            return usingExpressionAst.SubExpression.Accept(this);
            return variableExpressionAst.IsConstantVariable();
            // We defer trying to resolve a type expression as an attribute argument
            // until the script/function is first run, so it's OK if a type expression
            // as an attribute argument cannot be resolved yet.
            return CheckingAttributeArgument ||
                typeExpressionAst.TypeName.GetReflectionType() != null;
            if (!memberExpressionAst.Static || memberExpressionAst.Expression is not TypeExpressionAst)
            if (!(memberExpressionAst.Member is StringConstantExpressionAst member))
            var memberInfo = type.GetMember(member.Value, MemberTypes.Field,
            if (memberInfo.Length != 1)
            return (((FieldInfo)memberInfo[0]).Attributes & FieldAttributes.Literal) != 0;
            // An array literal is a constant when we're generating metadata, but when
            // we're generating code, we need to create new arrays or we'd have an aliasing problem.
            return (CheckingAttributeArgument || CheckingRequiresArgument) && arrayLiteralAst.Elements.All(e => (bool)e.Accept(this));
            return CheckingRequiresArgument &&
                   hashtableAst.KeyValuePairs.All(pair => (bool)pair.Item1.Accept(this) && (bool)pair.Item2.Accept(this));
            // A script block expression is a constant when we're generating metadata, but when
            // we're generating code, we need to create new script blocks so we can't use a constant.
            // Also - we have no way to describe a script block when generating .Net metadata, so
            // we must disallow script blocks as attribute arguments on/inside a class.
            return CheckingAttributeArgument && !CheckingClassAttributeArguments;
    internal class ConstantValueVisitor : ICustomAstVisitor2
        internal bool AttributeArgument { get; set; }
        internal bool RequiresArgument { get; set; }
        [Conditional("ASSERTIONS_TRACE")]
        private void CheckIsConstant(Ast ast, string msg)
                (bool)ast.Accept(new IsConstantValueVisitor { CheckingAttributeArgument = this.AttributeArgument, CheckingRequiresArgument = RequiresArgument }), msg);
        private static object CompileAndInvoke(Ast ast)
                var compiler = new Compiler { CompilingConstantExpression = true };
                return Expression.Lambda((Expression)ast.Accept(compiler)).Compile().DynamicInvoke();
        public object VisitErrorStatement(ErrorStatementAst errorStatementAst) { return AutomationNull.Value; }
        public object VisitErrorExpression(ErrorExpressionAst errorExpressionAst) { return AutomationNull.Value; }
        public object VisitScriptBlock(ScriptBlockAst scriptBlockAst) { return AutomationNull.Value; }
        public object VisitParamBlock(ParamBlockAst paramBlockAst) { return AutomationNull.Value; }
        public object VisitNamedBlock(NamedBlockAst namedBlockAst) { return AutomationNull.Value; }
        public object VisitTypeConstraint(TypeConstraintAst typeConstraintAst) { return AutomationNull.Value; }
        public object VisitAttribute(AttributeAst attributeAst) { return AutomationNull.Value; }
        public object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst) { return AutomationNull.Value; }
        public object VisitParameter(ParameterAst parameterAst) { return AutomationNull.Value; }
        public object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst) { return AutomationNull.Value; }
        public object VisitIfStatement(IfStatementAst ifStmtAst) { return AutomationNull.Value; }
        public object VisitTrap(TrapStatementAst trapStatementAst) { return AutomationNull.Value; }
        public object VisitSwitchStatement(SwitchStatementAst switchStatementAst) { return AutomationNull.Value; }
        public object VisitDataStatement(DataStatementAst dataStatementAst) { return AutomationNull.Value; }
        public object VisitForEachStatement(ForEachStatementAst forEachStatementAst) { return AutomationNull.Value; }
        public object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst) { return AutomationNull.Value; }
        public object VisitForStatement(ForStatementAst forStatementAst) { return AutomationNull.Value; }
        public object VisitWhileStatement(WhileStatementAst whileStatementAst) { return AutomationNull.Value; }
        public object VisitCatchClause(CatchClauseAst catchClauseAst) { return AutomationNull.Value; }
        public object VisitTryStatement(TryStatementAst tryStatementAst) { return AutomationNull.Value; }
        public object VisitBreakStatement(BreakStatementAst breakStatementAst) { return AutomationNull.Value; }
        public object VisitContinueStatement(ContinueStatementAst continueStatementAst) { return AutomationNull.Value; }
        public object VisitReturnStatement(ReturnStatementAst returnStatementAst) { return AutomationNull.Value; }
        public object VisitExitStatement(ExitStatementAst exitStatementAst) { return AutomationNull.Value; }
        public object VisitThrowStatement(ThrowStatementAst throwStatementAst) { return AutomationNull.Value; }
        public object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst) { return AutomationNull.Value; }
        public object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst) { return AutomationNull.Value; }
        public object VisitCommand(CommandAst commandAst) { return AutomationNull.Value; }
        public object VisitCommandExpression(CommandExpressionAst commandExpressionAst) { return AutomationNull.Value; }
        public object VisitCommandParameter(CommandParameterAst commandParameterAst) { return AutomationNull.Value; }
        public object VisitFileRedirection(FileRedirectionAst fileRedirectionAst) { return AutomationNull.Value; }
        public object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst) { return AutomationNull.Value; }
        public object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst) { return AutomationNull.Value; }
        public object VisitIndexExpression(IndexExpressionAst indexExpressionAst) { return AutomationNull.Value; }
        public object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst) { return AutomationNull.Value; }
        public object VisitBlockStatement(BlockStatementAst blockStatementAst) { return AutomationNull.Value; }
        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst) { return AutomationNull.Value; }
        public object VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst) { return AutomationNull.Value; }
        public object VisitPropertyMember(PropertyMemberAst propertyMemberAst) { return AutomationNull.Value; }
        public object VisitFunctionMember(FunctionMemberAst functionMemberAst) { return AutomationNull.Value; }
        public object VisitBaseCtorInvokeMemberExpression(BaseCtorInvokeMemberExpressionAst baseCtorInvokeMemberExpressionAst) { return AutomationNull.Value; }
        public object VisitUsingStatement(UsingStatementAst usingStatement) { return AutomationNull.Value; }
        public object VisitConfigurationDefinition(ConfigurationDefinitionAst configurationDefinitionAst) { return AutomationNull.Value; }
        public object VisitDynamicKeywordStatement(DynamicKeywordStatementAst dynamicKeywordAst) { return AutomationNull.Value; }
            CheckIsConstant(statementBlockAst, "Caller to verify ast is constant");
            return statementBlockAst.Statements[0].Accept(this);
            CheckIsConstant(pipelineAst, "Caller to verify ast is constant");
            return pipelineAst.GetPureExpression().Accept(this);
            CheckIsConstant(ternaryExpressionAst, "Caller to verify ast is constant");
            object condition = ternaryExpressionAst.Condition.Accept(this);
            return LanguagePrimitives.IsTrue(condition)
                ? ternaryExpressionAst.IfTrue.Accept(this)
                : ternaryExpressionAst.IfFalse.Accept(this);
            CheckIsConstant(binaryExpressionAst, "Caller to verify ast is constant");
            return CompileAndInvoke(binaryExpressionAst);
            CheckIsConstant(unaryExpressionAst, "Caller to verify ast is constant");
            return CompileAndInvoke(unaryExpressionAst);
            CheckIsConstant(convertExpressionAst, "Caller to verify ast is constant");
            return CompileAndInvoke(convertExpressionAst);
            CheckIsConstant(constantExpressionAst, "Caller to verify ast is constant");
            return constantExpressionAst.Value;
            CheckIsConstant(stringConstantExpressionAst, "Caller to verify ast is constant");
            return stringConstantExpressionAst.Value;
            CheckIsConstant(subExpressionAst, "Caller to verify ast is constant");
            CheckIsConstant(usingExpressionAst.SubExpression, "Caller to verify ast is constant");
            CheckIsConstant(variableExpressionAst, "Caller to verify ast is constant");
            string name = variableExpressionAst.VariablePath.UnqualifiedPath;
            if (name.Equals(SpecialVariables.True, StringComparison.OrdinalIgnoreCase))
            if (name.Equals(SpecialVariables.False, StringComparison.OrdinalIgnoreCase))
            Diagnostics.Assert(name.Equals(SpecialVariables.Null, StringComparison.OrdinalIgnoreCase), "Unexpected constant variable");
            CheckIsConstant(typeExpressionAst, "Caller to verify ast is constant");
            return typeExpressionAst.TypeName.GetReflectionType();
            CheckIsConstant(memberExpressionAst, "Caller to verify ast is constant");
            var member = ((StringConstantExpressionAst)memberExpressionAst.Member).Value;
            var memberInfo = type.GetMember(member, MemberTypes.Field,
            return ((FieldInfo)memberInfo[0]).GetValue(null);
            CheckIsConstant(arrayExpressionAst, "Caller to verify ast is constant");
            CheckIsConstant(arrayLiteralAst, "Caller to verify ast is constant");
            return arrayLiteralAst.Elements.Select(e => e.Accept(this)).ToArray();
            CheckIsConstant(scriptBlockExpressionAst, "Caller to verify ast is constant");
            return new ScriptBlock(scriptBlockExpressionAst.ScriptBlock, isFilter: false);
            CheckIsConstant(parenExpressionAst, "Caller to verify ast is constant");
            CheckIsConstant(hashtableAst, "Caller to verify ast is constant");
                result.Add(pair.Item1.Accept(this), pair.Item2.Accept(this));
