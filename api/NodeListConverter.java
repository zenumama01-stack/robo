 * The {@link NodeListConverter} is a concrete implementation of the {@code XStream} {@link Converter} interface used to
 * convert a list of XML tags within an XML document
 * into a {@link NodeList} object.
public class NodeListConverter extends GenericUnmarshaller<NodeList> {
    public NodeListConverter() {
        super(NodeList.class);
        List<?> values = (List<?>) context.convertAnother(context, List.class);
        return new NodeList(nodeName, attributes, Objects.requireNonNullElse(values, List.of()));
