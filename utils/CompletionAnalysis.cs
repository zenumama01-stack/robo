using System.Management.Automation.Subsystem;
using System.Management.Automation.Subsystem.DSC;
    internal class CompletionContext
        internal List<Ast> RelatedAsts { get; set; }
        // Only one of TokenAtCursor or TokenBeforeCursor is set
        // This is how we can tell if we're trying to complete part of something (like a member)
        // or complete an argument, where TokenBeforeCursor could be a parameter name.
        internal Token TokenAtCursor { get; set; }
        internal Token TokenBeforeCursor { get; set; }
        internal IScriptPosition CursorPosition { get; set; }
        internal PowerShellExecutionHelper Helper { get; set; }
        internal Hashtable Options { get; set; }
        internal string WordToComplete { get; set; }
        internal int ReplacementIndex { get; set; }
        internal int ReplacementLength { get; set; }
        internal ExecutionContext ExecutionContext { get; set; }
        internal PseudoBindingInfo PseudoBindingInfo { get; set; }
        internal TypeInferenceContext TypeInferenceContext { get; set; }
        internal bool GetOption(string option, bool @default)
            if (Options == null || !Options.ContainsKey(option))
                return @default;
            return LanguagePrimitives.ConvertTo<bool>(Options[option]);
    internal class CompletionAnalysis
        private readonly Ast _ast;
        private readonly Token[] _tokens;
        private readonly IScriptPosition _cursorPosition;
        private readonly Hashtable _options;
        internal CompletionAnalysis(Ast ast, Token[] tokens, IScriptPosition cursorPosition, Hashtable options)
            _ast = ast;
            _tokens = tokens;
            _cursorPosition = cursorPosition;
        private static bool IsInterestingToken(Token token)
            return token.Kind != TokenKind.NewLine && token.Kind != TokenKind.EndOfInput;
        private static bool IsCursorWithinOrJustAfterExtent(IScriptPosition cursor, IScriptExtent extent)
            return cursor.Offset > extent.StartOffset && cursor.Offset <= extent.EndOffset;
        private static bool IsCursorRightAfterExtent(IScriptPosition cursor, IScriptExtent extent)
            return cursor.Offset == extent.EndOffset;
        private static bool IsCursorAfterExtentAndInTheSameLine(IScriptPosition cursor, IScriptExtent extent)
            return cursor.Offset >= extent.EndOffset && extent.EndLineNumber == cursor.LineNumber;
        private static bool IsCursorBeforeExtent(IScriptPosition cursor, IScriptExtent extent)
            return cursor.Offset < extent.StartOffset;
        private static bool IsCursorAfterExtent(IScriptPosition cursor, IScriptExtent extent)
            return extent.EndOffset < cursor.Offset;
        private static bool IsCursorOutsideOfExtent(IScriptPosition cursor, IScriptExtent extent)
            return cursor.Offset < extent.StartOffset || cursor.Offset > extent.EndOffset;
        internal readonly struct AstAnalysisContext
            internal AstAnalysisContext(Token tokenAtCursor, Token tokenBeforeCursor, List<Ast> relatedAsts, int replacementIndex)
                TokenAtCursor = tokenAtCursor;
                TokenBeforeCursor = tokenBeforeCursor;
                RelatedAsts = relatedAsts;
                ReplacementIndex = replacementIndex;
            internal readonly Token TokenAtCursor;
            internal readonly Token TokenBeforeCursor;
            internal readonly List<Ast> RelatedAsts;
            internal readonly int ReplacementIndex;
        internal static AstAnalysisContext ExtractAstContext(Ast inputAst, Token[] inputTokens, IScriptPosition cursor)
            bool adjustLineAndColumn = false;
            IScriptPosition positionForAstSearch = cursor;
            Token tokenBeforeCursor = null;
            Token tokenAtCursor = InterstingTokenAtCursorOrDefault(inputTokens, cursor);
            if (tokenAtCursor == null)
                tokenBeforeCursor = InterstingTokenBeforeCursorOrDefault(inputTokens, cursor);
                if (tokenBeforeCursor != null)
                    positionForAstSearch = tokenBeforeCursor.Extent.EndScriptPosition;
                    adjustLineAndColumn = true;
                var stringExpandableToken = tokenAtCursor as StringExpandableToken;
                if (stringExpandableToken?.NestedTokens != null)
                    tokenAtCursor = InterstingTokenAtCursorOrDefault(stringExpandableToken.NestedTokens, cursor) ?? stringExpandableToken;
            int replacementIndex = adjustLineAndColumn ? cursor.Offset : 0;
            List<Ast> relatedAsts = AstSearcher.FindAll(
                inputAst,
                ast => IsCursorWithinOrJustAfterExtent(positionForAstSearch, ast.Extent),
                searchNestedScriptBlocks: true).ToList();
            if (relatedAsts.Count == 0)
                relatedAsts.Add(inputAst);
            // If the last ast is an unnamed block that starts with "param" the cursor is inside a param block.
            // To avoid adding special handling to all the completers that look at the last ast, we remove it here because it's not useful for completion.
            if (relatedAsts[^1].Extent.Text.StartsWith("param", StringComparison.OrdinalIgnoreCase)
                && relatedAsts[^1] is NamedBlockAst namedBlock && namedBlock.Unnamed)
                relatedAsts.RemoveAt(relatedAsts.Count - 1);
            Diagnostics.Assert(tokenAtCursor == null || tokenBeforeCursor == null, "Only one of these tokens can be non-null");
            return new AstAnalysisContext(tokenAtCursor, tokenBeforeCursor, relatedAsts, replacementIndex);
        internal CompletionContext CreateCompletionContext(PowerShell powerShell)
            var typeInferenceContext = new TypeInferenceContext(powerShell);
            return InitializeCompletionContext(typeInferenceContext);
        internal CompletionContext CreateCompletionContext(TypeInferenceContext typeInferenceContext)
        private CompletionContext InitializeCompletionContext(TypeInferenceContext typeInferenceContext)
            var astContext = ExtractAstContext(_ast, _tokens, _cursorPosition);
            typeInferenceContext.CurrentTypeDefinitionAst ??= Ast.GetAncestorTypeDefinitionAst(astContext.RelatedAsts.Last());
            ExecutionContext executionContext = typeInferenceContext.ExecutionContext;
            return new CompletionContext
                Options = _options,
                CursorPosition = _cursorPosition,
                TokenAtCursor = astContext.TokenAtCursor,
                TokenBeforeCursor = astContext.TokenBeforeCursor,
                RelatedAsts = astContext.RelatedAsts,
                ReplacementIndex = astContext.ReplacementIndex,
                ExecutionContext = executionContext,
                TypeInferenceContext = typeInferenceContext,
                Helper = typeInferenceContext.Helper,
                CustomArgumentCompleters = executionContext.CustomArgumentCompleters,
                NativeArgumentCompleters = executionContext.NativeArgumentCompleters,
        private static Token InterstingTokenAtCursorOrDefault(IReadOnlyList<Token> tokens, IScriptPosition cursorPosition)
            for (int i = tokens.Count - 1; i >= 0; --i)
                Token token = tokens[i];
                if (IsCursorWithinOrJustAfterExtent(cursorPosition, token.Extent) && IsInterestingToken(token))
        private static Token InterstingTokenBeforeCursorOrDefault(IReadOnlyList<Token> tokens, IScriptPosition cursorPosition)
                if (IsCursorAfterExtent(cursorPosition, token.Extent) && IsInterestingToken(token))
        private static Ast GetLastAstAtCursor(ScriptBlockAst scriptBlockAst, IScriptPosition cursorPosition)
            var asts = AstSearcher.FindAll(scriptBlockAst, ast => IsCursorRightAfterExtent(cursorPosition, ast.Extent), searchNestedScriptBlocks: true);
            return asts.LastOrDefault();
        #region Special Cases
        /// Check if we should complete file names for "switch -file"
        private static bool CompleteAgainstSwitchFile(Ast lastAst, Token tokenBeforeCursor)
            Tuple<Token, Ast> fileConditionTuple;
            if (lastAst is ErrorStatementAst errorStatement && errorStatement.Flags is not null && errorStatement.Kind is not null && tokenBeforeCursor is not null &&
                errorStatement.Kind.Kind.Equals(TokenKind.Switch) && errorStatement.Flags.TryGetValue("file", out fileConditionTuple))
                // Handle "switch -file <tab>"
                return fileConditionTuple.Item1.Extent.EndOffset == tokenBeforeCursor.Extent.EndOffset;
            if (lastAst.Parent is CommandExpressionAst)
                // Handle "switch -file m<tab>" or "switch -file *.ps1<tab>"
                if (lastAst.Parent.Parent is not PipelineAst pipeline)
                if (pipeline.Parent is not ErrorStatementAst parentErrorStatement || parentErrorStatement.Kind is null || parentErrorStatement.Flags is null)
                return (parentErrorStatement.Kind.Kind.Equals(TokenKind.Switch) &&
                        parentErrorStatement.Flags.TryGetValue("file", out fileConditionTuple) && fileConditionTuple.Item2 == pipeline);
        /// Check if we should complete parameter names for switch cases on $PSBoundParameters.Keys
        private static List<CompletionResult> CompleteAgainstSwitchCaseCondition(CompletionContext completionContext)
            var lastAst = completionContext.RelatedAsts.Last();
            PipelineAst conditionPipeline = null;
            Ast switchAst = null;
            // Check if we're in a switch statement (complete) or error statement (incomplete switch)
            if (lastAst.Parent is SwitchStatementAst switchStatementAst)
                // Verify that the lastAst is one of the clause conditions (not in the body)
                bool isClauseCondition = switchStatementAst.Clauses.Any(clause => clause.Item1 == lastAst);
                if (!isClauseCondition)
                conditionPipeline = switchStatementAst.Condition as PipelineAst;
                switchAst = switchStatementAst;
                // Check for incomplete switch parsed as ErrorStatementAst
                if (lastAst.Parent is not ErrorStatementAst errorStatementAst || errorStatementAst.Kind is null ||
                    errorStatementAst.Kind.Kind != TokenKind.Switch)
                // For ErrorStatementAst, the case value is in Bodies, condition is in Conditions
                bool isInBodies = errorStatementAst.Bodies != null && errorStatementAst.Bodies.Any(body => body == lastAst);
                if (!isInBodies)
                // Get the condition from ErrorStatementAst.Conditions
                if (errorStatementAst.Conditions != null && errorStatementAst.Conditions.Count > 0)
                    conditionPipeline = errorStatementAst.Conditions[0] as PipelineAst;
                switchAst = errorStatementAst;
            if (conditionPipeline == null || conditionPipeline.PipelineElements.Count != 1)
            if (conditionPipeline.PipelineElements[0] is not CommandExpressionAst commandExpressionAst)
            // Check if the expression is a member access on $PSBoundParameters.Keys
            if (commandExpressionAst.Expression is not MemberExpressionAst memberExpressionAst)
            // Check if the target is $PSBoundParameters
            if (memberExpressionAst.Expression is not VariableExpressionAst variableExpressionAst ||
                !variableExpressionAst.VariablePath.UserPath.Equals("PSBoundParameters", StringComparison.OrdinalIgnoreCase))
            // Check if the member is "Keys"
            if (memberExpressionAst.Member is not StringConstantExpressionAst memberNameAst ||
                !memberNameAst.Value.Equals("Keys", StringComparison.OrdinalIgnoreCase))
            // Find the nearest param block by traversing up the AST
            var paramBlockAst = FindNearestParamBlock(switchAst.Parent);
            if (paramBlockAst == null || paramBlockAst.Parameters.Count == 0)
            // Generate completion results from parameter names
            var wordToComplete = completionContext.WordToComplete ?? string.Empty;
            return CreateParameterCompletionResults(paramBlockAst, wordToComplete);
        /// Check if we should complete parameter names for $PSBoundParameters access patterns
        /// Supports: $PSBoundParameters.ContainsKey('...'), $PSBoundParameters['...'], $PSBoundParameters.Remove('...')
        private static List<CompletionResult> CompleteAgainstPSBoundParametersAccess(CompletionContext completionContext)
            // Must be a string constant
            if (lastAst is not StringConstantExpressionAst stringAst)
            ExpressionAst targetAst = null;
            // Check for method invocation: $PSBoundParameters.ContainsKey('...') or $PSBoundParameters.Remove('...')
            if (lastAst.Parent is InvokeMemberExpressionAst invokeMemberAst)
                if (invokeMemberAst.Member is StringConstantExpressionAst memberName &&
                    (memberName.Value.Equals("ContainsKey", StringComparison.OrdinalIgnoreCase) ||
                     memberName.Value.Equals("Remove", StringComparison.OrdinalIgnoreCase)))
                    targetAst = invokeMemberAst.Expression;
            // Check for indexer: $PSBoundParameters['...']
            else if (lastAst.Parent is IndexExpressionAst indexAst)
                targetAst = indexAst.Target;
            if (targetAst is null)
            // Check if target is $PSBoundParameters
            if (targetAst is not VariableExpressionAst variableAst ||
                !variableAst.VariablePath.UserPath.Equals("PSBoundParameters", StringComparison.OrdinalIgnoreCase))
            // Find the nearest param block
            var paramBlockAst = FindNearestParamBlock(lastAst.Parent);
            // Determine quote style based on the string constant type
            string quoteChar = string.Empty;
            if (stringAst.StringConstantType == StringConstantType.SingleQuoted)
                quoteChar = "'";
            else if (stringAst.StringConstantType == StringConstantType.DoubleQuoted)
                quoteChar = "\"";
            return CreateParameterCompletionResults(paramBlockAst, wordToComplete, quoteChar);
        /// Finds the nearest ParamBlockAst by traversing up the AST hierarchy.
        /// <param name="startAst">The AST node to start searching from.</param>
        /// <returns>The nearest ParamBlockAst if found; otherwise, null.</returns>
        private static ParamBlockAst FindNearestParamBlock(Ast startAst)
            Ast current = startAst;
            while (current != null)
                if (current is FunctionDefinitionAst functionDefinitionAst)
                    return functionDefinitionAst.Body?.ParamBlock;
                else if (current is ScriptBlockAst scriptBlockAst)
                    var paramBlock = scriptBlockAst.ParamBlock;
                    if (paramBlock != null)
                        return paramBlock;
                current = current.Parent;
        /// Creates completion results from parameter names with optional quote wrapping.
        /// <param name="paramBlockAst">The parameter block containing parameters to complete.</param>
        /// <param name="wordToComplete">The partial word to match against parameter names.</param>
        /// <param name="quoteChar">Optional quote character to wrap completion text (empty string for no quotes).</param>
        /// <returns>A list of completion results, or null if no matches found.</returns>
        private static List<CompletionResult> CreateParameterCompletionResults(
            ParamBlockAst paramBlockAst,
            string quoteChar = "")
            var result = paramBlockAst.Parameters
                .Select(parameter => parameter.Name.VariablePath.UserPath)
                .Where(parameterName => parameterName.StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase))
                .Select(parameterName =>
                    new CompletionResult(
                        quoteChar + parameterName + quoteChar,
                        CompletionResultType.ParameterValue,
                        parameterName))
            return result.Count > 0 ? result : null;
        private static bool CompleteOperator(Token tokenAtCursor, Ast lastAst)
            if (tokenAtCursor.Kind == TokenKind.Minus)
                return lastAst is BinaryExpressionAst;
            else if (tokenAtCursor.Kind == TokenKind.Parameter)
                if (lastAst is CommandParameterAst)
                    return lastAst.Parent is ExpressionAst;
        private static bool CompleteAgainstStatementFlags(Ast scriptAst, Ast lastAst, Token token, out TokenKind kind)
            kind = TokenKind.Unknown;
            // Handle "switch -f<tab>"
            var errorStatement = lastAst as ErrorStatementAst;
            if (errorStatement != null && errorStatement.Kind != null)
                switch (errorStatement.Kind.Kind)
                    case TokenKind.Switch:
                        kind = TokenKind.Switch;
            // Handle "switch -<tab>". Skip cases like "switch ($a) {} -<tab> "
            var scriptBlockAst = scriptAst as ScriptBlockAst;
            if (token != null && token.Kind == TokenKind.Minus && scriptBlockAst != null)
                var asts = AstSearcher.FindAll(scriptBlockAst, ast => IsCursorAfterExtent(token.Extent.StartScriptPosition, ast.Extent), searchNestedScriptBlocks: true);
                Ast last = asts.LastOrDefault();
                errorStatement = null;
                while (last != null)
                    errorStatement = last as ErrorStatementAst;
                    if (errorStatement != null) { break; }
                    last = last.Parent;
                            Tuple<Token, Ast> value;
                            if (errorStatement.Flags != null && errorStatement.Flags.TryGetValue(Parser.VERBATIM_ARGUMENT, out value))
                                if (IsTokenTheSame(value.Item1, token))
        private static bool IsTokenTheSame(Token x, Token y)
            if (x.Kind == y.Kind && x.TokenFlags == y.TokenFlags &&
                x.Extent.StartLineNumber == y.Extent.StartLineNumber &&
                x.Extent.StartColumnNumber == y.Extent.StartColumnNumber &&
                x.Extent.EndLineNumber == y.Extent.EndLineNumber &&
                x.Extent.EndColumnNumber == y.Extent.EndColumnNumber)
        #endregion Special Cases
        internal List<CompletionResult> GetResults(PowerShell powerShell, out int replacementIndex, out int replacementLength)
            var completionContext = CreateCompletionContext(powerShell);
            PSLanguageMode? previousLanguageMode = null;
                // Tab expansion is called from a trusted function - we should apply ConstrainedLanguage if necessary.
                if (completionContext.ExecutionContext.HasRunspaceEverUsedConstrainedLanguageMode)
                    previousLanguageMode = completionContext.ExecutionContext.LanguageMode;
                    completionContext.ExecutionContext.LanguageMode = PSLanguageMode.ConstrainedLanguage;
                return GetResultHelper(completionContext, out replacementIndex, out replacementLength);
                if (previousLanguageMode.HasValue)
                    completionContext.ExecutionContext.LanguageMode = previousLanguageMode.Value;
        internal List<CompletionResult> GetResultHelper(CompletionContext completionContext, out int replacementIndex, out int replacementLength)
            replacementIndex = -1;
            replacementLength = -1;
            var tokenAtCursor = completionContext.TokenAtCursor;
            List<CompletionResult> result = null;
            if (tokenAtCursor != null)
                replacementIndex = tokenAtCursor.Extent.StartScriptPosition.Offset;
                replacementLength = tokenAtCursor.Extent.EndScriptPosition.Offset - replacementIndex;
                completionContext.ReplacementIndex = replacementIndex;
                completionContext.ReplacementLength = replacementLength;
                switch (tokenAtCursor.Kind)
                    case TokenKind.Variable:
                    case TokenKind.SplattedVariable:
                        completionContext.WordToComplete = ((VariableToken)tokenAtCursor).VariablePath.UserPath;
                        result = CompletionCompleters.CompleteVariable(completionContext);
                    case TokenKind.Multiply:
                    case TokenKind.Generic:
                    case TokenKind.MinusMinus: // for native commands '--'
                    case TokenKind.Identifier:
                        if (!tokenAtCursor.TokenFlags.HasFlag(TokenFlags.TypeName))
                            result = CompleteUsingKeywords(completionContext.CursorPosition.Offset, _tokens, ref replacementIndex, ref replacementLength);
                            if (result is not null)
                            result = GetResultForIdentifier(completionContext, ref replacementIndex, ref replacementLength);
                    case TokenKind.Parameter:
                        completionContext.WordToComplete = tokenAtCursor.Text;
                        var cmdAst = lastAst.Parent as CommandAst;
                        if (lastAst is StringConstantExpressionAst && cmdAst != null && cmdAst.CommandElements.Count == 1)
                            result = CompleteFileNameAsCommand(completionContext);
                        TokenKind statementKind;
                        if (CompleteAgainstStatementFlags(null, lastAst, null, out statementKind))
                            result = CompletionCompleters.CompleteStatementFlags(statementKind, completionContext.WordToComplete);
                        if (CompleteOperator(tokenAtCursor, lastAst))
                            result = CompletionCompleters.CompleteOperator(completionContext.WordToComplete);
                        // Handle scenarios like this: dir -path:<tab>
                        if (completionContext.WordToComplete.EndsWith(':'))
                            replacementIndex = tokenAtCursor.Extent.EndScriptPosition.Offset;
                            replacementLength = 0;
                            completionContext.WordToComplete = string.Empty;
                            result = CompletionCompleters.CompleteCommandArgument(completionContext);
                            result = CompletionCompleters.CompleteCommandParameter(completionContext);
                    case TokenKind.Dot:
                    case TokenKind.ColonColon:
                    case TokenKind.QuestionDot:
                        replacementIndex += tokenAtCursor.Text.Length;
                        result = CompletionCompleters.CompleteMember(completionContext, @static: tokenAtCursor.Kind == TokenKind.ColonColon, ref replacementLength);
                    case TokenKind.Comment:
                        result = CompletionCompleters.CompleteComment(completionContext, ref replacementIndex, ref replacementLength);
                    case TokenKind.StringExpandable:
                    case TokenKind.StringLiteral:
                        // Search to see if we're looking at an assignment
                        if (lastAst.Parent is CommandExpressionAst
                            && lastAst.Parent.Parent is AssignmentStatementAst assignmentAst)
                            // Handle scenarios like `$ErrorActionPreference = '<tab>`
                            if (TryGetCompletionsForVariableAssignment(completionContext, assignmentAst, out List<CompletionResult> completions))
                                return completions;
                        else if (lastAst.Parent is BinaryExpressionAst binaryExpression)
                            completionContext.WordToComplete = (tokenAtCursor as StringToken).Value;
                            result = CompletionCompleters.CompleteComparisonOperatorValues(completionContext, binaryExpression.Left);
                        else if (lastAst.Parent is IndexExpressionAst indexExpressionAst)
                            // Handles quoted string inside index expression like: $PSVersionTable["<Tab>"]
                            // Check for $PSBoundParameters indexer first
                            var psBoundResult = CompleteAgainstPSBoundParametersAccess(completionContext);
                            if (psBoundResult != null && psBoundResult.Count > 0)
                                return psBoundResult;
                            return CompletionCompleters.CompleteIndexExpression(completionContext, indexExpressionAst.Target);
                        result = GetResultForString(completionContext, ref replacementIndex, ref replacementLength);
                    case TokenKind.RBracket:
                        if (lastAst is TypeExpressionAst)
                            var targetExpr = (TypeExpressionAst)lastAst;
                            var memberResult = new List<CompletionResult>();
                            CompletionCompleters.CompleteMemberHelper(
                                targetExpr,
                                completionContext, memberResult);
                            if (memberResult.Count > 0)
                                replacementIndex++;
                                result = (from entry in memberResult
                                          let completionText = TokenKind.ColonColon.Text() + entry.CompletionText
                                          select new CompletionResult(completionText, entry.ListItemText, entry.ResultType, entry.ToolTip)).ToList();
                    case TokenKind.Comma:
                        // Handle array elements such as the followings:
                        //  - `dir .\cd,<tab>`
                        //  - `dir -Path: .\cd,<tab>`
                        //  - `dir .\abc.txt,<tab> -File`
                        //  - `dir -Path .\abc.txt,<tab> -File`
                        //  - `dir -Path: .\abc.txt,<tab> -File`
                        if (lastAst is ErrorExpressionAst or ArrayLiteralAst &&
                            lastAst.Parent is CommandAst or CommandParameterAst)
                            replacementIndex += replacementLength;
                        else if (lastAst is AttributeAst)
                            completionContext.ReplacementIndex = replacementIndex += tokenAtCursor.Text.Length;
                            completionContext.ReplacementLength = replacementLength = 0;
                            result = GetResultForAttributeArgument(completionContext, ref replacementIndex, ref replacementLength);
                            // Handle auto completion for enum/dependson property of DSC resource,
                            // cursor is right after ','
                            // Configuration config
                            //     User test
                            //     {
                            //         DependsOn=@('[user]x',|)
                            result = GetResultForEnumPropertyValueOfDSCResource(completionContext, string.Empty, ref replacementIndex, ref replacementLength, out _);
                    case TokenKind.AtCurly:
                        // Handle scenarios such as 'Sort-Object @{<tab>' and  'gci | Format-Table @{'
                        result = GetResultForHashtable(completionContext);
                        replacementIndex += 2;
                    case TokenKind.Semi:
                        // Handle scenarios such as 'gci | Format-Table @{Label=...;<tab>'
                        if (lastAst is HashtableAst)
                            replacementIndex += 1;
                    case TokenKind.Number:
                        // Handle scenarios such as Get-Process -Id 5<tab> || Get-Process -Id 5210, 3<tab> || Get-Process -Id: 5210, 3<tab>
                        if (lastAst is ConstantExpressionAst &&
                            (lastAst.Parent is CommandAst || lastAst.Parent is CommandParameterAst ||
                            (lastAst.Parent is ArrayLiteralAst &&
                             (lastAst.Parent.Parent is CommandAst || lastAst.Parent.Parent is CommandParameterAst))))
                            replacementIndex = completionContext.ReplacementIndex;
                            replacementLength = completionContext.ReplacementLength;
                        else if (lastAst.Parent is CommandExpressionAst
                            && lastAst.Parent.Parent is AssignmentStatementAst assignmentAst2)
                            // Handle scenarios like '[ValidateSet(11,22)][int]$i = 11; $i = 2<tab>'
                            if (TryGetCompletionsForVariableAssignment(completionContext, assignmentAst2, out List<CompletionResult> completions))
                                result = completions;
                    case TokenKind.Redirection:
                        // Handle file name completion after the redirection operator: gps ><tab> || gps >><tab> || dir con 2><tab> || dir con 2>><tab>
                        if (lastAst is ErrorExpressionAst && lastAst.Parent is FileRedirectionAst)
                            completionContext.ReplacementIndex = (replacementIndex += tokenAtCursor.Text.Length);
                            result = new List<CompletionResult>(CompletionCompleters.CompleteFilename(completionContext));
                    case TokenKind.Minus:
                        // Handle operator completion: 55 -<tab> || "string" -<tab> || (Get-Something) -<tab>
                            result = CompletionCompleters.CompleteOperator(string.Empty);
                        // Handle the flag completion for statements, such as the switch statement
                        if (CompleteAgainstStatementFlags(completionContext.RelatedAsts[0], null, tokenAtCursor, out statementKind))
                    case TokenKind.DynamicKeyword:
                            DynamicKeywordStatementAst keywordAst;
                            ConfigurationDefinitionAst configureAst = GetAncestorConfigurationAstAndKeywordAst(
                                completionContext.CursorPosition, lastAst, out keywordAst);
                            Diagnostics.Assert(configureAst != null, "ConfigurationDefinitionAst should never be null");
                            bool matched = false;
                            completionContext.WordToComplete = tokenAtCursor.Text.Trim();
                            // Current token is within ConfigurationDefinitionAst or DynamicKeywordStatementAst
                            return GetResultForIdentifierInConfiguration(completionContext, configureAst, null, out matched);
                    case TokenKind.Equals:
                    case TokenKind.AtParen:
                    case TokenKind.LParen:
                            if (lastAst is AttributeAst)
                            else if (lastAst is HashtableAst hashTableAst && lastAst.Parent is not DynamicKeywordStatementAst && CheckForPendingAssignment(hashTableAst))
                                // Handle scenarios such as 'gci | Format-Table @{Label=<tab>' if incomplete parsing of the assignment.
                            else if (lastAst is AssignmentStatementAst assignmentAst2)
                                // Handle scenarios like '$ErrorActionPreference =<tab>'
                            else if (lastAst is VariableExpressionAst && lastAst.Parent is ParameterAst paramAst && paramAst.Attributes.Count > 0)
                                foreach (AttributeBaseAst attribute in paramAst.Attributes)
                                    if (IsCursorWithinOrJustAfterExtent(_cursorPosition, attribute.Extent))
                                // Handle scenarios such as 'configuration foo { File ab { Attributes ='
                                // (auto completion for enum/dependson property of DSC resource),
                                // cursor is right after '=', '(' or '@('
                                //         DependsOn=|
                                //         DependsOn=@(|)
                                //         DependsOn=(|
                    case TokenKind.Format:
                    case TokenKind.Bnot:
                    case TokenKind.Xor:
                    case TokenKind.Band:
                    case TokenKind.Bor:
                    case TokenKind.Bxor:
                    case TokenKind.Join:
                    case TokenKind.Ireplace:
                    case TokenKind.Iin:
                    case TokenKind.Isplit:
                    case TokenKind.Creplace:
                    case TokenKind.Csplit:
                    case TokenKind.As:
                    case TokenKind.Shl:
                    case TokenKind.Shr:
                        result = CompletionCompleters.CompleteOperator(tokenAtCursor.Text);
                    case TokenKind.LBracket:
                        if (lastAst.Parent is IndexExpressionAst indexExpression)
                            // Handles index expression with cursor right after lbracket like: $PSVersionTable[<Tab>]
                            result = CompletionCompleters.CompleteIndexExpression(completionContext, indexExpression.Target);
                                replacementLength--;
                        if ((tokenAtCursor.TokenFlags & TokenFlags.Keyword) != 0)
                            // Handle the file name completion
                            // Handle the command name completion
                            var commandNameResult = CompletionCompleters.CompleteCommand(completionContext);
                            if (commandNameResult != null && commandNameResult.Count > 0)
                                result.AddRange(commandNameResult);
                IScriptPosition cursor = completionContext.CursorPosition;
                bool isCursorLineEmpty = string.IsNullOrWhiteSpace(cursor.Line);
                var tokenBeforeCursor = completionContext.TokenBeforeCursor;
                bool isLineContinuationBeforeCursor = false;
                    // Handle following scenario, cursor is in next line and after a command call,
                    // we need to skip the command call autocompletion if there is no backtick character
                    // in the end of the previous line, since backtick means command call continues to the next line
                    //         DependsOn=zzz
                    //         |
                    isLineContinuationBeforeCursor = completionContext.TokenBeforeCursor.Kind == TokenKind.LineContinuation;
                bool skipAutoCompleteForCommandCall = isCursorLineEmpty && !isLineContinuationBeforeCursor;
                bool lastAstIsExpressionAst = lastAst is ExpressionAst;
                if (!skipAutoCompleteForCommandCall &&
                    (lastAst is CommandParameterAst || lastAst is CommandAst ||
                    (lastAstIsExpressionAst && lastAst.Parent is CommandAst) ||
                    (lastAstIsExpressionAst && lastAst.Parent is CommandParameterAst) ||
                    (lastAstIsExpressionAst && lastAst.Parent is ArrayLiteralAst &&
                    var hashTableAst = lastAst as HashtableAst;
                    // Do not do any tab completion if we have a hash table
                    // and an assignment is pending.  For cases like:
                    //   new-object System.Drawing.Point -prop @{ X=  -> Tab should not complete
                    // Note: This check works when all statements preceding the last are complete,
                    //       but if a preceding statement is incomplete this test fails because
                    //       the Ast mixes the statements due to incomplete parsing.
                    //   e.g.,
                    //   new-object System.Drawing.Point -prop @{ X = 100; Y =      <- Incomplete line
                    //   new-object new-object System.Drawing.Point -prop @{ X =    <- Tab will yield hash properties.
                    if (hashTableAst != null &&
                        CheckForPendingAssignment(hashTableAst))
                    if (hashTableAst != null)
                        completionContext.ReplacementIndex = replacementIndex = completionContext.CursorPosition.Offset;
                        result = CompletionCompleters.CompleteHashtableKey(completionContext, hashTableAst);
                    // Handle completion of empty line within configuration statement
                    // Ignore the auto completion if there is a backtick character in previous line
                    bool cursorAtLineContinuation;
                    if ((tokenAtCursor != null && tokenAtCursor.Kind == TokenKind.LineContinuation) ||
                        (tokenBeforeCursor != null && tokenBeforeCursor.Kind == TokenKind.LineContinuation))
                        cursorAtLineContinuation = true;
                        cursorAtLineContinuation = false;
                    if (isCursorLineEmpty && !cursorAtLineContinuation)
                        // Handle following scenario, both Configuration and DSC resource 'User' are not complete
                        // Check Hashtable first, and then fallback to configuration
                        //         DependsOn=''
                        if (result == null || result.Count == 0)
                            ConfigurationDefinitionAst configAst = GetAncestorConfigurationAstAndKeywordAst(cursor, lastAst, out keywordAst);
                            if (configAst != null)
                                result = GetResultForIdentifierInConfiguration(completionContext, configAst, keywordAst, out matched);
                        // Handles following scenario where user is tab completing a member on an empty line:
                        // "Hello".
                        // <Tab>
                        if ((result is null || result.Count == 0) && tokenBeforeCursor is not null)
                            switch (completionContext.TokenBeforeCursor.Kind)
                                    replacementIndex = cursor.Offset;
                                    result = CompletionCompleters.CompleteMember(completionContext, @static: completionContext.TokenBeforeCursor.Kind == TokenKind.ColonColon, ref replacementLength);
                                    if (lastAst is VariableExpressionAst && lastAst.Parent is ParameterAst paramAst && paramAst.Attributes.Count > 0)
                                    if (lastAst is BinaryExpressionAst binaryExpression)
                                        // Handles index expression where cursor is on a new line after the lbracket like: $PSVersionTable[\n<Tab>]
                    else if (completionContext.TokenAtCursor == null)
                            // cursor is after '=', ',', '(', or '@('
                            //         DependsOn= |
                            //         DependsOn=@('[user]x', |)
                            //         DependsOn=@( |)
                            switch (tokenBeforeCursor.Kind)
                                        if (lastAst is AssignmentStatementAst assignmentAst)
                                            // Handle scenarios like '$ErrorActionPreference = <tab>'
                                            if (TryGetCompletionsForVariableAssignment(completionContext, assignmentAst, out result))
                                case TokenKind.Break:
                                case TokenKind.Continue:
                                        if ((lastAst is BreakStatementAst breakStatement && breakStatement.Label is null)
                                            || (lastAst is ContinueStatementAst continueStatement && continueStatement.Label is null))
                                            result = CompleteLoopLabel(completionContext);
                                case TokenKind.Using:
                                    return CompleteUsingKeywords(completionContext.CursorPosition.Offset, _tokens, ref replacementIndex, ref replacementLength);
                                    // Handles following scenario with whitespace after member access token: "Hello". <Tab>
                                    result = CompletionCompleters.CompleteMember(completionContext, @static: tokenBeforeCursor.Kind == TokenKind.ColonColon, ref replacementLength);
                                    if (result is not null && result.Count > 0)
                                        // Handles index expression with whitespace between lbracket and cursor like: $PSVersionTable[ <Tab>]
                    if (result != null && result.Count > 0)
                        bool needFileCompletion = false;
                            // Handle file name completion after redirection operator: gps > <tab>
                            needFileCompletion = true;
                        else if (lastAst is ErrorStatementAst && CompleteAgainstSwitchFile(lastAst, completionContext.TokenBeforeCursor))
                            // Handle file name completion after "switch -file": switch -file <tab>
                        if (needFileCompletion)
                var typeAst = completionContext.RelatedAsts.OfType<TypeExpressionAst>().FirstOrDefault();
                TypeName typeNameToComplete = null;
                if (typeAst != null)
                    typeNameToComplete = FindTypeNameToComplete(typeAst.TypeName, _cursorPosition);
                    var typeConstraintAst = completionContext.RelatedAsts.OfType<TypeConstraintAst>().FirstOrDefault();
                    if (typeConstraintAst != null)
                        typeNameToComplete = FindTypeNameToComplete(typeConstraintAst.TypeName, _cursorPosition);
                if (typeNameToComplete is null && tokenAtCursor?.TokenFlags.HasFlag(TokenFlags.TypeName) == true)
                    typeNameToComplete = new TypeName(tokenAtCursor.Extent, tokenAtCursor.Text);
                if (typeNameToComplete != null)
                    // See if the typename to complete really is within the typename, and if so, which one, in the case of generics.
                    replacementIndex = typeNameToComplete.Extent.StartOffset;
                    replacementLength = typeNameToComplete.Extent.EndOffset - replacementIndex;
                    completionContext.WordToComplete = typeNameToComplete.FullName;
                    return CompletionCompleters.CompleteType(completionContext);
                // Handles the following scenario: [ipaddress]@{Address=""; <Tab> }
                if (result?.Count > 0)
                    replacementIndex = completionContext.CursorPosition.Offset;
                // Handle special file completion scenarios: .\+file.txt -> +<tab>
                string input = completionContext.RelatedAsts[0].Extent.Text;
                if (Regex.IsMatch(input, @"^[\S]+$") && completionContext.RelatedAsts.Count > 0 && completionContext.RelatedAsts[0] is ScriptBlockAst)
                    replacementIndex = completionContext.RelatedAsts[0].Extent.StartScriptPosition.Offset;
                    replacementLength = completionContext.RelatedAsts[0].Extent.EndScriptPosition.Offset - replacementIndex;
                    completionContext.WordToComplete = input;
        // Helper method to auto complete hashtable key
        private static List<CompletionResult> GetResultForHashtable(CompletionContext completionContext)
            Ast lastRelatedAst = null;
            var cursorPosition = completionContext.CursorPosition;
            // Enumeration is used over the LastAst pattern because empty lines following a key-value pair will set LastAst to the value.
            // @{
            //     Key1="Value1"
            //     <Tab>
            // In this case the last 3 Asts will be StringConstantExpression, CommandExpression, and Pipeline instead of the expected Hashtable
            for (int i = completionContext.RelatedAsts.Count - 1; i >= 0; i--)
                Ast ast = completionContext.RelatedAsts[i];
                if (cursorPosition.Offset >= ast.Extent.StartOffset && cursorPosition.Offset <= ast.Extent.EndOffset)
                    lastRelatedAst = ast;
            if (lastRelatedAst is HashtableAst hashtableAst)
                // Cursor is just after the hashtable: @{}<Tab>
                if (completionContext.TokenAtCursor is not null && completionContext.TokenAtCursor.Kind == TokenKind.RCurly)
                bool cursorIsWithinOrOnSameLineAsKeypair = false;
                    if (cursorPosition.Offset >= pair.Item1.Extent.StartOffset
                        && (cursorPosition.Offset <= pair.Item2.Extent.EndOffset || cursorPosition.LineNumber == pair.Item2.Extent.EndLineNumber))
                        cursorIsWithinOrOnSameLineAsKeypair = true;
                if (cursorIsWithinOrOnSameLineAsKeypair)
                    var tokenBeforeOrAtCursor = completionContext.TokenBeforeCursor ?? completionContext.TokenAtCursor;
                    if (tokenBeforeOrAtCursor.Kind != TokenKind.Semi)
                completionContext.ReplacementIndex = completionContext.CursorPosition.Offset;
                completionContext.ReplacementLength = 0;
                return CompletionCompleters.CompleteHashtableKey(completionContext, hashtableAst);
        // Helper method to look for an incomplete assignment pair in hash table.
        private static bool CheckForPendingAssignment(HashtableAst hashTableAst)
            foreach (var keyValue in hashTableAst.KeyValuePairs)
                if (keyValue.Item2 is ErrorStatementAst)
                    // This indicates the assignment has not completed.
        internal static TypeName FindTypeNameToComplete(ITypeName type, IScriptPosition cursor)
            var typeName = type as TypeName;
                // If the cursor is at the start offset, it's not really inside, so return null.
                // If the cursor is at the end offset, it's not really inside, but it's just before the cursor,
                // we don want to complete it.
                return (cursor.Offset > type.Extent.StartOffset && cursor.Offset <= type.Extent.EndOffset)
                           ? typeName
            var genericTypeName = type as GenericTypeName;
            if (genericTypeName != null)
                typeName = FindTypeNameToComplete(genericTypeName.TypeName, cursor);
                foreach (var t in genericTypeName.GenericArguments)
                    typeName = FindTypeNameToComplete(t, cursor);
            var arrayTypeName = type as ArrayTypeName;
            if (arrayTypeName != null)
                return FindTypeNameToComplete(arrayTypeName.ElementType, cursor) ?? null;
        private static string GetFirstLineSubString(string stringToComplete, out bool hasNewLine)
            hasNewLine = false;
            if (!string.IsNullOrEmpty(stringToComplete))
                var index = stringToComplete.AsSpan().IndexOfAny('\r', '\n');
                    stringToComplete = stringToComplete.Substring(0, index);
                    hasNewLine = true;
            return stringToComplete;
        private static Tuple<ExpressionAst, StatementAst> GetHashEntryContainsCursor(
            IScriptPosition cursor,
            HashtableAst hashTableAst,
            bool isCursorInString)
            Tuple<ExpressionAst, StatementAst> keyValuePairWithCursor = null;
            foreach (var kvp in hashTableAst.KeyValuePairs)
                if (IsCursorWithinOrJustAfterExtent(cursor, kvp.Item2.Extent))
                    keyValuePairWithCursor = kvp;
                if (!isCursorInString)
                    // Handle following case, cursor is after '=' but before next key value pair,
                    // next key value pair will be treated as kvp.Item2 of 'Ensure' key
                    //    configuration foo
                    //        File foo
                    //            DestinationPath = "\foo.txt"
                    //            Ensure = |
                    //            DependsOn =@("[User]x")
                    if (kvp.Item2.Extent.StartLineNumber > kvp.Item1.Extent.EndLineNumber &&
                        IsCursorAfterExtentAndInTheSameLine(cursor, kvp.Item1.Extent))
                    // If cursor is not within a string, then handle following two cases,
                    //  #1) cursor is after '=', in the same line of previous key value pair
                    //      configuration test{File testfile{DestinationPath='c:\test'; Ensure = |
                    //  #2) cursor is after '=', in the separate line of previous key value pair
                    //      configuration test{File testfile{DestinationPath='c:\test';
                    //        Ensure = |
                    if (!IsCursorBeforeExtent(cursor, kvp.Item1.Extent) &&
                        IsCursorAfterExtentAndInTheSameLine(cursor, kvp.Item2.Extent))
            return keyValuePairWithCursor;
        // Pulls the variable out of an assignment's LHS expression
        // Also brings back the innermost type constraint if there is one
        private static VariableExpressionAst GetVariableFromExpressionAst(
            ExpressionAst expression,
            ref Type typeConstraint,
            ref ValidateSetAttribute setConstraint)
            switch (expression)
                // $x = ...
                case VariableExpressionAst variableExpression:
                    return variableExpression;
                // [type]$x = ...
                case ConvertExpressionAst convertExpression:
                    typeConstraint = convertExpression.Type.TypeName.GetReflectionType();
                    return GetVariableFromExpressionAst(convertExpression.Child, ref typeConstraint, ref setConstraint);
                // [attribute()][type]$x = ...
                case AttributedExpressionAst attributedExpressionAst:
                        setConstraint = attributedExpressionAst.Attribute.GetAttribute() as ValidateSetAttribute;
                        // Do nothing, just prevent fallout from an unsuccessful attribute conversion
                    return GetVariableFromExpressionAst(attributedExpressionAst.Child, ref typeConstraint, ref setConstraint);
                // Something else, like `MemberExpressionAst` $a.p = <tab> which isn't currently handled
        // Gets any type constraints or validateset constraints on a given variable
        private static bool TryGetTypeConstraintOnVariable(
            CompletionContext completionContext,
            string variableName,
            out Type typeConstraint,
            out ValidateSetAttribute setConstraint)
            typeConstraint = null;
            setConstraint = null;
            PSVariable variable = completionContext.ExecutionContext.EngineSessionState.GetVariable(variableName);
            if (variable == null || variable.Attributes.Count == 0)
            foreach (Attribute attribute in variable.Attributes)
                if (attribute is ArgumentTypeConverterAttribute typeConverterAttribute)
                    typeConstraint = typeConverterAttribute.TargetType;
                if (attribute is ValidateSetAttribute validateSetAttribute)
                    setConstraint = validateSetAttribute;
            return typeConstraint != null || setConstraint != null;
        private static List<CompletionResult> CompletePropertyAssignment(MemberExpressionAst memberExpression, CompletionContext context)
            if (SafeExprEvaluator.TrySafeEval(memberExpression, context.ExecutionContext, out var evalValue))
                if (evalValue is not null)
                    Type type = evalValue.GetType();
                        return GetResultForEnum(type, context);
            _ = TryGetInferredCompletionsForAssignment(memberExpression, context, out List<CompletionResult> result);
        private static bool TryGetInferredCompletionsForAssignment(Ast expression, CompletionContext context, out List<CompletionResult> result)
            IList<PSTypeName> inferredTypes;
            if (expression.Parent is ConvertExpressionAst convertExpression)
                inferredTypes = new PSTypeName[] { new(convertExpression.Type.TypeName) };
            else if (expression is MemberExpressionAst)
                inferredTypes = AstTypeInference.InferTypeOf(expression);
            else if (expression is VariableExpressionAst varExpression)
                PSTypeName typeConstraint = CompletionCompleters.GetLastDeclaredTypeConstraint(varExpression, context.TypeInferenceContext);
                if (typeConstraint is null)
                inferredTypes = new PSTypeName[] { typeConstraint };
            if (inferredTypes.Count == 0)
            var values = new SortedSet<string>();
            foreach (PSTypeName type in inferredTypes)
                Type loadedType = type.Type;
                if (loadedType is not null)
                    if (loadedType.IsEnum)
                        foreach (string value in Enum.GetNames(loadedType))
                            _ = values.Add(value);
                else if (type is not null && type.TypeDefinitionAst.IsEnum)
                    foreach (MemberAst member in type.TypeDefinitionAst.Members)
                        if (member is PropertyMemberAst property)
                            _ = values.Add(property.Name);
            string wordToComplete;
            if (string.IsNullOrEmpty(context.WordToComplete))
                if (context.TokenAtCursor is not null && context.TokenAtCursor.Kind != TokenKind.Equals)
                    wordToComplete = context.TokenAtCursor.Text + "*";
                    wordToComplete = "*";
                wordToComplete = context.WordToComplete + "*";
            result = new List<CompletionResult>();
            var pattern = new WildcardPattern(wordToComplete, WildcardOptions.IgnoreCase);
            foreach (string name in values)
                string quotedName = GetQuotedString(name, context);
                if (pattern.IsMatch(quotedName))
                    result.Add(new CompletionResult(quotedName, name, CompletionResultType.Property, name));
        private static bool TryGetCompletionsForVariableAssignment(
            AssignmentStatementAst assignmentAst,
            out List<CompletionResult> completions)
            bool TryGetResultForEnum(Type typeConstraint, CompletionContext completionContext, out List<CompletionResult> completions)
                completions = null;
                if (typeConstraint != null && typeConstraint.IsEnum)
                    completions = GetResultForEnum(typeConstraint, completionContext);
            bool TryGetResultForSet(Type typeConstraint, ValidateSetAttribute setConstraint, CompletionContext completionContext1, out List<CompletionResult> completions)
                if (setConstraint?.ValidValues != null)
                    completions = GetResultForSet(typeConstraint, setConstraint.ValidValues, completionContext);
            if (assignmentAst.Left is MemberExpressionAst member)
                completions = CompletePropertyAssignment(member, completionContext);
                return completions is not null;
            // Try to get the variable from the assignment, plus any type constraint on it
            Type typeConstraint = null;
            ValidateSetAttribute setConstraint = null;
            VariableExpressionAst variableAst = GetVariableFromExpressionAst(assignmentAst.Left, ref typeConstraint, ref setConstraint);
            if (variableAst == null)
            // Assignment constraints override any existing ones, so try them first
            // Check any [ValidateSet()] constraint first since it's likely to be narrow
            if (TryGetResultForSet(typeConstraint, setConstraint, completionContext, out completions))
            // Then try to complete for an enum type
            if (TryGetResultForEnum(typeConstraint, completionContext, out completions))
            // If the assignment itself was unconstrained, the variable still might be
            if (!TryGetTypeConstraintOnVariable(completionContext, variableAst.VariablePath.UserPath, out typeConstraint, out setConstraint))
                return TryGetInferredCompletionsForAssignment(variableAst, completionContext, out completions);
            // Again try the [ValidateSet()] constraint first
            // Then try to complete for an enum type again
        private static List<CompletionResult> GetResultForSet(
            Type typeConstraint,
            IList<string> validValues,
            CompletionContext completionContext)
            var allValues = new List<string>();
            foreach (string value in validValues)
                if (typeConstraint != null && (typeConstraint == typeof(string) || typeConstraint.IsEnum))
                    allValues.Add(GetQuotedString(value, completionContext));
                    allValues.Add(value);
            return GetMatchedResults(allValues, completionContext);
        private static List<CompletionResult> GetMatchedResults(
            List<string> allValues,
            var stringToComplete = string.Empty;
            if (completionContext.TokenAtCursor != null && completionContext.TokenAtCursor.Kind != TokenKind.Equals)
                stringToComplete = completionContext.TokenAtCursor.Text;
            IEnumerable<string> matchedResults = null;
                string matchString = stringToComplete + "*";
                var wildcardPattern = WildcardPattern.Get(matchString, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
                matchedResults = allValues.Where(r => wildcardPattern.IsMatch(r));
                matchedResults = allValues;
            var result = new List<CompletionResult>();
            foreach (var match in matchedResults)
                result.Add(new CompletionResult(match));
        private static string GetQuotedString(
            if (completionContext.TokenAtCursor != null)
            var quote = stringToComplete.StartsWith('"') ? "\"" : "'";
            return quote + value + quote;
        private static List<CompletionResult> GetResultForEnum(
            Type type,
            var allNames = new List<string>();
            foreach (var name in Enum.GetNames(type))
                allNames.Add(GetQuotedString(name, completionContext));
            allNames.Sort();
            return GetMatchedResults(allNames, completionContext);
        private static List<CompletionResult> GetResultForEnumPropertyValueOfDSCResource(
            string stringToComplete,
            ref int replacementIndex,
            ref int replacementLength,
            out bool shouldContinue)
            shouldContinue = true;
            bool isCursorInString = completionContext.TokenAtCursor is StringToken;
            Ast lastChildofHashtableAst;
            var hashTableAst = Ast.GetAncestorHashtableAst(lastAst, out lastChildofHashtableAst);
            Diagnostics.Assert(stringToComplete != null, "stringToComplete should never be null");
            // Check if the hashtable within a DynamicKeyword statement
                var keywordAst = Ast.GetAncestorAst<DynamicKeywordStatementAst>(hashTableAst);
                if (keywordAst != null)
                    var keyValuePairWithCursor = GetHashEntryContainsCursor(cursor, hashTableAst, isCursorInString);
                    if (keyValuePairWithCursor != null)
                        var propertyNameAst = keyValuePairWithCursor.Item1 as StringConstantExpressionAst;
                        if (propertyNameAst != null)
                            DynamicKeywordProperty property;
                            if (keywordAst.Keyword.Properties.TryGetValue(propertyNameAst.Value, out property))
                                List<string> existingValues = null;
                                WildcardPattern wildcardPattern = null;
                                bool isDependsOnProperty = string.Equals(property.Name, @"DependsOn", StringComparison.OrdinalIgnoreCase);
                                bool hasNewLine = false;
                                string stringQuote = (completionContext.TokenAtCursor is StringExpandableToken) ? "\"" : "'";
                                if ((property.ValueMap != null && property.ValueMap.Count > 0) || isDependsOnProperty)
                                    shouldContinue = false;
                                    existingValues = new List<string>();
                                    if (string.Equals(property.TypeConstraint, "StringArray", StringComparison.OrdinalIgnoreCase))
                                        var arrayAst = Ast.GetAncestorAst<ArrayLiteralAst>(lastAst);
                                        if (arrayAst != null && arrayAst.Elements.Count > 0)
                                            foreach (ExpressionAst expression in arrayAst.Elements)
                                                // stringAst can be null in following case
                                                //      DependsOn='[user]x',|
                                                var stringAst = expression as StringConstantExpressionAst;
                                                if (stringAst != null && IsCursorOutsideOfExtent(cursor, expression.Extent))
                                                    existingValues.Add(stringAst.Value);
                                    // Make sure only auto-complete string value in current line
                                    stringToComplete = GetFirstLineSubString(stringToComplete, out hasNewLine);
                                    completionContext.WordToComplete = stringToComplete;
                                    replacementLength = completionContext.ReplacementLength = stringToComplete.Length;
                                    // Calculate the replacementIndex based on cursor location (relative to the string token)
                                    if (completionContext.TokenAtCursor is StringToken)
                                        replacementIndex = completionContext.TokenAtCursor.Extent.StartOffset + 1;
                                        replacementIndex = completionContext.CursorPosition.Offset - replacementLength;
                                    wildcardPattern = WildcardPattern.Get(matchString, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
                                Diagnostics.Assert(isCursorInString || (!hasNewLine), "hasNoQuote and hasNewLine cannot be true at the same time");
                                if (property.ValueMap != null && property.ValueMap.Count > 0)
                                    IEnumerable<string> orderedValues = property.ValueMap.Keys.Order().Where(v => !existingValues.Contains(v, StringComparer.OrdinalIgnoreCase));
                                    var matchedResults = orderedValues.Where(v => wildcardPattern.IsMatch(v));
                                    if (matchedResults == null || !matchedResults.Any())
                                        // Fallback to all allowed values
                                        matchedResults = orderedValues;
                                    foreach (var value in matchedResults)
                                        string completionText = isCursorInString ? value : stringQuote + value + stringQuote;
                                        if (hasNewLine)
                                            completionText += stringQuote;
                                        result.Add(new CompletionResult(
                                            completionText,
                                            CompletionResultType.Text,
                                            value));
                                else if (isDependsOnProperty)
                                    var configAst = Ast.GetAncestorAst<ConfigurationDefinitionAst>(keywordAst);
                                        var namedBlockAst = Ast.GetAncestorAst<NamedBlockAst>(keywordAst);
                                        if (namedBlockAst != null)
                                            List<string> allResources = new List<string>();
                                            foreach (var statementAst in namedBlockAst.Statements)
                                                var dynamicKeywordAst = statementAst as DynamicKeywordStatementAst;
                                                if (dynamicKeywordAst != null &&
                                                    dynamicKeywordAst != keywordAst &&
                                                    !string.Equals(dynamicKeywordAst.Keyword.Keyword, @"Node", StringComparison.OrdinalIgnoreCase))
                                                    if (!string.IsNullOrEmpty(dynamicKeywordAst.ElementName))
                                                        StringBuilder sb = new StringBuilder("[", 50);
                                                        sb.Append(dynamicKeywordAst.Keyword.Keyword);
                                                        sb.Append(dynamicKeywordAst.ElementName);
                                                        var resource = sb.ToString();
                                                        if (!existingValues.Contains(resource, StringComparer.OrdinalIgnoreCase) &&
                                                            !allResources.Contains(resource, StringComparer.OrdinalIgnoreCase))
                                                            allResources.Add(resource);
                                            var matchedResults = allResources.Where(r => wildcardPattern.IsMatch(r));
                                                matchedResults = allResources;
                                            foreach (var resource in matchedResults)
                                                string completionText = isCursorInString ? resource : stringQuote + resource + stringQuote;
                                                    resource,
                                                    resource));
        private static List<CompletionResult> GetResultForString(CompletionContext completionContext, ref int replacementIndex, ref int replacementLength)
            var expandableString = lastAst as ExpandableStringExpressionAst;
            var constantString = lastAst as StringConstantExpressionAst;
            if (constantString == null && expandableString == null) { return null; }
            string strValue = constantString != null ? constantString.Value : expandableString.Value;
            // Check for switch case completion on $PSBoundParameters.Keys
            completionContext.WordToComplete = strValue;
            var switchCaseResult = CompleteAgainstSwitchCaseCondition(completionContext);
            if (switchCaseResult != null && switchCaseResult.Count > 0)
                return switchCaseResult;
            // Check for $PSBoundParameters access patterns (ContainsKey, indexer, Remove)
            bool shouldContinue;
            List<CompletionResult> result = GetResultForEnumPropertyValueOfDSCResource(completionContext, strValue, ref replacementIndex, ref replacementLength, out shouldContinue);
            if (!shouldContinue || (result != null && result.Count > 0))
            var commandElementAst = lastAst as CommandElementAst;
            string wordToComplete =
                CompletionCompleters.ConcatenateStringPathArguments(commandElementAst, string.Empty, completionContext);
            if (wordToComplete != null)
                completionContext.WordToComplete = wordToComplete;
                // Handle scenarios like this: cd 'c:\windows\win'<tab>
                if (lastAst.Parent is CommandAst || lastAst.Parent is CommandParameterAst)
                // Handle scenarios like this: "c:\wind"<tab>. Treat the StringLiteral/StringExpandable as path/command
                    // Handle path/commandname completion for quoted string
                    // Try command name completion only if the text contains '-'
                    if (wordToComplete.Contains('-'))
        /// Find the configuration statement contains current cursor.
        /// <param name="ast"></param>
        /// <param name="keywordAst"></param>
        private static ConfigurationDefinitionAst GetAncestorConfigurationAstAndKeywordAst(
            IScriptPosition cursorPosition,
            Ast ast,
            out DynamicKeywordStatementAst keywordAst)
            ConfigurationDefinitionAst configureAst = Ast.GetAncestorConfigurationDefinitionAstAndDynamicKeywordStatementAst(ast, out keywordAst);
            // Find the configuration statement contains current cursor
            // Note: cursorPosition.Offset < configureAst.Extent.EndOffset means cursor locates inside the configuration
            //       cursorPosition.Offset = configureAst.Extent.EndOffset means cursor locates at the end of the configuration
            while (configureAst != null && cursorPosition.Offset > configureAst.Extent.EndOffset)
                configureAst = Ast.GetAncestorAst<ConfigurationDefinitionAst>(configureAst.Parent);
            return configureAst;
        /// Generate auto complete results for identifier within configuration.
        /// Results are generated based on DynamicKeywords matches given identifier.
        /// For example, following "Fi" matches "File", and "Us" matches "User"
        ///     Configuration
        ///     {
        ///         Fi^
        ///         Node("TargetMachine")
        ///             Us^
        /// <param name="completionContext"></param>
        /// <param name="configureAst"></param>
        /// <param name="matched"></param>
        private static List<CompletionResult> GetResultForIdentifierInConfiguration(
            ConfigurationDefinitionAst configureAst,
            DynamicKeywordStatementAst keywordAst,
            out bool matched)
            matched = false;
            IEnumerable<DynamicKeyword> keywords = configureAst.DefinedKeywords.Where(
                k => // Node is special case, legal in both Resource and Meta configuration
                    string.Equals(k.Keyword, @"Node", StringComparison.OrdinalIgnoreCase) ||
                        // Check compatibility between Resource and Configuration Type
                        k.IsCompatibleWithConfigurationType(configureAst.ConfigurationType) &&
                        !DynamicKeyword.IsHiddenKeyword(k.Keyword) &&
                        !k.IsReservedKeyword
            if (keywordAst != null && completionContext.CursorPosition.Offset < keywordAst.Extent.EndOffset)
                keywords = keywordAst.Keyword.GetAllowedKeywords(keywords);
            if (keywords != null && keywords.Any())
                string commandName = (completionContext.WordToComplete ?? string.Empty) + "*";
                var wildcardPattern = WildcardPattern.Get(commandName, WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
                // Filter by name
                var matchedResults = keywords.Where(k => wildcardPattern.IsMatch(k.Keyword));
                    // Fallback to all legal keywords in the configuration statement
                    matchedResults = keywords;
                    matched = true;
                foreach (var keyword in matchedResults)
                    string usageString = string.Empty;
                    ICrossPlatformDsc dscSubsystem = SubsystemManager.GetSubsystem<ICrossPlatformDsc>();
                    if (dscSubsystem != null)
                        usageString = dscSubsystem.GetDSCResourceUsageString(keyword);
                        usageString = Microsoft.PowerShell.DesiredStateConfiguration.Internal.DscClassCache.GetDSCResourceUsageString(keyword);
                    results ??= new List<CompletionResult>();
                    results.Add(new CompletionResult(
                        keyword.Keyword,
                        CompletionResultType.DynamicKeyword,
                        usageString));
        private static List<CompletionResult> GetResultForIdentifier(CompletionContext completionContext, ref int replacementIndex, ref int replacementLength)
            var tokenAtCursorText = tokenAtCursor.Text;
            completionContext.WordToComplete = tokenAtCursorText;
            if (lastAst.Parent is BreakStatementAst || lastAst.Parent is ContinueStatementAst)
                return CompleteLoopLabel(completionContext);
            var strConst = lastAst as StringConstantExpressionAst;
            if (strConst != null)
                if (strConst.Value.Equals("$", StringComparison.Ordinal))
                    return CompletionCompleters.CompleteVariable(completionContext);
                    UsingStatementAst usingState = strConst.Parent as UsingStatementAst;
                    if (usingState != null)
                        completionContext.ReplacementIndex = strConst.Extent.StartOffset;
                        completionContext.ReplacementLength = strConst.Extent.EndOffset - replacementIndex;
                        completionContext.WordToComplete = strConst.Extent.Text;
                        switch (usingState.UsingStatementKind)
                            case UsingStatementKind.Assembly:
                                HashSet<string> assemblyExtensions = new(StringComparer.OrdinalIgnoreCase)
                                    StringLiterals.PowerShellILAssemblyExtension
                                return CompletionCompleters.CompleteFilename(completionContext, containerOnly: false, assemblyExtensions).ToList();
                            case UsingStatementKind.Command:
                            case UsingStatementKind.Module:
                                var moduleExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                    StringLiterals.PowerShellModuleFileExtension,
                                    StringLiterals.PowerShellNgenAssemblyExtension,
                                    StringLiterals.PowerShellILAssemblyExtension,
                                    StringLiterals.PowerShellILExecutableExtension,
                                    StringLiterals.PowerShellCmdletizationFileExtension
                                result = CompletionCompleters.CompleteFilename(completionContext, false, moduleExtensions).ToList();
                                if (completionContext.WordToComplete.IndexOfAny(Utils.Separators.DirectoryOrDrive) != -1)
                                    // The partial input is a path, then we don't iterate modules under $ENV:PSModulePath
                                var moduleResults = CompletionCompleters.CompleteModuleName(completionContext, false);
                                if (moduleResults != null && moduleResults.Count > 0)
                                    result.AddRange(moduleResults);
                            case UsingStatementKind.Namespace:
                                result = CompletionCompleters.CompleteNamespace(completionContext);
                            case UsingStatementKind.Type:
                                throw new ArgumentOutOfRangeException("UsingStatementKind");
            if (completionContext.TokenAtCursor.TokenFlags == TokenFlags.MemberName)
                if (lastAst is NamedAttributeArgumentAst || lastAst.Parent is NamedAttributeArgumentAst)
                        if (IsCursorWithinOrJustAfterExtent(completionContext.CursorPosition, attribute.Extent))
            if ((tokenAtCursor.TokenFlags & TokenFlags.CommandName) != 0)
                // Handle completion for a path with variable, such as: $PSHOME\ty<tab>
                if (completionContext.RelatedAsts.Count > 0 && completionContext.RelatedAsts[0] is ScriptBlockAst)
                    Ast cursorAst = completionContext.RelatedAsts[0].FindAll(
                        ast => ast.Extent.EndOffset <= tokenAtCursor.Extent.StartOffset
                            && ast.Extent is not EmptyScriptExtent,
                        searchNestedScriptBlocks: true).LastOrDefault();
                    if (cursorAst is not null)
                        if (cursorAst.Extent.EndOffset == tokenAtCursor.Extent.StartOffset)
                            if (tokenAtCursorText.AsSpan().IndexOfAny('\\', '/') == 0)
                                    CompletionCompleters.ConcatenateStringPathArguments(cursorAst as CommandElementAst, tokenAtCursorText, completionContext);
                                        replacementIndex = cursorAst.Extent.StartScriptPosition.Offset;
                                        replacementLength += cursorAst.Extent.Text.Length;
                                    var variableAst = cursorAst as VariableExpressionAst;
                                    string fullPath = variableAst != null
                                        ? CompletionCompleters.CombineVariableWithPartialPath(
                                            variableAst: variableAst,
                                            extraText: tokenAtCursorText,
                                            executionContext: completionContext.ExecutionContext)
                                    if (fullPath == null) { return result; }
                                    // Continue trying the filename/commandname completion for scenarios like this: $aa\d<tab>
                                    completionContext.WordToComplete = fullPath;
                            // Continue trying the filename/commandname completion for scenarios like this: $aa[get-<tab>
                            else if (cursorAst is not ErrorExpressionAst || cursorAst.Parent is not IndexExpressionAst)
                        if (cursorAst.Parent is IndexExpressionAst indexExpression && indexExpression.Index is ErrorExpressionAst)
                            if (completionContext.WordToComplete.EndsWith(']'))
                                completionContext.WordToComplete = completionContext.WordToComplete.Remove(completionContext.WordToComplete.Length - 1);
                            // Handles index expression with unquoted word like: $PSVersionTable[psver<Tab>]
                            return CompletionCompleters.CompleteIndexExpression(completionContext, indexExpression.Target);
                // Handle the StringExpandableToken;
                var strToken = tokenAtCursor as StringExpandableToken;
                if (strToken != null && strToken.NestedTokens != null && strConst != null)
                        string expandedString = null;
                        var expandableStringAst = new ExpandableStringExpressionAst(strConst.Extent, strConst.Value, StringConstantType.BareWord);
                        if (CompletionCompleters.IsPathSafelyExpandable(expandableStringAst: expandableStringAst,
                                                                        extraText: string.Empty,
                                                                        executionContext: completionContext.ExecutionContext,
                                                                        expandedString: out expandedString))
                            completionContext.WordToComplete = expandedString;
                // Handle completion of DSC resources within Configuration
                ConfigurationDefinitionAst configureAst = GetAncestorConfigurationAstAndKeywordAst(completionContext.CursorPosition, lastAst, out keywordAst);
                List<CompletionResult> keywordResult = null;
                if (configureAst != null)
                    keywordResult = GetResultForIdentifierInConfiguration(completionContext, configureAst, keywordAst, out matched);
                // Handle the file completion before command name completion
                if (matched && keywordResult != null)
                    result.InsertRange(0, keywordResult);
                else if (!matched && keywordResult != null && commandNameResult.Count == 0)
                    result.AddRange(keywordResult);
            var isSingleDash = tokenAtCursorText.Length == 1 && tokenAtCursorText[0].IsDash();
            var isDoubleDash = tokenAtCursorText.Length == 2 && tokenAtCursorText[0].IsDash() && tokenAtCursorText[1].IsDash();
            var isParentCommandOrDynamicKeyword = (lastAst.Parent is CommandAst || lastAst.Parent is DynamicKeywordStatementAst);
            if ((isSingleDash || isDoubleDash) && isParentCommandOrDynamicKeyword)
                // When it's the content of a quoted string, we only handle variable/member completion
                if (isSingleDash)
                    var res = CompletionCompleters.CompleteCommandParameter(completionContext);
                    if (res.Count != 0)
                return CompletionCompleters.CompleteCommandArgument(completionContext);
            TokenKind memberOperator = TokenKind.Unknown;
            bool isMemberCompletion = lastAst.Parent is MemberExpressionAst;
            bool isStatic = false;
            if (isMemberCompletion)
                var currentExpression = (MemberExpressionAst)lastAst.Parent;
                // Handles following scenario with an incomplete member access token at the end of the statement:
                // [System.IO.FileInfo]::new<Tab>().Directory.BaseName.Length.
                // Traverses up the expressions until it finds one under at the cursor
                while (currentExpression.Extent.EndOffset >= completionContext.CursorPosition.Offset
                    && currentExpression.Expression is MemberExpressionAst memberExpression
                    && memberExpression.Member.Extent.EndOffset >= completionContext.CursorPosition.Offset)
                    currentExpression = memberExpression;
                isStatic = currentExpression.Static;
            bool isWildcard = false;
            if (!isMemberCompletion)
                // Still might be member completion, something like: echo $member.
                // We need to know if the previous element before the token is adjacent because
                // we don't have a MemberExpressionAst, we might have 2 command arguments.
                if (tokenAtCursorText.Equals(TokenKind.Dot.Text(), StringComparison.Ordinal))
                    memberOperator = TokenKind.Dot;
                    isMemberCompletion = true;
                else if (tokenAtCursorText.Equals(TokenKind.ColonColon.Text(), StringComparison.Ordinal))
                    memberOperator = TokenKind.ColonColon;
                else if (tokenAtCursor.Kind.Equals(TokenKind.Multiply) && lastAst is BinaryExpressionAst)
                    // Handle member completion with wildcard(wildcard is at the end): $a.p*
                    var binaryExpressionAst = (BinaryExpressionAst)lastAst;
                    var memberExpressionAst = binaryExpressionAst.Left as MemberExpressionAst;
                    var errorPosition = binaryExpressionAst.ErrorPosition;
                    if (memberExpressionAst != null && binaryExpressionAst.Operator == TokenKind.Multiply &&
                        errorPosition.StartOffset == memberExpressionAst.Member.Extent.EndOffset)
                        isStatic = memberExpressionAst.Static;
                        memberOperator = isStatic ? TokenKind.ColonColon : TokenKind.Dot;
                        isWildcard = true;
                        // Member completion will add back the '*', so pretend it wasn't there, at least from the "related asts" point of view,
                        // but add the member expression that we are really completing.
                        completionContext.RelatedAsts.Remove(binaryExpressionAst);
                        completionContext.RelatedAsts.Add(memberExpressionAst);
                        var memberAst = memberExpressionAst.Member as StringConstantExpressionAst;
                        if (memberAst != null)
                            replacementIndex = memberAst.Extent.StartScriptPosition.Offset;
                            replacementLength += memberAst.Extent.Text.Length;
                result = CompletionCompleters.CompleteMember(completionContext, @static: (isStatic || memberOperator == TokenKind.ColonColon), ref replacementLength);
                // If the last token was just a '.', we tried to complete members.  That may
                // have failed because it wasn't really an attempt to complete a member, in
                // which case we should try to complete as an argument.
                    if (!isWildcard && memberOperator != TokenKind.Unknown)
                        replacementIndex += tokenAtCursorText.Length;
            if (lastAst.Parent is HashtableAst)
                result = CompletionCompleters.CompleteHashtableKey(completionContext, (HashtableAst)lastAst.Parent);
            if (lastAst.Parent is FileRedirectionAst || CompleteAgainstSwitchFile(lastAst, completionContext.TokenBeforeCursor))
                    CompletionCompleters.ConcatenateStringPathArguments(lastAst as CommandElementAst, string.Empty, completionContext);
            else if (tokenAtCursorText.AsSpan().IndexOfAny('\\', '/') == 0)
                var command = lastAst.Parent as CommandBaseAst;
                if (command != null && command.Redirections.Count > 0)
                    var fileRedirection = command.Redirections[0] as FileRedirectionAst;
                    if (fileRedirection != null &&
                        fileRedirection.Extent.EndLineNumber == lastAst.Extent.StartLineNumber &&
                        fileRedirection.Extent.EndColumnNumber == lastAst.Extent.StartColumnNumber)
                            CompletionCompleters.ConcatenateStringPathArguments(fileRedirection.Location, tokenAtCursorText, completionContext);
                            replacementIndex = fileRedirection.Location.Extent.StartScriptPosition.Offset;
                            replacementLength += fileRedirection.Location.Extent.EndScriptPosition.Offset - replacementIndex;
                return new List<CompletionResult>(CompletionCompleters.CompleteFilename(completionContext));
        private static List<CompletionResult> GetResultForAttributeArgument(CompletionContext completionContext, ref int replacementIndex, ref int replacementLength)
            // Attribute member arguments
            Type attributeType = null;
            string argName = string.Empty;
            Ast argAst = completionContext.RelatedAsts.Find(static ast => ast is NamedAttributeArgumentAst);
            AttributeAst attAst;
            if (argAst is NamedAttributeArgumentAst namedArgAst)
                attAst = (AttributeAst)namedArgAst.Parent;
                attributeType = attAst.TypeName.GetReflectionAttributeType();
                argName = namedArgAst.ArgumentName;
                replacementIndex = namedArgAst.Extent.StartOffset;
                replacementLength = argName.Length;
                Ast astAtt = completionContext.RelatedAsts.Find(static ast => ast is AttributeAst);
                attAst = astAtt as AttributeAst;
                if (attAst is not null)
            if (attributeType is not null)
                int cursorPosition = completionContext.CursorPosition.Offset;
                var existingArguments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var namedArgument in attAst.NamedArguments)
                    if (cursorPosition < namedArgument.Extent.StartOffset || cursorPosition > namedArgument.Extent.EndOffset)
                        existingArguments.Add(namedArgument.ArgumentName);
                PropertyInfo[] propertyInfos = attributeType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                List<CompletionResult> result = new List<CompletionResult>();
                foreach (PropertyInfo property in propertyInfos)
                    // Ignore getter-only properties and properties that have already been set.
                    if (!property.CanWrite || existingArguments.Contains(property.Name))
                    if (property.Name.StartsWith(argName, StringComparison.OrdinalIgnoreCase))
                        result.Add(new CompletionResult(property.Name, property.Name, CompletionResultType.Property,
                            property.PropertyType.ToString() + " " + property.Name));
        /// Complete file name as command.
        private static List<CompletionResult> CompleteFileNameAsCommand(CompletionContext completionContext)
            var addAmpersandIfNecessary = CompletionCompleters.IsAmpersandNeeded(completionContext, true);
            var clearLiteralPathsKey = false;
            if (completionContext.Options == null)
                completionContext.Options = new Hashtable { { "LiteralPaths", true } };
            else if (!completionContext.Options.ContainsKey("LiteralPaths"))
                // Dont escape '[',']','`' when the file name is treated as command name
                completionContext.Options.Add("LiteralPaths", true);
                clearLiteralPathsKey = true;
                var fileNameResult = CompletionCompleters.CompleteFilename(completionContext);
                foreach (var entry in fileNameResult)
                    // Add '&' to file names that are quoted
                    var completionText = entry.CompletionText;
                    var len = completionText.Length;
                    if (addAmpersandIfNecessary && len > 2 && completionText[0].IsSingleQuote() && completionText[len - 1].IsSingleQuote())
                        completionText = "& " + completionText;
                        result.Add(new CompletionResult(completionText, entry.ListItemText, entry.ResultType, entry.ToolTip));
                        result.Add(entry);
                if (clearLiteralPathsKey)
                    completionContext.Options.Remove("LiteralPaths");
        /// Complete loop labels after labeled control flow statements such as Break and Continue.
        private static List<CompletionResult> CompleteLoopLabel(CompletionContext completionContext)
            foreach (Ast ast in completionContext.RelatedAsts)
                if (ast is LabeledStatementAst labeledStatement
                    && labeledStatement.Label is not null
                    && (completionContext.WordToComplete is null || labeledStatement.Label.StartsWith(completionContext.WordToComplete, StringComparison.OrdinalIgnoreCase)))
                    result.Add(new CompletionResult(labeledStatement.Label, labeledStatement.Label, CompletionResultType.Text, labeledStatement.Extent.Text));
                else if (ast is ErrorStatementAst errorStatement)
                    // Handles incomplete do/switch loops (other labeled statements do not need this special treatment)
                    // The regex looks for the loopLabel of errorstatements that look like do/switch loops
                    // For example in ":Label do " it will find "Label".
                    var labelMatch = Regex.Match(errorStatement.Extent.Text, @"(?<=^:)\w+(?=\s+(do|switch)\b(?!-))", RegexOptions.IgnoreCase);
                    if (labelMatch.Success)
                        result.Add(new CompletionResult(labelMatch.Value, labelMatch.Value, CompletionResultType.Text, errorStatement.Extent.Text));
        private static List<CompletionResult> CompleteUsingKeywords(int cursorOffset, Token[] tokens, ref int replacementIndex, ref int replacementLength)
            Token tokenAtCursor = null;
            for (int i = tokens.Length - 1; i >= 0; i--)
                if (tokens[i].Extent.EndOffset < cursorOffset && tokens[i].Kind != TokenKind.LineContinuation)
                    tokenBeforeCursor = tokens[i];
                else if (tokens[i].Extent.StartOffset <= cursorOffset && tokens[i].Extent.EndOffset >= cursorOffset && tokens[i].Kind != TokenKind.LineContinuation)
                    tokenAtCursor = tokens[i];
            if (tokenBeforeCursor is not null && tokenBeforeCursor.Kind == TokenKind.Using)
                string wordToComplete = null;
                if (tokenAtCursor is not null)
                    replacementIndex = tokenAtCursor.Extent.StartOffset;
                    replacementLength = tokenAtCursor.Extent.Text.Length;
                    wordToComplete = tokenAtCursor.Text;
                    replacementIndex = cursorOffset;
                foreach (var keyword in s_usingKeywords)
                    if (string.IsNullOrEmpty(wordToComplete) || keyword.StartsWith(wordToComplete, StringComparison.OrdinalIgnoreCase))
                        result.Add(new CompletionResult(keyword, keyword, CompletionResultType.Keyword, GetUsingKeywordToolTip(keyword)));
        private static string GetUsingKeywordToolTip(string keyword)
            switch (keyword)
                case "assembly":
                    return TabCompletionStrings.AssemblyKeywordDescription;
                case "module":
                    return TabCompletionStrings.ModuleKeywordDescription;
                case "namespace":
                    return TabCompletionStrings.NamespaceKeywordDescription;
                case "type":
                    return TabCompletionStrings.TypeKeywordDescription;
        private static readonly string[] s_usingKeywords = new string[]
            "assembly",
            "module",
            "namespace",
            "type"
