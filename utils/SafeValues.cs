 * This visitor makes a determination as to whether an operation is safe in a GetPowerShell API Context.
 * It is modeled on the ConstantValueVisitor with changes which allow those
 * operations which are deemed safe, rather than constant. The following are differences from
 * ConstantValueVisitor:
 *  o Because we are going to call for values in ScriptBlockToPowerShell, the
 *    Get*ValueVisitor class is removed
 *  o IsGetPowerShellSafeValueVisitor only needs to determine whether it is safe, we won't return
 *    anything but that determination (vs actually returning a value in the out constantValue parameter
 *    as is found in the ConstantValueVisitor).
 *  o the internal bool members (Checking* members in ConstantValues) aren't needed as those checks are not germane
 *  o VisitExpandableStringExpression may be safe under the proper circumstances
 *  o VisitIndexExpression may be safe under the proper circumstances
 *  o VisitStatementBlock is safe if its component statements are safe
 *  o VisitBinaryExpression is not safe as it allows for a DOS attack
 *  o VisitVariableExpression is generally safe, there are checks outside of this code for ensuring variables actually
 *    have provided references. Those other checks ensure that the variable isn't something like $PID or $HOME, etc.,
 *    otherwise it's a safe operation, such as reference to a variable such as $true, or passed parameters.
 *  o VisitTypeExpression is not safe as it enables determining what types are available on the system which
 *    can imply what software has been installed on the system.
 *  o VisitMemberExpression is not safe as allows for the same attack as VisitTypeExpression
 *  o VisitArrayExpression may be safe if its components are safe
 *  o VisitArrayLiteral may be safe if its components are safe
 *  o VisitHashtable may be safe if its components are safe
 *  o VisitTernaryExpression may be safe if its components are safe
    internal class IsSafeValueVisitor : ICustomAstVisitor2
        public static bool IsAstSafe(Ast ast, GetSafeValueVisitor.SafeValueContext safeValueContext)
            IsSafeValueVisitor visitor = new IsSafeValueVisitor(safeValueContext);
            return visitor.IsAstSafe(ast);
        internal IsSafeValueVisitor(GetSafeValueVisitor.SafeValueContext safeValueContext)
            _safeValueContext = safeValueContext;
        internal bool IsAstSafe(Ast ast)
            if ((bool)ast.Accept(this) && _visitCount < MaxVisitCount)
        // A readonly singleton with the default SafeValueContext.
        internal static readonly IsSafeValueVisitor Default = new IsSafeValueVisitor(GetSafeValueVisitor.SafeValueContext.Default);
        // This is a check of the number of visits
        private uint _visitCount = 0;
        private const uint MaxVisitCount = 5000;
        private const int MaxHashtableKeyCount = 500;
        // Used to determine if we are being called within a GetPowerShell() context,
        // which does some additional security verification outside of the scope of
        // what we can verify.
        private readonly GetSafeValueVisitor.SafeValueContext _safeValueContext;
            return (bool)indexExpressionAst.Index.Accept(this) && (bool)indexExpressionAst.Target.Accept(this);
            bool isSafe = true;
            foreach (var nestedExpression in expandableStringExpressionAst.NestedExpressions)
                _visitCount++;
                if (!(bool)nestedExpression.Accept(this))
                    isSafe = false;
            return isSafe;
            foreach (var statement in statementBlockAst.Statements)
                if (!(bool)statement.Accept(this))
            // This can be used for a denial of service
            // Write-Output (((((("AAAAAAAAAAAAAAAAAAAAAA"*2)*2)*2)*2)*2)*2)
            // Keep on going with that pattern, and we're generating gigabytes of strings.
            bool unaryExpressionIsSafe = unaryExpressionAst.TokenKind.HasTrait(TokenFlags.CanConstantFold) &&
                !unaryExpressionAst.TokenKind.HasTrait(TokenFlags.DisallowedInRestrictedMode) &&
            if (unaryExpressionIsSafe)
            return unaryExpressionIsSafe;
            // $using:true should be safe - it's silly to write that, but not harmful.
            if (_safeValueContext == GetSafeValueVisitor.SafeValueContext.GetPowerShell)
                // GetPowerShell does its own validation of allowed variables in the
                // context of the entire script block, and then supplies this visitor
                // with the CommandExpressionAst directly. This
                // prevents us from evaluating variable safety in this visitor,
                // so we rely on GetPowerShell's implementation.
            if (_safeValueContext == GetSafeValueVisitor.SafeValueContext.ModuleAnalysis)
                return variableExpressionAst.IsConstantVariable() ||
                       (variableExpressionAst.VariablePath.IsUnqualified &&
                        variableExpressionAst.VariablePath.UnqualifiedPath.Equals(SpecialVariables.PSScriptRoot, StringComparison.OrdinalIgnoreCase));
            bool unused = false;
            return variableExpressionAst.IsSafeVariableReference(null, ref unused);
            // Type expressions are not safe as they allow fingerprinting by providing
            // a set of types, you can inspect the types in the AppDomain implying which assemblies are in use
            // and their version
            // An Array expression *may* be safe, if its components are safe
            bool isSafe = arrayLiteralAst.Elements.All(e => (bool)e.Accept(this));
            // An array literal is safe
            if (hashtableAst.KeyValuePairs.Count > MaxHashtableKeyCount)
            return hashtableAst.KeyValuePairs.All(pair => (bool)pair.Item1.Accept(this) && (bool)pair.Item2.Accept(this));
            // Returning a ScriptBlock instance itself is OK, bad stuff only happens
            // when invoking one (which is blocked)
     * This implementation retrieves the safe value without directly calling the compiler
     * except in the case of handling the unary operator
     * ExecutionContext is provided to ensure we can resolve variables
    internal sealed class GetSafeValueVisitor : ICustomAstVisitor2
        internal enum SafeValueContext
            GetPowerShell,
            ModuleAnalysis,
            SkipHashtableSizeCheck,
        // future proofing
        private GetSafeValueVisitor() { }
        public static object GetSafeValue(Ast ast, ExecutionContext context, SafeValueContext safeValueContext)
            t_context = context;
            if (safeValueContext == SafeValueContext.SkipHashtableSizeCheck || IsSafeValueVisitor.IsAstSafe(ast, safeValueContext))
                return ast.Accept(new GetSafeValueVisitor());
            if (safeValueContext == SafeValueContext.ModuleAnalysis)
            throw PSTraceSource.NewArgumentException(nameof(ast));
        /// This field needs to be thread-static to make 'GetSafeValue' thread safe.
        private static ExecutionContext t_context;
        public object VisitErrorStatement(ErrorStatementAst errorStatementAst) { throw PSTraceSource.NewArgumentException(nameof(errorStatementAst)); }
        public object VisitErrorExpression(ErrorExpressionAst errorExpressionAst) { throw PSTraceSource.NewArgumentException(nameof(errorExpressionAst)); }
        public object VisitScriptBlock(ScriptBlockAst scriptBlockAst) { throw PSTraceSource.NewArgumentException(nameof(scriptBlockAst)); }
        public object VisitParamBlock(ParamBlockAst paramBlockAst) { throw PSTraceSource.NewArgumentException(nameof(paramBlockAst)); }
        public object VisitNamedBlock(NamedBlockAst namedBlockAst) { throw PSTraceSource.NewArgumentException(nameof(namedBlockAst)); }
        public object VisitTypeConstraint(TypeConstraintAst typeConstraintAst) { throw PSTraceSource.NewArgumentException(nameof(typeConstraintAst)); }
        public object VisitAttribute(AttributeAst attributeAst) { throw PSTraceSource.NewArgumentException(nameof(attributeAst)); }
        public object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst) { throw PSTraceSource.NewArgumentException(nameof(namedAttributeArgumentAst)); }
        public object VisitParameter(ParameterAst parameterAst) { throw PSTraceSource.NewArgumentException(nameof(parameterAst)); }
        public object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst) { throw PSTraceSource.NewArgumentException(nameof(functionDefinitionAst)); }
        public object VisitIfStatement(IfStatementAst ifStmtAst) { throw PSTraceSource.NewArgumentException(nameof(ifStmtAst)); }
        public object VisitTrap(TrapStatementAst trapStatementAst) { throw PSTraceSource.NewArgumentException(nameof(trapStatementAst)); }
        public object VisitSwitchStatement(SwitchStatementAst switchStatementAst) { throw PSTraceSource.NewArgumentException(nameof(switchStatementAst)); }
        public object VisitDataStatement(DataStatementAst dataStatementAst) { throw PSTraceSource.NewArgumentException(nameof(dataStatementAst)); }
        public object VisitForEachStatement(ForEachStatementAst forEachStatementAst) { throw PSTraceSource.NewArgumentException(nameof(forEachStatementAst)); }
        public object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst) { throw PSTraceSource.NewArgumentException(nameof(doWhileStatementAst)); }
        public object VisitForStatement(ForStatementAst forStatementAst) { throw PSTraceSource.NewArgumentException(nameof(forStatementAst)); }
        public object VisitWhileStatement(WhileStatementAst whileStatementAst) { throw PSTraceSource.NewArgumentException(nameof(whileStatementAst)); }
        public object VisitCatchClause(CatchClauseAst catchClauseAst) { throw PSTraceSource.NewArgumentException(nameof(catchClauseAst)); }
        public object VisitTryStatement(TryStatementAst tryStatementAst) { throw PSTraceSource.NewArgumentException(nameof(tryStatementAst)); }
        public object VisitBreakStatement(BreakStatementAst breakStatementAst) { throw PSTraceSource.NewArgumentException(nameof(breakStatementAst)); }
        public object VisitContinueStatement(ContinueStatementAst continueStatementAst) { throw PSTraceSource.NewArgumentException(nameof(continueStatementAst)); }
        public object VisitReturnStatement(ReturnStatementAst returnStatementAst) { throw PSTraceSource.NewArgumentException(nameof(returnStatementAst)); }
        public object VisitExitStatement(ExitStatementAst exitStatementAst) { throw PSTraceSource.NewArgumentException(nameof(exitStatementAst)); }
        public object VisitThrowStatement(ThrowStatementAst throwStatementAst) { throw PSTraceSource.NewArgumentException(nameof(throwStatementAst)); }
        public object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst) { throw PSTraceSource.NewArgumentException(nameof(doUntilStatementAst)); }
        public object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst) { throw PSTraceSource.NewArgumentException(nameof(assignmentStatementAst)); }
        public object VisitCommand(CommandAst commandAst) { throw PSTraceSource.NewArgumentException(nameof(commandAst)); }
        public object VisitCommandExpression(CommandExpressionAst commandExpressionAst) { throw PSTraceSource.NewArgumentException(nameof(commandExpressionAst)); }
        public object VisitCommandParameter(CommandParameterAst commandParameterAst) { throw PSTraceSource.NewArgumentException(nameof(commandParameterAst)); }
        public object VisitFileRedirection(FileRedirectionAst fileRedirectionAst) { throw PSTraceSource.NewArgumentException(nameof(fileRedirectionAst)); }
        public object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst) { throw PSTraceSource.NewArgumentException(nameof(mergingRedirectionAst)); }
        public object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst) { throw PSTraceSource.NewArgumentException(nameof(attributedExpressionAst)); }
        public object VisitBlockStatement(BlockStatementAst blockStatementAst) { throw PSTraceSource.NewArgumentException(nameof(blockStatementAst)); }
        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst) { throw PSTraceSource.NewArgumentException(nameof(invokeMemberExpressionAst)); }
        public object VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst) { throw PSTraceSource.NewArgumentException(nameof(typeDefinitionAst)); }
        public object VisitPropertyMember(PropertyMemberAst propertyMemberAst) { throw PSTraceSource.NewArgumentException(nameof(propertyMemberAst)); }
        public object VisitFunctionMember(FunctionMemberAst functionMemberAst) { throw PSTraceSource.NewArgumentException(nameof(functionMemberAst)); }
        public object VisitBaseCtorInvokeMemberExpression(BaseCtorInvokeMemberExpressionAst baseCtorInvokeMemberExpressionAst) { throw PSTraceSource.NewArgumentException(nameof(baseCtorInvokeMemberExpressionAst)); }
        public object VisitUsingStatement(UsingStatementAst usingStatement) { throw PSTraceSource.NewArgumentException(nameof(usingStatement)); }
        public object VisitConfigurationDefinition(ConfigurationDefinitionAst configurationDefinitionAst) { throw PSTraceSource.NewArgumentException(nameof(configurationDefinitionAst)); }
        public object VisitDynamicKeywordStatement(DynamicKeywordStatementAst dynamicKeywordAst) { throw PSTraceSource.NewArgumentException(nameof(dynamicKeywordAst)); }
        // This is similar to logic used deep in the engine for slicing something that can be sliced
        // It's recreated here because there isn't really a simple API which can be called for this case.
        // this can throw, but there really isn't useful information we can add, as the
        // offending expression will be presented in the case of any failure
        private static object GetSingleValueFromTarget(object target, object index)
            var targetString = target as string;
            if (targetString != null)
                var offset = (int)index;
                if (Math.Abs(offset) >= targetString.Length)
                return offset >= 0 ? targetString[offset] : targetString[targetString.Length + offset];
            var targetArray = target as object[];
            if (targetArray != null)
                // this can throw, that just gets percolated back
                if (Math.Abs(offset) >= targetArray.Length)
                return offset >= 0 ? targetArray[offset] : targetArray[targetArray.Length + offset];
            var targetHashtable = target as Hashtable;
            if (targetHashtable != null)
                return targetHashtable[index];
            // The actual exception doesn't really matter because the caller in ScriptBlockToPowerShell
            // will present the user with the offending script segment
            throw new Exception();
        private static object GetIndexedValueFromTarget(object target, object index)
            var indexArray = index as object[];
            return indexArray != null ? ((object[])indexArray).Select(i => GetSingleValueFromTarget(target, i)).ToArray() : GetSingleValueFromTarget(target, index);
            // Get the value of the index and value and call the compiler
            var index = indexExpressionAst.Index.Accept(this);
            var target = indexExpressionAst.Target.Accept(this);
            if (index is null || target is null)
                throw new ArgumentNullException(nameof(indexExpressionAst));
            return GetIndexedValueFromTarget(target, index);
            object[] safeValues = new object[expandableStringExpressionAst.NestedExpressions.Count];
            // retrieve OFS, and if it doesn't exist set it to space
            string ofs = null;
            if (t_context != null)
                ofs = t_context.SessionState.PSVariable.GetValue("OFS") as string;
            ofs ??= " ";
            for (int offset = 0; offset < safeValues.Length; offset++)
                var result = expandableStringExpressionAst.NestedExpressions[offset].Accept(this);
                // depending on the nested expression we may retrieve a variable, or even need to
                // execute a sub-expression. The result of which may be returned
                // as a scalar, array or nested array. If the unwrap of first array doesn't contain a nested
                // array we can then pass it to string.Join. If it *does* contain an array,
                // we need to unwrap the inner array and pass *that* to string.Join.
                // This means we get the same answer with GetPowerShell() as in the command-line
                // { echo "abc $true $(1) $(2,3) def" }.Invoke() gives the same answer as
                // { echo "abc $true $(1) $(2,3) def" }.GetPowerShell().Invoke()
                // abc True 1 2 3 def
                // as does { echo "abc $true $(1) $(@(1,2),@(3,4)) def"
                // which is
                // abc True 1 System.Object[] System.Object[] def
                // fortunately, at this point, we're dealing with strings, so whatever the result
                // from the ToString method of the array (or scalar) elements, that's symmetrical with
                // a standard scriptblock invocation behavior
                var resultArray = result as object[];
                // In this environment, we can't use $OFS as we might expect. Retrieving OFS
                // might possibly leak server side info which we don't want, so we'll
                // assign ' ' as our OFS for purposes of GetPowerShell
                // Also, this will not call any script implementations of ToString (ala types.clixml)
                // This *will* result in a different result in those cases. However, to execute some
                // arbitrary script at this stage would be opening ourselves up to an attack
                if (resultArray != null)
                    object[] subExpressionResult = new object[resultArray.Length];
                    for (int subExpressionOffset = 0;
                        subExpressionOffset < subExpressionResult.Length;
                        subExpressionOffset++)
                        // check to see if there is an array in our array,
                        object[] subResult = resultArray[subExpressionOffset] as object[];
                        if (subResult != null)
                            subExpressionResult[subExpressionOffset] = string.Join(ofs, subResult);
                        else // it is a scalar, so we can just add it to our collections
                            subExpressionResult[subExpressionOffset] = resultArray[subExpressionOffset];
                    safeValues[offset] = string.Join(ofs, subExpressionResult);
                    safeValues[offset] = result;
            return StringUtil.Format(expandableStringExpressionAst.FormatExpression, safeValues);
            ArrayList statementList = new ArrayList();
                if (statement != null)
                    var obj = statement.Accept(this);
                    var enumerator = LanguagePrimitives.GetEnumerator(obj);
                            statementList.Add(enumerator.Current);
                        statementList.Add(obj);
                    throw PSTraceSource.NewArgumentException(nameof(statementBlockAst));
            return statementList.ToArray();
                return expr.Accept(this);
            throw PSTraceSource.NewArgumentException(nameof(pipelineAst));
            if (t_context == null)
                throw PSTraceSource.NewArgumentException(nameof(ternaryExpressionAst));
            return Compiler.GetExpressionValue(ternaryExpressionAst, isTrustedInput: true, t_context, usingValues: null);
            throw PSTraceSource.NewArgumentException(nameof(binaryExpressionAst));
                throw PSTraceSource.NewArgumentException(nameof(unaryExpressionAst));
            return Compiler.GetExpressionValue(unaryExpressionAst, isTrustedInput: true, t_context, usingValues: null);
            // at this point, we know we're safe because we checked both the type and the child,
            // so now we can just call the compiler and indicate that it's trusted (at this point)
                throw PSTraceSource.NewArgumentException(nameof(convertExpressionAst));
            return Compiler.GetExpressionValue(convertExpressionAst, isTrustedInput: true, t_context, usingValues: null);
            // There are earlier checks to be sure that we are not using unreferenced variables
            // this ensures that we only use what was declared in the param block
            // other variables such as true/false/args etc have been already vetted
            if (variableExpressionAst.IsConstantVariable())
            if (name.Equals(SpecialVariables.PSScriptRoot, StringComparison.OrdinalIgnoreCase))
                var scriptFileName = variableExpressionAst.Extent.File;
                if (scriptFileName == null)
                return Path.GetDirectoryName(scriptFileName);
                return VariableOps.GetVariableValue(variableExpressionAst.VariablePath, t_context, variableExpressionAst);
            throw PSTraceSource.NewArgumentException(nameof(variableExpressionAst));
            throw PSTraceSource.NewArgumentException(nameof(typeExpressionAst));
            throw PSTraceSource.NewArgumentException(nameof(memberExpressionAst));
            var arrayExpressionAstResult = (object[])arrayExpressionAst.SubExpression.Accept(this);
            return arrayExpressionAstResult;
            ArrayList arrayElements = new ArrayList();
                arrayElements.Add(element.Accept(this));
            return arrayElements.ToArray();
            Hashtable hashtable = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                var key = pair.Item1.Accept(this);
                var value = pair.Item2.Accept(this);
                hashtable.Add(key, value);
            return hashtable;
            return ScriptBlock.Create(scriptBlockExpressionAst.Extent.Text);
