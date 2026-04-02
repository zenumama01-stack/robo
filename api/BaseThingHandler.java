import org.openhab.core.thing.binding.builder.ThingStatusInfoBuilder;
import org.openhab.core.thing.util.ThingHandlerHelper;
 * {@link BaseThingHandler} provides a base implementation for the {@link ThingHandler} interface.
 * The default behavior for {@link Thing} updates is to {@link #dispose()} this handler first, exchange the
 * {@link Thing} and {@link #initialize()} it again. Override the method {@link #thingUpdated(Thing)} to change the
 * default behavior.
 * It is recommended to extend this abstract base class, because it covers a lot of common logic.
 * @author Thomas Höfer - Added thing properties and config description validation
 * @author Stefan Bußweiler - Added new thing status handling, refactorings thing/bridge life cycle
 * @author Kai Kreuzer - Refactored isLinked method to not use deprecated functions anymore
 * @author Christoph Weitkamp - Moved OSGI ServiceTracker from BaseThingHandler to ThingHandlerCallback
 * @author Jan N. Klug - added time series support
public abstract class BaseThingHandler implements ThingHandler {
    private static final String THING_HANDLER_THREADPOOL_NAME = "thingHandler";
    private final Logger logger = LoggerFactory.getLogger(BaseThingHandler.class);
    protected final ScheduledExecutorService scheduler = ThreadPoolManager
            .getScheduledPool(THING_HANDLER_THREADPOOL_NAME);
    protected Thing thing;
    private @Nullable ThingHandlerCallback callback;
     * Creates a new instance of this class for the {@link Thing}.
     * @param thing the thing that should be handled, not null
    public BaseThingHandler(Thing thing) {
        updateStatus(ThingStatus.REMOVED);
        if (!isModifyingCurrentConfig(configurationParameters)) {
        validateConfigurationParameters(configurationParameters);
        Configuration configuration = editConfiguration();
        for (Entry<String, Object> configurationParameter : configurationParameters.entrySet()) {
            configuration.put(configurationParameter.getKey(), configurationParameter.getValue());
        if (isInitialized()) {
            // persist new configuration and reinitialize handler
            dispose();
            updateConfiguration(configuration);
            // persist new configuration and notify Thing Manager
            if (this.callback != null) {
                this.callback.configurationUpdated(this.getThing());
                logger.warn("Handler {} tried updating its configuration although the handler was already disposed.",
                        this.getClass().getSimpleName());
     * Checks whether a given list of parameters would mean any change to the existing Thing configuration if applied to
     * Note that the passed parameters might be a subset of the existing configuration.
     * @param configurationParameters the parameters to check against the current configuration
     * @return true if the parameters would result in a modified configuration, false otherwise
    protected boolean isModifyingCurrentConfig(Map<String, Object> configurationParameters) {
        Configuration currentConfig = getConfig();
        for (Entry<String, Object> entry : configurationParameters.entrySet()) {
            if (!Objects.equals(currentConfig.get(entry.getKey()), entry.getValue())) {
        return this.thing;
            this.callback = thingHandlerCallback;
     * Get the {@link ThingHandlerCallback} instance.
     * @return the {@link ThingHandlerCallback} instance. Only returns {@code null} while the handler is not
     *         initialized.
    protected @Nullable ThingHandlerCallback getCallback() {
        return this.callback;
        // standard behavior is to refresh the linked channel,
        // so the newly linked items will receive a state update.
        handleCommand(channelUID, RefreshType.REFRESH);
     * Validates the given configuration parameters against the configuration description.
    protected void validateConfigurationParameters(Map<String, Object> configurationParameters) {
        ThingHandlerCallback callback = this.callback;
            callback.validateConfigurationParameters(this.getThing(), configurationParameters);
            logger.warn("Handler {} tried validating its configuration although the handler was already disposed.",
     * Get the {@link ConfigDescription} of the thing
     * @return the config description (or null if not found or handler disposed)
    protected @Nullable ConfigDescription getConfigDescription() {
            return callback.getConfigDescription(this.thing.getThingTypeUID());
            logger.warn("Handler {} tried retrieving its config description although the handler was already disposed.",
     * Get the {@link ConfigDescription} for a {@link ChannelTypeUID}
    protected @Nullable ConfigDescription getConfigDescription(ChannelTypeUID channelTypeUID) {
            return callback.getConfigDescription(channelTypeUID);
                    "Handler {} tried retrieving a channel config description although the handler was already disposed.",
     * Returns the configuration of the thing.
     * @return configuration of the thing
        return getThing().getConfiguration();
     * Returns the configuration of the thing and transforms it to the given
     * @return configuration of thing in form of the given class
    protected <T> T getConfigAs(Class<T> configurationClass) {
     * Updates the state of the thing.
     * @param channelUID unique id of the channel, which was updated
     * @param state new state
                this.callback.stateUpdated(channelUID, state);
                        "Handler {} of thing {} tried updating channel {} although the handler was already disposed.",
                        this.getClass().getSimpleName(), channelUID.getThingUID(), channelUID.getId());
     * Updates the state of the thing. Will use the thing UID to infer the
     * unique channel UID from the given ID.
     * @param channelID id of the channel, which was updated
    protected void updateState(String channelID, State state) {
        ChannelUID channelUID = new ChannelUID(this.getThing().getUID(), channelID);
        updateState(channelUID, state);
     * Send a time series to the channel. This can be used to transfer historic data or forecasts.
     * @param channelUID unique id of the channel
     * @param timeSeries the {@link TimeSeries} that is sent
    protected void sendTimeSeries(ChannelUID channelUID, TimeSeries timeSeries) {
            ThingHandlerCallback callback1 = this.callback;
            if (callback1 != null) {
                callback1.sendTimeSeries(channelUID, timeSeries);
                        "Handler {} of thing {} tried sending to channel {} although the handler was already disposed.",
     * @param channelID id of the channel
    protected void sendTimeSeries(String channelID, TimeSeries timeSeries) {
        sendTimeSeries(channelUID, timeSeries);
     * Emits an event for the given channel.
     * @param channelUID UID of the channel over which the event will be emitted
     * @param event Event to emit
    protected void triggerChannel(ChannelUID channelUID, String event) {
                this.callback.channelTriggered(this.getThing(), channelUID, event);
                        "Handler {} of thing {} tried triggering channel {} although the handler was already disposed.",
     * Emits an event for the given channel. Will use the thing UID to infer the
     * @param channelID ID of the channel over which the event will be emitted
    protected void triggerChannel(String channelID, String event) {
        triggerChannel(new ChannelUID(this.getThing().getUID(), channelID), event);
     * unique channel UID.
    protected void triggerChannel(String channelUID) {
        triggerChannel(new ChannelUID(this.getThing().getUID(), channelUID), "");
    protected void triggerChannel(ChannelUID channelUID) {
        triggerChannel(channelUID, "");
     * Sends a command for a channel of the thing.
     * @param channelID id of the channel, which sends the command
     * @param command command
    protected void postCommand(String channelID, Command command) {
        postCommand(channelUID, command);
     * @param channelUID unique id of the channel, which sends the command
    protected void postCommand(ChannelUID channelUID, Command command) {
                this.callback.postCommand(channelUID, command);
                        "Handler {} of thing {} tried posting a command to channel {} although the handler was already disposed.",
     * Updates the status of the thing.
    protected void updateStatus(ThingStatus status, ThingStatusDetail statusDetail, @Nullable String description) {
                ThingStatusInfoBuilder statusBuilder = ThingStatusInfoBuilder.create(status, statusDetail);
                ThingStatusInfo statusInfo = statusBuilder.withDescription(description).build();
                this.callback.statusUpdated(this.thing, statusInfo);
                logger.warn("Handler {} tried updating the thing status although the handler was already disposed.",
    protected void updateStatus(ThingStatus status, ThingStatusDetail statusDetail) {
        updateStatus(status, statusDetail, null);
     * Updates the status of the thing. The detail of the status will be 'NONE'.
    protected void updateStatus(ThingStatus status) {
        updateStatus(status, ThingStatusDetail.NONE, null);
     * Creates a thing builder, which allows to modify the thing. The method
     * @return {@link ThingBuilder} which builds an exact copy of the thing (not null)
    protected ThingBuilder editThing() {
        return ThingBuilder.create(this.thing.getThingTypeUID(), this.thing.getUID())
                .withBridge(this.thing.getBridgeUID()).withChannels(this.thing.getChannels())
                .withConfiguration(this.thing.getConfiguration()).withLabel(this.thing.getLabel())
                .withLocation(this.thing.getLocation()).withProperties(this.thing.getProperties())
                .withSemanticEquipmentTag(this.thing.getSemanticEquipmentTag());
     * Informs the framework, that a thing was updated. This method must be called after the configuration or channels
     * was changed.
     * Any method overriding this method has to make sure that only things with valid configurations are passed to the
     * callback. This can be achieved by calling
     * {@link ThingHandlerCallback#validateConfigurationParameters(Thing, Map)}. It is also necessary to ensure that all
     * channel configurations are valid by calling
     * {@link ThingHandlerCallback#validateConfigurationParameters(Channel, Map)}.
     * @param thing thing, that was updated and should be persisted
    protected void updateThing(Thing thing) {
        if (thing == this.thing) {
                    "Changes must not be done on the current thing - create a copy, e.g. via editThing()");
        if (callback == null) {
            logger.warn("Handler {} tried updating thing {} although the handler was already disposed.",
                    this.getClass().getSimpleName(), thing.getUID());
            callback.validateConfigurationParameters(thing, thing.getConfiguration().getProperties());
            thing.getChannels().forEach(channel -> callback.validateConfigurationParameters(channel,
                    channel.getConfiguration().getProperties()));
        } catch (ConfigValidationException e) {
                    "Attempt to update thing '{}' with a thing containing invalid configuration '{}' blocked. This is most likely a bug: {}",
                    thing.getUID(), thing.getConfiguration(), e.getValidationMessages());
            callback.thingUpdated(thing);
     * Returns a copy of the configuration, that can be modified. The method
     * {@link BaseThingHandler#updateConfiguration(Configuration)} must be called to persist the configuration.
     * @return copy of the thing configuration (not null)
    protected Configuration editConfiguration() {
        Map<String, Object> properties = this.thing.getConfiguration().getProperties();
        return new Configuration(new HashMap<>(properties));
     * Updates the configuration of the thing and informs the framework about it.
     * Any method overriding this method has to make sure that only valid configurations are passed to the callback.
     * This can be achieved by calling {@link ThingHandlerCallback#validateConfigurationParameters(Thing, Map)}.
     * @param configuration configuration, that was updated and should be persisted
    protected void updateConfiguration(Configuration configuration) {
        Map<String, Object> old = this.thing.getConfiguration().getProperties();
            callback.validateConfigurationParameters(this.thing, configuration.getProperties());
                    "Attempt to apply invalid configuration '{}' on thing '{}' blocked. This is most likely a bug: {}",
                    configuration, thing.getUID(), e.getValidationMessages());
            this.thing.getConfiguration().setProperties(configuration.getProperties());
                    "Error while applying configuration changes: '{}: {}' - reverting configuration changes on thing '{}'.",
                    e.getClass().getSimpleName(), e.getMessage(), this.thing.getUID().getAsString());
            this.thing.getConfiguration().setProperties(old);
     * Returns a copy of the properties map, that can be modified. The method {@link
     * BaseThingHandler#updateProperties(Map)} must be called to persist the properties.
     * @return copy of the thing properties (not null)
    protected Map<String, String> editProperties() {
        Map<String, String> properties = this.thing.getProperties();
        return new HashMap<>(properties);
     * Informs the framework, that the given properties map of the thing was updated. This method performs a check, if
     * the properties were updated. If the properties did not change, the framework is not informed about changes.
     * @param properties properties map, that was updated and should be persisted (all properties cleared if null)
    protected void updateProperties(@Nullable Map<String, String> properties) {
            updatePropertiesInternal(null);
            Map<String, @Nullable String> updatedPropertiesMap = new HashMap<>(properties);
            updatePropertiesInternal(updatedPropertiesMap);
    private void updatePropertiesInternal(@Nullable Map<String, @Nullable String> properties) {
        boolean propertiesUpdated = false;
        if (properties == null && !this.thing.getProperties().isEmpty()) {
            this.thing.setProperties(Map.of());
            propertiesUpdated = true;
        } else if (properties != null) {
            for (Entry<String, @Nullable String> property : properties.entrySet()) {
                String propertyName = property.getKey();
                String propertyValue = property.getValue();
                String existingPropertyValue = thing.getProperties().get(propertyName);
                if (existingPropertyValue == null || !existingPropertyValue.equals(propertyValue)) {
                    this.thing.setProperty(propertyName, propertyValue);
        if (propertiesUpdated) {
                    this.callback.thingUpdated(thing);
                            "Handler {} tried updating its thing's properties although the handler was already disposed.",
     * Updates the given property value for the thing that is handled by this thing handler instance. The value is only
     * set for the given property name if there has not been set any value yet or if the value has been changed. If the
     * value of the property to be set is null then the property is removed.
     * This method also informs the framework about the updated thing, which in fact will persists the changes. So, if
     * multiple properties should be changed at the same time, the {@link BaseThingHandler#editProperties()} method
     * should be used.
     * @param name the name of the property to be set
     * @param value the value of the property
    protected void updateProperty(String name, @Nullable String value) {
        updatePropertiesInternal(Collections.singletonMap(name, value));
     * Returns the bridge of the thing.
     * @return returns the bridge of the thing or null if the thing has no bridge
    protected @Nullable Bridge getBridge() {
                return bridgeUID != null ? callback.getBridge(bridgeUID) : null;
                        "Handler {} of thing {} tried accessing its bridge although the handler was already disposed.",
                        getClass().getSimpleName(), thing.getUID());
     * Returns whether at least one item is linked for the given channel ID.
     * @param channelId channel ID (must not be null)
     * @return true if at least one item is linked, false otherwise
    protected boolean isLinked(String channelId) {
        ChannelUID channelUID = new ChannelUID(this.getThing().getUID(), channelId);
        return isLinked(channelUID);
     * Returns whether at least one item is linked for the given UID of the channel.
     * @param channelUID UID of the channel (must not be null)
    protected boolean isLinked(ChannelUID channelUID) {
            return callback.isChannelLinked(channelUID);
                    "Handler {} of thing {} tried checking if channel {} is linked although the handler was already disposed.",
     * Returns whether the handler has already been initialized.
     * @return true if handler is initialized, false otherwise
    protected boolean isInitialized() {
        return ThingHandlerHelper.isHandlerInitialized(this);
        if (bridgeStatusInfo.getStatus() == ThingStatus.ONLINE
                && getThing().getStatusInfo().getStatusDetail() == ThingStatusDetail.BRIDGE_OFFLINE) {
            updateStatus(ThingStatus.ONLINE, ThingStatusDetail.NONE);
        } else if (bridgeStatusInfo.getStatus() == ThingStatus.OFFLINE) {
    protected void changeThingType(ThingTypeUID thingTypeUID, Configuration configuration) {
            this.callback.migrateThingType(getThing(), thingTypeUID, configuration);
            logger.warn("Handler {} tried migrating the thing type although the handler was already disposed.",
