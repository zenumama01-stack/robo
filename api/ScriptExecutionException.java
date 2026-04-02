 * Exception that is thrown on errors during script execution.
public class ScriptExecutionException extends ScriptException {
    private static final long serialVersionUID = 149490362444673405L;
    public ScriptExecutionException(final String message, final int line, final int column, final int length) {
        super(message, null, line, column, length);
    public ScriptExecutionException(final ScriptError scriptError) {
        super(scriptError);
    public ScriptExecutionException(final String message, final Throwable cause, final int line, final int column,
        super(cause, message, null, line, column, length);
    public ScriptExecutionException(final String message) {
    public ScriptExecutionException(String message, Throwable exception) {
        super(message, exception);
