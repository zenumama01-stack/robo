    internal sealed class UpdatePositionInstruction : Instruction
        private readonly int _sequencePoint;
        private readonly bool _checkBreakpoints;
        private UpdatePositionInstruction(bool checkBreakpoints, int sequencePoint)
            _checkBreakpoints = checkBreakpoints;
            _sequencePoint = sequencePoint;
            var functionContext = frame.FunctionContext;
            var context = frame.ExecutionContext;
            functionContext._currentSequencePointIndex = _sequencePoint;
            if (_checkBreakpoints)
                if (context._debuggingMode > 0)
                    context.Debugger.OnSequencePointHit(functionContext);
        public static Instruction Create(int sequencePoint, bool checkBreakpoints)
            return new UpdatePositionInstruction(checkBreakpoints, sequencePoint);
