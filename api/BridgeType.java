 * The {@link BridgeType} describes a concrete type of a {@link Bridge}.
 * A {@link BridgeType} inherits a {@link ThingType} and signals a parent-child relation.
 * This description is used as template definition for the creation of the according concrete {@link Bridge} object.
 * @author Andre Fuechsel - Added representationProperty
public class BridgeType extends ThingType {
     * A new instance of BridgeType.
     * @see ThingType#ThingType(ThingTypeUID, List, String, String, String, boolean, String, List, List, Map, URI, List)
     * @param uid the unique identifier which identifies this Thing type within the overall system
     * @param supportedBridgeTypeUIDs the unique identifiers of the bridges this Thing type supports
     * @param label the human readable label for the according type
     * @param description the human readable description for the according type
     * @param category the category of the bridge (could be null)
     * @param listed determines whether it should be listed for manually pairing or not
     * @param representationProperty name of the property that uniquely identifies this Thing
     * @param channelDefinitions the channels this Thing type provides (could be null or empty)
     * @param channelGroupDefinitions the channel groups defining the channels this Thing type
     *            provides (could be null or empty)
     * @param properties the properties this Thing type provides (could be null)
     * @param configDescriptionURI the link to the concrete ConfigDescription (could be null)
     * @param extensibleChannelTypeIds the channel-type ids this thing-type is extensible with (could be null or empty).
     * @param semanticEquipmentTag the semantic (equipment) tag of the bridge (could be null)
     * @throws IllegalArgumentException if the UID is null or empty, or the meta information is null
    BridgeType(ThingTypeUID uid, @Nullable List<String> supportedBridgeTypeUIDs, String label,
            @Nullable String description, @Nullable String category, boolean listed,
            @Nullable String representationProperty, @Nullable List<ChannelDefinition> channelDefinitions,
            @Nullable List<ChannelGroupDefinition> channelGroupDefinitions, @Nullable Map<String, String> properties,
            @Nullable URI configDescriptionURI, @Nullable List<String> extensibleChannelTypeIds,
            @Nullable String semanticEquipmentTag) throws IllegalArgumentException {
        super(uid, supportedBridgeTypeUIDs, label, description, category, listed, representationProperty,
                channelDefinitions, channelGroupDefinitions, properties, configDescriptionURI, extensibleChannelTypeIds,
                semanticEquipmentTag);
