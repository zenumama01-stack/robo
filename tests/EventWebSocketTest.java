import static org.mockito.Mockito.clearInvocations;
import static org.mockito.Mockito.never;
import static org.openhab.core.io.websocket.event.EventWebSocket.WEBSOCKET_TOPIC_PREFIX;
import org.openhab.core.io.websocket.event.EventDTO;
import org.openhab.core.io.websocket.event.ItemEventUtility;
 * The {@link EventWebSocketTest} contains tests for the {@link EventWebSocket}
 * @author Florian Hotze - Add topic filter tests
public class EventWebSocketTest {
    private static final String REMOTE_WEBSOCKET_IMPLEMENTATION = "fooWebsocket";
    private static final String TEST_ITEM_NAME = "testItem";
    private static final String REX_TEST_ITEM_NAME = "rexTestItem";
    private static final NumberItem TEST_ITEM = new NumberItem(TEST_ITEM_NAME);
    private Gson gson = new Gson();
    private @Mock @NonNullByDefault({}) EventWebSocketAdapter servlet;
    private @Mock @NonNullByDefault({}) ItemRegistry itemRegistry;
    private @Mock @NonNullByDefault({}) RemoteEndpoint remoteEndpoint;
    private @NonNullByDefault({}) ItemEventUtility itemEventUtility;
    private @NonNullByDefault({}) EventWebSocket eventWebSocket;
        eventWebSocket = new EventWebSocket(gson, servlet, itemEventUtility, eventPublisher);
        when(session.getRemote()).thenReturn(remoteEndpoint);
        when(remoteEndpoint.getInetSocketAddress()).thenReturn(new InetSocketAddress(47115));
        when(itemRegistry.getItem(eq(TEST_ITEM_NAME))).thenReturn(TEST_ITEM);
        eventWebSocket.onConnect(session);
        verify(servlet).registerListener(eventWebSocket);
    public void listenerCorrectlyUnregisteredOnClose() {
        eventWebSocket.onClose(StatusCode.NORMAL, "Normal close.");
        verify(servlet).unregisterListener(eventWebSocket);
    public void sessionClosesOnErrorAndOnCloseCalled() {
        eventWebSocket.onError(session, new IllegalStateException());
        verify(session).close();
    public void stateEventWithIdFromWebsocketIsPublishedAndConfirmed() throws IOException {
        Event expectedEvent = ItemEventFactory.createStateEvent(TEST_ITEM_NAME, DecimalType.ZERO,
                REMOTE_WEBSOCKET_IMPLEMENTATION);
        EventDTO eventDTO = new EventDTO(expectedEvent);
        eventDTO.eventId = "id-1";
        EventDTO expectedResponse = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "response/success", "",
                null, eventDTO.eventId);
        assertEventProcessing(eventDTO, expectedEvent, expectedResponse);
    public void stateEventWithoutIdFromWebsocketIsPublished() throws IOException {
        assertEventProcessing(eventDTO, expectedEvent, null);
    public void commandEventWithIdFromWebsocketIsPublishedAndConfirmed() throws IOException {
        Event expectedEvent = ItemEventFactory.createCommandEvent(TEST_ITEM_NAME, DecimalType.ZERO,
    public void commandEventWithoutIdFromWebsocketIsPublished() throws IOException {
    public void illegalStateEventNotPublishedAndResponseSent() throws IOException {
        eventDTO.payload = "";
        EventDTO expectedResponse = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "response/failed",
                "Processing error: Failed to deserialize payload \u0027\u0027.", null, null);
        assertEventProcessing(eventDTO, null, expectedResponse);
    public void illegalCommandEventNotPublishedAndResponseSent() throws IOException {
        eventDTO.topic = "";
                "Processing error: Topic must follow the format {namespace}/{entityType}/{entity}/{action}.", null,
                eventDTO.eventId);
    public void heartBeat() throws IOException {
        EventDTO eventDTO = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "heartbeat", "PING", null,
        EventDTO expectedResponse = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "heartbeat", "PONG",
    public void eventFromBusSent() throws IOException {
        Event event = ItemEventFactory.createCommandEvent(TEST_ITEM_NAME, DecimalType.ZERO,
        eventWebSocket.processEvent(event);
        EventDTO eventDTO = new EventDTO(event);
        verify(remoteEndpoint).sendString(eq(gson.toJson(eventDTO)), any());
    public void eventFromBusFilterType() throws IOException {
        EventDTO eventDTO = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "filter/type",
                "[\"ItemCommandEvent\"]", null, null);
        EventDTO responseEventDTO = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "filter/type",
                eventDTO.payload, null, null);
        eventWebSocket.onText(gson.toJson(eventDTO));
        verify(remoteEndpoint).sendString(eq(gson.toJson(responseEventDTO)), any());
        // subscribed type is sent
        verify(remoteEndpoint).sendString(eq(gson.toJson(new EventDTO(event))), any());
        // not subscribed event not sent
        event = ItemEventFactory.createStateEvent(TEST_ITEM_NAME, DecimalType.ZERO, REMOTE_WEBSOCKET_IMPLEMENTATION);
        verify(remoteEndpoint, times(2)).sendString(any(), any());
    public void eventFromBusFilterSource() throws IOException {
        EventDTO eventDTO = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "filter/source",
                "[\"" + REMOTE_WEBSOCKET_IMPLEMENTATION + "\"]", null, null);
        EventDTO responseEventDTO = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "filter/source",
        // non-matching is sent
        Event event = ItemEventFactory.createCommandEvent(TEST_ITEM_NAME, DecimalType.ZERO);
        // matching is not sent
    public void eventFromBusFilterIncludeTopic() throws IOException {
        EventDTO eventDTO = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "filter/topic",
                // topic filters defined:
                // - openhab/items/{TEST_ITEM_NAME}/command => a single topic
                // - openhab/items/*/statechanged => wildcard => matches ItemStateChangedEvent &
                // GroupItemStateChangedEvent for all Items
                // - openhab/items/rex[^/]*/state => RegEx => matches ItemStateEvent for all Items starting with "rex"
                "[\"openhab/items/" + TEST_ITEM_NAME + "/command\", " + //
                        "\"openhab/items/*/statechanged\", " + //
                        "\"openhab/items/rex[^/]*/state\"]",
        EventDTO responseEventDTO = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "filter/topic",
        clearInvocations(remoteEndpoint);
        // subscribed topics are sent
        event = ItemEventFactory.createStateChangedEvent(TEST_ITEM_NAME, DecimalType.ZERO, DecimalType.ZERO, null,
        event = ItemEventFactory.createStateEvent(REX_TEST_ITEM_NAME, DecimalType.ZERO,
        // not subscribed topics are not sent
        event = ItemEventFactory.createCommandEvent(REX_TEST_ITEM_NAME, DecimalType.ZERO,
        verify(remoteEndpoint, never()).sendString(any(), any());
    public void eventFromBusFilterExcludeTopic() throws IOException {
                // - !openhab/items/{TEST_ITEM_NAME}/command => a single topic
                // - !openhab/items/rex[^/]*/state => RegEx => matches ItemStateEvent for all Items starting with "rex"
                "[\"!openhab/items/" + TEST_ITEM_NAME + "/command\", \"!openhab/items/rex[^/]*/state\"]", null, null);
        // excluded topics are not sent
        // not excluded topics are sent
        event = ItemEventFactory.createStateEvent("anotherItem", DecimalType.ZERO, REMOTE_WEBSOCKET_IMPLEMENTATION);
    public void eventFromBusFilterIncludeAndExcludeTopic() throws IOException {
                "[\"openhab/items/*/*\", \"!openhab/items/*/command\"]", null, null);
        // included topics are sent
        Event event = ItemEventFactory.createStateChangedEvent(TEST_ITEM_NAME, DecimalType.ZERO, DecimalType.ZERO, null,
        // excluded sub-topics are not sent
        event = ItemEventFactory.createCommandEvent(TEST_ITEM_NAME, DecimalType.ZERO, REMOTE_WEBSOCKET_IMPLEMENTATION);
    private void assertEventProcessing(EventDTO incoming, @Nullable Event expectedEvent,
            @Nullable EventDTO expectedResponse) throws IOException {
        eventWebSocket.onText(gson.toJson(incoming));
        if (expectedEvent != null) {
            verify(eventPublisher).post(eq(Objects.requireNonNull(expectedEvent)));
            verify(eventPublisher, never()).post(any());
        if (expectedResponse != null) {
            String expectedResponseString = gson.toJson(expectedResponse);
            verify(remoteEndpoint).sendString(eq(expectedResponseString), any());
            verify(remoteEndpoint, never()).sendString(any());
