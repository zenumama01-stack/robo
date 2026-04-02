 * Tests the {@link ThingTypeBuilder}.
public class ThingTypeBuilderTest {
    private static final String CONF_URI = "conf:uri";
    private static final String REPRESENTATION_PROPERTY = "representationProperty";
    private static final String THING_TYPE_ID = "thingTypeId";
    private @NonNullByDefault({}) ThingTypeBuilder builder;
        // set up a valid basic ThingTypeBuilder
        builder = ThingTypeBuilder.instance(BINDING_ID, THING_TYPE_ID, LABEL);
    public void whenThingTypeIdAndBindingIdBlankShouldFail() {
        assertThrows(IllegalArgumentException.class, () -> ThingTypeBuilder.instance("", "", LABEL).build());
    public void whenThingTypeIdBlankShouldFail() {
        assertThrows(IllegalArgumentException.class, () -> ThingTypeBuilder.instance(BINDING_ID, "", LABEL).build());
    public void whenBindingIdBlankShouldFail() {
        assertThrows(IllegalArgumentException.class, () -> ThingTypeBuilder.instance("", THING_TYPE_ID, LABEL).build());
    public void whenLabelBlankShouldFail() {
                () -> ThingTypeBuilder.instance(THING_TYPE_ID, BINDING_ID, "").build());
    public void withLabelAndThingTypeUIDShouldCreateThingType() {
        ThingType thingType = ThingTypeBuilder.instance(new ThingTypeUID(BINDING_ID, THING_TYPE_ID), LABEL).build();
        assertThat(thingType.getBindingId(), is(BINDING_ID));
        assertThat(thingType.getUID().getBindingId(), is(BINDING_ID));
        assertThat(thingType.getUID().getId(), is(THING_TYPE_ID));
        assertThat(thingType.getLabel(), is(LABEL));
    public void withLabelAndThingTypeIdAndBindingIDShouldCreateThingType() {
        ThingType thingType = builder.build();
    public void withLabelAndThingTypeIdAndBindingIDShouldSetListed() {
        assertThat(thingType.isListed(), is(true));
        ThingType thingType = builder.withDescription(DESCRIPTION).build();
        assertThat(thingType.getDescription(), is(DESCRIPTION));
        ThingType thingType = builder.withCategory(CATEGORY).build();
        assertThat(thingType.getCategory(), is(CATEGORY));
    public void withListedShouldBeListed() {
        ThingType thingType = builder.isListed(false).build();
        assertThat(thingType.isListed(), is(false));
    public void withRepresentationPropertyShouldSetRepresentationProperty() {
        ThingType thingType = builder.withRepresentationProperty(REPRESENTATION_PROPERTY).build();
        assertThat(thingType.getRepresentationProperty(), is(REPRESENTATION_PROPERTY));
        ThingType thingType = builder.withChannelDefinitions(mockList(ChannelDefinition.class, 2)).build();
        assertThat(thingType.getChannelDefinitions(), is(hasSize(2)));
            thingType.getChannelDefinitions().add(mock(ChannelDefinition.class));
    public void withChannelGroupDefinitionsShouldSetUnmodifiableChannelGroupDefinitions() {
        ThingType thingType = builder.withChannelGroupDefinitions(mockList(ChannelGroupDefinition.class, 2)).build();
        assertThat(thingType.getChannelGroupDefinitions(), is(hasSize(2)));
            thingType.getChannelGroupDefinitions().add(mock(ChannelGroupDefinition.class));
    public void withPropertiesShouldSetUnmodifiableProperties() {
        ThingType thingType = builder.withProperties(mockProperties()).build();
        assertThat(thingType.getProperties().entrySet(), is(hasSize(2)));
            thingType.getProperties().put("should", "fail");
    public void withConfigDescriptionURIShouldSetConfigDescriptionURI() throws Exception {
        ThingType thingType = builder.withConfigDescriptionURI(new URI(CONF_URI)).build();
        assertThat(thingType.getConfigDescriptionURI(), is(new URI(CONF_URI)));
    public void withExtensibleChannelTypeIdsShouldSetUnmodifiableExtensibleChannelTypeIds() {
        ThingType thingType = builder.withExtensibleChannelTypeIds(List.of("channelTypeId1", "channelTypeId2")).build();
        assertThat(thingType.getExtensibleChannelTypeIds(), is(hasSize(2)));
            thingType.getExtensibleChannelTypeIds().add("channelTypeId");
    public void withSupportedBridgeTypeUIDsShouldSetUnmodifiableSupportedBridgeTypeUIDs() {
        ThingType thingType = builder.withSupportedBridgeTypeUIDs(List.of("bridgeTypeUID1")).build();
        assertThat(thingType.getSupportedBridgeTypeUIDs(), is(hasSize(1)));
            thingType.getSupportedBridgeTypeUIDs().add("bridgeTypeUID");
    public void shouldBuildBridgeType() {
        BridgeType bridgeType = builder.buildBridge();
        assertThat(bridgeType.getBindingId(), is(BINDING_ID));
        assertThat(bridgeType.getUID().getBindingId(), is(BINDING_ID));
        assertThat(bridgeType.getUID().getId(), is(THING_TYPE_ID));
        assertThat(bridgeType.getLabel(), is(LABEL));
    private Map<String, String> mockProperties() {
        return Map.of("key1", "value1", "key2", "value2");
