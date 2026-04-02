    internal sealed class NewArrayInitInstruction<TElement> : Instruction
        private readonly int _elementCount;
        internal NewArrayInitInstruction(int elementCount)
            _elementCount = elementCount;
        public override int ConsumedStack { get { return _elementCount; } }
            TElement[] array = new TElement[_elementCount];
            for (int i = _elementCount - 1; i >= 0; i--)
                array[i] = (TElement)frame.Pop();
            frame.Push(array);
    internal sealed class NewArrayInstruction<TElement> : Instruction
        internal NewArrayInstruction() { }
        public override int ConsumedStack { get { return 1; } }
            int length = (int)frame.Pop();
            frame.Push(new TElement[length]);
    internal sealed class NewArrayBoundsInstruction : Instruction
        private readonly Type _elementType;
        private readonly int _rank;
        internal NewArrayBoundsInstruction(Type elementType, int rank)
            _elementType = elementType;
            _rank = rank;
        public override int ConsumedStack { get { return _rank; } }
            var lengths = new int[_rank];
            for (int i = _rank - 1; i >= 0; i--)
                lengths[i] = (int)frame.Pop();
            var array = Array.CreateInstance(_elementType, lengths);
    internal sealed class GetArrayItemInstruction<TElement> : Instruction
        internal GetArrayItemInstruction() { }
            int index = (int)frame.Pop();
            TElement[] array = (TElement[])frame.Pop();
            frame.Push(array[index]);
        public override string InstructionName
            get { return "GetArrayItem"; }
    internal sealed class SetArrayItemInstruction<TElement> : Instruction
        internal SetArrayItemInstruction() { }
        public override int ConsumedStack { get { return 3; } }
        public override int ProducedStack { get { return 0; } }
            TElement value = (TElement)frame.Pop();
            array[index] = value;
            get { return "SetArrayItem"; }
