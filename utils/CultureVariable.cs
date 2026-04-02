    /// A variable that represents $PSCulture.
    internal class PSCultureVariable : PSVariable
        /// Constructs an instance of the variable.
        internal PSCultureVariable()
            : base(SpecialVariables.PSCulture, true, ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope,
                   RunspaceInit.DollarPSCultureDescription)
        /// Gets or sets the value of the variable.
        public override object Value
                DebuggerCheckVariableRead();
                return System.Threading.Thread.CurrentThread.CurrentCulture.Name;
    /// A variable that represents $PSUICulture.
    internal class PSUICultureVariable : PSVariable
        internal PSUICultureVariable()
            : base(SpecialVariables.PSUICulture, true, ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope,
                   RunspaceInit.DollarPSUICultureDescription)
                return System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
