 * {@link ThingFactory} helps to create thing based on a given {@link ThingType} .
 * @author Dennis Nobel - Initial contribution, added support for channel groups
 * @author Thomas Höfer - added thing and thing type properties
public class ThingFactory {
    private static final Logger LOGGER = LoggerFactory.getLogger(ThingFactory.class);
     * Generates a random Thing UID for the given thingType
     * @param thingTypeUID thing type (must not be null)
     * @return random Thing UID
    public static ThingUID generateRandomThingUID(ThingTypeUID thingTypeUID) {
        String thingId = uuid.substring(uuid.length() - 12);
        return new ThingUID(thingTypeUID, thingId);
     * Creates a thing based on a given thing type.
     * @param thingType thing type (must not be null)
     * @param thingUID thindUID (must not be null)
     * @return thing the thing
    public static Thing createThing(ThingType thingType, ThingUID thingUID, Configuration configuration,
        return createThing(thingType, thingUID, configuration, bridgeUID, null);
     * Creates a thing based on a given thing type. It also creates the
     * default-configuration given in the configDescriptions if the
     * configDescriptionRegistry is not null
     * @param thingType (must not be null)
     * @param thingUID (must not be null)
     * @param configDescriptionRegistry (can be null)
            @Nullable ThingUID bridgeUID, @Nullable ConfigDescriptionRegistry configDescriptionRegistry) {
        List<Channel> channels = ThingFactoryHelper.createChannels(thingType, thingUID, configDescriptionRegistry);
        return createThingBuilder(thingType, thingUID).withConfiguration(configuration).withChannels(channels)
                .withProperties(thingType.getProperties()).withBridge(bridgeUID)
    public static @Nullable Thing createThing(ThingUID thingUID, Configuration configuration,
            @Nullable Map<String, String> properties, @Nullable ThingUID bridgeUID, ThingTypeUID thingTypeUID,
            List<ThingHandlerFactory> thingHandlerFactories) {
        for (ThingHandlerFactory thingHandlerFactory : thingHandlerFactories) {
            if (thingHandlerFactory.supportsThingType(thingTypeUID)) {
                Thing thing = thingHandlerFactory.createThing(thingTypeUID, configuration, thingUID, bridgeUID);
                            "Thing factory ({}) returned null on create thing when it reports to support the thing type ({}).",
                            thingHandlerFactory.getClass(), thingTypeUID);
                        for (Entry<String, String> entry : properties.entrySet()) {
     * Creates a thing based on given thing type.
     * @param thingUID thingUID (must not be null)
    public static Thing createThing(ThingType thingType, ThingUID thingUID, Configuration configuration) {
        return createThing(thingType, thingUID, configuration, null);
    private static ThingBuilder createThingBuilder(ThingType thingType, ThingUID thingUID) {
        if (thingType instanceof BridgeType) {
            return BridgeBuilder.create(thingType.getUID(), thingUID);
        return ThingBuilder.create(thingType.getUID(), thingUID);
