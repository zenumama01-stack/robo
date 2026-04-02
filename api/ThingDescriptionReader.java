 * The {@link ThingDescriptionReader} reads XML documents, which contain the {@code thing-description} XML tag, and
 * converts them to {@link ThingDescriptionList} objects consisting of {@link ThingTypeXmlResult},
 * {@link BridgeTypeXmlResult} and {@link ChannelTypeXmlResult} objects.
 * @author Chris Jackson - Added parameter groups and channel properties
 * @author Moritz Kammerer - Added triggers
 * @author Andrew Fiddian-Green - Added equipment property
public class ThingDescriptionReader extends XmlDocumentReader<List<?>> {
    public ThingDescriptionReader() {
        ClassLoader classLoader = ThingDescriptionReader.class.getClassLoader();
    public void registerConverters(XStream xstream) {
        xstream.registerConverter(new ThingDescriptionConverter());
        xstream.registerConverter(new ThingTypeConverter());
        xstream.registerConverter(new BridgeTypeConverter());
        xstream.registerConverter(new ChannelConverter());
        xstream.registerConverter(new ChannelTypeConverter());
        xstream.registerConverter(new ChannelGroupTypeConverter());
        xstream.registerConverter(new StateDescriptionConverter());
        xstream.registerConverter(new CommandDescriptionConverter());
        xstream.registerConverter(new EventDescriptionConverter());
    public void registerAliases(XStream xstream) {
        xstream.alias("thing-descriptions", ThingDescriptionList.class);
        xstream.alias("thing-type", ThingTypeXmlResult.class);
        xstream.alias("bridge-type", BridgeTypeXmlResult.class);
        xstream.alias("channel-type", ChannelTypeXmlResult.class);
        xstream.alias("channel-group-type", ChannelGroupTypeXmlResult.class);
        xstream.alias("supported-bridge-type-refs", NodeList.class);
        xstream.alias("bridge-type-ref", NodeAttributes.class);
        xstream.alias("item-type", NodeValue.class);
        xstream.alias("dimension", NodeValue.class);
        xstream.alias("kind", NodeValue.class);
        xstream.alias("label", NodeValue.class);
        xstream.alias("channels", NodeList.class);
        xstream.alias("channel", ChannelXmlResult.class);
        xstream.alias("channel-groups", NodeList.class);
        xstream.alias("channel-group", ChannelXmlResult.class);
        xstream.alias("category", NodeValue.class);
        xstream.alias("tags", NodeList.class);
        xstream.alias("tag", NodeValue.class);
        xstream.alias("state", StateDescription.class);
        xstream.alias("command", CommandDescription.class);
        xstream.alias("event", EventDescription.class);
        xstream.alias("config-descriptions", NodeList.class);
        xstream.alias("properties", NodeList.class);
        xstream.alias("property", NodeValue.class);
        xstream.alias("representation-property", NodeValue.class);
        xstream.alias("command-options", NodeList.class);
        xstream.alias("autoUpdatePolicy", NodeValue.class);
        xstream.alias("semantic-equipment-tag", NodeValue.class);
