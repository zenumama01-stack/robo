    internal sealed partial class DynamicInstructionN : Instruction
        private readonly CallInstruction _target;
        private readonly object _targetDelegate;
        private readonly CallSite _site;
        private readonly bool _isVoid;
        public DynamicInstructionN(Type delegateType, CallSite site)
            var methodInfo = delegateType.GetMethod("Invoke");
            var parameters = methodInfo.GetParameters();
            _target = CallInstruction.Create(methodInfo, parameters);
            _site = site;
            _argumentCount = parameters.Length - 1;
            _targetDelegate = site.GetType().GetField("Target").GetValue(site);
        public DynamicInstructionN(Type delegateType, CallSite site, bool isVoid)
            : this(delegateType, site)
            _isVoid = isVoid;
        public override int ProducedStack { get { return _isVoid ? 0 : 1; } }
        public override int ConsumedStack { get { return _argumentCount; } }
            object[] args = new object[1 + _argumentCount];
            args[0] = _site;
            for (int i = 0; i < _argumentCount; i++)
                args[1 + i] = frame.Data[first + i];
            object ret = _target.InvokeInstance(_targetDelegate, args);
            if (_isVoid)
            return "DynamicInstructionN(" + _site + ")";
