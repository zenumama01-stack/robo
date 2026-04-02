 * The {@link NodeAttributes} class contains all attributes for an XML tag.
 * This class <i>DOES NOT</i> support XML tags with attributes <i>AND</i> values, it only supports attributes.
 * This class can be used for an intermediate conversion result of attributes for an XML tag. The conversion can be done
 * by using the according {@link NodeAttributesConverter}.
public class NodeAttributes implements NodeName {
    private final String nodeName;
    private final @Nullable Map<String, String> attributes;
     * @param nodeName the name of the node this object belongs to (must neither be null, nor empty)
     * @param attributes the map of all attributes of the node this object belongs to
     *            by key-value pairs (could be null or empty)
     * @throws IllegalArgumentException if the name of the node is empty
    public NodeAttributes(String nodeName, @Nullable Map<String, String> attributes) throws IllegalArgumentException {
        if (nodeName.isEmpty()) {
            throw new IllegalArgumentException("The name of the node must not be empty!");
        this.nodeName = nodeName;
        this.attributes = attributes;
    public String getNodeName() {
        return this.nodeName;
     * Returns the value of the specified attribute.
     * @param name the name of the attribute whose value should be returned (could be null or empty)
     * @return the value of the specified attribute (could be empty)
    public @Nullable String getAttribute(String name) {
        if (attributes != null) {
            return attributes.get(name);
     * Returns the map of all attributes of a node by key-value pairs.
     * @return the map of all attributes of a node
    public @Nullable Map<String, String> getAttributes() {
        return this.attributes;
        return "NodeAttributes [nodeName=" + nodeName + ", attributes=" + attributes + "]";
