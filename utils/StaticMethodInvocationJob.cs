    /// Job wrapping invocation of a static CIM method.
    internal sealed class StaticMethodInvocationJob : ExtrinsicMethodInvocationJob
        internal StaticMethodInvocationJob(CimJobContext jobContext, MethodInvocationInfo methodInvocationInfo)
                : base(jobContext, false /* passThru */, jobContext.CmdletizationClassName, methodInvocationInfo)
            get { return null; }
                this.GetCimInstancesFromArguments());
