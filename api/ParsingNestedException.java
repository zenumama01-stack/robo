 * This class extends the {@link Exception} class functionality with keeping additional information about reasons for
 * exception during the parsing process.
public class ParsingNestedException extends Exception {
    public static final int MODULE_TYPE = 1;
    public static final int TEMPLATE = 2;
    public static final int RULE = 3;
     * Keeps information about the UID of the automation object for parsing - module type, template or rule.
    private final @Nullable String id;
     * Keeps information about the type of the automation object for parsing - module type, template or rule.
    private final int type;
     * Creates an exception based on exception thrown the parsing plus information about the type of the automation
     * object, its UID and additional message with additional information about the parsing process.
     * @param type is the type of the automation object for parsing.
     * @param id is the UID of the automation object for parsing.
     * @param msg is the additional message with additional information about the parsing process.
     * @param t is the exception thrown during the parsing.
    public ParsingNestedException(int type, @Nullable String id, String msg, @Nullable Throwable t) {
        super(msg, t);
     * Creates an exception based on exception thrown during the parsing plus information about the type of the
     * automation object and its UID.
    public ParsingNestedException(int type, @Nullable String id, @Nullable Throwable t) {
        super(t);
            case MODULE_TYPE:
                sb.append("[Module Type");
            case TEMPLATE:
                sb.append("[Template");
            case RULE:
                sb.append("[Rule");
            sb.append(" " + id);
        sb.append("] " + super.getMessage());
