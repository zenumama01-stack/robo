import org.junit.jupiter.api.Disabled;
public class DiscoveryResultBuilderTest {
    private static final ThingUID THING_UID = new ThingUID(THING_TYPE_UID, BRIDGE_UID, "thingId");
    private static final Map<String, Object> PROPERTIES = Map.of(KEY1, VALUE1, KEY2, VALUE2);
    private @NonNullByDefault({}) DiscoveryResultBuilder builder;
    private @NonNullByDefault({}) DiscoveryResult discoveryResult;
        builder = DiscoveryResultBuilder.create(THING_UID).withThingType(THING_TYPE_UID).withProperties(PROPERTIES)
                .withRepresentationProperty(KEY1).withLabel("Test");
        discoveryResult = builder.build();
    public void testInstance() {
        assertThat(builder, is(instanceOf(DiscoveryResultBuilder.class)));
        assertThat(builder.withLabel("TEST"), is(instanceOf(DiscoveryResultBuilder.class)));
        assertThat(builder.build(), is(instanceOf(DiscoveryResult.class)));
    void testDiscoveryResultBuilderWithMissingRepresentationProperty() {
        final String property = "test";
        builder.withRepresentationProperty(property);
        // there will be a warning now because the representation property is missing in properties map
        builder.build();
    void testDiscoveryResultBuilderWithExistingRepresentationProperty() {
        builder.withProperty(property, "test").withRepresentationProperty(property);
    public void testDiscoveryResultBuilder() {
        assertThat(discoveryResult.getThingUID(), is(THING_UID));
        assertThat(discoveryResult.getThingTypeUID(), is(THING_TYPE_UID));
        assertThat(discoveryResult.getBindingId(), is(BINDING_ID));
        assertThat(discoveryResult.getLabel(), is("Test"));
        assertThat(discoveryResult.getProperties().size(), is(2));
        assertThat(discoveryResult.getProperties(), hasEntry(KEY1, VALUE1));
        assertThat(discoveryResult.getProperties(), hasEntry(KEY2, VALUE2));
        assertThat(discoveryResult.getRepresentationProperty(), is(KEY1));
        assertThat(discoveryResult.getTimeToLive(), is(DiscoveryResult.TTL_UNLIMITED));
    public void testDiscoveryResultBuilderCopy() {
        DiscoveryResult r = DiscoveryResultBuilder.create(discoveryResult).build();
        assertThat(r.getThingUID(), is(discoveryResult.getThingUID()));
        assertThat(r.getThingTypeUID(), is(discoveryResult.getThingTypeUID()));
        assertThat(r.getBindingId(), is(discoveryResult.getBindingId()));
        assertThat(r.getLabel(), is(discoveryResult.getLabel()));
        assertThat(r.getProperties(), is(discoveryResult.getProperties()));
        assertThat(r.getRepresentationProperty(), is(discoveryResult.getRepresentationProperty()));
        assertThat(r.getTimeToLive(), is(discoveryResult.getTimeToLive()));
    public void testDiscoveryResultBuilderWithTTL() {
        DiscoveryResult otherDiscoveryResult = builder.withTTL(100L).build();
        assertThat(otherDiscoveryResult.getTimeToLive(), is(100L));
    public void testDiscoveryResultBuilderWithMatchingBridge() {
        DiscoveryResult otherDiscoveryResult = builder.withBridge(BRIDGE_UID).build();
        assertThat(otherDiscoveryResult.getBridgeUID(), is(BRIDGE_UID));
    public void testDiscoveryResultBuilderWithBridge() {
        DiscoveryResult otherDiscoveryResult = DiscoveryResultBuilder
                .create(new ThingUID(THING_TYPE_UID, "otherThingId")).withBridge(BRIDGE_UID).build();
    @Disabled
    public void subsequentBuildsCreateIndependentDiscoveryResults() {
        DiscoveryResult otherDiscoveryResult = builder.withLabel("Second Test").withProperties(Map.of()).build();
        assertThat(otherDiscoveryResult.getLabel(), is(not(discoveryResult.getLabel())));
        assertThat(otherDiscoveryResult.getProperties().size(), is(not(discoveryResult.getProperties().size())));
