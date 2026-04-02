//using Microsoft.Scripting.Generation;
    /// Manages creation of interpreted delegates. These delegates will get
    /// compiled if they are executed often enough.
    internal sealed class LightDelegateCreator
        // null if we are forced to compile
        private readonly Interpreter _interpreter;
        private readonly Expression _lambda;
        // Adaptive compilation support:
        private Type _compiledDelegateType;
        private Delegate _compiled;
        private readonly object _compileLock = new object();
        internal LightDelegateCreator(Interpreter interpreter, LambdaExpression lambda)
            Assert.NotNull(lambda);
            _interpreter = interpreter;
            _lambda = lambda;
        // internal LightDelegateCreator(Interpreter interpreter, LightLambdaExpression lambda) {
        //    Assert.NotNull(lambda);
        //    _interpreter = interpreter;
        //    _lambda = lambda;
        internal Interpreter Interpreter
            get { return _interpreter; }
        private bool HasClosure
            get { return _interpreter != null && _interpreter.ClosureSize > 0; }
        internal bool HasCompiled
            get { return _compiled != null; }
        /// True if the compiled delegate has the same type as the lambda;
        /// false if the type was changed for interpretation.
        internal bool SameDelegateType
            get { return _compiledDelegateType == DelegateType; }
        public Delegate CreateDelegate()
            return CreateDelegate(null);
        internal Delegate CreateDelegate(StrongBox<object>[] closure)
            if (_compiled != null)
                // If the delegate type we want is not a Func/Action, we can't
                // use the compiled code directly. So instead just fall through
                // and create an interpreted LightLambda, which will pick up
                // the compiled delegate on its first run.
                // Ideally, we would just rebind the compiled delegate using
                // Delegate.CreateDelegate. Unfortunately, it doesn't work on
                // dynamic methods.
                if (SameDelegateType)
                    return CreateCompiledDelegate(closure);
            if (_interpreter == null)
                // We can't interpret, so force a compile
                Compile(null);
                Delegate compiled = CreateCompiledDelegate(closure);
                Debug.Assert(compiled.GetType() == DelegateType);
                return compiled;
            // Otherwise, we'll create an interpreted LightLambda
            return new LightLambda(this, closure, _interpreter._compilationThreshold).MakeDelegate(DelegateType);
        private Type DelegateType
                LambdaExpression le = _lambda as LambdaExpression;
                if (le != null)
                    return le.Type;
                // return ((LightLambdaExpression)_lambda).Type;
        /// Used by LightLambda to get the compiled delegate.
        internal Delegate CreateCompiledDelegate(StrongBox<object>[] closure)
            Debug.Assert(HasClosure == (closure != null));
            if (HasClosure)
                // We need to apply the closure to get the actual delegate.
                var applyClosure = (Func<StrongBox<object>[], Delegate>)_compiled;
                return applyClosure(closure);
            return _compiled;
        /// Create a compiled delegate for the LightLambda, and saves it so
        /// future calls to Run will execute the compiled code instead of
        /// interpreting.
        internal void Compile(object state)
            // Compilation is expensive, we only want to do it once.
            lock (_compileLock)
                // PerfTrack.NoteEvent(PerfTrack.Categories.Compiler, "Interpreted lambda compiled");
                // Interpreter needs a standard delegate type.
                // So change the lambda's delegate type to Func<...> or
                // Action<...> so it can be called from the LightLambda.Run
                // methods.
                LambdaExpression lambda = (_lambda as LambdaExpression); // ?? (LambdaExpression)((LightLambdaExpression)_lambda).Reduce();
                if (_interpreter != null)
                    _compiledDelegateType = GetFuncOrAction(lambda);
                    lambda = Expression.Lambda(_compiledDelegateType, lambda.Body, lambda.Name, lambda.Parameters);
                    _compiled = LightLambdaClosureVisitor.BindLambda(lambda, _interpreter.ClosureVariables);
                    _compiled = lambda.Compile();
        private static Type GetFuncOrAction(LambdaExpression lambda)
            Type delegateType;
            bool isVoid = lambda.ReturnType == typeof(void);
            // if (isVoid && lambda.Parameters.Count == 2 &&
            //    lambda.Parameters[0].IsByRef && lambda.Parameters[1].IsByRef) {
            //    return typeof(ActionRef<,>).MakeGenericType(lambda.Parameters.Map(p => p.Type));
            Type[] types = lambda.Parameters.Map(static p => p.IsByRef ? p.Type.MakeByRefType() : p.Type);
                if (Expression.TryGetActionType(types, out delegateType))
                    return delegateType;
                types = types.AddLast(lambda.ReturnType);
                if (Expression.TryGetFuncType(types, out delegateType))
            return lambda.Type;
