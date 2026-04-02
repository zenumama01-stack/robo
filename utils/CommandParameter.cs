    /// Represents a parameter to the Command.
    [DebuggerDisplay("{ParameterName}")]
    internal sealed class CommandParameterInternal
        private sealed class Parameter
            internal Ast ast;
            internal string parameterName;
            internal string parameterText;
        private sealed class Argument
            internal object value;
            internal bool splatted;
        private Parameter _parameter;
        private Argument _argument;
        private bool _spaceAfterParameter;
        private bool _fromHashtableSplatting;
        internal bool SpaceAfterParameter => _spaceAfterParameter;
        internal bool ParameterNameSpecified => _parameter != null;
        internal bool ArgumentSpecified => _argument != null;
        internal bool ParameterAndArgumentSpecified => ParameterNameSpecified && ArgumentSpecified;
        internal bool FromHashtableSplatting => _fromHashtableSplatting;
        /// Gets and sets the string that represents parameter name, which does not include the '-' (dash).
        internal string ParameterName
                Diagnostics.Assert(ParameterNameSpecified, "Caller must verify parameter name was specified");
                return _parameter.parameterName;
                _parameter.parameterName = value;
        /// The text of the parameter, which typically includes the leading '-' (dash) and, if specified, the trailing ':'.
        internal string ParameterText
                return _parameter.parameterText;
        /// The ast of the parameter, if one was specified.
        internal Ast ParameterAst
            get => _parameter?.ast;
        /// The extent of the parameter, if one was specified.
        internal IScriptExtent ParameterExtent
            get => ParameterAst?.Extent ?? PositionUtilities.EmptyExtent;
        /// The ast of the optional argument, if one was specified.
        internal Ast ArgumentAst
            get => _argument?.ast;
        /// The extent of the optional argument, if one was specified.
        internal IScriptExtent ArgumentExtent
            get => ArgumentAst?.Extent ?? PositionUtilities.EmptyExtent;
        /// The value of the optional argument, if one was specified, otherwise UnboundParameter.Value.
        internal object ArgumentValue
            get { return _argument != null ? _argument.value : UnboundParameter.Value; }
        /// If an argument was specified and is to be splatted, returns true, otherwise false.
        internal bool ArgumentToBeSplatted
            get { return _argument != null && _argument.splatted; }
        /// Set the argument value and ast.
        internal void SetArgumentValue(Ast ast, object value)
            _argument ??= new Argument();
            _argument.value = value;
            _argument.ast = ast;
        /// The extent to use when reporting generic errors.  The argument extent is used, if it is not empty, otherwise
        /// the parameter extent is used.  Some errors may prefer the parameter extent and should not use this method.
        internal IScriptExtent ErrorExtent
                var argExtent = ArgumentExtent;
                return argExtent != PositionUtilities.EmptyExtent ? argExtent : ParameterExtent;
        /// Create a parameter when no argument has been specified.
        /// <param name="ast">The ast in script of the parameter.</param>
        /// <param name="parameterName">The parameter name (with no leading dash).</param>
        /// <param name="parameterText">The text of the parameter, as it did, or would, appear in script.</param>
        internal static CommandParameterInternal CreateParameter(
            string parameterText,
            Ast ast = null)
            return new CommandParameterInternal
                _parameter =
                           new Parameter { ast = ast, parameterName = parameterName, parameterText = parameterText }
        /// Create a positional argument to a command.
        /// <param name="value">The argument value.</param>
        /// <param name="ast">The ast of the argument value in the script.</param>
        /// <param name="splatted">True if the argument value is to be splatted, false otherwise.</param>
        internal static CommandParameterInternal CreateArgument(
            Ast ast = null,
            bool splatted = false)
                _argument = new Argument
                    value = value,
                    ast = ast,
                    splatted = splatted,
        /// Create an named argument, where the parameter name is known.  This can happen when:
        ///     * The user uses the ':' syntax, as in
        ///         foo -bar:val
        ///     * Splatting, as in
        ///         $x = @{ bar = val } ; foo @x
        ///     * Via an API - when converting a CommandParameter to CommandParameterInternal.
        ///     * In the parameter binder when it resolves a positional argument
        ///     * Other random places that manually construct command processors and know their arguments.
        /// <param name="parameterAst">The ast in script of the parameter.</param>
        /// <param name="argumentAst">The ast of the argument value in the script.</param>
        /// <param name="spaceAfterParameter">Used in native commands to correctly handle -foo:bar vs. -foo: bar.</param>
        /// <param name="fromSplatting">Indicate if this parameter-argument pair comes from splatting.</param>
        internal static CommandParameterInternal CreateParameterWithArgument(
            Ast parameterAst,
            Ast argumentAst,
            bool spaceAfterParameter,
            bool fromSplatting = false)
                _parameter = new Parameter { ast = parameterAst, parameterName = parameterName, parameterText = parameterText },
                _argument = new Argument { ast = argumentAst, value = value },
                _spaceAfterParameter = spaceAfterParameter,
                _fromHashtableSplatting = fromSplatting,
        internal bool IsDashQuestion()
            return ParameterNameSpecified && (ParameterName.Equals("?", StringComparison.OrdinalIgnoreCase));
