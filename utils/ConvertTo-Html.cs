    [Cmdlet(VerbsData.ConvertTo, "Html", DefaultParameterSetName = "Page",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096595", RemotingCapability = RemotingCapability.None)]
    public sealed
    class ConvertToHtmlCommand : PSCmdlet
        /// <summary>The incoming object</summary>
        [Parameter(ValueFromPipeline = true)]
        /// These take the form of a PSPropertyExpression.
        public object[] Property
        private object[] _property;
        /// Text to go after the opening body tag and before the table.
        [Parameter(ParameterSetName = "Page", Position = 3)]
        public string[] Body
                return _body;
                _body = value;
        private string[] _body;
        /// Text to go into the head section of the html doc.
        [Parameter(ParameterSetName = "Page", Position = 1)]
        public string[] Head
                return _head;
                _head = value;
        private string[] _head;
        /// The string for the title tag
        /// The title is also placed in the body of the document
        /// before the table between h3 tags
        /// If the -Head parameter is used, this parameter has no
        /// effect.
        [Parameter(ParameterSetName = "Page", Position = 2)]
                return _title;
                _title = value;
        private string _title = "HTML TABLE";
        /// This specifies whether the objects should
        /// be rendered as an HTML TABLE or
        /// HTML LIST.
        [ValidateSet("Table", "List")]
        public string As
                return _as;
                _as = value;
        private string _as = "Table";
        /// This specifies a full or partial URI
        /// for the CSS information.
        /// The HTML should reference the CSS file specified.
        [Parameter(ParameterSetName = "Page")]
        [Alias("cu", "uri")]
        public Uri CssUri
                return _cssuri;
                _cssuri = value;
                _cssuriSpecified = true;
        private Uri _cssuri;
        private bool _cssuriSpecified;
        /// When this switch is specified generate only the
        /// HTML representation of the incoming object
        /// without the HTML,HEAD,TITLE,BODY,etc tags.
        [Parameter(ParameterSetName = "Fragment")]
        public SwitchParameter Fragment
                return _fragment;
                _fragment = value;
        private SwitchParameter _fragment;
        /// Specifies the text to include prior the closing body tag of the HTML output.
        public string[] PostContent
                return _postContent;
                _postContent = value;
        private string[] _postContent;
        /// Specifies the text to include after the body tag of the HTML output.
        public string[] PreContent
                return _preContent;
                _preContent = value;
        private string[] _preContent;
        /// Sets and Gets the meta property of the HTML head.
        public Hashtable Meta
                return _meta;
                _meta = value;
                _metaSpecified = true;
        private Hashtable _meta;
        private bool _metaSpecified = false;
        /// Specifies the charset encoding for the HTML document.
        [ValidatePattern("^[A-Za-z0-9]\\w+\\S+[A-Za-z0-9]$")]
        public string Charset
                return _charset;
                _charset = value;
                _charsetSpecified = true;
        private string _charset;
        private bool _charsetSpecified = false;
        /// When this switch statement is specified,
        /// it will change the DOCTYPE to XHTML Transitional DTD.
        public SwitchParameter Transitional
                return _transitional;
                _transitional = true;
        private bool _transitional = false;
        /// Definitions for hash table keys.
        internal static class ConvertHTMLParameterDefinitionKeys
            internal const string LabelEntryKey = "label";
            internal const string AlignmentEntryKey = "alignment";
            internal const string WidthEntryKey = "width";
        /// This allows for @{e='foo';label='bar';alignment='center';width='20'}.
        internal sealed class ConvertHTMLExpressionParameterDefinition : CommandParameterDefinition
            protected override void SetEntries()
                this.hashEntries.Add(new ExpressionEntryDefinition());
                this.hashEntries.Add(new LabelEntryDefinition());
                this.hashEntries.Add(new HashtableEntryDefinition(ConvertHTMLParameterDefinitionKeys.AlignmentEntryKey, new[] { typeof(string) }));
                // Note: We accept "width" as either string or int.
                this.hashEntries.Add(new HashtableEntryDefinition(ConvertHTMLParameterDefinitionKeys.WidthEntryKey, new[] { typeof(string), typeof(int) }));
        /// Create a list of MshParameter from properties.
        /// <param name="properties">Can be a string, ScriptBlock, or Hashtable.</param>
        private List<MshParameter> ProcessParameter(object[] properties)
            TerminatingErrorContext invocationContext = new(this);
            ParameterProcessor processor =
                new(new ConvertHTMLExpressionParameterDefinition());
            properties ??= new object[] { "*" };
            return processor.ProcessParameters(properties, invocationContext);
        /// Resolve all wildcards in user input Property into resolvedNameMshParameters.
        private void InitializeResolvedNameMshParameters()
            // temp list of properties with wildcards resolved
            var resolvedNameProperty = new List<object>();
            foreach (MshParameter p in _propertyMshParameterList)
                string label = p.GetEntry(ConvertHTMLParameterDefinitionKeys.LabelEntryKey) as string;
                string alignment = p.GetEntry(ConvertHTMLParameterDefinitionKeys.AlignmentEntryKey) as string;
                // Accept the width both as a string and as an int.
                string width;
                int? widthNum = p.GetEntry(ConvertHTMLParameterDefinitionKeys.WidthEntryKey) as int?;
                width = widthNum != null ? widthNum.Value.ToString() : p.GetEntry(ConvertHTMLParameterDefinitionKeys.WidthEntryKey) as string;
                PSPropertyExpression ex = p.GetEntry(FormatParameterDefinitionKeys.ExpressionEntryKey) as PSPropertyExpression;
                List<PSPropertyExpression> resolvedNames = ex.ResolveNames(_inputObject);
                foreach (PSPropertyExpression resolvedName in resolvedNames)
                    Hashtable ht = CreateAuxPropertyHT(label, alignment, width);
                    if (resolvedName.Script != null)
                        // The argument is a calculated property whose value is calculated by a script block.
                        ht.Add(FormatParameterDefinitionKeys.ExpressionEntryKey, resolvedName.Script);
                        ht.Add(FormatParameterDefinitionKeys.ExpressionEntryKey, resolvedName.ToString());
                    resolvedNameProperty.Add(ht);
            _resolvedNameMshParameters = ProcessParameter(resolvedNameProperty.ToArray());
        private static Hashtable CreateAuxPropertyHT(
            string label,
            string alignment,
            string width)
            Hashtable ht = new();
            if (label != null)
                ht.Add(ConvertHTMLParameterDefinitionKeys.LabelEntryKey, label);
            if (alignment != null)
                ht.Add(ConvertHTMLParameterDefinitionKeys.AlignmentEntryKey, alignment);
            if (width != null)
                ht.Add(ConvertHTMLParameterDefinitionKeys.WidthEntryKey, width);
            return ht;
        /// Calls ToString. If an exception occurs, eats it and return string.Empty.
        private static string SafeToString(object obj)
                return obj.ToString();
                // eats exception if safe
            // ValidateNotNullOrEmpty attribute is not working for System.Uri datatype, so handling it here
            if ((_cssuriSpecified) && (string.IsNullOrEmpty(_cssuri.OriginalString.Trim())))
                ArgumentException ex = new(StringUtil.Format(UtilityCommonStrings.EmptyCSSUri, "CSSUri"));
                ErrorRecord er = new(ex, "ArgumentException", ErrorCategory.InvalidArgument, "CSSUri");
            _propertyMshParameterList = ProcessParameter(_property);
            if (!string.IsNullOrEmpty(_title))
                WebUtility.HtmlEncode(_title);
            // This first line ensures w3c validation will succeed. However we are not specifying
            // an encoding in the HTML because we don't know where the text will be written and
            // if a particular encoding will be used.
            if (!_fragment)
                if (!_transitional)
                    WriteObject("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"  \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">");
                    WriteObject("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"  \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
                WriteObject("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
                WriteObject("<head>");
                if (_charsetSpecified)
                    WriteObject("<meta charset=\"" + _charset + "\">");
                if (_metaSpecified)
                    List<string> useditems = new();
                    foreach (string s in _meta.Keys)
                        if (!useditems.Contains(s))
                            switch (s.ToLower())
                                case "content-type":
                                case "default-style":
                                case "x-ua-compatible":
                                    WriteObject("<meta http-equiv=\"" + s + "\" content=\"" + _meta[s] + "\">");
                                case "application-name":
                                case "author":
                                case "description":
                                case "generator":
                                case "keywords":
                                case "viewport":
                                    WriteObject("<meta name=\"" + s + "\" content=\"" + _meta[s] + "\">");
                                    MshCommandRuntime mshCommandRuntime = this.CommandRuntime as MshCommandRuntime;
                                    string Message = StringUtil.Format(ConvertHTMLStrings.MetaPropertyNotFound, s, _meta[s]);
                                    WarningRecord record = new(Message);
                                    if (GetVariableValue(SpecialVariables.MyInvocation) is InvocationInfo invocationInfo)
                                        record.SetInvocationInfo(invocationInfo);
                                    mshCommandRuntime.WriteWarning(record);
                            useditems.Add(s);
                WriteObject(_head ?? new string[] { "<title>" + _title + "</title>" }, true);
                if (_cssuriSpecified)
                    WriteObject("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + _cssuri + "\" />");
                WriteObject("</head><body>");
                if (_body != null)
                    WriteObject(_body, true);
            if (_preContent != null)
                WriteObject(_preContent, true);
            WriteObject("<table>");
            _isTHWritten = false;
        /// Reads Width and Alignment from Property and write Col tags.
        /// <param name="mshParams"></param>
        private void WriteColumns(List<MshParameter> mshParams)
            StringBuilder COLTag = new();
            COLTag.Append("<colgroup>");
            foreach (MshParameter p in mshParams)
                COLTag.Append("<col");
                if (p.GetEntry(ConvertHTMLParameterDefinitionKeys.WidthEntryKey) is string width)
                    COLTag.Append(" width = \"");
                    COLTag.Append(width);
                    COLTag.Append('"');
                if (p.GetEntry(ConvertHTMLParameterDefinitionKeys.AlignmentEntryKey) is string alignment)
                    COLTag.Append(" align = \"");
                    COLTag.Append(alignment);
                COLTag.Append("/>");
            COLTag.Append("</colgroup>");
            // The columngroup and col nodes will be printed in a single line.
            WriteObject(COLTag.ToString());
        /// Writes the list entries when the As parameter has value List.
        private void WriteListEntry()
            foreach (MshParameter p in _resolvedNameMshParameters)
                StringBuilder Listtag = new();
                Listtag.Append("<tr><td>");
                // for writing the property name
                WritePropertyName(Listtag, p);
                Listtag.Append(':');
                Listtag.Append("</td>");
                // for writing the property value
                Listtag.Append("<td>");
                WritePropertyValue(Listtag, p);
                Listtag.Append("</td></tr>");
                WriteObject(Listtag.ToString());
        /// To write the Property name.
        private static void WritePropertyName(StringBuilder Listtag, MshParameter p)
            if (p.GetEntry(ConvertHTMLParameterDefinitionKeys.LabelEntryKey) is string label)
                Listtag.Append(label);
                Listtag.Append(ex.ToString());
        /// To write the Property value.
        private void WritePropertyValue(StringBuilder Listtag, MshParameter p)
            PSPropertyExpression exValue = p.GetEntry(FormatParameterDefinitionKeys.ExpressionEntryKey) as PSPropertyExpression;
            // get the value of the property
            List<PSPropertyExpressionResult> resultList = exValue.GetValues(_inputObject);
            foreach (PSPropertyExpressionResult result in resultList)
                // create comma sep list for multiple results
                if (result.Result != null)
                    string htmlEncodedResult = WebUtility.HtmlEncode(SafeToString(result.Result));
                    Listtag.Append(htmlEncodedResult);
                Listtag.Append(", ");
            if (Listtag.ToString().EndsWith(", ", StringComparison.Ordinal))
                Listtag.Remove(Listtag.Length - 2, 2);
        /// To write the Table header for the object property names.
        private static void WriteTableHeader(StringBuilder THtag, List<MshParameter> resolvedNameMshParameters)
            // write the property names
            foreach (MshParameter p in resolvedNameMshParameters)
                THtag.Append("<th>");
                WritePropertyName(THtag, p);
                THtag.Append("</th>");
        /// To write the Table row for the object property values.
        private void WriteTableRow(StringBuilder TRtag, List<MshParameter> resolvedNameMshParameters)
            // write the property values
                TRtag.Append("<td>");
                WritePropertyValue(TRtag, p);
                TRtag.Append("</td>");
        // count of the objects
        private int _numberObjects = 0;
            // writes the table headers
            // it is not in BeginProcessing because the first inputObject is needed for
            // the number of columns and column name
            if (_inputObject == null || _inputObject == AutomationNull.Value)
            _numberObjects++;
            if (!_isTHWritten)
                InitializeResolvedNameMshParameters();
                if (_resolvedNameMshParameters == null || _resolvedNameMshParameters.Count == 0)
                // if the As parameter is given as List
                if (_as.Equals("List", StringComparison.OrdinalIgnoreCase))
                    // if more than one object,write the horizontal rule to put visual separator
                    if (_numberObjects > 1)
                        WriteObject("<tr><td><hr></td></tr>");
                    WriteListEntry();
                else // if the As parameter is Table, first we have to write the property names
                    WriteColumns(_resolvedNameMshParameters);
                    StringBuilder THtag = new("<tr>");
                    // write the table header
                    WriteTableHeader(THtag, _resolvedNameMshParameters);
                    THtag.Append("</tr>");
                    WriteObject(THtag.ToString());
                    _isTHWritten = true;
            // if the As parameter is Table, write the property values
            if (_as.Equals("Table", StringComparison.OrdinalIgnoreCase))
                StringBuilder TRtag = new("<tr>");
                // write the table row
                WriteTableRow(TRtag, _resolvedNameMshParameters);
                TRtag.Append("</tr>");
                WriteObject(TRtag.ToString());
            // if fragment,end with table
            WriteObject("</table>");
            if (_postContent != null)
                WriteObject(_postContent, true);
            // if not fragment end with body and html also
                WriteObject("</body></html>");
        /// List of incoming objects to compare.
        private bool _isTHWritten;
        private List<MshParameter> _propertyMshParameterList;
        private List<MshParameter> _resolvedNameMshParameters;
        // private string ResourcesBaseName = "ConvertHTMLStrings";
