 * Basic test cases for {@link GenericEventTriggerHandler}
 * @author Cody Cutrer - Initial contribution
class GenericEventTriggerHandlerTest {
    private @NonNullByDefault({}) GenericEventTriggerHandler handler;
    public Event createEvent(String topic, String source) {
        Event event = mock(Event.class);
        when(event.getTopic()).thenReturn(topic);
        when(event.getSource()).thenReturn(source);
    public void testTopicFilterIsGlobbed() {
        when(moduleMock.getConfiguration()).thenReturn(new Configuration(Map.of(GenericEventTriggerHandler.CFG_TOPIC,
                "openhab/items/*/command", GenericEventTriggerHandler.CFG_SOURCE, "",
                GenericEventTriggerHandler.CFG_TYPES, "", GenericEventTriggerHandler.CFG_PAYLOAD, "")));
        handler = new GenericEventTriggerHandler(moduleMock, contextMock);
        assertTrue(handler.apply(createEvent("openhab/items/myMotion1/command", "Source")));
    public void testsSourceFilterIsExactMatch() {
        when(moduleMock.getConfiguration()).thenReturn(new Configuration(
                Map.of(GenericEventTriggerHandler.CFG_TOPIC, "", GenericEventTriggerHandler.CFG_SOURCE, "ExactSource",
        assertTrue(handler.apply(createEvent("openhab/items/myMotion1/command", "ExactSource")));
    public void testsSourceFilterDoesntMatchSubstring() {
                Map.of(GenericEventTriggerHandler.CFG_TOPIC, "", GenericEventTriggerHandler.CFG_SOURCE, "Source",
        assertFalse(handler.apply(createEvent("openhab/items/myMotion1/command", "ExactSource")));
