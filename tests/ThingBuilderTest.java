public class ThingBuilderTest {
    private final Map<String, String> properties = Map.of(KEY1, VALUE1, KEY2, VALUE2);
    private @NonNullByDefault({}) ThingBuilder thingBuilder;
        thingBuilder = ThingBuilder.create(THING_TYPE_UID, THING_UID);
        assertThat(thingBuilder, is(instanceOf(ThingBuilder.class)));
        assertThat(thingBuilder.withLabel("TEST"), is(instanceOf(ThingBuilder.class)));
        assertThat(thingBuilder.build(), is(instanceOf(ThingImpl.class)));
        final BridgeBuilder bridgeBuilder = BridgeBuilder.create(THING_TYPE_UID, THING_UID);
        assertThat(bridgeBuilder, is(instanceOf(BridgeBuilder.class)));
        assertThat(bridgeBuilder.withLabel("TEST"), is(instanceOf(BridgeBuilder.class)));
        assertThat(bridgeBuilder.build(), is(instanceOf(BridgeImpl.class)));
    public void testWithChannelDuplicates() {
        thingBuilder.withChannel(ChannelBuilder.create(new ChannelUID(THING_UID, "channel1")).build());
                () -> thingBuilder.withChannel(ChannelBuilder.create(new ChannelUID(THING_UID, "channel1")).build()));
    public void testWithChannelsDuplicatesCollections() {
        assertThrows(IllegalArgumentException.class, () -> thingBuilder.withChannels(List.of( //
                ChannelBuilder.create(new ChannelUID(THING_UID, "channel1")).build(), //
                ChannelBuilder.create(new ChannelUID(THING_UID, "channel1")).build())));
    public void testWithChannelsDuplicatesVararg() {
        assertThrows(IllegalArgumentException.class, () -> thingBuilder.withChannels( //
                ChannelBuilder.create(new ChannelUID(THING_UID, "channel1")).build()));
    public void testWithoutChannel() {
        thingBuilder.withChannels( //
                ChannelBuilder.create(new ChannelUID(THING_UID, "channel2")).build());
        thingBuilder.withoutChannel(new ChannelUID(THING_UID, "channel1"));
        assertThat(thing.getChannels(), hasSize(1));
        assertThat(thing.getChannels().getFirst().getUID().getId(), is(equalTo("channel2")));
    public void testWithoutChannelMissing() {
        thingBuilder.withoutChannel(new ChannelUID(THING_UID, "channel3"));
        assertThat(thingBuilder.build().getChannels(), hasSize(2));
    public void testWithChannelWrongThing() {
        assertThrows(IllegalArgumentException.class, () -> thingBuilder.withChannel(
                ChannelBuilder.create(new ChannelUID(new ThingUID(THING_TYPE_UID, "wrong"), "channel1")).build()));
    public void subsequentBuildsCreateIndependentThings() {
        Thing thing = thingBuilder.withLabel("Test").withLocation("Some Place").withProperties(properties).build();
        Thing otherThing = thingBuilder.withLabel("Second Test").withLocation("Other Place").withProperties(Map.of())
        assertThat(otherThing.getLabel(), is(not(thing.getLabel())));
        assertThat(otherThing.getLocation(), is(not(thing.getLocation())));
        assertThat(otherThing.getProperties().size(), is(not(thing.getProperties().size())));
