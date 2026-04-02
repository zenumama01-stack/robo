    internal interface IBoxableInstruction
        Instruction? BoxIfIndexMatches(int index);
    internal abstract class LocalAccessInstruction : Instruction
        internal readonly int _index;
        protected LocalAccessInstruction(int index)
            return cookie == null ?
                InstructionName + "(" + _index + ")" :
                InstructionName + "(" + cookie + ": " + _index + ")";
    #region Load
    internal sealed class LoadLocalInstruction : LocalAccessInstruction, IBoxableInstruction
        internal LoadLocalInstruction(int index)
            : base(index)
            frame.Data[frame.StackIndex++] = frame.Data[_index];
            // frame.Push(frame.Data[_index]);
        public Instruction BoxIfIndexMatches(int index)
            return (index == _index) ? InstructionList.LoadLocalBoxed(index) : null;
    internal sealed class LoadLocalBoxedInstruction : LocalAccessInstruction
        internal LoadLocalBoxedInstruction(int index)
            var box = (StrongBox<object>)frame.Data[_index];
            frame.Data[frame.StackIndex++] = box.Value;
    internal sealed class LoadLocalFromClosureInstruction : LocalAccessInstruction
        internal LoadLocalFromClosureInstruction(int index)
            var box = frame.Closure[_index];
    internal sealed class LoadLocalFromClosureBoxedInstruction : LocalAccessInstruction
        internal LoadLocalFromClosureBoxedInstruction(int index)
            frame.Data[frame.StackIndex++] = box;
    #region Store, Assign
    internal sealed class AssignLocalInstruction : LocalAccessInstruction, IBoxableInstruction
        internal AssignLocalInstruction(int index)
            frame.Data[_index] = frame.Peek();
            return (index == _index) ? InstructionList.AssignLocalBoxed(index) : null;
    internal sealed class StoreLocalInstruction : LocalAccessInstruction, IBoxableInstruction
        internal StoreLocalInstruction(int index)
            frame.Data[_index] = frame.Data[--frame.StackIndex];
            // frame.Data[_index] = frame.Pop();
            return (index == _index) ? InstructionList.StoreLocalBoxed(index) : null;
    internal sealed class AssignLocalBoxedInstruction : LocalAccessInstruction
        internal AssignLocalBoxedInstruction(int index)
            box.Value = frame.Peek();
    internal sealed class StoreLocalBoxedInstruction : LocalAccessInstruction
        internal StoreLocalBoxedInstruction(int index)
            box.Value = frame.Data[--frame.StackIndex];
    internal sealed class AssignLocalToClosureInstruction : LocalAccessInstruction
        internal AssignLocalToClosureInstruction(int index)
    #region Initialize
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1012:AbstractTypesShouldNotHaveConstructors")]
    internal abstract class InitializeLocalInstruction : LocalAccessInstruction
        internal InitializeLocalInstruction(int index)
        internal sealed class Reference : InitializeLocalInstruction, IBoxableInstruction
            internal Reference(int index)
                frame.Data[_index] = null;
                return (index == _index) ? InstructionList.InitImmutableRefBox(index) : null;
                get { return "InitRef"; }
        internal sealed class ImmutableValue : InitializeLocalInstruction, IBoxableInstruction
            private readonly object _defaultValue;
            internal ImmutableValue(int index, object defaultValue)
                _defaultValue = defaultValue;
                frame.Data[_index] = _defaultValue;
                return (index == _index) ? new ImmutableBox(index, _defaultValue) : null;
                get { return "InitImmutableValue"; }
        internal sealed class ImmutableBox : InitializeLocalInstruction
            // immutable value:
            internal ImmutableBox(int index, object defaultValue)
                frame.Data[_index] = new StrongBox<object>(_defaultValue);
                get { return "InitImmutableBox"; }
        internal sealed class ParameterBox : InitializeLocalInstruction
            public ParameterBox(int index)
                frame.Data[_index] = new StrongBox<object>(frame.Data[_index]);
        internal sealed class Parameter : InitializeLocalInstruction, IBoxableInstruction
            internal Parameter(int index)
                // nop
                if (index == _index)
                    return InstructionList.ParameterBox(index);
                get { return "InitParameter"; }
        internal sealed class MutableValue : InitializeLocalInstruction, IBoxableInstruction
            internal MutableValue(int index, Type type)
                    frame.Data[_index] = Activator.CreateInstance(_type);
                    ExceptionHelpers.UpdateForRethrow(e.InnerException);
                return (index == _index) ? new MutableBox(index, _type) : null;
                get { return "InitMutableValue"; }
        internal sealed class MutableBox : InitializeLocalInstruction
            internal MutableBox(int index, Type type)
                frame.Data[_index] = new StrongBox<object>(Activator.CreateInstance(_type));
                get { return "InitMutableBox"; }
    #region RuntimeVariables
    internal sealed class RuntimeVariablesInstruction : Instruction
        private readonly int _count;
        public RuntimeVariablesInstruction(int count)
            _count = count;
        public override int ConsumedStack { get { return _count; } }
            var ret = new IStrongBox[_count];
            for (int i = ret.Length - 1; i >= 0; i--)
                ret[i] = (IStrongBox)frame.Pop();
            frame.Push(RuntimeVariables.Create(ret));
            return "GetRuntimeVariables()";
