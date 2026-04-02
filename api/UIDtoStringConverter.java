import org.openhab.core.thing.UID;
 * A {@link UIDtoStringConverter} is used to create {@link UID} string
 * representations from an input string and vice versa. If a segment of the
 * parsed {@link UID} string doesn't match the ID rule, it will be escaped.
public class UIDtoStringConverter implements IValueConverter<String> {
    private static final String SEPARATOR = ":";
    public String toValue(final String string, INode node) throws ValueConverterException {
        String[] ids = string.split(SEPARATOR);
        for (int i = 0; i < ids.length; i++) {
            String id = ids[i];
            if (id != null && id.startsWith("\"") && id.endsWith("\"")) {
                    ids[i] = Strings.convertFromJavaString(id.substring(1, id.length() - 1), true);
        return String.join(SEPARATOR, ids);
        String[] ids = value.split(SEPARATOR);
            if (id != null && !id.matches("[A-Za-z0-9_]*")) {
                // string escapes each segment which doesn't match:
                // terminal ID: '^'?('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'_'|'0'..'9')*;
                ids[i] = toEscapedString(id);
