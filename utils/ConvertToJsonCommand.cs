using Newtonsoft.Json;
    /// The ConvertTo-Json command.
    /// This command converts an object to a Json string representation.
    [Cmdlet(VerbsData.ConvertTo, "Json", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096925", RemotingCapability = RemotingCapability.None)]
    public class ConvertToJsonCommand : PSCmdlet, IDisposable
        /// Gets or sets the InputObject property.
        public object InputObject { get; set; }
        private int _depth = 2;
        private readonly CancellationTokenSource _cancellationSource = new();
        /// Gets or sets the Depth property.
        [ValidateRange(0, 100)]
        /// Gets or sets the Compress property.
        /// If the Compress property is set to be true, the Json string will
        /// be output in the compressed way. Otherwise, the Json string will
        /// be output with indentations.
        public SwitchParameter Compress { get; set; }
        /// Gets or sets the EnumsAsStrings property.
        /// If the EnumsAsStrings property is set to true, enum values will
        /// be converted to their string equivalent. Otherwise, enum values
        /// will be converted to their numeric equivalent.
        public SwitchParameter EnumsAsStrings { get; set; }
        /// Gets or sets the AsArray property.
        /// If the AsArray property is set to be true, the result JSON string will
        /// be returned with surrounding '[', ']' chars. Otherwise,
        /// the array symbols will occur only if there is more than one input object.
        public SwitchParameter AsArray { get; set; }
        /// Specifies how strings are escaped when writing JSON text.
        /// If the EscapeHandling property is set to EscapeHtml, the result JSON string will
        /// be returned with HTML (&lt;, &gt;, &amp;, ', ") and control characters (e.g. newline) are escaped.
        public StringEscapeHandling EscapeHandling { get; set; } = StringEscapeHandling.Default;
                _cancellationSource.Dispose();
        private readonly List<object> _inputObjects = new();
        /// Caching the input objects for the command.
            _inputObjects.Add(InputObject);
        /// Do the conversion to json and write output.
            if (_inputObjects.Count > 0)
                object objectToProcess = (_inputObjects.Count > 1 || AsArray) ? (_inputObjects.ToArray() as object) : _inputObjects[0];
                var context = new JsonObject.ConvertToJsonContext(
                    Depth,
                    EnumsAsStrings.IsPresent,
                    Compress.IsPresent,
                    EscapeHandling,
                    targetCmdlet: this,
                    _cancellationSource.Token);
                // null is returned only if the pipeline is stopping (e.g. ctrl+c is signaled).
                // in that case, we shouldn't write the null to the output pipe.
                string output = JsonObject.ConvertToJson(objectToProcess, in context);
                if (output != null)
        /// Process the Ctrl+C signal.
            _cancellationSource.Cancel();
