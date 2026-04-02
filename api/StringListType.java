 * This type can be used for items that are dealing with telephony functionality.
public class StringListType implements Command, State {
    protected final List<String> typeDetails;
    public static final String DELIMITER = ",";
    public static final String ESCAPED_DELIMITER = "\\" + DELIMITER;
    public static final String REGEX_SPLITTER = "(?<!\\\\)" + DELIMITER;
    public StringListType() {
        typeDetails = List.of();
    public StringListType(List<String> rows) {
        typeDetails = List.copyOf(rows);
    public StringListType(StringType... rows) {
        typeDetails = Arrays.stream(rows).map(StringType::toString).toList();
    public StringListType(String... rows) {
        typeDetails = List.of(rows);
     * Deserialize the input string, splitting it on every delimiter not preceded by a backslash.
    public StringListType(String serialized) {
        typeDetails = Arrays.stream(serialized.split(REGEX_SPLITTER, -1))
                .map(s -> s.replace(ESCAPED_DELIMITER, DELIMITER)).toList();
    public String getValue(final int index) {
        if (index < 0 || index >= typeDetails.size()) {
            throw new IllegalArgumentException("Index is out of range");
        return typeDetails.get(index);
     * Formats the value of this type according to a pattern (@see
     * {@link java.util.Formatter}). One single value of this type can be referenced
     * by the pattern using an index. The item order is defined by the natural
     * (alphabetical) order of their keys.
     * @param pattern the pattern to use containing indexes to reference the
     *            single elements of this type.
        return String.format(pattern, typeDetails.toArray());
        return typeDetails.stream().map(s -> s.replace(DELIMITER, ESCAPED_DELIMITER))
                .collect(Collectors.joining(DELIMITER));
    public static StringListType valueOf(String value) {
        return new StringListType(value);
        result = prime * result + typeDetails.hashCode();
        StringListType other = (StringListType) obj;
        return typeDetails.equals(other.typeDetails);
