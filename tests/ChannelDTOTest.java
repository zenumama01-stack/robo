 * This is the test class for {@link ChannelDTO}.
public class ChannelDTOTest {
    private static final ThingTypeUID THING_TYPE_UID = new ThingTypeUID("binding-id", "thing-type-id");
    private static final ThingUID THING_UID = new ThingUID(THING_TYPE_UID, "thing-id");
    private static final ChannelTypeUID CHANNEL_TYPE_UID = new ChannelTypeUID("binding-id", "channel-type-id");
    private static final ChannelUID CHANNEL_UID = new ChannelUID(THING_UID, "channel1");
    private final Map<String, String> properties = Map.of("key1", "value1");
    private final Set<String> tags = Set.of("tag1");
    public void testChannelDTOMappingIsBidirectional() {
        Channel subject = ChannelBuilder.create(CHANNEL_UID, CoreItemFactory.STRING).withType(CHANNEL_TYPE_UID)
                .withLabel("Test").withDescription("My test channel")
                .withConfiguration(new Configuration(Map.of("param1", "value1"))).withProperties(properties)
                .withDefaultTags(tags).withAutoUpdatePolicy(AutoUpdatePolicy.VETO).build();
        Channel result = ChannelDTOMapper.map(ChannelDTOMapper.map(subject));
        assertThat(result, is(instanceOf(Channel.class)));
        assertThat(result.getChannelTypeUID(), is(CHANNEL_TYPE_UID));
        assertThat(result.getUID(), is(CHANNEL_UID));
        assertThat(result.getAcceptedItemType(), is(subject.getAcceptedItemType()));
        assertThat(result.getKind(), is(subject.getKind()));
        assertThat(result.getLabel(), is(subject.getLabel()));
        assertThat(result.getDescription(), is(subject.getDescription()));
        assertThat(result.getConfiguration(), is(subject.getConfiguration()));
        assertThat(result.getProperties().values(), hasSize(1));
        assertThat(result.getProperties(), is(subject.getProperties()));
        assertThat(result.getDefaultTags(), hasSize(1));
        assertThat(result.getDefaultTags(), is(subject.getDefaultTags()));
        assertThat(result.getAutoUpdatePolicy(), is(subject.getAutoUpdatePolicy()));
