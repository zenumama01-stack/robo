 * The {@link ThingTypeXmlResult} is an intermediate XML conversion result object which
 * contains all fields needed to create a concrete {@link ThingType} object.
public class ThingTypeXmlResult {
    protected ThingTypeUID thingTypeUID;
    protected @Nullable List<String> supportedBridgeTypeUIDs;
    protected String label;
    protected boolean listed;
    protected @Nullable List<String> extensibleChannelTypeIds;
    protected @Nullable String representationProperty;
    protected @Nullable List<ChannelXmlResult> channelTypeReferences;
    protected @Nullable List<ChannelXmlResult> channelGroupTypeReferences;
    protected @Nullable List<NodeValue> properties;
    protected URI configDescriptionURI;
    protected ConfigDescription configDescription;
    protected @Nullable String semanticEquipmentTag;
    public ThingTypeXmlResult(ThingTypeUID thingTypeUID, @Nullable List<String> supportedBridgeTypeUIDs, String label,
        this.channelTypeReferences = channelTypeReferenceObjects[0];
        this.channelGroupTypeReferences = channelTypeReferenceObjects[1];
        this.configDescriptionURI = (URI) configDescriptionObjects[0];
        this.configDescription = (ConfigDescription) configDescriptionObjects[1];
                channelTypeDefinitions.add(channelTypeReference.toChannelDefinition(this.thingTypeUID.getBindingId()));
    protected @Nullable List<ChannelGroupDefinition> toChannelGroupDefinitions(
            @Nullable List<ChannelXmlResult> channelGroupTypeReferences) throws ConversionException {
        List<ChannelGroupDefinition> channelGroupTypeDefinitions = null;
        if (channelGroupTypeReferences != null && !channelGroupTypeReferences.isEmpty()) {
            channelGroupTypeDefinitions = new ArrayList<>(channelGroupTypeReferences.size());
            for (ChannelXmlResult channelGroupTypeReference : channelGroupTypeReferences) {
                String id = channelGroupTypeReference.getId();
                String typeId = channelGroupTypeReference.getTypeId();
                String typeUID = String.format("%s:%s", this.thingTypeUID.getBindingId(), typeId);
                ChannelGroupDefinition channelGroupDefinition = new ChannelGroupDefinition(id,
                        new ChannelGroupTypeUID(typeUID), channelGroupTypeReference.getLabel(),
                        channelGroupTypeReference.getDescription());
                channelGroupTypeDefinitions.add(channelGroupDefinition);
        return channelGroupTypeDefinitions;
    protected @Nullable Map<String, String> toPropertiesMap() {
        List<NodeValue> properties = this.properties;
        for (NodeValue property : properties) {
        return propertiesMap;
    ThingTypeBuilder getBuilder() {
        ThingTypeBuilder builder = ThingTypeBuilder.instance(thingTypeUID, label) //
                .isListed(listed) //
                .withConfigDescriptionURI(configDescriptionURI);
        List<String> supportedBridgeTypeUIDs = this.supportedBridgeTypeUIDs;
        if (supportedBridgeTypeUIDs != null) {
            builder.withSupportedBridgeTypeUIDs(supportedBridgeTypeUIDs);
        String representationProperty = this.representationProperty;
        if (representationProperty != null) {
            builder.withRepresentationProperty(representationProperty);
        List<ChannelGroupDefinition> channelGroupDefinitions = toChannelGroupDefinitions(channelGroupTypeReferences);
        if (channelGroupDefinitions != null) {
            builder.withChannelGroupDefinitions(channelGroupDefinitions);
        Map<String, String> properties = toPropertiesMap();
            builder.withProperties(properties);
        List<String> extensibleChannelTypeIds = this.extensibleChannelTypeIds;
        if (extensibleChannelTypeIds != null) {
            builder.withExtensibleChannelTypeIds(extensibleChannelTypeIds);
        String semanticEquipmentTag = this.semanticEquipmentTag;
        if (semanticEquipmentTag != null) {
            builder.withSemanticEquipmentTag(semanticEquipmentTag);
    public ThingType toThingType() throws ConversionException {
        return getBuilder().build();
        return "ThingTypeXmlResult [thingTypeUID=" + thingTypeUID + ", supportedBridgeTypeUIDs="
                + supportedBridgeTypeUIDs + ", label=" + label + ", description=" + description + ",  category="
