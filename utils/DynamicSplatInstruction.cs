    /// Implements dynamic call site with many arguments. Wraps the arguments into <see cref="ArgumentArray"/>.
    internal sealed class DynamicSplatInstruction : Instruction
        private readonly CallSite<Func<CallSite, ArgumentArray, object>> _site;
        internal DynamicSplatInstruction(int argumentCount, CallSite<Func<CallSite, ArgumentArray, object>> site)
            object ret = _site.Target(_site, new ArgumentArray(frame.Data, first, _argumentCount));
            return "DynamicSplatInstruction(" + _site + ")";
