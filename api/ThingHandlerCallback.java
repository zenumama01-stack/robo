import org.openhab.core.thing.ChannelGroupUID;
 * {@link ThingHandlerCallback} is callback interface for {@link ThingHandler}s. The implementation of a
 * {@link ThingHandler} must use the callback to inform the framework about changes like state updates, status updated
 * or an update of the whole thing.
 * @author Stefan Bußweiler - Added new thing status info, added new configuration update info
 * @author Christoph Weitkamp - Added preconfigured ChannelGroupBuilder
public interface ThingHandlerCallback {
     * Informs about an updated state for a channel.
     * @param channelUID channel UID (must not be null)
     * @param state state (must not be null)
    void stateUpdated(ChannelUID channelUID, State state);
     * Informs about a command, which is sent from the channel.
    void postCommand(ChannelUID channelUID, Command command);
     * Informs about a time series, whcihs is send from the channel.
     * @param timeSeries time series
    void sendTimeSeries(ChannelUID channelUID, TimeSeries timeSeries);
     * Informs about an updated status of a thing.
     * @param thing thing (must not be null)
     * @param thingStatus thing status (must not be null)
    void statusUpdated(Thing thing, ThingStatusInfo thingStatus);
     * Informs about an update of the whole thing.
     * @param thing thing that was updated (must not be null)
     * @throws IllegalStateException if the {@link Thing} is can't be found
     * @param thing thing with the updated configuration (must not be null)
    void validateConfigurationParameters(Thing thing, Map<String, Object> configurationParameters);
     * @param channel channel with the updated configuration (must not be null)
    void validateConfigurationParameters(Channel channel, Map<String, Object> configurationParameters);
     * @param channelTypeUID the channel type UID
     * @return the corresponding configuration description (or null if not found)
    ConfigDescription getConfigDescription(ChannelTypeUID channelTypeUID);
     * Get the {@link ConfigDescription} for a {@link ThingTypeUID}
     * @param thingTypeUID the thing type UID
    ConfigDescription getConfigDescription(ThingTypeUID thingTypeUID);
     * Informs about an updated configuration of a thing.
     * @param thing thing with the updated configuration (must no be null)
    void configurationUpdated(Thing thing);
     * Informs the framework that the ThingType of the given {@link Thing} should be changed.
     * @param thing thing that should be migrated to another ThingType (must not be null)
     * @param thingTypeUID the new type of the thing (must not be null)
     * @param configuration a configuration that should be applied to the given {@link Thing}
    void migrateThingType(Thing thing, ThingTypeUID thingTypeUID, Configuration configuration);
     * Informs the framework that a channel has been triggered.
     * @param channelUID UID of the channel over which has been triggered.
     * @param event Event.
    void channelTriggered(Thing thing, ChannelUID channelUID, String event);
     * Creates a {@link ChannelBuilder} which is preconfigured with values from the given {@link ChannelType}.
     * @param channelUID the UID of the {@link Channel} to be created
     * @param channelTypeUID the {@link ChannelTypeUID} for which the {@link Channel} should be created
     * @return a preconfigured {@link ChannelBuilder}
     * @throws IllegalArgumentException if the referenced {@link ChannelType} is not known
    ChannelBuilder createChannelBuilder(ChannelUID channelUID, ChannelTypeUID channelTypeUID);
     * Creates a {@link ChannelBuilder} which is preconfigured with values from the given {@link Channel} and allows to
     * modify it. The methods {@link BaseThingHandler#editThing()} and {@link BaseThingHandler#updateThing(Thing)}
     * must be called to persist the changes.
     * @param thing {@link Thing} (must not be null)
     * @param channelUID the UID of the {@link Channel} to be edited
     * @throws IllegalArgumentException if no {@link Channel} with the given UID exists for the given {@link Thing}
    ChannelBuilder editChannel(Thing thing, ChannelUID channelUID);
     * Creates a list of {@link ChannelBuilder}s which are preconfigured with values from the given
     * {@link ChannelGroupType}.
     * @param channelGroupUID the UID of the channel group to be created
     * @param channelGroupTypeUID the {@link ChannelGroupUID} for which the {@link Channel}s should be created
     * @return a list of preconfigured {@link ChannelBuilder}s
     * @throws IllegalArgumentException if the referenced {@link ChannelGroupType} is not known
    List<ChannelBuilder> createChannelBuilders(ChannelGroupUID channelGroupUID,
            ChannelGroupTypeUID channelGroupTypeUID);
    boolean isChannelLinked(ChannelUID channelUID);
     * @param bridgeUID {@link ThingUID} UID of the bridge (must not be null)
    Bridge getBridge(ThingUID bridgeUID);
