package org.openhab.core.audio.internal.webaudio;
 * This is an {@link org.openhab.core.events.Event Event} that is sent when a web client should play an audio stream
 * from a url.
public class PlayURLEvent extends AbstractEvent {
     * The extension event type.
    public static final String TYPE = PlayURLEvent.class.getSimpleName();
     * Constructs a new extension event object.
     * @param url the url to play
    public PlayURLEvent(String topic, String payload, String url) {
        return "Play URL '" + url + "'.";
