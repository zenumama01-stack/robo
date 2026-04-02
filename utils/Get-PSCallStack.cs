    /// This class implements Get-PSCallStack.
    [Cmdlet(VerbsCommon.Get, "PSCallStack", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096705")]
    [OutputType(typeof(CallStackFrame))]
    public class GetPSCallStackCommand : PSCmdlet
        /// Get the call stack.
            foreach (CallStackFrame frame in Context.Debugger.GetCallStack())
                WriteObject(frame);
