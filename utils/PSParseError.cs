/********************************************************************++
    Project:     PowerShell
    Contents:    PowerShell error interface for syntax editors
    Classes:     System.Management.Automation.PSParseError
--********************************************************************/
    /// This is a class that represents a syntax error from parsing.
    public sealed class PSParseError
        internal PSParseError(RuntimeException rte)
            Dbg.Assert(rte != null, "exception argument should not be null");
            Dbg.Assert(rte.ErrorToken != null, "token for exception should not be null");
            Message = rte.Message;
            Token = new PSToken(rte.ErrorToken);
        internal PSParseError(Language.ParseError error)
            Message = error.Message;
            Token = new PSToken(error.Extent);
        /// The token that indicates the error location.
        /// This can either be the real token at which place the error happens or a position
        /// token indicating the location where error happens.
        public PSToken Token { get; }
        /// Error message.
