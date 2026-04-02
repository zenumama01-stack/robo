/// Thrown during evaluation of <see cref="TestJsonCommand"/> when an attempt
/// to resolve a <code>$ref</code> or <code>$dynamicRef</code> fails.
internal sealed class JsonSchemaReferenceResolutionException : Exception
    /// Initializes a new instance of the <see cref="JsonSchemaReferenceResolutionException"/> class.
    /// The exception that is the cause of the current exception, or a null reference
    /// (<code>Nothing</code> in Visual Basic) if no inner exception is specified.
    public JsonSchemaReferenceResolutionException(Exception innerException)
        : base(message: null, innerException)
