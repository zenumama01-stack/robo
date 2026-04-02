 * The {@link ThingType} describes a concrete type of {@link Thing}.
 * This description is used as template definition for the creation of the according concrete {@link Thing} object.
 * @author Simon Kaufmann - Added listed field
 * @author Andre Fuechsel - Added representationProperty field
 * @author Stefan Triller - Added category field
public class ThingType extends AbstractDescriptionType {
    private final List<ChannelGroupDefinition> channelGroupDefinitions;
    private final List<String> extensibleChannelTypeIds;
    private final List<String> supportedBridgeTypeUIDs;
    private final @Nullable String representationProperty;
    private final boolean listed;
    private final @Nullable String semanticEquipmentTag;
     * @param category the category of the thing (could be null)
     * @param semanticEquipmentTag the semantic (equipment) tag of the thing (could be null)
    ThingType(ThingTypeUID uid, @Nullable List<String> supportedBridgeTypeUIDs, String label,
        this.supportedBridgeTypeUIDs = supportedBridgeTypeUIDs == null ? List.of()
                : Collections.unmodifiableList(supportedBridgeTypeUIDs);
        this.channelGroupDefinitions = channelGroupDefinitions == null ? List.of()
                : Collections.unmodifiableList(channelGroupDefinitions);
        this.extensibleChannelTypeIds = extensibleChannelTypeIds == null ? List.of()
                : Collections.unmodifiableList(extensibleChannelTypeIds);
     * Returns the unique identifier which identifies this Thing type within the overall system.
     * @return the unique identifier which identifies this Thing type within the overall system
    public ThingTypeUID getUID() {
        return (ThingTypeUID) super.getUID();
     * Returns the binding ID this Thing type belongs to.
     * @return the binding ID this Thing type belongs to (not null)
        return getUID().getBindingId();
     * Returns the unique identifiers of the bridges this {@link ThingType} supports.
     * @return the unique identifiers of the bridges this Thing type supports
    public List<String> getSupportedBridgeTypeUIDs() {
        return this.supportedBridgeTypeUIDs;
     * Returns the channels this {@link ThingType} provides.
     * @return the channels this Thing type provides (not null, could be empty)
        return this.channelDefinitions;
     * Returns the channel groups defining the channels this {@link ThingType} provides.
     * @return the channel groups defining the channels this Thing type provides
    public List<ChannelGroupDefinition> getChannelGroupDefinitions() {
        return this.channelGroupDefinitions;
     * Returns the properties for this {@link ThingType}
     * @return the properties for this {@link ThingType} (not null)
        return this.category;
     * Check, if things of this thing type should be listed for manually pairing or not.
     * @return {@code true}, if manual pairing is allowed
    public boolean isListed() {
        return listed;
     * Get the name of the representation property of this thing type. May be {code null}.
     * @return representation property name or {@code null}
        ThingType other = (ThingType) obj;
        return this.getUID().equals(other.getUID());
        return getUID().hashCode();
        return getUID().toString();
    public List<String> getExtensibleChannelTypeIds() {
        return extensibleChannelTypeIds;
     * Get the semantic (equipment) tag of this thing type. May be {code null}.
