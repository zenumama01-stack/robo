import org.eclipse.xtext.util.LineAndColumn;
 * A detailed error information for a script
 * @see ScriptException
 * @see ScriptExecutionException
 * @see ScriptParsingException
public final class ScriptError {
    private final int column;
    private final int line;
    // TODO Internationalize! Not an Error string, but a key...
    private final String message;
     * Creates new ScriptError.
     * @param message Error Message
     * @param line Line number, or -1 if unknown
     * @param column Column number, or -1 if unknown
     * @param length Length, or -1 if unknown
    public ScriptError(final String message, final int line, final int column, final int length) {
        this.line = line;
     * This constructor uses the given EObject instance to calculate the exact position.
     * @param atEObject the EObject instance to use for calculating the position
    public ScriptError(final String message, final EObject atEObject) {
        INode node = NodeModelUtils.getNode(atEObject);
        if (node == null) {
            this.line = 0;
            this.column = 0;
            this.length = -1;
            LineAndColumn lac = NodeModelUtils.getLineAndColumn(node, node.getOffset());
            this.line = lac.getLine();
            this.column = lac.getColumn();
            this.length = node.getEndOffset() - node.getOffset();
     * Returns a message containing the String passed to a constructor as well as line and column numbers if any of
     * these are known.
     * @return The error message.
    public String getMessage() {
        StringBuilder sb = new StringBuilder(message);
        if (line != -1) {
            sb.append("; line ");
            sb.append(line);
        if (column != -1) {
            sb.append(", column ");
            sb.append(column);
        if (length != -1) {
            sb.append(", length ");
            sb.append(length);
     * Get the line number on which an error occurred.
     * @return The line number. Returns -1 if a line number is unavailable.
    public int getLineNumber() {
     * Get the column number on which an error occurred.
     * @return The column number. Returns -1 if a column number is unavailable.
    public int getColumnNumber() {
        return column;
     * Get the number of columns affected by the error.
     * @return The number of columns. Returns -1 if unavailable.
    public int getLength() {
