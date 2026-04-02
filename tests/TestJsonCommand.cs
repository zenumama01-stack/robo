using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
    /// This class implements Test-Json command.
    [Cmdlet(VerbsDiagnostic.Test, "Json", DefaultParameterSetName = JsonStringParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096609")]
    public class TestJsonCommand : PSCmdlet
        private const string JsonStringParameterSet = "JsonString";
        private const string JsonStringWithSchemaStringParameterSet = "JsonStringWithSchemaString";
        private const string JsonStringWithSchemaFileParameterSet = "JsonStringWithSchemaFile";
        private const string JsonPathParameterSet = "JsonPath";
        private const string JsonPathWithSchemaStringParameterSet = "JsonPathWithSchemaString";
        private const string JsonPathWithSchemaFileParameterSet = "JsonPathWithSchemaFile";
        private const string JsonLiteralPathParameterSet = "JsonLiteralPath";
        private const string JsonLiteralPathWithSchemaStringParameterSet = "JsonLiteralPathWithSchemaString";
        private const string JsonLiteralPathWithSchemaFileParameterSet = "JsonLiteralPathWithSchemaFile";
        #region Json Document Option Constants
        private const string IgnoreCommentsOption = "IgnoreComments";
        private const string AllowTrailingCommasOption = "AllowTrailingCommas";
        /// Gets or sets JSON string to be validated.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = JsonStringParameterSet)]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = JsonStringWithSchemaStringParameterSet)]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = JsonStringWithSchemaFileParameterSet)]
        public string Json { get; set; }
        /// Gets or sets JSON file path to be validated.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = JsonPathParameterSet)]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = JsonPathWithSchemaStringParameterSet)]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = JsonPathWithSchemaFileParameterSet)]
        /// Gets or sets JSON literal file path to be validated.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = JsonLiteralPathParameterSet)]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = JsonLiteralPathWithSchemaStringParameterSet)]
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = JsonLiteralPathWithSchemaFileParameterSet)]
                return _isLiteralPath ? Path : null;
        /// Gets or sets schema to validate the JSON against.
        /// This is optional parameter.
        /// If the parameter is absent the cmdlet only attempts to parse the JSON string.
        /// If the parameter present the cmdlet attempts to parse the JSON string and
        /// then validates the JSON against the schema. Before testing the JSON string,
        /// the cmdlet parses the schema doing implicitly check the schema too.
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = JsonStringWithSchemaStringParameterSet)]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = JsonPathWithSchemaStringParameterSet)]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = JsonLiteralPathWithSchemaStringParameterSet)]
        public string Schema { get; set; }
        /// Gets or sets path to the file containing schema to validate the JSON string against.
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = JsonStringWithSchemaFileParameterSet)]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = JsonPathWithSchemaFileParameterSet)]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = JsonLiteralPathWithSchemaFileParameterSet)]
        public string SchemaFile { get; set; }
        /// Gets or sets JSON document options.
        [ValidateSet(IgnoreCommentsOption, AllowTrailingCommasOption)]
        public string[] Options { get; set; } = Array.Empty<string>();
        private JsonSchema _jschema;
        private JsonDocumentOptions _documentOptions;
        /// Prepare a JSON schema.
            // By default, a JSON Schema implementation isn't supposed to automatically fetch content.
            // Instead JsonSchema.Net has been set up with a registry so that users can pre-register
            // any schemas they may need to resolve.
            // However, pre-registering schemas doesn't make sense in the context of a Powershell command,
            // and automatically fetching referenced URIs is likely the preferred behavior.  To do that,
            // this property must be set with a method to retrieve and deserialize the content.
            // For more information, see https://json-everything.net/json-schema#automatic-resolution
            SchemaRegistry.Global.Fetch = static uri =>
                    string text;
                    switch (uri.Scheme)
                        case "http":
                        case "https":
                                using var client = new HttpClient();
                                text = client.GetStringAsync(uri).Result;
                        case "file":
                            var filename = Uri.UnescapeDataString(uri.AbsolutePath);
                            text = File.ReadAllText(filename);
                            throw new FormatException(string.Format(TestJsonCmdletStrings.InvalidUriScheme, uri.Scheme));
                    return JsonSerializer.Deserialize<JsonSchema>(text);
                    throw new JsonSchemaReferenceResolutionException(e);
            string resolvedpath = string.Empty;
                if (Schema != null)
                        _jschema = JsonSchema.FromText(Schema);
                    catch (JsonException e)
                        Exception exception = new(TestJsonCmdletStrings.InvalidJsonSchema, e);
                        WriteError(new ErrorRecord(exception, "InvalidJsonSchema", ErrorCategory.InvalidData, Schema));
                else if (SchemaFile != null)
                        resolvedpath = Context.SessionState.Path.GetUnresolvedProviderPathFromPSPath(SchemaFile);
                        _jschema = JsonSchema.FromFile(resolvedpath);
                        WriteError(new ErrorRecord(exception, "InvalidJsonSchema", ErrorCategory.InvalidData, SchemaFile));
            catch (Exception e) when (
                // Handle exceptions related to file access to provide more specific error message
                // https://learn.microsoft.com/dotnet/standard/io/handling-io-errors
                e is IOException ||
                e is UnauthorizedAccessException ||
                e is NotSupportedException ||
                e is SecurityException
                Exception exception = new(
                        TestJsonCmdletStrings.JsonSchemaFileOpenFailure,
                        resolvedpath),
                ThrowTerminatingError(new ErrorRecord(exception, "JsonSchemaFileOpenFailure", ErrorCategory.OpenError, resolvedpath));
                ThrowTerminatingError(new ErrorRecord(exception, "InvalidJsonSchema", ErrorCategory.InvalidData, resolvedpath));
            _documentOptions = new JsonDocumentOptions
                CommentHandling = Options.Contains(IgnoreCommentsOption, StringComparer.OrdinalIgnoreCase)
                    ? JsonCommentHandling.Skip
                    : JsonCommentHandling.Disallow,
                AllowTrailingCommas = Options.Contains(AllowTrailingCommasOption, StringComparer.OrdinalIgnoreCase)
        /// Validate a JSON.
            string jsonToParse = string.Empty;
            if (Json != null)
                jsonToParse = Json;
            else if (Path != null)
                string resolvedPath = PathUtils.ResolveFilePath(Path, this, _isLiteralPath);
                if (!File.Exists(resolvedPath))
                    ItemNotFoundException exception = new(
                        Path,
                    ThrowTerminatingError(exception.ErrorRecord);
                jsonToParse = File.ReadAllText(resolvedPath);
                var parsedJson = JsonNode.Parse(jsonToParse, nodeOptions: null, _documentOptions);
                if (_jschema != null)
                    EvaluationResults evaluationResults = _jschema.Evaluate(parsedJson, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });
                    result = evaluationResults.IsValid;
                        ReportValidationErrors(evaluationResults);
            catch (JsonSchemaReferenceResolutionException jsonExc)
                Exception exception = new(TestJsonCmdletStrings.InvalidJsonSchema, jsonExc);
                WriteError(new ErrorRecord(exception, "InvalidJsonSchema", ErrorCategory.InvalidData, _jschema));
                Exception exception = new(TestJsonCmdletStrings.InvalidJson, exc);
                WriteError(new ErrorRecord(exception, "InvalidJson", ErrorCategory.InvalidData, Json));
        /// Recursively reports validation errors from hierarchical evaluation results.
        /// Skips nodes (and their children) where IsValid is true to avoid false positives
        /// from constructs like OneOf or AnyOf.
        /// <param name="evaluationResult">The evaluation result to process.</param>
        private void ReportValidationErrors(EvaluationResults evaluationResult)
            // Skip this node and all children if validation passed
            if (evaluationResult.IsValid)
            // Report errors at this level
            HandleValidationErrors(evaluationResult);
            // Recursively process child results
            if (evaluationResult.HasDetails)
                foreach (var nestedResult in evaluationResult.Details)
                    ReportValidationErrors(nestedResult);
        private void HandleValidationErrors(EvaluationResults evaluationResult)
            if (!evaluationResult.HasErrors)
            foreach (var error in evaluationResult.Errors!)
                Exception exception = new(string.Format(TestJsonCmdletStrings.InvalidJsonAgainstSchemaDetailed, error.Value, evaluationResult.InstanceLocation));
                ErrorRecord errorRecord = new(exception, "InvalidJsonAgainstSchemaDetailed", ErrorCategory.InvalidData, null);
