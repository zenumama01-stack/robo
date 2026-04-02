 * The {@link ChannelTypeConverter} is a concrete implementation of the {@code XStream} {@link Converter} interface used
 * to convert channel type information within an XML document
 * into a {@link ChannelTypeXmlResult} object.
 * This converter converts {@code channel-type} XML tags. It uses the {@link AbstractDescriptionTypeConverter} which
 * offers base functionality for each type definition.
 * @author Ivan Iliev - Added support for system wide channel types
public class ChannelTypeConverter extends AbstractDescriptionTypeConverter<ChannelTypeXmlResult> {
    record ItemType(@Nullable String itemType, @Nullable String unitHint) {
    public ChannelTypeConverter() {
        super(ChannelTypeXmlResult.class, "channel-type");
                new String[][] { { "id", "true" }, { "advanced", "false" }, { "system", "false" } });
    private boolean readBoolean(Map<String, String> attributes, String attributeName, boolean defaultValue) {
        String advancedFlag = attributes.get(attributeName);
        if (advancedFlag != null) {
            return Boolean.parseBoolean(advancedFlag);
    private ItemType readItemType(NodeIterator nodeIterator) throws ConversionException {
        Object next = nodeIterator.next("item-type", false);
        if (next instanceof NodeValue nodeValue) {
            String itemType = (String) nodeValue.getValue();
            Map<String, String> attributes = nodeValue.getAttributes();
            String unitHint = attributes != null ? attributes.get("unitHint") : null;
            return new ItemType(itemType, unitHint);
        return new ItemType(null, null);
    private @Nullable String readKind(NodeIterator nodeIterator) throws ConversionException {
        return (String) nodeIterator.nextValue("kind", false);
    private @Nullable String readCategory(NodeIterator nodeIterator) throws ConversionException {
        return (String) nodeIterator.nextValue("category", false);
    private @Nullable Set<String> readTags(NodeIterator nodeIterator) throws ConversionException {
        Set<String> tags = null;
        List<@NonNull ?> tagsNode = nodeIterator.nextList("tags", false);
        if (tagsNode != null) {
            tags = new HashSet<>(tagsNode.size());
            for (Object tagNodeObject : tagsNode) {
                NodeValue tagNode = (NodeValue) tagNodeObject;
                if ("tag".equals(tagNode.getNodeName())) {
                    String tag = (String) tagNode.getValue();
                    throw new ConversionException("The 'tags' node must only contain 'tag' nodes!");
    private @Nullable StateDescription readStateDescription(NodeIterator nodeIterator) {
            if (nextNode instanceof StateDescription description) {
    private @Nullable EventDescription readEventDescription(NodeIterator nodeIterator) {
            if (nextNode instanceof EventDescription description) {
    private @Nullable CommandDescription readCommandDescription(NodeIterator nodeIterator) throws ConversionException {
            if (nextNode instanceof CommandDescription description) {
    protected @Nullable ChannelTypeXmlResult unmarshalType(HierarchicalStreamReader reader,
        boolean advanced = readBoolean(attributes, "advanced", false);
        boolean system = readBoolean(attributes, "system", false);
        String uid = system ? XmlHelper.getSystemUID(super.getID(attributes)) : super.getUID(attributes, context);
        ChannelTypeUID channelTypeUID = new ChannelTypeUID(uid);
        ItemType type = readItemType(nodeIterator);
        String itemType = type.itemType();
        String unitHint = type.unitHint();
        String kind = readKind(nodeIterator);
        Set<String> tags = readTags(nodeIterator);
        StateDescription stateDescription = readStateDescription(nodeIterator);
        StateDescriptionFragment stateDescriptionFragment = stateDescription != null
                ? StateDescriptionFragmentBuilder.create(stateDescription).build()
        CommandDescription commandDescription = readCommandDescription(nodeIterator);
        EventDescription eventDescription = readEventDescription(nodeIterator);
        Object[] configDescriptionObjects = super.getConfigDescriptionObjects(nodeIterator);
            // Default for kind is 'state'
            kind = ChannelKind.STATE.name();
        ChannelKind cKind = ChannelKind.parse(kind);
        URI configDescriptionURI = (URI) configDescriptionObjects[0];
        if (cKind == ChannelKind.STATE) {
            itemType = requireNonEmpty(itemType, "ChannelType 'itemType' must not be null or empty.");
            builder = ChannelTypeBuilder.state(channelTypeUID, label, itemType).isAdvanced(advanced)
                    .withConfigDescriptionURI(configDescriptionURI)
                    .withStateDescriptionFragment(stateDescriptionFragment).withAutoUpdatePolicy(autoUpdatePolicy)
                    .withCommandDescription(commandDescription).withUnitHint(unitHint);
        } else if (cKind == ChannelKind.TRIGGER) {
            TriggerChannelTypeBuilder triggerChannelTypeBuilder = ChannelTypeBuilder.trigger(channelTypeUID, label)
                    .isAdvanced(advanced).withConfigDescriptionURI(configDescriptionURI);
            builder = triggerChannelTypeBuilder;
                triggerChannelTypeBuilder.withEventDescription(eventDescription);
            throw new IllegalArgumentException(String.format("Unknown channel kind: '%s'", cKind));
            builder.withTags(tags);
        ChannelType channelType = builder.build();
        return new ChannelTypeXmlResult(channelType, (ConfigDescription) configDescriptionObjects[1], system);
