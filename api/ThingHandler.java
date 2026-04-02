 * A {@link ThingHandler} handles the communication between the openHAB framework and an entity from the real
 * world, e.g. a physical device, a web service, etc. represented by a {@link Thing}.
 * The communication is bidirectional. The framework informs a thing handler about commands, state and configuration
 * updates, and so on, by the corresponding handler methods. The handler can notify the framework about changes like
 * state and status updates, updates of the whole thing, by a {@link ThingHandlerCallback}.
 * @author Thomas Höfer - Added config description validation exception to handleConfigurationUpdate operation
 * @author Stefan Bußweiler - API changes due to bridge/thing life cycle refactoring
 * @author Stefan Triller - added getServices method
public interface ThingHandler {
     * Returns the {@link Thing}, which belongs to the handler.
     * @return {@link Thing}, which belongs to the handler
    Thing getThing();
     * Initializes the thing handler, e.g. update thing status, allocate resources, transfer configuration.
     * This method is only called, if the {@link Thing} contains all required configuration parameters.
     * Only {@link Thing}s with status {@link ThingStatus#UNKNOWN}, {@link ThingStatus#ONLINE} or
     * {@link ThingStatus#OFFLINE} are considered as <i>initialized</i> by the framework. To achieve that, the status
     * must be reported via {@link ThingHandlerCallback#statusUpdated(Thing, ThingStatusInfo)}.
     * The framework expects this method to be non-blocking and return quickly. For longer running initializations,
     * the implementation has to take care of scheduling a separate job which must guarantee to set the thing status
     * eventually.
     * Any anticipated error situations should be handled gracefully and need to result in {@link ThingStatus#OFFLINE}
     * with the corresponding status detail (e.g. *COMMUNICATION_ERROR* or *CONFIGURATION_ERROR* including a meaningful
     * description) instead of throwing exceptions.
    void initialize();
     * Disposes the thing handler, e.g. deallocate resources.
     * The framework expects this method to be non-blocking and return quickly.
     * Sets the {@link ThingHandlerCallback} of the handler, which must be used to inform the framework about changes.
     * The callback is added after the handler instance has been tracked by the framework and before
     * {@link #initialize()} is called. The callback is removed (set to null) after the handler
     * instance is no longer tracked and after {@link #dispose()} is called.
     * @param thingHandlerCallback the callback (can be null)
    void setCallback(@Nullable ThingHandlerCallback thingHandlerCallback);
     * Handles a command for a given channel.
     * This method is only called, if the thing has been initialized (status ONLINE/OFFLINE/UNKNOWN).
     * @param channelUID the {@link ChannelUID} of the channel to which the command was sent
     * @param command the {@link Command}
    void handleCommand(ChannelUID channelUID, Command command);
     * Handles a configuration update.
     * Note: An implementing class needs to persist the configuration changes if necessary.
     * @param configurationParameters map of changed configuration parameters
    void handleConfigurationUpdate(Map<String, Object> configurationParameters);
     * Notifies the handler about an updated {@link Thing}.
     * This method will only be called once the {@link #initialize()} method returned.
     * @param thing the {@link Thing}, that has been updated
    void thingUpdated(Thing thing);
     * Notifies the handler that a channel was linked.
     * @param channelUID UID of the linked channel
    void channelLinked(ChannelUID channelUID);
     * Notifies the handler that a channel was unlinked.
     * @param channelUID UID of the unlinked channel
    void channelUnlinked(ChannelUID channelUID);
     * Notifies the handler that the bridge's status has changed.
     * This method is called, when the status of the bridge has been changed to {@link ThingStatus#ONLINE},
     * {@link ThingStatus#OFFLINE} or {@link ThingStatus#UNKNOWN}, i.e. after a bridge has been initialized.
     * If the thing of this handler does not have a bridge, this method is never called.
     * If the bridge's status has changed to {@link ThingStatus#OFFLINE}, the status of the handled thing must be
     * updated to {@link ThingStatus#OFFLINE} with detail {@link ThingStatusDetail#BRIDGE_OFFLINE}. If the bridge
     * returns to {@link ThingStatus#ONLINE}, the thing status must be changed at least to {@link ThingStatus#OFFLINE}
     * with detail {@link ThingStatusDetail#NONE}.
     * @param bridgeStatusInfo the status info of the bridge
    void bridgeStatusChanged(ThingStatusInfo bridgeStatusInfo);
     * This method is called before a thing is removed. An implementing class can handle the removal in order to
     * trigger some tidying work for a thing.
     * For longer running tasks, the implementation has to take care of scheduling a separate job.
     * The {@link Thing} is in {@link ThingStatus#REMOVING} when this method is called.
     * Implementations of this method must signal to the framework that the handling has been
     * completed by setting the {@link Thing}s state to {@link ThingStatus#REMOVED}.
     * Only then it will be removed completely.
    void handleRemoval();
     * This method provides a list of classes which should be registered as services by the framework
     * @return - list of classes that will be registered as OSGi services
    default Collection<Class<? extends ThingHandlerService>> getServices() {
