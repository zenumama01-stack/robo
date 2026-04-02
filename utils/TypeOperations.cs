    internal sealed class CreateDelegateInstruction : Instruction
        private readonly LightDelegateCreator _creator;
        internal CreateDelegateInstruction(LightDelegateCreator delegateCreator)
            _creator = delegateCreator;
        public override int ConsumedStack { get { return _creator.Interpreter.ClosureSize; } }
            StrongBox<object>[] closure;
            if (ConsumedStack > 0)
                closure = new StrongBox<object>[ConsumedStack];
                for (int i = closure.Length - 1; i >= 0; i--)
                    closure[i] = (StrongBox<object>)frame.Pop();
                closure = null;
            Delegate d = _creator.CreateDelegate(closure);
            frame.Push(d);
    internal sealed class NewInstruction : Instruction
        private readonly ConstructorInfo _constructor;
        private readonly int _argCount;
        public NewInstruction(ConstructorInfo constructor)
            _constructor = constructor;
            _argCount = constructor.GetParameters().Length;
        public override int ConsumedStack { get { return _argCount; } }
            object[] args = new object[_argCount];
            for (int i = _argCount - 1; i >= 0; i--)
                args[i] = frame.Pop();
            object ret;
                ret = _constructor.Invoke(args);
            frame.Push(ret);
            return "New " + _constructor.DeclaringType.Name + "(" + _constructor + ")";
    internal sealed class DefaultValueInstruction<T> : Instruction
        internal DefaultValueInstruction() { }
            frame.Push(default(T));
            return "New " + typeof(T);
    internal sealed class TypeIsInstruction<T> : Instruction
        internal TypeIsInstruction() { }
            // unfortunately Type.IsInstanceOfType() is 35-times slower than "is T" so we use generic code:
            frame.Push(ScriptingRuntimeHelpers.BooleanToObject(frame.Pop() is T));
            return "TypeIs " + typeof(T).Name;
    internal sealed class TypeAsInstruction<T> : Instruction
        internal TypeAsInstruction() { }
            // can't use as w/o generic constraint
            if (value is T)
                frame.Push(value);
                frame.Push(null);
            return "TypeAs " + typeof(T).Name;
    internal sealed class TypeEqualsInstruction : Instruction
        public static readonly TypeEqualsInstruction Instance = new TypeEqualsInstruction();
        private TypeEqualsInstruction()
            object type = frame.Pop();
            object obj = frame.Pop();
            frame.Push(ScriptingRuntimeHelpers.BooleanToObject(obj != null && (object)obj.GetType() == type));
            get { return "TypeEquals()"; }
