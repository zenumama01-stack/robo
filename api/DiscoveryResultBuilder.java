import org.openhab.core.common.AbstractUID;
import org.openhab.core.config.discovery.internal.DiscoveryResultImpl;
 * The {@link DiscoveryResultBuilder} helps creating a {@link DiscoveryResult} through the builder pattern.
 * @see DiscoveryResult
public class DiscoveryResultBuilder {
    private Logger logger = LoggerFactory.getLogger(DiscoveryResultBuilder.class);
    private final ThingUID thingUID;
    private @Nullable ThingUID bridgeUID;
    private final Map<String, Object> properties = new HashMap<>();
    private @Nullable String representationProperty;
    private long ttl = DiscoveryResult.TTL_UNLIMITED;
    private @Nullable ThingTypeUID thingTypeUID;
    private DiscoveryResultBuilder(ThingUID thingUID) {
     * Creates a new builder for a given thing UID.
     * @param thingUID the thing UID for which the builder should be created-
     * @return a new instance of a {@link DiscoveryResultBuilder}
    public static DiscoveryResultBuilder create(ThingUID thingUID) {
        return new DiscoveryResultBuilder(thingUID);
     * Creates a new builder initialized with the values from the specified {@link DiscoveryResult}.
     * @param discoveryResult the {@link DiscoveryResult} to use for initialization.
     * @return The new {@link DiscoveryResultBuilder}.
    public static DiscoveryResultBuilder create(DiscoveryResult discoveryResult) {
        return new DiscoveryResultBuilder(discoveryResult.getThingUID()).withBridge(discoveryResult.getBridgeUID())
                .withProperties(discoveryResult.getProperties())
                .withRepresentationProperty(discoveryResult.getRepresentationProperty())
                .withLabel(discoveryResult.getLabel()).withTTL(discoveryResult.getTimeToLive())
                .withThingType(discoveryResult.getThingTypeUID());
     * Explicitly sets the thing type.
     * @param thingTypeUID the {@link ThingTypeUID}
    public DiscoveryResultBuilder withThingType(@Nullable ThingTypeUID thingTypeUID) {
        this.thingTypeUID = thingTypeUID;
     * Adds properties to the desired result.
     * @param properties of the desired result
    public DiscoveryResultBuilder withProperties(@Nullable Map<String, Object> properties) {
        if (properties != null) {
     * Adds a property to the desired result.
     * @param key property of the desired result
    public DiscoveryResultBuilder withProperty(String key, Object value) {
        properties.put(key, value);
     * Sets the representation Property of the desired result.
     * @param representationProperty the representation property of the desired result
    public DiscoveryResultBuilder withRepresentationProperty(@Nullable String representationProperty) {
        this.representationProperty = representationProperty;
     * Sets the bridgeUID of the desired result.
     * @param bridgeUID of the desired result
    public DiscoveryResultBuilder withBridge(@Nullable ThingUID bridgeUID) {
        this.bridgeUID = bridgeUID;
     * Sets the label of the desired result.
     * @param label of the desired result
    public DiscoveryResultBuilder withLabel(@Nullable String label) {
     * Sets the time to live for the result in seconds.
     * @param ttl time to live in seconds
    public DiscoveryResultBuilder withTTL(long ttl) {
        this.ttl = ttl;
    public DiscoveryResult build() {
        if (representationProperty != null && !properties.containsKey(representationProperty)) {
                    "Representation property '{}' of discovery result for thing '{}' is missing in properties map. It has to be fixed by the bindings developer.\n{}",
                    representationProperty, thingUID, getStackTrace(Thread.currentThread()));
        if (thingTypeUID == null) {
            String[] segments = thingUID.getAsString().split(AbstractUID.SEPARATOR);
            if (!segments[1].isEmpty()) {
                thingTypeUID = new ThingTypeUID(thingUID.getBindingId(), segments[1]);
        return new DiscoveryResultImpl(thingTypeUID, thingUID, bridgeUID, properties, representationProperty, label,
                ttl);
    private String getStackTrace(final Thread thread) {
        StackTraceElement[] elements = thread.getStackTrace();
        return Arrays.stream(elements).map(element -> "\tat " + element.toString()).collect(Collectors.joining("\n"));
