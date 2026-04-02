 * Marker interface for a publisher of events using SSE.
public interface SsePublisher {
    void broadcast(final Event event);
