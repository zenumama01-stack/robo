 * The {@link ChannelGroupTypeConverter} is a concrete implementation of the {@code XStream} {@link Converter} interface
 * used to convert channel group type information within an
 * XML document into a {@link ChannelGroupTypeXmlResult} object.
 * This converter converts {@code channel-group-type} XML tags. It uses the {@link AbstractDescriptionTypeConverter}
 * which offers base functionality for each type definition.
 * @author Chris Jackson - Modified to support channel properties
public class ChannelGroupTypeConverter extends AbstractDescriptionTypeConverter<ChannelGroupTypeXmlResult> {
    public ChannelGroupTypeConverter() {
        super(ChannelGroupTypeXmlResult.class, "channel-group-type");
        super.attributeMapValidator = new ConverterAttributeMapValidator(new String[][] { { "id", "true" } });
    protected @Nullable List<ChannelXmlResult> readChannelTypeDefinitions(NodeIterator nodeIterator)
        return (List<ChannelXmlResult>) nodeIterator.nextList("channels", false);
    protected @Nullable ChannelGroupTypeXmlResult unmarshalType(HierarchicalStreamReader reader,
            UnmarshallingContext context, Map<String, String> attributes, NodeIterator nodeIterator)
        ChannelGroupTypeUID channelGroupTypeUID = new ChannelGroupTypeUID(super.getUID(attributes, context));
        String label = super.readLabel(nodeIterator);
        String description = super.readDescription(nodeIterator);
        String category = readCategory(nodeIterator);
        List<ChannelXmlResult> channelTypeDefinitions = readChannelTypeDefinitions(nodeIterator);
        return new ChannelGroupTypeXmlResult(channelGroupTypeUID, label, description, category, channelTypeDefinitions);
    private @Nullable String readCategory(NodeIterator nodeIterator) {
        Object category = nodeIterator.nextValue("category", false);
        return category == null ? null : category.toString();
