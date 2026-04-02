 * The {@link ThingHandlerFactory} is responsible for creating {@link Thing}s and {@link ThingHandler}s. Therefore the
 * factory must be registered as OSGi service.
public interface ThingHandlerFactory {
     * Returns whether the handler is able to create a thing or register a thing handler for the given type.
     * @return true, if the handler supports the thing type, false otherwise
    boolean supportsThingType(ThingTypeUID thingTypeUID);
     * Creates a new {@link ThingHandler} instance. In addition, the handler can be registered as a service if it is
     * required, e.g. as {@link FirmwareUpdateHandler}, {@link ConfigStatusProvider}.
     * This method is only called if the {@link ThingHandlerFactory} supports the type of the given thing.
     * @param thing the thing for which a new handler must be registered
     * @return the created thing handler instance, not null
     * @throws IllegalStateException if the handler instance could not be created
    ThingHandler registerHandler(Thing thing);
     * Unregisters a {@link ThingHandler} instance.
     * @param thing the thing for which the handler must be unregistered
    void unregisterHandler(Thing thing);
     * Creates a thing for given arguments.
     * @param thingTypeUID thing type uid (not null)
     * @param configuration configuration
     * @param thingUID thing uid, which can be null
     * @param bridgeUID bridge uid, which can be null
     * @return created thing
    Thing createThing(ThingTypeUID thingTypeUID, Configuration configuration, @Nullable ThingUID thingUID,
            @Nullable ThingUID bridgeUID);
     * A thing with the given {@link Thing} UID was removed.
     * @param thingUID thing UID of the removed object
    void removeThing(ThingUID thingUID);
