 * Tests for class {@link ChannelUID}.
public class ChannelUIDTest {
    private static final String CHANNEL_ID = "id";
        assertThrows(IllegalArgumentException.class, () -> new ChannelUID(THING_UID, "id_with_invalidchar%"));
        assertThrows(IllegalArgumentException.class, () -> new ChannelUID("binding:thing-type:group#id"));
    public void testMultipleChannelGroupSeparators() {
                () -> new ChannelUID("binding:thing-type:thing:group#id#what_ever"));
    public void testChannelUID() {
        ChannelUID channelUID = new ChannelUID(THING_UID, CHANNEL_ID);
        assertEquals("binding:thing-type:thing:id", channelUID.toString());
        assertFalse(channelUID.isInGroup());
        assertEquals(CHANNEL_ID, channelUID.getId());
        assertEquals(CHANNEL_ID, channelUID.getIdWithoutGroup());
        assertNull(channelUID.getGroupId());
        assertEquals(THING_UID, channelUID.getThingUID());
    public void testChannelUIDWithGroup() {
        ChannelUID channelUID = new ChannelUID(THING_UID, GROUP_ID, CHANNEL_ID);
        assertEquals("binding:thing-type:thing:group#id", channelUID.toString());
        assertTrue(channelUID.isInGroup());
        assertEquals("group#id", channelUID.getId());
        assertEquals(GROUP_ID, channelUID.getGroupId());
        assertThrows(IllegalArgumentException.class, () -> new ChannelUID("binding:thing-type::channel"));
        assertThrows(IllegalArgumentException.class, () -> new ChannelUID("binding:thing-type:bridge::channel"));
