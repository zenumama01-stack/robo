import org.openhab.core.thing.events.ThingEventFactory;
 * Basic test cases for {@link ChannelEventTriggerHandler}
 * @author Thomas Weißschuh - Initial contribution
class ChannelEventTriggerHandlerTest {
    private @NonNullByDefault({}) ChannelEventTriggerHandler handler;
    private @NonNullByDefault({}) Trigger moduleMock;
    private @NonNullByDefault({}) BundleContext contextMock;
        moduleMock = mock(Trigger.class);
        contextMock = mock(BundleContext.class);
    public void testExactlyMatchingChannelIsApplied() {
        when(moduleMock.getConfiguration())
                .thenReturn(new Configuration(Map.of(ChannelEventTriggerHandler.CFG_CHANNEL, "foo:bar:baz:quux")));
        handler = new ChannelEventTriggerHandler(moduleMock, contextMock);
        assertTrue(handler.apply(ThingEventFactory.createTriggerEvent("PRESSED", new ChannelUID("foo:bar:baz:quux"))));
    public void testSubstringMatchingChannelIsNotApplied() {
                .thenReturn(new Configuration(Map.of(ChannelEventTriggerHandler.CFG_CHANNEL, "foo:bar:baz:q")));
        assertFalse(handler.apply(ThingEventFactory.createTriggerEvent("PRESSED", new ChannelUID("foo:bar:baz:quux"))));
