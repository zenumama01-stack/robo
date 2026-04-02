 * The {@link ThingTypeConverter} is a concrete implementation of the {@code XStream} {@link Converter} interface used
 * to convert {@code Thing} type information within
 * an XML document into a {@link ThingTypeXmlResult} object.
 * This converter converts {@code thing-type} XML tags. It uses the {@link AbstractDescriptionTypeConverter} which
 * @author Chris Jackson - Added channel properties
public class ThingTypeConverter extends AbstractDescriptionTypeConverter<ThingTypeXmlResult> {
    public ThingTypeConverter() {
        this(ThingTypeXmlResult.class, "thing-type");
     * @param clazz the class of the result object (must not be null)
     * @param type the name of the type (e.g. "bridge-type")
    protected ThingTypeConverter(Class clazz, String type) {
        super(clazz, type);
                new String[][] { { "id", "true" }, { "listed", "false" }, { "extensible", "false" } });
    protected @Nullable List<String> readSupportedBridgeTypeUIDs(NodeIterator nodeIterator,
            UnmarshallingContext context) {
        Object nextNode = nodeIterator.next("supported-bridge-type-refs", false);
            String thisBindingId = (String) context.get("thing-descriptions.bindingId");
            NodeList nodeList = (NodeList) nextNode;
            List<NodeAttributes> nodes = (List<NodeAttributes>) nodeList.getList();
            List<String> uids = new ArrayList<>(nodes.size());
            for (NodeAttributes node : nodes) {
                if ("bridge-type-ref".equals(node.getNodeName())) {
                    String id = node.getAttribute("id");
                    String bindingId = node.getAttribute("bindingId");
                        throw new ConversionException("Missing attribute 'id' in 'bridge-type-ref'!");
                    if (bindingId == null) {
                        bindingId = thisBindingId;
                    uids.add(String.format("%s:%s", bindingId, id));
                    throw new ConversionException("Invalid element in 'supported-bridge-type-refs'!");
            return uids;
    protected List<ChannelXmlResult>[] getChannelTypeReferenceObjects(NodeIterator nodeIterator)
        List<ChannelXmlResult> channelGroupTypeReferences = null;
        List<ChannelXmlResult> channelTypeReferences = (List<ChannelXmlResult>) nodeIterator.nextList("channels",
        if (channelTypeReferences == null) {
            channelGroupTypeReferences = (List<ChannelXmlResult>) nodeIterator.nextList("channel-groups", false);
        return new List[] { channelTypeReferences, channelGroupTypeReferences };
    protected @Nullable ThingTypeXmlResult unmarshalType(HierarchicalStreamReader reader, UnmarshallingContext context,
        return new ThingTypeXmlResult(new ThingTypeUID(super.getUID(attributes, context)),
                readSupportedBridgeTypeUIDs(nodeIterator, context), super.readLabel(nodeIterator),
                super.readDescription(nodeIterator), readCategory(nodeIterator), getListed(attributes),
                getRepresentationProperty(nodeIterator), super.getConfigDescriptionObjects(nodeIterator));
    protected List<String> getExtensibleChannelTypeIds(Map<String, String> attributes) {
        String extensible = attributes.get("extensible");
        if (extensible == null) {
        return Arrays.stream(extensible.split(",")).map(String::trim).toList();
    protected @Nullable String readCategory(NodeIterator nodeIterator) {
            return category.toString();
    protected boolean getListed(Map<String, String> attributes) {
        String listedFlag = attributes.get("listed");
        if (listedFlag != null) {
            return Boolean.parseBoolean(listedFlag);
    protected @Nullable String getRepresentationProperty(NodeIterator nodeIterator) {
        return (String) nodeIterator.nextValue("representation-property", false);
    protected @Nullable String getSemanticEquipmentTag(NodeIterator nodeIterator) {
        return (String) nodeIterator.nextValue("semantic-equipment-tag", false);
