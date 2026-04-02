    /// Describes how to handle the method parameter.
    public enum MethodParameterBindings
        /// Bind value of a method parameter based on arguments of a cmdlet parameter.
        In = 1,
        /// Method invocation is expected to set the value of the method parameter.  Cmdlet should emit the value of method parameter to the downstream pipe.
        Out = 2,
        /// Method invocation is expected to set the value of the method parameter.  Cmdlet should emit a non-terminating error when the value evaluates to $true.
        Error = 4,
    /// Parameter of a method in an object model wrapped by <see cref="CmdletAdapter&lt;TObjectInstance&gt;"/>
    public sealed class MethodParameter
        /// Name of the method parameter.
        /// Type of the parameter (as seen in the PowerShell layer on the client)
        public Type ParameterType { get; set; }
        /// Contents of the ETS type attribute in the CDXML file (or <see langword="null"/> if that attribute was not specified).
        /// The expectation is that the CmdletAdapter will stamp this value onto PSTypeNames of emitted objects.
        public string ParameterTypeName { get; set; }
        /// Bindings of the method parameter (in/out/error)
        public MethodParameterBindings Bindings { get; set; }
        /// Value of the argument of the method parameter.
        /// Whether the value is 1) an explicit default (*) or 2) has been bound from cmdlet parameter
        /// (*) explicit default = whatever was in DefaultValue attribute in Cmdletization XML.
        public bool IsValuePresent { get; set; }
        // TODO/FIXME: this should be renamed to ValueExplicitlySpecified or something like this
