    /// A cmdlet that gets the TraceSource instances that are instantiated in the process.
    [Cmdlet(VerbsCommon.Get, "TraceSource", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096707")]
    [OutputType(typeof(PSTraceSource))]
    public class GetTraceSourceCommand : TraceCommandBase
        /// Gets or sets the category parameter which determines which trace switch to get.
                return _names;
                if (value == null || value.Length == 0)
                    value = new string[] { "*" };
        #region Cmdlet code
        /// Gets the PSTraceSource for the specified category.
            var sources = GetMatchingTraceSource(_names, true);
            var result = sources.OrderBy(static source => source.Name);
            WriteObject(result, true);
        #endregion Cmdlet code
