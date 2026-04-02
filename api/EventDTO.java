package org.openhab.core.io.rest.sse.internal.dto;
 * Event bean for broadcasted events.
 * @author Dennis Nobel - Added event type and renamed object to payload
 * @author Markus Rathgeb - Follow the Data Transfer Objects Specification
public class EventDTO extends DTO {
    public String topic;
    public String payload;
package org.openhab.core.io.websocket.event;
 * The {@link EventDTO} is used for serialization and deserialization of events
public class EventDTO {
    public @Nullable String type;
    public @Nullable String topic;
    public @Nullable String payload;
    public @Nullable String source;
    public @Nullable String eventId;
    public EventDTO() {
    public EventDTO(String type, String topic, @Nullable String payload, @Nullable String source,
            @Nullable String eventId) {
        this.source = source;
        this.eventId = eventId;
    public EventDTO(Event event) {
        type = event.getType();
        topic = event.getTopic();
        source = event.getSource();
        payload = event.getPayload();
        EventDTO eventDTO = (EventDTO) o;
        return Objects.equals(type, eventDTO.type) && Objects.equals(topic, eventDTO.topic)
                && Objects.equals(payload, eventDTO.payload) && Objects.equals(source, eventDTO.source)
                && Objects.equals(eventId, eventDTO.eventId);
        return Objects.hash(type, topic, payload, source, eventId);
        return "EventDTO{type='" + type + "', topic='" + topic + "', payload='" + payload + "', source='" + source
                + "', eventId='" + eventId + "'}";
