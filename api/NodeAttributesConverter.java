 * The {@link NodeAttributesConverter} is a concrete implementation of the {@code XStream} {@link Converter} interface
 * used to convert the attributes of an XML tag within an XML
 * document into a {@link NodeAttributes} object.
public class NodeAttributesConverter extends GenericUnmarshaller<NodeAttributes> {
    public NodeAttributesConverter() {
        super(NodeAttributes.class);
        String nodeName = reader.getNodeName();
        Map<String, String> attributes = ConverterAttributeMapValidator.readValidatedAttributes(reader, null);
        return new NodeAttributes(nodeName, attributes);
