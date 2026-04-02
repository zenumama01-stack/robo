public class ThingImplTest {
    private static final String SECOND_CHANNEL_ID = "secondgroup#channel1";
    private static final ChannelUID SECOND_CHANNEL_UID = new ChannelUID(THING_UID, SECOND_CHANNEL_ID);
    public void testGetChannelMethods() {
        assertEquals(1, thing.getChannels().size());
        assertNull(thing.getChannel("channel1"));
        assertNotNull(thing.getChannel(FIRST_CHANNEL_UID));
        assertEquals(FIRST_CHANNEL_UID, thing.getChannel(FIRST_CHANNEL_UID).getUID());
        assertNotNull(thing.getChannel(FIRST_CHANNEL_ID));
        assertEquals(FIRST_CHANNEL_UID, thing.getChannel(FIRST_CHANNEL_ID).getUID());
    public void testGetGroupChannels() {
                .withChannel(ChannelBuilder.create(FIRST_CHANNEL_UID, CoreItemFactory.STRING).build())
                .withChannel(ChannelBuilder.create(SECOND_CHANNEL_UID, CoreItemFactory.STRING).build()).build();
        assertNotNull(thing.getChannel(SECOND_CHANNEL_UID));
        assertEquals(SECOND_CHANNEL_UID, thing.getChannel(SECOND_CHANNEL_UID).getUID());
        assertNotNull(thing.getChannel(SECOND_CHANNEL_ID));
        assertEquals(SECOND_CHANNEL_UID, thing.getChannel(SECOND_CHANNEL_ID).getUID());
