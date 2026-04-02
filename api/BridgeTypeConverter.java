 * The {@link BridgeTypeConverter} is a concrete implementation of the {@code XStream} {@link Converter} interface used
 * to convert bridge type information within an XML document
 * into a {@link BridgeTypeXmlResult} object.
 * This converter converts {@code bridge-type} XML tags. It uses the {@link ThingTypeConverter} since both contain the
 * same content.
public class BridgeTypeConverter extends ThingTypeConverter {
    public BridgeTypeConverter() {
        super(BridgeTypeXmlResult.class, "thing-type");
    protected @Nullable BridgeTypeXmlResult unmarshalType(HierarchicalStreamReader reader, UnmarshallingContext context,
            Map<String, String> attributes, NodeIterator nodeIterator) throws ConversionException {
        return new BridgeTypeXmlResult(new ThingTypeUID(getUID(attributes, context)),
                readSupportedBridgeTypeUIDs(nodeIterator, context), readLabel(nodeIterator),
                readDescription(nodeIterator), readCategory(nodeIterator), getListed(attributes),
                getExtensibleChannelTypeIds(attributes), getSemanticEquipmentTag(nodeIterator),
                getChannelTypeReferenceObjects(nodeIterator), getProperties(nodeIterator),
                getRepresentationProperty(nodeIterator), getConfigDescriptionObjects(nodeIterator));
