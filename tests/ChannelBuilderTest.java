import static org.openhab.core.thing.DefaultSystemChannelTypeProvider.SYSTEM_OUTDOOR_TEMPERATURE;
 * Tests the {@link ChannelBuilder}.
public class ChannelBuilderTest {
    private static final String KEY3 = "key3";
    private static final Map<String, String> PROPERTIES = Map.of(KEY1, VALUE1, KEY2, VALUE2);
    private @NonNullByDefault({}) ChannelBuilder builder;
    private @NonNullByDefault({}) Channel channel;
        ThingType thingType = ThingTypeBuilder.instance(new ThingTypeUID("bindingId", "thingTypeId"), "thingLabel")
        ChannelUID channelUID = new ChannelUID(new ThingUID(thingType.getUID(), "thingId"), "temperature");
        builder = ChannelBuilder.create(channelUID, SYSTEM_OUTDOOR_TEMPERATURE.getItemType()).withLabel("Test")
                .withDescription("My test channel").withType(SYSTEM_OUTDOOR_TEMPERATURE.getUID())
                .withProperties(PROPERTIES);
        channel = builder.build();
    public void testChannelBuilder() {
        assertThat(channel.getAcceptedItemType(), is(SYSTEM_OUTDOOR_TEMPERATURE.getItemType()));
        assertThat(channel.getChannelTypeUID(), is(SYSTEM_OUTDOOR_TEMPERATURE.getUID()));
        assertThat(channel.getDescription(), is("My test channel"));
        assertThat(channel.getKind(), is(ChannelKind.STATE));
        assertThat(channel.getLabel(), is("Test"));
        assertThat(channel.getProperties().size(), is(2));
        assertThat(channel.getProperties().get(KEY1), is(VALUE1));
        assertThat(channel.getProperties().get(KEY2), is(VALUE2));
    public void testChannelBuilderFromChannel() {
        Channel otherChannel = ChannelBuilder.create(channel).build();
        assertThat(otherChannel.getAcceptedItemType(), is(channel.getAcceptedItemType()));
        assertThat(otherChannel.getChannelTypeUID(), is(channel.getChannelTypeUID()));
        assertThat(otherChannel.getConfiguration(), is(channel.getConfiguration()));
        assertThat(otherChannel.getDefaultTags(), hasSize(channel.getDefaultTags().size()));
        assertThat(otherChannel.getDescription(), is(channel.getDescription()));
        assertThat(otherChannel.getKind(), is(channel.getKind()));
        assertThat(otherChannel.getLabel(), is(channel.getLabel()));
        assertThat(otherChannel.getAutoUpdatePolicy(), is(channel.getAutoUpdatePolicy()));
        assertThat(otherChannel.getProperties().size(), is(channel.getProperties().size()));
        assertThat(otherChannel.getProperties().get(KEY1), is(channel.getProperties().get(KEY1)));
        assertThat(otherChannel.getProperties().get(KEY2), is(channel.getProperties().get(KEY2)));
        assertThat(otherChannel.getUID(), is(channel.getUID()));
    public void subsequentBuildsCreateIndependentChannels() {
        Channel otherChannel = builder.withLabel("Second Test").withDescription("My second test channel")
                .withAcceptedItemType(CoreItemFactory.NUMBER).withProperties(Map.of()).build();
        assertThat(otherChannel.getDescription(), is(not(channel.getDescription())));
        assertThat(otherChannel.getLabel(), is(not(channel.getLabel())));
        assertThat(otherChannel.getAcceptedItemType(), is(not(channel.getAcceptedItemType())));
        assertThat(otherChannel.getProperties().size(), is(not(channel.getProperties().size())));
    public void testChannelBuilderWithInvalidProperties() {
        Map<@Nullable String, @Nullable String> map = new HashMap<>();
        map.put(KEY1, null);
        map.put(KEY2, VALUE2);
        map.put(KEY3, null);
        map.put(null, VALUE1);
        IllegalArgumentException exception = assertThrows(IllegalArgumentException.class, () -> {
            builder.withProperties(toNonNullStringMap(map));
                "Unexpected properties (key1, key3, null) with null key or value for channel bindingId:thingTypeId:thingId:temperature",
                exception.getMessage());
    @SuppressWarnings("unchecked") // Map may contain null keys or values; this is intentional for this test
    private static Map<String, String> toNonNullStringMap(Map<@Nullable String, @Nullable String> source) {
        return (Map<String, String>) (Map<?, ?>) source;
