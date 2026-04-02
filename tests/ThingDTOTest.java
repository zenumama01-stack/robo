 * This is the test class for {@link ThingDTO}.
public class ThingDTOTest {
    public void testThingDTOMappingIsBidirectional() {
        Thing subject = ThingBuilder.create(THING_TYPE_UID, THING_UID).withLabel("Test")
                .withBridge(new ThingUID(new ThingTypeUID("binding-id", "bridge-type-id"), "bridge-id"))
                .withChannels(
                        ChannelBuilder.create(new ChannelUID(THING_UID, "channel1"), CoreItemFactory.STRING).build(),
                        ChannelBuilder.create(new ChannelUID(THING_UID, "channel2"), CoreItemFactory.STRING).build())
                .withLocation("Somewhere over the rainbow").withSemanticEquipmentTag("MotionDetector").build();
        Thing result = ThingDTOMapper.map(ThingDTOMapper.map(subject), false);
        assertThat(result, is(instanceOf(ThingImpl.class)));
        assertThat(result.getUID(), is(THING_UID));
        assertThat(result.getBridgeUID(), is(subject.getBridgeUID()));
        assertThatChannelsArePresent(result.getChannels(), subject.getChannels());
        assertThat(result.getLocation(), is(subject.getLocation()));
        assertThat(result.getSemanticEquipmentTag(), is(subject.getSemanticEquipmentTag()));
    public void testBridgeDTOMappingIsBidirectional() {
        Bridge subject = BridgeBuilder.create(THING_TYPE_UID, THING_UID).build();
        Thing result = ThingDTOMapper.map(ThingDTOMapper.map(subject), true);
        assertThat(result, is(instanceOf(BridgeImpl.class)));
    private void assertThatChannelsArePresent(List<Channel> actual, List<Channel> expected) {
        assertThat(actual, hasSize(expected.size()));
        actual.stream().map(channel -> channel.getUID()).forEach(uid -> {
            assertThat(expected.stream().filter(channel -> uid.equals(channel.getUID())).findFirst().orElse(null),
