 * This class extends the {@link Exception} class functionality with functionality serving to accumulate the all
 * exceptions during the parsing process.
@SuppressWarnings("serial")
public class ParsingException extends Exception {
     * Keeps all accumulated exceptions.
    private List<ParsingNestedException> exceptions;
     * Creates the holder for one exception during the parsing process.
     * @param e is an exception during the parsing process.
    public ParsingException(ParsingNestedException e) {
        exceptions = List.of(e);
     * Creates a holder for several exceptions during the parsing process.
     * @param exceptions is a list with exceptions during the parsing process.
    public ParsingException(List<ParsingNestedException> exceptions) {
        this.exceptions = exceptions;
    public @Nullable String getMessage() {
        for (ParsingNestedException e : exceptions) {
            writer.append(e.getMessage() + "\n");
    public StackTraceElement[] getStackTrace() {
        int size = 0;
            size = size + e.getStackTrace().length;
        StackTraceElement[] st = new StackTraceElement[size];
        for (ParsingNestedException exception : exceptions) {
            StackTraceElement[] ste = exception.getStackTrace();
            System.arraycopy(ste, 0, st, index, ste.length);
            index += ste.length;
        return st;
