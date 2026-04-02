    internal sealed class LoadStaticFieldInstruction : Instruction
        private readonly FieldInfo _field;
        public LoadStaticFieldInstruction(FieldInfo field)
            Debug.Assert(field.IsStatic);
            _field = field;
            frame.Push(_field.GetValue(null));
    internal sealed class LoadFieldInstruction : Instruction
        public LoadFieldInstruction(FieldInfo field)
            Assert.NotNull(field);
            frame.Push(_field.GetValue(frame.Pop()));
    internal sealed class StoreFieldInstruction : Instruction
        public StoreFieldInstruction(FieldInfo field)
            object value = frame.Pop();
            object self = frame.Pop();
            _field.SetValue(self, value);
    internal sealed class StoreStaticFieldInstruction : Instruction
        public StoreStaticFieldInstruction(FieldInfo field)
            _field.SetValue(null, value);
