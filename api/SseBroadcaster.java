 * We do not use the SseBroadcaster as it seems registered SseEventSinks are not removed if the peer terminates the
 * connection.
 * @param <I> the type of the SSE event sink specific information
public class SseBroadcaster<@NonNull I> implements Closeable {
    public interface Listener<I> {
        void sseEventSinkRemoved(final SseEventSink sink, I info);
    private final Logger logger = LoggerFactory.getLogger(SseBroadcaster.class);
    private final List<Listener<I>> listeners = new CopyOnWriteArrayList<>();
    private final Map<SseEventSink, I> sinks = new ConcurrentHashMap<>();
    public void addListener(final Listener<I> listener) {
    public void removeListener(final Listener<I> listener) {
    public @Nullable I add(final SseEventSink sink, final I info) {
        return sinks.put(sink, info);
    public @Nullable I remove(final SseEventSink sink) {
        return sinks.remove(sink);
    public @Nullable I getInfo(final SseEventSink sink) {
        return sinks.get(sink);
    public Stream<I> getInfoIf(Predicate<I> predicate) {
        return sinks.values().stream().filter(predicate);
        final Iterator<Entry<SseEventSink, I>> it = sinks.entrySet().iterator();
            final Entry<SseEventSink, I> entry = it.next();
            close(entry.getKey());
            notifyAboutRemoval(entry.getKey(), entry.getValue());
    public void send(final OutboundSseEvent event) {
        sendIf(event, info -> true);
    public void sendIf(final OutboundSseEvent event, Predicate<I> predicate) {
        logger.trace("broadcast to potential {} sinks", sinks.size());
        sinks.forEach((sink, info) -> {
            // Check if we should send at all.
            if (!predicate.test(info)) {
            if (sink.isClosed()) {
                // We are using a concurrent collection, so we are allowed to modify the collection asynchronous (we
                // don't know if there is currently an iteration in progress or not, but it does not matter).
                handleRemoval(sink);
            sink.send(event).exceptionally(throwable -> {
                logger.debug("Sending event to sink failed", throwable);
                close(sink);
    public void closeAndRemoveIf(Predicate<I> predicate) {
            if (predicate.test(info)) {
    private void close(final SseEventSink sink) {
            logger.debug("SSE event sink is already closed");
            logger.debug("Closing SSE event sink");
            sink.close();
            logger.debug("Closing SSE event sink failed. Nothing we can do here...", ex);
    private void handleRemoval(final SseEventSink sink) {
        final @Nullable I info = sinks.remove(sink);
            notifyAboutRemoval(sink, info);
    private void notifyAboutRemoval(final SseEventSink sink, I info) {
        listeners.forEach(listener -> listener.sseEventSinkRemoved(sink, info));
