    /// A ProxyCommand class used to represent a Command constructed Dynamically.
    public sealed class ProxyCommand
        #region Private Constructor
        /// Private Constructor to restrict inheritance.
        private ProxyCommand()
        #region Public Static Methods
        /// This method constructs a string representing the command specified by <paramref name="commandMetadata"/>.
        /// The returned string is a ScriptBlock which can be used to configure a Cmdlet/Function in a Runspace.
        /// An instance of CommandMetadata representing a command.
        /// A string representing Command ScriptBlock.
        /// commandMetadata is null.
        public static string Create(CommandMetadata commandMetadata)
                throw PSTraceSource.NewArgumentNullException("commandMetaData");
            return commandMetadata.GetProxyCommand(string.Empty, true);
        /// <param name="helpComment">
        /// The string to be used as the help comment.
        public static string Create(CommandMetadata commandMetadata, string helpComment)
            return commandMetadata.GetProxyCommand(helpComment, true);
        /// <param name="generateDynamicParameters">
        /// A boolean that determines whether the generated proxy command should include the functionality required
        /// to proxy dynamic parameters of the underlying command.
        public static string Create(CommandMetadata commandMetadata, string helpComment, bool generateDynamicParameters)
            return commandMetadata.GetProxyCommand(helpComment, generateDynamicParameters);
        /// This method constructs a string representing the CmdletBinding attribute of the command
        /// specified by <paramref name="commandMetadata"/>.
        /// A string representing the CmdletBinding attribute of the command.
        public static string GetCmdletBindingAttribute(CommandMetadata commandMetadata)
            return commandMetadata.GetDecl();
        /// This method constructs a string representing the param block of the command
        /// specified by <paramref name="commandMetadata"/>.  The returned string only contains the
        /// parameters, it is not enclosed in "param()".
        /// A string representing the parameters of the command.
        public static string GetParamBlock(CommandMetadata commandMetadata)
            return commandMetadata.GetParamBlock();
        /// This method constructs a string representing the begin block of the command
        /// script, it is not enclosed in "begin { }".
        /// A string representing the begin block of the command.
        public static string GetBegin(CommandMetadata commandMetadata)
            return commandMetadata.GetBeginBlock();
        /// This method constructs a string representing the process block of the command
        /// script, it is not enclosed in "process { }".
        /// A string representing the process block of the command.
        public static string GetProcess(CommandMetadata commandMetadata)
            return commandMetadata.GetProcessBlock();
        /// This method constructs a string representing the dynamic parameter block of the command
        /// script, it is not enclosed in "dynamicparam { }".
        /// A string representing the dynamic parameter block of the command.
        public static string GetDynamicParam(CommandMetadata commandMetadata)
            return commandMetadata.GetDynamicParamBlock();
        /// This method constructs a string representing the end block of the command
        /// script, it is not enclosed in "end { }".
        /// A string representing the end block of the command.
        public static string GetEnd(CommandMetadata commandMetadata)
            return commandMetadata.GetEndBlock();
        /// This method constructs a string representing the clean block of the command
        /// specified by <paramref name="commandMetadata"/>. The returned string only contains the
        /// script, it is not enclosed in "clean { }".
        /// If <paramref name="commandMetadata"/> is null.
        public static string GetClean(CommandMetadata commandMetadata)
            return commandMetadata.GetCleanBlock();
        private static T GetProperty<T>(PSObject obj, string property) where T : class
            T result = null;
            if (obj != null && obj.Properties[property] != null)
                result = obj.Properties[property].Value as T;
        private static string GetObjText(object obj)
            string text = null;
            PSObject psobj = obj as PSObject;
            if (psobj != null)
                text = GetProperty<string>(psobj, "Text");
            return text ?? obj.ToString();
        private static void AppendContent(StringBuilder sb, string section, object obj)
                string text = GetObjText(obj);
                if (!string.IsNullOrEmpty(text))
                    sb.Append(section);
                    sb.Append("\n\n");
                    sb.Append(text);
        private static void AppendContent(StringBuilder sb, string section, PSObject[] array)
                foreach (PSObject obj in array)
        private static void AppendType(StringBuilder sb, string section, PSObject parent)
            PSObject type = GetProperty<PSObject>(parent, "type");
            PSObject name = GetProperty<PSObject>(type, "name");
            if (name != null)
                sb.Append(GetObjText(name));
                PSObject uri = GetProperty<PSObject>(type, "uri");
                if (uri != null)
                    sb.Append(GetObjText(uri));
        /// Construct the text that can be used in a multi-line comment for get-help.
        /// <param name="help">A custom PSObject created by Get-Help.</param>
        /// <returns>A string that can be used as the help comment for script for the input HelpInfo object.</returns>
        /// <exception cref="System.ArgumentNullException">When the help argument is null.</exception>
        /// <exception cref="System.InvalidOperationException">When the help argument is not recognized as a HelpInfo object.</exception>
        public static string GetHelpComments(PSObject help)
            ArgumentNullException.ThrowIfNull(help);
            bool isHelpObject = false;
            foreach (string typeName in help.InternalTypeNames)
                if (typeName.Contains("HelpInfo"))
                    isHelpObject = true;
            if (!isHelpObject)
                string error = ProxyCommandStrings.HelpInfoObjectRequired;
            AppendContent(sb, ".SYNOPSIS", GetProperty<string>(help, "Synopsis"));
            AppendContent(sb, ".DESCRIPTION", GetProperty<PSObject[]>(help, "Description"));
            PSObject parameters = GetProperty<PSObject>(help, "Parameters");
            PSObject[] parameter = GetProperty<PSObject[]>(parameters, "Parameter");
                foreach (PSObject param in parameter)
                    PSObject name = GetProperty<PSObject>(param, "Name");
                    PSObject[] description = GetProperty<PSObject[]>(param, "Description");
                    sb.Append("\n.PARAMETER ");
                    sb.Append(name);
                    foreach (PSObject obj in description)
                        string text = GetProperty<string>(obj, "Text") ?? obj.ToString();
            PSObject examples = GetProperty<PSObject>(help, "examples");
            PSObject[] example = GetProperty<PSObject[]>(examples, "example");
            if (example != null)
                foreach (PSObject ex in example)
                    StringBuilder exsb = new StringBuilder();
                    PSObject[] introduction = GetProperty<PSObject[]>(ex, "introduction");
                    if (introduction != null)
                        foreach (PSObject intro in introduction)
                            if (intro != null)
                                exsb.Append(GetObjText(intro));
                    PSObject code = GetProperty<PSObject>(ex, "code");
                    if (code != null)
                        exsb.Append(code.ToString());
                    PSObject[] remarks = GetProperty<PSObject[]>(ex, "remarks");
                    if (remarks != null)
                        exsb.Append('\n');
                            string remarkText = GetProperty<string>(remark, "text");
                            exsb.Append(remarkText);
                    if (exsb.Length > 0)
                        sb.Append("\n\n.EXAMPLE\n\n");
                        sb.Append(exsb);
            PSObject alertSet = GetProperty<PSObject>(help, "alertSet");
            AppendContent(sb, ".NOTES", GetProperty<PSObject[]>(alertSet, "alert"));
            PSObject inputtypes = GetProperty<PSObject>(help, "inputTypes");
            PSObject inputtype = GetProperty<PSObject>(inputtypes, "inputType");
            AppendType(sb, ".INPUTS", inputtype);
            PSObject returnValues = GetProperty<PSObject>(help, "returnValues");
            PSObject returnValue = GetProperty<PSObject>(returnValues, "returnValue");
            AppendType(sb, ".OUTPUTS", returnValue);
            PSObject relatedLinks = GetProperty<PSObject>(help, "relatedLinks");
            PSObject[] navigationLink = GetProperty<PSObject[]>(relatedLinks, "navigationLink");
            if (navigationLink != null)
                foreach (PSObject link in navigationLink)
                    // Most likely only one of these will append anything, but it
                    // isn't wrong to append them both.
                    AppendContent(sb, ".LINK", GetProperty<PSObject>(link, "uri"));
                    AppendContent(sb, ".LINK", GetProperty<PSObject>(link, "linkText"));
            AppendContent(sb, ".COMPONENT", GetProperty<PSObject>(help, "Component"));
            AppendContent(sb, ".ROLE", GetProperty<PSObject>(help, "Role"));
            AppendContent(sb, ".FUNCTIONALITY", GetProperty<PSObject>(help, "Functionality"));
