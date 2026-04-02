 * The {@link NodeList} class contains the node name and its according list of values for an XML tag.
 * This class can be used for an intermediate conversion result of a list of values for an XML tag. The conversion can
 * be done by using the according {@link NodeListConverter}.
public class NodeList implements NodeName {
    private String nodeName;
    private Map<String, String> attributes;
    private List<?> list;
     * @param nodeName the name of the node this object belongs to (must not be empty)
     * @param attributes all attributes of the node this object belongs to (could be empty)
     * @param list the list of the node this object belongs to (could be empty)
    public NodeList(String nodeName, Map<String, String> attributes, List<@NonNull ?> list)
        this.list = list;
     * Returns the attributes of the node as key-value map
     * @return the attributes of the node as key-value map (could be null or empty).
    public Map<String, String> getAttributes() {
     * Returns the list of values of the node
     * @return the list of values of the node (could be null or empty).
    public List<@NonNull ?> getList() {
     * @see #getAttributes(String, String, String)
    public List<String> getAttributes(String nodeName, String attributeName) throws ConversionException {
        return getAttributes(nodeName, attributeName, null);
     * Returns the attributes of the specified XML node and attribute name for the whole list.
     * This list <i>MUST ONLY</i> contain {@link NodeAttributes}.
     * @param nodeName the node name to be considered (must neither be null, nor empty)
     * @param attributeName the attribute name to be considered (must neither be null, nor empty)
     * @param formattedText the format for the output text using the placeholder format
     *            of the Java String (could be null or empty)
     * @return the attributes of the specified XML node and attribute name for the whole list
     * @throws ConversionException if the attribute could not be found in the specified node
    public List<String> getAttributes(String nodeName, String attributeName, @Nullable String formattedText)
        List<String> attributes = new ArrayList<>(list.size());
        String format = formattedText;
        if (format == null || format.isEmpty()) {
            format = "%s";
        for (NodeAttributes node : (List<NodeAttributes>) this.list) {
            if (nodeName.equals(node.getNodeName())) {
                String attributeValue = node.getAttribute(attributeName);
                if (attributeValue != null) {
                    attributes.add(String.format(format, attributeValue));
                    throw new ConversionException("Missing attribute '" + attributeName + "' in '" + nodeName + "'!");
                throw new ConversionException("Invalid attribute in '" + nodeName + "'!");
        return "NodeList [nodeName=" + nodeName + ", attributes=" + attributes + ", list=" + list + "]";
