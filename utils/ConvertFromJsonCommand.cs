    /// The ConvertFrom-Json command.
    /// This command converts a Json string representation to a JsonObject.
    [Cmdlet(VerbsData.ConvertFrom, "Json", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096606", RemotingCapability = RemotingCapability.None)]
    public class ConvertFromJsonCommand : Cmdlet
        /// Gets or sets the InputString property.
        /// InputObjectBuffer buffers all InputObject contents available in the pipeline.
        private readonly List<string> _inputObjectBuffer = new();
        /// Returned data structure is a Hashtable instead a CustomPSObject.
        public SwitchParameter AsHashtable { get; set; }
        /// Gets or sets the maximum depth the JSON input is allowed to have. By default, it is 1024.
        public int Depth { get; set; } = 1024;
        /// Gets or sets the switch to prevent ConvertFrom-Json from unravelling collections during deserialization, instead passing them as a single
        /// object through the pipeline.
        /// Gets or sets the switch to control how DateTime values are to be parsed as a dotnet object.
        public JsonDateKind DateKind { get; set; } = JsonDateKind.Default;
        /// Buffers InputObjet contents available in the pipeline.
        /// The main execution method for the ConvertFrom-Json command.
            // When Input is provided through pipeline, the input can be represented in the following two ways:
            // 1. Each input in the collection is a complete Json content. There can be multiple inputs of this format.
            // 2. The complete input is a collection which represents a single Json content. This is typically the majority of the case.
            if (_inputObjectBuffer.Count > 0)
                if (_inputObjectBuffer.Count == 1)
                    ConvertFromJsonHelper(_inputObjectBuffer[0]);
                    bool successfullyConverted = false;
                        // Try to deserialize the first element.
                        successfullyConverted = ConvertFromJsonHelper(_inputObjectBuffer[0]);
                        // The first input string does not represent a complete Json Syntax.
                        // Hence consider the entire input as a single Json content.
                    if (successfullyConverted)
                        for (int index = 1; index < _inputObjectBuffer.Count; index++)
                            ConvertFromJsonHelper(_inputObjectBuffer[index]);
                        // Process the entire input as a single Json content.
                        ConvertFromJsonHelper(string.Join(System.Environment.NewLine, _inputObjectBuffer.ToArray()));
        /// ConvertFromJsonHelper is a helper method to convert to Json input to .Net Type.
        /// <param name="input">Input string.</param>
        /// <returns>True if successfully converted, else returns false.</returns>
        private bool ConvertFromJsonHelper(string input)
            object result = JsonObject.ConvertFromJson(input, AsHashtable.IsPresent, Depth, DateKind, out error);
            WriteObject(result, !NoEnumerate.IsPresent);
            return (result != null);
