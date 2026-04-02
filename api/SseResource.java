package org.openhab.core.io.rest.sse;
import static org.openhab.core.io.rest.sse.internal.SseSinkItemInfo.*;
import static org.openhab.core.io.rest.sse.internal.SseSinkTopicInfo.matchesTopic;
import org.openhab.core.io.rest.sse.internal.SseItemStatesEventBuilder;
import org.openhab.core.io.rest.sse.internal.SsePublisher;
import org.openhab.core.io.rest.sse.internal.SseSinkItemInfo;
import org.openhab.core.io.rest.sse.internal.SseSinkTopicInfo;
import org.openhab.core.io.rest.sse.internal.dto.EventDTO;
import org.openhab.core.io.rest.sse.internal.util.SseUtil;
 * SSE Resource for pushing events to currently listening clients.
 * @author Ivan Iliev - Initial contribution
 * @author Yannick Schaus - Add endpoints to track item state updates
 * @author Markus Rathgeb - Drop Glassfish dependency and use API only
 * @author Wouter Born - Rework SSE item state sinks for dropping Glassfish
@Component(service = { RESTResource.class, SsePublisher.class })
@JaxrsName(SseResource.PATH_EVENTS)
@Path(SseResource.PATH_EVENTS)
@Tag(name = SseResource.PATH_EVENTS)
public class SseResource implements RESTResource, SsePublisher {
    public static final String PATH_EVENTS = "events";
    public static final int ALIVE_INTERVAL_SECONDS = 10;
    private final Logger logger = LoggerFactory.getLogger(SseResource.class);
    private final ScheduledFuture<?> aliveEventJob;
    private @Context @NonNullByDefault({}) Sse sse;
    private final SseBroadcaster<SseSinkItemInfo> itemStatesBroadcaster = new SseBroadcaster<>();
    private final SseItemStatesEventBuilder itemStatesEventBuilder;
    private final SseBroadcaster<SseSinkTopicInfo> topicBroadcaster = new SseBroadcaster<>();
    private ExecutorService executorService;
    public SseResource(@Reference SseItemStatesEventBuilder itemStatesEventBuilder) {
        this.executorService = Executors.newSingleThreadExecutor();
        this.itemStatesEventBuilder = itemStatesEventBuilder;
        aliveEventJob = scheduler.scheduleWithFixedDelay(() -> {
            if (sse != null) {
                logger.debug("Sending alive event to SSE connections");
                OutboundSseEvent aliveEvent = sse.newEventBuilder().name("alive")
                        .mediaType(MediaType.APPLICATION_JSON_TYPE).data(new AliveEvent()).build();
                itemStatesBroadcaster.send(aliveEvent);
                topicBroadcaster.send(aliveEvent);
        }, 1, ALIVE_INTERVAL_SECONDS, TimeUnit.SECONDS);
        itemStatesBroadcaster.close();
        topicBroadcaster.close();
        executorService.shutdown();
        aliveEventJob.cancel(true);
    public void broadcast(Event event) {
        executorService.execute(() -> {
            handleEventBroadcastTopic(event);
            if (event instanceof ItemStateChangedEvent changedEvent) {
                handleEventBroadcastItemState(changedEvent);
    private void addCommonResponseHeaders(final HttpServletResponse response) {
        // We want to make sure that the response is not compressed and buffered so that the client receives server sent
        // events at the moment of sending them.
        response.addHeader(HttpHeaders.CONTENT_ENCODING, "identity");
            response.flushBuffer();
            logger.trace("flush buffer failed", ex);
    @Operation(operationId = "getEvents", summary = "Get all events.", responses = {
            @ApiResponse(responseCode = "400", description = "Topic is empty or contains invalid characters") })
    public void listen(@Context final SseEventSink sseEventSink, @Context final HttpServletResponse response,
            @QueryParam("topics") @Parameter(description = "topics") String eventFilter) {
        if (!SseUtil.isValidTopicFilter(eventFilter)) {
        topicBroadcaster.add(sseEventSink, new SseSinkTopicInfo(eventFilter));
        addCommonResponseHeaders(response);
    private void handleEventBroadcastTopic(Event event) {
        final EventDTO eventDTO = SseUtil.buildDTO(event);
        final OutboundSseEvent sseEvent = SseUtil.buildEvent(sse.newEventBuilder(), eventDTO);
        topicBroadcaster.sendIf(sseEvent, matchesTopic(eventDTO.topic));
     * Subscribes the connecting client for state updates. It will initially only send a "ready" event with a unique
     * connectionId that the client can use to dynamically alter the list of tracked items.
    @Path("/states")
    @Operation(operationId = "initNewStateTacker", summary = "Initiates a new item state tracker connection", responses = {
    public void getStateEvents(@Context final SseEventSink sseEventSink, @Context final HttpServletResponse response) {
        final SseSinkItemInfo sinkItemInfo = new SseSinkItemInfo();
        itemStatesBroadcaster.add(sseEventSink, sinkItemInfo);
        String connectionId = sinkItemInfo.getConnectionId();
        OutboundSseEvent readyEvent = sse.newEventBuilder().id("0").name("ready").data(connectionId).build();
        itemStatesBroadcaster.sendIf(readyEvent, hasConnectionId(connectionId));
     * Alters the list of tracked items for a given state update connection
     * @param connectionId the connection Id to change
     * @param itemNames the list of items to track
    @Path("/states/{connectionId}")
    @Operation(operationId = "updateItemListForStateUpdates", summary = "Changes the list of items a SSE connection will receive state updates to.", responses = {
            @ApiResponse(responseCode = "404", description = "Unknown connectionId") })
    public Object updateTrackedItems(@PathParam("connectionId") @Nullable String connectionId,
            @Parameter(description = "items") @Nullable Set<String> itemNames) {
        if (connectionId == null) {
        Optional<SseSinkItemInfo> itemStateInfo = itemStatesBroadcaster.getInfoIf(hasConnectionId(connectionId))
                .findFirst();
        if (itemStateInfo.isEmpty()) {
        Set<String> trackedItemNames = (itemNames == null) ? Set.of() : itemNames;
        itemStateInfo.get().updateTrackedItems(trackedItemNames);
        OutboundSseEvent itemStateEvent = itemStatesEventBuilder.buildEvent(sse.newEventBuilder(), trackedItemNames);
        if (itemStateEvent != null) {
            itemStatesBroadcaster.sendIf(itemStateEvent, hasConnectionId(connectionId));
     * Broadcasts a state event to all currently listening clients, after transforming it to a simple map.
     * @param stateChangeEvent the {@link ItemStateChangedEvent} containing the new state
    public void handleEventBroadcastItemState(final ItemStateChangedEvent stateChangeEvent) {
        String itemName = stateChangeEvent.getItemName();
        boolean isTracked = itemStatesBroadcaster.getInfoIf(info -> true).anyMatch(tracksItem(itemName));
        if (isTracked) {
            OutboundSseEvent event = itemStatesEventBuilder.buildEvent(sse.newEventBuilder(), Set.of(itemName));
                itemStatesBroadcaster.sendIf(event, tracksItem(itemName));
    private static class AliveEvent {
        public final String type = "ALIVE";
        public final int interval = ALIVE_INTERVAL_SECONDS;
