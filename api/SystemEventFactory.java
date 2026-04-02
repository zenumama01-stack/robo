 * Factory that creates system events.
public class SystemEventFactory extends AbstractEventFactory {
    static final String SYSTEM_STARTLEVEL_TOPIC = "openhab/system/startlevel";
    public SystemEventFactory() {
        super(Set.of(StartlevelEvent.TYPE));
     * Creates a trigger event from a {@link org.openhab.core.types.Type}.
     * @param startlevel Startlevel of system
     * @return Created start level event.
    public static StartlevelEvent createStartlevelEvent(Integer startlevel) {
        SystemEventPayloadBean bean = new SystemEventPayloadBean(startlevel);
        return new StartlevelEvent(SYSTEM_STARTLEVEL_TOPIC, payload, null, startlevel);
        return createStartlevelEvent(topic, payload, source);
     * Creates a startlevel event from a payload.
     * @param topic Event topic
     * @param source Event source
     * @param payload Payload
     * @return created startlevel event
    public StartlevelEvent createStartlevelEvent(String topic, String payload, @Nullable String source) {
        SystemEventPayloadBean bean = deserializePayload(payload, SystemEventPayloadBean.class);
        return new StartlevelEvent(topic, payload, source, bean.getStartlevel());
     * This is a java bean that is used to serialize/deserialize system event payload.
    public static class SystemEventPayloadBean {
        private @NonNullByDefault({}) Integer startlevel;
        protected SystemEventPayloadBean() {
        public SystemEventPayloadBean(Integer startlevel) {
