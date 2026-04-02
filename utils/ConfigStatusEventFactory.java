package org.openhab.core.config.core.status.events;
import org.openhab.core.config.core.status.ConfigStatusInfo;
 * The {@link ConfigStatusEventFactory} is the event factory implementation to create configuration status events, e.g.
 * for {@link ConfigStatusInfoEvent}.
@Component(immediate = true, service = EventFactory.class)
public final class ConfigStatusEventFactory extends AbstractEventFactory {
    private static final Set<String> SUPPORTED_EVENT_TYPES = Set.of(ConfigStatusInfoEvent.TYPE);
     * Creates a new {@link ConfigStatusEventFactory}.
    public ConfigStatusEventFactory() {
        super(SUPPORTED_EVENT_TYPES);
    private Event createStatusInfoEvent(String topic, String payload) throws Exception {
        if (topicElements.length != 5) {
            throw new IllegalArgumentException("ConfigStatusInfoEvent creation failed, invalid topic: " + topic);
        ConfigStatusInfo thingStatusInfo = deserializePayload(payload, ConfigStatusInfo.class);
        return new ConfigStatusInfoEvent(topic, thingStatusInfo);
        if (ConfigStatusInfoEvent.TYPE.equals(eventType)) {
            return createStatusInfoEvent(topic, payload);
                eventType + " not supported by " + ConfigStatusEventFactory.class.getSimpleName());
