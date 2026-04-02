package org.openhab.core.io.rest.sse.internal.listeners;
 * The {@link SseEventSubscriber} is responsible for broadcasting openHAB events
 * to currently listening SSE clients.
 * @author Yannick Schaus - Broadcast state events to the specialized ItemStatesSseBroadcaster
public class SseEventSubscriber implements EventSubscriber {
    private final SsePublisher ssePublisher;
    public SseEventSubscriber(final @Reference SsePublisher ssePublisher) {
        this.ssePublisher = ssePublisher;
        ssePublisher.broadcast(event);
