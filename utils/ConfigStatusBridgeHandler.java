import org.openhab.core.config.core.status.ConfigStatusCallback;
 * The {@link ConfigStatusBridgeHandler} is an extension of {@link BaseBridgeHandler} that implements the
 * {@link ConfigStatusProvider} interface. It provides default implementations for
 * <li>{@link ConfigStatusProvider#supportsEntity(String)}</li>
 * <li>{@link ConfigStatusProvider#setConfigStatusCallback(ConfigStatusCallback)}</li>
 * Furthermore it overwrites {@link ThingHandler#handleConfigurationUpdate(Map)} and
 * {@link BaseBridgeHandler#updateConfiguration(Configuration)} to initiate a propagation of a new
 * configuration status. So sub classes need only to provide the current configuration status by implementing
 * {@link ConfigStatusProvider#getConfigStatus()}.
public abstract class ConfigStatusBridgeHandler extends BaseBridgeHandler implements ConfigStatusProvider {
    private @Nullable ConfigStatusCallback configStatusCallback;
     * Creates a new instance of this class for the given {@link Bridge}.
     * @param bridge the bridge for this handler
    public ConfigStatusBridgeHandler(Bridge bridge) {
    public boolean supportsEntity(String entityId) {
        return getThing().getUID().getAsString().equals(entityId);
    public void setConfigStatusCallback(@Nullable ConfigStatusCallback configStatusCallback) {
        this.configStatusCallback = configStatusCallback;
        if (configStatusCallback != null) {
            configStatusCallback.configUpdated(new ThingConfigStatusSource(getThing().getUID().getAsString()));
        super.updateConfiguration(configuration);
