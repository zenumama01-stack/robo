    internal abstract partial class CallInstruction : Instruction
        public abstract MethodInfo Info { get; }
        /// The number of arguments including "this" for instance methods.
        public abstract int ArgumentCount { get; }
        internal CallInstruction() { }
        private static readonly Dictionary<MethodInfo, CallInstruction> s_cache = new Dictionary<MethodInfo, CallInstruction>();
        public static CallInstruction Create(MethodInfo info)
            return Create(info, info.GetParameters());
        /// Creates a new ReflectedCaller which can be used to quickly invoke the provided MethodInfo.
        public static CallInstruction Create(MethodInfo info, ParameterInfo[] parameters)
            int argumentCount = parameters.Length;
            if (!info.IsStatic)
                argumentCount++;
            // A workaround for CLR bug #796414 (Unable to create delegates for Array.Get/Set):
            // T[]::Address - not supported by ETs due to T& return value
            if (info.DeclaringType != null && info.DeclaringType.IsArray && (info.Name == "Get" || info.Name == "Set"))
                return GetArrayAccessor(info, argumentCount);
            if (info is DynamicMethod || !info.IsStatic && info.DeclaringType.IsValueType)
                return new MethodInfoCallInstruction(info, argumentCount);
            if (argumentCount >= MaxHelpers)
                // no delegate for this size, fallback to reflection invoke
            foreach (ParameterInfo pi in parameters)
                if (pi.ParameterType.IsByRef)
                    // we don't support ref args via generics.
            // see if we've created one w/ a delegate
            CallInstruction res;
            if (ShouldCache(info))
                lock (s_cache)
                    if (s_cache.TryGetValue(info, out res))
            // create it
                if (argumentCount < MaxArgs)
                    res = FastCreate(info, parameters);
                    res = SlowCreate(info, parameters);
                if (tie.InnerException is not NotSupportedException)
                res = new MethodInfoCallInstruction(info, argumentCount);
                // if Delegate.CreateDelegate can't handle the method fallback to
                // the slow reflection version.  For example this can happen w/
                // a generic method defined on an interface and implemented on a class or
                // a virtual generic method.
            // cache it for future users if it's a reasonable method to cache
                    s_cache[info] = res;
        private static CallInstruction GetArrayAccessor(MethodInfo info, int argumentCount)
            Type arrayType = info.DeclaringType;
            bool isGetter = info.Name == "Get";
            switch (arrayType.GetArrayRank())
                    return Create(isGetter ?
                        arrayType.GetMethod("GetValue", new[] { typeof(int) }) :
                        new Action<Array, int, object>(ArrayItemSetter1).GetMethodInfo()
                        arrayType.GetMethod("GetValue", new[] { typeof(int), typeof(int) }) :
                        new Action<Array, int, int, object>(ArrayItemSetter2).GetMethodInfo()
                        arrayType.GetMethod("GetValue", new[] { typeof(int), typeof(int), typeof(int) }) :
                        new Action<Array, int, int, int, object>(ArrayItemSetter3).GetMethodInfo()
        public static void ArrayItemSetter1(Array array, int index0, object value)
            array.SetValue(value, index0);
        public static void ArrayItemSetter2(Array array, int index0, int index1, object value)
            array.SetValue(value, index0, index1);
        public static void ArrayItemSetter3(Array array, int index0, int index1, int index2, object value)
            array.SetValue(value, index0, index1, index2);
        private static bool ShouldCache(MethodInfo info)
            return info is not DynamicMethod;
        /// Gets the next type or null if no more types are available.
        private static Type TryGetParameterOrReturnType(MethodInfo target, ParameterInfo[] pi, int index)
            if (!target.IsStatic)
                    return target.DeclaringType;
            if (index < pi.Length)
                // next in signature
                return pi[index].ParameterType;
            if (target.ReturnType == typeof(void) || index > pi.Length)
                // no more parameters
            // last parameter on Invoke is return type
            return target.ReturnType;
        private static bool IndexIsNotReturnType(int index, MethodInfo target, ParameterInfo[] pi)
            return pi.Length != index || !target.IsStatic;
        /// Uses reflection to create new instance of the appropriate ReflectedCaller.
        private static CallInstruction SlowCreate(MethodInfo info, ParameterInfo[] pis)
            List<Type> types = new List<Type>();
                types.Add(info.DeclaringType);
            foreach (ParameterInfo pi in pis)
                types.Add(pi.ParameterType);
            if (info.ReturnType != typeof(void))
                types.Add(info.ReturnType);
            Type[] arrTypes = types.ToArray();
            return (CallInstruction)Activator.CreateInstance(GetHelperType(info, arrTypes), info);
        #region Instruction
        public sealed override int ProducedStack { get { return Info.ReturnType == typeof(void) ? 0 : 1; } }
        public sealed override int ConsumedStack { get { return ArgumentCount; } }
        public sealed override string InstructionName
            get { return "Call"; }
            return "Call(" + Info + ")";
    internal sealed partial class MethodInfoCallInstruction : CallInstruction
        private readonly MethodInfo _target;
        private readonly int _argumentCount;
        public override MethodInfo Info { get { return _target; } }
        public override int ArgumentCount { get { return _argumentCount; } }
        internal MethodInfoCallInstruction(MethodInfo target, int argumentCount)
            _argumentCount = argumentCount;
        public override object Invoke(params object[] args)
            return InvokeWorker(args);
        public override object InvokeInstance(object instance, params object[] args)
            if (_target.IsStatic)
                    return _target.Invoke(null, args);
                    throw ExceptionHelpers.UpdateForRethrow(e.InnerException);
                return _target.Invoke(instance, args);
        private object InvokeWorker(params object[] args)
                return _target.Invoke(args[0], GetNonStaticArgs(args));
        private static object[] GetNonStaticArgs(object[] args)
            object[] newArgs = new object[args.Length - 1];
            for (int i = 0; i < newArgs.Length; i++)
                newArgs[i] = args[i + 1];
            return newArgs;
        public sealed override int Run(InterpretedFrame frame)
            int first = frame.StackIndex - _argumentCount;
            object[] args = new object[_argumentCount];
                args[i] = frame.Data[first + i];
            object ret = Invoke(args);
            if (_target.ReturnType != typeof(void))
                frame.Data[first] = ret;
                frame.StackIndex = first + 1;
                frame.StackIndex = first;
