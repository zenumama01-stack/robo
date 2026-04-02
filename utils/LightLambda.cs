    internal sealed class LightLambdaCompileEventArgs : EventArgs
        public Delegate Compiled { get; }
        internal LightLambdaCompileEventArgs(Delegate compiled)
            Compiled = compiled;
    internal partial class LightLambda
        private readonly StrongBox<object>[] _closure;
        private static readonly CacheDict<Type, Func<LightLambda, Delegate>> s_runCache = new CacheDict<Type, Func<LightLambda, Delegate>>(100);
        // Adaptive compilation support
        private readonly LightDelegateCreator _delegateCreator;
        /// Provides notification that the LightLambda has been compiled.
        public event EventHandler<LightLambdaCompileEventArgs> Compile;
        internal LightLambda(LightDelegateCreator delegateCreator, StrongBox<object>[] closure, int compilationThreshold)
            _delegateCreator = delegateCreator;
            _closure = closure;
            _interpreter = delegateCreator.Interpreter;
        private static Func<LightLambda, Delegate> GetRunDelegateCtor(Type delegateType)
            lock (s_runCache)
                Func<LightLambda, Delegate> fastCtor;
                if (s_runCache.TryGetValue(delegateType, out fastCtor))
                    return fastCtor;
                return MakeRunDelegateCtor(delegateType);
        private static Func<LightLambda, Delegate> MakeRunDelegateCtor(Type delegateType)
            var method = delegateType.GetMethod("Invoke");
            var paramInfos = method.GetParameters();
            Type[] paramTypes;
            string name = "Run";
            if (paramInfos.Length >= MaxParameters)
            if (method.ReturnType == typeof(void))
                name += "Void";
                paramTypes = new Type[paramInfos.Length];
                paramTypes = new Type[paramInfos.Length + 1];
                paramTypes[paramTypes.Length - 1] = method.ReturnType;
            MethodInfo runMethod;
            if (method.ReturnType == typeof(void) && paramTypes.Length == 2 &&
                paramInfos[0].ParameterType.IsByRef && paramInfos[1].ParameterType.IsByRef)
                runMethod = typeof(LightLambda).GetMethod("RunVoidRef2", BindingFlags.NonPublic | BindingFlags.Instance);
                paramTypes[0] = paramInfos[0].ParameterType.GetElementType();
                paramTypes[1] = paramInfos[1].ParameterType.GetElementType();
            else if (method.ReturnType == typeof(void) && paramTypes.Length == 0)
                runMethod = typeof(LightLambda).GetMethod("RunVoid0", BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < paramInfos.Length; i++)
                    paramTypes[i] = paramInfos[i].ParameterType;
                    if (paramTypes[i].IsByRef)
                if (DelegateHelpers.MakeDelegate(paramTypes) == delegateType)
                    name = "Make" + name + paramInfos.Length;
                    MethodInfo ctorMethod = typeof(LightLambda).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(paramTypes);
                    return s_runCache[delegateType] = (Func<LightLambda, Delegate>)ctorMethod.CreateDelegate(typeof(Func<LightLambda, Delegate>));
                runMethod = typeof(LightLambda).GetMethod(name + paramInfos.Length, BindingFlags.NonPublic | BindingFlags.Instance);
#if !SILVERLIGHT
                DynamicMethod dm = new DynamicMethod("FastCtor", typeof(Delegate), new[] { typeof(LightLambda) }, typeof(LightLambda), true);
                var ilgen = dm.GetILGenerator();
                ilgen.Emit(OpCodes.Ldarg_0);
                ilgen.Emit(OpCodes.Ldftn, runMethod.IsGenericMethodDefinition ? runMethod.MakeGenericMethod(paramTypes) : runMethod);
                ilgen.Emit(OpCodes.Newobj, delegateType.GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
                ilgen.Emit(OpCodes.Ret);
                return s_runCache[delegateType] = (Func<LightLambda, Delegate>)dm.CreateDelegate(typeof(Func<LightLambda, Delegate>));
            // we don't have permission for restricted skip visibility dynamic methods, use the slower Delegate.CreateDelegate.
            var targetMethod = runMethod.IsGenericMethodDefinition ? runMethod.MakeGenericMethod(paramTypes) : runMethod;
            return s_runCache[delegateType] = lambda => targetMethod.CreateDelegate(delegateType, lambda);
        // TODO enable sharing of these custom delegates
        private Delegate CreateCustomDelegate(Type delegateType)
            // PerfTrack.NoteEvent(PerfTrack.Categories.Compiler, "Synchronously compiling a custom delegate");
            var parameters = new ParameterExpression[paramInfos.Length];
            var parametersAsObject = new Expression[paramInfos.Length];
                ParameterExpression parameter = Expression.Parameter(paramInfos[i].ParameterType, paramInfos[i].Name);
                parameters[i] = parameter;
                parametersAsObject[i] = Expression.Convert(parameter, typeof(object));
            var data = Expression.NewArrayInit(typeof(object), parametersAsObject);
            var self = AstUtils.Constant(this);
            var runMethod = typeof(LightLambda).GetMethod("Run");
            var body = Expression.Convert(Expression.Call(self, runMethod, data), method.ReturnType);
            var lambda = Expression.Lambda(delegateType, body, parameters);
            return lambda.Compile();
        internal Delegate MakeDelegate(Type delegateType)
            Func<LightLambda, Delegate> fastCtor = GetRunDelegateCtor(delegateType);
            if (fastCtor != null)
                return fastCtor(this);
                return CreateCustomDelegate(delegateType);
        private bool TryGetCompiled()
            // Use the compiled delegate if available.
            if (_delegateCreator.HasCompiled)
                _compiled = _delegateCreator.CreateCompiledDelegate(_closure);
                // Send it to anyone who's interested.
                var compileEvent = Compile;
                if (compileEvent != null && _delegateCreator.SameDelegateType)
                    compileEvent(this, new LightLambdaCompileEventArgs(_compiled));
#if SILVERLIGHT
                if (PlatformAdaptationLayer.IsCompactFramework) {
                    _compilationThreshold = Int32.MaxValue;
                if (_interpreter.CompileSynchronously)
                    _delegateCreator.Compile(null);
                    return TryGetCompiled();
                    ThreadPool.QueueUserWorkItem(_delegateCreator.Compile, null);
        private InterpretedFrame MakeFrame()
            return new InterpretedFrame(_interpreter, _closure);
        internal void RunVoidRef2<T0, T1>(ref T0 arg0, ref T1 arg1)
            // if (_compiled != null || TryGetCompiled()) {
            //    ((ActionRef<T0, T1>)_compiled)(ref arg0, ref arg1);
            //    return;
            // copy in and copy out for today...
            var currentFrame = frame.Enter();
                _interpreter.Run(frame);
                frame.Leave(currentFrame);
                arg0 = (T0)frame.Data[0];
                arg1 = (T1)frame.Data[1];
        public object Run(params object[] arguments)
            if (_compiled != null || TryGetCompiled())
                return _compiled.DynamicInvoke(arguments);
                frame.Data[i] = arguments[i];
            return frame.Pop();
