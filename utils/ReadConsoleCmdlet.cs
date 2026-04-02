    /// Retrieves input from the host virtual console and writes it to the pipeline output.
    [Cmdlet(VerbsCommunications.Read, "Host", DefaultParameterSetName = "AsString", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096610")]
    [OutputType(typeof(string), typeof(SecureString))]
    public sealed class ReadHostCommand : PSCmdlet
        /// Initializes a new instance of the <see cref="ReadHostCommand"/> class.
        ReadHostCommand()
            // empty, provided per design guidelines.
        /// The objects to display on the host before collecting input.
        [Parameter(Position = 0, ValueFromRemainingArguments = true)]
        object
        Prompt
                return _prompt;
                _prompt = value;
        /// Gets or sets to no echo the input as is typed. If set then the cmdlet returns a secure string.
        [Parameter(ParameterSetName = "AsSecureString")]
        SwitchParameter
        AsSecureString
                return _safe;
                _safe = value;
        /// Gets or sets whether the console will echo the input as is typed. If set then the cmdlet returns a regular string.
        MaskInput
        /// Write the prompt, then collect a line of input from the host, then output it to the output stream.
            PSHostUserInterface ui = Host.UI;
            string promptString;
            if (_prompt != null)
                IEnumerator e = LanguagePrimitives.GetEnumerator(_prompt);
                if (e != null)
                    while (e.MoveNext())
                        // The current object might itself be a collection, like a string array, as in read/console "foo","bar","baz"
                        // If it is, then the PSObject ToString() will take care of it.  We could go on unwrapping collections
                        // forever, but it's a pretty common use case to see a varags confused with an array.
                        string element = (string)LanguagePrimitives.ConvertTo(e.Current, typeof(string), CultureInfo.InvariantCulture);
                        if (!string.IsNullOrEmpty(element))
                            // Prepend a space if the stringbuilder isn't empty...
                            // We could consider using $OFS here but that's probably more
                            // effort than is really needed...
                            if (sb.Length > 0)
                                sb.Append(' ');
                            sb.Append(element);
                    promptString = sb.ToString();
                    promptString = (string)LanguagePrimitives.ConvertTo(_prompt, typeof(string), CultureInfo.InvariantCulture);
                FieldDescription fd = new(promptString);
                if (AsSecureString || MaskInput)
                    fd.SetParameterType(typeof(SecureString));
                    fd.SetParameterType(typeof(string));
                fdc.Add(fd);
                Dictionary<string, PSObject> result = Host.UI.Prompt(string.Empty, string.Empty, fdc);
                // Result can be null depending on the host implementation. One typical
                // example of a null return is for a canceled dialog.
                    foreach (PSObject o in result.Values)
                        if (MaskInput && o?.BaseObject is SecureString secureString)
                            WriteObject(Utils.GetStringFromSecureString(secureString));
                            WriteObject(o);
                    result = Host.UI.ReadLineAsSecureString();
                    result = Host.UI.ReadLine();
                if (MaskInput)
                    WriteObject(Utils.GetStringFromSecureString((SecureString)result));
        private object _prompt = null;
        private bool _safe = false;
