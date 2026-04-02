import org.eclipse.jetty.websocket.api.StatusCode;
import org.eclipse.jetty.websocket.api.WriteCallback;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketClose;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketConnect;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketError;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketMessage;
 * The {@link EventWebSocket} is the WebSocket implementation that extends the event bus
 * @author Florian Hotze - Add topic filter
public class EventWebSocket implements WriteCallback {
    public static final String WEBSOCKET_EVENT_TYPE = "WebSocketEvent";
    public static final String WEBSOCKET_TOPIC_PREFIX = "openhab/websocket/";
    private static final Type STRING_LIST_TYPE = TypeToken.getParameterized(List.class, String.class).getType();
    private final Logger logger = LoggerFactory.getLogger(EventWebSocket.class);
    private final EventWebSocketAdapter wsAdapter;
    private final ItemEventUtility itemEventUtility;
    private @Nullable Session session;
    private @Nullable RemoteEndpoint remoteEndpoint;
    private String remoteIdentifier = "<unknown>";
    private List<String> typeFilter = List.of();
    private List<String> sourceFilter = List.of();
    private @Nullable TopicEventFilter topicIncludeFilter = null;
    private @Nullable TopicEventFilter topicExcludeFilter = null;
    public EventWebSocket(Gson gson, EventWebSocketAdapter wsAdapter, ItemEventUtility itemEventUtility,
            EventPublisher eventPublisher) {
        this.itemEventUtility = itemEventUtility;
    @OnWebSocketClose
    public void onClose(int statusCode, String reason) {
        this.wsAdapter.unregisterListener(this);
            remoteIdentifier = "<unknown>";
            this.remoteEndpoint = null;
    @OnWebSocketConnect
    public void onConnect(Session session) {
        RemoteEndpoint remoteEndpoint = session.getRemote();
            this.remoteEndpoint = remoteEndpoint;
            this.remoteIdentifier = remoteEndpoint.getInetSocketAddress().toString();
        this.wsAdapter.registerListener(this);
    @OnWebSocketMessage
    public void onText(String message) {
        RemoteEndpoint remoteEndpoint;
        Session session;
        String remoteIdentifier;
            remoteEndpoint = this.remoteEndpoint;
            session = this.session;
            remoteIdentifier = this.remoteIdentifier;
        if (session == null || remoteEndpoint == null) {
            // no connection or no remote endpoint , do nothing this is possible due to async behavior
        EventDTO responseEvent;
            EventDTO eventDTO = gson.fromJson(message, EventDTO.class);
                if (eventDTO == null) {
                    throw new EventProcessingException("Deserialized event must not be null");
                String type = eventDTO.type;
                    throw new EventProcessingException("Event type must not be null.");
                    case "ItemCommandEvent":
                        Event itemCommandEvent = itemEventUtility.createCommandEvent(eventDTO);
                        responseEvent = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "response/success",
                                "", null, eventDTO.eventId);
                    case "ItemStateEvent":
                        Event itemStateEvent = itemEventUtility.createStateEvent(eventDTO);
                    case "ItemTimeSeriesEvent":
                        Event itemTimeseriesEvent = itemEventUtility.createTimeSeriesEvent(eventDTO);
                        eventPublisher.post(itemTimeseriesEvent);
                    case WEBSOCKET_EVENT_TYPE:
                        if ((WEBSOCKET_TOPIC_PREFIX + "heartbeat").equals(eventDTO.topic)
                                && "PING".equals(eventDTO.payload)) {
                            responseEvent = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "heartbeat",
                                    "PONG", null, eventDTO.eventId);
                        } else if ((WEBSOCKET_TOPIC_PREFIX + "filter/type").equals(eventDTO.topic)) {
                                typeFilter = Objects.requireNonNullElse(
                                        gson.fromJson(eventDTO.payload, STRING_LIST_TYPE), List.of());
                                    logger.debug("Setting type filter for connection to {}: {}",
                                            remoteEndpoint.getInetSocketAddress(), typeFilter);
                            responseEvent = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "filter/type",
                                    eventDTO.payload, null, eventDTO.eventId);
                        } else if ((WEBSOCKET_TOPIC_PREFIX + "filter/source").equals(eventDTO.topic)) {
                                sourceFilter = Objects.requireNonNullElse(
                                    logger.debug("Setting source filter for connection to {}: {}",
                                            remoteEndpoint.getInetSocketAddress(), sourceFilter);
                            responseEvent = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "filter/source",
                        } else if ((WEBSOCKET_TOPIC_PREFIX + "filter/topic").equals(eventDTO.topic)) {
                            List<String> topics = Objects
                                    .requireNonNullElse(gson.fromJson(eventDTO.payload, STRING_LIST_TYPE), List.of());
                            TopicEventFilter includeFilter = TopicFilterMapper.mapTopicsToIncludeFilter(topics);
                            TopicEventFilter excludeFilter = TopicFilterMapper.mapTopicsToExcludeFilter(topics);
                            if (includeFilter != null || excludeFilter != null) {
                                    topicIncludeFilter = includeFilter;
                                    topicExcludeFilter = excludeFilter;
                                    logger.debug("Setting topic filter for connection to {}: {}",
                                            remoteEndpoint.getInetSocketAddress(), topics);
                            responseEvent = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "filter/topic",
                            throw new EventProcessingException("Invalid topic or payload in WebSocketEvent");
                        throw new EventProcessingException("Unknown event type '" + eventDTO.type + "'");
                if (!WEBSOCKET_EVENT_TYPE.equals(type) && responseEvent.eventId == null) {
                    // skip only for successful processing of state/command, always send response if processing failed
                    logger.trace("Not sending response event {}, because no eventId present.", responseEvent);
            } catch (EventProcessingException | JsonParseException e) {
                logger.warn("Failed to process deserialized event '{}': {}", message, e.getMessage());
                responseEvent = new EventDTO(WEBSOCKET_EVENT_TYPE, WEBSOCKET_TOPIC_PREFIX + "response/failed",
                        "Processing error: " + e.getMessage(), null, eventDTO != null ? eventDTO.eventId : "");
            logger.warn("Could not deserialize '{}'", message);
                    "Deserialization error: " + e.getMessage(), null, null);
        sendMessage(gson.toJson(responseEvent), remoteEndpoint);
    @OnWebSocketError
    public void onError(@Nullable Session session, @Nullable Throwable error) {
        String message = error == null ? "<null>" : Objects.requireNonNullElse(error.getMessage(), "<null>");
        logger.info("WebSocket error: {}", message);
        onClose(StatusCode.NO_CODE, message);
    public void processEvent(Event event) {
        List<String> typeFilter;
        List<String> sourceFilter;
        TopicEventFilter topicIncludeFilter;
        TopicEventFilter topicExcludeFilter;
            typeFilter = this.typeFilter;
            sourceFilter = this.sourceFilter;
            topicIncludeFilter = this.topicIncludeFilter;
            topicExcludeFilter = this.topicExcludeFilter;
        if (remoteEndpoint == null) {
            logger.warn("Could not determine remote endpoint for event '{}'", event);
        String source = event.getSource();
        if ((source == null || !sourceFilter.contains(event.getSource()))
                && (typeFilter.isEmpty() || typeFilter.contains(event.getType()))
                && (topicIncludeFilter == null || topicIncludeFilter.apply(event))
                && (topicExcludeFilter == null || !topicExcludeFilter.apply(event))) {
            sendMessage(gson.toJson(new EventDTO(event)), remoteEndpoint);
    private void sendMessage(String message, RemoteEndpoint remoteEndpoint) {
        remoteEndpoint.sendString(message, this);
    public void writeFailed(@Nullable Throwable x) {
        logger.debug("Failed to send websocket message to '{}': {}", remoteIdentifier,
                x == null ? "No information" : x.getMessage());
    public void writeSuccess() {
