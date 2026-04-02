 * The {@link EventWebSocketAdapter} allows subscription to oh events over WebSocket
@Component(immediate = true, service = { EventSubscriber.class, WebSocketAdapter.class })
public class EventWebSocketAdapter implements EventSubscriber, WebSocketAdapter {
    public static final String ADAPTER_ID = "events";
    private final Set<EventWebSocket> webSockets = new CopyOnWriteArraySet<>();
    public EventWebSocketAdapter(@Reference EventPublisher eventPublisher, @Reference ItemRegistry itemRegistry) {
        itemEventUtility = new ItemEventUtility(gson, itemRegistry);
        return Set.of(EventSubscriber.ALL_EVENT_TYPES);
        webSockets.forEach(ws -> ws.processEvent(event));
    public void registerListener(EventWebSocket eventWebSocket) {
        webSockets.add(eventWebSocket);
    public void unregisterListener(EventWebSocket eventWebSocket) {
        webSockets.remove(eventWebSocket);
        return new EventWebSocket(gson, EventWebSocketAdapter.this, itemEventUtility, eventPublisher);
