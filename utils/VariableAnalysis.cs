    internal static class VariablePathExtensions
        internal static bool IsAnyLocal(this VariablePath variablePath)
            return variablePath.IsUnscopedVariable || variablePath.IsLocal || variablePath.IsPrivate;
    internal class VariableAnalysisDetails
        internal VariableAnalysisDetails()
            this.AssociatedAsts = new List<Ast>();
        public int BitIndex { get; set; }
        public int LocalTupleIndex { get; set; }
        public Type Type { get; set; }
        public bool Automatic { get; set; }
        public bool PreferenceVariable { get; set; }
        public bool Assigned { get; set; }
        public List<Ast> AssociatedAsts { get; }
    internal sealed class FindAllVariablesVisitor : AstVisitor
        private static readonly HashSet<string> s_hashOfPessimizingCmdlets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly string[] s_pessimizingCmdlets = new string[]
                                                              "Remove-Variable",
                                                              "Set-PSBreakpoint",
                                                              "Microsoft.PowerShell.Utility\\New-Variable",
                                                              "Microsoft.PowerShell.Utility\\Remove-Variable",
                                                              "Microsoft.PowerShell.Utility\\Set-Variable",
                                                              "Microsoft.PowerShell.Utility\\Set-PSBreakpoint",
                                                              "rv",
                                                              "sbp",
                                                              "sv",
        static FindAllVariablesVisitor()
            foreach (var cmdlet in s_pessimizingCmdlets)
                s_hashOfPessimizingCmdlets.Add(cmdlet);
        internal static Dictionary<string, VariableAnalysisDetails> Visit(TrapStatementAst trap)
            // We disable optimizations for trap because it simplifies what we need to do when invoking
            // the trap, and it's assumed that the code inside a trap rarely, if ever, actually creates
            // any local variables.
            var visitor = new FindAllVariablesVisitor(disableOptimizations: true, scriptCmdlet: false);
            trap.Body.InternalVisit(visitor);
            return visitor._variables;
        // Used to analyze an expression that is invoked separately, i.e. a default argument.
        internal static Dictionary<string, VariableAnalysisDetails> Visit(ExpressionAst exprAst)
            // We disable optimizations for default expressions because it simplifies what we need to do when
            // invoking the default expression, and it's assumed that the code inside a trap rarely, if ever,
            // actually creates any local variables.
            exprAst.InternalVisit(visitor);
        internal static Dictionary<string, VariableAnalysisDetails> Visit(IParameterMetadataProvider ast,
                                                                          bool disableOptimizations,
                                                                          bool scriptCmdlet,
                                                                          out int localsAllocated,
                                                                          out bool forceNoOptimizing)
            var visitor = new FindAllVariablesVisitor(disableOptimizations, scriptCmdlet);
            // Visit the body before the parameters so we don't allocate any tuple slots for parameters
            // if we won't be optimizing because of a call to new-variable/remove-variable, etc.
            ast.Body.InternalVisit(visitor);
            forceNoOptimizing = visitor._disableOptimizations;
            if (ast.Parameters != null)
                visitor.VisitParameters(ast.Parameters);
            localsAllocated = visitor._variables.Count(static details => details.Value.LocalTupleIndex != VariableAnalysis.Unanalyzed);
        private bool _disableOptimizations;
        private readonly Dictionary<string, VariableAnalysisDetails> _variables
            = new Dictionary<string, VariableAnalysisDetails>(StringComparer.OrdinalIgnoreCase);
        private FindAllVariablesVisitor(bool disableOptimizations, bool scriptCmdlet)
            _disableOptimizations = disableOptimizations;
            var automaticVariables = SpecialVariables.AutomaticVariables;
            Diagnostics.Assert(Array.IndexOf(automaticVariables, SpecialVariables.Underbar) == (int)AutomaticVariable.Underbar,
                               "automaticVariables order is incorrect (0)");
            Diagnostics.Assert(Array.IndexOf(automaticVariables, SpecialVariables.Args) == (int)AutomaticVariable.Args,
                               "automaticVariables order is incorrect (1)");
            Diagnostics.Assert(Array.IndexOf(automaticVariables, SpecialVariables.This) == (int)AutomaticVariable.This,
                               "automaticVariables order is incorrect (2)");
            Diagnostics.Assert(Array.IndexOf(automaticVariables, SpecialVariables.Input) == (int)AutomaticVariable.Input,
                               "automaticVariables order is incorrect (3)");
            Diagnostics.Assert(Array.IndexOf(automaticVariables, SpecialVariables.PSCmdlet) == (int)AutomaticVariable.PSCmdlet,
                               "automaticVariables order is incorrect (4)");
            Diagnostics.Assert(Array.IndexOf(automaticVariables, SpecialVariables.PSBoundParameters) == (int)AutomaticVariable.PSBoundParameters,
                               "automaticVariables order is incorrect (5)");
            Diagnostics.Assert(Array.IndexOf(automaticVariables, SpecialVariables.MyInvocation) == (int)AutomaticVariable.MyInvocation,
                               "automaticVariables order is incorrect (6)");
            Diagnostics.Assert(Array.IndexOf(automaticVariables, SpecialVariables.PSScriptRoot) == (int)AutomaticVariable.PSScriptRoot,
                               "automaticVariables order is incorrect (7)");
            Diagnostics.Assert(Array.IndexOf(automaticVariables, SpecialVariables.PSCommandPath) == (int)AutomaticVariable.PSCommandPath,
                               "automaticVariables order is incorrect (8)");
            for (i = 0; i < automaticVariables.Length; ++i)
                NoteVariable(automaticVariables[i], i, SpecialVariables.AutomaticVariableTypes[i], automatic: true);
            if (scriptCmdlet)
                var preferenceVariables = SpecialVariables.PreferenceVariables;
                for (i = 0; i < preferenceVariables.Length; ++i)
                    NoteVariable(preferenceVariables[i], i + (int)AutomaticVariable.NumberOfAutomaticVariables,
                        SpecialVariables.PreferenceVariableTypes[i], preferenceVariable: true);
            NoteVariable(SpecialVariables.Question, VariableAnalysis.Unanalyzed, typeof(bool), automatic: true);
        private void VisitParameters(ReadOnlyCollection<ParameterAst> parameters)
            foreach (ParameterAst t in parameters)
                var variableExpressionAst = t.Name;
                if (varPath.IsAnyLocal())
                    var variableName = VariableAnalysis.GetUnaliasedVariableName(varPath);
                    VariableAnalysisDetails analysisDetails;
                    if (_variables.TryGetValue(variableName, out analysisDetails))
                        // Forget whatever type we deduced in the body, we'll revisit that type after walking
                        // the flow graph.  We should see the parameter type for the variable first.
                        analysisDetails.Type = t.StaticType;
                        // If the parameter has no default value, we can't allocate a strongly typed
                        // slot in the tuple.  This only matters for value types where we allow
                        // comparisons with $null and don't try to convert the $null value to the
                        // valuetype because the parameter has no value yet.  For example:
                        //     & { param([System.Reflection.MemberTypes]$m) ($null -eq $m) }
                        if (!Compiler.TryGetDefaultParameterValue(analysisDetails.Type, out _))
                            analysisDetails.LocalTupleIndex = VariableAnalysis.ForceDynamic;
                        NoteVariable(variableName, VariableAnalysis.Unanalyzed, t.StaticType);
        // Add a variable to the variable dictionary
        private void NoteVariable(string variableName, int index, Type type, bool automatic = false, bool preferenceVariable = false)
            if (!_variables.ContainsKey(variableName))
                var details = new VariableAnalysisDetails
                    BitIndex = _variables.Count,
                    LocalTupleIndex = index,
                    Name = variableName,
                    Type = type,
                    Automatic = automatic,
                    PreferenceVariable = preferenceVariable,
                    Assigned = false,
                _variables.Add(variableName, details);
                NoteVariable(dataStatementAst.Variable, VariableAnalysis.Unanalyzed, null);
            NoteVariable(SpecialVariables.@switch, VariableAnalysis.Unanalyzed, typeof(IEnumerator));
            NoteVariable(SpecialVariables.@foreach, VariableAnalysis.Unanalyzed, typeof(IEnumerator));
                if (varPath.IsPrivate)
                    // TODO: force just this variable to be dynamic, not all variables.
                    _disableOptimizations = true;
                NoteVariable(VariableAnalysis.GetUnaliasedVariableName(varPath), VariableAnalysis.Unanalyzed, null);
        private int _runtimeUsingIndex;
            // On the local machine, we may have set the index because of a call to ScriptBlockToPowerShell or Invoke-Command.
            // On the remote machine, the index probably isn't set yet, so we set it here, mostly to avoid another pass
            // over the ast.  We assert below to ensure we're setting to the same value in both the local and remote cases.
            if (usingExpressionAst.RuntimeUsingIndex == -1)
                usingExpressionAst.RuntimeUsingIndex = _runtimeUsingIndex;
            Diagnostics.Assert(usingExpressionAst.RuntimeUsingIndex == _runtimeUsingIndex, "Logic error in visiting using expressions.");
            _runtimeUsingIndex += 1;
            var commandName = commandAst.CommandElements[0] as StringConstantExpressionAst;
            if (commandName != null && s_hashOfPessimizingCmdlets.Contains(commandName.Value))
                // TODO: psuedo-bind the command invocation to figure out the variable and only force that variable to be unoptimized
                // For code like:
                //     & { . { [string]$x = "abc" }; $x = 42; $x.GetType() }
                // We expect $x to be of type string, not int.
                // If we optimize, we'll end up throwing an error because the variable created in dotting is not consistent with the first
                // assignment to $x in the outer scope.  To support this scenario, we'll disable optimizing when dotting.
                // This is not a complete fix - some cmdlets (like foreach-object) dot scripts.  We don't want to disable optimizations
                // unnecessarily (foreach-object is used heavily).  This issue rarely comes up in foreach-object, so we'll live with the
                // errors. (See VariableNotWritableRare for the errors that happen when this issue arises.)
            // We don't want to discover any variables in nested functions - they get their own scope.
            // We don't want to discover any variables in script block expressions - they get their own scope.
            // We don't want to discover any variables in traps - they get their own scope.
    internal class VariableAnalysis : ICustomAstVisitor2
        // Tuple slots start at index 0.  >= 0 means a variable is allocated in the tuple.  -1 means we haven't
        // analyzed a specific use of a variable and don't know what slot it might be assigned to yet.
        internal const int Unanalyzed = -1;
        // In some cases, we want to force a variable to not be allocated in the tuple, but instead use the variable
        // table along with a PSVariable instance.  For example,
        //    1. if a variable's type might change in the same scope;
        //    2. if there might be any validation attributes or more than one argument conversion
        //    3. if there is one argument conversion but the conversion type cannot be resolved at compile time (it's
        //       possible that the assembly containing the type would be loaded during runtime)
        // in these cases, we rely on the setter PSVariable.Value to handle those attributes.
        internal const int ForceDynamic = -2;
            internal LoopGotoTargets(string label, Block breakTarget, Block continueTarget)
                this.BreakTarget = breakTarget;
                this.ContinueTarget = continueTarget;
            internal Block BreakTarget { get; }
            internal Block ContinueTarget { get; }
        private sealed class Block
            internal readonly List<Ast> _asts = new List<Ast>();
            private readonly List<Block> _successors = new List<Block>();
            internal readonly List<Block> _predecessors = new List<Block>();
            internal object _visitData;
            internal bool _throws;
            internal bool _returns;
            internal bool _unreachable { get; private set; }
            // Only Entry block, that can be constructed via NewEntryBlock() is reachable initially.
            // all other blocks are unreachable.
            // reachability of block should be proved with FlowsTo() calls.
            public Block()
                this._unreachable = true;
            public static Block NewEntryBlock()
                return new Block(unreachable: false);
            private Block(bool unreachable)
                this._unreachable = unreachable;
            /// Tell flow analysis that this block can flow to next block.
            /// <param name="next"></param>
            internal void FlowsTo(Block next)
                if (_successors.IndexOf(next) < 0)
                    if (!_unreachable)
                        next._unreachable = false;
                    _successors.Add(next);
                    next._predecessors.Add(this);
            internal void AddAst(Ast ast)
                Diagnostics.Assert(ast is VariableExpressionAst || ast is AssignmentTarget || ast is DataStatementAst,
                    "Only add variables and assignments");
                _asts.Add(ast);
            internal static List<Block> GenerateReverseDepthFirstOrder(Block block)
                List<Block> result = new List<Block>();
                VisitDepthFirstOrder(block, result);
                result.Reverse();
                    result[i]._visitData = null;
            private static void VisitDepthFirstOrder(Block block, List<Block> visitData)
                if (ReferenceEquals(block._visitData, visitData))
                block._visitData = visitData;
                foreach (Block succ in block._successors)
                    VisitDepthFirstOrder(succ, visitData);
                visitData.Add(block);
        private sealed class AssignmentTarget : Ast
            internal readonly ExpressionAst _targetAst;
            internal readonly string _variableName;
            internal readonly Type _type;
            public AssignmentTarget(ExpressionAst targetExpressionAst)
                : base(PositionUtilities.EmptyExtent)
                this._targetAst = targetExpressionAst;
            public AssignmentTarget(string variableName, Type type)
                this._variableName = variableName;
                this._type = type;
            public override Ast Copy()
                Diagnostics.Assert(false, "This code is unreachable.");
            internal override object Accept(ICustomAstVisitor visitor)
            internal override AstVisitAction InternalVisit(AstVisitor visitor)
        internal static string GetUnaliasedVariableName(string varName)
            return varName.Equals(SpecialVariables.PSItem, StringComparison.OrdinalIgnoreCase)
                       ? SpecialVariables.Underbar
                       : varName;
        internal static string GetUnaliasedVariableName(VariablePath varPath)
            return GetUnaliasedVariableName(varPath.UnqualifiedPath);
        // At compile time, we know specific variables are allscope and we can't optimize assignments.  This hashset must remain
        // constant though - it only contains variable names known to _always_ be allscope.  For other names, we need a special
        // check before choosing to run the optimized code or unoptimized (dotted) version which will correctly handle allscope
        // assignments.
        private static readonly ConcurrentDictionary<string, bool> s_allScopeVariables = new ConcurrentDictionary<string, bool>(1, 16, StringComparer.OrdinalIgnoreCase);
        internal static void NoteAllScopeVariable(string variableName)
            s_allScopeVariables.GetOrAdd(variableName, true);
        internal static bool AnyVariablesCouldBeAllScope(Dictionary<string, int> variableNames)
            return variableNames.Any(static keyValuePair => s_allScopeVariables.ContainsKey(keyValuePair.Key));
        private Dictionary<string, VariableAnalysisDetails> _variables;
        private Block _entryBlock;
        private Block _exitBlock;
        private Block _currentBlock;
        private int _localsAllocated;
        internal static Tuple<Type, Dictionary<string, int>> AnalyzeExpression(ExpressionAst exprAst)
            return (new VariableAnalysis()).AnalyzeImpl(exprAst);
        private Tuple<Type, Dictionary<string, int>> AnalyzeImpl(ExpressionAst exprAst)
            _variables = FindAllVariablesVisitor.Visit(exprAst);
            // We disable optimizations for expression because it simplifies what we need to do when invoking
            // the default argument, and it's assumed that the code inside a default argument rarely, if ever, actually creates
            _localsAllocated = SpecialVariables.AutomaticVariables.Length;
            _currentBlock = _entryBlock;
            exprAst.Accept(this);
            _currentBlock.FlowsTo(_exitBlock);
            return FinishAnalysis();
        internal static Tuple<Type, Dictionary<string, int>> AnalyzeTrap(TrapStatementAst trap)
            return (new VariableAnalysis()).AnalyzeImpl(trap);
        private Tuple<Type, Dictionary<string, int>> AnalyzeImpl(TrapStatementAst trap)
            _variables = FindAllVariablesVisitor.Visit(trap);
            trap.Body.Accept(this);
            _entryBlock = Block.NewEntryBlock();
            _exitBlock = new Block();
        internal static Tuple<Type, Dictionary<string, int>> Analyze(IParameterMetadataProvider ast, bool disableOptimizations, bool scriptCmdlet)
            return (new VariableAnalysis()).AnalyzeImpl(ast, disableOptimizations, scriptCmdlet);
        /// Analyze a member function, marking variable references as "dynamic" (so they can be reported as errors)
        /// and also analyze the control flow to make sure every block returns (or throws)
        internal static bool AnalyzeMemberFunction(FunctionMemberAst ast)
            VariableAnalysis va = (new VariableAnalysis());
            va.AnalyzeImpl(ast, false, false);
            return va._exitBlock._predecessors.All(static b => b._returns || b._throws || b._unreachable);
        private Tuple<Type, Dictionary<string, int>> AnalyzeImpl(IParameterMetadataProvider ast, bool disableOptimizations, bool scriptCmdlet)
            _variables = FindAllVariablesVisitor.Visit(ast, disableOptimizations, scriptCmdlet, out _localsAllocated, out _disableOptimizations);
                foreach (var parameter in ast.Parameters)
                    var variablePath = parameter.Name.VariablePath;
                    if (variablePath.IsAnyLocal())
                        bool anyAttributes = false;
                        int countConverts = -1; // First convert is really the parameter type, so it doesn't count
                        bool anyUnresolvedTypes = false;
                        foreach (var paramAst in parameter.Attributes)
                            if (paramAst is TypeConstraintAst)
                                countConverts += 1;
                                    type = paramAst.TypeName.GetReflectionType();
                                        anyUnresolvedTypes = true;
                                var attrType = paramAst.TypeName.GetReflectionAttributeType();
                                if (attrType == null)
                                else if (typeof(ValidateArgumentsAttribute).IsAssignableFrom(attrType)
                                    || typeof(ArgumentTransformationAttribute).IsAssignableFrom(attrType))
                                    // If there are any attributes that have semantic meaning, we need to use a PSVariable.
                                    anyAttributes = true;
                        var varName = GetUnaliasedVariableName(variablePath);
                        var details = _variables[varName];
                        details.Assigned = true;
                        type ??= details.Type ?? typeof(object);
                        // automatic and preference variables are pre-allocated, so they can't be unallocated
                        // and forced to be dynamic.
                        // unresolved types can happen at parse time
                        // [ref] parameters are forced to dynamic so that we can assign $null in the parameter
                        // binder w/o conversions kicking in (the setter in MutableTuple will convert $null to PSReference<Null>
                        // but that won't happen if we create a PSVariable (this is an ugly hack, but it works.)
                        if ((anyAttributes || anyUnresolvedTypes || countConverts > 0 || typeof(PSReference).IsAssignableFrom(type) || MustBeBoxed(type)) &&
                            !details.Automatic && !details.PreferenceVariable)
                            details.LocalTupleIndex = ForceDynamic;
                        _entryBlock.AddAst(new AssignmentTarget(varName, type));
            ast.Body.Accept(this);
            return FinishAnalysis(scriptCmdlet);
        private Tuple<Type, Dictionary<string, int>> FinishAnalysis(bool scriptCmdlet = false)
            var blocks = Block.GenerateReverseDepthFirstOrder(_entryBlock);
            // The first block has no predecessors, so analyze outside the loop to "prime" the bitarray.
            var bitArray = new BitArray(_variables.Count);
            blocks[0]._visitData = bitArray;
            AnalyzeBlock(bitArray, blocks[0]);
            for (int index = 1; index < blocks.Count; index++)
                var block = blocks[index];
                bitArray = new BitArray(_variables.Count);
                bitArray.SetAll(true);
                block._visitData = bitArray;
                int predCount = 0;
                foreach (var pred in block._predecessors)
                    // VisitData can be null when the pred occurs because of a continue statement.
                    if (pred._visitData != null)
                        predCount += 1;
                        bitArray.And((BitArray)pred._visitData);
                Diagnostics.Assert(predCount != 0, "If we didn't and anything, there is a flaw in the logic and incorrect code may be generated.");
                AnalyzeBlock(bitArray, block);
            Diagnostics.Assert(_exitBlock._predecessors.All(p => p._unreachable || p._visitData is BitArray), "VisitData wasn't set on a reachable block");
            foreach (var details in _variables.Values)
                if (details.LocalTupleIndex == ForceDynamic)
                    foreach (var ast in details.AssociatedAsts)
                        FixTupleIndex(ast, ForceDynamic);
                        FixAssigned(ast, details);
            // Automatic variables from 'SpecialVariables.AutomaticVariables' usually are pre-allocated,
            // but there could be situations where some of them are forced to be dynamic. We need to count
            // them in when creating tuple slots in such cases to make sure we create enough slots.
            // However, $? is not a real automatic variable from 'SpecialVariables.AutomaticVariables'
            // even though it's marked as so, and thus we need to exclude it.
            var orderedLocals = (from details in _variables.Values
                                 where (details.LocalTupleIndex >= 0 || (details.LocalTupleIndex == ForceDynamic &&
                                                                         details.Automatic &&
                                                                         details.Name != SpecialVariables.Question))
                                 orderby details.LocalTupleIndex
                                 select details).ToArray();
            Diagnostics.Assert(!_disableOptimizations
                || orderedLocals.Length == (int)AutomaticVariable.NumberOfAutomaticVariables +
                        (scriptCmdlet ? SpecialVariables.PreferenceVariables.Length : 0),
                "analysis is incorrectly allocating number of locals when optimizations are disabled.");
            var nameToIndexMap = new Dictionary<string, int>(0, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < orderedLocals.Length; ++i)
                var details = orderedLocals[i];
                var name = details.Name;
                nameToIndexMap.Add(name, i);
                if (details.LocalTupleIndex != i)
                        FixTupleIndex(ast, i);
                // Automatic variables assign the type directly, not relying on any analysis.  For
                // all other variables, we don't determine the type of the local until we're done
                // with the analysis.
                Diagnostics.Assert(details.Type != null, "Type should be resolved already");
            var tupleType = MutableTuple.MakeTupleType((from l in orderedLocals select l.Type).ToArray());
            return Tuple.Create(tupleType, nameToIndexMap);
        private static bool MustBeBoxed(Type type)
            // We need to box mutable value types so that member operations like
            //     $value.Property = 42
            // We make sure we never allocate an instance of such mutable types in the MutableType.
            return (type.IsValueType && PSVariableAssignmentBinder.IsValueTypeMutable(type)) && typeof(SwitchParameter) != type;
        private static void FixTupleIndex(Ast ast, int newIndex)
            var variableAst = ast as VariableExpressionAst;
                if (variableAst.TupleIndex != ForceDynamic)
                    variableAst.TupleIndex = newIndex;
                var dataStatementAst = ast as DataStatementAst;
                if (dataStatementAst != null)
                    if (dataStatementAst.TupleIndex != ForceDynamic)
                        dataStatementAst.TupleIndex = newIndex;
        private static void FixAssigned(Ast ast, VariableAnalysisDetails details)
            if (variableAst != null && details.Assigned)
                variableAst.Assigned = true;
        private void AnalyzeBlock(BitArray assignedBitArray, Block block)
            foreach (var ast in block._asts)
                var variableExpressionAst = ast as VariableExpressionAst;
                if (variableExpressionAst != null)
                        var varName = GetUnaliasedVariableName(varPath);
                        if (details.Automatic)
                            variableExpressionAst.TupleIndex = details.LocalTupleIndex;
                            variableExpressionAst.Automatic = true;
                            variableExpressionAst.TupleIndex = assignedBitArray[details.BitIndex] && !details.PreferenceVariable
                                                                   ? details.LocalTupleIndex
                                                                   : VariableAnalysis.ForceDynamic;
                var assignmentTarget = ast as AssignmentTarget;
                if (assignmentTarget != null)
                    if (assignmentTarget._targetAst != null)
                        CheckLHSAssign(assignmentTarget._targetAst, assignedBitArray);
                        CheckLHSAssignVar(assignmentTarget._variableName, assignedBitArray, assignmentTarget._type);
                    var details = CheckLHSAssignVar(dataStatementAst.Variable, assignedBitArray, typeof(object));
                    dataStatementAst.TupleIndex = details.LocalTupleIndex;
                    details.AssociatedAsts.Add(dataStatementAst);
                Diagnostics.Assert(false, "Unexpected type in list of ASTs");
        private void CheckLHSAssign(ExpressionAst lhs, BitArray assignedBitArray)
            var convertExpr = lhs as ConvertExpressionAst;
            Type convertType = null;
                lhs = convertExpr.Child;
                convertType = convertExpr.StaticType;
            var varExpr = lhs as VariableExpressionAst;
            Diagnostics.Assert(varExpr != null, "unexpected ast type on lhs");
            var varPath = varExpr.VariablePath;
                if (convertType == null &&
                    (varName.Equals(SpecialVariables.@foreach, StringComparison.OrdinalIgnoreCase) ||
                     varName.Equals(SpecialVariables.@switch, StringComparison.OrdinalIgnoreCase)))
                    // $switch/$foreach are normally typed as IEnumerator, but if the values are directly
                    // assigned (as opposed to implicitly assigned which goes directly to CheckLHSAssignVar),
                    // then force the type to object.
                    convertType = typeof(object);
                VariableAnalysisDetails analysisDetails = CheckLHSAssignVar(varName, assignedBitArray, convertType);
                analysisDetails.AssociatedAsts.Add(varExpr);
                analysisDetails.Assigned = true;
                varExpr.TupleIndex = analysisDetails.LocalTupleIndex;
                varExpr.Automatic = analysisDetails.Automatic;
                varExpr.TupleIndex = VariableAnalysis.ForceDynamic;
        private VariableAnalysisDetails CheckLHSAssignVar(string variableName, BitArray assignedBitArray, Type convertType)
            var analysisDetails = _variables[variableName];
            if (analysisDetails.LocalTupleIndex == VariableAnalysis.Unanalyzed)
                analysisDetails.LocalTupleIndex = _disableOptimizations || s_allScopeVariables.ContainsKey(variableName)
                                                      ? VariableAnalysis.ForceDynamic
                                                      : _localsAllocated++;
            if (convertType != null && MustBeBoxed(convertType))
            var type = analysisDetails.Type;
                analysisDetails.Type = convertType ?? typeof(object);
                if (!assignedBitArray[analysisDetails.BitIndex] && convertType == null)
                    // The variable has not been assigned in the current flow control path, but has been on some other
                    // path (because the type was already assigned.)  Make sure they are compatible by forcing a type comparison.
                if (convertType != null && !convertType.Equals(type))
                    if (analysisDetails.Automatic || analysisDetails.PreferenceVariable)
                        // Can't be dynamic, but we were optimistic that we could strongly type the automatic
                        // and it turns out we can't.
                        analysisDetails.Type = typeof(object);
                        analysisDetails.LocalTupleIndex = ForceDynamic;
            assignedBitArray.Set(analysisDetails.BitIndex, true);
            return analysisDetails;
            scriptBlockAst.DynamicParamBlock?.Accept(this);
            scriptBlockAst.BeginBlock?.Accept(this);
            scriptBlockAst.ProcessBlock?.Accept(this);
            scriptBlockAst.EndBlock?.Accept(this);
            scriptBlockAst.CleanBlock?.Accept(this);
            // Don't visit traps - they get their own scope
            return VisitStatementBlock(namedBlockAst.Statements);
            Diagnostics.Assert(false, "Code is unreachable");
            // Nothing to do now, we've already allocated parameters in the first pass looking for all variable naems.
            // Don't recurse into the function definition, it's variables are distinct from the script block
            // we're currently analyzing.
            return VisitStatementBlock(statementBlockAst.Statements);
        private object VisitStatementBlock(ReadOnlyCollection<StatementAst> statements)
            foreach (var stmt in statements)
                stmt.Accept(this);
            Block afterStmt = new Block();
            if (ifStmtAst.ElseClause == null)
                // There is no else, flow can go straight to afterStmt.
                _currentBlock.FlowsTo(afterStmt);
                var clause = ifStmtAst.Clauses[i];
                bool isLastClause = (i == (clauseCount - 1) && ifStmtAst.ElseClause == null);
                Block clauseBlock = new Block();
                Block nextBlock = isLastClause ? afterStmt : new Block();
                clause.Item1.Accept(this);
                _currentBlock.FlowsTo(clauseBlock);
                _currentBlock.FlowsTo(nextBlock);
                _currentBlock = clauseBlock;
                clause.Item2.Accept(this);
                _currentBlock = nextBlock;
                ifStmtAst.ElseClause.Accept(this);
            _currentBlock = afterStmt;
            var ifTrue = new Block();
            var ifFalse = new Block();
            var after = new Block();
            ternaryExpressionAst.Condition.Accept(this);
            _currentBlock.FlowsTo(ifTrue);
            _currentBlock.FlowsTo(ifFalse);
            _currentBlock = ifTrue;
            ternaryExpressionAst.IfTrue.Accept(this);
            _currentBlock.FlowsTo(after);
            _currentBlock = ifFalse;
            ternaryExpressionAst.IfFalse.Accept(this);
            _currentBlock = after;
            trapStatementAst.Body.Accept(this);
            var details = _variables[SpecialVariables.@switch];
            if (details.LocalTupleIndex == VariableAnalysis.Unanalyzed && !_disableOptimizations)
                details.LocalTupleIndex = _localsAllocated++;
            Action generateCondition = () =>
                switchStatementAst.Condition.Accept(this);
                // $switch is set after evaluating the condition.
                _currentBlock.AddAst(new AssignmentTarget(SpecialVariables.@switch, typeof(IEnumerator)));
            Action switchBodyGenerator = () =>
                bool hasDefault = (switchStatementAst.Default != null);
                    bool isLastClause = (i == (clauseCount - 1) && !hasDefault);
                    if (!isLastClause)
                if (hasDefault)
                    // If any clause was executed, we skip the default, so there is always a branch over the default.
                    switchStatementAst.Default.Accept(this);
            GenerateWhileLoop(switchStatementAst.Label, generateCondition, switchBodyGenerator);
            dataStatementAst.Body.Accept(this);
                _currentBlock.AddAst(dataStatementAst);
        private void GenerateWhileLoop(string loopLabel,
                                       Action generateCondition,
                                       Action generateLoopBody,
                                       Ast continueAction = null)
            // We model the flow graph like this (if continueAction is null, the first part is slightly different):
            //    goto L
            //        continueAction
            //    :L
            //        loop body
            //        // break -> goto BreakTarget
            //        // continue -> goto ContinueTarget
            //        goto ContinueTarget
            var continueBlock = new Block();
            if (continueAction != null)
                var blockAfterContinue = new Block();
                // Represent the goto over the condition before the first iteration.
                _currentBlock.FlowsTo(blockAfterContinue);
                _currentBlock = continueBlock;
                continueAction.Accept(this);
                _currentBlock = blockAfterContinue;
                _currentBlock.FlowsTo(continueBlock);
            var bodyBlock = new Block();
            var breakBlock = new Block();
            // Condition can be null from an uncommon for loop: for() {}
                generateCondition();
                _currentBlock.FlowsTo(breakBlock);
            _loopTargets.Add(new LoopGotoTargets(loopLabel ?? string.Empty, breakBlock, continueBlock));
            _currentBlock.FlowsTo(bodyBlock);
            _currentBlock = bodyBlock;
            generateLoopBody();
            _currentBlock = breakBlock;
        private void GenerateDoLoop(LoopStatementAst loopStatement)
            // We model the flow graph like this:
            //       // continue -> goto ContinueTarget
            var gotoRepeatTargetBlock = new Block();
            _loopTargets.Add(new LoopGotoTargets(loopStatement.Label ?? string.Empty, breakBlock, continueBlock));
            loopStatement.Body.Accept(this);
            loopStatement.Condition.Accept(this);
            _currentBlock.FlowsTo(gotoRepeatTargetBlock);
            _currentBlock = gotoRepeatTargetBlock;
            var foreachDetails = _variables[SpecialVariables.@foreach];
            if (foreachDetails.LocalTupleIndex == VariableAnalysis.Unanalyzed && !_disableOptimizations)
                foreachDetails.LocalTupleIndex = _localsAllocated++;
            var afterFor = new Block();
                forEachStatementAst.Condition.Accept(this);
                // The loop might not be executed, so add flow around the loop.
                _currentBlock.FlowsTo(afterFor);
                // $foreach and the iterator variable are set after evaluating the condition.
                _currentBlock.AddAst(new AssignmentTarget(SpecialVariables.@foreach, typeof(IEnumerator)));
                _currentBlock.AddAst(new AssignmentTarget(forEachStatementAst.Variable));
            GenerateWhileLoop(forEachStatementAst.Label, generateCondition, () => forEachStatementAst.Body.Accept(this));
            _currentBlock = afterFor;
            GenerateDoLoop(doWhileStatementAst);
            GenerateDoLoop(doUntilStatementAst);
            forStatementAst.Initializer?.Accept(this);
            var generateCondition = forStatementAst.Condition != null
                ? () => forStatementAst.Condition.Accept(this)
                : (Action)null;
            GenerateWhileLoop(forStatementAst.Label, generateCondition, () => forStatementAst.Body.Accept(this),
            GenerateWhileLoop(whileStatementAst.Label,
                              () => whileStatementAst.Condition.Accept(this),
                              () => whileStatementAst.Body.Accept(this));
            catchClauseAst.Body.Accept(this);
            // We don't attempt to accurately model flow in a try catch because every statement
            // can flow to each catch.  Instead, we'll assume the try block is not executed (because the very first statement
            // may throw), and have the data flow assume the block before the try is all that can reach the catches and finally.
            var blockBeforeTry = _currentBlock;
            _currentBlock = new Block();
            blockBeforeTry.FlowsTo(_currentBlock);
            tryStatementAst.Body.Accept(this);
            Block lastBlockInTry = _currentBlock;
            var finallyFirstBlock = tryStatementAst.Finally == null ? null : new Block();
            Block finallyLastBlock = null;
            // This is the first block after all the catches and finally (if present).
            var afterTry = new Block();
            bool isCatchAllPresent = false;
            foreach (var catchAst in tryStatementAst.CatchClauses)
                if (catchAst.IsCatchAll)
                    isCatchAllPresent = true;
                // Any statement in the try block could throw and reach the catch, so assume the worst (from a data
                // flow perspective) and make the predecessor to the catch the block before entering the try.
                catchAst.Accept(this);
                _currentBlock.FlowsTo(finallyFirstBlock ?? afterTry);
            if (finallyFirstBlock != null)
                lastBlockInTry.FlowsTo(finallyFirstBlock);
                _currentBlock = finallyFirstBlock;
                tryStatementAst.Finally.Accept(this);
                _currentBlock.FlowsTo(afterTry);
                finallyLastBlock = _currentBlock;
                // For finally block, there are 2 cases: when try-body throw and when it doesn't.
                // For these two cases value of 'finallyLastBlock._throws' would be different.
                if (!isCatchAllPresent)
                    // This flow exist only, if there is no catch for all exceptions.
                    blockBeforeTry.FlowsTo(finallyFirstBlock);
                    var rethrowAfterFinallyBlock = new Block();
                    finallyLastBlock.FlowsTo(rethrowAfterFinallyBlock);
                    rethrowAfterFinallyBlock._throws = true;
                    rethrowAfterFinallyBlock.FlowsTo(_exitBlock);
                // This flow always exists.
                finallyLastBlock.FlowsTo(afterTry);
                lastBlockInTry.FlowsTo(afterTry);
            _currentBlock = afterTry;
        private void BreakOrContinue(ExpressionAst label, Func<LoopGotoTargets, Block> fieldSelector)
            Block targetBlock = null;
                label.Accept(this);
                        targetBlock = (from t in _loopTargets
                targetBlock = fieldSelector(_loopTargets.Last());
            if (targetBlock == null)
                // We need to report an error about bad break statement here
                _currentBlock._throws = true;
                _currentBlock.FlowsTo(targetBlock);
            // The next block is unreachable, but is necessary to keep the flow graph correct.
            BreakOrContinue(breakStatementAst.Label, static t => t.BreakTarget);
            BreakOrContinue(continueStatementAst.Label, static t => t.ContinueTarget);
        private Block ControlFlowStatement(PipelineBaseAst pipelineAst)
            pipelineAst?.Accept(this);
            var lastBlockInStatement = _currentBlock;
            return lastBlockInStatement;
            ControlFlowStatement(returnStatementAst.Pipeline)._returns = true;
            ControlFlowStatement(exitStatementAst.Pipeline)._throws = true;
            // Even if we are in try-catch, we still can safely assume that flow can go to exit from here.
            // Additional exit point would not affect correctness of analysis: we handle throwing of any statement inside try-body in VisitTryStatement.
            ControlFlowStatement(throwStatementAst.Pipeline)._throws = true;
        private static IEnumerable<ExpressionAst> GetAssignmentTargets(ExpressionAst expressionAst)
            var parenExpr = expressionAst as ParenExpressionAst;
                foreach (var e in GetAssignmentTargets(parenExpr.Pipeline.GetPureExpression()))
                    yield return e;
                var arrayLiteral = expressionAst as ArrayLiteralAst;
                    foreach (var e in arrayLiteral.Elements.SelectMany(GetAssignmentTargets))
                    yield return expressionAst;
            assignmentStatementAst.Right.Accept(this);
            foreach (var assignTarget in GetAssignmentTargets(assignmentStatementAst.Left))
                int convertCount = 0;
                ConvertExpressionAst convertAst = null;
                var leftAst = assignTarget;
                while (leftAst is AttributedExpressionAst)
                    convertCount += 1;
                    convertAst = leftAst as ConvertExpressionAst;
                    if (convertAst == null)
                    leftAst = ((AttributedExpressionAst)leftAst).Child;
                if (leftAst is VariableExpressionAst)
                    // Two below if statements are similar, but there is a difference:
                    // The first one tells us about the dynamic nature of the local type.
                    // The second one tells us about the assignment, that happens in this block.
                    // Potentially there could be more complicated cases like [int[]]($a, $b) = (1,2)
                    // We don't handle them at all in the variable analysis.
                    if (anyAttributes || convertCount > 1 ||
                        (convertAst != null && convertAst.Type.TypeName.GetReflectionType() == null))
                        var varPath = ((VariableExpressionAst)leftAst).VariablePath;
                            var details = _variables[GetUnaliasedVariableName(varPath)];
                    if (!anyAttributes && convertCount <= 1)
                        _currentBlock.AddAst(new AssignmentTarget(assignTarget));
                    // We're not assigning to a simple variable, so visit the left so that variable references get
                    // marked with their proper tuple slots.
                    assignTarget.Accept(this);
            bool invokesCommand = false;
            foreach (var command in pipelineAst.PipelineElements)
                command.Accept(this);
                if (command is CommandAst)
                    invokesCommand = true;
                foreach (var redir in command.Redirections)
                    redir.Accept(this);
            // Because non-local gotos are supported, we must model them in the flow graph.  We can't detect them
            // in general, so we must be pessimistic and assume any command invocation could result in non-local
            // break or continue, so add the appropriate edges to our graph.  These edges occur after visiting
            // the command elements because command arguments could create new blocks, and we won't have executed
            // the command yet.
            if (invokesCommand && _loopTargets.Count > 0)
                foreach (var loopTarget in _loopTargets)
                    _currentBlock.FlowsTo(loopTarget.BreakTarget);
                    _currentBlock.FlowsTo(loopTarget.ContinueTarget);
                // The rest of the block is potentially unreachable, so split the current block.
                var newBlock = new Block();
                _currentBlock.FlowsTo(newBlock);
                _currentBlock = newBlock;
                element.Accept(this);
            commandExpressionAst.Expression.Accept(this);
            commandParameterAst.Argument?.Accept(this);
            fileRedirectionAst.Location.Accept(this);
            if (binaryExpressionAst.Operator == TokenKind.And || binaryExpressionAst.Operator == TokenKind.Or)
                // Logical and/or are short circuit operators, so we need to simulate the control flow.  The
                // left operand is always evaluated, visit it's expression in the current block.
                binaryExpressionAst.Left.Accept(this);
                // The right operand is conditionally evaluated.  We aren't generating any code here, just
                // modeling the flow graph, so we just visit the right operand in a new block, and have
                // both the current and new blocks flow to a post-expression block.
                var targetBlock = new Block();
                var nextBlock = new Block();
                binaryExpressionAst.Right.Accept(this);
                _currentBlock = targetBlock;
            unaryExpressionAst.Child.Accept(this);
            convertExpressionAst.Child.Accept(this);
            subExpressionAst.SubExpression.Accept(this);
            // The SubExpression is not visited, we treat this expression like it is a constant that is replaced
            // before the script block is executed
                // If the variable has already been allocated, we don't need to visit it again later.
                if (details.LocalTupleIndex != VariableAnalysis.Unanalyzed)
                    variableExpressionAst.TupleIndex = details.PreferenceVariable ? ForceDynamic : details.LocalTupleIndex;
                    variableExpressionAst.Automatic = details.Automatic;
                    // Save the variable reference in the current block so it can be marked later with
                    // the allocated tuple index if the variable is assigned in all paths to this reference.
                    _currentBlock.AddAst(variableExpressionAst);
                // Either way - if we later discover an inconsistency and need to force this reference to be dynamic,
                // keep track of the ast so that it is possible.
                details.AssociatedAsts.Add(variableExpressionAst);
                variableExpressionAst.TupleIndex = VariableAnalysis.ForceDynamic;
            memberExpressionAst.Expression.Accept(this);
            memberExpressionAst.Member.Accept(this);
            invokeMemberExpressionAst.Expression.Accept(this);
            invokeMemberExpressionAst.Member.Accept(this);
            if (invokeMemberExpressionAst.Arguments != null)
                foreach (var arg in invokeMemberExpressionAst.Arguments)
                    arg.Accept(this);
            arrayExpressionAst.SubExpression.Accept(this);
                pair.Item1.Accept(this);
                pair.Item2.Accept(this);
            // Don't recurse into the script block, it's variables are distinct from the script block
            parenExpressionAst.Pipeline.Accept(this);
            foreach (var expr in expandableStringExpressionAst.NestedExpressions)
                expr.Accept(this);
            indexExpressionAst.Target.Accept(this);
            indexExpressionAst.Index.Accept(this);
            attributedExpressionAst.Child.Accept(this);
            blockStatementAst.Body.Accept(this);
        public object VisitTypeDefinition(TypeDefinitionAst typeDefinitionAst) => null;
        public object VisitPropertyMember(PropertyMemberAst propertyMemberAst) => null;
        public object VisitFunctionMember(FunctionMemberAst functionMemberAst) => null;
        public object VisitBaseCtorInvokeMemberExpression(BaseCtorInvokeMemberExpressionAst baseCtorInvokeMemberExpressionAst) => null;
        public object VisitUsingStatement(UsingStatementAst usingStatement) => null;
        public object VisitConfigurationDefinition(ConfigurationDefinitionAst configurationDefinitionAst) => null;
        public object VisitDynamicKeywordStatement(DynamicKeywordStatementAst dynamicKeywordAst) => null;
