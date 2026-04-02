    /// Write result object to ps pipeline
    internal sealed class CimWriteResultObject : CimBaseAction
        /// Initializes a new instance of the <see cref="CimWriteResultObject"/> class.
        public CimWriteResultObject(object result, XOperationContextBase theContext)
            this.Result = result;
            this.Context = theContext;
            cmdlet.WriteObject(Result, this.Context);
        /// Result object.
        internal object Result { get; }
