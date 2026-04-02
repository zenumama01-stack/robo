 * The {@link NodeValueConverter} is a concrete implementation of the {@code XStream} {@link Converter} interface used
 * to convert the value of an XML tag within an XML
 * document into a {@link NodeValue} object.
public class NodeValueConverter extends GenericUnmarshaller<NodeValue> {
    public NodeValueConverter() {
        super(NodeValue.class);
        return new NodeValue(reader.getNodeName(), attributes, reader.getValue());
