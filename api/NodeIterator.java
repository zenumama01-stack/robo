 * The {@link NodeIterator} is an {@link Iterator} for nodes of an XML document.
 * This iterator offers a simple mechanism iterating through {@code Node}* objects by considering the required or
 * optional occurrences of attributes, values or list of values.
public class NodeIterator implements Iterator<@Nullable Object> {
    private List<?> nodes;
    private int index = 0;
     * Creates a new instance of this class with the specified argument.
     * @param nodes the list of nodes to be iterated through (could be null or empty)
    public NodeIterator(@Nullable List<?> nodes) {
        this.nodes = nodes != null ? nodes : new ArrayList<>(0);
    public boolean hasNext() {
        return (index < this.nodes.size());
    public @Nullable Object next() {
        if (hasNext()) {
            return this.nodes.get(index++);
    public void remove() {
     * Reverts the last {@link #next()} call.
     * After this method returns, the iteration counter is the same as before the last {@link #next()} call. Calling
     * this method multiple times decreases the iteration counter by one until the index of 0 has been reached.
    public void revert() {
     * Ensures that the end of the node has been reached.
     * @throws ConversionException if the end of the node has not reached yet
    public void assertEndOfType() throws ConversionException {
            List<@Nullable Object> nodes = new ArrayList<>();
            while (hasNext()) {
                nodes.add(next());
                    "The document is invalid, it contains further unsupported data: " + nodes + "!");
     * Returns the next object if the specified name of the node fits to the next node,
     * or {@code null} if the node does not exist. In the last case the iterator will
     * <i>not</i> increase its iteration counter.
     * @param nodeName the name of the node to be read next (must neither be null, nor empty)
     * @param required true if the occurrence of the node has to be ensured
     * @return the next object if the specified name of the node fits to the next node,
     * @throws ConversionException if the specified node could not be found in the next node
     *             however it was specified as required
    public @Nullable Object next(String nodeName, boolean required) throws ConversionException {
            Object nextNode = next();
            if (nextNode instanceof NodeName name) {
                if (nodeName.equals(name.getNodeName())) {
                    return nextNode;
            this.index--;
            throw new ConversionException("The node '" + nodeName + "' is missing!");
     * Returns the next attribute if the specified name of the node fits to the next node
     * and the attribute with the specified name could be found, or {@code null} if the
     * node or attribute does not exist. In the last case the iterator will <i>not</i>
     * increase its iteration counter.
     * The next node must be of the type {@link NodeAttributes}.
     * @param attributeName the name of the attribute of the node to be read next
     *            (must neither be null, nor empty)
     * @param required true if the occurrence of the node's attribute has to be ensured
     * @return the next attribute of the specified name of the node and attribute
     *         (could be null or empty)
     * @throws ConversionException if the specified node's attribute could not be found in the
     *             next node however it was specified as required
    public @Nullable String nextAttribute(String nodeName, String attributeName, boolean required)
            if (nextNode instanceof NodeAttributes attributes) {
                if (nodeName.equals(attributes.getNodeName())) {
                    return attributes.getAttribute(attributeName);
                    "The attribute '" + attributeName + "' in the node '" + nodeName + "' is missing!");
     * Returns the next value if the specified name of the node fits to the next node,
     * The next node must be of the type {@link NodeValue}.
     * @param required true if the occurrence of the node's value has to be ensured
     * @return the next value of the specified name of the node (could be null or empty)
     * @throws ConversionException if the specified node's value could not be found in the
    public @Nullable Object nextValue(String nodeName, boolean required) throws ConversionException {
        Object value = next(nodeName, required);
        if (value instanceof NodeValue nodeValue) {
            return nodeValue.getValue();
     * Returns the next list of values if the specified name of the node fits to the
     * next node, or {@code null} if the node does not exist. In the last case the
     * iterator will <i>not</i> increase its iteration counter.
     * The next node must be of the type {@link NodeList}.
     * @param required true if the occurrence of the node's list of values has to be ensured
     * @return the next list of values of the specified name of the node (could be null or empty)
     * @throws ConversionException if the specified node's list of values could not be found
     *             in the next node however it was specified as required
    public @Nullable List<@NonNull ?> nextList(String nodeName, boolean required) throws ConversionException {
        Object list = next(nodeName, required);
        if (list instanceof NodeList nodeList) {
            return nodeList.getList();
