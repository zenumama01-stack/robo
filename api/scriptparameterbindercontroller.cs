    /// This is the interface between the ScriptCommandProcessor and the
    /// parameter binders required to bind parameters to a shell function.
    internal class ScriptParameterBinderController : ParameterBinderController
        /// The script that contains the parameter metadata.
        /// The engine context the cmdlet is run in.
        /// The scope that the parameter binder will use to set parameters.
        internal ScriptParameterBinderController(
            SessionStateScope localScope)
            : base(invocationInfo, context, new ScriptParameterBinder(script, invocationInfo, context, command, localScope))
            this.DollarArgs = new List<object>();
            // Add the script parameter metadata to the bindable parameters
            if (script.HasDynamicParameters)
                UnboundParameters = this.BindableParameters.ReplaceMetadata(script.ParameterMetadata);
                _bindableParameters = script.ParameterMetadata;
                UnboundParameters = new List<MergedCompiledCommandParameter>(_bindableParameters.BindableParameters.Values);
        /// Holds the set of parameters that were not bound to any argument (i.e $args)
        internal List<object> DollarArgs { get; }
        /// Binds the command line parameters for shell functions/filters/scripts/scriptblocks.
        ///     The arguments to be bound.
        /// True if binding was successful or false otherwise.
            UnboundArguments = BindNamedParameters(uint.MaxValue, UnboundArguments);
            ParameterBindingException parameterBindingError;
                    out parameterBindingError);
                this.DefaultParameterBinder.RecordBoundParameters = false;
                // If there are any unbound parameters that have default values, then
                // set those default values.
                // If there are any unbound arguments, stick them into $args
                HandleRemainingArguments(UnboundArguments);
                this.DefaultParameterBinder.RecordBoundParameters = true;
        /// True if the parameter was successfully bound. Any error condition
        /// produces an exception.
        internal override bool BindParameter(CommandParameterInternal argument, ParameterBindingFlags flags)
            // Just pass the binding straight through.  No metadata to verify the parameter against.
            DefaultParameterBinder.BindParameter(argument.ParameterName, argument.ArgumentValue, parameterMetadata: null);
        /// Takes the remaining arguments that haven't been bound, and binds
        /// them to $args.
        ///     The remaining unbound arguments.
        /// An array containing the values that were bound to $args.
        private void HandleRemainingArguments(Collection<CommandParameterInternal> arguments)
            List<object> args = new List<object>();
            foreach (CommandParameterInternal parameter in arguments)
                object argValue = parameter.ArgumentSpecified ? parameter.ArgumentValue : null;
                // Proper automatic proxy generation requires the ability to prevent unbound arguments
                // in the proxy from binding to positional parameters in the proxied command.  We use
                // a special key ("$args") when splatting @CommandLineArguments to package up $args.
                // This special key is not created automatically because it is useful to splat @args,
                // just not in the automatically generated proxy.
                // Example usage:
                //   function foo { param($a, $b) $a; $b; $args }
                //   function foo_proxy { param($a) ; $CommandLineArguments.Add('$args', $args); foo @CommandLineArguments }
                //   foo_proxy 1 2 3
                // Then in foo, $a=1, $b=, $args=2,3
                // Here, we want $b in foo to be unbound because the proxy doesn't have $b (an Exchange scenario.)
                // So we pass $args (2,3) in the special entry in @CommandLineArguments.  If we had instead written:
                //   function foo_proxy { param($a) ; foo @CommandLineArguments @args }
                // Then in foo, $a=1, $b=2, $args=3
                // Note that the name $args is chosen to be:
                //   * descriptive
                //   * obscure (it can't be a property/field name in C#, and is an unlikely variable in script)
                // So we shouldn't have any real conflict.  Note that if someone actually puts ${$args} in their
                // param block, then the value will be bound and we won't have an unbound argument for "$args" here.
                if (parameter.ParameterAndArgumentSpecified &&
                    parameter.ParameterName.Equals("$args", StringComparison.OrdinalIgnoreCase))
                    // $args is normally an object[], but because this feature is accessible from script, it's possible
                    // for it to contain anything.
                    if (argValue is object[])
                        args.AddRange(argValue as object[]);
                        args.Add(argValue);
                    // Add a property to the string so we can tell the difference between:
                    //    foo -abc
                    //    foo "-abc"
                    // This is important when splatting, we reconstruct the parameter if the
                    // value is splatted.
                    var parameterText = new PSObject(new string(parameter.ParameterText));
                    if (parameterText.Properties[NotePropertyNameForSplattingParametersInArgs] == null)
                        var noteProperty = new PSNoteProperty(NotePropertyNameForSplattingParametersInArgs,
                                                              parameter.ParameterName)
                        { IsHidden = true };
                        parameterText.Properties.Add(noteProperty);
                    args.Add(parameterText);
            object[] argsArray = args.ToArray();
            DefaultParameterBinder.BindParameter(SpecialVariables.Args, argsArray, parameterMetadata: null);
            DollarArgs.AddRange(argsArray);
        internal const string NotePropertyNameForSplattingParametersInArgs = "<CommandParameterName>";
