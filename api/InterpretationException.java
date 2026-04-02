 * An exception used by {@link HumanLanguageInterpreter}s, if an error occurs.
public class InterpretationException extends Exception {
    private static final long serialVersionUID = 76120119745036525L;
     * Constructs a new interpretation exception.
     * @param msg the textual response. Should be short, localized and understandable by non-technical users.
    public InterpretationException(String msg) {
