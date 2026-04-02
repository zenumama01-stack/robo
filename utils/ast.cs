// This file contains all of the publicly visible parts of the PowerShell abstract syntax tree.
// Any private/internal methods or properties are found in the file AstCompile.cs.
    internal interface ISupportsAssignment
        IAssignableValue GetAssignableValue();
    internal interface IAssignableValue
        /// GetValue is only called for pre/post increment/decrement or for read/modify/write assignment operators (+=, -=, etc.)
        /// It returns the expressions that holds the value of the ast.  It may append the exprs or temps lists if the return
        /// value relies on temps and other expressions.
        Expression? GetValue(Compiler compiler, List<Expression> exprs, List<ParameterExpression> temps);
        /// SetValue is called to set the result of an assignment (=) or to write back the result of
        /// a pre/post increment/decrement.  It needs to use potentially cached temps if GetValue was called first.
        Expression SetValue(Compiler compiler, Expression rhs);
    internal interface IParameterMetadataProvider
        bool HasAnyScriptBlockAttributes();
        RuntimeDefinedParameterDictionary GetParameterMetadata(bool automaticPositions, ref bool usesCmdletBinding);
        IEnumerable<Attribute> GetScriptBlockAttributes();
        IEnumerable<ExperimentalAttribute> GetExperimentalAttributes();
        bool UsesCmdletBinding();
        ReadOnlyCollection<ParameterAst> Parameters { get; }
        ScriptBlockAst Body { get; }
        #region Remoting/Invoke Command
        PowerShell GetPowerShell(ExecutionContext context, Dictionary<string, object> variables, bool isTrustedInput,
            bool filterNonUsingVariables, bool? createLocalScope, params object[] args);
        string GetWithInputHandlingForInvokeCommand();
        /// Return value is Tuple[paramText, scriptBlockText]
        Tuple<string, string> GetWithInputHandlingForInvokeCommandWithUsingExpression(Tuple<List<VariableExpressionAst>, string> usingVariablesTuple);
        #endregion Remoting/Invoke Command
    /// The abstract base class for all PowerShell abstract syntax tree nodes.
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public abstract class Ast
        /// Initialize the common fields of an ast.
        /// <param name="extent">The extent of the expression.</param>
        /// If <paramref name="extent"/> is null.
        protected Ast(IScriptExtent extent)
            if (extent == null)
                throw PSTraceSource.NewArgumentNullException(nameof(extent));
            this.Extent = extent;
        /// The extent in the source this ast represents.
        /// The parent tree for this node.
        public Ast Parent { get; private set; }
        /// Visit the Ast using a visitor that can choose how the tree traversal is performed.  This visit method is
        /// for advanced uses of the visitor pattern where an <see cref="AstVisitor"/> is insufficient.
        /// <param name="astVisitor">The visitor.</param>
        /// <returns>Returns the value returned by the visitor.</returns>
        public object Visit(ICustomAstVisitor astVisitor)
            if (astVisitor == null)
                throw PSTraceSource.NewArgumentNullException(nameof(astVisitor));
            return this.Accept(astVisitor);
        /// Visit each node in the Ast, calling the methods in <paramref name="astVisitor"/> for each node in the ast.
        public void Visit(AstVisitor astVisitor)
            this.InternalVisit(astVisitor);
        /// Traverse the entire Ast, returning all nodes in the tree for which <paramref name="predicate"/> returns true.
        /// <param name="predicate">The predicate function.</param>
        /// <param name="searchNestedScriptBlocks">Search nested functions and script block expressions.</param>
        /// <returns>A possibly empty collection of matching Ast nodes.</returns>
        public IEnumerable<Ast> FindAll(Func<Ast, bool> predicate, bool searchNestedScriptBlocks)
                throw PSTraceSource.NewArgumentNullException(nameof(predicate));
            return AstSearcher.FindAll(this, predicate, searchNestedScriptBlocks);
        /// Traverse the entire Ast, returning the first node in the tree for which <paramref name="predicate"/> returns true.
        /// <param name="predicate">The predicate.</param>
        /// <returns>The first matching node, or null if there is no match.</returns>
        public Ast Find(Func<Ast, bool> predicate, bool searchNestedScriptBlocks)
            return AstSearcher.FindFirst(this, predicate, searchNestedScriptBlocks);
        /// Formats the ast and returns a string.
            return Extent.Text;
        /// Duplicates the AST, allowing it to be composed into other ASTs.
        /// <returns>A copy of the AST, with the link to the previous parent removed.</returns>
        public abstract Ast Copy();
        /// Constructs the resultant object from the AST and returns it if it is safe.
        /// <returns>The object represented by the AST as a safe object.</returns>
        /// If <paramref name="extent"/> is deemed unsafe
        public object SafeGetValue()
            return SafeGetValue(skipHashtableSizeCheck: false);
        /// <param name="skipHashtableSizeCheck">Set to skip hashtable limit validation.</param>
        /// If <paramref name="extent"/> is deemed unsafe.
        public object SafeGetValue(bool skipHashtableSizeCheck)
                if (System.Management.Automation.Runspaces.Runspace.DefaultRunspace != null)
                    context = System.Management.Automation.Runspaces.Runspace.DefaultRunspace.ExecutionContext;
                return GetSafeValueVisitor.GetSafeValue(this, context, skipHashtableSizeCheck ? GetSafeValueVisitor.SafeValueContext.SkipHashtableSizeCheck : GetSafeValueVisitor.SafeValueContext.Default);
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, AutomationExceptions.CantConvertWithDynamicExpression, this.Extent.Text));
        /// Copy a collection of AST elements.
        /// <typeparam name="T">The actual AST type</typeparam>
        /// <param name="elements">Collection of ASTs.</param>
        internal static T[] CopyElements<T>(ReadOnlyCollection<T> elements) where T : Ast
            if (elements == null || elements.Count == 0) { return null; }
            var result = new T[elements.Count];
                result[i] = (T)elements[i].Copy();
        /// Copy a single AST element.
        /// <param name="element">An AST instance.</param>
        internal static T CopyElement<T>(T element) where T : Ast
            if (element == null) { return null; }
            return (T)element.Copy();
        // Should be protected AND internal, but C# doesn't support that.
        internal void SetParents<T>(ReadOnlyCollection<T> children)
            where T : Ast
            for (int index = 0; index < children.Count; index++)
                var child = children[index];
                SetParent(child);
        internal void SetParents<T1, T2>(ReadOnlyCollection<Tuple<T1, T2>> children)
            where T1 : Ast
            where T2 : Ast
                SetParent(child.Item1);
                SetParent(child.Item2);
        internal void SetParent(Ast child)
            if (child.Parent != null)
                throw new InvalidOperationException(ParserStrings.AstIsReused);
            Diagnostics.Assert(child.Parent == null, "Parent can only be set once");
            child.Parent = this;
        internal void ClearParent()
            this.Parent = null;
        internal abstract object Accept(ICustomAstVisitor visitor);
        internal abstract AstVisitAction InternalVisit(AstVisitor visitor);
        internal static readonly PSTypeName[] EmptyPSTypeNameArray = Array.Empty<PSTypeName>();
        internal bool IsInWorkflow()
            // Scan up the AST's parents, looking for a script block that is either
            // a workflow, or has a job definition attribute.
            // Stop scanning when we encounter a FunctionDefinitionAst
            Ast current = this;
            bool stopScanning = false;
            while (current != null && !stopScanning)
                if (current is ScriptBlockAst scriptBlock)
                    // See if this uses the workflow keyword
                    if (scriptBlock.Parent is FunctionDefinitionAst functionDefinition)
                        stopScanning = true;
                        if (functionDefinition.IsWorkflow)
                if (current is CommandAst commandAst &&
                    string.Equals(TokenKind.InlineScript.Text(), commandAst.GetCommandName(), StringComparison.OrdinalIgnoreCase) &&
                    this != commandAst)
        internal bool HasSuspiciousContent { get; set; }
        #region Search Ancestor Ast
        internal static ConfigurationDefinitionAst GetAncestorConfigurationDefinitionAstAndDynamicKeywordStatementAst(
            ConfigurationDefinitionAst configAst = null;
            keywordAst = GetAncestorAst<DynamicKeywordStatementAst>(ast);
            configAst = (keywordAst != null) ? GetAncestorAst<ConfigurationDefinitionAst>(keywordAst) : GetAncestorAst<ConfigurationDefinitionAst>(ast);
            return configAst;
        internal static HashtableAst GetAncestorHashtableAst(Ast ast, out Ast lastChildOfHashtable)
            lastChildOfHashtable = null;
                lastChildOfHashtable = ast;
            return hashtableAst;
        internal static TypeDefinitionAst GetAncestorTypeDefinitionAst(Ast ast)
            TypeDefinitionAst typeDefinitionAst = null;
                typeDefinitionAst = ast as TypeDefinitionAst;
                // Nested function isn't really a member of the type so stop looking
                // Anonymous script blocks are though
                if (ast is FunctionDefinitionAst functionDefinitionAst && functionDefinitionAst.Parent is not FunctionMemberAst)
            return typeDefinitionAst;
        /// Get ancestor Ast of the given type of the given ast.
        internal static T GetAncestorAst<T>(Ast ast) where T : Ast
            T targetAst = null;
                targetAst = parent as T;
                if (targetAst != null)
            return targetAst;
    // A dummy class to hold an extent for open/close curlies so we can step in the debugger.
    // This Ast is never produced by the parser, only from the compiler.
    internal class SequencePointAst : Ast
        public SequencePointAst(IScriptExtent extent)
            : base(extent)
        /// Copy the SequencePointAst instance.
            Diagnostics.Assert(false, "code should be unreachable");
            return visitor.CheckForPostAction(this, AstVisitAction.Continue);
    /// A placeholder statement used when there are syntactic errors in the source script.
    public class ErrorStatementAst : PipelineBaseAst
        internal ErrorStatementAst(IScriptExtent extent, IEnumerable<Ast> nestedAsts = null)
            if (nestedAsts != null && nestedAsts.Any())
                NestedAst = new ReadOnlyCollection<Ast>(nestedAsts.ToArray());
                SetParents(NestedAst);
        internal ErrorStatementAst(IScriptExtent extent, Token kind, IEnumerable<Ast> nestedAsts = null)
            if (kind == null)
                throw PSTraceSource.NewArgumentNullException(nameof(kind));
        internal ErrorStatementAst(IScriptExtent extent, Token kind, IEnumerable<KeyValuePair<string, Tuple<Token, Ast>>> flags, IEnumerable<Ast> conditions, IEnumerable<Ast> bodies)
            if (flags != null && flags.Any())
                Flags = new Dictionary<string, Tuple<Token, Ast>>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, Tuple<Token, Ast>> entry in flags)
                    if (Flags.ContainsKey(entry.Key))
                    Flags.Add(entry.Key, entry.Value);
                    if (entry.Value.Item2 != null)
                        SetParent(entry.Value.Item2);
            if (conditions != null && conditions.Any())
                Conditions = new ReadOnlyCollection<Ast>(conditions.ToArray());
                SetParents(Conditions);
            if (bodies != null && bodies.Any())
                Bodies = new ReadOnlyCollection<Ast>(bodies.ToArray());
                SetParents(Bodies);
        /// Indicate the kind of the ErrorStatement. e.g. Kind == Switch means that this error statement is generated
        /// when parsing a switch statement.
        public Token Kind { get; }
        /// The flags specified and their value. The value is null if it's not specified.
        /// e.g. switch -regex -file c:\demo.txt  --->   regex -- null
        ///                                              file  -- { c:\demo.txt }
        /// TODO, Changing this to an IDictionary because ReadOnlyDictionary is available only in .NET 4.5
        /// This is a temporary workaround and will be fixed later. Tracked by Win8: 354135
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Dictionary<string, Tuple<Token, Ast>> Flags { get; }
        /// The conditions specified.
        public ReadOnlyCollection<Ast> Conditions { get; }
        /// The bodies specified.
        public ReadOnlyCollection<Ast> Bodies { get; }
        /// Sometimes a valid ast is parsed successfully within the extent that this error statement represents.  Those
        /// asts are contained in this collection.  This collection may contain other error asts.  This collection may
        /// be null when no asts were successfully constructed within the extent of this error ast.
        public ReadOnlyCollection<Ast> NestedAst { get; }
        /// Copy the ErrorStatementAst instance.
            if (this.Kind == null)
                var newNestedAst = CopyElements(this.NestedAst);
                return new ErrorStatementAst(this.Extent, newNestedAst);
            else if (Flags != null || Conditions != null || Bodies != null)
                var newConditions = CopyElements(this.Conditions);
                var newBodies = CopyElements(this.Bodies);
                Dictionary<string, Tuple<Token, Ast>> newFlags = null;
                if (this.Flags != null)
                    newFlags = new Dictionary<string, Tuple<Token, Ast>>(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, Tuple<Token, Ast>> entry in this.Flags)
                        var newAst = CopyElement(entry.Value.Item2);
                        newFlags.Add(entry.Key, new Tuple<Token, Ast>(entry.Value.Item1, newAst));
                return new ErrorStatementAst(this.Extent, this.Kind, newFlags, newConditions, newBodies);
                return new ErrorStatementAst(this.Extent, this.Kind, newNestedAst);
        #region Visitors
            return visitor.VisitErrorStatement(this);
            var action = visitor.VisitErrorStatement(this);
            if (action == AstVisitAction.SkipChildren)
            if (action == AstVisitAction.Continue && NestedAst != null)
                for (int index = 0; index < NestedAst.Count; index++)
                    var ast = NestedAst[index];
                    action = ast.InternalVisit(visitor);
                    if (action != AstVisitAction.Continue) break;
            if (action == AstVisitAction.Continue && Flags != null)
                foreach (var tuple in Flags.Values)
                    if (tuple.Item2 == null)
                    action = tuple.Item2.InternalVisit(visitor);
            if (action == AstVisitAction.Continue && Conditions != null)
                for (int index = 0; index < Conditions.Count; index++)
                    var ast = Conditions[index];
            if (action == AstVisitAction.Continue && Bodies != null)
                for (int index = 0; index < Bodies.Count; index++)
                    var ast = Bodies[index];
            return visitor.CheckForPostAction(this, action);
        #endregion Visitors
    /// A placeholder expression used when there are syntactic errors in the source script.
    public class ErrorExpressionAst : ExpressionAst
        internal ErrorExpressionAst(IScriptExtent extent, IEnumerable<Ast> nestedAsts = null)
        /// Sometimes a valid ast is parsed successfully within the extent that this error expression represents.  Those
        /// Copy the ErrorExpressionAst instance.
            return new ErrorExpressionAst(this.Extent, newNestedAst);
            return visitor.VisitErrorExpression(this);
            var action = visitor.VisitErrorExpression(this);
    public class ScriptRequirements
        internal static readonly ReadOnlyCollection<PSSnapInSpecification> EmptySnapinCollection =
            Utils.EmptyReadOnlyCollection<PSSnapInSpecification>();
        internal static readonly ReadOnlyCollection<string> EmptyAssemblyCollection =
            Utils.EmptyReadOnlyCollection<string>();
        internal static readonly ReadOnlyCollection<ModuleSpecification> EmptyModuleCollection =
            Utils.EmptyReadOnlyCollection<ModuleSpecification>();
        internal static readonly ReadOnlyCollection<string> EmptyEditionCollection =
        /// The application id this script requires, specified like:
        ///     <code>#requires -Shellid Shell</code>
        /// If no application id has been specified, this property is null.
        public string RequiredApplicationId { get; internal set; }
        /// The PowerShell version this script requires, specified like:
        ///     <code>#requires -Version 3</code>
        /// If no version has been specified, this property is null.
        public Version RequiredPSVersion { get; internal set; }
        /// The PowerShell Edition this script requires, specified like:
        ///     <code>#requires -PSEdition Desktop</code>
        /// If no PSEdition has been specified, this property is an empty collection.
        public ReadOnlyCollection<string> RequiredPSEditions { get; internal set; }
        /// The modules this script requires, specified like:
        ///     <code>#requires -Module NetAdapter</code>
        ///     <code>#requires -Module @{Name="NetAdapter"; Version="1.0.0.0"}</code>
        /// If no modules are required, this property is an empty collection.
        public ReadOnlyCollection<ModuleSpecification> RequiredModules { get; internal set; }
        /// The assemblies this script requires, specified like:
        ///     <code>#requires -Assembly path\to\foo.dll</code>
        ///     <code>#requires -Assembly "System.Management.Automation, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"</code>
        /// If no assemblies are required, this property is an empty collection.
        public ReadOnlyCollection<string> RequiredAssemblies { get; internal set; }
        /// Specifies if this script requires elevated privileges, specified like:
        ///     <code>#requires -RunAsAdministrator</code>
        /// If nothing is specified, this property is false.
        public bool IsElevationRequired { get; internal set; }
    /// A ScriptBlockAst is the root ast node for a complete script.
    public class ScriptBlockAst : Ast, IParameterMetadataProvider
        private static readonly ReadOnlyCollection<AttributeAst> s_emptyAttributeList =
            Utils.EmptyReadOnlyCollection<AttributeAst>();
        private static readonly ReadOnlyCollection<UsingStatementAst> s_emptyUsingStatementList =
            Utils.EmptyReadOnlyCollection<UsingStatementAst>();
        internal bool HadErrors { get; set; }
        internal bool IsConfiguration { get; private set; }
        internal bool PostParseChecksPerformed { get; set; }
        /// Construct a ScriptBlockAst that uses explicitly named begin/process/end blocks.
        /// <param name="extent">The extent of the script block.</param>
        /// <param name="usingStatements">The list of using statements, may be null.</param>
        /// <param name="attributes">The set of attributes for the script block.</param>
        /// <param name="paramBlock">The ast for the param block, may be null.</param>
        /// <param name="beginBlock">The ast for the begin block, may be null.</param>
        /// <param name="processBlock">The ast for the process block, may be null.</param>
        /// <param name="endBlock">The ast for the end block, may be null.</param>
        /// <param name="dynamicParamBlock">The ast for the dynamicparam block, may be null.</param>
        public ScriptBlockAst(IScriptExtent extent,
                              IEnumerable<UsingStatementAst> usingStatements,
                              IEnumerable<AttributeAst> attributes,
                              ParamBlockAst paramBlock,
                              NamedBlockAst beginBlock,
                              NamedBlockAst processBlock,
                              NamedBlockAst endBlock,
                              NamedBlockAst dynamicParamBlock)
                attributes,
                paramBlock,
                cleanBlock: null,
                dynamicParamBlock)
        /// Initializes a new instance of the <see cref="ScriptBlockAst"/> class.
        /// This construction uses explicitly named begin/process/end/clean blocks.
        /// <param name="cleanBlock">The ast for the clean block, may be null.</param>
        public ScriptBlockAst(
            IScriptExtent extent,
            NamedBlockAst cleanBlock,
            SetUsingStatements(usingStatements);
                this.Attributes = new ReadOnlyCollection<AttributeAst>(attributes.ToArray());
                SetParents(Attributes);
                this.Attributes = s_emptyAttributeList;
                this.ParamBlock = paramBlock;
                SetParent(paramBlock);
                this.BeginBlock = beginBlock;
                SetParent(beginBlock);
                this.ProcessBlock = processBlock;
                SetParent(processBlock);
                this.EndBlock = endBlock;
                SetParent(endBlock);
            if (cleanBlock != null)
                this.CleanBlock = cleanBlock;
                SetParent(cleanBlock);
            if (dynamicParamBlock != null)
                this.DynamicParamBlock = dynamicParamBlock;
                SetParent(dynamicParamBlock);
            : this(extent, usingStatements, null, paramBlock, beginBlock, processBlock, endBlock, dynamicParamBlock)
            : this(extent, usingStatements, null, paramBlock, beginBlock, processBlock, endBlock, cleanBlock, dynamicParamBlock)
            : this(extent, null, paramBlock, beginBlock, processBlock, endBlock, dynamicParamBlock)
            : this(extent, null, paramBlock, beginBlock, processBlock, endBlock, cleanBlock, dynamicParamBlock)
        /// Construct a ScriptBlockAst that does not use explicitly named blocks.
        /// <param name="statements">
        /// The statements that go in the end block if <paramref name="isFilter"/> is false, or the
        /// process block if <paramref name="isFilter"/> is true.
        /// <param name="isFilter">True if the script block is a filter, false if it is a function or workflow.</param>
        /// If <paramref name="extent"/> or <paramref name="statements"/> is null.
        public ScriptBlockAst(IScriptExtent extent, List<UsingStatementAst> usingStatements, ParamBlockAst paramBlock, StatementBlockAst statements, bool isFilter)
            : this(extent, usingStatements, null, paramBlock, statements, isFilter, false)
        public ScriptBlockAst(IScriptExtent extent, ParamBlockAst paramBlock, StatementBlockAst statements, bool isFilter)
            : this(extent, null, null, paramBlock, statements, isFilter, false)
        /// <param name="isConfiguration">True if the script block is a configuration.</param>
        public ScriptBlockAst(IScriptExtent extent, ParamBlockAst paramBlock, StatementBlockAst statements, bool isFilter, bool isConfiguration)
            : this(extent, null, null, paramBlock, statements, isFilter, isConfiguration)
        public ScriptBlockAst(IScriptExtent extent, IEnumerable<UsingStatementAst> usingStatements, ParamBlockAst paramBlock, StatementBlockAst statements, bool isFilter, bool isConfiguration)
            : this(extent, usingStatements, null, paramBlock, statements, isFilter, isConfiguration)
        /// <param name="attributes">The attributes for the script block.</param>
        public ScriptBlockAst(IScriptExtent extent, IEnumerable<AttributeAst> attributes, ParamBlockAst paramBlock, StatementBlockAst statements, bool isFilter, bool isConfiguration)
            : this(extent, null, attributes, paramBlock, statements, isFilter, isConfiguration)
        public ScriptBlockAst(IScriptExtent extent, IEnumerable<UsingStatementAst> usingStatements, IEnumerable<AttributeAst> attributes, ParamBlockAst paramBlock, StatementBlockAst statements, bool isFilter, bool isConfiguration)
            if (statements == null)
                throw PSTraceSource.NewArgumentNullException(nameof(statements));
            if (isFilter)
                this.ProcessBlock = new NamedBlockAst(statements.Extent, TokenKind.Process, statements, true);
                SetParent(ProcessBlock);
                this.EndBlock = new NamedBlockAst(statements.Extent, TokenKind.End, statements, true);
                this.IsConfiguration = isConfiguration;
                SetParent(EndBlock);
        private void SetUsingStatements(IEnumerable<UsingStatementAst> usingStatements)
            if (usingStatements != null)
                this.UsingStatements = new ReadOnlyCollection<UsingStatementAst>(usingStatements.ToArray());
                SetParents(UsingStatements);
                this.UsingStatements = s_emptyUsingStatementList;
        /// The asts for attributes (such as [DscLocalConfigurationManager()]) used before the scriptblock.
        /// This property is never null.
        public ReadOnlyCollection<AttributeAst> Attributes { get; }
        /// The asts for any using statements.  This property is never null.
        /// Elements of the collection are instances of either <see cref="UsingStatementAst"/>
        /// or (only in error cases) <see cref="ErrorStatementAst"/>.
        public ReadOnlyCollection<UsingStatementAst> UsingStatements { get; private set; }
        /// The ast representing the parameters for a script block, or null if no param block was specified.
        public ParamBlockAst ParamBlock { get; }
        /// The ast representing the begin block for a script block, or null if no begin block was specified.
        public NamedBlockAst BeginBlock { get; }
        /// The ast representing the process block for a script block, or null if no process block was specified.
        public NamedBlockAst ProcessBlock { get; }
        /// The ast representing the end block for a script block, or null if no end block was specified.
        public NamedBlockAst EndBlock { get; }
        /// Gets the ast representing the clean block for a script block, or null if no clean block was specified.
        public NamedBlockAst CleanBlock { get; }
        /// The ast representing the dynamicparam block for a script block, or null if no dynamicparam block was specified.
        public NamedBlockAst DynamicParamBlock { get; }
        /// All of the parsed information from any #requires in the script, or null if #requires was not used.
        /// This property is only set for the top level script block (where <see cref="Ast.Parent"/>) is null.
        public ScriptRequirements ScriptRequirements { get; internal set; }
        /// Return the help content, if any, for the script block.
        public CommentHelpInfo GetHelpContent()
            var commentTokens = HelpCommentsParser.GetHelpCommentTokens(this, scriptBlockTokenCache);
            if (commentTokens != null)
                return HelpCommentsParser.GetHelpContents(commentTokens.Item1, commentTokens.Item2);
        /// Convert the ast into a script block that can be invoked.
        /// Thrown if there are any semantic errors in the ast.
        public ScriptBlock GetScriptBlock()
            if (!PostParseChecksPerformed)
                // we call PerformPostParseChecks on root ScriptBlockAst, to obey contract of SymbolResolver.
                // It needs to be run from the top of the tree.
                // It's ok to report an error from a different part of AST in this case.
                var root = GetRootScriptBlockAst();
                root.PerformPostParseChecks(parser);
            if (HadErrors)
                throw new PSInvalidOperationException();
            return new ScriptBlock(this, isFilter: false);
        private ScriptBlockAst GetRootScriptBlockAst()
            ScriptBlockAst rootScriptBlockAst = this;
            ScriptBlockAst parent;
            while ((parent = Ast.GetAncestorAst<ScriptBlockAst>(rootScriptBlockAst.Parent)) != null)
                rootScriptBlockAst = parent;
            return rootScriptBlockAst;
        /// Copy the ScriptBlockAst instance.
            var newParamBlock = CopyElement(this.ParamBlock);
            var newBeginBlock = CopyElement(this.BeginBlock);
            var newProcessBlock = CopyElement(this.ProcessBlock);
            var newEndBlock = CopyElement(this.EndBlock);
            var newCleanBlock = CopyElement(this.CleanBlock);
            var newDynamicParamBlock = CopyElement(this.DynamicParamBlock);
            var newAttributes = CopyElements(this.Attributes);
            var newUsingStatements = CopyElements(this.UsingStatements);
                this.Extent,
                newUsingStatements,
                newAttributes,
                newParamBlock,
                newBeginBlock,
                newProcessBlock,
                newEndBlock,
                newCleanBlock,
                newDynamicParamBlock)
                IsConfiguration = this.IsConfiguration,
                ScriptRequirements = this.ScriptRequirements
        internal string ToStringForSerialization()
            string result = this.ToString();
                // Parent is FunctionDefinitionAst or ScriptBlockExpressionAst
                // The extent includes curlies which we want to exclude.
                Diagnostics.Assert(result[0] == '{' && result[result.Length - 1] == '}',
                    "There is an incorrect assumption about the extent.");
        internal string ToStringForSerialization(Tuple<List<VariableExpressionAst>, string> usingVariablesTuple, int initialStartOffset, int initialEndOffset)
            Diagnostics.Assert(usingVariablesTuple.Item1 != null && usingVariablesTuple.Item1.Count > 0 && !string.IsNullOrEmpty(usingVariablesTuple.Item2),
                               "Caller makes sure the value passed in is not null or empty");
            Diagnostics.Assert(initialStartOffset < initialEndOffset && initialStartOffset >= this.Extent.StartOffset && initialEndOffset <= this.Extent.EndOffset,
                               "Caller makes sure the section is within the ScriptBlockAst");
            List<VariableExpressionAst> usingVars = usingVariablesTuple.Item1; // A list of using variables
            string newParams = usingVariablesTuple.Item2; // The new parameters are separated by the comma
            // astElements contains
            //  -- UsingVariable
            //  -- ParamBlockAst
            var astElements = new List<Ast>(usingVars);
            if (ParamBlock != null)
                astElements.Add(ParamBlock);
            int indexOffset = this.Extent.StartOffset;
            int startOffset = initialStartOffset - indexOffset;
            int endOffset = initialEndOffset - indexOffset;
            string script = this.ToString();
            var newScript = new StringBuilder();
            foreach (var ast in astElements.OrderBy(static ast => ast.Extent.StartOffset))
                int astStartOffset = ast.Extent.StartOffset - indexOffset;
                int astEndOffset = ast.Extent.EndOffset - indexOffset;
                // Skip the ast that is before the section that we care about
                if (astStartOffset < startOffset) { continue; }
                // We are done processing the section that we care about
                if (astStartOffset >= endOffset) { break; }
                if (ast is VariableExpressionAst varAst)
                    VariablePath varPath = varAst.VariablePath;
                    string varName = varPath.IsDriveQualified ? $"{varPath.DriveName}_{varPath.UnqualifiedPath}" : $"{varPath.UnqualifiedPath}";
                    string varSign = varAst.Splatted ? "@" : "$";
                    string newVarName = varSign + UsingExpressionAst.UsingPrefix + varName;
                    newScript.Append(script.AsSpan(startOffset, astStartOffset - startOffset));
                    newScript.Append(newVarName);
                    startOffset = astEndOffset;
                    var paramAst = ast as ParamBlockAst;
                    Diagnostics.Assert(paramAst != null, "The elements in astElements are either ParamBlockAst or VariableExpressionAst");
                    int currentOffset;
                    if (paramAst.Parameters.Count == 0)
                        currentOffset = astEndOffset - 1;
                        var firstParam = paramAst.Parameters[0];
                        currentOffset = firstParam.Attributes.Count == 0 ? firstParam.Name.Extent.StartOffset - indexOffset : firstParam.Attributes[0].Extent.StartOffset - indexOffset;
                        newParams += ",\n";
                    newScript.Append(script.AsSpan(startOffset, currentOffset - startOffset));
                    newScript.Append(newParams);
                    startOffset = currentOffset;
            newScript.Append(script.AsSpan(startOffset, endOffset - startOffset));
            string result = newScript.ToString();
            if (Parent != null && initialStartOffset == this.Extent.StartOffset && initialEndOffset == this.Extent.EndOffset)
        internal void PerformPostParseChecks(Parser parser)
            if (etwEnabled) ParserEventSource.Log.ResolveSymbolsStart();
            SymbolResolver.ResolveSymbols(parser, this);
                ParserEventSource.Log.ResolveSymbolsStop();
                ParserEventSource.Log.SemanticChecksStart();
            SemanticChecks.CheckAst(parser, this);
            if (etwEnabled) ParserEventSource.Log.SemanticChecksStop();
            Diagnostics.Assert(PostParseChecksPerformed, "Post parse checks not set during semantic checks");
            return visitor.VisitScriptBlock(this);
            var action = visitor.VisitScriptBlock(this);
            if (visitor is AstVisitor2 visitor2)
                if (action == AstVisitAction.Continue)
                    foreach (var usingStatement in UsingStatements)
                        action = usingStatement.InternalVisit(visitor2);
                        if (action != AstVisitAction.Continue)
                    foreach (var attr in Attributes)
                        action = attr.InternalVisit(visitor2);
                _ = VisitAndShallContinue(ParamBlock) &&
                    VisitAndShallContinue(DynamicParamBlock) &&
                    VisitAndShallContinue(BeginBlock) &&
                    VisitAndShallContinue(ProcessBlock) &&
                    VisitAndShallContinue(EndBlock) &&
                    VisitAndShallContinue(CleanBlock);
            bool VisitAndShallContinue(Ast ast)
                if (ast is not null)
                return action == AstVisitAction.Continue;
        #region IParameterMetadataProvider implementation
        bool IParameterMetadataProvider.HasAnyScriptBlockAttributes()
            return Attributes.Count > 0 || ParamBlock != null && ParamBlock.Attributes.Count > 0;
        RuntimeDefinedParameterDictionary IParameterMetadataProvider.GetParameterMetadata(bool automaticPositions, ref bool usesCmdletBinding)
                return Compiler.GetParameterMetaData(ParamBlock.Parameters, automaticPositions, ref usesCmdletBinding);
            return new RuntimeDefinedParameterDictionary { Data = RuntimeDefinedParameterDictionary.EmptyParameterArray };
        IEnumerable<Attribute> IParameterMetadataProvider.GetScriptBlockAttributes()
            for (int index = 0; index < Attributes.Count; index++)
                var attributeAst = Attributes[index];
                yield return Compiler.GetAttribute(attributeAst);
                for (int index = 0; index < ParamBlock.Attributes.Count; index++)
                    var attributeAst = ParamBlock.Attributes[index];
        IEnumerable<ExperimentalAttribute> IParameterMetadataProvider.GetExperimentalAttributes()
                AttributeAst attributeAst = Attributes[index];
                ExperimentalAttribute expAttr = GetExpAttributeHelper(attributeAst);
                if (expAttr != null) { yield return expAttr; }
                    var expAttr = GetExpAttributeHelper(attributeAst);
            static ExperimentalAttribute GetExpAttributeHelper(AttributeAst attributeAst)
                AttributeAst potentialExpAttr = null;
                string expAttrTypeName = typeof(ExperimentalAttribute).FullName;
                string attrAstTypeName = attributeAst.TypeName.Name;
                if (TypeAccelerators.Get.TryGetValue(attrAstTypeName, out Type attrType) && attrType == typeof(ExperimentalAttribute))
                    potentialExpAttr = attributeAst;
                else if (expAttrTypeName.EndsWith(attrAstTypeName, StringComparison.OrdinalIgnoreCase))
                    // Handle two cases:
                    //   1. declare the attribute using full type name;
                    //   2. declare the attribute using partial type name due to 'using namespace'.
                    int expAttrLength = expAttrTypeName.Length;
                    int attrAstLength = attrAstTypeName.Length;
                    if (expAttrLength == attrAstLength || expAttrTypeName[expAttrLength - attrAstLength - 1] == '.')
                if (potentialExpAttr != null)
                        return Compiler.GetAttribute(potentialExpAttr) as ExperimentalAttribute;
                        // catch all and assume it's not a declaration of ExperimentalAttribute
        ReadOnlyCollection<ParameterAst> IParameterMetadataProvider.Parameters
            get { return (ParamBlock != null) ? this.ParamBlock.Parameters : null; }
        ScriptBlockAst IParameterMetadataProvider.Body { get { return this; } }
        #region PowerShell Conversion
        PowerShell IParameterMetadataProvider.GetPowerShell(ExecutionContext context, Dictionary<string, object> variables, bool isTrustedInput,
            bool filterNonUsingVariables, bool? createLocalScope, params object[] args)
            return ScriptBlockToPowerShellConverter.Convert(this, null, isTrustedInput, context, variables, filterNonUsingVariables, createLocalScope, args);
        string IParameterMetadataProvider.GetWithInputHandlingForInvokeCommand()
            return GetWithInputHandlingForInvokeCommandImpl(null);
        Tuple<string, string> IParameterMetadataProvider.GetWithInputHandlingForInvokeCommandWithUsingExpression(
            Tuple<List<VariableExpressionAst>, string> usingVariablesTuple)
            string additionalNewParams = usingVariablesTuple.Item2;
            string scriptBlockText = GetWithInputHandlingForInvokeCommandImpl(usingVariablesTuple);
            string paramText = null;
            if (ParamBlock == null)
                paramText = "param(" + additionalNewParams + ")" + Environment.NewLine;
            return new Tuple<string, string>(paramText, scriptBlockText);
        private string GetWithInputHandlingForInvokeCommandImpl(Tuple<List<VariableExpressionAst>, string> usingVariablesTuple)
            // do not add "$input |" to complex pipelines
            var pipelineAst = GetSimplePipeline(false, out _, out _);
                return (usingVariablesTuple == null)
                    ? this.ToStringForSerialization()
                    : this.ToStringForSerialization(usingVariablesTuple, this.Extent.StartOffset, this.Extent.EndOffset);
            // do not add "$input |" to pipelines beginning with an expression
            if (pipelineAst.PipelineElements[0] is CommandExpressionAst)
            // do not add "$input |" to commands that reference $input in their arguments
            if (AstSearcher.IsUsingDollarInput(this))
            // all checks above failed - change script into "$input | <original script>"
                string paramText = (usingVariablesTuple == null)
                    ? ParamBlock.ToString()
                    : this.ToStringForSerialization(usingVariablesTuple, ParamBlock.Extent.StartOffset, ParamBlock.Extent.EndOffset);
                sb.Append(paramText);
            sb.Append("$input |");
            string pipelineText = (usingVariablesTuple == null)
                ? pipelineAst.ToString()
                : this.ToStringForSerialization(usingVariablesTuple, pipelineAst.Extent.StartOffset, pipelineAst.Extent.EndOffset);
            sb.Append(pipelineText);
        #endregion PowerShell Conversion
        bool IParameterMetadataProvider.UsesCmdletBinding()
                usesCmdletBinding = this.ParamBlock.Attributes.Any(static attribute => typeof(CmdletBindingAttribute) == attribute.TypeName.GetReflectionAttributeType());
                if (!usesCmdletBinding)
                    usesCmdletBinding = ParamBlockAst.UsesCmdletBinding(ParamBlock.Parameters);
            return usesCmdletBinding;
        #endregion IParameterMetadataProvider implementation
        internal PipelineAst GetSimplePipeline(bool allowMultiplePipelines, out string errorId, out string errorMsg)
            if (BeginBlock != null
                || ProcessBlock != null
                || CleanBlock != null
                || DynamicParamBlock != null)
                errorId = nameof(AutomationExceptions.CanConvertOneClauseOnly);
                errorMsg = AutomationExceptions.CanConvertOneClauseOnly;
            if (EndBlock == null || EndBlock.Statements.Count < 1)
                errorId = "CantConvertEmptyPipeline";
                errorMsg = AutomationExceptions.CantConvertEmptyPipeline;
            if (EndBlock.Traps != null && EndBlock.Traps.Count > 0)
                errorId = "CantConvertScriptBlockWithTrap";
                errorMsg = AutomationExceptions.CantConvertScriptBlockWithTrap;
            // Make sure all statements are pipelines.
            if (EndBlock.Statements.Any(ast => ast is not PipelineAst))
                errorId = "CanOnlyConvertOnePipeline";
                errorMsg = AutomationExceptions.CanOnlyConvertOnePipeline;
            if (EndBlock.Statements.Count != 1 && !allowMultiplePipelines)
            errorMsg = null;
            return EndBlock.Statements[0] as PipelineAst;
    /// The ast representing the param statement in a script block.
    public class ParamBlockAst : Ast
        private static readonly ReadOnlyCollection<ParameterAst> s_emptyParameterList =
            Utils.EmptyReadOnlyCollection<ParameterAst>();
        /// Construct the ast for a param statement of a script block.
        /// <param name="extent">The extent of the param statement, from any possible attributes to the closing paren.</param>
        /// <param name="attributes">The attributes (such as [cmdletbinding()]) specified on the param statement.  May be null.</param>
        /// <param name="parameters">The parameters to the script block.  May be null.</param>
        public ParamBlockAst(IScriptExtent extent, IEnumerable<AttributeAst> attributes, IEnumerable<ParameterAst> parameters)
                this.Parameters = new ReadOnlyCollection<ParameterAst>(parameters.ToArray());
                SetParents(Parameters);
                this.Parameters = s_emptyParameterList;
        /// The asts for attributes (such as [cmdletbinding()]) used before the param keyword.
        /// The asts for the parameters of the param statement.
        public ReadOnlyCollection<ParameterAst> Parameters { get; }
        /// Copy the ParamBlockAst instance.
            var newParameters = CopyElements(this.Parameters);
            return new ParamBlockAst(this.Extent, newAttributes, newParameters);
            return visitor.VisitParamBlock(this);
            var action = visitor.VisitParamBlock(this);
                    action = attributeAst.InternalVisit(visitor);
                for (int index = 0; index < Parameters.Count; index++)
                    var paramAst = Parameters[index];
                    action = paramAst.InternalVisit(visitor);
        internal static bool UsesCmdletBinding(IEnumerable<ParameterAst> parameters)
                usesCmdletBinding = parameter.Attributes.Any(attribute => (attribute.TypeName.GetReflectionAttributeType() != null) &&
                                                                          attribute.TypeName.GetReflectionAttributeType() == typeof(ParameterAttribute));
                if (usesCmdletBinding)
    /// The ast representing a begin, process, end, or dynamicparam block in a scriptblock.  This ast is used even
    /// when the block is unnamed, in which case the block is either an end block (for functions) or process block (for filters).
    public class NamedBlockAst : Ast
        /// Construct the ast for a begin, process, end, clean, or dynamic param block.
        /// <param name="extent">
        /// The extent of the block.  If <paramref name="unnamed"/> is false, the extent includes
        /// the keyword through the closing curly, otherwise the extent is the as the extent of <paramref name="statementBlock"/>.
        /// <param name="blockName">
        /// The kind of block, must be one of:
        /// <item><see cref="TokenKind.Begin"/></item>
        /// <item><see cref="TokenKind.Process"/></item>
        /// <item><see cref="TokenKind.End"/></item>
        /// <item><see cref="TokenKind.Clean"/></item>
        /// <item><see cref="TokenKind.Dynamicparam"/></item>
        /// <param name="statementBlock">The ast for the statements in this named block.</param>
        /// <param name="unnamed">True if the block was not explicitly named.</param>
        /// If <paramref name="extent"/> or <paramref name="statementBlock"/> is null.
        /// If <paramref name="blockName"/> is not one of the valid kinds for a named block,
        /// or if <paramref name="unnamed"/> is <see langword="true"/> and <paramref name="blockName"/> is neither
        /// <see cref="TokenKind.Process"/> nor <see cref="TokenKind.End"/>.
        public NamedBlockAst(IScriptExtent extent, TokenKind blockName, StatementBlockAst statementBlock, bool unnamed)
            // Validate the block name.  If the block is unnamed, it must be an End block (for a function)
            // or Process block (for a filter).
            if (HasInvalidBlockName(blockName, unnamed))
                throw PSTraceSource.NewArgumentException(nameof(blockName));
                throw PSTraceSource.NewArgumentNullException(nameof(statementBlock));
            this.Unnamed = unnamed;
            this.BlockKind = blockName;
            var statements = statementBlock.Statements;
            this.Statements = statements;
            for (int index = 0; index < statements.Count; index++)
                var stmt = statements[index];
                stmt.ClearParent();
            SetParents(statements);
            var traps = statementBlock.Traps;
            if (traps != null && traps.Count > 0)
                this.Traps = traps;
                    trap.ClearParent();
                SetParents(traps);
            if (!unnamed)
                if (statementBlock.Extent is InternalScriptExtent statementsExtent)
                    this.OpenCurlyExtent = new InternalScriptExtent(statementsExtent.PositionHelper, statementsExtent.StartOffset, statementsExtent.StartOffset + 1);
                    this.CloseCurlyExtent = new InternalScriptExtent(statementsExtent.PositionHelper, statementsExtent.EndOffset - 1, statementsExtent.EndOffset);
        /// For a function/filter that did not explicitly name the end/process block (which is quite common),
        /// this property will return true.
        public bool Unnamed { get; }
        /// The kind of block, always one of:
        public TokenKind BlockKind { get; }
        /// The asts for all of the statements represented by this statement block.  This property is never null.
        public ReadOnlyCollection<StatementAst> Statements { get; }
        /// The asts for all of the trap statements specified by this statement block, or null if no trap statements were
        /// specified in this block.
        public ReadOnlyCollection<TrapStatementAst> Traps { get; }
        /// Copy the NamedBlockAst instance.
            var newTraps = CopyElements(this.Traps);
            var newStatements = CopyElements(this.Statements);
            var statementBlockExtent = this.Extent;
            if (this.OpenCurlyExtent != null && this.CloseCurlyExtent != null)
                // For explicitly named block, the Extent of the StatementBlockAst is not
                // the same as the Extent of the NamedBlockAst. In this case, reconstruct
                // the Extent of the StatementBlockAst from openExtent/closeExtent.
                var openExtent = (InternalScriptExtent)this.OpenCurlyExtent;
                var closeExtent = (InternalScriptExtent)this.CloseCurlyExtent;
                statementBlockExtent = new InternalScriptExtent(openExtent.PositionHelper, openExtent.StartOffset, closeExtent.EndOffset);
            var statementBlock = new StatementBlockAst(statementBlockExtent, newStatements, newTraps);
            return new NamedBlockAst(this.Extent, this.BlockKind, statementBlock, this.Unnamed);
        private static bool HasInvalidBlockName(TokenKind blockName, bool unnamed)
            return !blockName.HasTrait(TokenFlags.ScriptBlockBlockName)
                || (unnamed
                    && blockName != TokenKind.Process
                    && blockName != TokenKind.End);
        // Used by the debugger for command breakpoints
        internal IScriptExtent OpenCurlyExtent { get; }
        internal IScriptExtent CloseCurlyExtent { get; }
            return visitor.VisitNamedBlock(this);
            var action = visitor.VisitNamedBlock(this);
                action = StatementBlockAst.InternalVisit(visitor, Traps, Statements, action);
    /// The ast representing a named attribute argument.  For example, in <c>[Parameter(Mandatory=$true)]</c>, this ast
    /// represents <c>Mandatory=$true</c>.
    public class NamedAttributeArgumentAst : Ast
        /// Construct the ast for a named attribute argument.
        /// The extent of the named attribute argument, starting with the name, ending with the expression, or if the expression
        /// is omitted from the source, then ending at the end of the name.
        /// <param name="argumentName">The name of the argument specified.  May not be null or empty.</param>
        /// <param name="argument">The argument expression.  May not be null even if the expression is omitted from the source.</param>
        /// <param name="expressionOmitted">
        /// True when an explicit argument is not provided in the source, e.g. <c>[Parameter(Mandatory)]</c>.  In this case,
        /// an ast for the argument expression must still be provided.
        /// If <paramref name="extent"/>, <paramref name="argumentName"/>, or <paramref name="argument"/> is null, or if
        /// <paramref name="argumentName"/> is an empty string.
        public NamedAttributeArgumentAst(IScriptExtent extent, string argumentName, ExpressionAst argument, bool expressionOmitted)
            if (string.IsNullOrEmpty(argumentName))
                throw PSTraceSource.NewArgumentNullException(nameof(argumentName));
            SetParent(argument);
            this.ArgumentName = argumentName;
            this.ExpressionOmitted = expressionOmitted;
        /// The named argument specified by this ast, is never null or empty.
        public string ArgumentName { get; }
        /// The ast of the value of the argument specified by this ast.  This property is never null.
        public ExpressionAst Argument { get; }
        /// If the source omitted an expression, this returns true, otherwise false.  This allows a caller to distinguish
        /// the difference between <c>[Parameter(Mandatory)]</c> and <c>[Parameter(Mandatory=$true)]</c>
        public bool ExpressionOmitted { get; }
        /// Copy the NamedAttributeArgumentAst instance.
            var newArgument = CopyElement(this.Argument);
            return new NamedAttributeArgumentAst(this.Extent, this.ArgumentName, newArgument, this.ExpressionOmitted);
            return visitor.VisitNamedAttributeArgument(this);
            var action = visitor.VisitNamedAttributeArgument(this);
                action = Argument.InternalVisit(visitor);
    /// An abstract base class representing attributes that accept optional arguments, e.g. <c>[Parameter()]</c>, as well as
    /// type constraints, such as <c>[int]</c>.
    public abstract class AttributeBaseAst : Ast
        /// Initialize the common fields for an attribute.
        /// <param name="extent">The extent of the attribute, from the opening '[' to the closing ']'.</param>
        /// <param name="typeName">The type named by the attribute.</param>
        /// If <paramref name="extent"/> or <paramref name="typeName"/> is null.
        protected AttributeBaseAst(IScriptExtent extent, ITypeName typeName)
        /// The type name for the attribute.  This property is never null.
        public ITypeName TypeName { get; }
        internal abstract Attribute GetAttribute();
    /// The ast representing an attribute with optional positional and named arguments.
    public class AttributeAst : AttributeBaseAst
        private static readonly ReadOnlyCollection<ExpressionAst> s_emptyPositionalArguments =
            Utils.EmptyReadOnlyCollection<ExpressionAst>();
        private static readonly ReadOnlyCollection<NamedAttributeArgumentAst> s_emptyNamedAttributeArguments =
            Utils.EmptyReadOnlyCollection<NamedAttributeArgumentAst>();
        /// Construct an attribute ast.
        /// <param name="extent">The extent of the attribute from opening '[' to closing ']'.</param>
        /// <param name="namedArguments">The named arguments, may be null.</param>
        /// <param name="positionalArguments">The positional arguments, may be null.</param>
        /// <param name="typeName">The attribute name.</param>
        public AttributeAst(IScriptExtent extent,
                            ITypeName typeName,
                            IEnumerable<ExpressionAst> positionalArguments,
                            IEnumerable<NamedAttributeArgumentAst> namedArguments)
            : base(extent, typeName)
            if (positionalArguments != null)
                this.PositionalArguments = new ReadOnlyCollection<ExpressionAst>(positionalArguments.ToArray());
                SetParents(PositionalArguments);
                this.PositionalArguments = s_emptyPositionalArguments;
            if (namedArguments != null)
                this.NamedArguments = new ReadOnlyCollection<NamedAttributeArgumentAst>(namedArguments.ToArray());
                SetParents(NamedArguments);
                this.NamedArguments = s_emptyNamedAttributeArguments;
        /// The asts for the attribute arguments specified positionally.
        public ReadOnlyCollection<ExpressionAst> PositionalArguments { get; }
        /// The asts for the named attribute arguments.
        public ReadOnlyCollection<NamedAttributeArgumentAst> NamedArguments { get; }
        /// Copy the AttributeAst instance.
            var newPositionalArguments = CopyElements(this.PositionalArguments);
            var newNamedArguments = CopyElements(this.NamedArguments);
            return new AttributeAst(this.Extent, this.TypeName, newPositionalArguments, newNamedArguments);
            return visitor.VisitAttribute(this);
            var action = visitor.VisitAttribute(this);
                for (int index = 0; index < PositionalArguments.Count; index++)
                    var expressionAst = PositionalArguments[index];
                    action = expressionAst.InternalVisit(visitor);
                for (int index = 0; index < NamedArguments.Count; index++)
                    var namedAttributeArgumentAst = NamedArguments[index];
                    action = namedAttributeArgumentAst.InternalVisit(visitor);
        internal override Attribute GetAttribute()
            return Compiler.GetAttribute(this);
    /// The ast representing a type constraint, which is simply a typename with no arguments.
    public class TypeConstraintAst : AttributeBaseAst
        /// Construct a type constraint from a possibly not yet resolved typename.
        /// <param name="extent">The extent of the constraint, from the opening '[' to the closing ']'.</param>
        /// <param name="typeName">The type for the constraint.</param>
        public TypeConstraintAst(IScriptExtent extent, ITypeName typeName)
        /// Construct a type constraint from a <see cref="Type"/>.
        /// <param name="type">The type for the constraint.</param>
        /// If <paramref name="extent"/> or <paramref name="type"/> is null.
        public TypeConstraintAst(IScriptExtent extent, Type type)
            : base(extent, new ReflectionTypeName(type))
        /// Copy the TypeConstraintAst instance.
            return new TypeConstraintAst(this.Extent, this.TypeName);
            return visitor.VisitTypeConstraint(this);
            var action = visitor.VisitTypeConstraint(this);
            return visitor.CheckForPostAction(this, action == AstVisitAction.SkipChildren ? AstVisitAction.Continue : action);
    /// The ast representing a parameter to a script.  Parameters may appear in one of 2 places, either just after the
    /// name of the function, e.g. <c>function foo($a){}</c> or in a param statement, e.g. <c>param($a)</c>.
    public class ParameterAst : Ast
        private static readonly ReadOnlyCollection<AttributeBaseAst> s_emptyAttributeList =
            Utils.EmptyReadOnlyCollection<AttributeBaseAst>();
        /// Construct a parameter ast from the name, attributes, and default value.
        /// <param name="extent">The extent of the parameter, including the attributes and default if specified.</param>
        /// <param name="attributes">The attributes, or null if no attributes were specified.</param>
        /// <param name="defaultValue">The default value of the parameter, or null if no default value was specified.</param>
        /// If <paramref name="extent"/> or <paramref name="name"/> is null.
        public ParameterAst(IScriptExtent extent,
                            VariableExpressionAst name,
                            IEnumerable<AttributeBaseAst> attributes,
                            ExpressionAst defaultValue)
                this.Attributes = new ReadOnlyCollection<AttributeBaseAst>(attributes.ToArray());
            SetParent(name);
            if (defaultValue != null)
                SetParent(defaultValue);
        /// The asts for any attributes or type constraints specified on the parameter.
        public ReadOnlyCollection<AttributeBaseAst> Attributes { get; }
        /// The variable path for the parameter.  This property is never null.
        public VariableExpressionAst Name { get; }
        /// The ast for the default value of the parameter, or null if no default value was specified.
        public ExpressionAst DefaultValue { get; }
        /// Returns the type of the parameter.  If the parameter is constrained to be a specific type, that type is returned,
        /// otherwise <c>typeof(object)</c> is returned.
        public Type StaticType
                var typeConstraint = Attributes.OfType<TypeConstraintAst>().FirstOrDefault();
                    type = typeConstraint.TypeName.GetReflectionType();
                return type ?? typeof(object);
        /// Copy the ParameterAst instance.
            var newName = CopyElement(this.Name);
            var newDefaultValue = CopyElement(this.DefaultValue);
            return new ParameterAst(this.Extent, newName, newAttributes, newDefaultValue);
        internal string GetTooltip()
            var type = typeConstraint != null ? typeConstraint.TypeName.FullName : "object";
            return type + " " + Name.VariablePath.UserPath;
        /// Get the text that represents this ParameterAst based on the $using variables passed in.
        /// A parameter name cannot be a using variable, but its default value could contain any number of UsingExpressions, for example:
        ///     function bar ($x = (Get-X @using:defaultSettings.Parameters)) { ... }
        /// This method goes through the ParameterAst text and replace each $using variable with its new synthetic name (remove the $using prefix).
        /// This method is used when we call Invoke-Command targeting a PSv2 remote machine. In that case, we might need to call this method
        /// to process the script block text, since $using prefix cannot be recognized by PSv2.
        /// <param name="orderedUsingVar">A sorted enumerator of using variable asts, ascendingly sorted based on StartOffSet.</param>
        /// The text of the ParameterAst with $using variable being replaced with a new variable name.
        internal string GetParamTextWithDollarUsingHandling(IEnumerator<VariableExpressionAst> orderedUsingVar)
            int indexOffset = Extent.StartOffset;
            int startOffset = 0;
            int endOffset = Extent.EndOffset - Extent.StartOffset;
            string paramText = ToString();
            if (orderedUsingVar.Current == null && !orderedUsingVar.MoveNext())
                return paramText;
            var newParamText = new StringBuilder();
                var varAst = orderedUsingVar.Current;
                int astStartOffset = varAst.Extent.StartOffset - indexOffset;
                int astEndOffset = varAst.Extent.EndOffset - indexOffset;
                // Skip the VariableAst that is before section we care about
                // We are done processing the current ParameterAst
                newParamText.Append(paramText.AsSpan(startOffset, astStartOffset - startOffset));
                newParamText.Append(newVarName);
            } while (orderedUsingVar.MoveNext());
            if (startOffset == 0)
                // Nothing changed within the ParameterAst text
            newParamText.Append(paramText.AsSpan(startOffset, endOffset - startOffset));
            return newParamText.ToString();
            return visitor.VisitParameter(this);
            var action = visitor.VisitParameter(this);
                action = Name.InternalVisit(visitor);
            if (action == AstVisitAction.Continue && DefaultValue != null)
                action = DefaultValue.InternalVisit(visitor);
    /// The ast representing a block of statements.  The block of statements could be part of a script block or some other
    /// statement such as an if statement or while statement.
    public class StatementBlockAst : Ast
        private static readonly ReadOnlyCollection<StatementAst> s_emptyStatementCollection = Utils.EmptyReadOnlyCollection<StatementAst>();
        /// Construct a statement block.
        /// <param name="extent">The extent of the statement block.  If curly braces are part of the statement block (and
        /// not some other ast like in a script block), then the curly braces are included in the extent, otherwise the
        /// extent runs from the first statement or trap to the last statement or trap.</param>
        /// <param name="statements">The (possibly null) collection of statements.</param>
        /// <param name="traps">The (possibly null) collection of trap statements.</param>
        public StatementBlockAst(IScriptExtent extent, IEnumerable<StatementAst> statements, IEnumerable<TrapStatementAst> traps)
            if (statements != null)
                this.Statements = new ReadOnlyCollection<StatementAst>(statements.ToArray());
                SetParents(Statements);
                this.Statements = s_emptyStatementCollection;
            if (traps != null && traps.Any())
                this.Traps = new ReadOnlyCollection<TrapStatementAst>(traps.ToArray());
                SetParents(Traps);
        /// Copy the StatementBlockAst instance.
            return new StatementBlockAst(this.Extent, newStatements, newTraps);
            return visitor.VisitStatementBlock(this);
            var action = visitor.VisitStatementBlock(this);
            return visitor.CheckForPostAction(this, InternalVisit(visitor, Traps, Statements, action));
        internal static AstVisitAction InternalVisit(AstVisitor visitor,
                                                     AstVisitAction action)
            if (action == AstVisitAction.Continue && traps != null)
                    var trapAst = traps[index];
                    action = trapAst.InternalVisit(visitor);
            if (action == AstVisitAction.Continue && statements != null)
                    var statementAst = statements[index];
                    action = statementAst.InternalVisit(visitor);
    /// An abstract base class for any statement like an if statement or while statement.
    public abstract class StatementAst : Ast
        /// Initialize the common fields of a statement.
        /// <param name="extent">The extent of the statement.</param>
        protected StatementAst(IScriptExtent extent)
    /// Specifies type attributes.
    public enum TypeAttributes
        /// <summary>No attributes specified.</summary>
        /// <summary>The type specifies a class.</summary>
        Class = 0x01,
        /// <summary>The type specifies an interface.</summary>
        Interface = 0x02,
        /// <summary>The type specifies an enum.</summary>
        Enum = 0x04,
    /// The ast representing a type definition including attributes, base class and
    /// implemented interfaces, plus it's members.
    public class TypeDefinitionAst : StatementAst
        private static readonly ReadOnlyCollection<MemberAst> s_emptyMembersCollection =
            Utils.EmptyReadOnlyCollection<MemberAst>();
        private static readonly ReadOnlyCollection<TypeConstraintAst> s_emptyBaseTypesCollection =
            Utils.EmptyReadOnlyCollection<TypeConstraintAst>();
        /// Construct a type definition.
        /// <param name="extent">The extent of the type definition, from any attributes to the closing curly brace.</param>
        /// <param name="members">The members, or null if no members were specified.</param>
        /// <param name="typeAttributes">The attributes (like class or interface) of the type.</param>
        /// <param name="baseTypes">Base class and implemented interfaces for the type.</param>
        public TypeDefinitionAst(IScriptExtent extent, string name, IEnumerable<AttributeAst> attributes, IEnumerable<MemberAst> members, TypeAttributes typeAttributes, IEnumerable<TypeConstraintAst> baseTypes)
            if (attributes != null && attributes.Any())
                Attributes = new ReadOnlyCollection<AttributeAst>(attributes.ToArray());
                Attributes = s_emptyAttributeList;
            if (members != null && members.Any())
                Members = new ReadOnlyCollection<MemberAst>(members.ToArray());
                SetParents(Members);
                Members = s_emptyMembersCollection;
            if (baseTypes != null && baseTypes.Any())
                BaseTypes = new ReadOnlyCollection<TypeConstraintAst>(baseTypes.ToArray());
                SetParents(BaseTypes);
                BaseTypes = s_emptyBaseTypesCollection;
            this.TypeAttributes = typeAttributes;
        /// The name of the type.
        /// The asts for the custom attributes specified on the type.  This property is never null.
        /// The asts for the base types. This property is never null.
        public ReadOnlyCollection<TypeConstraintAst> BaseTypes { get; }
        /// The asts for the members of the type.  This property is never null.
        public ReadOnlyCollection<MemberAst> Members { get; }
        /// The type attributes (like class or interface) of the type.
        public TypeAttributes TypeAttributes { get; }
        /// Returns true if the type defines an enum.
        public bool IsEnum { get { return (TypeAttributes & TypeAttributes.Enum) == TypeAttributes.Enum; } }
        /// Returns true if the type defines a class.
        public bool IsClass { get { return (TypeAttributes & TypeAttributes.Class) == TypeAttributes.Class; } }
        /// Returns true if the type defines an interface.
        public bool IsInterface { get { return (TypeAttributes & TypeAttributes.Interface) == TypeAttributes.Interface; } }
                // The assert may seem a little bit confusing.
                // It's because RuntimeType is not a public class and I don't want to use Name string in assert.
                // The basic idea is that Type field should go thru 3 stages:
                // 1. null
                // 2. TypeBuilder
                // 3. RuntimeType
                // We also allow wipe type (assign to null), because there could be errors.
                Diagnostics.Assert(value == null || _type == null || _type is TypeBuilder, "Type must be assigned only once to RuntimeType");
        /// Copy the TypeDefinitionAst.
            return new TypeDefinitionAst(Extent, Name, CopyElements(Attributes), CopyElements(Members), TypeAttributes, CopyElements(BaseTypes));
            var visitor2 = visitor as ICustomAstVisitor2;
            return visitor2?.VisitTypeDefinition(this);
            var action = AstVisitAction.Continue;
                action = visitor2.VisitTypeDefinition(this);
            // REVIEW: should old visitors completely skip the attributes and
            // bodies of methods, or should they get a chance to see them.  If
            // we want to skip them, the code below needs to move up into the
            // above test 'visitor2 != null'.
                    var attribute = Attributes[index];
                    action = attribute.InternalVisit(visitor);
                for (int index = 0; index < BaseTypes.Count; index++)
                    var baseTypes = BaseTypes[index];
                    action = baseTypes.InternalVisit(visitor);
                for (int index = 0; index < Members.Count; index++)
                    var member = Members[index];
                    action = member.InternalVisit(visitor);
    /// The kind of using statement.
    public enum UsingStatementKind
        /// A parse time reference to an assembly.
        Assembly = 0,
        /// A parse time command alias.
        Command = 1,
        /// A parse time reference or alias to a module.
        Module = 2,
        /// A parse time statement that allows specifying types without their full namespace.
        Namespace = 3,
        /// A parse time type alias (type accelerator).
        Type = 4,
    /// The ast representing a using statement.
    public class UsingStatementAst : StatementAst
        /// Construct a simple using statement (one that is not a form of an alias).
        /// <param name="extent">The extent of the using statement including the using keyword.</param>
        /// <param name="kind">
        /// The kind of using statement, cannot be <see cref="System.Management.Automation.Language.UsingStatementKind.Command"/>
        /// or <see cref="System.Management.Automation.Language.UsingStatementKind.Type"/>
        /// <param name="name">The item (assembly, module, or namespace) being used.</param>
        public UsingStatementAst(IScriptExtent extent, UsingStatementKind kind, StringConstantExpressionAst name)
            if (kind == UsingStatementKind.Command || kind == UsingStatementKind.Type)
                throw PSTraceSource.NewArgumentException(nameof(kind));
            UsingStatementKind = kind;
            SetParent(Name);
        /// Construct a simple (one that is not a form of an alias) using module statement with module specification as hashtable.
        /// <param name="moduleSpecification">HashtableAst that describes <see cref="Microsoft.PowerShell.Commands.ModuleSpecification"/> object.</param>
        public UsingStatementAst(IScriptExtent extent, HashtableAst moduleSpecification)
            if (moduleSpecification == null)
                throw PSTraceSource.NewArgumentNullException(nameof(moduleSpecification));
            UsingStatementKind = UsingStatementKind.Module;
            ModuleSpecification = moduleSpecification;
            SetParent(moduleSpecification);
        /// Construct a using statement that aliases an item.
        /// The kind of using statement, cannot be <see cref="System.Management.Automation.Language.UsingStatementKind.Assembly"/>.
        /// <param name="aliasName">The name of the alias.</param>
        /// <param name="resolvedAliasAst">The item being aliased.</param>
        public UsingStatementAst(IScriptExtent extent, UsingStatementKind kind, StringConstantExpressionAst aliasName,
                                 StringConstantExpressionAst resolvedAliasAst)
            if (aliasName == null)
            if (resolvedAliasAst == null)
                throw PSTraceSource.NewArgumentNullException(nameof(resolvedAliasAst));
            if (kind == UsingStatementKind.Assembly)
            Name = aliasName;
            Alias = resolvedAliasAst;
            SetParent(Alias);
        /// Construct a using module statement that aliases an item with module specification as hashtable.
        /// <param name="moduleSpecification">The module being aliased. Hashtable that describes <see cref="Microsoft.PowerShell.Commands.ModuleSpecification"/></param>
        public UsingStatementAst(IScriptExtent extent, StringConstantExpressionAst aliasName, HashtableAst moduleSpecification)
        public UsingStatementKind UsingStatementKind { get; }
        /// When <see cref="Alias"/> is null or <see cref="ModuleSpecification"/> is null, the item being used, otherwise the alias name.
        public StringConstantExpressionAst Name { get; }
        /// The name of the item being aliased.
        /// This property is mutually exclusive with <see cref="ModuleSpecification"/> property.
        public StringConstantExpressionAst Alias { get; }
        /// Hashtable that can be converted to <see cref="Microsoft.PowerShell.Commands.ModuleSpecification"/>. Only for 'using module' case, otherwise null.
        /// This property is mutually exclusive with <see cref="Alias"/> property.
        public HashtableAst ModuleSpecification { get; }
        /// ModuleInfo about used module. Only for 'using module' case, otherwise null.
        internal PSModuleInfo ModuleInfo { get; private set; }
        /// Copy the UsingStatementAst.
            var copy = Alias != null
                ? new UsingStatementAst(Extent, UsingStatementKind, Name, Alias)
                : new UsingStatementAst(Extent, UsingStatementKind, Name);
            copy.ModuleInfo = ModuleInfo;
            return visitor2?.VisitUsingStatement(this);
                action = visitor2.VisitUsingStatement(this);
            if (ModuleSpecification != null)
                action = ModuleSpecification.InternalVisit(visitor);
            if (Alias != null)
                action = Alias.InternalVisit(visitor);
        /// Define imported module and all type definitions imported by this using statement.
        /// <returns>Return ExportedTypeTable for this module.</returns>
        internal ReadOnlyDictionary<string, TypeDefinitionAst> DefineImportedModule(PSModuleInfo moduleInfo)
            var types = moduleInfo.GetExportedTypeDefinitions();
            ModuleInfo = moduleInfo;
        /// Is UsingStatementKind Module or Assembly.
        /// <returns>True, if it is.</returns>
        internal bool IsUsingModuleOrAssembly()
            return UsingStatementKind == UsingStatementKind.Assembly || UsingStatementKind == UsingStatementKind.Module;
    /// An abstract base class for type members.
    public abstract class MemberAst : Ast
        /// Initialize the common fields of a type member.
        /// <param name="extent">The extent of the type member.</param>
        protected MemberAst(IScriptExtent extent) : base(extent)
        /// The name of the member.  This property is never null.
        internal abstract string GetTooltip();
    /// The attributes for a property.
    public enum PropertyAttributes
        /// <summary>The property is public.</summary>
        Public = 0x01,
        /// <summary>The property is private.</summary>
        Private = 0x02,
        /// <summary>The property is static.</summary>
        Static = 0x10,
        /// <summary>The property is a literal.</summary>
        Literal = 0x20,
        /// <summary>The property is a hidden.</summary>
        Hidden = 0x40,
    /// The ast for a property.
    public class PropertyMemberAst : MemberAst
        /// Construct a property member.
        /// <param name="extent">The extent of the property starting with any custom attributes.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="propertyType">The ast for the type of the property - may be null.</param>
        /// <param name="attributes">The custom attributes for the property.</param>
        /// <param name="propertyAttributes">The attributes (like public or static) for the property.</param>
        /// <param name="initialValue">The initial value of the property (may be null).</param>
        public PropertyMemberAst(IScriptExtent extent, string name, TypeConstraintAst propertyType, IEnumerable<AttributeAst> attributes, PropertyAttributes propertyAttributes, ExpressionAst initialValue)
            if ((propertyAttributes & (PropertyAttributes.Private | PropertyAttributes.Public)) ==
                (PropertyAttributes.Private | PropertyAttributes.Public))
                throw PSTraceSource.NewArgumentException(nameof(propertyAttributes));
                SetParent(PropertyType);
            PropertyAttributes = propertyAttributes;
            InitialValue = initialValue;
            if (InitialValue != null)
                SetParent(InitialValue);
        /// The name of the property.
        public override string Name { get; }
        /// The ast for the type of the property.  This property may be null if no type was specified.
        public TypeConstraintAst PropertyType { get; }
        /// The custom attributes of the property.  This property is never null.
        /// The attributes (like public or static) of the property.
        public PropertyAttributes PropertyAttributes { get; }
        /// The ast for the initial value of the property.  This property may be null if no initial value was specified.
        public ExpressionAst InitialValue { get; }
        /// Return true if the property is public.
        public bool IsPublic { get { return (PropertyAttributes & PropertyAttributes.Public) != 0; } }
        /// Return true if the property is private.
        public bool IsPrivate { get { return (PropertyAttributes & PropertyAttributes.Private) != 0; } }
        /// Return true if the property is hidden.
        public bool IsHidden { get { return (PropertyAttributes & PropertyAttributes.Hidden) != 0; } }
        /// Return true if the property is static.
        public bool IsStatic { get { return (PropertyAttributes & PropertyAttributes.Static) != 0; } }
        /// Copy the PropertyMemberAst.
            var newPropertyType = CopyElement(PropertyType);
            var newAttributes = CopyElements(Attributes);
            var newInitialValue = CopyElement(InitialValue);
            return new PropertyMemberAst(Extent, Name, newPropertyType, newAttributes, PropertyAttributes, newInitialValue);
        internal override string GetTooltip()
            var type = PropertyType != null ? PropertyType.TypeName.FullName : "object";
            return IsStatic
                ? "static " + type + " " + Name
                : type + " " + Name;
            return visitor2?.VisitPropertyMember(this);
                action = visitor2.VisitPropertyMember(this);
                if (action == AstVisitAction.Continue && PropertyType != null)
                    action = PropertyType.InternalVisit(visitor);
                if (action == AstVisitAction.Continue && InitialValue != null)
                    action = InitialValue.InternalVisit(visitor);
    /// Flags for a method.
    public enum MethodAttributes
        /// <summary>No flags specified.</summary>
        /// <summary>The method is public.</summary>
        /// <summary>The method is private.</summary>
        /// <summary>The method is static.</summary>
    /// The ast for a method.
    public class FunctionMemberAst : MemberAst, IParameterMetadataProvider
        private readonly FunctionDefinitionAst _functionDefinitionAst;
        /// Construct a member function.
        /// <param name="extent">The extent of the method starting from any attributes to the closing curly.</param>
        /// <param name="functionDefinitionAst">The main body of the method.</param>
        /// <param name="returnType">The return type of the method, may be null.</param>
        /// <param name="attributes">The custom attributes for the function.</param>
        /// <param name="methodAttributes">The method attributes like public or static.</param>
        public FunctionMemberAst(IScriptExtent extent, FunctionDefinitionAst functionDefinitionAst, TypeConstraintAst returnType, IEnumerable<AttributeAst> attributes, MethodAttributes methodAttributes)
            if (functionDefinitionAst == null)
                throw PSTraceSource.NewArgumentNullException(nameof(functionDefinitionAst));
            if ((methodAttributes & (MethodAttributes.Private | MethodAttributes.Public)) ==
                (MethodAttributes.Private | MethodAttributes.Public))
                throw PSTraceSource.NewArgumentException(nameof(methodAttributes));
            if (returnType != null)
                ReturnType = returnType;
                SetParent(returnType);
            _functionDefinitionAst = functionDefinitionAst;
            SetParent(functionDefinitionAst);
            MethodAttributes = methodAttributes;
        /// The name of the method.  This property is never null.
        public override string Name { get { return _functionDefinitionAst.Name; } }
        /// The attributes specified on the method.  This property is never null.
        /// The ast representing the return type for the method.  This property may be null if no return type was specified.
        public TypeConstraintAst ReturnType { get; }
        /// The parameters specified immediately after the function name.  This property is never null.
        public ReadOnlyCollection<ParameterAst> Parameters
            get { return _functionDefinitionAst.Parameters ?? s_emptyParameterList; }
        /// The body of the function.  This property is never null.
        public ScriptBlockAst Body { get { return _functionDefinitionAst.Body; } }
        /// Method attribute flags.
        public MethodAttributes MethodAttributes { get; }
        /// Returns true if the method is public.
        public bool IsPublic { get { return (MethodAttributes & MethodAttributes.Public) != 0; } }
        public bool IsPrivate { get { return (MethodAttributes & MethodAttributes.Private) != 0; } }
        /// Returns true if the method is hidden.
        public bool IsHidden { get { return (MethodAttributes & MethodAttributes.Hidden) != 0; } }
        /// Returns true if the method is static.
        public bool IsStatic { get { return (MethodAttributes & MethodAttributes.Static) != 0; } }
        /// Returns true if the method is a constructor.
        public bool IsConstructor
            get { return Name.Equals(((TypeDefinitionAst)Parent).Name, StringComparison.OrdinalIgnoreCase); }
        internal IScriptExtent NameExtent { get { return _functionDefinitionAst.NameExtent; } }
        private string _toolTip;
        /// Copy a function member ast.
            var newDefn = CopyElement(_functionDefinitionAst);
            var newReturnType = CopyElement(ReturnType);
            return new FunctionMemberAst(Extent, newDefn, newReturnType, newAttributes, MethodAttributes);
            if (!string.IsNullOrEmpty(_toolTip))
            var classMembers = ((TypeDefinitionAst)Parent).Members;
            for (int i = 0; i < classMembers.Count; i++)
                var methodMember = classMembers[i] as FunctionMemberAst;
                if (methodMember is null ||
                    !Name.Equals(methodMember.Name) ||
                    IsStatic != methodMember.IsStatic)
                    sb.AppendLine();
                if (methodMember.IsStatic)
                    sb.Append("static ");
                if (!methodMember.IsConstructor)
                    sb.Append(methodMember.IsReturnTypeVoid() ? "void" : methodMember.ReturnType.TypeName.FullName);
                sb.Append(methodMember.Name);
                sb.Append('(');
                for (int j = 0; j < methodMember.Parameters.Count; j++)
                    if (j > 0)
                    sb.Append(methodMember.Parameters[j].GetTooltip());
                sb.Append(')');
            _toolTip = sb.ToString();
            return visitor2?.VisitFunctionMember(this);
                action = visitor2.VisitFunctionMember(this);
                if (action == AstVisitAction.Continue && ReturnType != null)
                    action = ReturnType.InternalVisit(visitor);
                    action = _functionDefinitionAst.InternalVisit(visitor);
            return ((IParameterMetadataProvider)_functionDefinitionAst).HasAnyScriptBlockAttributes();
            get { return ((IParameterMetadataProvider)_functionDefinitionAst).Parameters; }
            return ((IParameterMetadataProvider)_functionDefinitionAst).GetParameterMetadata(automaticPositions, ref usesCmdletBinding);
            return ((IParameterMetadataProvider)_functionDefinitionAst).GetScriptBlockAttributes();
            return ((IParameterMetadataProvider)_functionDefinitionAst).GetExperimentalAttributes();
            return ((IParameterMetadataProvider)_functionDefinitionAst).UsesCmdletBinding();
            // OK?  I think this isn't reachable
        Tuple<string, string> IParameterMetadataProvider.GetWithInputHandlingForInvokeCommandWithUsingExpression(Tuple<List<VariableExpressionAst>, string> usingVariablesTuple)
        internal bool IsReturnTypeVoid()
            if (ReturnType == null)
            var typeName = ReturnType.TypeName as TypeName;
            return typeName != null && typeName.IsType(typeof(void));
        internal Type GetReturnType()
            return ReturnType == null ? typeof(void) : ReturnType.TypeName.GetReflectionType();
    internal enum SpecialMemberFunctionType
        DefaultConstructor,
        StaticConstructor,
    internal class CompilerGeneratedMemberFunctionAst : MemberAst, IParameterMetadataProvider
        internal CompilerGeneratedMemberFunctionAst(IScriptExtent extent, TypeDefinitionAst definingType, SpecialMemberFunctionType type)
            StatementAst statement = null;
            if (type == SpecialMemberFunctionType.DefaultConstructor)
                var invokeMemberAst = new BaseCtorInvokeMemberExpressionAst(extent, extent, Array.Empty<ExpressionAst>());
                statement = new CommandExpressionAst(extent, invokeMemberAst, null);
            Body = new ScriptBlockAst(extent, null, new StatementBlockAst(extent, statement == null ? null : new[] { statement }, null), false);
            this.SetParent(Body);
            definingType.SetParent(this);
            DefiningType = definingType;
            // This is fine for now, but if we add non-constructors, the name will be wrong.
            get { return DefiningType.Name; }
        internal TypeDefinitionAst DefiningType { get; }
        internal SpecialMemberFunctionType Type { get; }
            return DefiningType.Name + " new()";
            return new CompilerGeneratedMemberFunctionAst(Extent, (TypeDefinitionAst)DefiningType.Copy(), Type);
        public bool HasAnyScriptBlockAttributes()
            return ((IParameterMetadataProvider)Body).HasAnyScriptBlockAttributes();
        public RuntimeDefinedParameterDictionary GetParameterMetadata(bool automaticPositions, ref bool usesCmdletBinding)
        public IEnumerable<Attribute> GetScriptBlockAttributes()
            return ((IParameterMetadataProvider)Body).GetScriptBlockAttributes();
        public IEnumerable<ExperimentalAttribute> GetExperimentalAttributes()
            return ((IParameterMetadataProvider)Body).GetExperimentalAttributes();
        public bool UsesCmdletBinding()
        public ReadOnlyCollection<ParameterAst> Parameters { get { return null; } }
        public ScriptBlockAst Body { get; }
        public PowerShell GetPowerShell(ExecutionContext context, Dictionary<string, object> variables, bool isTrustedInput,
        public string GetWithInputHandlingForInvokeCommand()
        public Tuple<string, string> GetWithInputHandlingForInvokeCommandWithUsingExpression(Tuple<List<VariableExpressionAst>, string> usingVariablesTuple)
    /// The ast that represents a function or filter definition.  The function is always named.
    public class FunctionDefinitionAst : StatementAst, IParameterMetadataProvider
        /// Construct a function definition.
        /// The extent of the function definition, starting with the function or filter keyword, ending at the closing curly.
        /// <param name="isFilter">True if the filter keyword was used.</param>
        /// <param name="isWorkflow">True if the workflow keyword was used.</param>
        /// The parameters specified after the function name.  This does not include parameters specified with a param statement.
        /// <param name="body">The body of the function/filter.</param>
        /// If <paramref name="extent"/>, <paramref name="name"/>, or <paramref name="body"/> is null, or
        /// if <paramref name="name"/> is an empty string.
        public FunctionDefinitionAst(IScriptExtent extent,
                                     bool isFilter,
                                     bool isWorkflow,
                                     IEnumerable<ParameterAst> parameters,
                                     ScriptBlockAst body)
                throw PSTraceSource.NewArgumentNullException(nameof(body));
            if (isFilter && isWorkflow)
                throw PSTraceSource.NewArgumentException(nameof(isFilter));
            this.IsFilter = isFilter;
            this.IsWorkflow = isWorkflow;
            if (parameters != null && parameters.Any())
            this.Body = body;
            SetParent(body);
        internal FunctionDefinitionAst(IScriptExtent extent,
                                       Token functionNameToken,
            : this(extent,
                   isFilter,
                   isWorkflow,
                   (functionNameToken.Kind == TokenKind.Generic) ? ((StringToken)functionNameToken).Value : functionNameToken.Text,
                   parameters,
                   body)
            NameExtent = functionNameToken.Extent;
        /// If true, the filter keyword was used.
        public bool IsFilter { get; }
        /// If true, the workflow keyword was used.
        public bool IsWorkflow { get; }
        /// The name of the function or filter.  This property is never null or empty.
        /// The parameters specified immediately after the function name, or null if no parameters were specified.
        /// <para>It is possible that this property may have a value and <see cref="ScriptBlockAst.ParamBlock"/> to also have a
        /// value.  Normally this is not allowed in a valid script, but in one rare case it is allowed:</para>
        /// <c>function foo() { param($a) }</c>
        /// In this example, the parameters specified after the function name must be empty or the script is not valid.
        internal IScriptExtent NameExtent { get; private set; }
        /// Return the help content, if any, for the function.
        /// Use this overload when parsing multiple functions within a single scope.
        /// <param name="scriptBlockTokenCache">A dictionary that the parser will use to
        /// map AST nodes to their respective tokens. The parser uses this to improve performance
        /// while repeatedly parsing the parent script blocks of a function (since the parent
        /// script blocks may contain help comments related to this function.
        /// To conserve memory, clear / null-out this cache when done with repeated parsing.</param>
        public CommentHelpInfo GetHelpContent(Dictionary<Ast, Token[]> scriptBlockTokenCache)
            ArgumentNullException.ThrowIfNull(scriptBlockTokenCache);
        /// Copy the FunctionDefinitionAst instance.
            var newBody = CopyElement(this.Body);
            return new FunctionDefinitionAst(this.Extent, this.IsFilter, this.IsWorkflow, this.Name, newParameters, newBody) { NameExtent = this.NameExtent };
        internal string GetParamTextFromParameterList(Tuple<List<VariableExpressionAst>, string> usingVariablesTuple = null)
            Diagnostics.Assert(Parameters != null, "Caller makes sure that Parameters is not null before calling this method.");
            string additionalNewUsingParams = null;
            IEnumerator<VariableExpressionAst> orderedUsingVars = null;
            if (usingVariablesTuple != null)
                    usingVariablesTuple.Item1 != null && usingVariablesTuple.Item1.Count > 0 && !string.IsNullOrEmpty(usingVariablesTuple.Item2),
                    "Caller makes sure the value passed in is not null or empty.");
                orderedUsingVars = usingVariablesTuple.Item1.OrderBy(static varAst => varAst.Extent.StartOffset).GetEnumerator();
                additionalNewUsingParams = usingVariablesTuple.Item2;
            var sb = new StringBuilder("param(");
            if (additionalNewUsingParams != null)
                // Add the $using variable parameters if necessary
                sb.Append(additionalNewUsingParams);
            for (int i = 0; i < Parameters.Count; i++)
                var param = Parameters[i];
                sb.Append(orderedUsingVars != null
                              ? param.GetParamTextWithDollarUsingHandling(orderedUsingVars)
                              : param.ToString());
            return visitor.VisitFunctionDefinition(this);
            var action = visitor.VisitFunctionDefinition(this);
                if (Parameters != null)
                        var param = Parameters[index];
                        action = param.InternalVisit(visitor);
                    action = Body.InternalVisit(visitor);
                return Compiler.GetParameterMetaData(Parameters, automaticPositions, ref usesCmdletBinding);
            if (Body.ParamBlock != null)
                return Compiler.GetParameterMetaData(Body.ParamBlock.Parameters, automaticPositions, ref usesCmdletBinding);
            get { return Parameters ?? (Body.ParamBlock?.Parameters); }
            return ScriptBlockToPowerShellConverter.Convert(this.Body, this.Parameters, isTrustedInput, context, variables, filterNonUsingVariables, createLocalScope, args);
            string result = ((IParameterMetadataProvider)Body).GetWithInputHandlingForInvokeCommand();
            return Parameters == null ? result : (GetParamTextFromParameterList() + result);
            Tuple<string, string> result =
                ((IParameterMetadataProvider)Body).GetWithInputHandlingForInvokeCommandWithUsingExpression(usingVariablesTuple);
            if (Parameters == null)
            string paramText = GetParamTextFromParameterList(usingVariablesTuple);
            return new Tuple<string, string>(paramText, result.Item2);
                usesCmdletBinding = ParamBlockAst.UsesCmdletBinding(Parameters);
            else if (Body.ParamBlock != null)
                usesCmdletBinding = ((IParameterMetadataProvider)Body).UsesCmdletBinding();
    /// The ast that represents an if statement.
    public class IfStatementAst : StatementAst
        /// Construct an if statement.
        /// The extent of the statement, starting with the if keyword, ending at the closing curly of the last clause.
        /// <param name="clauses">
        /// A non-empty collection of pairs of condition expressions and statement blocks.
        /// <param name="elseClause">The else clause, or null if no clause was specified.</param>
        /// <exception cref="PSArgumentNullException">If <paramref name="extent"/> is null.</exception>
        /// <exception cref="PSArgumentException">If <paramref name="clauses"/> is null or empty.</exception>
        public IfStatementAst(IScriptExtent extent, IEnumerable<IfClause> clauses, StatementBlockAst elseClause)
            if (clauses == null || !clauses.Any())
                throw PSTraceSource.NewArgumentException(nameof(clauses));
            this.Clauses = new ReadOnlyCollection<IfClause>(clauses.ToArray());
            SetParents(Clauses);
                this.ElseClause = elseClause;
                SetParent(elseClause);
        /// The asts representing a pair of (condition,statements) that are tested, in sequence until the first condition
        /// tests true, in which case it's statements are executed, otherwise the <see cref="ElseClause"/>, if any, is
        /// executed.  This property is never null and always has at least 1 value.
        public ReadOnlyCollection<IfClause> Clauses { get; }
        /// The ast for the else clause, or null if no else clause is specified.
        public StatementBlockAst ElseClause { get; }
        /// Copy the IfStatementAst instance.
            var newClauses = new List<IfClause>(this.Clauses.Count);
            for (int i = 0; i < this.Clauses.Count; i++)
                var clause = this.Clauses[i];
                var newCondition = CopyElement(clause.Item1);
                var newStatementBlock = CopyElement(clause.Item2);
                newClauses.Add(Tuple.Create(newCondition, newStatementBlock));
            var newElseClause = CopyElement(this.ElseClause);
            return new IfStatementAst(this.Extent, newClauses, newElseClause);
            return visitor.VisitIfStatement(this);
            var action = visitor.VisitIfStatement(this);
                for (int index = 0; index < Clauses.Count; index++)
                    var ifClause = Clauses[index];
                    action = ifClause.Item1.InternalVisit(visitor);
                    action = ifClause.Item2.InternalVisit(visitor);
            if (action == AstVisitAction.Continue && ElseClause != null)
                action = ElseClause.InternalVisit(visitor);
    /// The ast representing the data statement.
    public class DataStatementAst : StatementAst
        private static readonly ExpressionAst[] s_emptyCommandsAllowed = Array.Empty<ExpressionAst>();
        /// Construct a data statement.
        /// <param name="extent">The extent of the data statement, extending from the data keyword to the closing curly brace.</param>
        /// <param name="variableName">The name of the variable, if specified, otherwise null.</param>
        /// <param name="commandsAllowed">The list of commands allowed in the data statement, if specified, otherwise null.</param>
        /// <param name="body">The body of the data statement.</param>
        /// If <paramref name="extent"/> or <paramref name="body"/> is null.
        public DataStatementAst(IScriptExtent extent,
                                IEnumerable<ExpressionAst> commandsAllowed,
                                StatementBlockAst body)
            if (string.IsNullOrWhiteSpace(variableName))
                variableName = null;
            this.Variable = variableName;
            if (commandsAllowed != null && commandsAllowed.Any())
                this.CommandsAllowed = new ReadOnlyCollection<ExpressionAst>(commandsAllowed.ToArray());
                SetParents(CommandsAllowed);
                this.HasNonConstantAllowedCommand = CommandsAllowed.Any(static ast => ast is not StringConstantExpressionAst);
                this.CommandsAllowed = new ReadOnlyCollection<ExpressionAst>(s_emptyCommandsAllowed);
        /// The name of the variable this data statement sets, or null if no variable name was specified.
        /// The asts naming the commands allowed to execute in this data statement.
        public ReadOnlyCollection<ExpressionAst> CommandsAllowed { get; }
        /// The ast for the body of the data statement.  This property is never null.
        public StatementBlockAst Body { get; }
        /// Copy the DataStatementAst instance.
            var newCommandsAllowed = CopyElements(this.CommandsAllowed);
            return new DataStatementAst(this.Extent, this.Variable, newCommandsAllowed, newBody);
        internal bool HasNonConstantAllowedCommand { get; }
            return visitor.VisitDataStatement(this);
            var action = visitor.VisitDataStatement(this);
        #region Code Generation Details
        internal int TupleIndex { get; set; } = VariableAnalysis.Unanalyzed;
        #endregion Code Generation Details
    #region Looping Statements
    /// An abstract base class for statements that have labels such as a while statement or a switch statement.
    public abstract class LabeledStatementAst : StatementAst
        /// Initialize the properties common to labeled statements.
        /// <param name="label">The optionally null label for the statement.</param>
        /// <param name="condition">The optionally null pipeline for the condition test of the statement.</param>
        protected LabeledStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition)
            if (string.IsNullOrWhiteSpace(label))
                label = null;
                this.Condition = condition;
                SetParent(condition);
        /// The label name if specified, otherwise null.
        /// The ast for the condition that is tested on each iteration of the loop, or the condition tested on a switch.
        /// This property may be null if the statement is a <see cref="ForStatementAst"/>, otherwise it is never null.
        public PipelineBaseAst Condition { get; }
    /// An abstract base class for looping statements including a the do/while statement, the do/until statement,
    /// the foreach statement, the for statement, and the while statement.
    public abstract class LoopStatementAst : LabeledStatementAst
        /// Initialize the properties common to all loop statements.
        /// <param name="body">The body of the statement.</param>
        protected LoopStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
            : base(extent, label, condition)
        /// The body of a loop statement.  This property is never null.
    /// Flags that are specified on a foreach statement.  Values may be or'ed together, not all invalid combinations
    /// of flags are detected.
    public enum ForEachFlags
        /// The -parallel flag.
        Parallel = 0x01,
        // If any flags are added that impact evaluation of items during the foreach statement, then
        // a binder (and caching strategy) needs to be added similar to SwitchClauseEvalBinder.
    /// The ast representing the foreach statement.
    public class ForEachStatementAst : LoopStatementAst
        /// Construct a foreach statement.
        /// The extent of the statement, starting from the optional label or the foreach keyword and ending at the closing curly brace.
        /// <param name="label">The optionally null label.</param>
        /// <param name="flags">Any flags that affect how the foreach statement is processed.</param>
        /// <param name="variable">The variable set on each iteration of the loop.</param>
        /// <param name="expression">The pipeline generating values to iterate through.</param>
        /// <param name="body">The body to execute for each element written from pipeline.</param>
        /// If <paramref name="extent"/>, <paramref name="expression"/>, or <paramref name="variable"/> is null.
        public ForEachStatementAst(IScriptExtent extent,
                                   ForEachFlags flags,
                                   VariableExpressionAst variable,
                                   PipelineBaseAst expression,
            : base(extent, label, expression, body)
            if (expression == null || variable == null)
                throw PSTraceSource.NewArgumentNullException(expression == null ? "expression" : "variablePath");
            this.Variable = variable;
            SetParent(variable);
        /// <param name="throttleLimit">The limit to be obeyed during parallel processing, if any.</param>
                                   ExpressionAst throttleLimit,
            : this(extent, label, flags, variable, expression, body)
            this.ThrottleLimit = throttleLimit;
            if (throttleLimit != null)
                SetParent(throttleLimit);
        /// The name of the variable set for each item as the loop iterates.  This property is never null.
        public VariableExpressionAst Variable { get; }
        /// The limit to be obeyed during parallel processing, if any.
        public ExpressionAst ThrottleLimit { get; }
        /// The flags, if any specified on the foreach statement.
        public ForEachFlags Flags { get; }
        /// Copy the ForEachStatementAst instance.
            var newVariable = CopyElement(this.Variable);
            var newExpression = CopyElement(this.Condition);
            if (this.ThrottleLimit != null)
                var newThrottleLimit = CopyElement(this.ThrottleLimit);
                return new ForEachStatementAst(this.Extent, this.Label, this.Flags, newThrottleLimit,
                                               newVariable, newExpression, newBody);
            return new ForEachStatementAst(this.Extent, this.Label, this.Flags, newVariable, newExpression, newBody);
            return visitor.VisitForEachStatement(this);
            var action = visitor.VisitForEachStatement(this);
                action = Variable.InternalVisit(visitor);
                action = Condition.InternalVisit(visitor);
    /// The ast for a for statement.
    public class ForStatementAst : LoopStatementAst
        /// Construct a for statement.
        /// <param name="extent">The extent of the statement, from the label or for keyword to the closing curly.</param>
        /// <param name="initializer">The optionally null initialization expression executed before the loop.</param>
        /// <param name="condition">The optionally null condition expression tested on each iteration of the loop.</param>
        /// <param name="iterator">The optionally null iteration expression executed after each iteration of the loop.</param>
        /// <param name="body">The body executed on each iteration of the loop.</param>
        public ForStatementAst(IScriptExtent extent,
                               PipelineBaseAst initializer,
                               PipelineBaseAst condition,
                               PipelineBaseAst iterator,
            : base(extent, label, condition, body)
                this.Initializer = initializer;
                SetParent(initializer);
                this.Iterator = iterator;
                SetParent(iterator);
        /// The ast for the initialization expression of a for statement, or null if none was specified.
        public PipelineBaseAst Initializer { get; }
        /// The ast for the iteration expression of a for statement, or null if none was specified.
        public PipelineBaseAst Iterator { get; }
        /// Copy the ForStatementAst instance.
            var newInitializer = CopyElement(this.Initializer);
            var newCondition = CopyElement(this.Condition);
            var newIterator = CopyElement(this.Iterator);
            return new ForStatementAst(this.Extent, this.Label, newInitializer, newCondition, newIterator, newBody);
            return visitor.VisitForStatement(this);
            var action = visitor.VisitForStatement(this);
            if (action == AstVisitAction.Continue && Initializer != null)
                action = Initializer.InternalVisit(visitor);
            if (action == AstVisitAction.Continue && Condition != null)
            if (action == AstVisitAction.Continue && Iterator != null)
                action = Iterator.InternalVisit(visitor);
    /// The ast that represents the do/while statement.
    public class DoWhileStatementAst : LoopStatementAst
        /// Construct a do/while statement.
        /// <param name="extent">The extent of the do/while statement from the label or do keyword to the closing curly brace.</param>
        /// <param name="condition">The condition tested on each iteration of the loop.</param>
        /// If <paramref name="extent"/> or <paramref name="condition"/> is null.
        public DoWhileStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
                throw PSTraceSource.NewArgumentNullException(nameof(condition));
        /// Copy the DoWhileStatementAst instance.
            return new DoWhileStatementAst(this.Extent, this.Label, newCondition, newBody);
            return visitor.VisitDoWhileStatement(this);
            var action = visitor.VisitDoWhileStatement(this);
    /// The ast that represents a do/until statement.
    public class DoUntilStatementAst : LoopStatementAst
        /// Construct a do/until statement.
        /// <param name="extent">The extent of the statement, from the label or do keyword to the closing curly brace.</param>
        public DoUntilStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
        /// Copy the DoUntilStatementAst instance.
            return new DoUntilStatementAst(this.Extent, this.Label, newCondition, newBody);
            return visitor.VisitDoUntilStatement(this);
            var action = visitor.VisitDoUntilStatement(this);
    /// The ast for a while statement.
    public class WhileStatementAst : LoopStatementAst
        /// Construct a while statement.
        /// <param name="extent">The extent of the statement, from the label or while keyword to the closing curly brace.</param>
        public WhileStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body)
        /// Copy the WhileStatementAst instance.
            return new WhileStatementAst(this.Extent, this.Label, newCondition, newBody);
            return visitor.VisitWhileStatement(this);
            var action = visitor.VisitWhileStatement(this);
    /// Flags that are specified on a switch statement.  Values may be or'ed together, not all invalid combinations
    public enum SwitchFlags
        /// The -file flag.
        File = 0x01,
        /// The -regex flag.
        Regex = 0x02,
        /// The -wildcard flag.
        Wildcard = 0x04,
        /// The -exact flag.
        Exact = 0x08,
        /// The -casesensitive flag.
        CaseSensitive = 0x10,
        Parallel = 0x20,
        // If any flags are added that influence evaluation of switch elements,
        // then the caching strategy in SwitchClauseEvalBinder needs to be updated,
        // and possibly its _binderCache.
    /// The ast that represents a switch statement.
    public class SwitchStatementAst : LabeledStatementAst
        private static readonly SwitchClause[] s_emptyClauseArray = Array.Empty<SwitchClause>();
        /// Construct a switch statement.
        /// <param name="extent">The extent of the statement, from the label or switch keyword to the closing curly.</param>
        /// <param name="condition">The expression being switched upon.</param>
        /// <param name="flags">Any flags that affect how the <paramref name="condition"/> is tested.</param>
        /// A possibly null or empty collection of conditions and block of statements to execute if the condition matches.
        /// <param name="default">The default clause to execute if no clauses match.</param>
        /// If <paramref name="default"/> and <paramref name="clauses"/> are both null or empty.
        public SwitchStatementAst(IScriptExtent extent,
                                  SwitchFlags flags,
                                  IEnumerable<SwitchClause> clauses,
                                  StatementBlockAst @default)
            if ((clauses == null || !clauses.Any()) && @default == null)
                // Must specify either clauses or default.  If neither, just complain about clauses as that's the most likely
                // invalid argument.
            this.Clauses = new ReadOnlyCollection<SwitchClause>(
                (clauses != null && clauses.Any()) ? clauses.ToArray() : s_emptyClauseArray);
                this.Default = @default;
                SetParent(@default);
        /// The flags, if any specified on the switch statement.
        public SwitchFlags Flags { get; }
        /// A possibly empty collection of conditions and statement blocks representing the cases of the switch statement.
        /// If the collection is empty, the default clause is not null.
        public ReadOnlyCollection<SwitchClause> Clauses { get; }
        /// The ast for the default of the switch statement, or null if no default block was specified.
        public StatementBlockAst Default { get; }
        /// Copy the SwitchStatementAst instance.
            var newDefault = CopyElement(this.Default);
            List<SwitchClause> newClauses = null;
            if (this.Clauses.Count > 0)
                newClauses = new List<SwitchClause>(this.Clauses.Count);
                    var newSwitchItem1 = CopyElement(clause.Item1);
                    var newSwitchItem2 = CopyElement(clause.Item2);
                    newClauses.Add(Tuple.Create(newSwitchItem1, newSwitchItem2));
            return new SwitchStatementAst(this.Extent, this.Label, newCondition, this.Flags, newClauses, newDefault);
            return visitor.VisitSwitchStatement(this);
            var action = visitor.VisitSwitchStatement(this);
                    var switchClauseAst = Clauses[index];
                    action = switchClauseAst.Item1.InternalVisit(visitor);
                    action = switchClauseAst.Item2.InternalVisit(visitor);
            if (action == AstVisitAction.Continue && Default != null)
                action = Default.InternalVisit(visitor);
    #endregion Looping Statements
    #region Exception Handling Statements
    /// The ast that represents a single catch as part of a try statement.
    public class CatchClauseAst : Ast
        private static readonly ReadOnlyCollection<TypeConstraintAst> s_emptyCatchTypes =
        /// Construct a catch clause.
        /// <param name="extent">The extent of the catch, from the catch keyword to the closing curly brace.</param>
        /// <param name="catchTypes">The collection of types caught by this catch clause, may be null if all types are caught.</param>
        /// <param name="body">The body of the catch clause.</param>
        public CatchClauseAst(IScriptExtent extent, IEnumerable<TypeConstraintAst> catchTypes, StatementBlockAst body)
            if (catchTypes != null)
                this.CatchTypes = new ReadOnlyCollection<TypeConstraintAst>(catchTypes.ToArray());
                SetParents(CatchTypes);
                this.CatchTypes = s_emptyCatchTypes;
        /// A possibly empty collection of types caught by this catch block.  If the collection is empty, the catch handler
        /// catches all exceptions.
        public ReadOnlyCollection<TypeConstraintAst> CatchTypes { get; }
        /// Returns true if this handler handles any kind of exception.
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CatchAll")]
        public bool IsCatchAll { get { return CatchTypes.Count == 0; } }
        /// The body of the catch block.  This property is never null.
        /// Copy the CatchClauseAst instance.
            var newCatchTypes = CopyElements(this.CatchTypes);
            return new CatchClauseAst(this.Extent, newCatchTypes, newBody);
            return visitor.VisitCatchClause(this);
            var action = visitor.VisitCatchClause(this);
            for (int index = 0; index < CatchTypes.Count; index++)
                var catchType = CatchTypes[index];
                action = catchType.InternalVisit(visitor);
    /// The ast that represents a try statement.
    public class TryStatementAst : StatementAst
        private static readonly ReadOnlyCollection<CatchClauseAst> s_emptyCatchClauses =
            Utils.EmptyReadOnlyCollection<CatchClauseAst>();
        /// Construct a try statement ast.
        /// The extent of the try statement, from the try keyword to the closing curly of the last catch or finally.
        /// <param name="body">The region of guarded code.</param>
        /// <param name="catchClauses">The list of catch clauses, may be null.</param>
        /// <param name="finally">The finally clause, may be null.</param>
        /// If <paramref name="catchClauses"/> is null or is an empty collection and <paramref name="finally"/> is also
        /// null, then an exception is also raised as the try block must have a finally or at least one catch.
        public TryStatementAst(IScriptExtent extent,
                               StatementBlockAst body,
                               IEnumerable<CatchClauseAst> catchClauses,
                               StatementBlockAst @finally)
            if ((catchClauses == null || !catchClauses.Any()) && @finally == null)
                // If no catches and no finally, just complain about catchClauses as that's the most likely invalid argument.
                throw PSTraceSource.NewArgumentException(nameof(catchClauses));
            if (catchClauses != null)
                this.CatchClauses = new ReadOnlyCollection<CatchClauseAst>(catchClauses.ToArray());
                SetParents(CatchClauses);
                this.CatchClauses = s_emptyCatchClauses;
            if (@finally != null)
                this.Finally = @finally;
                SetParent(@finally);
        /// The body of the try statement.  This property is never null.
        /// A collection of catch clauses, which is empty if there are no catches.
        public ReadOnlyCollection<CatchClauseAst> CatchClauses { get; }
        /// The ast for the finally block, or null if no finally block was specified, in which case <see cref="CatchClauses"/>
        /// is a non-null, non-empty collection.
        public StatementBlockAst Finally { get; }
        /// Copy the TryStatementAst instance.
            var newCatchClauses = CopyElements(this.CatchClauses);
            var newFinally = CopyElement(this.Finally);
            return new TryStatementAst(this.Extent, newBody, newCatchClauses, newFinally);
            return visitor.VisitTryStatement(this);
            var action = visitor.VisitTryStatement(this);
                for (int index = 0; index < CatchClauses.Count; index++)
                    var catchClause = CatchClauses[index];
                    action = catchClause.InternalVisit(visitor);
            if (action == AstVisitAction.Continue && Finally != null)
                action = Finally.InternalVisit(visitor);
    /// The ast that represents the trap statement.
    public class TrapStatementAst : StatementAst
        /// Construct a trap statement.
        /// The extent of the trap statement, starting with the trap keyword and ending with the closing curly of the body.
        /// <param name="trapType">The type handled by the trap statement, may be null if all exceptions are trapped.</param>
        /// <param name="body">The handler for the error.</param>
        public TrapStatementAst(IScriptExtent extent, TypeConstraintAst trapType, StatementBlockAst body)
            if (trapType != null)
                this.TrapType = trapType;
                SetParent(trapType);
        /// The ast for the type trapped by this trap block, or null if no type was specified.
        public TypeConstraintAst TrapType { get; }
        /// The body for the trap block.  This property is never null.
        /// Copy the TrapStatementAst instance.
            var newTrapType = CopyElement(this.TrapType);
            return new TrapStatementAst(this.Extent, newTrapType, newBody);
            return visitor.VisitTrap(this);
            var action = visitor.VisitTrap(this);
            if (action == AstVisitAction.Continue && TrapType != null)
                action = TrapType.InternalVisit(visitor);
    #endregion Exception Handling Statements
    #region Flow Control Statements
    /// The ast representing the break statement.
    public class BreakStatementAst : StatementAst
        /// Construct a break statement ast.
        /// <param name="extent">The extent of the statement, including the break keyword and the optional label.</param>
        /// <param name="label">The optional label expression.</param>
        public BreakStatementAst(IScriptExtent extent, ExpressionAst label)
                SetParent(label);
        /// The expression or label to break to, or null if no label was specified.
        public ExpressionAst Label { get; }
        /// Copy the BreakStatementAst instance.
            var newLabel = CopyElement(this.Label);
            return new BreakStatementAst(this.Extent, newLabel);
            return visitor.VisitBreakStatement(this);
            var action = visitor.VisitBreakStatement(this);
            if (action == AstVisitAction.Continue && Label != null)
                action = Label.InternalVisit(visitor);
    /// The ast representing the continue statement.
    public class ContinueStatementAst : StatementAst
        /// Construct a continue statement.
        /// <param name="extent">The extent of the statement including the optional label.</param>
        public ContinueStatementAst(IScriptExtent extent, ExpressionAst label)
        /// The expression or label to continue to, or null if no label was specified.
        /// Copy the ContinueStatementAst instance.
            return new ContinueStatementAst(this.Extent, newLabel);
            return visitor.VisitContinueStatement(this);
            var action = visitor.VisitContinueStatement(this);
    /// The ast representing the return statement.
    public class ReturnStatementAst : StatementAst
        /// Construct a return statement.
        /// <param name="extent">The extent of the statement including the optional return value.</param>
        /// <param name="pipeline">The optional return value.</param>
        public ReturnStatementAst(IScriptExtent extent, PipelineBaseAst pipeline)
            if (pipeline != null)
                this.Pipeline = pipeline;
                SetParent(pipeline);
        /// The pipeline specified in the return statement, or null if none was specified.
        public PipelineBaseAst Pipeline { get; }
        /// Copy the ReturnStatementAst instance.
            var newPipeline = CopyElement(this.Pipeline);
            return new ReturnStatementAst(this.Extent, newPipeline);
            return visitor.VisitReturnStatement(this);
            var action = visitor.VisitReturnStatement(this);
            if (action == AstVisitAction.Continue && Pipeline != null)
                action = Pipeline.InternalVisit(visitor);
    /// The ast representing the exit statement.
    public class ExitStatementAst : StatementAst
        /// Construct an exit statement.
        /// <param name="extent">The extent of the exit statement including the optional exit value.</param>
        /// <param name="pipeline">The optional exit value.</param>
        public ExitStatementAst(IScriptExtent extent, PipelineBaseAst pipeline)
        /// The pipeline specified in the exit statement, or null if none was specified.
        /// Copy the ExitStatementAst instance.
            return new ExitStatementAst(this.Extent, newPipeline);
            return visitor.VisitExitStatement(this);
            var action = visitor.VisitExitStatement(this);
    /// The ast representing the throw statement.
    public class ThrowStatementAst : StatementAst
        /// Construct a throw statement.
        /// <param name="extent">The extent of the throw statement, including the optional value to throw.</param>
        /// <param name="pipeline">The optional value to throw.</param>
        public ThrowStatementAst(IScriptExtent extent, PipelineBaseAst pipeline)
        /// The pipeline specified in the throw statement, or null if none was specified.
        /// If the throw statement is a rethrow.  In PowerShell, a throw statement need not throw anything.  Such
        /// a throw statement throws a new exception if it does not appear lexically withing a catch, otherwise
        /// it rethrows the caught exception.  Examples:
        ///     if ($true) { throw } # not a rethrow
        ///     try { foo } catch { throw } # rethrow
        ///     try { foo } catch { . { throw } } # rethrow
        ///     try { foo } catch { function foo { throw } } # rethrow
        ///     try { foo } finally { throw } # not a rethrow
        public bool IsRethrow
                if (Pipeline != null)
                var parent = Parent;
                    if (parent is CatchClauseAst)
        /// Copy the ThrowStatementAst instance.
            return new ThrowStatementAst(this.Extent, newPipeline);
            return visitor.VisitThrowStatement(this);
            var action = visitor.VisitThrowStatement(this);
    /// An AST representing a syntax element chainable with '&amp;&amp;' or '||'.
    public abstract class ChainableAst : PipelineBaseAst
        /// Initializes a new instance of the new chainable AST with the given extent.
        /// <param name="extent">The script extent of the AST.</param>
        protected ChainableAst(IScriptExtent extent) : base(extent)
    /// A command-oriented flow-controlled pipeline chain.
    /// E.g. <c>npm build &amp;&amp; npm test</c> or <c>Get-Content -Raw ./file.txt || "default"</c>.
    public class PipelineChainAst : ChainableAst
        /// Initializes a new instance of the new statement chain AST from two statements and an operator.
        /// <param name="extent">The extent of the chained statement.</param>
        /// <param name="lhsChain">The pipeline or pipeline chain to the left of the operator.</param>
        /// <param name="rhsPipeline">The pipeline to the right of the operator.</param>
        /// <param name="chainOperator">The operator used.</param>
        /// <param name="background">True when this chain has been invoked with the background operator, false otherwise.</param>
        public PipelineChainAst(
            ChainableAst lhsChain,
            PipelineAst rhsPipeline,
            TokenKind chainOperator,
            bool background = false)
            ArgumentNullException.ThrowIfNull(lhsChain);
            ArgumentNullException.ThrowIfNull(rhsPipeline);
            if (chainOperator != TokenKind.AndAnd && chainOperator != TokenKind.OrOr)
                throw new ArgumentException(nameof(chainOperator));
            LhsPipelineChain = lhsChain;
            RhsPipeline = rhsPipeline;
            Operator = chainOperator;
            Background = background;
            SetParent(LhsPipelineChain);
            SetParent(RhsPipeline);
        /// Gets the left hand pipeline in the chain.
        public ChainableAst LhsPipelineChain { get; }
        /// Gets the right hand pipeline in the chain.
        public PipelineAst RhsPipeline { get; }
        /// Gets the chaining operator used.
        public TokenKind Operator { get; }
        /// Gets a flag that indicates whether this chain has been invoked with the background operator.
        public bool Background { get; }
        /// Create a copy of this Ast.
        /// A fresh copy of this PipelineChainAst instance.
            return new PipelineChainAst(Extent, CopyElement(LhsPipelineChain), CopyElement(RhsPipeline), Operator, Background);
            return (visitor as ICustomAstVisitor2)?.VisitPipelineChain(this);
            AstVisitAction action = AstVisitAction.Continue;
            // Can only visit new AST type if using AstVisitor2
                action = visitor2.VisitPipelineChain(this);
                action = LhsPipelineChain.InternalVisit(visitor);
                action = RhsPipeline.InternalVisit(visitor);
    #endregion Flow Control Statements
    /// An abstract base class for statements that include command invocations, pipelines, expressions, and assignments.
    /// Any statement that does not begin with a keyword is derives from PipelineBastAst.
    public abstract class PipelineBaseAst : StatementAst
        /// Initialize the common parts of a PipelineBaseAst.
        protected PipelineBaseAst(IScriptExtent extent)
        /// If the pipeline represents a pure expression, the expression is returned, otherwise null is returned.
        public virtual ExpressionAst GetPureExpression()
    /// The ast that represents a PowerShell pipeline, e.g. <c>gci -re . *.cs | select-string Foo</c> or <c> 65..90 | % { [char]$_ }</c>.
    /// A pipeline must have at least 1 command.  The first command may be an expression or a command invocation.
    public class PipelineAst : ChainableAst
        /// Construct a pipeline from a collection of commands.
        /// <param name="extent">The extent of the pipeline.</param>
        /// <param name="pipelineElements">The collection of commands representing the pipeline.</param>
        /// <param name="background">Indicates that this pipeline should be run in the background.</param>
        /// If <paramref name="pipelineElements"/> is null or is an empty collection.
        public PipelineAst(IScriptExtent extent, IEnumerable<CommandBaseAst> pipelineElements, bool background)
            if (pipelineElements == null || !pipelineElements.Any())
                throw PSTraceSource.NewArgumentException(nameof(pipelineElements));
            this.Background = background;
            this.PipelineElements = new ReadOnlyCollection<CommandBaseAst>(pipelineElements.ToArray());
            SetParents(PipelineElements);
        public PipelineAst(IScriptExtent extent, IEnumerable<CommandBaseAst> pipelineElements) : this(extent, pipelineElements, background: false)
        /// Construct a pipeline from a single command.
        /// <param name="extent">The extent of the pipeline (which should be the extent of the command).</param>
        /// <param name="commandAst">The command for the pipeline.</param>
        /// If <paramref name="extent"/> or <paramref name="commandAst"/> is null.
        public PipelineAst(IScriptExtent extent, CommandBaseAst commandAst, bool background)
                throw PSTraceSource.NewArgumentNullException(nameof(commandAst));
            this.PipelineElements = new ReadOnlyCollection<CommandBaseAst>(new CommandBaseAst[] { commandAst });
            SetParent(commandAst);
        public PipelineAst(IScriptExtent extent, CommandBaseAst commandAst) : this(extent, commandAst, background: false)
        /// A non-null, non-empty collection of commands that represent the pipeline.
        public ReadOnlyCollection<CommandBaseAst> PipelineElements { get; }
        /// Indicates that this pipeline should be run in the background.
        public bool Background { get; internal set; }
        public override ExpressionAst GetPureExpression()
            if (PipelineElements.Count != 1)
            CommandExpressionAst expr = PipelineElements[0] as CommandExpressionAst;
            if (expr != null && expr.Redirections.Count == 0)
        /// Copy the PipelineAst instance.
        /// <returns>A fresh copy of this PipelineAst instance.</returns>
            var newPipelineElements = CopyElements(this.PipelineElements);
            return new PipelineAst(this.Extent, newPipelineElements, this.Background);
            return visitor.VisitPipeline(this);
            var action = visitor.VisitPipeline(this);
                for (int index = 0; index < PipelineElements.Count; index++)
                    var commandAst = PipelineElements[index];
                    action = commandAst.InternalVisit(visitor);
    /// An abstract base class for the components of a <see cref="CommandAst"/>.
    public abstract class CommandElementAst : Ast
        /// Initialize the common fields of a comment element.
        /// <param name="extent">The extent of the command element.</param>
        protected CommandElementAst(IScriptExtent extent)
    /// The ast that represents a parameter to a command, e.g. <c>dir -Path a*</c>, this class represent '-Path', and
    /// in <c>dir -Path:a*</c>, this class represents '-Path:a*'.
    /// In the first case, the argument 'a*' is not represented by this class because the parser can't know until runtime
    /// if the argument is positional or if -Path accepts an argument.  In the later case, the argument 'a*' always
    /// belongs to the parameter -Path.
    public class CommandParameterAst : CommandElementAst
        /// Construct a command parameter.
        /// The extent of the parameter, starting from the dash character, ending at the end of the parameter name, or else
        /// at the end of the optional argument.
        /// The parameter name, without the leading dash and without the trailing colon, if a colon was used.
        /// If the parameter includes an argument with the syntax like <c>-Path:a*</c>, then the expression for 'a*' is
        /// passed as the argument.  An argument is not required.
        /// <param name="errorPosition">
        /// The extent to use for error reporting when parameter binding fails with this parameter.  If <paramref name="argument"/>
        /// is null, this extent is the same as <paramref name="extent"/>, otherwise it is the extent of the parameter token
        /// itself.
        /// If <paramref name="extent"/> or <paramref name="parameterName"/>is null, or if <paramref name="parameterName"/>
        /// is an empty string.
        public CommandParameterAst(IScriptExtent extent, string parameterName, ExpressionAst argument, IScriptExtent errorPosition)
            if (errorPosition == null || string.IsNullOrEmpty(parameterName))
                throw PSTraceSource.NewArgumentNullException(errorPosition == null ? "errorPosition" : "parameterName");
            this.ParameterName = parameterName;
            if (argument != null)
            this.ErrorPosition = errorPosition;
        /// The name of the parameter.  This value does not include a leading dash, and in the case that an argument
        /// is specified, no trailing colon is included either.  This property is never null or empty.
        public string ParameterName { get; }
        /// The ast for the argument if specified (e.g. -Path:-abc, then the argument is the ast for '-ast'), otherwise null
        /// if no argument was specified.
        /// The error position to use when parameter binding fails.  This extent does not include the argument if one was
        /// specified, which means this extent is often the same as <see cref="Ast.Extent"/>.
        public IScriptExtent ErrorPosition { get; }
        /// Copy the CommandParameterAst instance.
            return new CommandParameterAst(this.Extent, this.ParameterName, newArgument, this.ErrorPosition);
            return visitor.VisitCommandParameter(this);
            var action = visitor.VisitCommandParameter(this);
            if (Argument != null && action == AstVisitAction.Continue)
    /// An abstract base class for a command and an expression wrapper that allows an expression as a command in a pipeline.
    public abstract class CommandBaseAst : StatementAst
        private static readonly ReadOnlyCollection<RedirectionAst> s_emptyRedirections =
            Utils.EmptyReadOnlyCollection<RedirectionAst>();
        internal const int MaxRedirections = (int)RedirectionStream.Information + 1;
        /// Initialize the common fields of a command.
        /// <param name="extent">The extent of the command.</param>
        /// <param name="redirections">The redirections for the command, may be null.</param>
        protected CommandBaseAst(IScriptExtent extent, IEnumerable<RedirectionAst> redirections)
            if (redirections != null)
                this.Redirections = new ReadOnlyCollection<RedirectionAst>(redirections.ToArray());
                SetParents(Redirections);
                this.Redirections = s_emptyRedirections;
        /// The possibly empty collection of redirections for this command.
        public ReadOnlyCollection<RedirectionAst> Redirections { get; }
    /// The ast for a command invocation, e.g. <c>dir *.ps1</c>.
    public class CommandAst : CommandBaseAst
        /// Construct a command invocation.
        /// The extent of the command, starting with either the optional invocation operator '&amp;' or '.' or the command name
        /// and ending with the last command element.
        /// <param name="commandElements">The elements of the command (command name, parameters and expressions.).</param>
        /// <param name="invocationOperator">The invocation operator that was used, if any.</param>
        /// If <paramref name="commandElements"/> is null or is an empty collection.
        public CommandAst(IScriptExtent extent,
                          IEnumerable<CommandElementAst> commandElements,
                          TokenKind invocationOperator,
                          IEnumerable<RedirectionAst> redirections)
            : base(extent, redirections)
            if (commandElements == null || !commandElements.Any())
                throw PSTraceSource.NewArgumentException(nameof(commandElements));
            if (invocationOperator != TokenKind.Dot && invocationOperator != TokenKind.Ampersand && invocationOperator != TokenKind.Unknown)
                throw PSTraceSource.NewArgumentException(nameof(invocationOperator));
            this.CommandElements = new ReadOnlyCollection<CommandElementAst>(commandElements.ToArray());
            SetParents(CommandElements);
            this.InvocationOperator = invocationOperator;
        /// A non-empty collection of command elements.  This property is never null.
        public ReadOnlyCollection<CommandElementAst> CommandElements { get; }
        /// The invocation operator (either <see cref="TokenKind.Dot"/> or <see cref="TokenKind.Ampersand"/>) if one was specified,
        /// otherwise the value is <see cref="TokenKind.Unknown"/>.
        public TokenKind InvocationOperator { get; }
        /// <para>Returns the name of the command invoked by this ast.</para>
        /// <para>This command name may not be known statically, in which case null is returned.</para>
        /// For example, if the command name is in a variable: <code>&amp; $foo</code>, then the parser cannot know which command is executed.
        /// Similarly, if the command is being invoked in a module: <code>&amp; (gmo SomeModule) Bar</code>, then the parser does not know the
        /// command name is Bar because the parser can't determine that the expression <code>(gmo SomeModule)</code> returns a module instead
        /// of a string.
        /// <returns>The command name, if known, null otherwise.</returns>
        public string GetCommandName()
            var name = CommandElements[0] as StringConstantExpressionAst;
            return name?.Value;
        /// If this command was synthesized out of a dynamic keyword, this property will point to the DynamicKeyword
        /// data structure that was used to create this command.
        public DynamicKeyword DefiningKeyword { get; set; }
        /// Copy the CommandAst instance.
            var newCommandElements = CopyElements(this.CommandElements);
            var newRedirections = CopyElements(this.Redirections);
            return new CommandAst(this.Extent, newCommandElements, this.InvocationOperator, newRedirections)
                DefiningKeyword = this.DefiningKeyword
            return visitor.VisitCommand(this);
            var action = visitor.VisitCommand(this);
                for (int index = 0; index < CommandElements.Count; index++)
                    var commandElementAst = CommandElements[index];
                    action = commandElementAst.InternalVisit(visitor);
                for (int index = 0; index < Redirections.Count; index++)
                    var redirection = Redirections[index];
                        action = redirection.InternalVisit(visitor);
    /// The ast representing an expression when the expression is used as the first command of a pipeline.
    public class CommandExpressionAst : CommandBaseAst
        /// Construct a command that wraps an expression.
        /// <param name="expression">The expression being wrapped.</param>
        /// If <paramref name="extent"/> or <paramref name="expression"/> is null.
        public CommandExpressionAst(IScriptExtent extent,
            this.Expression = expression;
            SetParent(expression);
        /// The ast for the expression that is or starts a pipeline.  This property is never null.
        public ExpressionAst Expression { get; }
        /// Copy the CommandExpressionAst instance.
            var newExpression = CopyElement(this.Expression);
            return new CommandExpressionAst(this.Extent, newExpression, newRedirections);
            return visitor.VisitCommandExpression(this);
            var action = visitor.VisitCommandExpression(this);
                action = Expression.InternalVisit(visitor);
    /// An abstract base class representing both file redirections and merging redirections.
    public abstract class RedirectionAst : Ast
        /// Initialize the common fields in a redirection.
        /// <param name="extent">The extent of the redirection.</param>
        /// <param name="from">The stream to read from.</param>
        protected RedirectionAst(IScriptExtent extent, RedirectionStream from)
            this.FromStream = from;
        /// The stream to read objects from.  Objects are either merged with another stream, or written to a file.
        public RedirectionStream FromStream { get; }
    /// The stream number that is redirected.
    public enum RedirectionStream
        /// All streams, used when the redirection token uses '*' as the stream number.
        All = 0,
        /// The normal output stream.
        /// The error stream.
        /// The warning stream.
        /// The verbose stream.
        /// The debug stream.
        /// The information stream.
        Information = 6
    /// The ast representing a redirection that merges 2 streams, e.g. <c>dir 2>&amp;1</c>
    public class MergingRedirectionAst : RedirectionAst
        /// Construct a merging redirection.
        /// <param name="to">The stream to write to - must always be <see cref="RedirectionStream.Output"/></param>
        public MergingRedirectionAst(IScriptExtent extent, RedirectionStream from, RedirectionStream to)
            : base(extent, from)
            this.ToStream = to;
        /// The stream that results will be written to.
        public RedirectionStream ToStream { get; }
        /// Copy the MergingRedirectionAst instance.
            return new MergingRedirectionAst(this.Extent, this.FromStream, this.ToStream);
            return visitor.VisitMergingRedirection(this);
            var action = visitor.VisitMergingRedirection(this);
    /// The ast representing a redirection to a file, e.g. <c>dir > out.txt</c>, the '> out.txt' is represented by this ast.
    public class FileRedirectionAst : RedirectionAst
        /// Construct a redirection to a file.
        /// The extent of the redirection, starting with the redirection operator and including the file.
        /// <param name="stream">
        /// The stream being redirected.
        /// <param name="file">
        /// The optional location to redirect to.  Merging operators may not specify a file, the other redirection
        /// operators must specify a location.
        /// <param name="append">
        /// True if the file is being appended, false otherwise.
        /// <exception cref="PSArgumentException">If <paramref name="file"/> is null.</exception>
        public FileRedirectionAst(IScriptExtent extent, RedirectionStream stream, ExpressionAst file, bool append)
            : base(extent, stream)
            if (file == null)
                throw PSTraceSource.NewArgumentNullException(nameof(file));
            this.Location = file;
            SetParent(file);
            this.Append = append;
        /// The ast for the location to redirect to.
        public ExpressionAst Location { get; }
        /// True if the file is appended, false otherwise.
        public bool Append { get; }
        /// Copy the FileRedirectionAst instance.
            var newFile = CopyElement(this.Location);
            return new FileRedirectionAst(this.Extent, this.FromStream, newFile, this.Append);
            return visitor.VisitFileRedirection(this);
            var action = visitor.VisitFileRedirection(this);
                action = Location.InternalVisit(visitor);
    /// The ast that represents an assignment statement, e.g. <c>$x = 42</c>.
    public class AssignmentStatementAst : PipelineBaseAst
        /// Construct an assignment statement.
        /// <param name="extent">The extent of the assignment statement.</param>
        /// <param name="left">The value being assigned.</param>
        /// <param name="operator">The assignment operator, e.g. '=' or '+='.</param>
        /// <param name="right">The value to assign.</param>
        /// <param name="errorPosition">The position to report an error if an error occurs at runtime.</param>
        /// If <paramref name="extent"/>, <paramref name="left"/>, <paramref name="right"/>,
        /// or <paramref name="errorPosition"/> is null.
        public AssignmentStatementAst(IScriptExtent extent, ExpressionAst left, TokenKind @operator, StatementAst right, IScriptExtent errorPosition)
            if (left == null || right == null || errorPosition == null)
                throw PSTraceSource.NewArgumentNullException(left == null ? "left" : right == null ? "right" : "errorPosition");
            if ((@operator.GetTraits() & TokenFlags.AssignmentOperator) == 0)
                throw PSTraceSource.NewArgumentException(nameof(@operator));
            // If the assignment is just an expression and the expression is not backgrounded then
            // remove the pipeline wrapping the expression.
            if (right is PipelineAst pipelineAst
                && !pipelineAst.Background
                && pipelineAst.PipelineElements.Count == 1
                && pipelineAst.PipelineElements[0] is CommandExpressionAst commandExpressionAst)
                right = commandExpressionAst;
                right.ClearParent();
            this.Operator = @operator;
            this.Left = left;
            SetParent(left);
            this.Right = right;
            SetParent(right);
        /// The ast for the location being assigned.  This property is never null.
        public ExpressionAst Left { get; }
        /// The operator for token assignment (such as =, +=, -=, etc.).  The value is always some assignment operator.
        /// The ast for the value to assign.  This property is never null.
        public StatementAst Right { get; }
        /// The position to report at runtime if there is an error during assignment.  This property is never null.
        /// Copy the AssignmentStatementAst instance.
            var newLeft = CopyElement(this.Left);
            var newRight = CopyElement(this.Right);
            return new AssignmentStatementAst(this.Extent, newLeft, this.Operator, newRight, this.ErrorPosition);
        /// Return all of the expressions assigned by the assignment statement.  Typically
        /// it's just a variable expression, but if <see cref="Left"/> is an <see cref="ArrayLiteralAst"/>,
        /// then all of the elements are assigned.
        /// <returns>All of the expressions assigned by the assignment statement.</returns>
        public IEnumerable<ExpressionAst> GetAssignmentTargets()
            if (Left is ArrayLiteralAst arrayExpression)
                foreach (var element in arrayExpression.Elements)
                    yield return element;
            yield return Left;
            return visitor.VisitAssignmentStatement(this);
            var action = visitor.VisitAssignmentStatement(this);
                action = Left.InternalVisit(visitor);
                action = Right.InternalVisit(visitor);
    /// Defines types of configuration document.
    public enum ConfigurationType
        /// Resource configuration.
        Resource = 0,
        /// Meta configuration.
        Meta = 1
    /// The ast represents the DSC configuration statement.
    public class ConfigurationDefinitionAst : StatementAst
        /// Construct a configuration statement.
        /// The extent of the expression, starting with the attribute and ending after the expression being attributed.
        /// <param name="body"><see cref="ScriptBlockExpressionAst"/> of the configuration statement.</param>
        /// <param name="type">The type of the configuration.</param>
        /// <param name="instanceName">The configuration name expression.</param>
        /// If <paramref name="extent"/>, <paramref name="body"/>, or <paramref name="instanceName"/> is null.
        public ConfigurationDefinitionAst(IScriptExtent extent,
            ScriptBlockExpressionAst body,
            ConfigurationType type,
            ExpressionAst instanceName) : base(extent)
                throw PSTraceSource.NewArgumentNullException(nameof(instanceName));
            this.ConfigurationType = type;
            this.InstanceName = instanceName;
            SetParent(instanceName);
        /// This ast represents configuration body script block.
        public ScriptBlockExpressionAst Body { get; }
        /// The configuration type.
        public ConfigurationType ConfigurationType { get; }
        /// The name of the configuration instance,
        /// For example, Instance name of 'configuration test { ...... }' is 'test'
        public ExpressionAst InstanceName { get; }
        /// Duplicates the <see cref="ConfigurationDefinitionAst"/>, allowing it to be composed into other ASTs.
        /// <returns>A copy of the <see cref="ConfigurationDefinitionAst"/>, with the link to the previous parent removed.</returns>
            ScriptBlockExpressionAst body = CopyElement(Body);
            ExpressionAst instanceName = CopyElement(InstanceName);
            return new ConfigurationDefinitionAst(Extent, body, ConfigurationType, instanceName)
                LCurlyToken = this.LCurlyToken,
                ConfigurationToken = this.ConfigurationToken,
                CustomAttributes = this.CustomAttributes?.Select(static e => (AttributeAst)e.Copy())
            return visitor2?.VisitConfigurationDefinition(this);
                action = visitor2.VisitConfigurationDefinition(this);
                action = InstanceName.InternalVisit(visitor);
                Body.InternalVisit(visitor);
        #region Internal methods/properties
        internal Token LCurlyToken { get; set; }
        internal Token ConfigurationToken { get; set; }
        internal IEnumerable<AttributeAst> CustomAttributes { get; set; }
        /// A dynamic keyword may also define additional keywords in the child scope
        /// of the scriptblock. This collection will contain those keywords.
        internal List<DynamicKeyword> DefinedKeywords { get; set; }
        /// Generate ast that defines a function for this <see cref="ConfigurationDefinitionAst"/> object.
        /// The <see cref="PipelineAst"/> that defines a function for this <see cref="ConfigurationDefinitionAst"/> object
        internal PipelineAst GenerateSetItemPipelineAst()
            // **************************
            // Now construct the AST to call the function that will build the actual object.
            // This is composed of a call to command with the signature
            //    function PSDesiredStateConfiguration\\Configuration
            //      param (
            //          ResourceModuleTuplesToImport,      # list of required resource-module tuples
            //          $OutputPath = ".",      # output path where the MOF will be placed
            //          $Name,                  # name of the configuration == name of the wrapper function
            //          [scriptblock]
            //              $Body,              # the body of the configuration
            //          [hashtable]
            //              $ArgsToBody,        # the argument values to pass to the body scriptblock
            //              $ConfigurationData, # a collection of property bags describe the configuration environment
            //              $InstanceName = ""  # THe name of the configuration instance being created.
            //          [boolean]
            //               $IsMetaConfig = $false # the configuration to generated is a meta configuration
            //      )
            var cea = new Collection<CommandElementAst>
                new StringConstantExpressionAst(this.Extent,
                                                @"PSDesiredStateConfiguration\Configuration",
                                                StringConstantType.BareWord),
                new CommandParameterAst(LCurlyToken.Extent, "ArgsToBody", new VariableExpressionAst(LCurlyToken.Extent, "toBody", false), LCurlyToken.Extent),
                new CommandParameterAst(LCurlyToken.Extent, "Name", (ExpressionAst)InstanceName.Copy(), LCurlyToken.Extent)
            ///////////////////////////
            // get import parameters
            var bodyStatements = Body.ScriptBlock.EndBlock.Statements;
            var resourceModulePairsToImport = new List<Tuple<string[], ModuleSpecification[], Version>>();
            var resourceBody = (from stm in bodyStatements where !IsImportCommand(stm, resourceModulePairsToImport) select (StatementAst)stm.Copy()).ToList();
            cea.Add(new CommandParameterAst(PositionUtilities.EmptyExtent, "ResourceModuleTuplesToImport", new ConstantExpressionAst(PositionUtilities.EmptyExtent, resourceModulePairsToImport), PositionUtilities.EmptyExtent));
            var scriptBlockBody = new ScriptBlockAst(Body.Extent,
                CustomAttributes?.Select(static att => (AttributeAst)att.Copy()).ToList(),
                new StatementBlockAst(Body.Extent, resourceBody, null),
            var scriptBlockExp = new ScriptBlockExpressionAst(Body.Extent, scriptBlockBody);
            // then the configuration scriptblock as -Body
            cea.Add(new CommandParameterAst(LCurlyToken.Extent, "Body", scriptBlockExp, LCurlyToken.Extent));
            cea.Add(new CommandParameterAst(LCurlyToken.Extent, "Outputpath", new VariableExpressionAst(LCurlyToken.Extent, "OutputPath", false), LCurlyToken.Extent));
            cea.Add(new CommandParameterAst(LCurlyToken.Extent, "ConfigurationData", new VariableExpressionAst(LCurlyToken.Extent, "ConfigurationData", false), LCurlyToken.Extent));
            cea.Add(new CommandParameterAst(LCurlyToken.Extent, "InstanceName", new VariableExpressionAst(LCurlyToken.Extent, "InstanceName", false), LCurlyToken.Extent));
            // copy the configuration parameter to the new function parameter
            // the new set-item created function will have below parameters
            //    [cmdletbinding()]
            //    param(
            //            [string]
            //                $InstanceName,
            //            [string[]]
            //                $DependsOn,
            //                $OutputPath,
            //            [hashtable]
            //            [Microsoft.PowerShell.DesiredStateConfiguration.ArgumentToConfigurationDataTransformation()]
            //               $ConfigurationData
            var attribAsts =
                ConfigurationBuildInParameterAttribAsts.Select(static attribAst => (AttributeAst)attribAst.Copy()).ToList();
            var paramAsts = ConfigurationBuildInParameters.Select(static paramAst => (ParameterAst)paramAst.Copy()).ToList();
            // the parameters defined in the configuration keyword will be combined with above parameters
            // it will be used to construct $ArgsToBody in the set-item created function boby using below statement
            //         $toBody = @{}+$PSBoundParameters
            //         $toBody.Remove(""OutputPath"")
            //         $toBody.Remove(""ConfigurationData"")
            //         $ConfigurationData = $psboundparameters[""ConfigurationData""]
            //         $Outputpath = $psboundparameters[""Outputpath""]
            if (Body.ScriptBlock.ParamBlock != null)
                paramAsts.AddRange(Body.ScriptBlock.ParamBlock.Parameters.Select(static parameterAst => (ParameterAst)parameterAst.Copy()));
            var paramBlockAst = new ParamBlockAst(this.Extent, attribAsts, paramAsts);
            var cmdAst = new CommandAst(this.Extent, cea, TokenKind.Unknown, null);
            var pipeLineAst = new PipelineAst(this.Extent, cmdAst, background: false);
            var funcStatements = ConfigurationExtraParameterStatements.Select(static statement => (StatementAst)statement.Copy()).ToList();
            funcStatements.Add(pipeLineAst);
            var statmentBlockAst = new StatementBlockAst(this.Extent, funcStatements, null);
            var funcBody = new ScriptBlockAst(Body.Extent,
                paramBlockAst, statmentBlockAst, false, true);
            var funcBodyExp = new ScriptBlockExpressionAst(this.Extent, funcBody);
            #region "Construct Set-Item pipeline"
            // Now construct the AST to call the set-item cmdlet that will create the function
            // it will do set-item -Path function:\ConfigurationNameExpr -Value funcBody
            // create function:\confignameexpression
            var funcDriveStrExpr = new StringConstantExpressionAst(this.Extent, @"function:\",
                                                                   StringConstantType.BareWord);
            var funcPathStrExpr = new BinaryExpressionAst(this.Extent, funcDriveStrExpr,
                                                          TokenKind.Plus,
                                                          (ExpressionAst)InstanceName.Copy(),
                                                          this.Extent);
            var setItemCmdElements = new Collection<CommandElementAst>
                new StringConstantExpressionAst(this.Extent, @"set-item",
                new CommandParameterAst(this.Extent, "Path",
                                        funcPathStrExpr,
                                        this.Extent),
                new CommandParameterAst(this.Extent, "Value", funcBodyExp,
                                        this.Extent)
            // then the configuration scriptblock as function body
            var setItemCmdlet = new CommandAst(this.Extent, setItemCmdElements, TokenKind.Unknown, null);
            var returnPipelineAst = new PipelineAst(this.Extent, setItemCmdlet, background: false);
            SetParent(returnPipelineAst);
            return returnPipelineAst;
        #region static fields/methods
        /// <param name="stmt"></param>
        /// <param name="resourceModulePairsToImport">Item1 - ResourceName, Item2 - ModuleName, Item3 - ModuleVersion.</param>
        private static bool IsImportCommand(StatementAst stmt, List<Tuple<string[], ModuleSpecification[], Version>> resourceModulePairsToImport)
            var dkwsAst = stmt as DynamicKeywordStatementAst;
            if (dkwsAst == null || !dkwsAst.Keyword.Keyword.Equals("Import-DscResource", StringComparison.OrdinalIgnoreCase))
            var commandAst = new CommandAst(dkwsAst.Extent, CopyElements(dkwsAst.CommandElements), TokenKind.Unknown, null);
            ParameterBindingResult moduleNames = null;
            ParameterBindingResult resourceNames = null;
            ParameterBindingResult moduleVersion = null;
            // we are only interested in Name and ModuleName parameter
            foreach (var entry in bindingResult.BoundParameters)
                string paramName = entry.Key;
                var paramValue = entry.Value;
                if ((paramName.Length <= nameParam.Length) && (paramName.AsSpan().Equals(nameParam.AsSpan(0, paramName.Length), StringComparison.OrdinalIgnoreCase)))
                    resourceNames = paramValue;
                // Since both parameters -ModuleName and -ModuleVersion has same start string i.e. Module so we will try to resolve it to -ModuleName
                // if user specifies like -Module
                if ((paramName.Length <= moduleNameParam.Length) && (paramName.AsSpan().Equals(moduleNameParam.AsSpan(0, paramName.Length), StringComparison.OrdinalIgnoreCase)))
                    moduleNames = paramValue;
                else if ((paramName.Length <= moduleVersionParam.Length) && (paramName.AsSpan().Equals(moduleVersionParam.AsSpan(0, paramName.Length), StringComparison.OrdinalIgnoreCase)))
                    moduleVersion = paramValue;
            string[] resourceNamesTyped = new[] { "*" };
            ModuleSpecification[] moduleNamesTyped = null;
            Version moduleVersionTyped = null;
            if (resourceNames != null)
                object resourceNameEvaluated;
                IsConstantValueVisitor.IsConstant(resourceNames.Value, out resourceNameEvaluated, true, true);
                resourceNamesTyped = LanguagePrimitives.ConvertTo<string[]>(resourceNameEvaluated);
                object moduleNameEvaluated;
                IsConstantValueVisitor.IsConstant(moduleNames.Value, out moduleNameEvaluated, true, true);
                moduleNamesTyped = LanguagePrimitives.ConvertTo<ModuleSpecification[]>(moduleNameEvaluated);
                object moduleVersionEvaluated;
                IsConstantValueVisitor.IsConstant(moduleVersion.Value, out moduleVersionEvaluated, true, true);
                if (moduleVersionEvaluated is double)
                    moduleVersionEvaluated = moduleVersion.Value.Extent.Text;
                moduleVersionTyped = LanguagePrimitives.ConvertTo<Version>(moduleVersionEvaluated);
                // Use -ModuleVersion <Version> only in the case, if -ModuleName specified.
                // Override ModuleName versions.
                if (moduleNamesTyped != null && moduleNamesTyped.Length == 1)
                    for (int i = 0; i < moduleNamesTyped.Length; i++)
                        moduleNamesTyped[i] = new ModuleSpecification(new Hashtable()
                            {"ModuleName", moduleNamesTyped[i].Name},
                            {"ModuleVersion", moduleVersionTyped}
            resourceModulePairsToImport.Add(new Tuple<string[], ModuleSpecification[], Version>(resourceNamesTyped, moduleNamesTyped, moduleVersionTyped));
        private const string ConfigurationBuildInParametersStr = @"
                        [cmdletbinding()]
                                $InstanceName,
                                $DependsOn,
                            [PSCredential]
                                $PsDscRunAsCredential,
                                $OutputPath,
                            [Microsoft.PowerShell.DesiredStateConfiguration.ArgumentToConfigurationDataTransformation()]
                               $ConfigurationData
                        )";
        private static IEnumerable<ParameterAst> ConfigurationBuildInParameters
                if (s_configurationBuildInParameters == null)
                    s_configurationBuildInParameters = new List<ParameterAst>();
                    var sba = Parser.ParseInput(ConfigurationBuildInParametersStr, out tokens, out errors);
                    if (sba != null)
                        foreach (var parameterAst in sba.ParamBlock.Parameters)
                            s_configurationBuildInParameters.Add((ParameterAst)parameterAst.Copy());
                return s_configurationBuildInParameters;
        private static List<ParameterAst> s_configurationBuildInParameters;
        private static IEnumerable<AttributeAst> ConfigurationBuildInParameterAttribAsts
                if (s_configurationBuildInParameterAttrAsts == null)
                    s_configurationBuildInParameterAttrAsts = new List<AttributeAst>();
                        foreach (var attribAst in sba.ParamBlock.Attributes)
                            s_configurationBuildInParameterAttrAsts.Add((AttributeAst)attribAst.Copy());
                return s_configurationBuildInParameterAttrAsts;
        private static List<AttributeAst> s_configurationBuildInParameterAttrAsts;
        private static IEnumerable<StatementAst> ConfigurationExtraParameterStatements
                if (s_configurationExtraParameterStatements == null)
                    s_configurationExtraParameterStatements = new List<StatementAst>();
                    var sba = Parser.ParseInput(@"
                        Import-Module Microsoft.PowerShell.Management -Verbose:$false
                        Import-Module PSDesiredStateConfiguration -Verbose:$false
                        $toBody = @{}+$PSBoundParameters
                        $toBody.Remove(""OutputPath"")
                        $toBody.Remove(""ConfigurationData"")
                        $ConfigurationData = $psboundparameters[""ConfigurationData""]
                        $Outputpath = $psboundparameters[""Outputpath""]", out tokens, out errors);
                        foreach (var statementAst in sba.EndBlock.Statements)
                            s_configurationExtraParameterStatements.Add((StatementAst)statementAst.Copy());
                return s_configurationExtraParameterStatements;
        private static List<StatementAst> s_configurationExtraParameterStatements;
    /// The ast represents the DynamicKeyword statement.
    public class DynamicKeywordStatementAst : StatementAst
        /// Construct a DynamicKeyword statement.
        /// <param name="commandElements">A collection of <see cref="CommandElementAst"/> used to invoke <see cref="DynamicKeyword"/> specific command.</param>
        /// If <paramref name="commandElements"/> is null or empty.
        public DynamicKeywordStatementAst(IScriptExtent extent,
            IEnumerable<CommandElementAst> commandElements) : base(extent)
        /// A non-empty collection of command elements represent the content of the
        /// DynamicKeyword.
        /// It may represents a command, such as “Import-DSCResource”,
        /// or DSC resources, then CommandElements includes:
        ///   (1) Keyword Name
        ///   (2) InstanceName
        ///   (3) Body, could be ScriptBlockExpressionAst (for Node keyword) or a HashtableAst (remaining)
        /// This property is never null and never empty.
        /// Duplicates the <see cref="DynamicKeywordStatementAst"/>, allowing it to be composed into other ASTs.
        /// <returns>A copy of the <see cref="DynamicKeywordStatementAst"/>, with the link to the previous parent removed.</returns>
            IEnumerable<CommandElementAst> commandElements = CopyElements(CommandElements);
            return new DynamicKeywordStatementAst(Extent, commandElements)
                Keyword = this.Keyword,
                LCurly = this.LCurly,
                FunctionName = this.FunctionName,
                InstanceName = CopyElement(this.InstanceName),
                OriginalInstanceName = CopyElement(this.OriginalInstanceName),
                BodyExpression = CopyElement(this.BodyExpression),
                ElementName = this.ElementName,
            return visitor2?.VisitDynamicKeywordStatement(this);
                action = visitor2.VisitDynamicKeywordStatement(this);
                foreach (CommandElementAst elementAst in CommandElements)
                    action = elementAst.InternalVisit(visitor);
        #region Internal Properties/Methods
        internal DynamicKeyword Keyword
                return _keyword;
                _keyword = value.Copy();
        private DynamicKeyword _keyword;
        internal Token LCurly { get; set; }
        internal Token FunctionName { get; set; }
        internal ExpressionAst InstanceName { get; set; }
        internal ExpressionAst OriginalInstanceName { get; set; }
        internal ExpressionAst BodyExpression { get; set; }
        internal string ElementName { get; set; }
        private PipelineAst _commandCallPipelineAst;
        internal PipelineAst GenerateCommandCallPipelineAst()
            if (_commandCallPipelineAst != null)
                return _commandCallPipelineAst;
            // Now construct the AST to call the function that defines implements the keywords logic. There are
            // two different types of ASTs that may be generated, depending on the settings in the DynamicKeyword object.
            // The first type uses a fixed set of 4 arguments and has the command with the signature
            //  function moduleName\keyword
            //  {
            //          $KeywordData,       # the DynamicKeyword object processed by this rule.
            //          $Name,              # the value of the name expression syntax element. This may be null for keywords that don't take a name
            //          $Value,             # the body of the keyword - either a hashtable or a scriptblock
            //          $SourceMetadata     # a string containing the original source line information so that errors
            //      # function logic...
            // The second type, where the DirectCall flag is set to true, simply calls the command directly.
            // In the original source, the keyword body will be a property collection where the allowed properties
            // in the collection correspond to the parameters on the actual called function.
            var cea = new Collection<CommandElementAst>();
            // First add the name of the command to call. If a module name has been provided
            // then use the module-qualified form of the command name..
            if (string.IsNullOrEmpty(Keyword.ImplementingModule))
                cea.Add(
                    new StringConstantExpressionAst(
                        FunctionName.Extent,
                        FunctionName.Text,
                        StringConstantType.BareWord));
                        Keyword.ImplementingModule + '\\' + FunctionName.Text,
            ExpressionAst expr = BodyExpression;
            if (Keyword.DirectCall)
                // If this keyword takes a name, then add it as the parameter -InstanceName
                if (Keyword.NameMode != DynamicKeywordNameMode.NoName)
                        new CommandParameterAst(
                            "InstanceName",
                            InstanceName,
                            FunctionName.Extent));
                // If it's a direct call keyword, then we just unravel the properties
                // in the hash literal expression and map them to parameters.
                // We've already checked to make sure that they're all valid names.
                if (expr is HashtableAst hashtable)
                    bool isHashtableValid = true;
                            isHashtableValid = false;
                            if (!Keyword.Properties.ContainsKey(propName.Value))
                        if (keyValueTuple.Item2 is ErrorStatementAst)
                    if (isHashtableValid)
                        // Construct the real parameters if Hashtable is valid
                            var propName = (StringConstantExpressionAst)keyValueTuple.Item1;
                            ExpressionAst propValue = new SubExpressionAst(
                                new StatementBlockAst(
                                    new StatementAst[] { (StatementAst)keyValueTuple.Item2.Copy() }, null));
                                    propValue,
                                    LCurly.Extent));
                        // Construct a fake parameter with the HashtableAst to be its value, so that
                        // tab completion on the property names would work.
                                "InvalidPropertyHashtable",
                                hashtable,
                // Add the -KeywordData parameter using the expression
                //  ([type]("System.Management.Automation.Language.DynamicKeyword"))::GetKeyword(name)
                // To invoke the method to get the keyword data object for that keyword.
                var indexExpr = new InvokeMemberExpressionAst(
                    new TypeExpressionAst(
                        new TypeName(
                            typeof(DynamicKeyword).FullName)),
                        "GetKeyword",
                    new List<ExpressionAst>
                                StringConstantType.BareWord)
                        "KeywordData",
                // Add the -Name parameter
                // Add the -Value parameter
                        LCurly.Extent,
                // Add the -SourceMetadata parameter
                string sourceMetadata = FunctionName.Extent.File
                                        + "::" + FunctionName.Extent.StartLineNumber
                                        + "::" + FunctionName.Extent.StartColumnNumber
                                        + "::" + FunctionName.Extent.Text;
                        LCurly.Extent, "SourceMetadata",
                            sourceMetadata,
            // Build the final statement - a pipeline containing a single element with is a CommandAst
            // containing the command we've built up.
            var cmdAst = new CommandAst(FunctionName.Extent, cea, TokenKind.Unknown, null);
            cmdAst.DefiningKeyword = Keyword;
            _commandCallPipelineAst = new PipelineAst(FunctionName.Extent, cmdAst, background: false);
    /// An abstract base class that represents all PowerShell expressions.
    public abstract class ExpressionAst : CommandElementAst
        /// Initialize the fields common to all expressions.
        protected ExpressionAst(IScriptExtent extent)
        /// By default, the static type of an expression is unknown and hence we assume <c>typeof(object)</c>.
        public virtual Type StaticType { get { return typeof(object); } }
        /// Determine if the results of ParenExpression/SubExpression should be preserved in case of exception.
        /// We should preserve the partial output in case of exception only if the SubExpression/ParenExpression meets following conditions:
        ///  1. the SubExpr/ParenExpr is the first expression, and the only element in a pipeline
        ///  2. the pipeline's parent is a StatementBlockAst or NamedBlockAst. e.g. $(1; throw 2) OR if (true) { $(1; throw 2) }
        internal virtual bool ShouldPreserveOutputInCaseOfException()
            if (this is not ParenExpressionAst and not SubExpressionAst)
            if (!(this.Parent is CommandExpressionAst commandExpr))
            var pipelineAst = commandExpr.Parent as PipelineAst;
            if (pipelineAst == null || pipelineAst.PipelineElements.Count > 1)
            if (pipelineAst.Parent is ParenExpressionAst parenExpressionAst)
                return parenExpressionAst.ShouldPreserveOutputInCaseOfException();
                return (pipelineAst.Parent is StatementBlockAst || pipelineAst.Parent is NamedBlockAst);
    /// The ast representing a ternary expression, e.g. <c>$a ? 1 : 2</c>.
    public class TernaryExpressionAst : ExpressionAst
        /// Initializes a new instance of the a ternary expression.
        /// <param name="condition">The condition operand.</param>
        /// <param name="ifTrue">The if clause.</param>
        /// <param name="ifFalse">The else clause.</param>
        public TernaryExpressionAst(IScriptExtent extent, ExpressionAst condition, ExpressionAst ifTrue, ExpressionAst ifFalse)
            Condition = condition ?? throw PSTraceSource.NewArgumentNullException(nameof(condition));
            IfTrue = ifTrue ?? throw PSTraceSource.NewArgumentNullException(nameof(ifTrue));
            IfFalse = ifFalse ?? throw PSTraceSource.NewArgumentNullException(nameof(ifFalse));
            SetParent(Condition);
            SetParent(IfTrue);
            SetParent(IfFalse);
        /// Gets the ast for the condition of the ternary expression. The property is never null.
        public ExpressionAst Condition { get; }
        /// Gets the ast for the if-operand of the ternary expression. The property is never null.
        public ExpressionAst IfTrue { get; }
        /// Gets the ast for the else-operand of the ternary expression. The property is never null.
        public ExpressionAst IfFalse { get; }
        /// Copy the TernaryExpressionAst instance.
        /// Returns a copy of the ast.
            ExpressionAst newCondition = CopyElement(this.Condition);
            ExpressionAst newIfTrue = CopyElement(this.IfTrue);
            ExpressionAst newIfFalse = CopyElement(this.IfFalse);
            return new TernaryExpressionAst(this.Extent, newCondition, newIfTrue, newIfFalse);
            if (visitor is ICustomAstVisitor2 visitor2)
                return visitor2.VisitTernaryExpression(this);
                action = visitor2.VisitTernaryExpression(this);
                action = IfTrue.InternalVisit(visitor);
                action = IfFalse.InternalVisit(visitor);
    /// The ast representing a binary expression, e.g. <c>$a + $b</c>.
    public class BinaryExpressionAst : ExpressionAst
        /// Initializes a new instance of the binary expression.
        /// <param name="left">The left hand operand.</param>
        /// <param name="operator">The binary operator.</param>
        /// <param name="right">The right hand operand.</param>
        /// The position to report if an error occurs at runtime while evaluating the binary operation.
        /// If <paramref name="operator"/> is not a valid binary operator.
        public BinaryExpressionAst(IScriptExtent extent, ExpressionAst left, TokenKind @operator, ExpressionAst right, IScriptExtent errorPosition)
            if ((@operator.GetTraits() & TokenFlags.BinaryOperator) == 0)
        /// The operator token kind.  The value returned is always a binary operator.
        /// The ast for the left hand side of the binary expression.  The property is never null.
        /// The ast for the right hand side of the binary expression.  The property is never null.
        public ExpressionAst Right { get; }
        /// The position to report an error if an error occurs at runtime.  The property is never null.
        /// Copy the BinaryExpressionAst instance.
            return new BinaryExpressionAst(this.Extent, newLeft, this.Operator, newRight, this.ErrorPosition);
        /// The result type of the operation.  For most binary operators, the type is unknown until runtime, but
        /// xor always results in <c>typeof(bool)</c>.
        public override Type StaticType
                switch (Operator)
        internal static readonly PSTypeName[] BoolTypeNameArray = new PSTypeName[] { new PSTypeName(typeof(bool)) };
        internal static readonly PSTypeName[] StringTypeNameArray = new PSTypeName[] { new PSTypeName(typeof(string)) };
        internal static readonly PSTypeName[] StringArrayTypeNameArray = new PSTypeName[] { new PSTypeName(typeof(string[])) };
            return visitor.VisitBinaryExpression(this);
            var action = visitor.VisitBinaryExpression(this);
    /// The ast representing an expression with a unary operator.
    public class UnaryExpressionAst : ExpressionAst
        /// Construct a unary expression.
        /// <param name="extent">The extent of the expression, including the operator (which may be prefix or postfix.).</param>
        /// <param name="tokenKind">The unary operator token kind for the operation.</param>
        /// <param name="child">The expression that the unary operator is applied to.</param>
        /// If <paramref name="extent"/> or <paramref name="child"/> is null.
        /// If <paramref name="tokenKind"/> is not a valid unary operator.
        public UnaryExpressionAst(IScriptExtent extent, TokenKind tokenKind, ExpressionAst child)
            if ((tokenKind.GetTraits() & TokenFlags.UnaryOperator) == 0)
                throw PSTraceSource.NewArgumentException(nameof(tokenKind));
                throw PSTraceSource.NewArgumentNullException(nameof(child));
            this.TokenKind = tokenKind;
            this.Child = child;
        /// The operator token for the unary expression.  The value returned is always a unary operator.
        public TokenKind TokenKind { get; }
        /// The child expression the unary operator is applied to.  The property is never null.
        public ExpressionAst Child { get; }
        /// Copy the UnaryExpressionAst instance.
            var newChild = CopyElement(this.Child);
            return new UnaryExpressionAst(this.Extent, this.TokenKind, newChild);
        /// Returns <c>typeof(bool)</c> if the unary operator is a logical negation, otherwise returns <c>typeof(object)</c>.
                return (TokenKind == TokenKind.Not || TokenKind == TokenKind.Exclaim)
                    ? typeof(bool)
            return visitor.VisitUnaryExpression(this);
            var action = visitor.VisitUnaryExpression(this);
                action = Child.InternalVisit(visitor);
    /// The ast that represents a scriptblock with a keyword name. This is normally allowed only for script workflow.
    /// e.g. <c>parallel { ... }</c> or <c>sequence { ... }</c>.
    public class BlockStatementAst : StatementAst
        /// Construct a keyword block expression.
        /// <param name="kind"></param>
        /// <param name="body"></param>
        public BlockStatementAst(IScriptExtent extent, Token kind, StatementBlockAst body)
            if (kind == null || body == null)
                throw PSTraceSource.NewArgumentNullException(kind == null ? "kind" : "body");
            if (kind.Kind != TokenKind.Sequence && kind.Kind != TokenKind.Parallel)
            this.Kind = kind;
        /// The scriptblockexpression that has a keyword applied to it. This property is nerver null.
        /// The keyword name.
        /// Copy the BlockStatementAst instance.
            return new BlockStatementAst(this.Extent, this.Kind, newBody);
            return visitor.VisitBlockStatement(this);
            var action = visitor.VisitBlockStatement(this);
    /// The ast that represents an expression with an attribute.  This is normally allowed only on parameters or variables
    /// being assigned, e.g. <c>[Parameter()]$PassThru</c> or <c>[ValidateScript({$true})$abc = 42</c>.
    public class AttributedExpressionAst : ExpressionAst, ISupportsAssignment, IAssignableValue
        /// Construct an attributed expression.
        /// <param name="attribute">The attribute being applied to <paramref name="child"/></param>
        /// <param name="child">The expression being attributed by <paramref name="attribute"/></param>
        /// If <paramref name="extent"/>, <paramref name="attribute"/>, or <paramref name="child"/> is null.
        public AttributedExpressionAst(IScriptExtent extent, AttributeBaseAst attribute, ExpressionAst child)
            if (attribute == null || child == null)
                throw PSTraceSource.NewArgumentNullException(attribute == null ? "attribute" : "child");
            this.Attribute = attribute;
            SetParent(attribute);
        /// The expression that has an attribute or type constraint applied to it.  This property is never null.
        /// The attribute or type constraint for this expression.  This property is never null.
        public AttributeBaseAst Attribute { get; }
        /// Copy the AttributedExpressionAst instance.
            var newAttribute = CopyElement(this.Attribute);
            return new AttributedExpressionAst(this.Extent, newAttribute, newChild);
            return visitor.VisitAttributedExpression(this);
            var action = visitor.VisitAttributedExpression(this);
                action = Attribute.InternalVisit(visitor);
        private ISupportsAssignment GetActualAssignableAst()
            ExpressionAst child = this;
            var childAttributeAst = child as AttributedExpressionAst;
            while (childAttributeAst != null)
                child = childAttributeAst.Child;
                childAttributeAst = child as AttributedExpressionAst;
            // Semantic checks ensure this cast succeeds
            return (ISupportsAssignment)child;
        private List<AttributeBaseAst> GetAttributes()
            var attributes = new List<AttributeBaseAst>();
            var childAttributeAst = this;
                attributes.Add(childAttributeAst.Attribute);
                childAttributeAst = childAttributeAst.Child as AttributedExpressionAst;
        IAssignableValue ISupportsAssignment.GetAssignableValue()
        Expression IAssignableValue.GetValue(Compiler compiler, List<Expression> exprs, List<ParameterExpression> temps)
            return (Expression)this.Accept(compiler);
        Expression IAssignableValue.SetValue(Compiler compiler, Expression rhs)
            var attributes = GetAttributes();
            var assignableValue = GetActualAssignableAst().GetAssignableValue();
            if (!(assignableValue is VariableExpressionAst variableExpr))
                return assignableValue.SetValue(compiler, Compiler.ConvertValue(rhs, attributes));
            return Compiler.CallSetVariable(Expression.Constant(variableExpr.VariablePath), rhs, Expression.Constant(attributes.ToArray()));
    /// The ast that represents a cast expression, e.g. <c>[wmiclass]"Win32_Process"</c>.
    public class ConvertExpressionAst : AttributedExpressionAst, ISupportsAssignment
        /// Construct a cast expression.
        /// The extent of the expression, starting with the type literal and ending after the expression being converted.
        /// <param name="typeConstraint">The type to convert to.</param>
        /// <param name="child">The expression being converted.</param>
        /// If <paramref name="extent"/>, <paramref name="typeConstraint"/>, or <paramref name="child"/> is null.
        public ConvertExpressionAst(IScriptExtent extent, TypeConstraintAst typeConstraint, ExpressionAst child)
            : base(extent, typeConstraint, child)
        /// The type to convert to.
        public TypeConstraintAst Type { get { return (TypeConstraintAst)Attribute; } }
        /// Copy the ConvertExpressionAst instance.
            var newTypeConstraint = CopyElement(this.Type);
            return new ConvertExpressionAst(this.Extent, newTypeConstraint, newChild);
        /// The static type produced after the cast is normally the type named by <see cref="Type"/>, but in some cases
        /// it may not be, in which, <see cref="Object"/> is assumed.
            get { return this.Type.TypeName.GetReflectionType() ?? typeof(object); }
            return visitor.VisitConvertExpression(this);
            var action = visitor.VisitConvertExpression(this);
                action = Type.InternalVisit(visitor);
            if (Child is VariableExpressionAst varExpr && varExpr.TupleIndex >= 0)
                // In the common case of a single cast on the lhs of an assignment, we may have saved the type of the
                // variable in the mutable tuple, so conversions will get generated elsewhere, and we can just use
                // the child as the assignable value.
                return varExpr;
        internal bool IsRef()
            return Type.TypeName.Name.Equals("ref", StringComparison.OrdinalIgnoreCase);
    /// The ast that represents accessing a member as a property, e.g. <c>$x.Length</c> or <c>[int]::MaxValue</c>.
    /// Most often this is a simple property access, but methods can also be access in this manner, returning an object
    /// that supports invoking that member.
    public class MemberExpressionAst : ExpressionAst, ISupportsAssignment
        /// Initializes a new instance of the <see cref="MemberExpressionAst"/> class.
        /// The extent of the expression, starting with the expression before the operator '.' or '::' and ending after
        /// membername or expression naming the member.
        /// <param name="expression">The expression before the member access operator '.' or '::'.</param>
        /// <param name="member">The name or expression naming the member to access.</param>
        /// <param name="static">True if the '::' operator was used, false if '.' is used.
        /// True if the member access is for a static member, using '::', false if accessing a member on an instance using '.'.
        /// If <paramref name="extent"/>, <paramref name="expression"/>, or <paramref name="member"/> is null.
        public MemberExpressionAst(IScriptExtent extent, ExpressionAst expression, CommandElementAst member, bool @static)
            if (expression == null || member == null)
                throw PSTraceSource.NewArgumentNullException(expression == null ? "expression" : "member");
            this.Member = member;
            SetParent(member);
            this.Static = @static;
        /// The extent of the expression, starting with the expression before the operator '.', '::' or '?.' and ending after
        /// <param name="expression">The expression before the member access operator '.', '::' or '?.'.</param>
        /// <param name="static">True if the '::' operator was used, false if '.' or '?.' is used.</param>
        /// <param name="nullConditional">True if '?.' used.</param>
        public MemberExpressionAst(IScriptExtent extent, ExpressionAst expression, CommandElementAst member, bool @static, bool nullConditional)
            : this(extent, expression, member, @static)
            this.NullConditional = nullConditional;
        /// The expression that produces the value to retrieve the member from.  This property is never null.
        /// The name of the member to retrieve.  This property is never null.
        public CommandElementAst Member { get; }
        /// True if the member to return is static, false if the member is an instance member.
        public bool Static { get; }
        /// Gets a value indicating true if the operator used is ?. or ?[].
        public bool NullConditional { get; protected set; }
        /// Copy the MemberExpressionAst instance.
            var newMember = CopyElement(this.Member);
                newExpression,
                newMember,
                this.Static,
                this.NullConditional);
            return visitor.VisitMemberExpression(this);
            var action = visitor.VisitMemberExpression(this);
                action = Member.InternalVisit(visitor);
            return new MemberAssignableValue { MemberExpression = this };
    /// The ast that represents the invocation of a method, e.g. <c>$sb.Append('abc')</c> or <c>[math]::Sign($i)</c>.
    public class InvokeMemberExpressionAst : MemberExpressionAst, ISupportsAssignment
        /// Initializes a new instance of the <see cref="InvokeMemberExpressionAst"/> class.
        /// The extent of the expression, starting with the expression before the invocation operator and ending with the
        /// closing paren after the arguments.
        /// <param name="expression">The expression before the invocation operator ('.', '::').</param>
        /// <param name="method">The method to invoke.</param>
        /// <param name="arguments">The arguments to pass to the method.</param>
        /// <param name="static">
        /// True if the invocation is for a static method, using '::', false if invoking a method on an instance using '.'.
        /// <param name="genericTypes">The generic type arguments passed to the method.</param>
        public InvokeMemberExpressionAst(
            CommandElementAst method,
            IEnumerable<ExpressionAst> arguments,
            : base(extent, expression, method, @static)
            if (arguments != null && arguments.Any())
                this.Arguments = new ReadOnlyCollection<ExpressionAst>(arguments.ToArray());
                SetParents(Arguments);
            if (genericTypes != null && genericTypes.Count > 0)
                this.GenericTypeArguments = new ReadOnlyCollection<ITypeName>(genericTypes);
            bool @static)
            : this(extent, expression, method, arguments, @static, genericTypes: null)
        /// <param name="expression">The expression before the invocation operator ('.', '::' or '?.').</param>
        /// True if the invocation is for a static method, using '::', false if invoking a method on an instance using '.' or '?.'.
        /// <param name="nullConditional">True if the operator used is '?.'.</param>
            bool nullConditional,
            : this(extent, expression, method, arguments, @static, genericTypes)
            bool nullConditional)
            : this(extent, expression, method, arguments, @static, nullConditional, genericTypes: null)
        /// Gets a list of generic type arguments passed to this method invocation.
        public ReadOnlyCollection<ITypeName> GenericTypeArguments { get; }
        /// The non-empty collection of arguments to pass when invoking the method, or null if no arguments were specified.
        public ReadOnlyCollection<ExpressionAst> Arguments { get; }
        /// Copy the InvokeMemberExpressionAst instance.
            var newMethod = CopyElement(this.Member);
            var newArguments = CopyElements(this.Arguments);
                newMethod,
                newArguments,
                this.NullConditional,
                this.GenericTypeArguments);
            return visitor.VisitInvokeMemberExpression(this);
            var action = visitor.VisitInvokeMemberExpression(this);
                action = this.InternalVisitChildren(visitor);
        internal AstVisitAction InternalVisitChildren(AstVisitor visitor)
            var action = Expression.InternalVisit(visitor);
            if (action == AstVisitAction.Continue && Arguments != null)
                for (int index = 0; index < Arguments.Count; index++)
                    var arg = Arguments[index];
                    action = arg.InternalVisit(visitor);
            return new InvokeMemberAssignableValue { InvokeMemberExpressionAst = this };
    /// The ast that represents the invocation of a base ctor method from PS class instance ctor, e.g. <c>class B : A{ B() : base() {} }</c>.
    public class BaseCtorInvokeMemberExpressionAst : InvokeMemberExpressionAst
        /// Construct an instance of a base ctor invocation expression.
        /// <param name="baseKeywordExtent">
        /// The extent of the base keyword, i.e. for
        /// <c>class B : A { B() : base(100) {} }</c>
        /// it will be "base".
        /// Can be empty extent (i.e. for implicit base ctor call).
        /// <param name="baseCallExtent">
        /// The extent of the base ctor call expression, i.e. for
        /// it will be "base(100)"
        /// <param name="arguments">The arguments to pass to the ctor.</param>
        public BaseCtorInvokeMemberExpressionAst(IScriptExtent baseKeywordExtent, IScriptExtent baseCallExtent, IEnumerable<ExpressionAst> arguments)
                baseCallExtent,
                new VariableExpressionAst(baseKeywordExtent, "this", false),
                new StringConstantExpressionAst(baseKeywordExtent, ".ctor", StringConstantType.BareWord),
                @static: false)
                action = visitor2.VisitBaseCtorInvokeMemberExpression(this);
            return visitor2?.VisitBaseCtorInvokeMemberExpression(this);
    /// The name and attributes of a type.
    public interface ITypeName
        /// The full name of the type, including any namespace and assembly name.
        string FullName { get; }
        /// The name of the type, including any namespace, but not including the assembly name.
        /// The name of the assembly, if specified, otherwise null.
        string? AssemblyName { get; }
        /// Returns true if the type names an array, false otherwise.
        bool IsArray { get; }
        /// Returns true if the type names a closed generic type (has generic type arguments), false otherwise.
        bool IsGeneric { get; }
        /// Returns the <see cref="System.Type"/> that this typename represents, if such a type exists, null otherwise.
        Type? GetReflectionType();
        /// Assuming the typename is an attribute, returns the <see cref="System.Type"/> that this typename represents.
        /// By convention, the typename may omit the suffix "Attribute".  Lookup will attempt to resolve the type as is,
        /// and if that fails, the suffix "Attribute" will be appended.
        Type? GetReflectionAttributeType();
        /// The extent of the typename.
        IScriptExtent Extent { get; }
    internal interface ISupportsTypeCaching
        Type? CachedType { get; set; }
    /// A simple type that is not an array or does not have generic arguments.
    public sealed class TypeName : ITypeName, ISupportsTypeCaching
        private readonly int _genericArgumentCount;
        internal TypeDefinitionAst _typeDefinitionAst;
        /// Construct a simple typename.
        /// <param name="extent">The extent of the typename.</param>
        /// If <paramref name="name"/> is null or the empty string.
        /// If <paramref name="name"/> contains characters that are only allowed in a generic or array typename.
        public TypeName(IScriptExtent extent, string name)
            if (extent == null || string.IsNullOrEmpty(name))
                throw PSTraceSource.NewArgumentNullException(extent == null ? "extent" : "name");
            var c = name[0];
            if (c == '[' || c == ']' || c == ',')
            if (name.Contains('`'))
                name = name.Replace("``", "`");
            this._extent = extent;
            this._name = name;
        /// Construct a typename with an assembly specification.
        /// <param name="assembly">The assembly the type belongs to.</param>
        /// If <paramref name="extent"/> is null or if <paramref name="name"/> or <paramref name="assembly"/> is null or the empty string.
        public TypeName(IScriptExtent extent, string name, string assembly)
            : this(extent, name)
            if (string.IsNullOrEmpty(assembly))
            AssemblyName = assembly;
        /// Construct a typename that represents a generic type definition.
        /// <param name="genericArgumentCount">The number of generic arguments.</param>
        internal TypeName(IScriptExtent extent, string name, int genericArgumentCount)
            ArgumentOutOfRangeException.ThrowIfLessThan(genericArgumentCount, 0);
            if (genericArgumentCount > 0 && !_name.Contains('`'))
                _genericArgumentCount = genericArgumentCount;
        /// Returns the full name of the type.
        public string FullName { get { return AssemblyName != null ? _name + "," + AssemblyName : _name; } }
        /// Returns the name of the type, w/o any assembly name if one was specified.
        public string Name { get { return _name; } }
        public string AssemblyName { get; internal set; }
        /// Always returns false, array typenames are instances of <see cref="ArrayTypeName"/>.
        public bool IsArray { get { return false; } }
        /// Always returns false, generic typenames are instances of <see cref="GenericTypeName"/>.
        public bool IsGeneric { get { return false; } }
        public IScriptExtent Extent { get { return _extent; } }
        internal bool HasDefaultCtor()
            if (_typeDefinitionAst == null)
                Type reflectionType = GetReflectionType();
                if (reflectionType == null)
                    // we are pessimistic about default ctor presence.
                return reflectionType.HasDefaultCtor();
            bool hasExplicitCtor = false;
                if (member is FunctionMemberAst function)
                    if (function.IsConstructor)
                        // TODO: add check for default values, once default values for parameters supported
                        if (function.Parameters.Count == 0)
                        hasExplicitCtor = true;
            // implicit ctor is default as well
            return !hasExplicitCtor;
        /// Get the <see cref="Type"/> from a typename.
        /// The <see cref="Type"/> if possible, null otherwise.  Null may be returned for valid typenames if the assembly
        /// containing the type has not been loaded.
        public Type GetReflectionType()
                Type type = _typeDefinitionAst != null ? _typeDefinitionAst.Type : TypeResolver.ResolveTypeName(this, out _);
                if (type is null && _genericArgumentCount > 0)
                    // We try an alternate name only if it failed to resolve with the original name.
                    // This is because for a generic type like `System.Tuple<type1, type2>`, the original name `System.Tuple`
                    // can be resolved and hence `genericTypeName.TypeName.GetReflectionType()` in that case has always been
                    // returning the type `System.Tuple`. If we change to directly use the alternate name for resolution, the
                    // return value will become 'System.Tuple`1' in that case, and that's a breaking change.
                    TypeName newTypeName = new(
                        _extent,
                        string.Create(CultureInfo.InvariantCulture, $"{_name}`{_genericArgumentCount}"))
                        AssemblyName = AssemblyName
                    type = TypeResolver.ResolveTypeName(newTypeName, out _);
                        var unused = type.TypeHandle;
                        // If the value of 'type' comes from typeBuilder.AsType(), then the value of 'type.GetTypeInfo()'
                        // is actually the TypeBuilder instance itself. This is the same on both FullCLR and CoreCLR.
                        Diagnostics.Assert(_typeDefinitionAst != null, "_typeDefinitionAst can never be null");
                    Interlocked.CompareExchange(ref _type, type, null);
        /// Returns the <see cref="Type"/> this type represents, assuming the type is an attribute.  The suffix
        /// "Attribute" may be appended, if necessary, to resolve the type.
        public Type GetReflectionAttributeType()
            var result = GetReflectionType();
            if (result == null || !typeof(Attribute).IsAssignableFrom(result))
                TypeName attrTypeName = new(_extent, $"{_name}Attribute", _genericArgumentCount)
                result = attrTypeName.GetReflectionType();
                if (result != null && !typeof(Attribute).IsAssignableFrom(result))
        internal void SetTypeDefinition(TypeDefinitionAst typeDefinitionAst)
            Diagnostics.Assert(_typeDefinitionAst == null, "Class definition is already set and cannot be changed");
            Diagnostics.Assert(_type == null, "Cannot set class definition if type is already resolved");
        /// Simply return the <see cref="FullName"/> of the type.
            if (!(obj is TypeName other))
            if (!_name.Equals(other._name, StringComparison.OrdinalIgnoreCase))
            if (AssemblyName == null)
                return other.AssemblyName == null;
            if (other.AssemblyName == null)
            return AssemblyName.Equals(other.AssemblyName, StringComparison.OrdinalIgnoreCase);
            var nameHashCode = stringComparer.GetHashCode(_name);
                return nameHashCode;
            return Utils.CombineHashCodes(nameHashCode, stringComparer.GetHashCode(AssemblyName));
        /// Check if the type names a <see cref="System.Type"/>, false otherwise.
        /// <param name="type">The given <see cref="System.Type"/></param>
        /// <returns>Returns true if the type names a <see cref="System.Type"/>, false otherwise.</returns>
        ///  This helper function is now used to check 'Void' type only;
        ///  Other types may not work, for example, 'int'
        internal bool IsType(Type type)
            string fullTypeName = type.FullName;
            if (fullTypeName.Equals(Name, StringComparison.OrdinalIgnoreCase))
            int lastDotIndex = fullTypeName.LastIndexOf('.');
            if (lastDotIndex >= 0)
                return fullTypeName.AsSpan(lastDotIndex + 1).Equals(Name, StringComparison.OrdinalIgnoreCase);
        Type ISupportsTypeCaching.CachedType
            get { return _type; }
            set { _type = value; }
    /// Represent a closed generic type including its arguments.
    public sealed class GenericTypeName : ITypeName, ISupportsTypeCaching
        private string _cachedFullName;
        /// Construct a generic type name.
        /// <param name="extent">The extent of the generic typename.</param>
        /// <param name="genericTypeName">
        /// The name of the generic class.  The name does not need to include the backtick and number of expected arguments,
        /// (e.g. <c>System.Collections.Generic.Dictionary`2</c>, but the backtick and number be included.
        /// <param name="genericArguments">
        /// The list of typenames that represent the arguments to the generic type named by <paramref name="genericTypeName"/>.
        /// If <paramref name="genericTypeName"/> is null.
        /// If <paramref name="genericArguments"/> is null or if <paramref name="genericArguments"/> is an empty collection.
        public GenericTypeName(IScriptExtent extent, ITypeName genericTypeName, IEnumerable<ITypeName> genericArguments)
            if (genericTypeName == null || extent == null)
                throw PSTraceSource.NewArgumentNullException(extent == null ? "extent" : "genericTypeName");
            if (genericArguments == null)
                throw PSTraceSource.NewArgumentException(nameof(genericArguments));
            this.TypeName = genericTypeName;
            this.GenericArguments = new ReadOnlyCollection<ITypeName>(genericArguments.ToArray());
            if (this.GenericArguments.Count == 0)
        /// Return the typename, using PowerShell syntax for generic type arguments.
                if (_cachedFullName == null)
                    sb.Append(TypeName.Name);
                    for (int index = 0; index < GenericArguments.Count; index++)
                        ITypeName typename = GenericArguments[index];
                        sb.Append(typename.FullName);
                    var assemblyName = TypeName.AssemblyName;
                    if (assemblyName != null)
                        sb.Append(assemblyName);
                    Interlocked.CompareExchange(ref _cachedFullName, sb.ToString(), null);
                return _cachedFullName;
        /// The name of the type, including any namespace, but not including the assembly name, using PowerShell syntax for generic type arguments.
                    sb.Append(typename.Name);
        public string AssemblyName { get { return TypeName.AssemblyName; } }
        /// Always returns false because this class does not represent arrays.
        /// Always returns true because this class represents generics.
        public bool IsGeneric { get { return true; } }
        /// The typename that specifies the generic class.
        /// The generic arguments for this typename.
        public ReadOnlyCollection<ITypeName> GenericArguments { get; }
                Type generic = GetGenericType(TypeName.GetReflectionType());
                if (generic != null && generic.ContainsGenericParameters)
                    var argumentList = new List<Type>();
                    foreach (var arg in GenericArguments)
                        var type = arg.GetReflectionType();
                        argumentList.Add(type);
                        var type = generic.MakeGenericType(argumentList.ToArray());
                            // We don't need the TypeHandle, but it's a good indication that a type doesn't
                            // support reflection which we need for many things.  So if this throws, we
                            // won't cache the type.  This situation arises while defining a type that
                            // refers to itself somehow (e.g. returns an instance of this type, array of
                            // this type, or this type as a generic parameter.)
                            // By not caching, we'll eventually get the real reflection capable type after
                            // the type is fully defined.
                        Interlocked.CompareExchange(ref _cachedType, type, null);
                        // We don't want to throw exception from GetReflectionType
        /// Get the actual generic type if it's necessary.
        /// <param name="generic"></param>
        internal Type GetGenericType(Type generic)
            if (generic == null || !generic.ContainsGenericParameters)
                if (!TypeName.FullName.Contains('`'))
                        Extent,
                        string.Create(CultureInfo.InvariantCulture, $"{TypeName.Name}`{GenericArguments.Count}"))
                        AssemblyName = TypeName.AssemblyName
                    generic = newTypeName.GetReflectionType();
            return generic;
            Type type = GetReflectionType();
                Type generic = TypeName.GetReflectionAttributeType();
                            string.Create(CultureInfo.InvariantCulture, $"{TypeName.Name}Attribute`{GenericArguments.Count}"))
                    type = generic.MakeGenericType((from arg in GenericArguments select arg.GetReflectionType()).ToArray());
            if (!(obj is GenericTypeName other))
            if (!TypeName.Equals(other.TypeName))
            if (GenericArguments.Count != other.GenericArguments.Count)
            var count = GenericArguments.Count;
                if (!GenericArguments[i].Equals(other.GenericArguments[i]))
            int hash = TypeName.GetHashCode();
                hash = Utils.CombineHashCodes(hash, GenericArguments[i].GetHashCode());
            get { return _cachedType; }
            set { _cachedType = value; }
    /// Represents the name of an array type including the dimensions.
    public sealed class ArrayTypeName : ITypeName, ISupportsTypeCaching
        /// Construct an ArrayTypeName.
        /// <param name="extent">The extent of the array typename.</param>
        /// <param name="elementType">The name of the element type.</param>
        /// <param name="rank">The number of dimensions in the array.</param>
        /// If <paramref name="extent"/> or <paramref name="elementType"/> is null.
        /// If <paramref name="rank"/> is 0 or negative.
        public ArrayTypeName(IScriptExtent extent, ITypeName elementType, int rank)
            if (extent == null || elementType == null)
                throw PSTraceSource.NewArgumentException(nameof(rank));
            this.Rank = rank;
            this.ElementType = elementType;
        private string GetName(bool includeAssemblyName)
                sb.Append(ElementType.Name);
                if (Rank > 1)
                    sb.Append(',', Rank - 1);
                if (includeAssemblyName)
                    var assemblyName = ElementType.AssemblyName;
        /// Return the typename, using PowerShell syntax for the array dimensions.
                    Interlocked.CompareExchange(ref _cachedFullName, GetName(includeAssemblyName: true), null);
        /// The name of the type, including any namespace, but not including the assembly name, using PowerShell syntax for the array dimensions.
            get { return GetName(includeAssemblyName: false); }
        public string AssemblyName { get { return ElementType.AssemblyName; } }
        /// Returns true always as this class represents arrays.
        public bool IsArray { get { return true; } }
        /// Returns false always as this class never represents generics.
        /// The element type of the array.
        public ITypeName ElementType { get; }
        /// The rank of the array.
        public int Rank { get; }
                    Type elementType = ElementType.GetReflectionType();
                    if (elementType != null)
                        Type type = Rank == 1 ? elementType.MakeArrayType() : elementType.MakeArrayType(Rank);
        /// Always return null, arrays can never be an attribute.
            if (!(obj is ArrayTypeName other))
            return ElementType.Equals(other.ElementType) && Rank == other.Rank;
            return Utils.CombineHashCodes(ElementType.GetHashCode(), Rank.GetHashCode());
    /// A class that allows a <see cref="System.Type"/> to be used directly in the PowerShell ast.
    public sealed class ReflectionTypeName : ITypeName, ISupportsTypeCaching
        /// Construct a typename from a <see cref="System.Type"/>.
        /// <param name="type">The type to wrap.</param>
        public ReflectionTypeName(Type type)
        /// Returns the typename in PowerShell syntax.
        public string FullName { get { return ToStringCodeMethods.Type(_type); } }
        public string Name { get { return FullName; } }
        /// The name of the assembly.
        public string AssemblyName { get { return _type.Assembly.FullName; } }
        /// Returns true if the type is an array, false otherwise.
        public bool IsArray { get { return _type.IsArray; } }
        /// Returns true if the type is a generic, false otherwise.
        public bool IsGeneric { get { return _type.IsGenericType; } }
        public IScriptExtent Extent { get { return PositionUtilities.EmptyExtent; } }
        /// Returns the <see cref="System.Type"/> for this typename.  Never returns null.
            if (!(obj is ReflectionTypeName other))
            return _type == other._type;
            return _type.GetHashCode();
            set { throw new InvalidOperationException(); }
    /// The ast that represents a type literal expression, e.g. <c>[int]</c>.
    public class TypeExpressionAst : ExpressionAst
        /// Construct a type literal expression.
        /// <param name="extent">The extent of the typename, including the opening and closing square braces.</param>
        /// <param name="typeName">The typename for the constructed ast.</param>
        public TypeExpressionAst(IScriptExtent extent, ITypeName typeName)
        /// The name of the type.  This property is never null.
        /// Copy the TypeExpressionAst instance.
            return new TypeExpressionAst(this.Extent, this.TypeName);
        /// The static type of a type literal is always <c>typeof(Type)</c>.
        public override Type StaticType { get { return typeof(Type); } }
            return visitor.VisitTypeExpression(this);
            var action = visitor.VisitTypeExpression(this);
            return visitor.CheckForPostAction(this, (action == AstVisitAction.SkipChildren ? AstVisitAction.Continue : action));
    /// The ast representing a variable reference, either normal references, e.g. <c>$true</c>, or splatted references
    /// <c>@PSBoundParameters</c>.
    public class VariableExpressionAst : ExpressionAst, ISupportsAssignment, IAssignableValue
        /// Construct a variable reference.
        /// <param name="extent">The extent of the variable.</param>
        /// <param name="variableName">
        /// The name of the variable.  A leading '$' or '@' is not removed, those characters are assumed to be part of
        /// the variable name.
        /// <param name="splatted">True if splatting, like <c>@PSBoundParameters</c>, false otherwise, like <c>$false</c></param>
        /// If <paramref name="extent"/> or <paramref name="variableName"/> is null, or if <paramref name="variableName"/>
        public VariableExpressionAst(IScriptExtent extent, string variableName, bool splatted)
                throw PSTraceSource.NewArgumentNullException(nameof(variableName));
            this.VariablePath = new VariablePath(variableName);
            this.Splatted = splatted;
        /// Construct a variable reference from a token.  Used from the parser.
        internal VariableExpressionAst(VariableToken token)
            : this(token.Extent, token.VariablePath, (token.Kind == TokenKind.SplattedVariable))
        /// Construct a variable reference with an existing VariablePath (rather than construct a new one.)
        /// If <paramref name="extent"/> or <paramref name="variablePath"/> is null.
        public VariableExpressionAst(IScriptExtent extent, VariablePath variablePath, bool splatted)
            this.VariablePath = variablePath;
        /// The name of the variable.  This property is never null.
        public VariablePath VariablePath { get; }
        /// True if splatting syntax was used, false otherwise.
        public bool Splatted { get; }
        /// Check if the variable is one of $true, $false and $null.
        /// True if it is a constant variable
        public bool IsConstantVariable()
            if (this.VariablePath.IsVariable)
                string name = this.VariablePath.UnqualifiedPath;
                if (name.Equals(SpecialVariables.True, StringComparison.OrdinalIgnoreCase) ||
                    name.Equals(SpecialVariables.False, StringComparison.OrdinalIgnoreCase) ||
                    name.Equals(SpecialVariables.Null, StringComparison.OrdinalIgnoreCase))
        /// Copy the VariableExpressionAst instance.
            return new VariableExpressionAst(this.Extent, this.VariablePath, this.Splatted);
        internal bool IsSafeVariableReference(HashSet<string> validVariables, ref bool usesParameter)
            if (this.VariablePath.IsAnyLocal())
                var varName = this.VariablePath.UnqualifiedPath;
                if (((validVariables != null) && validVariables.Contains(varName)) ||
                    varName.Equals(SpecialVariables.Args, StringComparison.OrdinalIgnoreCase))
                    usesParameter = true;
                    ok = !this.Splatted
                         && this.IsConstantVariable();
            return visitor.VisitVariableExpression(this);
            var action = visitor.VisitVariableExpression(this);
        internal bool Automatic { get; set; }
        internal bool Assigned { get; set; }
            return (Expression)compiler.VisitVariableExpression(this);
            if (this.VariablePath.IsVariable && this.VariablePath.UnqualifiedPath.Equals(SpecialVariables.Null, StringComparison.OrdinalIgnoreCase))
            IEnumerable<PropertyInfo> tupleAccessPath;
            bool localInTuple;
            Type targetType = GetVariableType(compiler, out tupleAccessPath, out localInTuple);
            // Value types must be copied on assignment (if they are mutable), boxed or not.  This preserves language
            // semantics from V1/V2, and might be slightly more natural for a dynamic language.
            // To generate good code, we assume any object or PSObject could be a boxed value type, and generate a dynamic
            // site to handle copying as necessary.  Given this assumption, here are the possibilities:
            //     - rhs is reference type
            //         * just convert to lhs type
            //     - rhs is value type
            //         * lhs type is value type, copy made by simple assignment
            //         * lhs type is object, copy is made by boxing, also handled in simple assignment
            //     - rhs is boxed value type
            //         * lhs type is value type, copy is made by unboxing conversion
            //         * lhs type is object/psobject, copy must be made dynamically (boxed value type can't be known statically)
            var rhsType = rhs.Type;
            if (localInTuple &&
                (targetType == typeof(object) || targetType == typeof(PSObject)) &&
                (rhsType == typeof(object) || rhsType == typeof(PSObject)))
                rhs = DynamicExpression.Dynamic(PSVariableAssignmentBinder.Get(), typeof(object), rhs);
            rhs = rhs.Convert(targetType);
            if (!localInTuple)
                return Compiler.CallSetVariable(Expression.Constant(VariablePath), rhs);
            Expression lhs = compiler.LocalVariablesParameter;
            foreach (var property in tupleAccessPath)
                lhs = Expression.Property(lhs, property);
            return Expression.Assign(lhs, rhs);
        internal Type GetVariableType(Compiler compiler, out IEnumerable<PropertyInfo> tupleAccessPath, out bool localInTuple)
            Type targetType = null;
            localInTuple = TupleIndex >= 0 &&
                                (compiler.Optimize || TupleIndex < (int)AutomaticVariable.NumberOfAutomaticVariables);
            tupleAccessPath = null;
            if (localInTuple)
                tupleAccessPath = MutableTuple.GetAccessPath(compiler.LocalVariablesTupleType, TupleIndex);
                targetType = tupleAccessPath.Last().PropertyType;
                targetType = typeof(object);
    /// The ast representing constant values, such as numbers.  Constant values mean truly constant, as in, the value is
    /// always the same.  Expandable strings with variable references (e.g. <c>"$val"</c>) or sub-expressions
    /// (e.g. <c>"$(1)"</c>) are not considered constant.
    public class ConstantExpressionAst : ExpressionAst
        /// Construct a constant expression.
        /// <param name="extent">The extent of the constant.</param>
        /// <param name="value">The value of the constant.</param>
        public ConstantExpressionAst(IScriptExtent extent, object value)
        internal ConstantExpressionAst(NumberToken token)
            : base(token.Extent)
            this.Value = token.Value;
        /// The value of the constant.  This property is null only if the expression represents the null constant.
        /// Copy the ConstantExpressionAst instance.
            return new ConstantExpressionAst(this.Extent, this.Value);
        /// The static type of a constant is whatever type the value is, or if null, then assume it's <c>typeof(object)</c>.
            get { return Value != null ? Value.GetType() : typeof(object); }
            return visitor.VisitConstantExpression(this);
            var action = visitor.VisitConstantExpression(this);
    /// The kind of string constant.
    public enum StringConstantType
        /// A string enclosed in single quotes, e.g. <c>'some text'</c>.
        SingleQuoted,
        /// A here string enclosed in single quotes, e.g. <c> @'
        /// a here string
        /// '@
        SingleQuotedHereString,
        /// A string enclosed in double quotes, e.g. <c>"some text"</c>.
        DoubleQuoted,
        /// A here string enclosed in double quotes, e.g. <c> @"
        /// "@
        DoubleQuotedHereString,
        /// A string like token not enclosed in any quotes.  This usually includes a command name or command argument.
        BareWord
    /// The ast that represents a constant string expression that is always constant.  This includes both single and
    /// double quoted strings, but the double quoted strings will not be scanned for variable references and sub-expressions.
    /// If expansion of the string is required, use <see cref="ExpandableStringExpressionAst"/>.
    public class StringConstantExpressionAst : ConstantExpressionAst
        /// Construct a string constant expression.
        /// <param name="extent">The extent of the string constant, including quotes.</param>
        /// <param name="value">The value of the string.</param>
        /// <param name="stringConstantType">The type of string.</param>
        /// If <paramref name="extent"/> or <paramref name="value"/> is null.
        public StringConstantExpressionAst(IScriptExtent extent, string value, StringConstantType stringConstantType)
            : base(extent, value)
            this.StringConstantType = stringConstantType;
        internal StringConstantExpressionAst(StringToken token)
            : base(token.Extent, token.Value)
            this.StringConstantType = MapTokenKindToStringConstantKind(token);
        /// The type of string.
        public StringConstantType StringConstantType { get; }
        /// The value of the string, not including the quotes used.
        public new string Value { get { return (string)base.Value; } }
        /// Copy the StringConstantExpressionAst instance.
            return new StringConstantExpressionAst(this.Extent, this.Value, this.StringConstantType);
        /// The type of a StringConstantExpressionAst is always <c>typeof(string)</c>.
            get { return typeof(string); }
        internal static StringConstantType MapTokenKindToStringConstantKind(Token token)
                    return StringConstantType.DoubleQuoted;
                    return StringConstantType.SingleQuotedHereString;
                    return StringConstantType.DoubleQuotedHereString;
                    return StringConstantType.SingleQuoted;
                    return StringConstantType.BareWord;
            return visitor.VisitStringConstantExpression(this);
            var action = visitor.VisitStringConstantExpression(this);
    /// The ast that represents a double quoted string (here string or normal string) and can have nested variable
    /// references or sub-expressions, e.g. <c>"Name: $name`nAge: $([DateTime]::Now.Year - $dob.Year)"</c>.
    public class ExpandableStringExpressionAst : ExpressionAst
        /// Construct an expandable string.  The value is scanned for nested variable references and expressions
        /// which are evaluated at runtime when this ast is compiled.
        /// <param name="extent">The extent of the string.</param>
        /// <param name="value">The unexpanded value of the string.</param>
        /// <param name="type">The kind of string, must be one of<list>
        /// <see cref="System.Management.Automation.Language.StringConstantType.DoubleQuoted"/>
        /// <see cref="System.Management.Automation.Language.StringConstantType.DoubleQuotedHereString"/>
        /// <see cref="System.Management.Automation.Language.StringConstantType.BareWord"/>
        /// </list></param>
        /// If <paramref name="value"/> or <paramref name="extent"/> is null.
        public ExpandableStringExpressionAst(IScriptExtent extent,
                                             StringConstantType type)
            if (type != StringConstantType.DoubleQuoted && type != StringConstantType.DoubleQuotedHereString
                && type != StringConstantType.BareWord)
            var ast = Language.Parser.ScanString(value);
            if (ast is ExpandableStringExpressionAst expandableStringAst)
                this.FormatExpression = expandableStringAst.FormatExpression;
                this.NestedExpressions = expandableStringAst.NestedExpressions;
                // We always compile to a format expression.  In the rare case that some external code (this can't happen
                // internally) passes in a string that doesn't require any expansion, we still need to generate code that
                // works.  This is slow as the ast is a string constant, but it should be rare.
                this.FormatExpression = "{0}";
                this.NestedExpressions = new ReadOnlyCollection<ExpressionAst>(new[] { ast });
            // Set parents properly
            for (int i = 0; i < this.NestedExpressions.Count; i++)
                this.NestedExpressions[i].ClearParent();
            SetParents(this.NestedExpressions);
            this.StringConstantType = type;
        /// Construct an expandable string expression from a string token.  Used from the parser after parsing
        /// the nested tokens.  This method is internal mainly so we can avoid validating <paramref name="formatString"/>.
        internal ExpandableStringExpressionAst(Token token, string value, string formatString, IEnumerable<ExpressionAst> nestedExpressions)
            : this(token.Extent, value, formatString,
                   StringConstantExpressionAst
                        .MapTokenKindToStringConstantKind(token),
                   nestedExpressions)
        private ExpandableStringExpressionAst(IScriptExtent extent, string value, string formatString,
                                              StringConstantType type, IEnumerable<ExpressionAst> nestedExpressions)
            Diagnostics.Assert(nestedExpressions != null && nestedExpressions.Any(), "Must specify non-empty expressions.");
            this.FormatExpression = formatString;
            this.NestedExpressions = new ReadOnlyCollection<ExpressionAst>(nestedExpressions.ToArray());
            SetParents(NestedExpressions);
        /// The value of string, not including the quote characters and without any variables replaced.
        public string Value { get; }
        /// A non-empty collection of expressions contained within the string.  The nested expressions are always either
        /// instances of <see cref="VariableExpressionAst"/> or <see cref="SubExpressionAst"/>.
        public ReadOnlyCollection<ExpressionAst> NestedExpressions { get; }
        /// Copy the ExpandableStringExpressionAst instance.
            var newNestedExpressions = CopyElements(this.NestedExpressions);
            return new ExpandableStringExpressionAst(this.Extent, this.Value, this.FormatExpression, this.StringConstantType, newNestedExpressions);
        /// The format expression needed to execute this ast.  It is generated by the scanner, it is not provided by clients.
        internal string FormatExpression { get; }
            return visitor.VisitExpandableStringExpression(this);
            var action = visitor.VisitExpandableStringExpression(this);
            if (action == AstVisitAction.Continue && NestedExpressions != null)
                for (int index = 0; index < NestedExpressions.Count; index++)
                    var exprAst = NestedExpressions[index];
                    action = exprAst.InternalVisit(visitor);
    /// The ast that represents an anonymous script block expression, e.g. <c>{ dir }</c>.
    public class ScriptBlockExpressionAst : ExpressionAst
        /// Construct a script block expression.
        /// <param name="extent">The extent of the script block, from the opening curly brace to the closing curly brace.</param>
        /// <param name="scriptBlock">The script block.</param>
        /// If <paramref name="extent"/> or <paramref name="scriptBlock"/> is null.
        public ScriptBlockExpressionAst(IScriptExtent extent, ScriptBlockAst scriptBlock)
            this.ScriptBlock = scriptBlock;
            SetParent(scriptBlock);
        /// The ast for the scriptblock that this ast represent.  This property is never null.
        public ScriptBlockAst ScriptBlock { get; }
        /// Copy the ScriptBlockExpressionAst instance.
            var newScriptBlock = CopyElement(this.ScriptBlock);
            return new ScriptBlockExpressionAst(this.Extent, newScriptBlock);
        /// The result of a <see cref="ScriptBlockExpressionAst"/> is always <c>typeof(<see cref="ScriptBlock"/></c>).
            get { return typeof(ScriptBlock); }
            return visitor.VisitScriptBlockExpression(this);
            var action = visitor.VisitScriptBlockExpression(this);
                action = ScriptBlock.InternalVisit(visitor);
    /// The ast that represents an array literal expression, e.g. <c>1,2,3</c>.  An array expression, e.g. <c>@(dir)</c>,
    /// is represented by <see cref="ArrayExpressionAst"/>.  An array literal expression can be constructed from a single
    /// element, as happens with the unary comma operator, e.g. <c>,4</c>.
    public class ArrayLiteralAst : ExpressionAst, ISupportsAssignment
        /// Construct an array literal expression.
        /// <param name="extent">The extent of all of the elements.</param>
        /// <param name="elements">The collection of asts that represent the array literal.</param>
        /// If <paramref name="elements"/> is null or is an empty collection.
        public ArrayLiteralAst(IScriptExtent extent, IList<ExpressionAst> elements)
            if (elements == null || elements.Count == 0)
                throw PSTraceSource.NewArgumentException(nameof(elements));
            this.Elements = new ReadOnlyCollection<ExpressionAst>(elements);
            SetParents(Elements);
        /// The non-empty collection of asts of the elements of the array.
        public ReadOnlyCollection<ExpressionAst> Elements { get; }
        /// Copy the ArrayLiteralAst instance.
            var newElements = CopyElements(this.Elements);
            return new ArrayLiteralAst(this.Extent, newElements);
        /// The result of an <see cref="ArrayLiteralAst"/> is always <c>typeof(object[])</c>.
        public override Type StaticType { get { return typeof(object[]); } }
            return visitor.VisitArrayLiteral(this);
            var action = visitor.VisitArrayLiteral(this);
                for (int index = 0; index < Elements.Count; index++)
                    var element = Elements[index];
                    action = element.InternalVisit(visitor);
            return new ArrayAssignableValue { ArrayLiteral = this };
    /// The ast that represents a hash literal, e.g. <c>@{a = 1}</c>.
    public class HashtableAst : ExpressionAst
        private static readonly ReadOnlyCollection<KeyValuePair> s_emptyKeyValuePairs = Utils.EmptyReadOnlyCollection<KeyValuePair>();
        /// Construct a hash literal ast.
        /// <param name="extent">The extent of the literal, from '@{' to the closing '}'.</param>
        /// <param name="keyValuePairs">The optionally null or empty list of key/value pairs.</param>
        public HashtableAst(IScriptExtent extent, IEnumerable<KeyValuePair> keyValuePairs)
            if (keyValuePairs != null)
                this.KeyValuePairs = new ReadOnlyCollection<KeyValuePair>(keyValuePairs.ToArray());
                SetParents(KeyValuePairs);
                this.KeyValuePairs = s_emptyKeyValuePairs;
        /// The pairs of key names and asts for values used to construct the hash table.
        public ReadOnlyCollection<KeyValuePair> KeyValuePairs { get; }
        /// Copy the HashtableAst instance.
            List<KeyValuePair> newKeyValuePairs = null;
            if (this.KeyValuePairs.Count > 0)
                newKeyValuePairs = new List<KeyValuePair>(this.KeyValuePairs.Count);
                for (int i = 0; i < this.KeyValuePairs.Count; i++)
                    var keyValuePair = this.KeyValuePairs[i];
                    var newKey = CopyElement(keyValuePair.Item1);
                    var newValue = CopyElement(keyValuePair.Item2);
                    newKeyValuePairs.Add(Tuple.Create(newKey, newValue));
            return new HashtableAst(this.Extent, newKeyValuePairs);
        /// The result type of a <see cref="HashtableAst"/> is always <c>typeof(<see cref="Hashtable"/>)</c>.
        public override Type StaticType { get { return typeof(Hashtable); } }
        // Indicates that this ast was constructed as part of a schematized object instead of just a plain hash literal.
        internal bool IsSchemaElement { get; set; }
            return visitor.VisitHashtable(this);
            var action = visitor.VisitHashtable(this);
                for (int index = 0; index < KeyValuePairs.Count; index++)
                    var keyValuePairAst = KeyValuePairs[index];
                    action = keyValuePairAst.Item1.InternalVisit(visitor);
                    action = keyValuePairAst.Item2.InternalVisit(visitor);
    /// The ast that represents an array expression, e.g. <c>@(1)</c>.  The array literal (e.g. <c>1,2,3</c>) is
    /// represented by <see cref="ArrayLiteralAst"/>.
    public class ArrayExpressionAst : ExpressionAst
        /// Construct an expression that forces the result to be an array.
        /// <param name="extent">The extent of the expression, including the opening '@(' and closing ')'.</param>
        /// <param name="statementBlock">The statements executed as part of the expression.</param>
        public ArrayExpressionAst(IScriptExtent extent, StatementBlockAst statementBlock)
            this.SubExpression = statementBlock;
            SetParent(statementBlock);
        /// The expression/statements represented by this sub-expression.
        public StatementBlockAst SubExpression { get; }
        /// Copy the ArrayExpressionAst instance.
            var newStatementBlock = CopyElement(this.SubExpression);
            return new ArrayExpressionAst(this.Extent, newStatementBlock);
        /// The result of an ArrayExpressionAst is always <c>typeof(object[])</c>.
            return visitor.VisitArrayExpression(this);
            var action = visitor.VisitArrayExpression(this);
                action = SubExpression.InternalVisit(visitor);
    /// The ast that represents an expression (or pipeline) that is enclosed in parentheses, e.g. <c>(1)</c> or <c>(dir)</c>
    public class ParenExpressionAst : ExpressionAst, ISupportsAssignment
        /// Construct a parenthesized expression.
        /// <param name="extent">The extent of the expression, including the opening and closing parentheses.</param>
        /// <param name="pipeline">The pipeline (or expression) enclosed in parentheses.</param>
        /// If <paramref name="extent"/> or <paramref name="pipeline"/> is null.
        public ParenExpressionAst(IScriptExtent extent, PipelineBaseAst pipeline)
        /// The pipeline (which is frequently but not always an expression) for this parenthesized expression.
        /// Copy the ParenExpressionAst instance.
            return new ParenExpressionAst(this.Extent, newPipeline);
            return visitor.VisitParenExpression(this);
            var action = visitor.VisitParenExpression(this);
            return ((ISupportsAssignment)Pipeline.GetPureExpression()).GetAssignableValue();
    /// The ast that represents a subexpression, e.g. <c>$(1)</c>.
    public class SubExpressionAst : ExpressionAst
        /// Construct a subexpression.
        /// <param name="statementBlock"></param>
        public SubExpressionAst(IScriptExtent extent, StatementBlockAst statementBlock)
        /// The expression/statements represented by this sub-expression.  This property is never null.
        /// Copy the SubExpressionAst instance.
            return new SubExpressionAst(this.Extent, newStatementBlock);
            return visitor.VisitSubExpression(this);
            var action = visitor.VisitSubExpression(this);
    /// The ast that represents a "using" expression, e.g. <c>$using:pshome</c>
    public class UsingExpressionAst : ExpressionAst
        /// Construct a using expression.
        /// <param name="extent">The extent of the using expression.</param>
        /// <param name="expressionAst">The sub-expression of the using expression.</param>
        /// If <paramref name="extent"/> or <paramref name="expressionAst"/> is null.
        public UsingExpressionAst(IScriptExtent extent, ExpressionAst expressionAst)
                throw PSTraceSource.NewArgumentNullException(nameof(expressionAst));
            RuntimeUsingIndex = -1;
            this.SubExpression = expressionAst;
            SetParent(SubExpression);
        /// The expression represented by this using expression.  This property is never null.
        public ExpressionAst SubExpression { get; }
        // Used from code gen to get the value from a well known location.
        internal int RuntimeUsingIndex
        /// Copy the UsingExpressionAst instance.
            var newExpression = CopyElement(this.SubExpression);
            var newUsingExpression = new UsingExpressionAst(this.Extent, newExpression);
            newUsingExpression.RuntimeUsingIndex = this.RuntimeUsingIndex;
            return newUsingExpression;
        #region UsingExpression Utilities
        internal const string UsingPrefix = "__using_";
        /// Get the underlying "using variable" from a UsingExpressionAst.
        /// <param name="usingExpressionAst">
        /// A UsingExpressionAst
        /// The underlying VariableExpressionAst of the UsingExpression
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We want to get the underlying variable only for the UsingExpressionAst.")]
        public static VariableExpressionAst ExtractUsingVariable(UsingExpressionAst usingExpressionAst)
            ArgumentNullException.ThrowIfNull(usingExpressionAst);
            return ExtractUsingVariableImpl(usingExpressionAst);
        /// A UsingExpressionAst must contains a VariableExpressionAst.
        /// <param name="expression"></param>
        private static VariableExpressionAst ExtractUsingVariableImpl(ExpressionAst expression)
            VariableExpressionAst variableExpr;
            if (expression is UsingExpressionAst usingExpr)
                variableExpr = usingExpr.SubExpression as VariableExpressionAst;
                    return variableExpr;
                return ExtractUsingVariableImpl(usingExpr.SubExpression);
            if (expression is IndexExpressionAst indexExpr)
                variableExpr = indexExpr.Target as VariableExpressionAst;
                return ExtractUsingVariableImpl(indexExpr.Target);
            if (expression is MemberExpressionAst memberExpr)
                variableExpr = memberExpr.Expression as VariableExpressionAst;
                return ExtractUsingVariableImpl(memberExpr.Expression);
            Diagnostics.Assert(false, "We should always be able to get a VariableExpressionAst from a UsingExpressionAst");
        #endregion UsingExpression Utilities
            return visitor.VisitUsingExpression(this);
            var action = visitor.VisitUsingExpression(this);
    /// The ast that represents an index expression, e.g. <c>$a[0]</c>.
    public class IndexExpressionAst : ExpressionAst, ISupportsAssignment
        /// Construct an ast for an index expression.
        /// <param name="target">The expression being indexed.</param>
        /// <param name="index">The index expression.</param>
        /// If <paramref name="extent"/>, <paramref name="target"/>, or <paramref name="index"/> is null.
        public IndexExpressionAst(IScriptExtent extent, ExpressionAst target, ExpressionAst index)
            if (target == null || index == null)
                throw PSTraceSource.NewArgumentNullException(target == null ? "target" : "index");
            this.Target = target;
            SetParent(target);
            this.Index = index;
            SetParent(index);
        /// Initializes a new instance of the <see cref="IndexExpressionAst"/> class.
        /// <param name="nullConditional">Access the index only if the target is not null.</param>
        public IndexExpressionAst(IScriptExtent extent, ExpressionAst target, ExpressionAst index, bool nullConditional)
            : this(extent, target, index)
        /// Return the ast for the expression being indexed.  This value is never null.
        public ExpressionAst Target { get; }
        /// Return the ast for the index expression.  This value is never null.
        public ExpressionAst Index { get; }
        /// Gets a value indicating whether ?[] operator is being used.
        public bool NullConditional { get; }
        /// Copy the IndexExpressionAst instance.
            var newTarget = CopyElement(this.Target);
            var newIndex = CopyElement(this.Index);
            return new IndexExpressionAst(this.Extent, newTarget, newIndex, this.NullConditional);
            return visitor.VisitIndexExpression(this);
            var action = visitor.VisitIndexExpression(this);
                action = Target.InternalVisit(visitor);
                action = Index.InternalVisit(visitor);
            return new IndexAssignableValue { IndexExpressionAst = this };
    #region Help
    /// The help content specified via help comments for a given script or script function.
    public sealed class CommentHelpInfo
        /// The help content of the .SYNOPSIS section, if specified, otherwise null.
        public string Synopsis { get; internal set; }
        /// The help content of the .DESCRIPTION section, if specified, otherwise null.
        /// The help content of the .NOTES section, if specified, otherwise null.
        public string Notes { get; internal set; }
        /// The help content for each parameter where help content is specified.  The
        /// key is the parameter name, the value is the help content.
        public IDictionary<string, string> Parameters { get; internal set; }
        /// The help content from all of the specified .LINK sections.
        public ReadOnlyCollection<string> Links { get; internal set; }
        /// The help content from all of the specified .EXAMPLE sections.
        public ReadOnlyCollection<string> Examples { get; internal set; }
        /// The help content from all of the specified .INPUT sections.
        public ReadOnlyCollection<string> Inputs { get; internal set; }
        /// The help content from all of the specified .OUTPUT sections.
        public ReadOnlyCollection<string> Outputs { get; internal set; }
        /// The help content of the .COMPONENT section, if specified, otherwise null.
        public string Component { get; internal set; }
        /// The help content of the .ROLE section, if specified, otherwise null.
        /// The help content of the .FUNCTIONALITY section, if specified, otherwise null.
        public string Functionality { get; internal set; }
        /// The help content of the .FORWARDHELPTARGETNAME section, if specified, otherwise null.
        public string ForwardHelpTargetName { get; internal set; }
        /// The help content of the .FORWARDHELPCATEGORY section, if specified, otherwise null.
        public string ForwardHelpCategory { get; internal set; }
        /// The help content of the .REMOTEHELPRUNSPACE section, if specified, otherwise null.
        public string RemoteHelpRunspace { get; internal set; }
        /// The help content of the .MAMLHELPFILE section, if specified, otherwise null.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Maml")]
        public string MamlHelpFile { get; internal set; }
        /// Returns the help info as a comment block.
        public string GetCommentBlock()
            sb.AppendLine("<#");
            if (!string.IsNullOrEmpty(Synopsis))
                sb.AppendLine(".SYNOPSIS");
                sb.AppendLine(Synopsis);
            if (!string.IsNullOrEmpty(Description))
                sb.AppendLine(".DESCRIPTION");
                sb.AppendLine(Description);
            foreach (var parameter in Parameters)
                sb.Append(".PARAMETER ");
                sb.AppendLine(parameter.Key);
                sb.AppendLine(parameter.Value);
            for (int index = 0; index < Inputs.Count; index++)
                var input = Inputs[index];
                sb.AppendLine(".INPUTS");
                sb.AppendLine(input);
            for (int index = 0; index < Outputs.Count; index++)
                var output = Outputs[index];
                sb.AppendLine(".OUTPUTS");
                sb.AppendLine(output);
            if (!string.IsNullOrEmpty(Notes))
                sb.AppendLine(".NOTES");
                sb.AppendLine(Notes);
            for (int index = 0; index < Examples.Count; index++)
                var example = Examples[index];
                sb.AppendLine(".EXAMPLE");
                sb.AppendLine(example);
            for (int index = 0; index < Links.Count; index++)
                var link = Links[index];
                sb.AppendLine(".LINK");
                sb.AppendLine(link);
            if (!string.IsNullOrEmpty(ForwardHelpTargetName))
                sb.Append(".FORWARDHELPTARGETNAME ");
                sb.AppendLine(ForwardHelpTargetName);
            if (!string.IsNullOrEmpty(ForwardHelpCategory))
                sb.Append(".FORWARDHELPCATEGORY ");
                sb.AppendLine(ForwardHelpCategory);
            if (!string.IsNullOrEmpty(RemoteHelpRunspace))
                sb.Append(".REMOTEHELPRUNSPACE ");
                sb.AppendLine(RemoteHelpRunspace);
            if (!string.IsNullOrEmpty(Component))
                sb.AppendLine(".COMPONENT");
                sb.AppendLine(Component);
            if (!string.IsNullOrEmpty(Role))
                sb.AppendLine(".ROLE");
                sb.AppendLine(Role);
            if (!string.IsNullOrEmpty(Functionality))
                sb.AppendLine(".FUNCTIONALITY");
                sb.AppendLine(Functionality);
            if (!string.IsNullOrEmpty(MamlHelpFile))
                sb.Append(".EXTERNALHELP ");
                sb.AppendLine(MamlHelpFile);
            sb.AppendLine("#>");
    #endregion Help
