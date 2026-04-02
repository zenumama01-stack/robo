    /// parameter binders required to bind parameters to a native command.
    internal class NativeCommandParameterBinderController : ParameterBinderController
        internal NativeCommandParameterBinderController(NativeCommand command)
            : base(command.MyInvocation, command.Context, new NativeCommandParameterBinder(command))
                return ((NativeCommandParameterBinder)DefaultParameterBinder).Arguments;
        /// Gets the value of the command arguments as an array of strings.
                return ((NativeCommandParameterBinder)DefaultParameterBinder).ArgumentList;
        /// Gets the value indicating what type of native argument binding to use.
                return ((NativeCommandParameterBinder)DefaultParameterBinder).ArgumentPassingStyle;
        /// Passes the binding directly through to the parameter binder.
        /// It does no verification against metadata.
        /// The name and value of the variable to bind.
        /// True if the parameter was successfully bound. Any error condition produces an exception.
        internal override Collection<CommandParameterInternal> BindParameters(Collection<CommandParameterInternal> parameters)
