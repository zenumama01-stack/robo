 * The {@link ConverterValueMap} reads all children elements of a node and provides
 * them as key-value pair map.
 * This class should be used for nodes whose children elements <i>only</i> contain simple values (without children) and
 * whose order is unpredictable. There must be only one children with the same name.
public class ConverterValueMap {
    private HierarchicalStreamReader reader;
    private Map<String, Object> valueMap;
    private UnmarshallingContext context;
     * @param reader the reader to be used to read-in all children
    public ConverterValueMap(HierarchicalStreamReader reader, UnmarshallingContext context) {
        this(reader, -1, context);
     * @param reader the reader to be used to read-in all children (must not be null)
     * @param numberOfValues the number of children to be read-in ({@code< 0} = until end of section)
     * @throws ConversionException if not all children could be read-in
    public ConverterValueMap(HierarchicalStreamReader reader, int numberOfValues, UnmarshallingContext context)
            throws ConversionException {
        this.reader = reader;
        this.valueMap = readValueMap(this.reader, Math.max(numberOfValues, -1), this.context);
     * Returns the key-value map containing all read-in children.
     * @return the key-value map containing all read-in children (could be empty)
    public Map<String, Object> getValueMap() {
        return valueMap;
     * Reads-in {@code N} children in a key-value map and returns it.
     * @param reader the reader to be used to read-in the children
     * @param numberOfValues the number of children to be read in ({@code < 0} = until end of section)
     * @return the key-value map containing the read-in children (not null, could be empty)
    public static Map<String, Object> readValueMap(HierarchicalStreamReader reader, int numberOfValues,
            UnmarshallingContext context) throws ConversionException {
        Map<String, Object> valueMap = new HashMap<>((numberOfValues >= 0) ? numberOfValues : 10);
        int counter = 0;
        while (reader.hasMoreChildren() && ((counter < numberOfValues) || (numberOfValues == -1))) {
            reader.moveDown();
                List<?> list = (List<?>) context.convertAnother(context, List.class);
                valueMap.put(reader.getNodeName(), list);
                valueMap.put(reader.getNodeName(), reader.getValue());
            reader.moveUp();
            counter++;
        if ((counter < numberOfValues) && (numberOfValues > 0)) {
            throw new ConversionException("Not all children could be read-in!");
     * Returns the object associated with the specified name of the child's node.
     * @param nodeName the name of the child's node
     * @return the object associated with the specified name of the child's node
    public @Nullable Object getObject(String nodeName) {
        return valueMap.get(nodeName);
     * @param defaultValue the value to be returned if the node could not be found
    public @Nullable Object getObject(String nodeName, @Nullable Object defaultValue) {
        Object value = this.valueMap.get(nodeName);
     * Returns the text associated with the specified name of the child's node.
     * @return the text associated with the specified name of the child's node
    public @Nullable String getString(String nodeName) {
        return getString(nodeName, null);
     * @param defaultValue the text to be returned if the node could not be found
    public @Nullable String getString(String nodeName, @Nullable String defaultValue) {
        if (value instanceof String string) {
            // fixes a formatting problem with line breaks in text
            return string.replaceAll("\\n\\s*", " ").trim();
     * Returns the boolean associated with the specified name of the child's node.
     * @return the boolean associated with the specified name of the child's node
    public @Nullable Boolean getBoolean(String nodeName) {
        return getBoolean(nodeName, null);
     * @param defaultValue the boolean to be returned if the node could not be found
    public @Nullable Boolean getBoolean(String nodeName, @Nullable Boolean defaultValue) {
            return Boolean.parseBoolean(value.toString());
     * Returns the numeric value associated with the specified name of the child's node.
     * @return the numeric value associated with the specified name of the child's node
     * @throws ConversionException if the value could not be converted to a numeric value
    public @Nullable Integer getInteger(String nodeName) throws ConversionException {
        return getInteger(nodeName, null);
     * @param defaultValue the numeric value to be returned if the node could not be found
    public @Nullable Integer getInteger(String nodeName, @Nullable Integer defaultValue) throws ConversionException {
                return Integer.parseInt(value.toString());
                throw new ConversionException("The value '" + value + "' cannot be converted to a numeric value!", nfe);
