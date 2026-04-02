    internal sealed class LoadObjectInstruction : Instruction
        private readonly object _value;
        internal LoadObjectInstruction(object value)
            frame.Data[frame.StackIndex++] = _value;
            return "LoadObject(" + (_value ?? "null") + ")";
    internal sealed class LoadCachedObjectInstruction : Instruction
        private readonly uint _index;
        internal LoadCachedObjectInstruction(uint index)
            frame.Data[frame.StackIndex++] = frame.Interpreter._objects[_index];
            return string.Format(CultureInfo.InvariantCulture, "LoadCached({0}: {1})", _index, objects[(int)_index]);
            return "LoadCached(" + _index + ")";
    internal sealed class PopInstruction : Instruction
        internal static readonly PopInstruction Instance = new PopInstruction();
        private PopInstruction() { }
            frame.Pop();
            return "Pop()";
    internal sealed class DupInstruction : Instruction
        internal static readonly DupInstruction Instance = new DupInstruction();
        private DupInstruction() { }
            frame.Data[frame.StackIndex++] = frame.Peek();
            return "Dup()";
