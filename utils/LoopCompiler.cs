    internal sealed class LoopCompiler : ExpressionVisitor
        private struct LoopVariable
            public ExpressionAccess Access;
            // a variable that holds on the strong box for closure variables:
            public ParameterExpression BoxStorage;
            public LoopVariable(ExpressionAccess access, ParameterExpression box)
                Access = access;
                BoxStorage = box;
                return Access.ToString() + " " + BoxStorage;
        private readonly ParameterExpression _frameDataVar;
        private readonly ParameterExpression _frameClosureVar;
        private readonly ParameterExpression _frameVar;
        private readonly LabelTarget _returnLabel;
        // locals and closure variables defined outside the loop
        private readonly Dictionary<ParameterExpression, LocalVariable> _outerVariables, _closureVariables;
        private readonly PowerShellLoopExpression _loop;
        private List<ParameterExpression> _temps;
        // tracks variables that flow in and flow out for initialization and
        private readonly Dictionary<ParameterExpression, LoopVariable> _loopVariables;
        // variables which are defined and used within the loop
        private HashSet<ParameterExpression> _loopLocals;
        private readonly HybridReferenceDictionary<LabelTarget, BranchLabel> _labelMapping;
        private readonly int _loopStartInstructionIndex;
        private readonly int _loopEndInstructionIndex;
        internal LoopCompiler(PowerShellLoopExpression loop,
                              HybridReferenceDictionary<LabelTarget, BranchLabel> labelMapping,
                              Dictionary<ParameterExpression, LocalVariable> locals,
                              Dictionary<ParameterExpression, LocalVariable> closureVariables,
                              int loopStartInstructionIndex,
                              int loopEndInstructionIndex)
            _outerVariables = locals;
            _closureVariables = closureVariables;
            _frameDataVar = Expression.Parameter(typeof(object[]));
            _frameClosureVar = Expression.Parameter(typeof(StrongBox<object>[]));
            _frameVar = Expression.Parameter(typeof(InterpretedFrame));
            _loopVariables = new Dictionary<ParameterExpression, LoopVariable>();
            _returnLabel = Expression.Label(typeof(int));
            _labelMapping = labelMapping;
            _loopStartInstructionIndex = loopStartInstructionIndex;
            _loopEndInstructionIndex = loopEndInstructionIndex;
        internal LoopFunc CreateDelegate()
            var loop = Visit(_loop);
            var body = new List<Expression>();
            var finallyClause = new List<Expression>();
            foreach (var variable in _loopVariables)
                if (!_outerVariables.TryGetValue(variable.Key, out local))
                    local = _closureVariables[variable.Key];
                Expression elemRef = local.LoadFromArray(_frameDataVar, _frameClosureVar);
                if (local.InClosureOrBoxed)
                    var box = variable.Value.BoxStorage;
                    Debug.Assert(box != null);
                    body.Add(Expression.Assign(box, elemRef));
                    AddTemp(box);
                    // Always initialize the variable even if it is only written to.
                    // If a write-only variable is actually not assigned during execution of the loop we will still write some value back.
                    // This value must be the original value, which we assign at entry.
                    body.Add(Expression.Assign(variable.Key, AstUtils.Convert(elemRef, variable.Key.Type)));
                    if ((variable.Value.Access & ExpressionAccess.Write) != 0)
                        finallyClause.Add(Expression.Assign(elemRef, AstUtils.Box(variable.Key)));
                    AddTemp(variable.Key);
            if (finallyClause.Count > 0)
                body.Add(Expression.TryFinally(loop, Expression.Block(finallyClause)));
                body.Add(loop);
            body.Add(Expression.Label(_returnLabel, Expression.Constant(_loopEndInstructionIndex - _loopStartInstructionIndex)));
            var lambda = Expression.Lambda<LoopFunc>(
                _temps != null ? Expression.Block(_temps, body) : Expression.Block(body),
                new[] { _frameDataVar, _frameClosureVar, _frameVar }
            // Reduce extensions before we visit them so that we operate on a plain DLR tree,
            // where we know relationships among the nodes (which nodes represent write context etc.).
            if (node.CanReduce)
                return Visit(node.Reduce());
            return base.VisitExtension(node);
        #region Gotos
        protected override Expression VisitGoto(GotoExpression node)
            BranchLabel label;
            var target = node.Target;
            var value = Visit(node.Value);
            // TODO: Is it possible for an inner reducible node of the loop to rely on nodes produced by reducing outer reducible nodes?
            // Unknown label => must be within the loop:
            if (!_labelMapping.TryGetValue(target, out label))
                return node.Update(target, value);
            // Known label within the loop:
            if (label.TargetIndex >= _loopStartInstructionIndex && label.TargetIndex < _loopEndInstructionIndex)
            return Expression.Return(_returnLabel,
                (value != null && value.Type != typeof(void)) ?
                    Expression.Call(_frameVar, InterpretedFrame.GotoMethod, Expression.Constant(label.LabelIndex), AstUtils.Box(value)) :
                    Expression.Call(_frameVar, InterpretedFrame.VoidGotoMethod, Expression.Constant(label.LabelIndex)),
                node.Type
        #region Local Variables
        // Gather all outer variables accessed in the loop.
        // Determines which ones are read from and written to.
        // We will consider a variable as "read" if it is read anywhere in the loop even though
        // the first operation might actually always be "write". We could do better if we had CFG.
            var variables = ((BlockExpression)node).Variables;
            var prevLocals = EnterVariableScope(variables);
            var res = base.VisitBlock(node);
            ExitVariableScope(prevLocals);
        private HashSet<ParameterExpression> EnterVariableScope(ICollection<ParameterExpression> variables)
            if (_loopLocals == null)
                _loopLocals = new HashSet<ParameterExpression>(variables);
            var prevLocals = new HashSet<ParameterExpression>(_loopLocals);
            _loopLocals.UnionWith(variables);
            return prevLocals;
                var prevLocals = EnterVariableScope(new[] { node.Variable });
                var res = base.VisitCatchBlock(node);
                return base.VisitCatchBlock(node);
            var prevLocals = EnterVariableScope(node.Parameters);
                return base.VisitLambda<T>(node);
        private void ExitVariableScope(HashSet<ParameterExpression> prevLocals)
            _loopLocals = prevLocals;
            // reduce compound assignments:
            Debug.Assert(!node.NodeType.IsReadWriteAssignment());
            var param = node.Left as ParameterExpression;
            if (param != null && node.NodeType == ExpressionType.Assign)
                var left = VisitVariable(param, ExpressionAccess.Write);
                var right = Visit(node.Right);
                // left parameter is a boxed variable:
                if (left.Type != param.Type)
                    Debug.Assert(left.Type == typeof(object));
                    Expression rightVar;
                    if (right.NodeType != ExpressionType.Parameter)
                        // { left.Value = (object)(rightVar = right), rightVar }
                        rightVar = AddTemp(Expression.Parameter(right.Type));
                        right = Expression.Assign(rightVar, right);
                        // { left.Value = (object)right, right }
                        rightVar = right;
                        node.Update(left, null, Expression.Convert(right, left.Type)),
                        rightVar
                    return node.Update(left, null, right);
        protected override Expression VisitUnary(UnaryExpression node)
            // reduce inplace increment/decrement:
            return base.VisitUnary(node);
        // TODO: if we supported ref/out parameter we would need to override
        // MethodCallExpression, VisitDynamic and VisitNew
            return VisitVariable(node, ExpressionAccess.Read);
        private Expression VisitVariable(ParameterExpression node, ExpressionAccess access)
            ParameterExpression box;
            LoopVariable existing;
            if (_loopLocals.Contains(node))
                // local to the loop - not propagated in or out
            else if (_loopVariables.TryGetValue(node, out existing))
                // existing outer variable that we are already tracking
                box = existing.BoxStorage;
                _loopVariables[node] = new LoopVariable(existing.Access | access, box);
            else if (_outerVariables.TryGetValue(node, out loc) ||
              (_closureVariables != null && _closureVariables.TryGetValue(node, out loc)))
                // not tracking this variable yet, but defined in outer scope and seen for the 1st time
                box = loc.InClosureOrBoxed ? Expression.Parameter(typeof(StrongBox<object>), node.Name) : null;
                _loopVariables[node] = new LoopVariable(access, box);
                // node is a variable defined in a nested lambda -> skip
            if (box != null)
                if ((access & ExpressionAccess.Write) != 0)
                    // compound assignments were reduced:
                    Debug.Assert((access & ExpressionAccess.Read) == 0);
                    // box.Value = (object)rhs
                    return LightCompiler.Unbox(box);
                    // (T)box.Value
                    return Expression.Convert(LightCompiler.Unbox(box), node.Type);
        private ParameterExpression AddTemp(ParameterExpression variable)
            _temps ??= new List<ParameterExpression>();
            _temps.Add(variable);
