    /// Define a parameter for <see cref="Command"/>
    public sealed class CommandParameter
        #region Public constructors
        /// Create a named parameter with a null value.
        /// <param name="name">Parameter name.</param>
        /// Name length is zero after trimming whitespace.
        public CommandParameter(string name)
        /// Create a named parameter.
        /// <param name="value">Parameter value.</param>
        public CommandParameter(string name, object value)
                Name = null;
        #endregion Public constructors
        #region Public properties
        /// Gets the value of the parameter.
        #endregion Public properties
        /// Gets whether the parameter was from splatting a Hashtable.
        private bool FromHashtableSplatting { get; set; }
        #region Conversion from and to CommandParameterInternal
        internal static CommandParameter FromCommandParameterInternal(CommandParameterInternal internalParameter)
            if (internalParameter == null)
                throw PSTraceSource.NewArgumentNullException(nameof(internalParameter));
            // we want the name to preserve 1) dashes, 2) colons, 3) followed-by-space information
            if (internalParameter.ParameterNameSpecified)
                name = internalParameter.ParameterText;
                if (internalParameter.SpaceAfterParameter)
                    name += " ";
                Diagnostics.Assert(name != null, "'name' variable should be initialized at this point");
                Diagnostics.Assert(name[0].IsDash(), "first character in parameter name must be a dash");
                Diagnostics.Assert(name.Trim().Length != 1, "Parameter name has to have some non-whitespace characters in it");
            CommandParameter result = internalParameter.ParameterAndArgumentSpecified
                ? new CommandParameter(name, internalParameter.ArgumentValue)
                : name != null
                    ? new CommandParameter(name)
                    : new CommandParameter(name: null, internalParameter.ArgumentValue);
            result.FromHashtableSplatting = internalParameter.FromHashtableSplatting;
        internal static CommandParameterInternal ToCommandParameterInternal(CommandParameter publicParameter, bool forNativeCommand)
            if (publicParameter == null)
                throw PSTraceSource.NewArgumentNullException(nameof(publicParameter));
            string name = publicParameter.Name;
            object value = publicParameter.Value;
            Debug.Assert((name == null) || (name.Trim().Length != 0), "Parameter name has to null or have some non-whitespace characters in it");
                return CommandParameterInternal.CreateArgument(value);
            string parameterText;
            if (!name[0].IsDash())
                parameterText = forNativeCommand ? name : "-" + name;
                return CommandParameterInternal.CreateParameterWithArgument(
                    parameterName: name,
                    parameterText: parameterText,
            // if first character of name is '-', then we try to fake the original token
            // reconstructing dashes, colons and followed-by-space information
            // find the last non-whitespace character
            bool spaceAfterParameter = false;
            int endPosition = name.Length;
            while ((endPosition > 0) && char.IsWhiteSpace(name[endPosition - 1]))
                spaceAfterParameter = true;
                endPosition--;
            Debug.Assert(endPosition > 0, "parameter name should have some non-whitespace characters in it");
            // now make sure that parameterText doesn't have whitespace at the end,
            parameterText = name.Substring(0, endPosition);
            // parameterName should contain only the actual name of the parameter (no whitespace, colons, dashes)
            bool hasColon = (name[endPosition - 1] == ':');
            var parameterName = parameterText.Substring(1, parameterText.Length - (hasColon ? 2 : 1));
            // At this point we have rebuilt the token.  There are 3 strings that might be different:
            //           name = nameToken.Script = "-foo: " <- needed to fake FollowedBySpace=true (i.e. for "testecho.exe -a:b -c: d")
            // tokenString = nameToken.TokenText = "-foo:" <- needed to preserve full token text (i.e. for write-output)
            //                    nameToken.Data =  "foo" <- needed to preserve name of parameter so parameter binding works
            // Now we just need to use the token to build appropriate CommandParameterInternal object
            // is this a name+value pair, or is it just a name (of a parameter)?
            if (!hasColon && value == null)
                // just a name
                return CommandParameterInternal.CreateParameter(parameterName, parameterText);
            // name+value pair
                parameterText,
                spaceAfterParameter,
                publicParameter.FromHashtableSplatting);
        /// Creates a CommandParameter object from a PSObject property bag.
        /// <param name="parameterAsPSObject">PSObject to rehydrate.</param>
        /// CommandParameter rehydrated from a PSObject property bag
        internal static CommandParameter FromPSObjectForRemoting(PSObject parameterAsPSObject)
            if (parameterAsPSObject == null)
                throw PSTraceSource.NewArgumentNullException(nameof(parameterAsPSObject));
            string name = RemotingDecoder.GetPropertyValue<string>(parameterAsPSObject, RemoteDataNameStrings.ParameterName);
            object value = RemotingDecoder.GetPropertyValue<object>(parameterAsPSObject, RemoteDataNameStrings.ParameterValue);
            return new CommandParameter(name, value);
            PSObject parameterAsPSObject = RemotingEncoder.CreateEmptyPSObject();
            parameterAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ParameterName, this.Name));
            parameterAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ParameterValue, this.Value));
            return parameterAsPSObject;
    /// Defines a collection of parameters.
    public sealed class CommandParameterCollection : Collection<CommandParameter>
        // TODO: this class needs a mechanism to lock further changes
        /// Create a new empty instance of this collection type.
        public CommandParameterCollection()
        /// Add a parameter with given name and default null value.
        /// <param name="name">Name of the parameter.</param>
        public void Add(string name)
            Add(new CommandParameter(name));
        /// Add a parameter with given name and value.
        /// <param name="value">Value of the parameter.</param>
        /// Both name and value are null. One of these must be non-null.
        public void Add(string name, object value)
            Add(new CommandParameter(name, value));
