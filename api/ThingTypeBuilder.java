 * A {@link ThingType} builder.
public class ThingTypeBuilder {
    private @Nullable List<ChannelGroupDefinition> channelGroupDefinitions;
    private @Nullable List<String> extensibleChannelTypeIds;
    private @Nullable List<String> supportedBridgeTypeUIDs;
    private @Nullable URI configDescriptionURI;
    private boolean listed;
    private final String bindingId;
    private final String thingTypeId;
     * Create and return a {@link ThingTypeBuilder} with the given {@code bindingId}, {@code thingTypeId} and
     * {@code label}. Also, {@code listed} defaults to {@code true}.
     * @param bindingId the binding Id the resulting {@link ThingType} will have. Must not be null or empty.
     * @param thingTypeId the thingTypeId the resulting {@link ThingType} will have (builds a {@link ThingTypeUID} with
     *            {@code bindingId:thingTypeId}. Must not be null or empty.
     * @param label the label of the resulting {@link ThingType}. Must not be null or empty.
     * @return the new {@link ThingTypeBuilder}.
    public static ThingTypeBuilder instance(String bindingId, String thingTypeId, String label) {
        return new ThingTypeBuilder(bindingId, thingTypeId, label);
     * Create and return a {@link ThingTypeBuilder} with the given {@link ThingTypeUID} and {@code label}. Also,
     * {@code listed} defaults to {@code true}.
     * @param thingTypeUID the {@link ThingTypeUID} the resulting {@link ThingType} will have. Must not be null.
    public static ThingTypeBuilder instance(ThingTypeUID thingTypeUID, String label) {
        return new ThingTypeBuilder(thingTypeUID.getBindingId(), thingTypeUID.getId(), label);
     * Create this builder with all properties from the given {@link ThingType}.
     * @param thingType take all properties from this {@link ThingType}.
     * @return a new {@link ThingTypeBuilder} configured with all properties from the given {@link ThingType};
    public static ThingTypeBuilder instance(ThingType thingType) {
        return new ThingTypeBuilder(thingType);
    private ThingTypeBuilder(String bindingId, String thingTypeId, String label) {
        this.bindingId = bindingId;
        this.thingTypeId = thingTypeId;
        this.listed = true;
    private ThingTypeBuilder(ThingType thingType) {
        this(thingType.getBindingId(), thingType.getUID().getId(), thingType.getLabel());
        description = thingType.getDescription();
        channelGroupDefinitions = thingType.getChannelGroupDefinitions();
        channelDefinitions = thingType.getChannelDefinitions();
        extensibleChannelTypeIds = thingType.getExtensibleChannelTypeIds();
        supportedBridgeTypeUIDs = thingType.getSupportedBridgeTypeUIDs();
        properties = thingType.getProperties();
        representationProperty = thingType.getRepresentationProperty();
        configDescriptionURI = thingType.getConfigDescriptionURI();
        listed = thingType.isListed();
        category = thingType.getCategory();
        semanticEquipmentTag = thingType.getSemanticEquipmentTag();
     * Builds and returns a new {@link ThingType} according to the given values from this builder.
     * @return a new {@link ThingType} according to the given values from this builder.
     * @throws IllegalStateException if one of {@code bindingId}, {@code thingTypeId} or {@code label} are not given.
    public ThingType build() {
        if (bindingId.isBlank()) {
            throw new IllegalArgumentException("The bindingId must neither be null nor empty.");
        if (thingTypeId.isBlank()) {
            throw new IllegalArgumentException("The thingTypeId must neither be null nor empty.");
            throw new IllegalArgumentException("The label must neither be null nor empty.");
        return new ThingType(new ThingTypeUID(bindingId, thingTypeId), supportedBridgeTypeUIDs, label, description,
                category, listed, representationProperty, channelDefinitions, channelGroupDefinitions, properties,
                configDescriptionURI, extensibleChannelTypeIds, semanticEquipmentTag);
     * Builds and returns a new {@link BridgeType} according to the given values from this builder.
     * @return a new {@link BridgeType} according to the given values from this builder.
    public BridgeType buildBridge() {
        return new BridgeType(new ThingTypeUID(bindingId, thingTypeId), supportedBridgeTypeUIDs, label, description,
    public ThingTypeBuilder withLabel(String label) {
    public ThingTypeBuilder withDescription(String description) {
    public ThingTypeBuilder withCategory(String category) {
    public ThingTypeBuilder isListed(boolean listed) {
    public ThingTypeBuilder withRepresentationProperty(String representationProperty) {
    public ThingTypeBuilder withChannelDefinitions(List<ChannelDefinition> channelDefinitions) {
    public ThingTypeBuilder withChannelGroupDefinitions(List<ChannelGroupDefinition> channelGroupDefinitions) {
        this.channelGroupDefinitions = channelGroupDefinitions;
    public ThingTypeBuilder withProperties(Map<String, String> properties) {
    public ThingTypeBuilder withConfigDescriptionURI(URI configDescriptionURI) {
    public ThingTypeBuilder withExtensibleChannelTypeIds(List<String> extensibleChannelTypeIds) {
    public ThingTypeBuilder withSupportedBridgeTypeUIDs(List<String> supportedBridgeTypeUIDs) {
    public ThingTypeBuilder withSemanticEquipmentTag(String semanticEquipmentTag) {
    public ThingTypeBuilder withSemanticEquipmentTag(SemanticTag semanticEquipmentTag) {
