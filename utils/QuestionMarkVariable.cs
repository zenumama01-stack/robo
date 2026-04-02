    /// A variable that represents $?
    internal class QuestionMarkVariable : PSVariable
        /// Constructs an instance of the variable with execution context.
        /// Execution context
        internal QuestionMarkVariable(ExecutionContext context)
            : base(SpecialVariables.Question, true, ScopedItemOptions.ReadOnly | ScopedItemOptions.AllScope, RunspaceInit.DollarHookDescription)
                return _context.QuestionMarkVariableValue;
                // Call base's setter to force an error (because the variable is readonly).
                base.Value = value;
