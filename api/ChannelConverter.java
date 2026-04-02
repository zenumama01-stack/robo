 * The {@link ChannelConverter} is a concrete implementation of the {@code XStream} {@link Converter} interface used
 * to convert channel information within an XML document
 * into a {@link ChannelXmlResult} object.
 * This converter converts {@code channel} XML tags.
 * @author Simon Kaufmann - Fixing wrong inheritance
 * @author Chris Jackson - Added label and description
public class ChannelConverter extends GenericUnmarshaller<ChannelXmlResult> {
    public ChannelConverter() {
        super(ChannelXmlResult.class);
        attributeMapValidator = new ConverterAttributeMapValidator(
                new String[][] { { "id", "true" }, { "typeId", "false" } });
    protected @Nullable List<NodeValue> getProperties(NodeIterator nodeIterator) {
        return (List<NodeValue>) nodeIterator.nextList("properties", false);
    protected ChannelXmlResult unmarshalType(HierarchicalStreamReader reader, UnmarshallingContext context,
        String id = requireNonEmpty(attributes.get("id"), "Channel id attribute is null or empty");
        String typeId = requireNonEmpty(attributes.get("typeId"), "Channel typeId attribute is null or empty");
        String label = (String) nodeIterator.nextValue("label", false);
        String description = (String) nodeIterator.nextValue("description", false);
        List<NodeValue> properties = getProperties(nodeIterator);
        AutoUpdatePolicy autoUpdatePolicy = readAutoUpdatePolicy(nodeIterator);
        return new ChannelXmlResult(id, typeId, label, description, properties, autoUpdatePolicy);
    private @Nullable AutoUpdatePolicy readAutoUpdatePolicy(NodeIterator nodeIterator) {
        String string = (String) nodeIterator.nextValue("autoUpdatePolicy", false);
            return AutoUpdatePolicy.valueOf(string.toUpperCase(Locale.ENGLISH));
