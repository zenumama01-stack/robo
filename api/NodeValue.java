 * The {@link NodeValue} class contains the node name and its according value for an XML tag.
 * This class can be used for an intermediate conversion result of a single value for an XML tag. The conversion can be
 * done by using the according {@link NodeValueConverter}.
public class NodeValue implements NodeName {
    private @Nullable Map<String, String> attributes;
    private @Nullable Object value;
     * @param attributes the attributes of the node this object belongs to
     * @param value the value of the node this object belongs to
    public NodeValue(String nodeName, @Nullable Map<String, String> attributes, @Nullable Object value)
        this.value = formatText(value);
    private @Nullable Object formatText(@Nullable Object object) {
        if (object instanceof String string) {
     * Returns the attributes of the node.
     * @return the attributes of the node
     * Returns the value of the node.
     * @return the value of the node (could be null or empty)
    public @Nullable Object getValue() {
        return "NodeValue [nodeName=" + nodeName + ", value=" + value + "]";
