    /// A simple forth-style stack machine for executing Expression trees
    /// without the need to compile to IL and then invoke the JIT.  This trades
    /// off much faster compilation time for a slower execution performance.
    /// For code that is only run a small number of times this can be a
    /// sweet spot.
    /// The core loop in the interpreter is the RunInstructions method.
    internal sealed class Interpreter
        internal static readonly object NoValue = new object();
        internal const int RethrowOnReturn = Int32.MaxValue;
        // zero: sync compilation
        // negative: default
        internal readonly int _compilationThreshold;
        internal readonly object[] _objects;
        internal readonly RuntimeLabel[] _labels;
        internal readonly string _name;
        internal readonly DebugInfo[] _debugInfos;
        internal Interpreter(string name, LocalVariables locals, HybridReferenceDictionary<LabelTarget, BranchLabel> labelMapping,
            InstructionArray instructions, DebugInfo[] debugInfos, int compilationThreshold)
            LocalCount = locals.LocalCount;
            ClosureVariables = locals.ClosureVariables;
            _objects = instructions.Objects;
            _labels = instructions.Labels;
            LabelMapping = labelMapping;
            _debugInfos = debugInfos;
        internal int ClosureSize
                if (ClosureVariables == null)
                return ClosureVariables.Count;
        internal int LocalCount { get; }
        internal bool CompileSynchronously
            get { return _compilationThreshold <= 1; }
        internal InstructionArray Instructions { get; }
        internal Dictionary<ParameterExpression, LocalVariable> ClosureVariables { get; }
        internal HybridReferenceDictionary<LabelTarget, BranchLabel> LabelMapping { get; }
        /// Runs instructions within the given frame.
        /// Interpreted stack frames are linked via Parent reference so that each CLR frame of this method corresponds
        /// to an interpreted stack frame in the chain. It is therefore possible to combine CLR stack traces with
        /// interpreted stack traces by aligning interpreted frames to the frames of this method.
        /// Each group of subsequent frames of Run method corresponds to a single interpreted frame.
        [SpecialName, MethodImpl(MethodImplOptions.NoInlining)]
        public void Run(InterpretedFrame frame)
            var instructions = Instructions.Instructions;
            while (index < instructions.Length)
