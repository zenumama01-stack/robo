package org.openhab.core.events.system;
 * {@link StartlevelEvent}s will be delivered through the openHAB event bus if the start level of the system has
 * changed.
public class StartlevelEvent extends AbstractEvent {
    public static final String TYPE = StartlevelEvent.class.getSimpleName();
     * Creates a new system startlevel event object.
     * @param startlevel the system startlevel
    protected StartlevelEvent(String topic, String payload, @Nullable String source, Integer startlevel) {
        this.startlevel = startlevel;
     * Gets the system startlevel.
     * @return the startlevel
    public Integer getStartlevel() {
        return startlevel;
        return String.format("Startlevel '%d' reached.", startlevel);
