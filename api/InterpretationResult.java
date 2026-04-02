 * Bundles results of an interpretation. Represents final outcome and user feedback. This class is immutable.
public final class InterpretationResult {
     * Represents successful parsing and interpretation.
    public static final InterpretationResult OK = new InterpretationResult(true, "");
     * Represents a syntactical problem during parsing.
    public static final InterpretationResult SYNTAX_ERROR = new InterpretationResult(false, "Syntax error.");
     * Represents a problem in the interpretation step after successful parsing.
    public static final InterpretationResult SEMANTIC_ERROR = new InterpretationResult(false, "Semantic error.");
    private @Nullable InterpretationException exception;
    private String response = "";
     * Constructs a successful result.
     * @param response the textual response. Should be short, localized and understandable by non-technical users.
    public InterpretationResult(String response) {
     * Constructs an unsuccessful result.
     * @param exception the responsible exception
    public InterpretationResult(InterpretationException exception) {
        this.exception = exception;
        this.success = false;
     * Constructs a result.
     * @param success if the result represents a successful or unsuccessful interpretation
    public InterpretationResult(boolean success, String response) {
     * @return if interpretation was successful
     * @return the exception
    public @Nullable InterpretationException getException() {
     * @return the response
    public String getResponse() {
