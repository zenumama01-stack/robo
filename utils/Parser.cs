using Dsc = Microsoft.PowerShell.DesiredStateConfiguration.Internal;
    using SwitchClause = Tuple<ExpressionAst, StatementBlockAst>;
    internal enum ParseMode
        ModuleAnalysis = 1
    /// The parser that parses PowerShell script and returns a <see cref="ScriptBlockAst"/>, tokens, and error messages
    /// if the script cannot be parsed successfully.
    public sealed class Parser
        private readonly Tokenizer _tokenizer;
        internal Token _ungotToken;
        private bool _disableCommaOperator;
        private bool _savingTokens;
        private bool _inConfiguration;
        private ParseMode _parseMode;
        internal string _fileName;
        internal bool ProduceV2Tokens { get; set; }
        internal const string VERBATIM_ARGUMENT = "--%";
        internal const string VERBATIM_PARAMETERNAME = "-%";  // Same as VERBATIM_ARGUMENT w/o the first '-'.
        internal Parser()
            _tokenizer = new Tokenizer(this);
            ErrorList = new List<ParseError>();
            _fileName = null;
        /// Parse input from the specified file.
        /// <param name="fileName">The name of the file to parse.</param>
        /// <param name="tokens">Returns the tokens from parsing the script.</param>
        /// <param name="errors">Returns errors, if any, discovered while parsing the script.</param>
        /// <returns>The <see cref="ScriptBlockAst"/> that represents the input script file.</returns>
        public static ScriptBlockAst ParseFile(string fileName, out Token[] tokens, out ParseError[] errors)
            const string scriptSchemaExtension = ".schema.psm1";
            var parseDscResource = false;
            // If the file has the 'schema.psm1' extension, then it is a 'DSC module file' however we don't actually load the
            // module at parse time so we can't use the normal mechanisms to bind the module name for configuration commands.
            // As an alternative, we extract the base name of the module file and use that as the module name for any keywords exported by this file.
            if (!string.IsNullOrEmpty(fileName) && fileName.Length > scriptSchemaExtension.Length && fileName.EndsWith(scriptSchemaExtension, StringComparison.OrdinalIgnoreCase))
                parser._keywordModuleName = Path.GetFileName(fileName.AsSpan(0, fileName.Length - scriptSchemaExtension.Length)).ToString();
                parseDscResource = true;
            string scriptContents;
                var esi = new ExternalScriptInfo(fileName, fileName);
                scriptContents = esi.ScriptContents;
                var emptyExtent = new EmptyScriptExtent();
                var errorMsg = string.Format(CultureInfo.CurrentCulture, ParserStrings.FileReadError, e.Message);
                errors = new[] { new ParseError(emptyExtent, "FileReadError", errorMsg) };
                tokens = Array.Empty<Token>();
                return new ScriptBlockAst(emptyExtent, null, new StatementBlockAst(emptyExtent, null, null), false);
            var tokenList = new List<Token>();
            ScriptBlockAst result;
                if (!parseDscResource)
                    DynamicKeyword.Push();
                result = parser.Parse(fileName, scriptContents, tokenList, out errors, ParseMode.Default);
                throw new ParseException(ParserStrings.UnrecoverableParserError, e);
                    DynamicKeyword.Pop();
            tokens = tokenList.ToArray();
        private string _keywordModuleName;
        /// Parse input that does not come from a file.
        /// <param name="input">The input to parse.</param>
        public static ScriptBlockAst ParseInput(string input, out Token[] tokens, out ParseError[] errors)
            return ParseInput(input, null /* fileName */, out tokens, out errors);
        /// <param name="fileName">The fileName if present or null.</param>
        public static ScriptBlockAst ParseInput(string input, string fileName, out Token[] tokens, out ParseError[] errors)
                result = parser.Parse(fileName, input, tokenList, out errors, ParseMode.Default);
        internal ScriptBlockAst Parse(string fileName, string input, List<Token> tokenList, out ParseError[] errors, ParseMode parseMode)
                return ParseTask(fileName, input, tokenList, false, parseMode);
                errors = ErrorList.ToArray();
        private ScriptBlockAst ParseTask(string fileName, string input, List<Token> tokenList, bool recursed, ParseMode parseMode)
            bool etwEnabled = ParserEventSource.Log.IsEnabled();
            if (etwEnabled) ParserEventSource.Log.ParseStart(ParserEventSource.GetFileOrScript(fileName, input), input.Length);
            _parseMode = parseMode;
            _fileName = fileName;
            _tokenizer.Initialize(fileName, input, tokenList);
            _savingTokens = (tokenList != null);
            ErrorList.Clear();
                ast = ScriptBlockRule(null, isFilter: false);
                ast.InternalVisit(new CheckAllParentsSet(ast));
                ast.ScriptRequirements = _tokenizer.GetScriptRequirements();
                if (parseMode == ParseMode.Default)
                    ast.PerformPostParseChecks(this);
                ast.InternalVisit(new CheckTypeBuilder());
                if (!recursed)
                    // We'll try parsing once more, this time on a new thread.  The assumption here is
                    // that the stack was close to overflowing before we tried to parse, and that won't
                    // be a problem on a new thread.
                    var task = new Task<ScriptBlockAst>(() => ParseTask(fileName, input, tokenList, true, parseMode));
                    task.Start();
                    task.Wait();
                    ast = task.Result;
                    ReportError(_tokenizer.CurrentExtent(),
                        nameof(ParserStrings.ScriptTooComplicated),
                        ParserStrings.ScriptTooComplicated);
            if (etwEnabled) ParserEventSource.Log.ParseStop();
            return ast;
        // This helper routine is used from the runtime to convert a string to a number.
        internal static object ScanNumber(string str, Type toType, bool shouldTryCoercion = true)
                // For backwards compatibility, we treat the empty string (w/ or w/o whitespace) as the
                // integer constant 0.  This is a slight change in semantics, the empty string was previously
                // only considered a number if we committed to a numeric operation, e.g.:
                //    5 - ""   - this results in 5
                // Returning 0 here means that
                //   "" - ""   - will result in 0, previously was an error.
                // This is mostly of interest for the operators -, /, %, -band, -bor, -bxor as these operators
                // allow strings as the lval and still do a numeric operation, whereas + and * treat string
                // lvals differently.
            var tokenizer = (new Parser())._tokenizer;
            tokenizer.Initialize(null, str, null);
            tokenizer.AllowSignedNumbers = true;
            var token = tokenizer.NextToken() as NumberToken;
            if (token == null || !tokenizer.IsAtEndOfScript(token.Extent))
                if (shouldTryCoercion)
                    // We call ConvertTo, primarily because we expect it will throw an exception,
                    // but it's possible it could succeed, e.g. if the string had commas, our lexer
                    // will fail, but Convert.ChangeType could succeed.
                    return LanguagePrimitives.ConvertTo(str, toType, CultureInfo.InvariantCulture);
                    throw new ParseException();
            return token.Value;
        internal static ITypeName ScanType(string typename, bool ignoreErrors)
            typename = typename.Trim();
            if (typename.Length == 0)
            var tokenizer = parser._tokenizer;
            tokenizer.Initialize(null, typename, null);
            var result = parser.TypeNameRule(allowAssemblyQualifiedNames: true, firstTypeNameToken: out _);
            SemanticChecks.CheckArrayTypeNameDepth(result, PositionUtilities.EmptyExtent, parser);
            if (!ignoreErrors && result is not null && (parser.ErrorList.Count > 0 || !result.Extent.Text.Equals(typename, StringComparison.OrdinalIgnoreCase)))
        internal static ExpressionAst ScanString(string str)
            str = str.Replace("\"", "\"\"");
            parser._tokenizer.Initialize(null, '"' + str + '"', null);
            var strToken = (StringExpandableToken)parser._tokenizer.NextToken();
            var ast = parser.ExpandableStringRule(strToken);
        private string _previousFirstTokenText;
        private string _previousLastTokenText;
        private static bool IgnoreTokenWhenUpdatingPreviousFirstLast(Token token)
            return (token.Kind == TokenKind.Variable || token.Kind == TokenKind.Generic) &&
                   (token.Text.Equals("$^", StringComparison.OrdinalIgnoreCase) ||
                    token.Text.Equals("$$", StringComparison.OrdinalIgnoreCase));
        internal void SetPreviousFirstLastToken(ExecutionContext context)
            var firstToken = _tokenizer.FirstToken;
            if (firstToken != null)
                context.SetVariable(SpecialVariables.FirstTokenVarPath, _previousFirstTokenText);
                if (!IgnoreTokenWhenUpdatingPreviousFirstLast(firstToken))
                    var stringToken = firstToken as StringToken;
                    _previousFirstTokenText = stringToken != null
                        ? stringToken.Value
                        : firstToken.Text;
                context.SetVariable(SpecialVariables.LastTokenVarPath, _previousLastTokenText);
                var lastToken = _tokenizer.LastToken;
                if (!IgnoreTokenWhenUpdatingPreviousFirstLast(lastToken))
                    var stringToken = lastToken as StringToken;
                    _previousLastTokenText = stringToken != null
                        : lastToken.Text;
        internal List<ParseError> ErrorList { get; }
        private void SkipNewlines()
            if (_ungotToken == null || _ungotToken.Kind == TokenKind.NewLine)
                _ungotToken = null;
                _tokenizer.SkipNewlines(skipSemis: false);
        private void SkipNewlinesAndSemicolons()
            if (_ungotToken == null || _ungotToken.Kind == TokenKind.NewLine || _ungotToken.Kind == TokenKind.Semi)
                _tokenizer.SkipNewlines(skipSemis: true);
        private void SyncOnError(bool consumeClosingToken, params TokenKind[] syncTokens)
            int parens = syncTokens.Contains(TokenKind.RParen) ? 1 : 0;
            int curlies = syncTokens.Contains(TokenKind.RCurly) ? 1 : 0;
            int braces = syncTokens.Contains(TokenKind.RBracket) ? 1 : 0;
                Token token = NextToken();
                switch (token.Kind)
                    case TokenKind.LParen: ++parens; break;
                    case TokenKind.RParen:
                        --parens;
                        if (parens == 0 && syncTokens.Contains(TokenKind.RParen))
                            if (!consumeClosingToken)
                                UngetToken(token);
                    case TokenKind.LCurly: ++curlies; break;
                    case TokenKind.RCurly:
                        --curlies;
                        if (curlies == 0 && syncTokens.Contains(TokenKind.RCurly))
                    case TokenKind.LBracket: ++braces; break;
                        --braces;
                        if (braces == 0 && syncTokens.Contains(TokenKind.RBracket))
                    case TokenKind.EndOfInput:
                        // Never consume <EOF>, but return it to caller
                if (syncTokens.Contains(token.Kind) && parens == 0 && curlies == 0 && braces == 0)
        private Token NextToken()
            Token token = _ungotToken ?? _tokenizer.NextToken();
        private Token PeekToken()
            _ungotToken ??= token;
        private Token NextMemberAccessToken(bool allowLBracket)
            // If _ungotToken is not null, we're in some sort of error state, don't return the token.
            if (_ungotToken != null)
            return _tokenizer.GetMemberAccessOperator(allowLBracket);
        private Token NextInvokeMemberToken()
            return _tokenizer.GetInvokeMemberOpenParen();
        private Token NextLBracket()
                if (_ungotToken.Kind == TokenKind.LBracket) return NextToken();
            return _tokenizer.GetLBracket();
        private StringToken GetVerbatimCommandArgumentToken()
            if (_ungotToken == null || _ungotToken.Kind == TokenKind.Parameter)
                return _tokenizer.GetVerbatimCommandArgument();
        private void SkipToken()
            Diagnostics.Assert(_ungotToken != null, "Don't skip a token you didn't unget");
        private void UngetToken(Token token)
            Diagnostics.Assert(_ungotToken == null, "Only 1 token lookahead is supported");
            _ungotToken = token;
        private void SetTokenizerMode(TokenizerMode mode)
            if (mode != _tokenizer.Mode && _ungotToken != null)
                // Only rescan tokens that differ b/w command and expression modes.
                if (!_ungotToken.Kind.HasTrait(TokenFlags.ParseModeInvariant))
                    Resync(_ungotToken);
                else if (_ungotToken.Kind != TokenKind.EndOfInput)
                    // Verify the comment above.
                    Token ungotToken = _ungotToken;
                    var oldTokenList = _tokenizer.TokenList;
                    _tokenizer.TokenList = null;
                    _tokenizer.Mode = mode;
                    Token rescan = _tokenizer.NextToken();
                    Diagnostics.Assert(ungotToken.Kind == rescan.Kind, "Rescan failed to return same kind");
                    Diagnostics.Assert(ungotToken.Text == rescan.Text, "Rescan failed to return same text");
                    IScriptPosition pos1 = ungotToken.Extent.StartScriptPosition;
                    IScriptPosition pos2 = rescan.Extent.StartScriptPosition;
                    Diagnostics.Assert(pos1.ColumnNumber == pos2.ColumnNumber, "Rescan failed to return same start column");
                    Diagnostics.Assert(pos1.LineNumber == pos2.LineNumber, "Rescan failed to return same start line#");
                    pos1 = ungotToken.Extent.EndScriptPosition;
                    pos2 = rescan.Extent.EndScriptPosition;
                    Diagnostics.Assert(pos1.ColumnNumber == pos2.ColumnNumber, "Rescan failed to return same end column");
                    Diagnostics.Assert(pos1.LineNumber == pos2.LineNumber, "Rescan failed to return same end line#");
                    // Make sure we leave things as they were - Resync clears _ungotToken.
                    _ungotToken = ungotToken;
                    _tokenizer.TokenList = oldTokenList;
        private void Resync(Token token)
            _tokenizer.Resync(token);
        private void Resync(int restorePoint)
            _tokenizer.Resync(restorePoint);
        private static bool IsSpecificParameter(Token token, string parameter)
            Diagnostics.Assert(token.Kind == TokenKind.Parameter, "Token must be a ParameterToken");
            var paramToken = (ParameterToken)token;
            return parameter.StartsWith(paramToken.ParameterName, StringComparison.OrdinalIgnoreCase);
        internal void RequireStatementTerminator()
            var terminatorToken = PeekToken();
            if (terminatorToken.Kind == TokenKind.NewLine || terminatorToken.Kind == TokenKind.Semi)
                SkipToken();
            else if (terminatorToken.Kind != TokenKind.EndOfInput)
                ReportIncompleteInput(terminatorToken.Extent,
                    nameof(ParserStrings.MissingStatementTerminator),
                    ParserStrings.MissingStatementTerminator);
        internal static IScriptExtent ExtentOf(IScriptExtent first, IScriptExtent last)
            if (first is EmptyScriptExtent) return last;
            if (last is EmptyScriptExtent) return first;
            Diagnostics.Assert(first is InternalScriptExtent && last is InternalScriptExtent,
                "Private method expects internal position representation");
            InternalScriptExtent l = (InternalScriptExtent)first;
            InternalScriptExtent r = (InternalScriptExtent)last;
            Diagnostics.Assert(l.PositionHelper == r.PositionHelper, "Can't get the extent across files");
            return new InternalScriptExtent(l.PositionHelper, l.StartOffset, r.EndOffset);
        internal static IScriptExtent Before(IScriptExtent extent)
            Diagnostics.Assert(extent is InternalScriptExtent, "Private method expects internal position representation");
            InternalScriptExtent scriptExtent = (InternalScriptExtent)extent;
            int offset = scriptExtent.StartOffset - 1;
            if (offset < 0) offset = 0;
            return new InternalScriptExtent(scriptExtent.PositionHelper, offset, offset);
        internal static IScriptExtent After(IScriptExtent extent)
            int offset = scriptExtent.EndOffset;
        internal static IScriptExtent LastCharacterOf(IScriptExtent extent)
            int offset = scriptExtent.EndOffset - 1;
                offset = 0;
        internal static IScriptExtent ExtentFromFirstOf(params object[] objs)
                    var token = obj as Token;
                        return token.Extent;
                    var ast = obj as Ast;
                        return ast.Extent;
                    var typename = obj as ITypeName;
                    if (typename != null)
                        return typename.Extent;
                    Diagnostics.Assert(obj is IScriptExtent, "Only accepts tokens, asts, and IScriptExtents");
                    return (IScriptExtent)obj;
            Diagnostics.Assert(false, "One of the objects must not be null");
            return PositionUtilities.EmptyExtent;
        internal static IScriptExtent ExtentOf(Token first, Token last) { return ExtentOf(first.Extent, last.Extent); }
        internal static IScriptExtent ExtentOf(Ast first, Ast last) { return ExtentOf(first.Extent, last.Extent); }
        internal static IScriptExtent ExtentOf(Ast first, Token last) { return ExtentOf(first.Extent, last.Extent); }
        internal static IScriptExtent ExtentOf(Token first, Ast last) { return ExtentOf(first.Extent, last.Extent); }
        internal static IScriptExtent ExtentOf(IScriptExtent first, Ast last) { return ExtentOf(first, last.Extent); }
        internal static IScriptExtent ExtentOf(IScriptExtent first, Token last) { return ExtentOf(first, last.Extent); }
        internal static IScriptExtent ExtentOf(Ast first, IScriptExtent last) { return ExtentOf(first.Extent, last); }
        internal static IScriptExtent ExtentOf(Token first, IScriptExtent last) { return ExtentOf(first.Extent, last); }
        // private static IScriptExtent Before(Ast ast) { return Before(ast.Extent); }
        internal static IScriptExtent Before(Token token) { return Before(token.Extent); }
        internal static IScriptExtent After(Ast ast) { return After(ast.Extent); }
        internal static IScriptExtent After(Token token) { return After(token.Extent); }
        private static IEnumerable<Ast> GetNestedErrorAsts(params object[] asts)
            foreach (var obj in asts)
                    Ast ast = obj as Ast;
                        yield return ast;
                        var enumerable = obj as IEnumerable<Ast>;
                            foreach (var ast2 in enumerable)
                                if (ast2 != null)
                                    yield return ast2;
                            Diagnostics.Assert(false, "Caller to pass only asts, or IEnumerable<Ast>");
        /// Parses the specified constant hashtable string into a Hashtable object.
        /// <param name="input">The Hashtable string.</param>
        /// <param name="result">The Hashtable object.</param>
        internal static bool TryParseAsConstantHashtable(string input, out Hashtable result)
            if (string.IsNullOrWhiteSpace(input))
            var ast = Parser.ParseInput(input, out throwAwayTokens, out parseErrors);
            if (ast == null
                || parseErrors.Length > 0
                || ast.BeginBlock != null
                || ast.ProcessBlock != null
                || ast.CleanBlock != null
                || ast.DynamicParamBlock != null
                || ast.EndBlock.Traps != null)
            var statements = ast.EndBlock.Statements;
            if (statements.Count != 1)
            if (!(statements[0] is PipelineAst pipelineAst))
            if (expr == null)
            if (!(expr is HashtableAst hashTableAst))
            object hashtable;
            if (!IsConstantValueVisitor.IsConstant(hashTableAst, out hashtable, forRequires: true))
            var data = hashtable as Hashtable;
            Diagnostics.Assert((data != null), "IsConstantValueVisitor.IsConstant() should return false when the specified HashtableAst is not a avalid Hashtable");
            result = data;
        #endregion Utilities
        private ScriptBlockAst ScriptBlockRule(Token lCurly, bool isFilter)
            return ScriptBlockRule(lCurly, isFilter, null);
        private ScriptBlockAst ScriptBlockRule(Token lCurly, bool isFilter, StatementAst predefinedStatementAst)
            // G  script-block:
            // G      using-statements:opt   param-block:opt   statement-terminators:opt   script-block-body:opt
            // G
            // G  using-statements:
            // G      using-statement
            // G      using-statements   using-statement
            // We could set the mode here, but we can avoid rescanning keywords if the caller
            // sets the mode before skipping newlines.
            Diagnostics.Assert(_tokenizer.Mode == TokenizerMode.Command,
                "Caller to make sure the mode is correct.");
            // Skipping newlines here saves a more expensive resync if there is no parameter block.
            SkipNewlines();
            var usingStatements = lCurly == null ? UsingStatementsRule() : null;
            var restorePoint = _tokenizer.GetRestorePoint();
            ParamBlockAst paramBlock = ParamBlockRule();
            if (paramBlock == null)
                // In case we scanned some attributes or type constraints but they didn't
                // belong to a param statement, we need to reparse them (because they will
                // mean something different, such as a type literal expression, or a cast.)
                Resync(restorePoint);
            SkipNewlinesAndSemicolons();
            return ScriptBlockBodyRule(lCurly, usingStatements, paramBlock, isFilter, predefinedStatementAst);
        private List<UsingStatementAst> UsingStatementsRule()
            List<UsingStatementAst> result = null;
                var token = PeekToken();
                if (token.Kind == TokenKind.Using)
                    var statement = UsingStatementRule(token);
                    result ??= new List<UsingStatementAst>();
                    var usingStatement = statement as UsingStatementAst;
                    // otherwise returned statement is ErrorStatementAst.
                    // We ignore it here, because error already reported to the parser.
                    if (usingStatement != null)
                        result.Add(usingStatement);
                // Normally we don't need to resync, but our caller may speculatively scan much more than
                // one token, so it needs the restore point to include this token.
                Resync(token);
        private ParamBlockAst ParamBlockRule()
            // G  param-block:
            // G      new-lines:opt   attribute-list:opt   new-lines:opt   'param'   new-lines:opt
            // G              '('   parameter-list:opt   new-lines:opt   ')'
            List<AttributeBaseAst> candidateAttributes = AttributeListRule(false);
            Token paramToken = PeekToken();
            if (paramToken.Kind != TokenKind.Param)
            Token lparen = NextToken();
            if (lparen.Kind != TokenKind.LParen)
                UngetToken(lparen);
                // This is not an error, we'll end up trying to invoke a command named 'param'.
            List<ParameterAst> parameters = ParameterListRule();
            Token rParen = NextToken();
            var endExtent = rParen.Extent;
            if (rParen.Kind != TokenKind.RParen)
                // ErrorRecovery: assume we saw the closing paren and continue like normal.
                UngetToken(rParen);
                endExtent = Before(rParen);
                ReportIncompleteInput(After(parameters != null && parameters.Count > 0 ? parameters.Last().Extent : lparen.Extent),
                    nameof(ParserStrings.MissingEndParenthesisInFunctionParameterList),
                    ParserStrings.MissingEndParenthesisInFunctionParameterList);
            List<AttributeAst> attributes = new List<AttributeAst>();
            if (candidateAttributes != null)
                foreach (AttributeBaseAst attr in candidateAttributes)
                    AttributeAst attribute = attr as AttributeAst;
                        // ErrorRecovery: nothing to do, this is a semantic error that is caught in the parser
                        // because the ast only allows attributes, no type constraints.
                        ReportError(attr.Extent,
                            nameof(ParserStrings.TypeNotAllowedBeforeParam),
                            ParserStrings.TypeNotAllowedBeforeParam,
                            attr.TypeName.FullName);
            return new ParamBlockAst(ExtentOf(paramToken, endExtent), attributes, parameters);
        private List<ParameterAst> ParameterListRule()
            // G  parameter-list:
            // G      script-parameter
            // G      parameter-list   new-lines:opt   ','   script-parameter
            List<ParameterAst> parameters = new List<ParameterAst>();
            Token commaToken = null;
                ParameterAst parameter = ParameterRule();
                    if (commaToken != null)
                        // ErrorRecovery: ??
                        ReportIncompleteInput(After(commaToken),
                            nameof(ParserStrings.MissingExpressionAfterToken),
                            ParserStrings.MissingExpressionAfterToken,
                            commaToken.Kind.Text());
                parameters.Add(parameter);
                commaToken = PeekToken();
                if (commaToken.Kind != TokenKind.Comma)
        private ParameterAst ParameterRule()
            // G  script-parameter:
            // G      new-lines:opt   attribute-list:opt   new-lines:opt   variable   script-parameter-default:opt
            // G  script-parameter-default:
            // G      new-lines:opt   '='   new-lines:opt   expression
            List<AttributeBaseAst> attributes;
            VariableToken variableToken;
            ExpressionAst defaultValue = null;
            bool oldDisableCommaOperator = _disableCommaOperator;
            var oldTokenizerMode = _tokenizer.Mode;
                _disableCommaOperator = true;
                SetTokenizerMode(TokenizerMode.Expression);
                attributes = AttributeListRule(false);
                if (token.Kind != TokenKind.Variable && token.Kind != TokenKind.SplattedVariable)
                        // ErrorRecovery: skip to closing paren because returning null signals the last parameter.
                        ReportIncompleteInput(After(attributes.Last()),
                            nameof(ParserStrings.InvalidFunctionParameter),
                            ParserStrings.InvalidFunctionParameter);
                        SyncOnError(true, TokenKind.RParen);
                        // Even though we don't have a complete parameter, we do have attributes.  Intellisense
                        // might want to complete something in the attributes, so we need to return something useful.
                        var extent = ExtentOf(attributes[0].Extent, attributes[attributes.Count - 1].Extent);
                        return new ParameterAst(extent, new VariableExpressionAst(extent, "__error__", false), attributes, null);
                variableToken = ((VariableToken)token);
                Token equalsToken = PeekToken();
                if (equalsToken.Kind == TokenKind.Equals)
                    defaultValue = ExpressionRule();
                    if (defaultValue == null)
                        ReportIncompleteInput(After(equalsToken),
                            equalsToken.Kind.Text());
                _disableCommaOperator = oldDisableCommaOperator;
                SetTokenizerMode(oldTokenizerMode);
            IScriptExtent startExtent = (attributes == null) ? variableToken.Extent : attributes[0].Extent;
            IScriptExtent endExtent = (defaultValue == null) ? variableToken.Extent : defaultValue.Extent;
            return new ParameterAst(ExtentOf(startExtent, endExtent),
                new VariableExpressionAst(variableToken), attributes, defaultValue);
        private List<AttributeBaseAst> AttributeListRule(bool inExpressionMode)
            // G  attribute-list:
            // G      attribute
            // G      attribute-list   attribute
            List<AttributeBaseAst> attributes = new List<AttributeBaseAst>();
            AttributeBaseAst attribute = AttributeRule();
            while (attribute != null)
                if (!inExpressionMode || attribute is AttributeAst)
                attribute = AttributeRule();
            if (attributes.Count == 0)
            return attributes;
        private AttributeBaseAst AttributeRule()
            // G  attribute:
            // G      '['   attribute-name   '('   attribute-arguments   ')'   ']'
            // G  attribute-name:
            // G      type-spec
            var lBracket = NextLBracket();
            if (lBracket == null)
            Token firstTypeNameToken;
            ITypeName typeName = TypeNameRule(allowAssemblyQualifiedNames: true, firstTypeNameToken: out firstTypeNameToken);
                // ErrorRecovery: Return null so we stop looking for attributes.
                Resync(lBracket);  // TypeNameRule might have consumed some tokens
                ReportIncompleteInput(After(lBracket),
                    nameof(ParserStrings.MissingTypename),
                    ParserStrings.MissingTypename);
            Token lParenOrRBracket = NextToken();
            if (lParenOrRBracket.Kind == TokenKind.LParen)
                List<ExpressionAst> positionalArguments = new List<ExpressionAst>();
                List<NamedAttributeArgumentAst> namedArguments = new List<NamedAttributeArgumentAst>();
                IScriptExtent lastItemExtent = lParenOrRBracket.Extent;
                    AttributeArgumentsRule(positionalArguments, namedArguments, ref lastItemExtent);
                    // ErrorRecovery: pretend we saw a ')', attempt to find an ']'.
                    rParen = null;
                    ReportIncompleteInput(After(lastItemExtent),
                        nameof(ParserStrings.MissingEndParenthesisInExpression),
                        ParserStrings.MissingEndParenthesisInExpression);
                Token rBracket = NextToken();
                if (rBracket.Kind != TokenKind.RBracket)
                    // ErrorRecovery: pretend we saw a ']', return our result.
                    UngetToken(rBracket);
                    rBracket = null;
                    // Don't bother reporting a missing ']' if we reported a missing ')'.
                    if (rParen != null)
                        ReportIncompleteInput(After(rParen),
                            nameof(ParserStrings.EndSquareBracketExpectedAtEndOfAttribute),
                            ParserStrings.EndSquareBracketExpectedAtEndOfAttribute);
                firstTypeNameToken.TokenFlags |= TokenFlags.AttributeName;
                return new AttributeAst(ExtentOf(lBracket, ExtentFromFirstOf(rBracket, rParen, lastItemExtent)), typeName, positionalArguments, namedArguments);
            if (ProduceV2Tokens)
                var typeToken = new Token((InternalScriptExtent)ExtentOf(lBracket, lParenOrRBracket),
                    TokenKind.Identifier, TokenFlags.TypeName);
                _tokenizer.ReplaceSavedTokens(lBracket, lParenOrRBracket, typeToken);
            if (lParenOrRBracket.Kind != TokenKind.RBracket)
                UngetToken(lParenOrRBracket);
                ReportError(Before(lParenOrRBracket),
                lParenOrRBracket = null;
            return new TypeConstraintAst(ExtentOf(lBracket, ExtentFromFirstOf(lParenOrRBracket, typeName.Extent)), typeName);
        private void AttributeArgumentsRule(ICollection<ExpressionAst> positionalArguments,
                                            ICollection<NamedAttributeArgumentAst> namedArguments,
                                            ref IScriptExtent lastItemExtent)
            // G  attribute-arguments:
            // G      attribute-argument
            // G      attribute-argument   new-lines:opt   ','   attribute-arguments
            // G  attribute-argument:
            // G      new-lines:opt   expression
            // G      new-lines:opt   property-name   '='   new-lines:opt   expression
            HashSet<string> keysSeen = new HashSet<string>();
                    StringConstantExpressionAst name = SimpleNameRule();
                    ExpressionAst expr;
                    bool expressionOmitted = false;
                        Token token = PeekToken();
                        if (token.Kind == TokenKind.Equals)
                            token = NextToken();
                            expr = ExpressionRule();
                                // ErrorRecovery: ?
                                IScriptExtent errorPosition = After(token);
                                ReportIncompleteInput(
                                    nameof(ParserStrings.MissingExpressionInNamedArgument),
                                    ParserStrings.MissingExpressionInNamedArgument);
                                expr = new ErrorExpressionAst(errorPosition);
                                SyncOnError(true, TokenKind.Comma, TokenKind.RParen, TokenKind.RBracket, TokenKind.NewLine);
                            lastItemExtent = expr.Extent;
                            // If the expression is missing, assume the value true
                            // and record that it was defaulted for better error messages.
                            expr = new ConstantExpressionAst(name.Extent, true);
                            expressionOmitted = true;
                        if (keysSeen.Contains(name.Value))
                            // ErrorRecovery: this is a semantic error, so just keep parsing.
                            ReportError(name.Extent,
                                nameof(ParserStrings.DuplicateNamedArgument),
                                ParserStrings.DuplicateNamedArgument,
                                name.Value);
                            namedArguments.Add(new NamedAttributeArgumentAst(ExtentOf(name, expr),
                                name.Value, expr, expressionOmitted));
                    else if (expr != null)
                        positionalArguments.Add(expr);
                    else if (commaToken != null)
                        // ErrorRecovery: Pretend we saw the argument and keep going.
                        IScriptExtent errorExtent = After(commaToken);
                            errorExtent,
                        positionalArguments.Add(new ErrorExpressionAst(errorExtent));
                        lastItemExtent = errorExtent;
                    lastItemExtent = commaToken.Extent;
        private ITypeName TypeNameRule(bool allowAssemblyQualifiedNames, out Token firstTypeNameToken)
            // G  type-spec:
            // G      array-type-name    dimension:opt   ']'
            // G      generic-type-name   generic-type-arguments   ']'
            // G      type-name
            // G  array-type-name:
            // G      type-name   '['
            // G  generic-type-name:
            // G  dimension:
            // G      ','
            // G      dimension   ','
            // G  generic-type-arguments:
            // G      generic-type-arguments   ','   type-spec
            // G  type-name:
            // G      namespace-type-name
            // G      namespace-type-name   ','   assembly-name-spec
            // G  namespace-type-name:
            // G      nested-type-name
            // G      namespace-spec   '.'   nested-type-name
            // G  nested-type-name:
            // G      type-name-identifier
            // G      nested-type-name   '+'   type-name-identifier
            // G  namespace-spec:
            // G      type-name-identifier   '.'   type-name-identifier
            // G  type-name-identifier:
            // G      type-name-identifier-char
            // G      type-name-identifier   type-name-identifier-char
            // G  type-name-identifier-char:
            // G      any letter, digit, '.', '`', or '_'.
            // The above grammar is specified by the CLR.  We don't attempt to implement the above grammar precisely, e.g.
            // we would permit 'a+b.c'.  The above grammar would disallow this.
                // Switch to typename mode to avoid aggressive argument tokenization.
                SetTokenizerMode(TokenizerMode.TypeName);
                Token typeName = NextToken();
                if (typeName.Kind != TokenKind.Identifier)
                    UngetToken(typeName);
                    firstTypeNameToken = null;
                // firstTypeNameToken is returned so we can mark it as an attribute if it turns out
                // that we've got a attribute instead of a type literal.
                firstTypeNameToken = typeName;
                return FinishTypeNameRule(typeName, allowAssemblyQualifiedNames: allowAssemblyQualifiedNames);
        private ITypeName FinishTypeNameRule(Token typeName, bool unBracketedGenericArg = false, bool allowAssemblyQualifiedNames = true)
            Diagnostics.Assert(typeName.Kind == TokenKind.Identifier, "Caller must verify the argument.");
            if (token.Kind == TokenKind.LBracket)
                var lbracket = token;
                // Array or generic
                        var elementType = new TypeName(typeName.Extent, typeName.Text);
                        return CompleteArrayTypeName(elementType, elementType, token, unBracketedGenericArg);
                        return GenericTypeNameRule(typeName, token, unBracketedGenericArg);
                        // ErrorRecovery: sync to ']', and return non-null to avoid cascading errors.
                        if (token.Kind != TokenKind.EndOfInput)
                            ReportError(token.Extent,
                                nameof(ParserStrings.UnexpectedToken),
                                ParserStrings.UnexpectedToken,
                                token.Text);
                            SyncOnError(true, TokenKind.RBracket);
                            ReportIncompleteInput(After(lbracket),
                        return new TypeName(typeName.Extent, typeName.Text);
            if (token.Kind == TokenKind.Comma && allowAssemblyQualifiedNames && !unBracketedGenericArg)
                string assemblyNameSpec = _tokenizer.GetAssemblyNameSpec();
                if (string.IsNullOrWhiteSpace(assemblyNameSpec))
                    ReportError(After(token),
                        nameof(ParserStrings.MissingAssemblyNameSpecification),
                        ParserStrings.MissingAssemblyNameSpecification);
                return new TypeName(ExtentOf(typeName.Extent, _tokenizer.CurrentExtent()), typeName.Text, assemblyNameSpec);
        private ITypeName GetSingleGenericArgument(Token firstToken)
            if (firstToken.Kind == TokenKind.Identifier)
                return FinishTypeNameRule(firstToken, unBracketedGenericArg: true);
            Diagnostics.Assert(firstToken.Kind == TokenKind.LBracket, "caller to verify correct token kind");
                ReportIncompleteInput(After(token),
                return new TypeName(firstToken.Extent, ":ErrorTypeName:");
            ITypeName typeName = FinishTypeNameRule(token);
                    // ErrorRecovery: pretend we saw the closing bracket.
                    ReportIncompleteInput(Before(rBracket),
                        nameof(ParserStrings.EndSquareBracketExpectedAtEndOfType),
                        ParserStrings.EndSquareBracketExpectedAtEndOfType);
        private List<ITypeName> GenericTypeArgumentsRule(Token firstToken, out Token lastToken)
            Diagnostics.Assert(firstToken.Kind == TokenKind.Identifier || firstToken.Kind == TokenKind.LBracket, "unexpected first token");
            var genericArguments = new List<ITypeName>();
            ITypeName typeName = GetSingleGenericArgument(firstToken);
            genericArguments.Add(typeName);
                lastToken = NextToken();
                if (lastToken.Kind != TokenKind.Comma)
                if (token.Kind == TokenKind.Identifier || token.Kind == TokenKind.LBracket)
                    typeName = GetSingleGenericArgument(token);
                        After(lastToken),
                    typeName = new TypeName(lastToken.Extent, ":ErrorTypeName:");
            return genericArguments;
        private ITypeName GenericTypeNameRule(Token genericTypeName, Token firstToken, bool unbracketedGenericArg)
            List<ITypeName> genericArguments = GenericTypeArgumentsRule(firstToken, out Token rBracketToken);
            if (rBracketToken.Kind != TokenKind.RBracket)
                // ErrorRecovery: pretend we had the closing bracket and just continue on.
                UngetToken(rBracketToken);
                    Before(rBracketToken),
                rBracketToken = null;
            var openGenericType = new TypeName(genericTypeName.Extent, genericTypeName.Text, genericArguments.Count);
            var result = new GenericTypeName(
                ExtentOf(genericTypeName.Extent, ExtentFromFirstOf(rBracketToken, genericArguments.LastOrDefault(), firstToken)),
                openGenericType,
                genericArguments);
                return CompleteArrayTypeName(result, openGenericType, NextToken(), unbracketedGenericArg);
            if (token.Kind == TokenKind.Comma && !unbracketedGenericArg)
                if (string.IsNullOrEmpty(assemblyNameSpec))
                    ReportError(
                        After(token),
                    openGenericType.AssemblyName = assemblyNameSpec;
        private ITypeName CompleteArrayTypeName(ITypeName elementType, TypeName typeForAssemblyQualification, Token firstTokenAfterLBracket, bool unBracketedGenericArg)
                Token token;
                switch (firstTokenAfterLBracket.Kind)
                        int dim = 1;
                        token = firstTokenAfterLBracket;
                        Token lastComma;
                            lastComma = token;
                            dim += 1;
                        } while (token.Kind == TokenKind.Comma);
                        // The dimensions for an array must be less than or equal to 32.
                        // Search the doc for 'Type.MakeArrayType(int rank)' for more details.
                        if (dim > 32)
                            // If the next token is right bracket, we swallow it to make it easier to parse the rest of script.
                            // Otherwise, we unget the token for the subsequent parsing to consume.
                            if (token.Kind != TokenKind.RBracket)
                                ExtentOf(firstTokenAfterLBracket, lastComma),
                                nameof(ParserStrings.ArrayHasTooManyDimensions),
                                ParserStrings.ArrayHasTooManyDimensions,
                                arg: dim);
                            // ErrorRecovery: just pretend we saw a ']'.
                            ReportError(After(lastComma),
                        elementType = new ArrayTypeName(ExtentOf(elementType.Extent, token.Extent), elementType, dim);
                        elementType = new ArrayTypeName(ExtentOf(elementType.Extent, firstTokenAfterLBracket.Extent), elementType, 1);
                        UngetToken(firstTokenAfterLBracket);
                        ReportError(Before(firstTokenAfterLBracket),
                        // ErrorRecovery: sync to ']', and return null to avoid cascading errors.
                        ReportError(firstTokenAfterLBracket.Extent,
                            firstTokenAfterLBracket.Text);
                token = PeekToken();
                // An array declared inside an unbracketed generic type argument cannot be assembly qualified
                if (!unBracketedGenericArg && token.Kind == TokenKind.Comma)
                    var assemblyName = _tokenizer.GetAssemblyNameSpec();
                        typeForAssemblyQualification.AssemblyName = assemblyName;
                if (token.Kind != TokenKind.LBracket)
                // Jagged array, skip the '[' and keep parsing.
                firstTokenAfterLBracket = NextToken();
        private bool CompleteScriptBlockBody(Token lCurly, ref IScriptExtent bodyExtent, out IScriptExtent fullBodyExtent)
            // If the caller passed in the open curly, then they expect us to consume the closing curly
            // and include that in the extent.
            if (lCurly != null)
                Token rCurly = NextToken();
                IScriptExtent endScriptBlock;
                if (rCurly.Kind != TokenKind.RCurly)
                    // ErrorRecovery: pretend we saw the closing curly.
                    UngetToken(rCurly);
                    endScriptBlock = bodyExtent ?? lCurly.Extent;
                    ReportIncompleteInput(lCurly.Extent,
                        rCurly.Extent,
                        nameof(ParserStrings.MissingEndCurlyBrace),
                        ParserStrings.MissingEndCurlyBrace);
                    endScriptBlock = rCurly.Extent;
                    // If body was empty, use the extent b/w (but not including) the curlies, but only
                    // if that region has at least 1 character.
                    if (bodyExtent == null && (lCurly.Extent.EndColumnNumber != rCurly.Extent.StartColumnNumber))
                        bodyExtent = ExtentOf(After(lCurly), Before(rCurly));
                fullBodyExtent = ExtentOf(lCurly, endScriptBlock);
                fullBodyExtent = _tokenizer.GetScriptExtent();
                    // ErrorRecovery: eat the unexpected token, and continue parsing to find more
                    // of the script.
        private ScriptBlockAst ScriptBlockBodyRule(Token lCurly, List<UsingStatementAst> usingStatements, ParamBlockAst paramBlockAst, bool isFilter, StatementAst predefinedStatementAst)
            // G  script-block-body:
            // G      named-block-list
            // G      statement-list
            if ((token.TokenFlags & TokenFlags.ScriptBlockBlockName) == TokenFlags.ScriptBlockBlockName)
                return NamedBlockListRule(lCurly, usingStatements, paramBlockAst);
            List<TrapStatementAst> traps = new List<TrapStatementAst>();
            List<StatementAst> statements = new List<StatementAst>();
            if (predefinedStatementAst != null)
                statements.Add(predefinedStatementAst);
            IScriptExtent statementListExtent = paramBlockAst?.Extent;
            IScriptExtent scriptBlockExtent;
                IScriptExtent extent = StatementListRule(statements, traps);
                if (statementListExtent == null)
                    statementListExtent = extent;
                else if (extent != null)
                    statementListExtent = ExtentOf(statementListExtent, extent);
                if (CompleteScriptBlockBody(lCurly, ref statementListExtent, out scriptBlockExtent))
            return new ScriptBlockAst(scriptBlockExtent, usingStatements, paramBlockAst,
                new StatementBlockAst(statementListExtent ?? PositionUtilities.EmptyExtent, statements, traps), isFilter);
        private ScriptBlockAst NamedBlockListRule(Token lCurly, List<UsingStatementAst> usingStatements, ParamBlockAst paramBlockAst)
            // G  named-block-list:
            // G      named-block
            // G      named-block-list   named-block
            // G  named-block:
            // G      statement-terminators:opt   block-name   statement-block
            // G  block-name:  one of
            // G      'dynamicparam'   'begin'   'process'    'end'
            NamedBlockAst dynamicParamBlock = null;
            NamedBlockAst beginBlock = null;
            NamedBlockAst processBlock = null;
            NamedBlockAst endBlock = null;
            NamedBlockAst cleanBlock = null;
            IScriptExtent startExtent = lCurly?.Extent ?? paramBlockAst?.Extent;
            IScriptExtent endExtent = null;
            IScriptExtent extent = null;
            IScriptExtent scriptBlockExtent = null;
                Token blockNameToken = NextToken();
                switch (blockNameToken.Kind)
                        // Next token is unexpected.
                        // ErrorRecovery: if 'lCurly' is present, pretend we saw a closing curly; otherwise, eat the unexpected token.
                            UngetToken(blockNameToken);
                            scriptBlockExtent = ExtentOf(startExtent, endExtent);
                            // If "lCurly == null", then it's a ps1/psm1 file, and thus the extent is the whole file.
                            scriptBlockExtent = _tokenizer.GetScriptExtent();
                        // Report error about the unexpected token.
                        ReportError(blockNameToken.Extent,
                            nameof(ParserStrings.MissingNamedBlocks),
                            ParserStrings.MissingNamedBlocks,
                            blockNameToken.Text);
                        goto return_script_block_ast;
                        // If the next token is RCurly or <eof>, handle it in 'CompleteScriptBlockBody'.
                        extent = ExtentOf(startExtent, endExtent);
                        goto finished_named_block_list;
                    case TokenKind.Dynamicparam:
                    case TokenKind.Begin:
                    case TokenKind.Process:
                    case TokenKind.End:
                    case TokenKind.Clean:
                startExtent ??= blockNameToken.Extent;
                endExtent = blockNameToken.Extent;
                StatementBlockAst statementBlock = StatementBlockRule();
                if (statementBlock == null)
                    // ErrorRecovery: Eat the block name and keep going, there might be a valid block next.
                    ReportIncompleteInput(After(blockNameToken.Extent),
                        nameof(ParserStrings.MissingNamedStatementBlock),
                        ParserStrings.MissingNamedStatementBlock,
                        blockNameToken.Kind.Text());
                    statementBlock = new StatementBlockAst(blockNameToken.Extent, Array.Empty<StatementAst>(), null);
                    endExtent = statementBlock.Extent;
                extent = ExtentOf(blockNameToken, endExtent);
                if (blockNameToken.Kind == TokenKind.Begin && beginBlock == null)
                    beginBlock = new NamedBlockAst(extent, TokenKind.Begin, statementBlock, false);
                else if (blockNameToken.Kind == TokenKind.Process && processBlock == null)
                    processBlock = new NamedBlockAst(extent, TokenKind.Process, statementBlock, false);
                else if (blockNameToken.Kind == TokenKind.End && endBlock == null)
                    endBlock = new NamedBlockAst(extent, TokenKind.End, statementBlock, false);
                else if (blockNameToken.Kind == TokenKind.Clean && cleanBlock == null)
                    cleanBlock = new NamedBlockAst(extent, TokenKind.Clean, statementBlock, false);
                else if (blockNameToken.Kind == TokenKind.Dynamicparam && dynamicParamBlock == null)
                    dynamicParamBlock = new NamedBlockAst(extent, TokenKind.Dynamicparam, statementBlock, false);
                    // ErrorRecovery: this is a semantic error, we can keep parsing w/o trouble.
                    ReportError(extent,
                        nameof(ParserStrings.DuplicateScriptCommandClause),
                        ParserStrings.DuplicateScriptCommandClause,
        finished_named_block_list:
            CompleteScriptBlockBody(lCurly, ref extent, out scriptBlockExtent);
        return_script_block_ast:
            return new ScriptBlockAst(
                scriptBlockExtent,
                usingStatements,
                paramBlockAst,
                beginBlock,
                processBlock,
                endBlock,
                cleanBlock,
                dynamicParamBlock);
        private StatementBlockAst StatementBlockRule()
            // G  statement-block:
            // G      new-lines:opt   '{'   statement-list:opt   new-lines:opt   '}'
            Token lCurly = NextToken();
            if (lCurly.Kind != TokenKind.LCurly)
                UngetToken(lCurly);
            IScriptExtent statementListExtent = StatementListRule(statements, traps);
            IScriptExtent endBlock;
                // ErrorRecovery: Pretend we saw the missing curly and keep parsing.
                endBlock = statementListExtent ?? lCurly.Extent;
                endBlock = rCurly.Extent;
            return new StatementBlockAst(ExtentOf(lCurly, endBlock), statements, traps);
        private IScriptExtent StatementListRule(List<StatementAst> statements, List<TrapStatementAst> traps)
            // G  statement-list:
            // G      statement
            // G      statement-list   statement
            StatementAst firstStatement = null;
            StatementAst lastStatement = null;
                StatementAst statement = StatementRule();
                if (statement == null)
                _tokenizer.CheckAstIsBeforeSignature(statement);
                var trapStatementAst = statement as TrapStatementAst;
                if (trapStatementAst != null)
                    traps.Add(trapStatementAst);
                    statements.Add(statement);
                // Track the last statement inside our loop so we don't use the EmptyPipeline
                // as our last statement.  The last statement is used to track the extent of
                // this statement list.
                firstStatement ??= statement;
                lastStatement = statement;
                if (token.Kind == TokenKind.RParen || token.Kind == TokenKind.RCurly)
            return (firstStatement == null) ? null : ExtentOf(firstStatement, lastStatement);
        /// Parse a single statement.
        /// <returns>A statement ast.  Never returns null, always returns PipelineAst.EmptyPipeline if there was no statement.</returns>
        private StatementAst StatementRule()
            // G  statement:
            // G      if-statement
            // G      label:opt   labeled-statement
            // G      function-statement
            // G      flow-control-statement   statement-terminator
            // G      trap-statement
            // G      try-statement
            // G      data-statement
            // G      pipeline-chain   statement-terminator
            // G  labeled-statement:
            // G      switch-statement
            // G      foreach-statement
            // G      for-statement
            // G      while-statement
            // G      do-statement
            // G  flow-control-statement:
            // G      'break'   label-expression:opt
            // G      'continue'   label-expression:opt
            // G      'throw'    pipeline:opt
            // G      'return'   pipeline:opt
            // G      'exit'   pipeline:opt
            // G  statement-terminator:
            // G      ';'
            // G      new-line-character
            int restorePoint = 0;
            StatementAst statement;
            List<AttributeBaseAst> attributes = null;
            if (token.Kind == TokenKind.Generic && token.Text[0] == '[')
                restorePoint = token.Extent.StartOffset;
                if (attributes != null
                    && attributes.Count > 0)
                    if ((token.TokenFlags & TokenFlags.StatementDoesntSupportAttributes) != 0)
                        if (attributes.OfType<TypeConstraintAst>().Any())
                            ReportError(attributes[0].Extent,
                                nameof(ParserStrings.UnexpectedAttribute),
                                ParserStrings.UnexpectedAttribute,
                                attributes[0].TypeName.FullName);
                    else if ((token.TokenFlags & TokenFlags.Keyword) != 0)
                        foreach (var attr in attributes.Where(static attr => attr is not AttributeAst))
                                nameof(ParserStrings.TypeNotAllowedBeforeStatement),
                                ParserStrings.TypeNotAllowedBeforeStatement,
                case TokenKind.If:
                    statement = IfStatementRule(token);
                    statement = SwitchStatementRule(null, token);
                case TokenKind.Foreach:
                    statement = ForeachStatementRule(null, token);
                case TokenKind.For:
                    statement = ForStatementRule(null, token);
                case TokenKind.While:
                    statement = WhileStatementRule(null, token);
                case TokenKind.Do:
                    statement = DoWhileStatementRule(null, token);
                case TokenKind.Function:
                case TokenKind.Filter:
                case TokenKind.Workflow:
                    statement = FunctionDeclarationRule(token);
                case TokenKind.Return:
                    statement = ReturnStatementRule(token);
                case TokenKind.Throw:
                    statement = ThrowStatementRule(token);
                case TokenKind.Exit:
                    statement = ExitStatementRule(token);
                    statement = BreakStatementRule(token);
                    statement = ContinueStatementRule(token);
                case TokenKind.Trap:
                    statement = TrapStatementRule(token);
                case TokenKind.Try:
                    statement = TryStatementRule(token);
                case TokenKind.Data:
                    statement = DataStatementRule(token);
                case TokenKind.Parallel:
                case TokenKind.Sequence:
                    statement = BlockStatementRule(token);
                case TokenKind.Configuration:
                    statement = ConfigurationStatementRule(attributes?.OfType<AttributeAst>(), token);
                case TokenKind.From:
                case TokenKind.Define:
                case TokenKind.Var:
                        nameof(ParserStrings.ReservedKeywordNotAllowed),
                        ParserStrings.ReservedKeywordNotAllowed,
                        token.Kind.Text());
                    statement = new ErrorStatementAst(token.Extent);
                case TokenKind.Label:
                    statement = LabeledStatementRule((LabelToken)token);
                        statement = PipelineChainRule();
                        statement = null;
                case TokenKind.Else:
                case TokenKind.ElseIf:
                case TokenKind.Catch:
                case TokenKind.Until:
                    if (ErrorList.Count > 0)
                        // If we have already seen an error, just eat these tokens.  By eating the token, we won't
                        // generate an odd pipeline with the keyword as a command name, which should provider a better
                        // user experience (e.g., better syntax coloring.)
                        return StatementRule();
                    DynamicKeyword keywordData = DynamicKeyword.GetKeyword(token.Text);
                    statement = DynamicKeywordStatementRule(token, keywordData);
                case TokenKind.Class:
                    statement = ClassDefinitionRule(attributes, token);
                case TokenKind.Enum:
                    statement = EnumDefinitionRule(attributes, token);
                    statement = UsingStatementRule(token);
                    // Report an error - usings must appear before anything else in the script, but parse it anyway
                    ReportError(statement.Extent,
                        nameof(ParserStrings.UsingMustBeAtStartOfScript),
                        ParserStrings.UsingMustBeAtStartOfScript);
        private StringConstantExpressionAst SimpleNameRule()
            return SimpleNameRule(out token);
        private StringConstantExpressionAst SimpleNameRule(out Token token)
                _tokenizer.WantSimpleName = true;
                _tokenizer.WantSimpleName = false;
                // We mark the token as a member name even though it may be a loop name or data variable name.
                // V2 did this, so it's at least backwards compatible.
                token.TokenFlags |= TokenFlags.MemberName;
                return new StringConstantExpressionAst(token.Extent, token.Text, StringConstantType.BareWord);
        private ExpressionAst LabelOrKeyRule()
            // G  label-expression:
            // G      simple-name
            // G      unary-expression
            // G  key-expression:
            var simpleName = SimpleNameRule();
            if (simpleName != null)
                return simpleName;
            if (token.Kind != TokenKind.NewLine && token.Kind != TokenKind.Semi)
                ExpressionAst labelExpr;
                bool disableCommaOperator = _disableCommaOperator;
                    labelExpr = UnaryExpressionRule();
                    _disableCommaOperator = disableCommaOperator;
                if (labelExpr != null)
                    return labelExpr;
                // If this is a label, then any token other than EOF here is effectively unreachable code.  E.g.
                //         break & "cmd"
                // Gets parsed as:
                //         break ; & "cmd"
        private BreakStatementAst BreakStatementRule(Token breakToken)
            ExpressionAst labelExpr = LabelOrKeyRule();
            IScriptExtent extent = (labelExpr != null)
                ? ExtentOf(breakToken, labelExpr)
                : breakToken.Extent;
            return new BreakStatementAst(extent, labelExpr);
        private ContinueStatementAst ContinueStatementRule(Token continueToken)
                ? ExtentOf(continueToken, labelExpr)
                : continueToken.Extent;
            return new ContinueStatementAst(extent, labelExpr);
        private ReturnStatementAst ReturnStatementRule(Token token)
            // G      'return'   pipeline-chain:opt
            PipelineBaseAst pipeline = PipelineChainRule();
            IScriptExtent extent = (pipeline != null)
                ? ExtentOf(token, pipeline)
                : token.Extent;
            return new ReturnStatementAst(extent, pipeline);
        private ExitStatementAst ExitStatementRule(Token token)
            // G      'exit'   pipeline-chain:opt
            return new ExitStatementAst(extent, pipeline);
        private ThrowStatementAst ThrowStatementRule(Token token)
            return new ThrowStatementAst(extent, pipeline);
        private StatementAst LabeledStatementRule(LabelToken label)
            // G      pipeline-chain
                    statement = SwitchStatementRule(label, token);
                    statement = ForeachStatementRule(label, token);
                    statement = ForStatementRule(label, token);
                    statement = WhileStatementRule(label, token);
                    statement = DoWhileStatementRule(label, token);
                    // We can only unget 1 token, but have 2 to unget, so resync on the label.
                    Resync(label);
        private StatementAst BlockStatementRule(Token kindToken)
            // G block-statement
            // G      keyword    statement-block
            StatementBlockAst body = StatementBlockRule();
            // ErrorRecovery: nothing more to look for, so just return the error statement.
            if (body == null)
                ReportIncompleteInput(After(kindToken.Extent),
                    nameof(ParserStrings.MissingStatementAfterKeyword),
                    ParserStrings.MissingStatementAfterKeyword,
                    kindToken.Text);
                return new ErrorStatementAst(ExtentOf(kindToken, kindToken));
            return new BlockStatementAst(ExtentOf(kindToken, body), kindToken, body);
        /// Handle the InlineScript syntax in the script workflow.
        /// <param name="inlineScriptToken"></param>
        /// <param name="elements"></param>
        /// true  -- InlineScript parsing successful
        /// false -- InlineScript parsing unsuccessful
        private bool InlineScriptRule(Token inlineScriptToken, List<CommandElementAst> elements)
            // G Command
            // G      InlineScript    scriptblock-expression
            Diagnostics.Assert(elements != null && elements.Count == 0, "The CommandElement list should be empty");
            var commandName = new StringConstantExpressionAst(inlineScriptToken.Extent, inlineScriptToken.Text, StringConstantType.BareWord);
            inlineScriptToken.TokenFlags |= TokenFlags.CommandName;
            elements.Add(commandName);
                // ErrorRecovery: If there is no opening curly, assume it hasn't been entered yet and don't consume anything.
                ReportIncompleteInput(After(inlineScriptToken),
                    inlineScriptToken.Text);
            var expr = ScriptBlockExpressionRule(lCurly);
            elements.Add(expr);
        private StatementAst IfStatementRule(Token ifToken)
            // G  if-statement:
            // G      'if'   new-lines:opt   '('   pipeline-chain   ')'   statement-block   elseif-clauses:opt   else-clause:opt
            // G  elseif-clauses:
            // G      elseif-clause
            // G      elseif-clauses   elseif-clause
            // G  elseif-clause:
            // G      'elseif'   new-lines:opt   '('   pipeline-chain   ')'   statement-block
            // G  else-clause:
            // G      'else'   statement-block
            List<IfClause> clauses = new List<IfClause>();
            List<Ast> componentAsts = new List<Ast>();
            StatementBlockAst elseClause = null;
            Token keyword = ifToken;
                Token lParen = NextToken();
                if (lParen.Kind != TokenKind.LParen)
                    // ErrorRecovery: assume user just typed 'if' or 'elseif' and hadn't started typing anything
                    // else yet.  Next token is likely a newline, so just put it back and keep parsing.
                    UngetToken(lParen);
                    ReportIncompleteInput(After(keyword),
                        nameof(ParserStrings.MissingOpenParenthesisInIfStatement),
                        ParserStrings.MissingOpenParenthesisInIfStatement,
                        keyword.Text);
                    return new ErrorStatementAst(ExtentOf(ifToken, keyword), componentAsts);
                PipelineBaseAst condition = PipelineChainRule();
                    // ErrorRecovery: assume pipeline just hasn't been entered yet, continue hoping
                    // to find a close paren and statement block.
                    IScriptExtent errorPosition = After(lParen);
                        nameof(ParserStrings.IfStatementMissingCondition),
                        ParserStrings.IfStatementMissingCondition,
                    condition = new ErrorStatementAst(errorPosition);
                    componentAsts.Add(condition);
                    // ErrorRecovery: assume the next token is a newline or part of something else,
                    // so stop parsing the statement and try parsing something else if possible.
                    // Don't bother reporting this error if we already reported an empty condition error.
                    if (condition is not ErrorStatementAst)
                        ReportIncompleteInput(rParen.Extent,
                            nameof(ParserStrings.MissingEndParenthesisAfterStatement),
                            ParserStrings.MissingEndParenthesisAfterStatement,
                    return new ErrorStatementAst(ExtentOf(ifToken, Before(rParen)), componentAsts);
                    // so stop parsing the statement and try parsing something else.
                        nameof(ParserStrings.MissingStatementBlock),
                        ParserStrings.MissingStatementBlock,
                    return new ErrorStatementAst(ExtentOf(ifToken, rParen), componentAsts);
                componentAsts.Add(body);
                clauses.Add(new IfClause(condition, body));
                // Save a restore point here. In case there is no 'elseif' or 'else' following,
                // we should resync back here to preserve the possible new lines. The new lines
                // could be important for the following parsing. For example, in case we are in
                // a HashExpression, a new line might be needed for parsing the key-value that
                // is following the if statement:
                //    @{
                //       a = if (1) {}
                //       b = 10
                int restorePoint = _ungotToken == null ? _tokenizer.GetRestorePoint() : _ungotToken.Extent.StartOffset;
                keyword = PeekToken();
                if (keyword.Kind == TokenKind.ElseIf)
                else if (keyword.Kind == TokenKind.Else)
                    elseClause = StatementBlockRule();
                    if (elseClause == null)
                            nameof(ParserStrings.MissingStatementBlockAfterElse),
                            ParserStrings.MissingStatementBlockAfterElse);
                    // There is no 'elseif' or 'else' following, so resync back to the possible new lines.
            IScriptExtent endExtent = (elseClause != null)
                ? elseClause.Extent
                : clauses[clauses.Count - 1].Item2.Extent;
            IScriptExtent extent = ExtentOf(ifToken, endExtent);
            return new IfStatementAst(extent, clauses, elseClause);
        private StatementAst SwitchStatementRule(LabelToken labelToken, Token switchToken)
            // G  switch-statement:
            // G      'switch'   new-lines:opt   switch-parameters:opt   switch-condition   switch-body
            // G  switch-parameters:
            // G      switch-parameter
            // G      switch-parameters   switch-parameter
            // G  switch-parameter:
            // G      '-regex'
            // G      '-wildcard'
            // G      '-exact'
            // G      '-casesensitive'
            // G      '-parallel'
            // G  switch-condition:
            // G      '('   new-lines:opt   pipeline   new-lines:opt   ')'
            // G      -file   new-lines:opt   switch-filename
            // G  switch-filename:
            // G      command-argument
            // G      primary-expression
            // G  switch-body:
            // G      new-lines:opt   '{'   new-lines:opt   switch-clauses   '}'
            // G  switch-clauses:
            // G      switch-clause
            // G      switch-clauses   switch-clause
            // G  switch-clause:
            // G      switch-clause-condition   statement-block   statement-terminators:opt
            // G  switch-clause-condition:
            IScriptExtent startExtent = (labelToken ?? switchToken).Extent;
            IScriptExtent endErrorStatement = startExtent;
            bool isError = false;
            bool isIncompleteError = false;
            bool needErrorCondition = false; // Only used to track if we need to include (condition) ast for the error statement.
            PipelineBaseAst condition = null;
            Dictionary<string, Tuple<Token, Ast>> specifiedFlags = null; // Only used to track all flags specified for the error ast
            Token switchParameterToken = PeekToken();
            SwitchFlags flags = SwitchFlags.None;
            while (switchParameterToken.Kind == TokenKind.Parameter)
                endErrorStatement = switchParameterToken.Extent;
                specifiedFlags ??= new Dictionary<string, Tuple<Token, Ast>>();
                if (IsSpecificParameter(switchParameterToken, "regex"))
                    flags |= SwitchFlags.Regex;
                    flags &= ~SwitchFlags.Wildcard;
                    if (!specifiedFlags.ContainsKey("regex"))
                        specifiedFlags.Add("regex", new Tuple<Token, Ast>(switchParameterToken, null));
                else if (IsSpecificParameter(switchParameterToken, "wildcard"))
                    flags |= SwitchFlags.Wildcard;
                    flags &= ~SwitchFlags.Regex;
                    if (!specifiedFlags.ContainsKey("wildcard"))
                        specifiedFlags.Add("wildcard", new Tuple<Token, Ast>(switchParameterToken, null));
                else if (IsSpecificParameter(switchParameterToken, "exact"))
                    if (!specifiedFlags.ContainsKey("exact"))
                        specifiedFlags.Add("exact", new Tuple<Token, Ast>(switchParameterToken, null));
                else if (IsSpecificParameter(switchParameterToken, "casesensitive"))
                    flags |= SwitchFlags.CaseSensitive;
                    if (!specifiedFlags.ContainsKey("casesensitive"))
                        specifiedFlags.Add("casesensitive", new Tuple<Token, Ast>(switchParameterToken, null));
                else if (IsSpecificParameter(switchParameterToken, "parallel"))
                    flags |= SwitchFlags.Parallel;
                    if (!specifiedFlags.ContainsKey("parallel"))
                        specifiedFlags.Add("parallel", new Tuple<Token, Ast>(switchParameterToken, null));
                else if (IsSpecificParameter(switchParameterToken, "file"))
                    flags |= SwitchFlags.File;
                    ExpressionAst fileNameExpr = GetSingleCommandArgument(CommandArgumentContext.FileName);
                    if (fileNameExpr == null)
                        // ErrorRecovery: pretend we saw the filename and continue.
                        isError = true;
                        isIncompleteError = ReportIncompleteInput(After(switchParameterToken),
                            nameof(ParserStrings.MissingFilenameOption),
                            ParserStrings.MissingFilenameOption);
                        if (!specifiedFlags.ContainsKey("file"))
                            specifiedFlags.Add("file", new Tuple<Token, Ast>(switchParameterToken, null));
                        endErrorStatement = fileNameExpr.Extent;
                        condition = new PipelineAst(fileNameExpr.Extent,
                                                    new CommandExpressionAst(fileNameExpr.Extent, fileNameExpr, null), background: false);
                            specifiedFlags.Add("file", new Tuple<Token, Ast>(switchParameterToken, condition));
                    // ErrorRecovery: just ignore the token, continue parsing.
                    ReportError(switchParameterToken.Extent,
                        nameof(ParserStrings.InvalidSwitchFlag),
                        ParserStrings.InvalidSwitchFlag,
                        ((ParameterToken)switchParameterToken).ParameterName);
                switchParameterToken = PeekToken();
            if (switchParameterToken.Kind == TokenKind.Minus)
                specifiedFlags.Add(VERBATIM_ARGUMENT, new Tuple<Token, Ast>(switchParameterToken, null));
            Token lParen = PeekToken();
            if (lParen.Kind == TokenKind.LParen)
                endErrorStatement = lParen.Extent;
                if ((flags & SwitchFlags.File) == SwitchFlags.File)
                    // ErrorRecovery: nothing special this is a semantic error.
                    ReportError(lParen.Extent,
                        nameof(ParserStrings.PipelineValueRequired),
                        ParserStrings.PipelineValueRequired);
                needErrorCondition = true; // need to add condition ast to the error statement if the parsing fails
                condition = PipelineChainRule();
                    // ErrorRecovery: pretend we saw the condition and keep parsing.
                    isIncompleteError = ReportIncompleteInput(After(lParen),
                    endErrorStatement = condition.Extent;
                    // ErrorRecovery: Try to parse the switch body, if we don't find a body, then bail.
                    if (!isIncompleteError)
                        isIncompleteError =
                            ReportIncompleteInput(After(endErrorStatement),
                                                  nameof(ParserStrings.MissingEndParenthesisInSwitchStatement),
                                                  ParserStrings.MissingEndParenthesisInSwitchStatement);
                    endErrorStatement = rParen.Extent;
            else if (condition == null)
                if ((flags & SwitchFlags.File) == 0)
                    isIncompleteError = ReportIncompleteInput(After(endErrorStatement),
                    Diagnostics.Assert(isError, "An error should already have been issued");
            StatementBlockAst @default = null;
            List<SwitchClause> clauses = new List<SwitchClause>();
            List<Ast> errorAsts = new List<Ast>();  // in case there is an error, we want the asts parsed up to the error.
            Token rCurly = null;
                // ErrorRecovery: Assume we don't have any switch body to parse.
                        nameof(ParserStrings.MissingCurlyBraceInSwitchStatement),
                        ParserStrings.MissingCurlyBraceInSwitchStatement);
                endErrorStatement = lCurly.Extent;
                    bool isDefaultClause = token.Kind == TokenKind.Default;
                    ExpressionAst clauseCondition = null;
                    if (isDefaultClause)
                        // Consume the 'default' token.
                        clauseCondition = new StringConstantExpressionAst(token.Extent, token.Text, StringConstantType.BareWord);
                        clauseCondition = GetSingleCommandArgument(CommandArgumentContext.SwitchCondition);
                        if (clauseCondition == null)
                            // ErrorRecovery: if we don't have anything that looks like a condition, we won't
                            // find a body (because a body is just a script block, which works as a condition.)
                            // So don't look for a body, hope we find the '}' next.
                                nameof(ParserStrings.MissingSwitchConditionExpression),
                                ParserStrings.MissingSwitchConditionExpression);
                            // Consume a closing curly, if there is one, to avoid an extra error
                            if (PeekToken().Kind == TokenKind.RCurly)
                    errorAsts.Add(clauseCondition);
                    endErrorStatement = clauseCondition.Extent;
                    StatementBlockAst clauseBody = StatementBlockRule();
                    if (clauseBody == null)
                        // ErrorRecovery: We might find another condition/body pair, so keep going.
                                                                  nameof(ParserStrings.MissingSwitchStatementClause),
                                                                  ParserStrings.MissingSwitchStatementClause);
                        errorAsts.Add(clauseBody);
                        endErrorStatement = clauseBody.Extent;
                            if (@default != null)
                                // ErrorRecovery: just report the error and continue, forget the previous default clause.
                                ReportError(clauseCondition.Extent,
                                    nameof(ParserStrings.MultipleSwitchDefaultClauses),
                                    ParserStrings.MultipleSwitchDefaultClauses);
                            @default = clauseBody;
                            clauses.Add(new SwitchClause(clauseCondition, clauseBody));
                    if (token.Kind == TokenKind.RCurly)
                        rCurly = token;
                    if (token.Kind == TokenKind.EndOfInput)
                                token.Extent,
            if (isError)
                return new ErrorStatementAst(ExtentOf(startExtent, endErrorStatement), switchToken, specifiedFlags,
                                             needErrorCondition ? GetNestedErrorAsts(condition) : null, GetNestedErrorAsts(errorAsts));
            return new SwitchStatementAst(ExtentOf(labelToken ?? switchToken, rCurly),
                labelToken?.LabelText, condition, flags, clauses, @default);
        private StatementAst ConfigurationStatementRule(IEnumerable<AttributeAst> customAttributes, Token configurationToken)
            // G  configuration-statement:
            // G      'configuration'   new-lines:opt  singleNameExpression  new-lines:opt statement-block
            // G  singleNameExpression:
            IScriptExtent startExtent = configurationToken.Extent;
            // The expression that returns the configuration name.
            ExpressionAst configurationName;
            string simpleConfigurationNameValue = null;
            Token configurationNameToken = NextToken();
            Token configurationKeywordToken = configurationNameToken;
            if (configurationNameToken.Kind == TokenKind.LCurly)
                ReportError(After(startExtent),
                    nameof(ParserStrings.MissingConfigurationName),
                    ParserStrings.MissingConfigurationName);
                // Try reading the configuration body - this should keep the parse in sync - but we won't return it
                ScriptBlockExpressionRule(configurationNameToken);
            if (configurationNameToken.Kind is TokenKind.EndOfInput or TokenKind.Comma)
                UngetToken(configurationNameToken);
                ReportIncompleteInput(After(configurationNameToken.Extent),
            // Unget the configuration token so it can possibly be re-read as part of an expression
            // Finally read the name for this configuration
            configurationName = GetWordOrExpression(configurationNameToken);
            if (configurationName == null)
                ReportIncompleteInput(configurationNameToken.Extent,
                object outValue;
                if (IsConstantValueVisitor.IsConstant(configurationName, out outValue))
                    simpleConfigurationNameValue = outValue as string;
                    if (simpleConfigurationNameValue == null ||
                        !System.Text.RegularExpressions.Regex.IsMatch(simpleConfigurationNameValue, "^[A-Za-z][A-Za-z0-9_./-]*$"))
                        // This is actually a semantics check, the syntax is fine at this point.
                        // Continue parsing to get as much information as possible
                        ReportError(configurationName.Extent,
                            nameof(ParserStrings.InvalidConfigurationName),
                            ParserStrings.InvalidConfigurationName,
                            simpleConfigurationNameValue ?? string.Empty);
            // Load the system classes and import them as keywords
            Runspaces.Runspace localRunspace = null;
            bool topLevel = false;
                // At this point, we'll need a runspace to use to hold the metadata for the parse. If there is no
                // current runspace to use, we create one and set it to be the default for this thread...
                    localRunspace =
                        Runspaces.RunspaceFactory.CreateRunspace(Runspaces.InitialSessionState.CreateDefault2());
                    localRunspace.ThreadOptions = Runspaces.PSThreadOptions.UseCurrentThread;
                    localRunspace.Open();
                    Runspaces.Runspace.DefaultRunspace = localRunspace;
                // Configuration is not supported in ConstrainedLanguage
                if (Runspace.DefaultRunspace?.ExecutionContext?.LanguageMode == PSLanguageMode.ConstrainedLanguage)
                        ReportError(configurationToken.Extent,
                                    nameof(ParserStrings.ConfigurationNotAllowedInConstrainedLanguage),
                                    ParserStrings.ConfigurationNotAllowedInConstrainedLanguage,
                                    configurationToken.Kind.Text());
                        context: Runspace.DefaultRunspace?.ExecutionContext,
                        title: ParserStrings.WDACParserConfigKeywordLogTitle,
                        message: ParserStrings.WDACParserConfigKeywordLogMessage,
                        fqid: "ConfigurationLanguageKeywordNotAllowed",
                // Configuration is not supported for ARM or ARM64 process architecture.
                if (PsUtils.IsRunningOnProcessArchitectureARM())
                        configurationToken.Extent,
                        nameof(ParserStrings.ConfigurationNotAllowedOnArm64),
                        ParserStrings.ConfigurationNotAllowedOnArm64,
                // Configuration is not supported on WinPE
                if (Utils.IsWinPEHost())
                                nameof(ParserStrings.ConfigurationNotAllowedOnWinPE),
                                ParserStrings.ConfigurationNotAllowedOnWinPE,
                ExpressionAst configurationBodyScriptBlock = null;
                PowerShell p = null;
                // Save the parser we're using so we can resume the current parse when we're done.
                var currentParser = Runspaces.Runspace.DefaultRunspace.ExecutionContext.Engine.EngineParser;
                Runspaces.Runspace.DefaultRunspace.ExecutionContext.Engine.EngineParser = new Parser();
                        p = PowerShell.Create();
                        p.Runspace = localRunspace;
                        p = PowerShell.Create(RunspaceMode.CurrentRunspace);
                        // See of the default CIM keywords are already loaded. If they haven't been
                        // then this is the top level. Record that information and then load the defaults
                        // keywords.
                        if (DynamicKeyword.GetKeyword("OMI_ConfigurationDocument") == null)
                            // Load the default CIM keywords
                            Collection<Exception> CIMKeywordErrors = new Collection<Exception>();
                            // DscSubsystem is auto-registered when PSDesiredStateConfiguration v3 module is loaded
                            // so if DscSubsystem is registered that means user intention to use v3 APIs.
                                dscSubsystem.LoadDefaultKeywords(CIMKeywordErrors);
                                Dsc.DscClassCache.LoadDefaultCimKeywords(CIMKeywordErrors);
                            // Report any errors encountered while loading CIM dynamic keywords.
                            if (CIMKeywordErrors.Count > 0)
                                ReportErrorsAsWarnings(CIMKeywordErrors);
                            // Load any keywords that have been defined earlier in the script.
                            if (_configurationKeywordsDefinedInThisFile != null)
                                foreach (var kw in _configurationKeywordsDefinedInThisFile.Values)
                                    if (!DynamicKeyword.ContainsKeyword(kw.Keyword))
                                        DynamicKeyword.AddKeyword(kw);
                            topLevel = true;
                        // This shouldn't happen - the system classes should always be good, but just in case,
                        // we'll catch the exception and report it as an error.
                        ReportError(configurationKeywordToken.Extent,
                            nameof(ParserStrings.ParserError),
                            ParserStrings.ParserError,
                    p?.Dispose();
                    // Put the parser back...
                    Runspaces.Runspace.DefaultRunspace.ExecutionContext.Engine.EngineParser = currentParser;
                    ReportIncompleteInput(After(lCurly.Extent),
                        nameof(ParserStrings.MissingCurlyInConfigurationStatement),
                        ParserStrings.MissingCurlyInConfigurationStatement);
                    var oldInConfiguration = _inConfiguration;
                        _inConfiguration = true;
                        configurationBodyScriptBlock = ScriptBlockExpressionRule(lCurly);
                        _inConfiguration = oldInConfiguration;
                    if (configurationBodyScriptBlock == null)
                        ReportError(After(lCurly.Extent),
                            nameof(ParserStrings.ConfigurationBodyEmpty),
                            ParserStrings.ConfigurationBodyEmpty);
                    return new ErrorStatementAst(ExtentOf(startExtent, endErrorStatement), configurationToken);
                #region "Add Configuration Keywords"
                // If the configuration name is a constant string, then
                // if we're not at the top level, we'll add it to the list of configuration resource keywords.
                // If we are at the top level, then we'll add it to the list of keywords defined in this
                // parse so it can be used as a resource in subsequent config statements.
                var scAst = configurationName as StringConstantExpressionAst;
                if (scAst != null)
                    var keywordToAddForThisConfigurationStatement = new System.Management.Automation.Language.DynamicKeyword
                        ImplementingModule = _keywordModuleName,
                        Keyword = scAst.Value,
                        DirectCall = true,
                    // Add the DependsOn property.
                    var dependsOnProp = new DynamicKeywordProperty
                        Name = "DependsOn",
                    keywordToAddForThisConfigurationStatement.Properties.Add(dependsOnProp.Name, dependsOnProp);
                    // Add the PsDscRunAsCredential property.
                    var RunAsProp = new DynamicKeywordProperty
                        Name = "PsDscRunAsCredential",
                    keywordToAddForThisConfigurationStatement.Properties.Add(RunAsProp.Name, RunAsProp);
                    // Extract the parameters, if any and them to the keyword definition.
                    var sbeAst = configurationBodyScriptBlock as ScriptBlockExpressionAst;
                    if (sbeAst != null)
                        var pList = sbeAst.ScriptBlock.ParamBlock;
                        if (pList != null)
                            foreach (var parm in pList.Parameters)
                                var keywordProp = new DynamicKeywordProperty();
                                keywordProp.Name = parm.Name.VariablePath.UserPath;
                                if (parm.Attributes != null)
                                    foreach (var attr in parm.Attributes)
                                        var typeConstraint = attr as TypeConstraintAst;
                                            keywordProp.TypeConstraint = typeConstraint.TypeName.Name;
                                        var aAst = attr as AttributeAst;
                                        if (aAst != null)
                                            if (string.Equals(aAst.TypeName.Name, "Parameter", StringComparison.OrdinalIgnoreCase))
                                                if (aAst.NamedArguments != null)
                                                    foreach (var na in aAst.NamedArguments)
                                                        if (string.Equals(na.ArgumentName, "Mandatory", StringComparison.OrdinalIgnoreCase))
                                                            if (na.ExpressionOmitted)
                                                                keywordProp.Mandatory = true;
                                                            else if (na.Argument != null)
                                                                ConstantExpressionAst ceAst = na.Argument as ConstantExpressionAst;
                                                                if (ceAst != null)
                                                                    keywordProp.Mandatory = System.Management.Automation.LanguagePrimitives.IsTrue(ceAst.Value);
                                keywordToAddForThisConfigurationStatement.Properties.Add(keywordProp.Name, keywordProp);
                    if (topLevel)
                        _configurationKeywordsDefinedInThisFile ??= new Dictionary<string, DynamicKeyword>();
                        _configurationKeywordsDefinedInThisFile[keywordToAddForThisConfigurationStatement.Keyword] = keywordToAddForThisConfigurationStatement;
                        System.Management.Automation.Language.DynamicKeyword.AddKeyword(keywordToAddForThisConfigurationStatement);
                // End of dynamic keyword definition for this function...
                //############################################################
                bool isMetaConfiguration = false;
                if (customAttributes != null)
                    isMetaConfiguration = customAttributes.Any(
                         attribute => (attribute.TypeName.GetReflectionAttributeType() != null) &&
                                      (attribute.TypeName.GetReflectionAttributeType() == typeof(DscLocalConfigurationManagerAttribute)));
                ScriptBlockExpressionAst bodyAst = configurationBodyScriptBlock as ScriptBlockExpressionAst;
                IScriptExtent configurationExtent = ExtentOf(startExtent, bodyAst);
                return new ConfigurationDefinitionAst(configurationExtent,
                    bodyAst,
                    isMetaConfiguration ? ConfigurationType.Meta : ConfigurationType.Resource,
                    configurationName)
                    LCurlyToken = lCurly,
                    ConfigurationToken = configurationToken,
                    CustomAttributes = customAttributes,
                    DefinedKeywords = DynamicKeyword.GetKeyword()
                // In theory this should never happen so if it does, we'll report the actual exception rather than introducing a new message
                    "ConfigurationStatementToken: " + e);
                // If we had to allocate a runspace for the parser, we'll free it now.
                    // If a runspace was created for this operation, close it and reset the default runspace to null.
                    localRunspace.Close();
                    Runspaces.Runspace.DefaultRunspace = null;
                    // Clear out all of the cached classes and keywords.
                    // They will need to be reloaded when the generated function is actually run.
                        dscSubsystem.ClearCache();
                        Dsc.DscClassCache.ClearCache();
                    System.Management.Automation.Language.DynamicKeyword.Reset();
                // Finally resync the tokenizer at the current position
                // this will flush any cached dynamic keyword tokens.
        private Dictionary<string, DynamicKeyword> _configurationKeywordsDefinedInThisFile;
        /// Reads an argument expression for a keyword or keyword parameter.
        /// This can be either a bare word or an expression.
        /// <param name="keywordToken">The token of the associated keyword.</param>
        private ExpressionAst GetWordOrExpression(Token keywordToken)
            Token nameToken = NextToken();
            if (nameToken.Kind == TokenKind.EndOfInput)
                // ErrorRecovery: report incomplete statement and return
                UngetToken(nameToken);
                ReportIncompleteInput(After(keywordToken),
                    nameof(ParserStrings.RequiredNameOrExpressionMissing),
                    ParserStrings.RequiredNameOrExpressionMissing);
            ExpressionAst argument = GetCommandArgument(CommandArgumentContext.CommandArgument, nameToken);
                var extent = keywordToken.Extent;
                ReportError(After(extent),
                    nameof(ParserStrings.ParameterRequiresArgument),
                    ParserStrings.ParameterRequiresArgument,
                    keywordToken.Text);
        private StatementAst ForeachStatementRule(LabelToken labelToken, Token forEachToken)
            // G  foreach-statement:
            // G      'foreach'   new-lines:opt   foreach-parameters:opt   new-lines:opt   '('
            // G          new-lines:opt   variable   new-lines:opt   'in'   new-lines:opt   pipeline
            // G          new-lines:opt   ')'   statement-block
            // G  foreach-parameters:
            // G      foreach-parameter
            // G      foreach-parameters   foreach-parameter
            // G  foreach-parameter:
            // G      '-throttlelimit' new-lines:opt   foreach-throttlelimit
            // G  foreach-throttlelimit:
            IScriptExtent startOfStatement = labelToken != null ? labelToken.Extent : forEachToken.Extent;
            IScriptExtent endErrorStatement = null;
            // Process parameters on foreach
            Token foreachParameterToken = PeekToken();
            ForEachFlags flags = ForEachFlags.None;
            ExpressionAst throttleLimit = null;
            while (foreachParameterToken.Kind == TokenKind.Parameter)
                if (IsSpecificParameter(foreachParameterToken, "parallel"))
                    flags |= ForEachFlags.Parallel;
                else if (IsSpecificParameter(foreachParameterToken, "throttlelimit"))
                    throttleLimit = GetSingleCommandArgument(CommandArgumentContext.CommandArgument);
                    if (throttleLimit == null)
                        // ErrorRecovery: pretend we saw the throttle limit and continue.
                        ReportIncompleteInput(After(foreachParameterToken),
                            nameof(ParserStrings.MissingThrottleLimit),
                            ParserStrings.MissingThrottleLimit);
                    endErrorStatement = foreachParameterToken.Extent;
                    ReportError(foreachParameterToken.Extent,
                        nameof(ParserStrings.InvalidForeachFlag),
                        ParserStrings.InvalidForeachFlag,
                        ((ParameterToken)foreachParameterToken).ParameterName);
                foreachParameterToken = PeekToken();
                // ErrorRecovery: assume the rest of the statement is missing.
                endErrorStatement = forEachToken.Extent;
                    nameof(ParserStrings.MissingOpenParenthesisAfterKeyword),
                    ParserStrings.MissingOpenParenthesisAfterKeyword,
                    forEachToken.Kind.Text());
                return new ErrorStatementAst(ExtentOf(startOfStatement, endErrorStatement));
                    nameof(ParserStrings.MissingVariableNameAfterForeach),
                    ParserStrings.MissingVariableNameAfterForeach);
            var variableAst = new VariableExpressionAst((VariableToken)token);
            PipelineBaseAst pipeline = null;
            StatementBlockAst body = null;
            Token inToken = NextToken();
            if (inToken.Kind != TokenKind.In)
                UngetToken(inToken);
                endErrorStatement = variableAst.Extent;
                    nameof(ParserStrings.MissingInInForeach),
                    ParserStrings.MissingInInForeach);
                pipeline = PipelineChainRule();
                    endErrorStatement = inToken.Extent;
                        nameof(ParserStrings.MissingForeachExpression),
                        ParserStrings.MissingForeachExpression);
                        endErrorStatement = pipeline.Extent;
                            nameof(ParserStrings.MissingEndParenthesisAfterForeach),
                            ParserStrings.MissingEndParenthesisAfterForeach);
                        body = StatementBlockRule();
                                nameof(ParserStrings.MissingForeachStatement),
                                ParserStrings.MissingForeachStatement);
            if (endErrorStatement != null)
                return new ErrorStatementAst(ExtentOf(startOfStatement, endErrorStatement),
                                             GetNestedErrorAsts(variableAst, pipeline, body));
            return new ForEachStatementAst(ExtentOf(startOfStatement, body),
                labelToken?.LabelText,
                throttleLimit, variableAst, pipeline, body);
        private StatementAst ForStatementRule(LabelToken labelToken, Token forToken)
            // G  for-statement:
            // G      'for'   new-lines:opt   '('
            // G            new-lines:opt   for-initializer:opt   statement-terminator
            // G            new-lines:opt   for-condition:opt   statement-terminator
            // G            new-lines:opt   for-iterator:opt
            // G            new-lines:opt   ')'   statement-block
            // G            new-lines:opt   for-condition:opt
            // G            new-lines:opt   for-initializer:opt
            // G  for-initializer:
            // G  for-condition:
            // G  for-iterator:
                // ErrorRecovery: don't continue parsing the for statement.
                endErrorStatement = forToken.Extent;
                    forToken.Kind.Text());
                return new ErrorStatementAst(ExtentOf(labelToken ?? forToken, endErrorStatement));
            PipelineBaseAst initializer = PipelineChainRule();
                endErrorStatement = initializer.Extent;
            if (PeekToken().Kind == TokenKind.Semi)
                endErrorStatement = NextToken().Extent;
            PipelineBaseAst iterator = PipelineChainRule();
            if (iterator != null)
                endErrorStatement = iterator.Extent;
                endErrorStatement ??= lParen.Extent;
                    // ErrorRecovery: return an error statement.
                                          nameof(ParserStrings.MissingLoopStatement),
                                          ParserStrings.MissingLoopStatement,
                return new ErrorStatementAst(ExtentOf(labelToken ?? forToken, endErrorStatement),
                    GetNestedErrorAsts(initializer, condition, iterator));
            return new ForStatementAst(ExtentOf(labelToken ?? forToken, body),
                labelToken?.LabelText, initializer, condition, iterator, body);
        private StatementAst WhileStatementRule(LabelToken labelToken, Token whileToken)
            // G  while-statement:
            // G      'while '  new-lines:opt   '('   new-lines:opt   while-condition   new-lines:opt   ')'   statement-block
            // G   while-condition:
            // G       pipeline-chain
                // ErrorRecovery: assume user just typed 'while' and hadn't started typing anything
                ReportIncompleteInput(After(whileToken),
                    whileToken.Text);
                return new ErrorStatementAst(ExtentOf(labelToken ?? whileToken, whileToken));
            PipelineBaseAst errorCondition = null;
                    nameof(ParserStrings.MissingExpressionAfterKeyword),
                    ParserStrings.MissingExpressionAfterKeyword,
                    whileToken.Kind.Text());
                errorCondition = condition;
                    ReportIncompleteInput(After(condition),
                return new ErrorStatementAst(ExtentOf(labelToken ?? whileToken, condition), GetNestedErrorAsts(errorCondition));
                // ErrorRecovery: assume the next token is a newline or part of something else.
                return new ErrorStatementAst(ExtentOf(labelToken ?? whileToken, rParen), GetNestedErrorAsts(errorCondition));
            return new WhileStatementAst(ExtentOf(labelToken ?? whileToken, body),
                labelToken?.LabelText, condition, body);
        /// Parse a dynamic keyword statement which will be either of the form
        ///     keyword [parameters] [name] { a=1; b=2; } # constructor with properties
        ///     keyword [parameters] [name] { ... }  # constructor with a simple body.
        /// or keywordcommand parameters
        /// This custom keyword does not introduce a new AST node type. Instead it generates a
        /// CommandAst that calls a PowerShell command to implement the keyword's logic.
        /// This command has one of two signatures:
        ///     keywordImplCommand.
        /// <param name="functionName">The name of the function to invoke.</param>
        /// <param name="keywordData">The data for this keyword definition.</param>
        private StatementAst DynamicKeywordStatementRule(Token functionName, DynamicKeyword keywordData)
            // If a custom action was provided. then invoke it
            if (keywordData.PreParse != null)
                    ParseError[] errors = keywordData.PreParse(keywordData);
                        foreach (var e in errors)
                            ReportError(e);
                    ReportError(functionName.Extent,
                        nameof(ParserStrings.DynamicKeywordPreParseException),
                        ParserStrings.DynamicKeywordPreParseException,
                        keywordData.ResourceName, e.ToString());
            if (keywordData.IsReservedKeyword)
                // ErrorRecovery: eat the token
                    nameof(ParserStrings.UnsupportedReservedKeyword),
                    ParserStrings.UnsupportedReservedKeyword,
                    keywordData.Keyword);
            if (keywordData.HasReservedProperties)
                    nameof(ParserStrings.UnsupportedReservedProperty),
                    ParserStrings.UnsupportedReservedProperty,
                    "'Require', 'Trigger', 'Notify', 'Before', 'After' and 'Subscribe'");
            string elementName = string.Empty;
            DynamicKeywordStatementAst dynamicKeywordAst;
            if (keywordData.BodyMode == DynamicKeywordBodyMode.Command)
                UngetToken(functionName);
                dynamicKeywordAst = (DynamicKeywordStatementAst)CommandRule(forDynamicKeyword: true);
                dynamicKeywordAst.Keyword = keywordData;
                dynamicKeywordAst.FunctionName = functionName;
                // The expression that returns the resource name or names.
                ExpressionAst instanceName = null;
                    if (keywordData.NameMode == DynamicKeywordNameMode.NameRequired || keywordData.NameMode == DynamicKeywordNameMode.SimpleNameRequired)
                        ReportIncompleteInput(After(functionName),
                        // Name not required so report missing brace
                        ReportIncompleteInput(After(functionName.Extent),
                            nameof(ParserStrings.MissingBraceInObjectDefinition),
                            ParserStrings.MissingBraceInObjectDefinition);
                // If it's an lcurly, then no name was provided, and we skip to the body processing
                Token lCurly = null;
                if (nameToken.Kind == TokenKind.LCurly)
                    lCurly = nameToken;
                        ReportError(After(functionName),
                else if (nameToken.Kind == TokenKind.Identifier || nameToken.Kind == TokenKind.DynamicKeyword)
                    if (keywordData.NameMode == DynamicKeywordNameMode.NoName)
                            nameof(ParserStrings.UnexpectedNameForType),
                            ParserStrings.UnexpectedNameForType,
                            functionName.Text,
                            nameToken.Text);
                    // If it's an identifier then this is the name for the data object
                    elementName = nameToken.Text;
                    // If only a simple name is allowed, then the string must be non-null.
                    if ((keywordData.NameMode == DynamicKeywordNameMode.SimpleNameRequired || keywordData.NameMode == DynamicKeywordNameMode.SimpleOptionalName) && string.IsNullOrEmpty(elementName))
                    // see if an expression was provided instead of a bare word...
                    instanceName = GetSingleCommandArgument(CommandArgumentContext.CommandName);
                    if (instanceName == null)
                        if (keywordData.NameMode == DynamicKeywordNameMode.SimpleNameRequired || keywordData.NameMode == DynamicKeywordNameMode.SimpleOptionalName)
                            // It wasn't an '{' and it wasn't a name expression so it's a unexpected token.
                    // Ok, we got a name expression, but we're expecting no name, so it's and error.
                            instanceName.ToString());
                    // We were expecting a simple name so report an error
                        // If no match, then this is an incomplete token BUGBUG fix message
                        ReportError(nameToken.Extent,
                // If we didn't get a resource expression AST, then we need to build one out of the
                // name that was specified. It may be the case that we don't have
                // a resource name in which case it will be the empty string. Even in the cases were
                // we aren't expecting a name, we still do this so that the signature of the implementing function remains
                ExpressionAst originalInstanceName = instanceName;
                instanceName ??= new StringConstantExpressionAst(nameToken.Extent, elementName, StringConstantType.BareWord);
                // Now look for the body of the data statement.
                if (lCurly == null)
                    lCurly = NextToken();
                    if (lCurly.Kind == TokenKind.EndOfInput)
                        // Preserve the name expression for tab completion
                        return originalInstanceName == null
                                   : new ErrorStatementAst(ExtentOf(functionName, originalInstanceName),
                                                           GetNestedErrorAsts(originalInstanceName));
                        // We need to generate a reasonable error message for this case:
                        // Configuration C {
                        //   node $AllNode.NodeName{ # There is no space before curly, and we are converting scriptblock to and argument to call 'NodeName'
                        // } # we don't want to simple report an unexpected token here, it would be super-confusing.
                        InvokeMemberExpressionAst instanceInvokeMemberExpressionAst = instanceName as InvokeMemberExpressionAst;
                        if (instanceInvokeMemberExpressionAst != null &&
                            instanceInvokeMemberExpressionAst.Arguments.Count == 1 &&
                            instanceInvokeMemberExpressionAst.Arguments[0] is ScriptBlockExpressionAst &&
                            // the last condition checks that there is no space between "method" name and '{'
                            instanceInvokeMemberExpressionAst.Member.Extent.EndOffset == instanceInvokeMemberExpressionAst.Arguments[0].Extent.StartOffset)
                            ReportError(LastCharacterOf(instanceInvokeMemberExpressionAst.Member.Extent),
                                nameof(ParserStrings.UnexpectedTokenInDynamicKeyword),
                                ParserStrings.UnexpectedTokenInDynamicKeyword,
                                functionName.Text);
                            ReportError(lCurly.Extent,
                                lCurly.Text);
                        if (lCurly.Kind == TokenKind.Dot && originalInstanceName != null && lCurly.Extent.StartOffset == originalInstanceName.Extent.EndOffset)
                            // Generate more useful ast for tab-completing extension methods on special DSC collection variables
                            // e.g. configuration foo { node $AllNodes.<tab>
                            IScriptExtent errorExtent = ExtentOf(originalInstanceName, lCurly);
                            var errorExpr = new ErrorExpressionAst(errorExtent);
                            var memberExpr = new MemberExpressionAst(originalInstanceName.Extent, originalInstanceName, errorExpr, @static: false);
                            return new ErrorStatementAst(errorExtent, new[] { memberExpr });
                // The keyword data is used to see
                // if a scriptblock or a hashtable is expected.
                ExpressionAst body = null;
                if (keywordData.BodyMode == DynamicKeywordBodyMode.ScriptBlock)
                        _inConfiguration = false;
                        body = ScriptBlockExpressionRule(lCurly);
                else if (keywordData.BodyMode == DynamicKeywordBodyMode.Hashtable)
                    // Resource property value could be set to nested DSC resources except Script resource
                    bool isScriptResource = string.Equals(functionName.Text, @"Script", StringComparison.OrdinalIgnoreCase);
                        if (isScriptResource)
                        body = HashExpressionRule(lCurly, true /* parsingSchemaElement */);
                // commandast
                // elements: instancename/dynamickeyword/hashtable or scripblockexpress
                    // Failed to read the statement body
                    ReportIncompleteInput(After(lCurly),
                //////////////////////////////////////////////////////////////////////////
                // The statement is now fully parsed
                // Create DynamicKeywordStatementAst
                Collection<CommandElementAst> commandElements = new Collection<CommandElementAst>
                    new StringConstantExpressionAst(functionName.Extent, functionName.Text, StringConstantType.BareWord),
                    (ExpressionAst)instanceName.Copy(),
                    (ExpressionAst)body.Copy()
                Token nextToken = NextToken();
                IScriptExtent dynamicKeywordExtent = ExtentOf(functionName, Before(nextToken));
                UngetToken(nextToken);
                dynamicKeywordAst = new DynamicKeywordStatementAst(dynamicKeywordExtent, commandElements)
                    Keyword = keywordData,
                    LCurly = lCurly,
                    FunctionName = functionName,
                    InstanceName = instanceName,
                    OriginalInstanceName = originalInstanceName,
                    BodyExpression = body,
                    ElementName = elementName,
            if (keywordData.PostParse != null)
                    ParseError[] errors = keywordData.PostParse(dynamicKeywordAst);
                        nameof(ParserStrings.DynamicKeywordPostParseException),
                        ParserStrings.DynamicKeywordPostParseException,
                        keywordData.Keyword,
            return dynamicKeywordAst;
        internal StatementAst CreateErrorStatementAst(
            Token functionName,
            ExpressionAst instanceName,
            ExpressionAst bodyExpression)
            return new ErrorStatementAst(ExtentOf(functionName, bodyExpression), GetNestedErrorAsts(instanceName, bodyExpression));
        private StatementAst DoWhileStatementRule(LabelToken labelToken, Token doToken)
            // G  do-statement:
            // G      'do'   statement-block  new-lines:opt   'while'   new-lines:opt   '('   while-condition   new-lines:opt   ')'
            // G      'do'   statement-block   new-lines:opt   'until'   new-lines:opt   '('   while-condition   new-lines:opt   ')'
            // G  while-condition:
            // G      new-lines:opt   pipeline
            IScriptExtent startExtent = (labelToken ?? doToken).Extent;
            Token rParen = null;
            Token whileOrUntilToken = null;
                // ErrorRecovery: Skip the keyword and stop trying to parse this statement, continue on whatever
                // comes next.
                endErrorStatement = doToken.Extent;
                    TokenKind.Do.Text());
                whileOrUntilToken = NextToken();
                if (whileOrUntilToken.Kind != TokenKind.While && whileOrUntilToken.Kind != TokenKind.Until)
                    // ErrorRecovery: Skip looking for a condition, continue on whatever comes next.
                    UngetToken(whileOrUntilToken);
                    endErrorStatement = body.Extent;
                        nameof(ParserStrings.MissingWhileOrUntilInDoWhile),
                        ParserStrings.MissingWhileOrUntilInDoWhile);
                        // ErrorRecovery: Skip looking for the condition, return an error statement.
                        endErrorStatement = whileOrUntilToken.Extent;
                            whileOrUntilToken.Kind.Text());
                            // ErrorRecovery: try to get the matching close paren, then return an error statement.
                        rParen = NextToken();
                            // ErrorRecovery: this is it, so just pretend we saw the paren, return an error statement.
                            // If condition == null, we issue an error message already, don't bother with this one.
                return new ErrorStatementAst(ExtentOf(startExtent, endErrorStatement), GetNestedErrorAsts(body, condition));
            IScriptExtent extent = ExtentOf(startExtent, rParen);
            string label = labelToken?.LabelText;
            if (whileOrUntilToken.Kind == TokenKind.Until)
                return new DoUntilStatementAst(extent, label, condition, body);
            return new DoWhileStatementAst(extent, label, condition, body);
        private StatementAst ClassDefinitionRule(List<AttributeBaseAst> customAttributes, Token classToken)
            // G  class-statement:
            // G      attribute-list:opt   'class'   new-lines:opt   class-name   new-lines:opt  '{'   class-member-list   '}'
            // G      attribute-list:opt   'class'   new-lines:opt   class-name   new-lines:opt  ':'  base-type-list  '{'  new-lines:opt  class-member-list:opt  '}'
            // G  class-name:
            // G  base-type-list:
            // G      new-lines:opt   type-name   new-lines:opt
            // G      base-class-list  ','   new-lines:opt   type-name   new-lines:opt
            // G  class-member-list:
            // G      class-member  new-lines:opt
            // G      class-member-list   class-member
            // PowerShell classes are not supported in ConstrainedLanguage
                    ReportError(classToken.Extent,
                                nameof(ParserStrings.ClassesNotAllowedInConstrainedLanguage),
                                ParserStrings.ClassesNotAllowedInConstrainedLanguage,
                                classToken.Kind.Text());
                    title: ParserStrings.WDACParserClassKeywordLogTitle,
                    message: ParserStrings.WDACParserClassKeywordLogMessage,
                    fqid: "ClassLanguageKeywordNotAllowed",
            Token classNameToken;
            var name = SimpleNameRule(out classNameToken);
                ReportIncompleteInput(After(classToken),
                    nameof(ParserStrings.MissingNameAfterKeyword),
                    ParserStrings.MissingNameAfterKeyword,
                    classToken.Text);
                return new ErrorStatementAst(classToken.Extent);
            // Class name token represents a name of type, not a member. Highlight it as a type name.
            classNameToken.TokenFlags &= ~TokenFlags.MemberName;
            classNameToken.TokenFlags |= TokenFlags.TypeName;
            // handle inheritance constraint.
            var superClassesList = new List<TypeConstraintAst>();
                SetTokenizerMode(TokenizerMode.Signature);
                Token colonToken = PeekToken();
                if (colonToken.Kind == TokenKind.Colon)
                    this.SkipToken();
                    ITypeName superClass;
                        superClass = this.TypeNameRule(allowAssemblyQualifiedNames: false, firstTypeNameToken: out _);
                        if (superClass == null)
                            ReportIncompleteInput(After(ExtentFromFirstOf(commaToken, colonToken)),
                                nameof(ParserStrings.TypeNameExpected),
                                ParserStrings.TypeNameExpected);
                        superClassesList.Add(new TypeConstraintAst(superClass.Extent, superClass));
                        commaToken = this.PeekToken();
                        if (commaToken.Kind == TokenKind.Comma)
                            this.SkipNewlines();
                    var lastElement = (superClassesList.Count > 0) ? (Ast)superClassesList[superClassesList.Count - 1] : name;
                    ReportIncompleteInput(After(lastElement),
                        nameof(ParserStrings.MissingTypeBody),
                        ParserStrings.MissingTypeBody,
                    return new ErrorStatementAst(ExtentOf(classToken, lastElement), superClassesList);
                IScriptExtent lastExtent = lCurly.Extent;
                List<Ast> nestedAsts = null;
                MemberAst member;
                List<MemberAst> members = new List<MemberAst>();
                List<Ast> astsOnError = null;
                while ((member = ClassMemberRule(name.Value, out astsOnError)) != null || astsOnError != null)
                        members.Add(member);
                        lastExtent = member.Extent;
                    if (astsOnError != null && astsOnError.Count > 0)
                        nestedAsts ??= new List<Ast>();
                        nestedAsts.AddRange(astsOnError);
                        lastExtent = astsOnError.Last().Extent;
                var rCurly = NextToken();
                        After(lCurly),
                    lastExtent = rCurly.Extent;
                var startExtent = customAttributes != null && customAttributes.Count > 0
                                      ? customAttributes[0].Extent
                                      : classToken.Extent;
                var extent = ExtentOf(startExtent, lastExtent);
                var classDefn = new TypeDefinitionAst(extent, name.Value, customAttributes?.OfType<AttributeAst>(), members, TypeAttributes.Class, superClassesList);
                if (customAttributes != null && customAttributes.OfType<TypeConstraintAst>().Any())
                    // no need to report error since the error is reported in method StatementRule
                    nestedAsts.AddRange(customAttributes.OfType<TypeConstraintAst>());
                    nestedAsts.Add(classDefn);
                if (nestedAsts != null && nestedAsts.Count > 0)
                    return new ErrorStatementAst(extent, nestedAsts);
                return classDefn;
        private MemberAst ClassMemberRule(string className, out List<Ast> astsOnError)
            // G  class-member:
            // G      method-member
            // G      property-member
            // G  method-member:
            // G      member-attribute-list:opt function-statement
            // G  property-member:
            // G      member-attribute-list:opt  variable
            // G      member-attribute-list:opt  variable  '='  expression
            // G  member-attribute-list:
            // G      member-attribute
            // G      member-attribute-list  member-attribute
            // G  member-attribute:
            // G      'static'
            // G      'hidden'
            IScriptExtent startExtent = null;
            var attributeList = new List<AttributeAst>();
            TypeConstraintAst typeConstraint = null;
            bool scanningAttributes = true;
            Token staticToken = null;
            Token hiddenToken = null;
            Token token = null;
            object lastAttribute = null;
            astsOnError = null;
#if SUPPORT_PUBLIC_PRIVATE
// Public/private not yet supported
            Token publicToken = null;
            Token privateToken = null;
            while (scanningAttributes)
                var attribute = AttributeRule();
                    lastAttribute = attribute;
                    startExtent ??= attribute.Extent;
                    var attributeAst = attribute as AttributeAst;
                    if (attributeAst != null)
                        attributeList.Add(attributeAst);
                    else if (typeConstraint == null)
                        typeConstraint = (TypeConstraintAst)attribute;
                        ReportError(attribute.Extent, nameof(ParserStrings.TooManyTypes), ParserStrings.TooManyTypes);
                startExtent ??= token.Extent;
                    case TokenKind.Public:
                        if (publicToken != null)
                                nameof(ParserStrings.DuplicateQualifier),
                                ParserStrings.DuplicateQualifier,
                        if (privateToken != null)
                                nameof(ParserStrings.ModifiersCannotBeCombined),
                                ParserStrings.ModifiersCannotBeCombined,
                                privateToken.Text,
                        publicToken = token;
                    case TokenKind.Private:
                                publicToken.Text,
                        privateToken = token;
                    case TokenKind.Hidden:
                        if (hiddenToken != null)
                        hiddenToken = token;
                        lastAttribute = token;
                    case TokenKind.Static:
                        if (staticToken != null)
                        staticToken = token;
                        scanningAttributes = false;
            if (token.Kind == TokenKind.Variable)
                var varToken = token as VariableToken;
                ExpressionAst initialValueAst = null;
                var assignToken = PeekToken();
                if (assignToken.Kind == TokenKind.Equals)
                    initialValueAst = ExpressionRule();
                PropertyAttributes attributes = privateToken != null ? PropertyAttributes.Private : PropertyAttributes.Public;
                PropertyAttributes attributes = PropertyAttributes.Public;
                    attributes |= PropertyAttributes.Static;
                    attributes |= PropertyAttributes.Hidden;
                var endExtent = initialValueAst != null ? initialValueAst.Extent : varToken.Extent;
                Token terminatorToken = PeekToken();
                if (terminatorToken.Kind != TokenKind.NewLine && terminatorToken.Kind != TokenKind.Semi && terminatorToken.Kind != TokenKind.RCurly)
                    ReportIncompleteInput(After(endExtent),
                        nameof(ParserStrings.MissingPropertyTerminator),
                        ParserStrings.MissingPropertyTerminator);
                // Include the semicolon in the extent but not newline or rcurly as that will look weird, e.g. if an error is reported on the full extent
                if (terminatorToken.Kind == TokenKind.Semi)
                    endExtent = terminatorToken.Extent;
                if (!string.IsNullOrEmpty(varToken.Name))
                    return new PropertyMemberAst(ExtentOf(startExtent, endExtent), varToken.Name,
                        typeConstraint, attributeList, attributes, initialValueAst);
                    // Incompleted input like:
                    // class foo { $private: }
                    // Error message already emitted by tokenizer ScanVariable
                    RecordErrorAsts(attributeList, ref astsOnError);
                    RecordErrorAsts(typeConstraint, ref astsOnError);
                    RecordErrorAsts(initialValueAst, ref astsOnError);
            if (TryUseTokenAsSimpleName(token))
                var functionDefinition = MethodDeclarationRule(token, className, staticToken != null) as FunctionDefinitionAst;
                if (functionDefinition == null)
                    // TODO: better error recovery - shouldn't assume this was the last class member
                    Diagnostics.Assert(ErrorList.Count > 0, "Should be an error if we don't have a function");
                    SyncOnError(false, TokenKind.RCurly);
                MethodAttributes attributes = privateToken != null ? MethodAttributes.Private : MethodAttributes.Public;
                MethodAttributes attributes = MethodAttributes.Public;
                    attributes |= MethodAttributes.Static;
                    attributes |= MethodAttributes.Hidden;
                return new FunctionMemberAst(ExtentOf(startExtent, functionDefinition), functionDefinition, typeConstraint, attributeList, attributes);
            if (lastAttribute != null)
                // We have the start of a member, but didn't see a variable or 'def'.
                ReportIncompleteInput(After(ExtentFromFirstOf(lastAttribute)),
                    nameof(ParserStrings.IncompleteMemberDefinition),
                    ParserStrings.IncompleteMemberDefinition);
        private static bool TryUseTokenAsSimpleName(Token token)
            if (token.Kind == TokenKind.Identifier
                || token.Kind == TokenKind.DynamicKeyword
                || token.TokenFlags.HasFlag(TokenFlags.Keyword))
                token.TokenFlags = TokenFlags.None;
        private static void RecordErrorAsts(Ast errAst, ref List<Ast> astsOnError)
            if (errAst == null)
            astsOnError ??= new List<Ast>();
            astsOnError.Add(errAst);
        private static void RecordErrorAsts(IEnumerable<Ast> errAsts, ref List<Ast> astsOnError)
            if (errAsts == null || !errAsts.Any())
            astsOnError.AddRange(errAsts);
        private Token NextTypeIdentifierToken()
        private StatementAst EnumDefinitionRule(List<AttributeBaseAst> customAttributes, Token enumToken)
            // G  enum-statement:
            // G      'enum'   new-lines:opt   enum-name   '{'   enum-member-list   '}'
            // G      'enum'   new-lines:opt   enum-name   ':'  enum-underlying-type  '{'   enum-member-list   '}'
            // G  enum-name:
            // G  enum-underlying-type:
            // G      new-lines:opt   valid-type-name   new-lines:opt
            // G  enum-member-list:
            // G      enum-member  new-lines:opt
            // G      enum-member-list   enum-member
            const TypeCode ValidUnderlyingTypeCodes = TypeCode.Byte | TypeCode.Int16 | TypeCode.Int32 | TypeCode.Int64 | TypeCode.SByte | TypeCode.UInt16 | TypeCode.UInt32 | TypeCode.UInt64;
            var name = SimpleNameRule();
                    After(enumToken),
                    enumToken.Text);
                return new ErrorStatementAst(enumToken.Extent);
            TypeConstraintAst underlyingTypeConstraint = null;
                    ITypeName underlyingType;
                    underlyingType = this.TypeNameRule(allowAssemblyQualifiedNames: false, firstTypeNameToken: out _);
                    if (underlyingType == null)
                            After(colonToken),
                        var resolvedType = underlyingType.GetReflectionType();
                        if (resolvedType == null || !ValidUnderlyingTypeCodes.HasFlag(resolvedType.GetTypeCode()))
                                underlyingType.Extent,
                                underlyingType.Name);
                        underlyingTypeConstraint = new TypeConstraintAst(underlyingType.Extent, underlyingType);
                        After(name),
                        enumToken.Kind.Text());
                    return new ErrorStatementAst(ExtentOf(enumToken, name));
                while ((member = EnumMemberRule()) != null)
                                          : enumToken.Extent;
                var extent = ExtentOf(startExtent, rCurly);
                var enumDefn = new TypeDefinitionAst(extent, name.Value, customAttributes?.OfType<AttributeAst>(), members, TypeAttributes.Enum, underlyingTypeConstraint == null ? null : new[] { underlyingTypeConstraint });
                    // No need to report error since there is error reported in method StatementRule
                    List<Ast> nestedAsts = new List<Ast>();
                    nestedAsts.Add(enumDefn);
                    return new ErrorStatementAst(startExtent, nestedAsts);
                return enumDefn;
        private MemberAst EnumMemberRule()
            // G  enum-member:
            // G      enum-member-name
            // G      enum-member-name  '='  expression
            // G  enum-member-name:
            const PropertyAttributes enumMemberAttributes =
                PropertyAttributes.Public | PropertyAttributes.Static | PropertyAttributes.Literal;
            var enumeratorName = SimpleNameRule();
            if (enumeratorName == null)
            IScriptExtent endExtent = enumeratorName.Extent;
            var missingInitializer = false;
            Token assignToken = null;
                assignToken = PeekToken();
                    if (initialValueAst == null)
                        ReportError(After(assignToken),
                            nameof(ParserStrings.ExpectedValueExpression),
                            ParserStrings.ExpectedValueExpression,
                            assignToken.Kind.Text());
                        endExtent = assignToken.Extent;
                        missingInitializer = true;
                        endExtent = initialValueAst.Extent;
                // If the initializer is missing, no sense in reporting another error about a missing terminator
                if (!missingInitializer)
            return new PropertyMemberAst(ExtentOf(enumeratorName, endExtent), enumeratorName.Value, null, null, enumMemberAttributes, initialValueAst);
        private StatementAst UsingStatementRule(Token usingToken)
            // G  using-statement:
            // G      'using'   'namespace'   identifier
            // G      'using'   'namespace'   identifier   '='   identifier
            // G      'using'   'module'   identifier
            // G      'using'   'module'   identifier   '='   identifier
            // G      'using'   'module'   hashtable
            // G      'using'   'module'   identifier   '='   hashtable
            // G      'using'   'type'   identifier   '='   identifier
            // G      'using'   'assembly'   identifier
            // G      'using'   'command'   identifier   '='   identifier
            // in case of aliasing (=), first identifier is alias name, second identifier is alias value (the real name).
            var directiveToken = NextToken();
            UsingStatementKind kind;
            bool aliasAllowed = false;
            bool aliasRequired = false;
            switch (directiveToken.Kind)
                case TokenKind.Namespace:
                    kind = UsingStatementKind.Namespace;
                    aliasAllowed = true;
                case TokenKind.Type:
                    kind = UsingStatementKind.Type;
                    aliasRequired = true;
                case TokenKind.Module:
                    kind = UsingStatementKind.Module;
                case TokenKind.Command:
                    kind = UsingStatementKind.Command;
                case TokenKind.Assembly:
                    kind = UsingStatementKind.Assembly;
                    UngetToken(directiveToken);
                    ReportIncompleteInput(After(usingToken),
                        nameof(ParserStrings.MissingUsingStatementDirective),
                        ParserStrings.MissingUsingStatementDirective);
                    return new ErrorStatementAst(usingToken.Extent);
            var itemToken = NextToken();
            switch (itemToken.Kind)
                case TokenKind.NewLine:
                // Example: 'using module ,exampleModuleName'
                // GetCommandArgument will successfully return an argument for a unary array argument
                // but we don't want to allow that syntax with a using statement.
                        ReportIncompleteInput(After(directiveToken),
                            nameof(ParserStrings.MissingUsingItemName),
                            ParserStrings.MissingUsingItemName);
                        return new ErrorStatementAst(ExtentOf(usingToken, directiveToken));
            var itemAst = GetCommandArgument(CommandArgumentContext.CommandArgument, itemToken);
            if (itemAst == null)
                ReportError(itemToken.Extent,
                    nameof(ParserStrings.InvalidValueForUsingItemName),
                    ParserStrings.InvalidValueForUsingItemName,
                    itemToken.Text);
                // ErrorRecovery: If there is no identifier, skip whole 'using' line
                SyncOnError(true, TokenKind.Semi, TokenKind.NewLine);
                return new ErrorStatementAst(ExtentOf(usingToken, itemToken.Extent));
            if (itemAst is not StringConstantExpressionAst
                && (kind != UsingStatementKind.Module || itemAst is not HashtableAst))
                ReportError(ExtentFromFirstOf(itemAst, itemToken),
                    itemAst.Extent.Text);
                return new ErrorStatementAst(ExtentOf(usingToken, ExtentFromFirstOf(itemAst, itemToken)));
            HashtableAst htAst = null;
                    itemAst = ResolveUsingAssembly((StringConstantExpressionAst)itemAst);
                    htAst = itemAst as HashtableAst;
            // if htAst is not null, then we don't expect alias
            if ((aliasAllowed || aliasRequired) && (htAst == null))
                var equalsToken = PeekToken();
                    var aliasToken = NextToken();
                    if (aliasToken.Kind is TokenKind.EndOfInput or TokenKind.NewLine or TokenKind.Semi)
                        UngetToken(aliasToken);
                            nameof(ParserStrings.MissingNamespaceAlias),
                            ParserStrings.MissingNamespaceAlias);
                        return new ErrorStatementAst(ExtentOf(usingToken, equalsToken));
                    if (aliasToken.Kind == TokenKind.Comma)
                        ReportError(aliasToken.Extent, nameof(ParserStrings.UnexpectedUnaryOperator), ParserStrings.UnexpectedUnaryOperator, aliasToken.Text);
                        return new ErrorStatementAst(ExtentOf(usingToken, aliasToken));
                    var aliasAst = GetCommandArgument(CommandArgumentContext.CommandArgument, aliasToken);
                    if (kind == UsingStatementKind.Module && aliasAst is HashtableAst)
                        htAst = (HashtableAst)aliasAst;
                    else if (aliasAst is not StringConstantExpressionAst)
                        var errorExtent = ExtentFromFirstOf(aliasAst, aliasToken);
                        Ast[] nestedAsts;
                        if (aliasAst is null)
                            nestedAsts = new Ast[] { itemAst };
                            nestedAsts = new Ast[] { itemAst, aliasAst };
                        ReportError(errorExtent, nameof(ParserStrings.InvalidValueForUsingItemName), ParserStrings.InvalidValueForUsingItemName, errorExtent.Text);
                        return new ErrorStatementAst(ExtentOf(usingToken, errorExtent), nestedAsts);
                    RequireStatementTerminator();
                    if (htAst == null)
                        return new UsingStatementAst(
                            ExtentOf(usingToken, aliasToken),
                            kind,
                            (StringConstantExpressionAst)itemAst,
                            (StringConstantExpressionAst)aliasAst);
                            htAst);
                if (aliasRequired)
                    ReportIncompleteInput(After(itemToken),
                        nameof(ParserStrings.MissingEqualsInUsingAlias),
                        ParserStrings.MissingEqualsInUsingAlias);
                    return new ErrorStatementAst(ExtentOf(usingToken, itemAst), new Ast[] { itemAst });
                return new UsingStatementAst(ExtentOf(usingToken, itemAst), kind, (StringConstantExpressionAst)itemAst);
                return new UsingStatementAst(ExtentOf(usingToken, itemAst), htAst);
        private StringConstantExpressionAst ResolveUsingAssembly(StringConstantExpressionAst name)
            var assemblyName = name.Value;
            // assemblyName can be invalid, i.e. during typing.
            // We only use uri to check that we don't allow UNC paths.
            if (Uri.TryCreate(assemblyName, UriKind.Absolute, out uri))
                if (uri.IsUnc)
                        nameof(ParserStrings.CannotLoadAssemblyFromUncPath),
                        ParserStrings.CannotLoadAssemblyFromUncPath,
                // don't allow things like 'using assembly http://microsoft.com'
                if (uri.Scheme != "file")
                        nameof(ParserStrings.CannotLoadAssemblyWithUriSchema),
                        ParserStrings.CannotLoadAssemblyWithUriSchema,
                        uri.Scheme);
                string assemblyFileName = assemblyName;
                    var scriptFileName = name.Extent.File;
                    if (!Path.IsPathRooted(assemblyFileName))
                        string workingDirectory;
                        if (string.IsNullOrEmpty(scriptFileName))
                            // We are in REPL, or Invoke-Expression, or ScriptBlock.Create, etc.
                            // It's legal to use '.\foo.dll', and we should do lookup in $pwd.
                            // We resolving it in parse time to avoid difference in script behavior,
                            // when script execution is delayed from parsing, i.e.
                            // $script = [scriptblock]::Create('using assembly .\foo.dll; [Foo].foo()')
                            // # using assembly behavior should be the same in these two cases:
                            // cd A; $script.Invoke()
                            // cd B; $script.Invoke()
                            if (Runspaces.Runspace.DefaultRunspace != null)
                                workingDirectory = Runspaces.Runspace.DefaultRunspace.ExecutionContext.EngineIntrinsics.SessionState.Path.CurrentLocation.Path;
                                // In this case, our best guess is process working directory.
                                workingDirectory = Directory.GetCurrentDirectory();
                            workingDirectory = Path.GetDirectoryName(scriptFileName);
                        assemblyFileName = Path.Combine(workingDirectory, assemblyFileName);
                if (assemblyFileName == null || !File.Exists(assemblyFileName))
                        nameof(ParserStrings.ErrorLoadingAssembly),
                    return new StringConstantExpressionAst(name.Extent, assemblyFileName, name.StringConstantType);
        private StatementAst MethodDeclarationRule(Token functionNameToken, string className, bool isStaticMethod)
            // G  method-statement:
            // G      new-lines:opt   function-name   function-parameter-declaration   base-ctor-call:opt  '{'   script-block   '}'
            // G  base-ctor-call: // can be present only if function-name == className
            // G      ':'    new-lines:opt   'base'   new-lines:opt   parenthesized-expression   new-lines:opt
            // G  function-name:
            var functionName = functionNameToken.Text;
            List<ParameterAst> parameters;
            Token lParen = this.PeekToken();
                parameters = this.FunctionParameterDeclarationRule(out endErrorStatement, out rParen);
                this.ReportIncompleteInput(After(functionNameToken),
                    nameof(ParserStrings.MissingMethodParameterList),
                    ParserStrings.MissingMethodParameterList);
                parameters = new List<ParameterAst>();
            bool isCtor = functionName.Equals(className, StringComparison.OrdinalIgnoreCase);
            List<ExpressionAst> baseCtorCallParams = null;
            Token baseToken = null;
            IScriptExtent baseCallLastExtent = null;
            TokenizerMode oldTokenizerMode;
            if (isCtor && !isStaticMethod)
                oldTokenizerMode = _tokenizer.Mode;
                        baseToken = PeekToken();
                        if (baseToken.Kind == TokenKind.Base)
                            lParen = PeekToken();
                                // we don't allow syntax
                                // : base{ script }
                                // as a short for
                                // : base( { script } )
                                baseCtorCallParams = InvokeParamParenListRule(lParen, out baseCallLastExtent);
                                endErrorStatement = baseToken.Extent;
                                ReportIncompleteInput(After(baseToken),
                            endErrorStatement = colonToken.Extent;
                            ReportIncompleteInput(After(colonToken),
                                nameof(ParserStrings.MissingBaseCtorCall),
                                ParserStrings.MissingBaseCtorCall);
                baseCtorCallParams ??=
                    // Assuming implicit default ctor
                    new List<ExpressionAst>();
                if (endErrorStatement == null)
                    endErrorStatement = ExtentFromFirstOf(rParen, functionNameToken);
                        nameof(ParserStrings.MissingFunctionBody),
                        ParserStrings.MissingFunctionBody);
                return new ErrorStatementAst(ExtentOf(functionNameToken, endErrorStatement), parameters);
            StatementAst baseCtorCallStatement = null;
                IScriptExtent baseCallExtent;
                IScriptExtent baseKeywordExtent;
                if (baseToken != null)
                    baseCallExtent = ExtentOf(baseToken, baseCallLastExtent);
                    baseKeywordExtent = baseToken.Extent;
                    baseCallExtent = PositionUtilities.EmptyExtent;
                    baseKeywordExtent = PositionUtilities.EmptyExtent;
                var invokeMemberAst = new BaseCtorInvokeMemberExpressionAst(baseKeywordExtent, baseCallExtent, baseCtorCallParams);
                baseCtorCallStatement = new CommandExpressionAst(invokeMemberAst.Extent, invokeMemberAst, null);
                SetTokenizerMode(TokenizerMode.Command);
                ScriptBlockAst scriptBlock = ScriptBlockRule(lCurly, false, baseCtorCallStatement);
                var result = new FunctionDefinitionAst(ExtentOf(functionNameToken, scriptBlock),
                    /*isFilter:*/false, /*isWorkflow:*/false, functionNameToken, parameters, scriptBlock);
        private StatementAst FunctionDeclarationRule(Token functionToken)
            // G  function-statement:
            // G      'function'   new-lines:opt   function-name   function-parameter-declaration:opt   '{'   script-block   '}'
            // G      'filter'   new-lines:opt   function-name   function-parameter-declaration:opt   '{'   script-block   '}'
            // G      'workflow'   new-lines:opt   function-name   function-parameter-declaration:opt   '{'   script-block   '}'
            Token functionNameToken = NextToken();
            switch (functionNameToken.Kind)
                // The following are not allowed as function names, anything else is allowed:
                // The general rule is - punctuators and unary operators are not allowed
                // We want to allow as much as reasonably possible, e.g. if it could be a
                // valid command name, we want to allow it with very limited exceptions.
                case TokenKind.Pipe:
                case TokenKind.LCurly:
                case TokenKind.RedirectInStd:
                case TokenKind.AndAnd:
                case TokenKind.OrOr:
                case TokenKind.Ampersand:
                case TokenKind.HereStringExpandable:
                case TokenKind.HereStringLiteral:
                    // ErrorRecovery: Don't continue, assume the function isn't there yet.
                    UngetToken(functionNameToken);
                    ReportIncompleteInput(After(functionToken),
                        functionToken.Text);
                    return new ErrorStatementAst(functionToken.Extent);
            // A function name that matches a keyword isn't really a keyword, so don't color it that way
            functionNameToken.TokenFlags &= ~TokenFlags.Keyword;
            IScriptExtent endErrorStatement;
            Token rParen;
            var parameters = this.FunctionParameterDeclarationRule(out endErrorStatement, out rParen);
                return new ErrorStatementAst(ExtentOf(functionToken, endErrorStatement), parameters);
            bool isFilter = functionToken.Kind == TokenKind.Filter;
            bool isWorkflow = functionToken.Kind == TokenKind.Workflow;
            bool oldTokenizerWorkflowContext = _tokenizer.InWorkflowContext;
                _tokenizer.InWorkflowContext = isWorkflow;
                ScriptBlockAst scriptBlock = ScriptBlockRule(lCurly, isFilter);
                var functionName = (functionNameToken.Kind == TokenKind.Generic)
                                       ? ((StringToken)functionNameToken).Value
                                       : functionNameToken.Text;
                FunctionDefinitionAst result = new FunctionDefinitionAst(ExtentOf(functionToken, scriptBlock),
                    isFilter, isWorkflow, functionNameToken, parameters, scriptBlock);
                _tokenizer.InWorkflowContext = oldTokenizerWorkflowContext;
        private List<ParameterAst> FunctionParameterDeclarationRule(out IScriptExtent endErrorStatement, out Token rParen)
            // G  function-parameter-declaration:
            // G      new-lines:opt   '('   parameter-list   new-lines:opt   ')'
            List<ParameterAst> parameters = null;
            endErrorStatement = null;
                parameters = ParameterListRule();
                    // ErrorRecovery: assume a body follows, so just keep parsing.
                    endErrorStatement = parameters.Count > 0 ? parameters.Last().Extent : lParen.Extent;
        private StatementAst TrapStatementRule(Token trapToken)
            // G  trap-statement:
            // G      'trap'  new-lines:opt   type-literal:opt   statement-block
            AttributeBaseAst type = AttributeRule();
            var typeConstraintAst = type as TypeConstraintAst;
            if (type != null && typeConstraintAst == null)
                // Presumably we parsed an attribute instead of a type.  Put it back and let the code
                // below report a missing trap body.  The attribute might belong to something that
                // is after the trap, something like:
                //    trap
                //    [ValidateNotNullOrEmpty()]$var
            var body = StatementBlockRule();
                // ErrorRecovery: just return an error statement.
                IScriptExtent endErrorStatement = ExtentFromFirstOf(typeConstraintAst, trapToken);
                    nameof(ParserStrings.MissingTrapStatement),
                    ParserStrings.MissingTrapStatement);
                return new ErrorStatementAst(ExtentOf(trapToken, endErrorStatement), GetNestedErrorAsts(typeConstraintAst));
            return new TrapStatementAst(ExtentOf(trapToken, body), typeConstraintAst, body);
        /// Parse a catch block.
        /// <param name="endErrorStatement">
        /// Set to the last thing scanned that is definitely part of the catch, but only set after issuing an error.
        /// <param name="errorAsts">
        /// If there are any errors and CatchBlockRule is returning null, this list is used to return back any asts
        /// consumed here (essentially the type constraints.)
        /// <returns>A catch clause, or null there is no catch or there was some error.</returns>
        private CatchClauseAst CatchBlockRule(ref IScriptExtent endErrorStatement, ref List<TypeConstraintAst> errorAsts)
            // G  catch-clause:
            // G      new-lines:opt   'catch'   catch-type-list:opt   statement-block
            // G  catch-type-list:
            // G      new-lines:opt   type-literal
            // G      catch-type-list   new-lines:opt   ','   new-lines:opt   type-literal
            Token catchToken = NextToken();
            if (catchToken.Kind != TokenKind.Catch)
                UngetToken(catchToken);
            List<TypeConstraintAst> exceptionTypes = null;
                        // ErrorRecovery: Just consume the comma, pretend it wasn't there and look for the handler block.
                        endErrorStatement = commaToken.Extent;
                            nameof(ParserStrings.MissingTypeLiteralToken),
                            ParserStrings.MissingTypeLiteralToken);
                if (typeConstraintAst == null)
                    // below report a missing catch clause body.  The attribute might belong to something that
                    // is after the try/catch, something like:
                    //    catch
                exceptionTypes ??= new List<TypeConstraintAst>();
                exceptionTypes.Add(typeConstraintAst);
            StatementBlockAst handler = StatementBlockRule();
            if (handler == null)
                // Avoid issuing an extra error if we've already issued one, plus the end of the ErrorStatementAst
                // shouldn't be updated.
                if (commaToken == null || endErrorStatement != commaToken.Extent)
                    // ErrorRecovery: just use the "missing" block in the result ast.
                    endErrorStatement = exceptionTypes != null ? exceptionTypes.Last().Extent : catchToken.Extent;
                        nameof(ParserStrings.MissingCatchHandlerBlock),
                        ParserStrings.MissingCatchHandlerBlock);
                if (exceptionTypes != null)
                    if (errorAsts == null)
                        errorAsts = exceptionTypes;
                        errorAsts.AddRange(exceptionTypes);
            return new CatchClauseAst(ExtentOf(catchToken, handler), exceptionTypes, handler);
        private StatementAst TryStatementRule(Token tryToken)
            // G  try-statement:
            // G      'try'   statement-block   catch-clauses
            // G      'try'   statement-block   finally-clause
            // G      'try'   statement-block   catch-clauses   finally-clause
            // G  catch-clauses:
            // G      catch-clause
            // G      catch-clauses   catch-clause
            // G  finally-clause:
            // G      new-lines:opt   'finally'   statement-block
                // ErrorRecovery: don't parse more, return an error statement.
                ReportIncompleteInput(After(tryToken),
                    nameof(ParserStrings.MissingTryStatementBlock),
                    ParserStrings.MissingTryStatementBlock);
                return new ErrorStatementAst(tryToken.Extent);
            CatchClauseAst catchClause;
            List<CatchClauseAst> catches = new List<CatchClauseAst>();
            List<TypeConstraintAst> errorAsts = null;
            while ((catchClause = CatchBlockRule(ref endErrorStatement, ref errorAsts)) != null)
                catches.Add(catchClause);
            Token finallyToken = PeekToken();
            StatementBlockAst finallyBlock = null;
            if (finallyToken.Kind == TokenKind.Finally)
                finallyBlock = StatementBlockRule();
                if (finallyBlock == null)
                    // ErrorRecovery: just return null, a null finally block is fine in the resulting ast (but maybe consider
                    // marking the resulting ast as having an error.)
                    endErrorStatement = finallyToken.Extent;
                                          nameof(ParserStrings.MissingFinallyStatementBlock),
                                          ParserStrings.MissingFinallyStatementBlock,
                                          finallyToken.Kind.Text());
            if (catches.Count == 0 && finallyBlock == null && endErrorStatement == null)
                    nameof(ParserStrings.MissingCatchOrFinally),
                    ParserStrings.MissingCatchOrFinally);
                return new ErrorStatementAst(ExtentOf(tryToken, endErrorStatement), GetNestedErrorAsts(body, catches, errorAsts));
            return new TryStatementAst(ExtentOf(tryToken, finallyBlock != null ? finallyBlock.Extent : catches.Last().Extent),
                                       body, catches, finallyBlock);
        private StatementAst DataStatementRule(Token dataToken)
            // G  data-statement:
            // G      'data'    new-lines:opt   data-name   data-commands-allowed:opt   statement-block
            // G  data-name:
            // G  data-commands-allowed:
            // G      new-lines:opt   '-supportedcommand'   data-commands-list
            // G  data-commands-list:
            // G      new-lines:opt   data-command
            // G      data-commands-list   ','   new-lines:opt   data-command
            // G  data-command:
            // G      command-name-expr
            var dataVariableNameAst = SimpleNameRule();
            string dataVariableName = dataVariableNameAst?.Value;
            Token supportedCommandToken = PeekToken();
            List<ExpressionAst> commands = null;
            if (supportedCommandToken.Kind == TokenKind.Parameter)
                if (!IsSpecificParameter(supportedCommandToken, "SupportedCommand"))
                    // ErrorRecovery: Assume the parameter is just misspelled, look for command names next
                    endErrorStatement = supportedCommandToken.Extent;
                    ReportError(endErrorStatement,
                        nameof(ParserStrings.InvalidParameterForDataSectionStatement),
                        ParserStrings.InvalidParameterForDataSectionStatement,
                        ((ParameterToken)supportedCommandToken).ParameterName);
                commands = new List<ExpressionAst>();
                    ExpressionAst ast = GetSingleCommandArgument(CommandArgumentContext.CommandName);
                        // ErrorRecovery: Look for the data statement body.
                        // Only report an error if an error hasn't already been issued.
                            ReportIncompleteInput(After(commaToken ?? supportedCommandToken),
                                nameof(ParserStrings.MissingValueForSupportedCommandInDataSectionStatement),
                                ParserStrings.MissingValueForSupportedCommandInDataSectionStatement);
                        endErrorStatement = commaToken != null ? commaToken.Extent : supportedCommandToken.Extent;
                    commands.Add(ast);
                    endErrorStatement = commands != null
                                            ? commands.Last().Extent
                                            : ExtentFromFirstOf(dataVariableNameAst, dataToken);
                        nameof(ParserStrings.MissingStatementBlockForDataSection),
                        ParserStrings.MissingStatementBlockForDataSection);
                return new ErrorStatementAst(ExtentOf(dataToken, endErrorStatement), GetNestedErrorAsts(commands));
            return new DataStatementAst(ExtentOf(dataToken, body), dataVariableName, commands, body);
        private PipelineBaseAst PipelineChainRule()
            // G  pipeline-chain:
            // G      pipeline
            // G      pipeline-chain chain-operator pipeline
            // G  chain-operator:
            // G      '||'
            // G      '&&'
            // If this feature is not enabled,
            // just look for pipelines as before.
            // First look for assignment, since PipelineRule once handled that and this supersedes that.
            // We may end up with an expression here as a result,
            // in which case we hang on to it to pass it into the first pipeline rule call.
            // Look for an expression,
            // which could either be a variable to assign to,
            // or the first expression in a pipeline: "Hi" | % { $_ }
                    // We peek here because we are in expression mode, otherwise =0 will get scanned
                    // as a single token.
                    if (token.Kind.HasTrait(TokenFlags.AssignmentOperator))
                        assignToken = token;
            // If we have an assign token, deal with assignment
            if (expr != null && assignToken != null)
                    // ErrorRecovery: we are very likely at EOF because pretty much anything should result in some
                    // pipeline, so just keep parsing.
                    IScriptExtent errorExtent = After(assignToken);
                    statement = new ErrorStatementAst(errorExtent);
                return new AssignmentStatementAst(
                    ExtentOf(expr, statement),
                    assignToken.Kind,
                    statement,
                    assignToken.Extent);
            // Start scanning for a pipeline chain,
            // possibly starting with the expression from earlier
            ExpressionAst startExpression = expr;
            ChainableAst currentPipelineChain = null;
            Token currentChainOperatorToken = null;
            Token nextToken = null;
            bool background = false;
                // Look for the next pipeline in the chain,
                // at this point we should already have parsed all assignments
                // in enclosing calls to PipelineChainRule
                PipelineAst nextPipeline;
                Token firstPipelineToken = null;
                if (startExpression != null)
                    nextPipeline = (PipelineAst)PipelineRule(startExpression);
                    startExpression = null;
                    // Remember the token for error reporting,
                    // since erroneous results in the rules still consume tokens
                    firstPipelineToken = PeekToken();
                    nextPipeline = (PipelineAst)PipelineRule();
                if (nextPipeline == null)
                    if (currentChainOperatorToken == null)
                        // We haven't seen a chain token, so the caller is responsible
                        // for expecting a pipeline and must manage this
                    // See if we are responsible for reporting the issue
                    switch (firstPipelineToken.Kind)
                            // If we're at EOF, we should allow input to complete
                                After(currentChainOperatorToken),
                                nameof(ParserStrings.EmptyPipelineChainElement),
                                ParserStrings.EmptyPipelineChainElement,
                                currentChainOperatorToken.Text);
                            // If something like 'command && &' or 'command && .' was provided,
                            // CommandRule has already reported the error.
                                ExtentOf(currentChainOperatorToken, firstPipelineToken),
                    return new ErrorStatementAst(
                        ExtentOf(currentPipelineChain, currentChainOperatorToken),
                        currentChainOperatorToken,
                        new[] { currentPipelineChain });
                // Look ahead for a chain operator
                nextToken = PeekToken();
                switch (nextToken.Kind)
                    // Background operators may also occur here
                                ReportError(nextToken.Extent, nameof(ParserStrings.BackgroundOperatorInPipelineChain), ParserStrings.BackgroundOperatorInPipelineChain);
                                return new ErrorStatementAst(ExtentOf(currentPipelineChain ?? nextPipeline, nextToken.Extent));
                        background = true;
                    // No more chain operators -- return
                        // If we haven't seen a chain yet, pass through the pipeline
                        // Simplifies the AST and prevents allocation
                        if (currentPipelineChain == null)
                            if (!background)
                                return nextPipeline;
                            // Set background on the pipeline AST
                            nextPipeline.Background = true;
                        return new PipelineChainAst(
                            ExtentOf(currentPipelineChain.Extent, nextPipeline.Extent),
                            currentPipelineChain,
                            nextPipeline,
                            currentChainOperatorToken.Kind,
                            background);
                // Assemble the new chain statement AST
                currentPipelineChain = currentPipelineChain == null
                    ? (ChainableAst)nextPipeline
                    : new PipelineChainAst(
                        currentChainOperatorToken.Kind);
                // Remember the last operator to chain the coming pipeline
                currentChainOperatorToken = nextToken;
                // Look ahead to report incomplete input if needed
                if (PeekToken().Kind == TokenKind.EndOfInput)
                        After(nextToken),
                        ParserStrings.EmptyPipelineChainElement);
                    return currentPipelineChain;
        private PipelineBaseAst PipelineRule(
            ExpressionAst startExpression = null,
            bool allowBackground = false)
            // G  pipeline:
            // G      assignment-expression
            // G      expression   redirections:opt  pipeline-tail:opt
            // G      command   pipeline-tail:opt
            // G  assignment-expression:
            // G      expression   assignment-operator   statement
            // G  pipeline-tail:
            // G      new-lines:opt   '|'   new-lines:opt   command   pipeline-tail:opt
            var pipelineElements = new List<CommandBaseAst>();
            bool scanning = true;
            ExpressionAst expr = startExpression;
            while (scanning)
                CommandBaseAst commandAst;
                    // Look for an expression at the beginning of a pipeline
                    if (pipelineElements.Count > 0)
                            expr.Extent,
                            nameof(ParserStrings.ExpressionsMustBeFirstInPipeline),
                            ParserStrings.ExpressionsMustBeFirstInPipeline);
                    RedirectionAst[] redirections = null;
                    var redirectionToken = PeekToken() as RedirectionToken;
                    RedirectionAst lastRedirection = null;
                    while (redirectionToken != null)
                        redirections ??= new RedirectionAst[CommandBaseAst.MaxRedirections];
                        IScriptExtent unused = null;
                        lastRedirection = RedirectionRule(redirectionToken, redirections, ref unused);
                        redirectionToken = PeekToken() as RedirectionToken;
                    var exprExtent = lastRedirection != null ? ExtentOf(expr, lastRedirection) : expr.Extent;
                    commandAst = new CommandExpressionAst(
                        exprExtent,
                        redirections?.Where(static r => r != null));
                    commandAst = (CommandAst)CommandRule(forDynamicKeyword: false);
                if (commandAst != null)
                    startExtent ??= commandAst.Extent;
                    pipelineElements.Add(commandAst);
                else if (pipelineElements.Count > 0 || PeekToken().Kind == TokenKind.Pipe)
                    // ErrorRecovery: just fall through
                    // If the first pipe element is null, the position points to the pipe (ideally it would
                    // point before, but the pipe could be the first character), otherwise the empty element
                    // is after the pipe character.
                    IScriptExtent errorPosition = nextToken != null ? After(nextToken) : PeekToken().Extent;
                        nameof(ParserStrings.EmptyPipeElement),
                        ParserStrings.EmptyPipeElement);
                // Reset the expression for the next loop
                expr = null;
                // Skip newlines before pipe tokens to support (pipe)line continuation when pipe
                // tokens start the next line of script
                if (nextToken.Kind == TokenKind.NewLine && _tokenizer.IsPipeContinuation(nextToken.Extent))
                        // Handled by invoking rule
                        scanning = false;
                        if (!allowBackground)
                        // ErrorRecovery: don't eat the token, assume it belongs to something else.
                            nextToken.Extent,
                            nextToken.Text);
            if (pipelineElements.Count == 0)
            return new PipelineAst(ExtentOf(startExtent, pipelineElements[pipelineElements.Count - 1]), pipelineElements, background);
        private RedirectionAst RedirectionRule(RedirectionToken redirectionToken, RedirectionAst[] redirections, ref IScriptExtent extent)
            // G  redirections:
            // G      redirection
            // G      redirections   redirection
            // G  redirection:
            // G      merging-redirection-operator
            // G      file-redirection-operator   redirected-file-name
            // G  redirected-file-name:
            RedirectionAst result;
            var fileRedirectionToken = redirectionToken as FileRedirectionToken;
            if (fileRedirectionToken != null || (redirectionToken is InputRedirectionToken))
                // get location
                var filename = GetSingleCommandArgument(CommandArgumentContext.FileName);
                if (filename == null)
                    // ErrorRecovery: Just pretend we have a filename and continue parsing.
                    ReportError(After(redirectionToken),
                        nameof(ParserStrings.MissingFileSpecification),
                        ParserStrings.MissingFileSpecification);
                    filename = new ErrorExpressionAst(redirectionToken.Extent);
                if (fileRedirectionToken == null)
                    // Must be an input redirection
                    ReportError(redirectionToken.Extent,
                        nameof(ParserStrings.RedirectionNotSupported),
                        ParserStrings.RedirectionNotSupported,
                        redirectionToken.Text);
                    extent = ExtentOf(redirectionToken, filename);
                result = new FileRedirectionAst(ExtentOf(fileRedirectionToken, filename), fileRedirectionToken.FromStream,
                                                filename, fileRedirectionToken.Append);
                var mergingRedirectionToken = (MergingRedirectionToken)redirectionToken;
                RedirectionStream fromStream = mergingRedirectionToken.FromStream;
                RedirectionStream toStream = mergingRedirectionToken.ToStream;
                if (toStream != RedirectionStream.Output)
                    // Have we seen something like 1>&2 or 2>&3
                    // ErrorRecovery: This is just a semantic error, so no special recovery.
                        mergingRedirectionToken.Text);
                    toStream = RedirectionStream.Output;
                else if (fromStream == toStream)
                    // Make sure 1>&1, 2>&2, etc. is an error.
                result = new MergingRedirectionAst(mergingRedirectionToken.Extent, mergingRedirectionToken.FromStream, toStream);
            if (redirections[(int)result.FromStream] == null)
                redirections[(int)result.FromStream] = result;
                string errorStream;
                switch (result.FromStream)
                    case RedirectionStream.All: errorStream = ParserStrings.AllStream; break;
                    case RedirectionStream.Output: errorStream = ParserStrings.OutputStream; break;
                    case RedirectionStream.Error: errorStream = ParserStrings.ErrorStream; break;
                    case RedirectionStream.Warning: errorStream = ParserStrings.WarningStream; break;
                    case RedirectionStream.Verbose: errorStream = ParserStrings.VerboseStream; break;
                    case RedirectionStream.Debug: errorStream = ParserStrings.DebugStream; break;
                    case RedirectionStream.Information: errorStream = ParserStrings.InformationStream; break;
                        throw PSTraceSource.NewArgumentOutOfRangeException("result.FromStream", result.FromStream);
                ReportError(result.Extent,
                    nameof(ParserStrings.StreamAlreadyRedirected),
                    ParserStrings.StreamAlreadyRedirected,
                    errorStream);
            extent = result.Extent;
        private ExpressionAst GetSingleCommandArgument(CommandArgumentContext context)
            if (PeekToken().Kind == TokenKind.Comma || PeekToken().Kind == TokenKind.EndOfInput)
                return GetCommandArgument(context, NextToken());
        private enum CommandArgumentContext
            CommandName = 0x01,
            CommandNameAfterInvocationOperator = 0x02 | CommandName,
            FileName = 0x04,
            CommandArgument = 0x08,
            SwitchCondition = 0x10,
        private ExpressionAst GetCommandArgument(CommandArgumentContext context, Token token)
            Diagnostics.Assert(token.Kind != TokenKind.Comma, "A unary comma is an error in command mode, and should have already been reported.");
            ExpressionAst exprAst;
            List<ExpressionAst> commandArgs = null;
            bool foundVerbatimArgument = false;
                    // The following tokens are never allowed as command arguments.
                        // If we haven't seen an argument, the caller must issue an error.  If we've seen at least one
                        // argument, then we will issue the error and return back the arguments seen so far.
                        if (commaToken == null)
                        // ErrorRecovery: stop looking for additional arguments, exclude the trailing comma
                            nameof(ParserStrings.MissingExpression),
                            ParserStrings.MissingExpression,
                            ",");
                        return new ErrorExpressionAst(ExtentOf(commandArgs[0], commaToken), commandArgs);
                    case TokenKind.DollarParen:
                        exprAst = PrimaryExpressionRule(withMemberAccess: true);
                        Diagnostics.Assert(exprAst != null, "PrimaryExpressionRule should never return null");
                        if ((context & CommandArgumentContext.CommandName) != 0)
                            token.TokenFlags |= TokenFlags.CommandName;
                        var genericToken = (StringToken)token;
                        var expandableToken = genericToken as StringExpandableToken;
                        // A command name w/o invocation operator is not expandable even if the token has expandable parts
                        // If we have seen an invocation operator, the command name is expandable.
                        if (expandableToken != null && context != CommandArgumentContext.CommandName)
                            var nestedExpressions = ParseNestedExpressions(expandableToken);
                            exprAst = new ExpandableStringExpressionAst(expandableToken, expandableToken.Value,
                                                                        expandableToken.FormatString, nestedExpressions);
                            exprAst = new StringConstantExpressionAst(genericToken.Extent, genericToken.Value, StringConstantType.BareWord);
                            // If this is a verbatim argument, then don't continue peeking
                            if (string.Equals(genericToken.Value, VERBATIM_ARGUMENT, StringComparison.OrdinalIgnoreCase))
                                foundVerbatimArgument = true;
                        exprAst = new StringConstantExpressionAst(token.Extent, token.Text, StringConstantType.BareWord);
                        // A command/argument that matches a keyword isn't really a keyword, so don't color it that way
                        token.TokenFlags &= ~TokenFlags.Keyword;
                            case CommandArgumentContext.CommandName:
                            case CommandArgumentContext.CommandNameAfterInvocationOperator:
                            case CommandArgumentContext.FileName:
                            case CommandArgumentContext.CommandArgument:
                            case CommandArgumentContext.SwitchCondition:
                                token.SetIsCommandArgument();
                if (context != CommandArgumentContext.CommandArgument)
                if (foundVerbatimArgument)
                if (token.Kind != TokenKind.Comma)
                commaToken = token;
                commandArgs ??= new List<ExpressionAst>();
                commandArgs.Add(exprAst);
            Diagnostics.Assert(commandArgs != null || exprAst != null, "How did that happen?");
            if (commandArgs != null)
                return new ArrayLiteralAst(ExtentOf(commandArgs[0], commandArgs[commandArgs.Count - 1]), commandArgs);
            return exprAst;
        internal Ast CommandRule(bool forDynamicKeyword)
            // G  command:
            // G      command-name   command-elements:opt
            // G      command-invocation-operator   command-module:opt  command-name-expr   command-elements:opt
            // G  command-invocation-operator:  one of
            // G      '&'   '.'
            // G  command-module:
            // G  command-name:
            // G      generic-token
            // G      generic-token-with-subexpr
            // G  generic-token-with-subexpr: No whitespace is allowed between ) and command-name.
            // G      generic-token-with-subexpr-start   statement-list:opt   )   command-name
            // G  command-name-expr:
            // G      command-name
            // G  command-elements:
            // G      command-element
            // G      command-elements   command-element
            // G  command-element:
            // G      command-parameter
            // G  command-argument:
            Token firstToken;
            bool dotSource, ampersand;
            bool sawDashDash = false;
            IScriptExtent endExtent;
            var elements = new List<CommandElementAst>();
                firstToken = token;
                endExtent = token.Extent;
                dotSource = false;
                ampersand = false;
                CommandArgumentContext context;
                if (token.Kind == TokenKind.Dot)
                    dotSource = true;
                    context = CommandArgumentContext.CommandNameAfterInvocationOperator;
                else if (token.Kind == TokenKind.Ampersand)
                    ampersand = true;
                    context = CommandArgumentContext.CommandName;
                            // Add the first -- as a parameter, which is then ignored when constructing the command processor unless it's a native
                            // command.  All subsequent -- are added as arguments.
                            elements.Add(sawDashDash
                                ? (CommandElementAst)new StringConstantExpressionAst(token.Extent, token.Text, StringConstantType.BareWord)
                                : new CommandParameterAst(token.Extent, "-", null, token.Extent));
                            sawDashDash = true;
                                nameof(ParserStrings.MissingArgument),
                                ParserStrings.MissingArgument);
                            if ((context & CommandArgumentContext.CommandName) != 0 || sawDashDash)
                                var commandName = new StringConstantExpressionAst(token.Extent, token.Text, StringConstantType.BareWord);
                            var parameterToken = (ParameterToken)token;
                            ExpressionAst parameterArgs;
                            IScriptExtent extent;
                            // If the next token is a comma, don't grab it as part of the argument.  The next time through this
                            // loop will issue an error.
                            if (parameterToken.UsedColon && PeekToken().Kind != TokenKind.Comma)
                                parameterArgs = GetCommandArgument(CommandArgumentContext.CommandArgument, NextToken());
                                if (parameterArgs == null)
                                    extent = parameterToken.Extent;
                                        parameterToken.Text);
                                    extent = ExtentOf(token, parameterArgs);
                                parameterArgs = null;
                                extent = token.Extent;
                            endExtent = extent;
                            var paramAst = new CommandParameterAst(extent, parameterToken.ParameterName, parameterArgs, token.Extent);
                            elements.Add(paramAst);
                            if ((context & CommandArgumentContext.CommandName) == 0)
                                RedirectionRule((RedirectionToken)token, redirections, ref endExtent);
                                // For backwards compatibility, we allow redirection operators as command names.
                                // V2 did not allow:
                                //     & <<
                                // but V3 and on will because it falls out rather naturally here.
                                elements.Add(new StringConstantExpressionAst(token.Extent, token.Text, StringConstantType.BareWord));
                            if (token.Kind == TokenKind.InlineScript && context == CommandArgumentContext.CommandName)
                                scanning = InlineScriptRule(token, elements);
                                Diagnostics.Assert(elements.Count >= 1, "We should at least have the command name: inlinescript");
                                endExtent = elements.Last().Extent;
                                if (!scanning) { continue; }
                                var ast = GetCommandArgument(context, token);
                                // If this is the special verbatim argument syntax, look for the next element
                                StringToken argumentToken = token as StringToken;
                                if ((argumentToken != null) && string.Equals(argumentToken.Value, VERBATIM_ARGUMENT, StringComparison.OrdinalIgnoreCase))
                                    elements.Add(ast);
                                    endExtent = ast.Extent;
                                    var verbatimToken = GetVerbatimCommandArgumentToken();
                                    if (verbatimToken != null)
                                        ast = new StringConstantExpressionAst(verbatimToken.Extent, verbatimToken.Value, StringConstantType.BareWord);
                    if (!foundVerbatimArgument)
                        context = CommandArgumentContext.CommandArgument;
            if (elements.Count == 0)
                Diagnostics.Assert(!forDynamicKeyword, "DynamicKeyword should have command name at least");
                if (dotSource || ampersand)
                    IScriptExtent extent = firstToken.Extent;
                        firstToken.Text);
            if (forDynamicKeyword)
                // TODO: report error if any redirections
                return new DynamicKeywordStatementAst(ExtentOf(firstToken, endExtent), elements);
            return new CommandAst(ExtentOf(firstToken, endExtent), elements,
                                  dotSource || ampersand ? firstToken.Kind : TokenKind.Unknown,
        /// <summary>Parse an expression.</summary>
        /// <param name="endNumberOnTernaryOpChars">
        /// When it's known for sure that we are expecting an expression, allowing a generic token like '12?' or '12:' is
        /// not useful. In those cases, we force to start a new token upon seeing '?' and ':' when scanning for a number
        /// by setting this parameter to true, hoping to find a ternary expression.
        private ExpressionAst ExpressionRule(bool endNumberOnTernaryOpChars = false)
            // G  expression:
            // G      logical-expression
            // G  logical-expression:
            // G      binary-expression
            // G      ternary-expression
            // G  ternary-expression:
            // G      binary-expression   '?'   new-lines:opt   ternary-expression   new-lines:opt   ':'   new-lines:opt   ternary-expression
                ExpressionAst condition = BinaryExpressionRule(endNumberOnTernaryOpChars);
                if (token.Kind != TokenKind.QuestionMark)
                // We have seen the ternary operator '?' and now expecting the 'IfTrue' expression.
                ExpressionAst ifTrue = ExpressionRule(endNumberOnTernaryOpChars: true);
                if (ifTrue == null)
                    // ErrorRecovery: create an error expression to fill out the ast and keep parsing.
                    IScriptExtent extent = After(token);
                        extent,
                    ifTrue = new ErrorExpressionAst(extent);
                if (token.Kind != TokenKind.Colon)
                    var componentAsts = new List<Ast>() { condition };
                    // ErrorRecovery: we have done the expression parsing and should try parsing something else.
                    // Don't bother reporting this error if we already reported an empty 'IfTrue' operand error.
                    if (ifTrue is not ErrorExpressionAst)
                        componentAsts.Add(ifTrue);
                            nameof(ParserStrings.MissingColonInTernaryExpression),
                            ParserStrings.MissingColonInTernaryExpression);
                    return new ErrorExpressionAst(ExtentOf(condition, Before(token)), componentAsts);
                ExpressionAst ifFalse = ExpressionRule(endNumberOnTernaryOpChars: true);
                if (ifFalse == null)
                    ifFalse = new ErrorExpressionAst(extent);
                return new TernaryExpressionAst(ExtentOf(condition, ifFalse), condition, ifTrue, ifFalse);
        /// <summary>Parse a binary expression.</summary>
        private ExpressionAst BinaryExpressionRule(bool endNumberOnTernaryOpChars = false)
            // G  binary-expression:
            // G      bitwise-expression
            // G      binary-expression   '-and'   new-lines:opt   bitwise-expression
            // G      binary-expression   '-or'   new-lines:opt   bitwise-expression
            // G      binary-expression   '-xor'   new-lines:opt   bitwise-expression
            // G  bitwise-expression:
            // G      comparison-expression
            // G      bitwise-expression   '-band'   new-lines:opt   comparison-expression
            // G      bitwise-expression   '-bor'   new-lines:opt   comparison-expression
            // G      bitwise-expression   '-bxor'   new-lines:opt   comparison-expression
            // G  comparison-expression:
            // G      nullcoalesce-expression
            // G      comparison-expression   comparison-operator   new-lines:opt   nullcoalesce-expression
            // G  nullcoalesce-expression:
            // G      additive-expression
            // G      nullcoalesce-expression   '??'   new-lines:opt   additive-expression
            // G  additive-expression:
            // G      multiplicative-expression
            // G      additive-expression   '+'   new-lines:opt   multiplicative-expression
            // G      additive-expression   dash   new-lines:opt   multiplicative-expression
            // G  multiplicative-expression:
            // G      format-expression
            // G      multiplicative-expression   '*'   new-lines:opt   format-expression
            // G      multiplicative-expression   '/'   new-lines:opt   format-expression
            // G      multiplicative-expression   '%'   new-lines:opt   format-expression
            // G  format-expression:
            // G      range-expression
            // G      format-expression   format-operator    new-lines:opt   range-expression
            // G  range-expression:
            // G      array-literal-expression
            // G      range-expression   '..'   new-lines:opt   array-literal-expression
                ExpressionAst lhs, rhs;
                ExpressionAst expr = ArrayLiteralRule(endNumberOnTernaryOpChars);
                ParameterToken paramToken;
                if (!token.Kind.HasTrait(TokenFlags.BinaryOperator))
                    paramToken = token as ParameterToken;
                    if (paramToken != null)
                        return ErrorRecoveryParameterInExpression(paramToken, expr);
                else if (token.Kind == TokenKind.AndAnd || token.Kind == TokenKind.OrOr)
                Stack<ExpressionAst> operandStack = new Stack<ExpressionAst>();
                Stack<Token> operatorStack = new Stack<Token>();
                operandStack.Push(expr);
                operatorStack.Push(token);
                int precedence = token.Kind.GetBinaryPrecedence();
                    // We have seen a binary operator token and now expecting the right-hand-side expression.
                    expr = ArrayLiteralRule(endNumberOnTernaryOpChars: true);
                        // Use token.Text, not token.Kind.Text() b/c the kind might not match the actual operator used
                        // when a case insensitive operator is used.
                        expr = new ErrorExpressionAst(extent);
                        // Remember the token that we stopped on, but only parameters, used for error recovery
                    int newPrecedence = token.Kind.GetBinaryPrecedence();
                    while (newPrecedence <= precedence)
                        rhs = operandStack.Pop();
                        lhs = operandStack.Pop();
                        Token op = operatorStack.Pop();
                        operandStack.Push(new BinaryExpressionAst(ExtentOf(lhs, rhs), lhs, op.Kind, rhs, op.Extent));
                        if (operatorStack.Count == 0)
                        precedence = operatorStack.Peek().Kind.GetBinaryPrecedence();
                    precedence = newPrecedence;
                Diagnostics.Assert(operandStack.Count == operatorStack.Count, "Stacks out of sync");
                while (operandStack.Count > 0)
                    token = operatorStack.Pop();
                    rhs = new BinaryExpressionAst(ExtentOf(lhs, rhs), lhs, token.Kind, rhs, token.Extent);
                    return ErrorRecoveryParameterInExpression(paramToken, rhs);
                return rhs;
        private ExpressionAst ErrorRecoveryParameterInExpression(ParameterToken paramToken, ExpressionAst expr)
            // ErrorRecovery - when we have a parameter in an expression, eat it under the assumption
            // that it's an incomplete operator.  This simplifies analysis later, e.g. trying to autocomplete
            // operators.
            ReportError(paramToken.Extent,
                paramToken.Text);
            return new ErrorExpressionAst(ExtentOf(expr, paramToken),
                new Ast[] {
                    new CommandParameterAst(paramToken.Extent, paramToken.ParameterName, null, paramToken.Extent)});
        /// <summary>Parse an array literal expression.</summary>
        private ExpressionAst ArrayLiteralRule(bool endNumberOnTernaryOpChars = false)
            // G  array-literal-expression:
            // G      unary-expression   ','    new-lines:opt   array-literal-expression
            ExpressionAst lastExpr = UnaryExpressionRule(endNumberOnTernaryOpChars);
            if (lastExpr == null)
            ExpressionAst firstExpr = lastExpr;
            Token commaToken = PeekToken();
            if (commaToken.Kind != TokenKind.Comma || _disableCommaOperator)
                return lastExpr;
            var arrayValues = new List<ExpressionAst> { lastExpr };
            while (commaToken.Kind == TokenKind.Comma)
                // We have seen a comma token and now expecting an expression as an array element.
                lastExpr = UnaryExpressionRule(endNumberOnTernaryOpChars: true);
                    // ErrorRecovery: create an error expression for the ast and break.
                        commaToken.Text);
                    lastExpr = new ErrorExpressionAst(commaToken.Extent);
                    arrayValues.Add(lastExpr);
            return new ArrayLiteralAst(ExtentOf(firstExpr, lastExpr), arrayValues);
        /// <summary>Parse an unary expression.</summary>
        private ExpressionAst UnaryExpressionRule(bool endNumberOnTernaryOpChars = false)
            // G  unary-expression:
            // G      expression-with-unary-operator
            // G  expression-with-unary-operator:
            // G      ','   new-lines:opt   unary-expression
            // G      '-not'   new-lines:opt   unary-expression
            // G      '!'   new-lines:opt   unary-expression
            // G      '-bnot'   new-lines:opt   unary-expression
            // G      '+'   new-lines:opt   unary-expression
            // G      dash   new-lines:opt   unary-expression
            // G      pre-increment-expression
            // G      pre-decrement-expression
            // G      cast-expression
            // G      '-split'   new-lines:opt   unary-expression
            // G      '-join'   new-lines:opt   unary-expression
            // G  pre-increment-expression:
            // G      '++'   new-lines:opt   unary-expression
            // G  pre-decrement-expression:
            // G      dashdash   new-lines:opt   unary-expression
            // G  cast-expression:
            // G      type-literal   unary-expression
            ExpressionAst expr = null;
            bool oldAllowSignedNumbers = _tokenizer.AllowSignedNumbers;
            bool oldForceEndNumberOnTernaryOperators = _tokenizer.ForceEndNumberOnTernaryOpChars;
                _tokenizer.AllowSignedNumbers = true;
                _tokenizer.ForceEndNumberOnTernaryOpChars = endNumberOnTernaryOpChars;
                    // Possibly a signed number. Need to resync.
                    bool needResync = _ungotToken.Kind == TokenKind.Minus;
                    if (!needResync)
                        // A generic token possibly composed of numbers and ternary operator chars. Need to resync.
                        needResync = endNumberOnTernaryOpChars && _ungotToken.Kind == TokenKind.Generic;
                    if (needResync)
                _tokenizer.AllowSignedNumbers = oldAllowSignedNumbers;
                _tokenizer.ForceEndNumberOnTernaryOpChars = oldForceEndNumberOnTernaryOperators;
            ExpressionAst child;
            if (token.Kind.HasTrait(TokenFlags.UnaryOperator))
                if (_disableCommaOperator && token.Kind == TokenKind.Comma)
                // We have seen a unary operator token and now expecting an expression.
                child = UnaryExpressionRule(endNumberOnTernaryOpChars: true);
                    if (token.Kind == TokenKind.Comma)
                        expr = new ArrayLiteralAst(ExtentOf(token, child), new ExpressionAst[] { child });
                        expr = new UnaryExpressionAst(ExtentOf(token, child), token.Kind, child);
                    // ErrorRecovery: don't bother constructing a unary expression, but we know we must have
                    // some sort of expression, so return an error expression.
                        nameof(ParserStrings.MissingExpressionAfterOperator),
                        ParserStrings.MissingExpressionAfterOperator,
                    return new ErrorExpressionAst(token.Extent);
            else if (token.Kind == TokenKind.LBracket)
                // Possibly a type literal or attribute.
                var attributes = AttributeListRule(true);
                if (attributes == null)
                AttributeBaseAst lastAttribute = attributes.Last();
                if (lastAttribute is AttributeAst)
                    // We are now expecting a child expression.
                    if (child == null)
                        // ErrorRecovery: We have a list of attributes, and we know it's not before a param statement,
                        // so we know we must have some sort of expression.  Return an error expression then.
                            lastAttribute.Extent,
                            lastAttribute.TypeName.FullName);
                        return new ErrorExpressionAst(ExtentOf(token, lastAttribute), attributes);
                    expr = new AttributedExpressionAst(ExtentOf(lastAttribute, child), lastAttribute, child);
                        _ungotToken == null || ErrorList.Count > 0,
                        "Unexpected lookahead from AttributeListRule.");
                    // If we've looked ahead, don't go looking for a member access token, we've already issued an error,
                    // just assume we're not trying to access a member.
                    var memberAccessToken = _ungotToken != null ? null : NextMemberAccessToken(false);
                    if (memberAccessToken != null)
                        expr = CheckPostPrimaryExpressionOperators(
                            memberAccessToken,
                            new TypeExpressionAst(lastAttribute.Extent, lastAttribute.TypeName));
                        if (token.Kind != TokenKind.NewLine && token.Kind != TokenKind.Comma)
                                expr = new ConvertExpressionAst(
                                    ExtentOf(lastAttribute, child),
                                    (TypeConstraintAst)lastAttribute, child);
                    expr ??= new TypeExpressionAst(lastAttribute.Extent, lastAttribute.TypeName);
                for (int i = attributes.Count - 2; i >= 0; --i)
                    var typeConstraint = attributes[i] as TypeConstraintAst;
                    expr = typeConstraint != null
                                ? new ConvertExpressionAst(ExtentOf(typeConstraint, expr), typeConstraint, expr)
                                : new AttributedExpressionAst(ExtentOf(attributes[i], expr), attributes[i], expr);
                expr = PrimaryExpressionRule(withMemberAccess: true);
                TokenKind operation = (token.Kind == TokenKind.PlusPlus)
                                            ? TokenKind.PostfixPlusPlus
                                            : (token.Kind == TokenKind.MinusMinus)
                                                ? TokenKind.PostfixMinusMinus
                                                : TokenKind.Unknown;
                if (operation != TokenKind.Unknown)
                    expr = new UnaryExpressionAst(ExtentOf(expr, token), operation, expr);
        private ExpressionAst PrimaryExpressionRule(bool withMemberAccess)
            // G  primary-expression:
            // G      value
            // G      member-access
            // G      element-access
            // G      invocation-expression
            // G      post-increment-expression
            // G      post-decrement-expression
            // G  value:
            // G      parenthesized-expression
            // G      sub-expression
            // G      array-expression
            // G      script-block-expression
            // G      hash-literal-expression
            // G      literal
            // G      type-literal
            // G      variable
                    expr = CheckUsingVariable((VariableToken)token, withMemberAccess: withMemberAccess);
                    expr = new ConstantExpressionAst((NumberToken)token);
                    expr = ExpandableStringRule((StringExpandableToken)token);
                    expr = new StringConstantExpressionAst((StringToken)token);
                    expr = ParenthesizedExpressionRule(token);
                    expr = SubExpressionRule(token);
                    expr = HashExpressionRule(token, false /* parsingSchemaElement */ );
                    expr = ScriptBlockExpressionRule(token);
            if (!withMemberAccess)
            return CheckPostPrimaryExpressionOperators(NextMemberAccessToken(true), expr);
        private ExpressionAst CheckUsingVariable(VariableToken variableToken, bool withMemberAccess)
            var variablePath = variableToken.VariablePath;
            if (variablePath.IsDriveQualified && variablePath.DriveName.Equals("using", StringComparison.OrdinalIgnoreCase) && variablePath.UnqualifiedPath.Length > 0)
                var realVariablePath = new VariablePath(variablePath.UnqualifiedPath);
                ExpressionAst childExpr = new VariableExpressionAst(variableToken.Extent, realVariablePath, (variableToken.Kind == TokenKind.SplattedVariable));
                if (withMemberAccess)
                    childExpr = CheckPostPrimaryExpressionOperators(NextMemberAccessToken(true), childExpr);
                return new UsingExpressionAst(childExpr.Extent, childExpr);
            return new VariableExpressionAst(variableToken);
        private ExpressionAst CheckPostPrimaryExpressionOperators(Token token, ExpressionAst expr)
            while (token != null)
                // To support fluent style programming, allow newlines after the member access operator.
                if (token.Kind == TokenKind.Dot || token.Kind == TokenKind.ColonColon || token.Kind == TokenKind.QuestionDot)
                    expr = MemberAccessRule(expr, token);
                else if (token.Kind == TokenKind.LBracket || token.Kind == TokenKind.QuestionLBracket)
                    expr = ElementAccessRule(expr, token);
                token = NextMemberAccessToken(true);
        private ExpressionAst HashExpressionRule(Token atCurlyToken, bool parsingSchemaElement)
            // G  hash-literal-expression:
            // G      '@{'   new-lines:opt   hash-literal-body:opt   new-lines:opt   '}'
            // G  hash-literal-body:
            // G      hash-entry
            // G      hash-literal-body   statement-terminators   hash-entry
            // G  statement-terminators:
            // G      statement-terminator
            // G      statement-terminators   statement-terminator
            List<KeyValuePair> keyValuePairs = new List<KeyValuePair>();
                KeyValuePair pair = GetKeyValuePair(parsingSchemaElement);
                if (pair == null)
                keyValuePairs.Add(pair);
                string errorMsg;
                if (parsingSchemaElement)
                    errorId = nameof(ParserStrings.IncompletePropertyAssignmentBlock);
                    errorMsg = ParserStrings.IncompletePropertyAssignmentBlock;
                    errorId = nameof(ParserStrings.MissingEndCurlyBrace);
                    errorMsg = ParserStrings.MissingEndCurlyBrace;
                ReportIncompleteInput(After(atCurlyToken), rCurly.Extent, errorId, errorMsg);
                endExtent = Before(rCurly);
                endExtent = rCurly.Extent;
            var hashAst = new HashtableAst(ExtentOf(atCurlyToken, endExtent), keyValuePairs);
            hashAst.IsSchemaElement = parsingSchemaElement;
            return hashAst;
        private KeyValuePair GetKeyValuePair(bool parsingSchemaElement)
            // G  hash-entry:
            // G      key-expression   '='   new-lines:opt   statement
            Token equals;
            ExpressionAst key;
                key = LabelOrKeyRule();
                equals = NextToken();
            if (equals.Kind != TokenKind.Equals)
                // ErrorRecovery: Pretend we saw the '=' and a statement.
                UngetToken(equals);
                IScriptExtent errorExtent = After(key);
                    errorId = nameof(ParserStrings.MissingEqualsInPropertyAssignmentBlock);
                    errorMsg = ParserStrings.MissingEqualsInPropertyAssignmentBlock;
                    errorId = nameof(ParserStrings.MissingEqualsInHashLiteral);
                    errorMsg = ParserStrings.MissingEqualsInHashLiteral;
                ReportError(errorExtent, errorId, errorMsg);
                SyncOnError(false, TokenKind.RCurly, TokenKind.Semi, TokenKind.NewLine);
                return new KeyValuePair(key, new ErrorStatementAst(errorExtent));
                statement = StatementRule();
                    // ErrorRecovery: pretend we saw a statement and keep parsing.
                    IScriptExtent errorExtent = After(equals);
                        errorId = nameof(ParserStrings.MissingStatementInHashLiteral);
                        errorMsg = ParserStrings.MissingStatementInHashLiteral;
                    ReportIncompleteInput(errorExtent, errorId, errorMsg);
            return new KeyValuePair(key, statement);
        private ExpressionAst ScriptBlockExpressionRule(Token lCurly)
            // G  script-block-expression:
            // G      '{'   new-lines:opt   script-block   new-lines:opt   '}'
            ScriptBlockAst scriptBlockAst;
                _disableCommaOperator = false;
                scriptBlockAst = ScriptBlockRule(lCurly, isFilter: false);
            return new ScriptBlockExpressionAst(scriptBlockAst.Extent, scriptBlockAst);
        private ExpressionAst SubExpressionRule(Token firstToken)
            // G  array-expression:
            // G      '@('   new-lines:opt   statement-list:opt   new-lines:opt   ')'
            // G  sub-expression:
            // G      '$('   new-lines:opt   statement-list:opt   new-lines:opt   ')'
            IScriptExtent statementListExtent;
                statementListExtent = StatementListRule(statements, traps);
                    // ErrorRecovery: Assume only the closing paren is missing, continue as though it was present.
                        nameof(ParserStrings.MissingEndParenthesisInSubexpression),
                        ParserStrings.MissingEndParenthesisInSubexpression);
            // End extent is rparen, end of the statement list (if no rparen), or the first token (if no statements).
            IScriptExtent extent = ExtentOf(firstToken,
                                            rParen.Kind == TokenKind.RParen ? rParen.Extent : statementListExtent ?? firstToken.Extent);
            if (firstToken.Kind == TokenKind.DollarParen)
                return new SubExpressionAst(extent,
                                            new StatementBlockAst(statementListExtent ?? PositionUtilities.EmptyExtent,
                                                                  statements, traps));
            Diagnostics.Assert(firstToken.Kind == TokenKind.AtParen, "only support $() and @() here.");
            return new ArrayExpressionAst(extent,
                                          new StatementBlockAst(statementListExtent ?? PositionUtilities.EmptyExtent, statements, traps));
        private ExpressionAst ParenthesizedExpressionRule(Token lParen)
            // G  parenthesized-expression:
            // G      '('   new-lines:opt   pipeline-chain   new-lines:opt   ')'
            PipelineBaseAst pipelineAst;
            var oldDisableCommaOperator = _disableCommaOperator;
                pipelineAst = PipelineChainRule();
                if (pipelineAst == null)
                        nameof(ParserStrings.ExpectedExpression),
                        ParserStrings.ExpectedExpression);
                    pipelineAst = new ErrorStatementAst(errorPosition);
                    ReportIncompleteInput(After(pipelineAst),
            return new ParenExpressionAst(ExtentOf(lParen, ExtentFromFirstOf(rParen, pipelineAst)), pipelineAst);
        private List<ExpressionAst> ParseNestedExpressions(StringExpandableToken expandableStringToken)
            List<ExpressionAst> nestedExpressions = new List<ExpressionAst>();
            List<Token> newNestedTokens = _savingTokens ? new List<Token>() : null;
            foreach (var token in expandableStringToken.NestedTokens)
                Diagnostics.Assert(!token.HasError || ErrorList.Count > 0, "No nested tokens should have unreported errors.");
                if (varToken != null)
                    exprAst = CheckUsingVariable(varToken, false);
                    if (_savingTokens) { newNestedTokens.Add(varToken); }
                // Enable if we decide we still need to support
                //     "${}"  or "$var:"
                // else if (token.Kind == TokenKind.Unknown)
                //    // Diagnostics.Assert(token.Text.Equals("${}", StringComparison.OrdinalIgnoreCase),
                //    //    "The unknown token is only used in an expandable string when it's an empty variable name.");
                //    // TODO: Need strict-mode check at runtime.
                //    // TODO: in V2, "${}" expanded to '$', but "$var:" expanded to the empty string
                //    exprAst = new StringConstantExpressionAst(token.Extent, "$", StringConstantType.BareWord);
                //    if (_savingTokens) { newNestedTokens.Add(token); }
                    TokenizerState ts = null;
                        ts = _tokenizer.StartNestedScan((UnscannedSubExprToken)token);
                        if (_savingTokens) { newNestedTokens.AddRange(_tokenizer.TokenList); }
                        // _ungotToken is probably <EOF>, but if there were errors, it could be something
                        // else.  Either way, we don't want it.
                        _tokenizer.FinishNestedScan(ts);
                nestedExpressions.Add(exprAst);
            if (_savingTokens) { expandableStringToken.NestedTokens = new ReadOnlyCollection<Token>(newNestedTokens); }
            return nestedExpressions;
        private ExpressionAst ExpandableStringRule(StringExpandableToken strToken)
            // We need to scan the nested tokens even if there was some error. This is used by the tab completion: "pshome is $psh<tab>
            if (strToken.NestedTokens != null)
                List<ExpressionAst> nestedExpressions = ParseNestedExpressions(strToken);
                expr = new ExpandableStringExpressionAst(strToken, strToken.Value, strToken.FormatString, nestedExpressions);
                expr = new StringConstantExpressionAst(strToken);
        private ExpressionAst MemberNameRule()
            // G  member-name:
            // G      string-literal
            // G      string-literal-with-subexpression
            ExpressionAst simpleName = SimpleNameRule();
            if (token.Kind.HasTrait(TokenFlags.UnaryOperator) || token.Kind == TokenKind.LBracket)
                return UnaryExpressionRule();
            return PrimaryExpressionRule(withMemberAccess: false);
        private ExpressionAst MemberAccessRule(ExpressionAst targetExpr, Token operatorToken)
            // G  member-access: No whitespace is allowed between terms in these productions.
            // G      primary-expression   '.'   member-name
            // G      primary-expression   '::'   member-name
            // On entry, we've verified that operatorToken is not preceded by whitespace.
            CommandElementAst member = MemberNameRule();
                // ErrorRecovery: pretend we saw a property name, don't bother looking for an invocation,
                // and keep parsing.
                ReportIncompleteInput(After(operatorToken),
                    nameof(ParserStrings.MissingPropertyName),
                    ParserStrings.MissingPropertyName);
                member = GetSingleCommandArgument(CommandArgumentContext.CommandArgument) ??
                    new ErrorExpressionAst(ExtentOf(targetExpr, operatorToken));
            else if (_ungotToken == null)
                // Member name may be an incomplete token like `$a.$(Command-Name`, in which case, '_ungotToken != null'.
                // We do not look for generic args or invocation token if the member name token is recognisably incomplete.
                int resyncIndex = _tokenizer.GetRestorePoint();
                List<ITypeName> genericTypeArguments = GenericMethodArgumentsRule(resyncIndex, out Token rBracket);
                Token lParen = NextInvokeMemberToken();
                if (lParen != null)
                    // When we reach here, we either had a legit section of generic arguments (in which case, `rBracket`
                    // won't be null), or we saw `lParen` directly following the member token (in which case, `rBracket`
                    // will be null).
                    int endColumnNumber = rBracket is null ? member.Extent.EndColumnNumber : rBracket.Extent.EndColumnNumber;
                    Diagnostics.Assert(lParen.Kind == TokenKind.LParen || lParen.Kind == TokenKind.LCurly, "token kind incorrect");
                        endColumnNumber == lParen.Extent.StartColumnNumber,
                        "member and paren must be adjacent when the method is not generic");
                    return MemberInvokeRule(targetExpr, lParen, operatorToken, member, genericTypeArguments);
                else if (rBracket != null)
                    // We had a legit section of generic arguments but no 'lParen' following that, so this is not a method
                    // invocation, but an invalid indexing operation. Resync the tokenizer back to before the generic arg
                    // parsing and then continue.
                    Resync(resyncIndex);
            return new MemberExpressionAst(
                ExtentOf(targetExpr, member),
                member,
                @static: operatorToken.Kind == TokenKind.ColonColon,
                nullConditional: operatorToken.Kind == TokenKind.QuestionDot);
        private List<ITypeName> GenericMethodArgumentsRule(int resyncIndex, out Token rBracketToken)
            List<ITypeName> genericTypes = null;
            Token lBracket = NextToken();
            if (lBracket.Kind != TokenKind.LBracket)
                // We cannot avoid this Resync(); if we use PeekToken() to try to avoid a Resync(), the method called
                // after this [`NextInvokeMemberToken()` or `NextMemberAccessToken()`] will note that an _ungotToken
                // is present and assume an error state. That will cause any property accesses or non-generic method
                // calls to throw a parse error.
            // This is either a InvokeMember expression with generic type arguments, or some sort of collection index
            // on a property.
            TokenizerMode oldTokenizerMode = _tokenizer.Mode;
                Token firstToken = NextToken();
                // For method generic arguments, we only support the syntax `$var.Method[TypeName1 <, TypeName2 ...>]`,
                // not the syntax `$var.Method[[TypeName1] <, [TypeName2] ...>]`.
                // The latter syntax has been supported for type expression since the beginning, but it's ambiguous in
                // this scenario because we could be looking at an indexing operation on a property like:
                //    `$var.Property[<expression>]`
                // and the `<expression>` could start with a type expression like `[TypeName]::Method()`, or even just
                // a single type expression acting as a key to a hashtable property. Such cases will cause ambiguities.
                // It could be possible to write code that sorts out the ambiguity and continue to support the latter
                // syntax for method generic arguments, and thus to allow assembly-qualified type names. But we choose
                // not to do so because:
                //   1. that will definitely increase the complexity of the parsing code and also make it fragile;
                //   2. the latter syntax hurts readability a lot due to the number of opening/closing brackets.
                // The downside is that the assembly-qualified type names won't be supported for method generic args,
                // but that's likely not a problem in practice, and we can revisit if it turns out otherwise.
                    resyncIndex = -1;
                    genericTypes = GenericTypeArgumentsRule(firstToken, out rBracketToken);
                if (resyncIndex > 0)
            return genericTypes;
        private ExpressionAst MemberInvokeRule(
            Token lBracket,
            Token operatorToken,
            CommandElementAst member,
            IList<ITypeName> genericTypes)
            // G  invocation-expression: target-expression passed as a parameter. lBracket can be '(' or '{'.
            // G      target-expression   member-name   invoke-param-list
            // G  invoke-param-list:
            // G      '('   invoke-param-paren-list
            // G      script-block
            IScriptExtent lastExtent = null;
            List<ExpressionAst> arguments;
            if (lBracket.Kind == TokenKind.LParen)
                arguments = this.InvokeParamParenListRule(lBracket, out lastExtent);
                arguments = new List<ExpressionAst>();
                // handle the construct $x.methodName{2+2} as through it had been written $x.methodName({2+2})
                ExpressionAst argument = ScriptBlockExpressionRule(lBracket);
                arguments.Add(argument);
                lastExtent = argument.Extent;
            return new InvokeMemberExpressionAst(
                ExtentOf(targetExpr, lastExtent),
                operatorToken.Kind == TokenKind.ColonColon,
                operatorToken.Kind == TokenKind.QuestionDot,
                genericTypes);
        private List<ExpressionAst> InvokeParamParenListRule(Token lParen, out IScriptExtent lastExtent)
            // G  argument-list: '(' is passed in lParen
            // G      argument-expression-list:opt   new-lines:opt   ')'
            // G  argument-expression-list:
            // G      argument-expression
            // G      argument-expression   new-lines:opt   ','   argument-expression-list
            // G  argument-expression:
            // G      See grammar for expression - the only difference is that an
            // G      array-literal-expression is not allowed - the comma is used
            // G      to separate argument-expressions.
            Token comma = null;
            bool reportedError = false;
                    ExpressionAst argument = ExpressionRule();
                        if (comma != null)
                            // ErrorRecovery: sync at closing paren or newline.
                            ReportIncompleteInput(After(comma),
                                TokenKind.Comma.Text());
                            reportedError = true;
                    comma = NextToken();
                    if (comma.Kind != TokenKind.Comma)
                        UngetToken(comma);
                        comma = null;
                    // ErrorRecovery: pretend we saw a closing paren or curly and keep parsing.
                    if (!reportedError)
                        ReportIncompleteInput(arguments.Count > 0 ? After(arguments.Last()) : After(lParen),
                            nameof(ParserStrings.MissingEndParenthesisInMethodCall),
                            ParserStrings.MissingEndParenthesisInMethodCall);
            lastExtent = ExtentFromFirstOf(rParen, comma, arguments.LastOrDefault(), lParen);
            return arguments;
        private ExpressionAst ElementAccessRule(ExpressionAst primaryExpression, Token lBracket)
            // G  element-access:
            // G      primary-expression   '['   new-lines:opt   expression   new-lines:opt   ']'
            ExpressionAst indexExpr = null;
                indexExpr = ExpressionRule();
            if (indexExpr == null)
                // ErrorRecovery: hope we see a closing bracket.  If we don't, we'll pretend we saw
                // the closing bracket, but build an expression that can't compile.
                var errorExtent = After(lBracket);
                    nameof(ParserStrings.MissingArrayIndexExpression),
                    ParserStrings.MissingArrayIndexExpression);
                indexExpr = new ErrorExpressionAst(lBracket.Extent);
                // ErrorRecovery: just pretend we had a closing bracket and continue parsing.
                // Skip reporting the error if we've already reported a missing index.
                if (indexExpr is not ErrorExpressionAst)
                    ReportIncompleteInput(After(indexExpr),
                        nameof(ParserStrings.MissingEndSquareBracket),
                        ParserStrings.MissingEndSquareBracket);
            return new IndexExpressionAst(ExtentOf(primaryExpression, ExtentFromFirstOf(rBracket, indexExpr)), primaryExpression, indexExpr, lBracket.Kind == TokenKind.QuestionLBracket);
        #region Error Reporting
        private void SaveError(ParseError error)
                foreach (ParseError err in ErrorList)
                    if (err.ErrorId.Equals(error.ErrorId, StringComparison.Ordinal)
                        && err.Extent.EndColumnNumber == error.Extent.EndColumnNumber
                        && err.Extent.EndLineNumber == error.Extent.EndLineNumber
                        && err.Extent.StartColumnNumber == error.Extent.StartColumnNumber
                        && err.Extent.StartLineNumber == error.Extent.StartLineNumber)
            ErrorList.Add(error);
        private void SaveError(IScriptExtent extent, string errorId, string errorMsg, bool incompleteInput, params object[] args)
            AssertErrorIdCorrespondsToMsgString(errorId, errorMsg);
            if (args != null && args.Length > 0)
                errorMsg = string.Format(CultureInfo.CurrentCulture, errorMsg, args);
            ParseError errorToSave = new ParseError(extent, errorId, errorMsg, incompleteInput);
            SaveError(errorToSave);
        /// Debug assertion to ensure that all errors saved by the parser come
        /// from resource (.resx) files.
        /// <param name="errorId">The error ID string (.resx key).</param>
        /// <param name="errorMsg">The error message, which may be a template string (.resx value).</param>
        [System.Diagnostics.Conditional("ASSERTIONS_TRACE")]
        private static void AssertErrorIdCorrespondsToMsgString(string errorId, string errorMsg)
            // These types are the ones known to contain
            // strings used by the parser as errors
            Type[] resxTypes = new[]
                typeof(ParserStrings),
                typeof(DiscoveryExceptions),
                typeof(ExtendedTypeSystem),
                typeof(MshSnapInCmdletResources),
                typeof(ParameterBinderStrings)
            // Go through each resource type and see if the errorId key is in it, and whether the value corresponds to the errorMsg
            bool msgCorrespondsToString = false;
            foreach (Type resxType in resxTypes)
                string resxErrorBody = resxType.GetProperty(errorId, BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as string;
                if (string.Equals(errorMsg, resxErrorBody, StringComparison.Ordinal))
                    msgCorrespondsToString = true;
            Diagnostics.Assert(msgCorrespondsToString, $"Parser error ID \"{errorId}\" must correspond to the error message \"{errorMsg}\"");
        private static object[] arrayOfOneArg
            get { return t_arrayOfOneArg ??= new object[1]; }
        private static object[] t_arrayOfOneArg;
        private static object[] arrayOfTwoArgs
            get { return t_arrayOfTwoArgs ??= new object[2]; }
        private static object[] t_arrayOfTwoArgs;
        internal bool ReportIncompleteInput(IScriptExtent extent, string errorId, string errorMsg)
            // If the error position isn't at the end of the input, then we don't want to mark the error
            // as incomplete input.
            bool incompleteInput = _tokenizer.IsAtEndOfScript(extent, checkCommentsAndWhitespace: true);
            SaveError(extent, errorId, errorMsg, incompleteInput, null);
            return incompleteInput;
        internal bool ReportIncompleteInput(IScriptExtent extent, string errorId, string errorMsg, object arg)
            arrayOfOneArg[0] = arg;
            SaveError(extent, errorId, errorMsg, incompleteInput, arrayOfOneArg);
        internal bool ReportIncompleteInput(IScriptExtent errorPosition,
                                            IScriptExtent errorDetectedPosition,
                                            string errorMsg,
            bool incompleteInput = _tokenizer.IsAtEndOfScript(errorDetectedPosition, checkCommentsAndWhitespace: true);
            SaveError(errorPosition, errorId, errorMsg, incompleteInput, args);
        internal void ReportError(IScriptExtent extent, string errorId, string errorMsg)
            SaveError(extent, errorId, errorMsg, false, null);
        internal void ReportError(IScriptExtent extent, string errorId, string errorMsg, object arg)
            SaveError(extent, errorId, errorMsg, false, arrayOfOneArg);
        internal void ReportError(IScriptExtent extent, string errorId, string errorMsg, object arg1, object arg2)
            arrayOfTwoArgs[0] = arg1;
            arrayOfTwoArgs[1] = arg2;
            SaveError(extent, errorId, errorMsg, false, arrayOfTwoArgs);
        internal void ReportError(IScriptExtent extent, string errorId, string errorMsg, params object[] args)
            SaveError(extent, errorId, errorMsg, false, args);
        internal void ReportError(ParseError error)
            SaveError(error);
        private static void ReportErrorsAsWarnings(Collection<Exception> errors)
            var executionContext = Runspaces.Runspace.DefaultRunspace.ExecutionContext;
            if (executionContext != null && executionContext.InternalHost != null && executionContext.InternalHost.UI != null)
                        executionContext.InternalHost.UI.WriteWarningLine(error.ToString());
        #endregion Error Reporting
    #region Error related classes
    public class ParseError
        /// Creates a new parse error.
        /// <param name="extent">The IScriptExtent that represents the location of the error.</param>
        /// <param name="errorId">The error ID to associate with the error.</param>
        /// <param name="message">The message of the error.</param>
        public ParseError(IScriptExtent extent, string errorId, string message)
            : this(extent, errorId, message, false)
        internal ParseError(IScriptExtent extent, string errorId, string message, bool incompleteInput)
            Diagnostics.Assert(extent != null, "can't have a null position for a parse error");
            Diagnostics.Assert(!string.IsNullOrEmpty(message), "can't have a null error message");
            Diagnostics.Assert(!string.IsNullOrEmpty(errorId), "can't have a null error id");
            Extent = extent;
            ErrorId = errorId;
            Message = message;
            IncompleteInput = incompleteInput;
            return PositionUtilities.VerboseMessage(Extent) + Environment.NewLine + Message;
        public IScriptExtent Extent { get; }
        public string ErrorId { get; }
        public bool IncompleteInput { get; }
    #endregion Error related classes
    // Guid is {eba789d9-533b-58d4-cd1f-2e6520e3a9c2}
    [EventSource(Name = "Microsoft-PowerShell-Parser")]
    internal class ParserEventSource : EventSource
        internal static readonly ParserEventSource Log = new ParserEventSource();
        internal const int MaxScriptLengthToLog = 50;
        public void ParseStart(string FileName, int Length) { WriteEvent(1, FileName, Length); }
        public void ParseStop() { WriteEvent(2); }
        public void ResolveSymbolsStart() { WriteEvent(3); }
        public void ResolveSymbolsStop() { WriteEvent(4); }
        public void SemanticChecksStart() { WriteEvent(5); }
        public void SemanticChecksStop() { WriteEvent(6); }
        public void CheckSecurityStart(string FileName) { WriteEvent(7, FileName); }
        public void CheckSecurityStop(string FileName) { WriteEvent(8, FileName); }
        public void CompileStart(string FileName, int Length, bool Optimized) { WriteEvent(9, FileName, Length, Optimized); }
        public void CompileStop() { WriteEvent(10); }
        internal static string GetFileOrScript(string fileName, string input)
            return fileName ?? input.AsSpan(0, Math.Min(256, input.Length)).Trim().ToString();
