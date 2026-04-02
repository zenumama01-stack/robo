 * Event for configuration status information.
public final class ConfigStatusInfoEvent extends AbstractEvent {
    static final String TYPE = "ConfigStatusInfoEvent";
    private final ConfigStatusInfo configStatusInfo;
    private static final Gson GSON = new Gson();
     * Creates a new {@link ConfigStatusInfoEvent}.
     * @param configStatusInfo the corresponding configuration status information to be put as payload into the event
    public ConfigStatusInfoEvent(String topic, ConfigStatusInfo configStatusInfo) {
        super(topic, GSON.toJson(configStatusInfo), null);
        this.configStatusInfo = configStatusInfo;
        return configStatusInfo.toString();
