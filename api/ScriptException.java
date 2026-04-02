 * Abstract class for exceptions thrown by the script engine.
public abstract class ScriptException extends Exception {
    private static final long serialVersionUID = -4155948282895039148L;
    private String scriptText;
    private final List<ScriptError> errors;
    protected ScriptException(String message) {
        this.errors = new ArrayList<>(1);
        errors.add(new ScriptError(message, 0, 0, -1));
    protected ScriptException(ScriptError scriptError) {
        super(scriptError.getMessage());
        errors.add(scriptError);
     * @param cause
    protected ScriptException(final String message, final String scriptText, final Throwable cause) {
        this.scriptText = scriptText;
        this.errors = new LinkedList<>();
    protected ScriptException(final String message, final String scriptText) {
    public ScriptException(final String message, final String scriptText, final int line, final int column,
            final int length) {
        this(scriptText, new ScriptError(message, line, column, length));
    public ScriptException(final Throwable cause, final String message, final String scriptText, final int line,
            final int column, final int length) {
        this(cause, scriptText, new ScriptError(message, line, column, length));
    private ScriptException(final Throwable cause, final String scriptText, final ScriptError error) {
        super(error.getMessage(), cause); // ?
        errors.add(error);
     * Creates a ScriptException with one Error.
    private ScriptException(final String scriptText, final ScriptError error) {
        super(error.getMessage()); // ?
    public ScriptException(String message, Throwable cause) {
        this.errors = new ArrayList<>(0);
     * All Errors that lead to this Exception.
     * @return List of Error. Size >= 1, there is at last one ScriptError.
    public List<ScriptError> getErrors() {
    public void setScriptText(final String scriptText) {
     * Returns a concatenation of all errors in contained ScriptError instances.
     * Separated by newline, except for last error; no \n if only one error.
     * @return The Message.
     * @see ScriptError#getMessage()
        if (scriptText == null) {
            sb.append(super.getMessage());
            int l = 1;
            int c = 0;
            for (int x = 0; x < scriptText.length(); x++) {
                if (hasMatchingError(l, c)) {
                    sb.append(" ___ ");
                sb.append(scriptText.charAt(x));
                if (scriptText.charAt(x) == '\n') {
                    ++l;
                    ++c;
            for (ScriptError e : getErrors()) {
                if (!sb.isEmpty()) {
                    sb.append('\n');
                sb.append("   ");
                if (getErrors().size() > 1) {
                    sb.append(i++);
                    sb.append(". ");
                sb.append(e.getMessage());
    private boolean hasMatchingError(int l, int c) {
            if (e.getLineNumber() == l && e.getColumnNumber() == c) {
