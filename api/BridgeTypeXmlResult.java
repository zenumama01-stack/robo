 * The {@link BridgeTypeXmlResult} is an intermediate XML conversion result object which
 * contains all fields needed to create a concrete {@link BridgeType} object.
 * If a {@link ConfigDescription} object exists, it must be added to the according {@link ConfigDescriptionProvider}.
public class BridgeTypeXmlResult extends ThingTypeXmlResult {
    public BridgeTypeXmlResult(ThingTypeUID bridgeTypeUID, @Nullable List<String> supportedBridgeTypeUIDs, String label,
            @Nullable List<String> extensibleChannelTypeIds, @Nullable String semanticEquipmentTag,
            @Nullable List<ChannelXmlResult>[] channelTypeReferenceObjects, @Nullable List<NodeValue> properties,
            @Nullable String representationProperty, Object[] configDescriptionObjects) {
        super(bridgeTypeUID, supportedBridgeTypeUIDs, label, description, category, listed, extensibleChannelTypeIds,
                semanticEquipmentTag, channelTypeReferenceObjects, properties, representationProperty,
                configDescriptionObjects);
    public BridgeType toThingType() throws ConversionException {
        return getBuilder().buildBridge();
        return "BridgeTypeXmlResult [thingTypeUID=" + thingTypeUID + ", supportedBridgeTypeUIDs="
                + supportedBridgeTypeUIDs + ", label=" + label + ", description=" + description + ", category="
                + category + ", listed=" + listed + ", representationProperty=" + representationProperty
                + ", channelTypeReferences=" + channelTypeReferences + ", channelGroupTypeReferences="
                + channelGroupTypeReferences + ", extensibelChannelTypeIds=" + extensibleChannelTypeIds
                + ", properties=" + properties + ", configDescriptionURI=" + configDescriptionURI
                + ", configDescription=" + configDescription + ", semanticEquipmentTag=" + semanticEquipmentTag + "]";
