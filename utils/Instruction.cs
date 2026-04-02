    internal interface IInstructionProvider
        void AddInstructions(LightCompiler compiler);
    internal abstract class Instruction
        public const int UnknownInstrIndex = int.MaxValue;
        public virtual int ConsumedStack { get { return 0; } }
        public virtual int ProducedStack { get { return 0; } }
        public virtual int ConsumedContinuations { get { return 0; } }
        public virtual int ProducedContinuations { get { return 0; } }
        public int StackBalance
            get { return ProducedStack - ConsumedStack; }
        public int ContinuationsBalance
            get { return ProducedContinuations - ConsumedContinuations; }
        public abstract int Run(InterpretedFrame frame);
        public virtual string InstructionName
            get { return GetType().Name.Replace("Instruction", string.Empty); }
            return InstructionName + "()";
        public virtual string ToDebugString(int instructionIndex, object cookie, Func<int, int> labelIndexer, IList<object> objects)
            return ToString();
        public virtual object GetDebugCookie(LightCompiler compiler)
    internal sealed class NotInstruction : Instruction
        public static readonly Instruction Instance = new NotInstruction();
        private NotInstruction() { }
            frame.Push((bool)frame.Pop() ? ScriptingRuntimeHelpers.False : ScriptingRuntimeHelpers.True);
