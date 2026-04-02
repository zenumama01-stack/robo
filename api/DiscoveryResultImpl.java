public class DiscoveryResultImpl implements DiscoveryResult {
    private @NonNullByDefault({}) ThingUID thingUID;
    private Map<String, Object> properties = Map.of();
    private @NonNullByDefault({}) DiscoveryResultFlag flag;
    private @NonNullByDefault({}) String label;
    private Instant timestamp = Instant.MIN;
    private long timeToLive = TTL_UNLIMITED;
    DiscoveryResultImpl() {
     * @param thingUID the {@link ThingUID} to be set. If a {@code Thing} disappears and is discovered again, the same
     *            {@code Thing} ID must be created. A typical {@code Thing} ID could be the serial number. It's usually
     *            <i>not</i> a product name.
     * @param bridgeUID the unique {@link org.openhab.core.thing.Bridge} ID to be set
     * @param properties the properties to be set
     * @param representationProperty the representationProperty to be set
     * @param label the human readable label to set
     * @param timeToLive time to live in seconds
     * @throws IllegalArgumentException if the {@link ThingUID} is null or the time to live is less than 1
     * @deprecated use {@link org.openhab.core.config.discovery.DiscoveryResultBuilder} instead.
    public DiscoveryResultImpl(@Nullable ThingTypeUID thingTypeUID, ThingUID thingUID, @Nullable ThingUID bridgeUID,
            @Nullable Map<String, Object> properties, @Nullable String representationProperty, @Nullable String label,
            long timeToLive) throws IllegalArgumentException {
        if (timeToLive < 1 && timeToLive != TTL_UNLIMITED) {
            throw new IllegalArgumentException("The ttl must not be 0 or negative!");
        this.properties = properties == null ? Map.of() : Collections.unmodifiableMap(properties);
        this.label = label == null ? "" : label;
        this.timestamp = Instant.now();
        this.timeToLive = timeToLive;
        this.flag = DiscoveryResultFlag.NEW;
    public ThingUID getThingUID() {
    public ThingTypeUID getThingTypeUID() {
        ThingTypeUID localThingTypeUID = thingTypeUID;
        if (localThingTypeUID != null) {
            return localThingTypeUID;
            // fallback for discovery result which were created before the thingTypeUID field was added
                return new ThingTypeUID(thingUID.getBindingId(), segments[1]);
            throw new IllegalArgumentException("ThingTypeUID of thing '" + thingUID.getAsString() + "' is null.");
    public String getBindingId() {
        return thingUID.getBindingId();
    public @Nullable String getRepresentationProperty() {
        return representationProperty;
    public DiscoveryResultFlag getFlag() {
        return flag;
    public @Nullable ThingUID getBridgeUID() {
        return bridgeUID;
     * Merges the content of the specified source {@link DiscoveryResult} into this object.
     * <i>Hint:</i> The {@link DiscoveryResultFlag} of this object keeps its state.
     * This method returns silently if the specified source {@link DiscoveryResult} is {@code null} or its {@code Thing}
     * type or ID does not fit to this object.
     * @param sourceResult the discovery result which is used as source for the merge
    public void synchronize(@Nullable DiscoveryResult sourceResult) {
        if (sourceResult != null && thingUID.equals(sourceResult.getThingUID())) {
            this.properties = sourceResult.getProperties();
            this.representationProperty = sourceResult.getRepresentationProperty();
            this.label = sourceResult.getLabel();
            this.timeToLive = sourceResult.getTimeToLive();
    public void normalizePropertiesOnConfigDescription(List<String> configurationParameters) {
        properties = properties.entrySet().stream().map(e -> {
            if (!configurationParameters.contains(e.getKey())) {
                return Map.entry(e.getKey(), String.valueOf(e.getValue()));
        }).collect(Collectors.toMap(Map.Entry::getKey, Map.Entry::getValue));
     * Sets the flag of this result object.<br>
     * @param flag the flag of this result object to be set
    public void setFlag(@Nullable DiscoveryResultFlag flag) {
        this.flag = flag == null ? DiscoveryResultFlag.NEW : flag;
        return 31 + thingUID.hashCode();
        DiscoveryResultImpl other = (DiscoveryResultImpl) obj;
        return "DiscoveryResult [thingUID=" + thingUID + ", properties=" + properties + ", representationProperty="
                + representationProperty + ", flag=" + flag + ", label=" + label + ", bridgeUID=" + bridgeUID + ", ttl="
                + timeToLive + ", timestamp=" + timestamp + "]";
    public Instant getCreationTime() {
        return timestamp;
    public long getTimeToLive() {
        return timeToLive;
