 * The {@link ThingImpl} class is a concrete implementation of the {@link Thing}.
 * This class is mutable.
 * @author Michael Grammling - Configuration could never be null but may be empty
 * @author Benedikt Niehues - Fix ESH Bug 450236
 *         https://bugs.eclipse.org/bugs/show_bug.cgi?id=450236 - Considering
 *         ThingType Description
public class ThingImpl implements Thing {
    private final Map<ChannelUID, Channel> channels = new LinkedHashMap<>();
    private Map<String, String> properties = new HashMap<>();
    private @NonNullByDefault({}) ThingUID uid;
    private @NonNullByDefault({}) ThingTypeUID thingTypeUID;
    private transient volatile ThingStatusInfo status = ThingStatusInfoBuilder
            .create(ThingStatus.UNINITIALIZED, ThingStatusDetail.NONE).build();
    private transient volatile @Nullable ThingHandler thingHandler;
    ThingImpl() {
     * @param thingId thing ID
    public ThingImpl(ThingTypeUID thingTypeUID, String thingId) {
        this.uid = new ThingUID(thingTypeUID.getBindingId(), thingTypeUID.getId(), thingId);
    public ThingImpl(ThingTypeUID thingTypeUID, ThingUID thingUID) {
        this.uid = thingUID;
        return this.bridgeUID;
    public List<Channel> getChannels() {
        return List.copyOf(this.channels.values());
    public List<Channel> getChannelsOfGroup(String channelGroupId) {
        return this.channels.entrySet().stream().filter(c -> channelGroupId.equals(c.getKey().getGroupId()))
                .map(Map.Entry::getValue).toList();
    public @Nullable Channel getChannel(String channelId) {
        return getChannel(new ChannelUID(uid, channelId));
        return this.channels.get(channelUID);
    public @Nullable ThingHandler getHandler() {
        return this.thingHandler;
    public ThingUID getUID() {
        return status.getStatus();
    public void setBridgeUID(@Nullable ThingUID bridgeUID) {
    public void addChannel(Channel channel) {
        this.channels.put(channel.getUID(), channel);
    public void setChannels(List<Channel> channels) {
        channels.forEach(this::addChannel);
    public void setConfiguration(@Nullable Configuration configuration) {
        this.configuration = (configuration == null) ? new Configuration() : configuration;
    public void setHandler(@Nullable ThingHandler thingHandler) {
        this.thingHandler = thingHandler;
    public void setId(ThingUID id) {
        this.uid = id;
    public void setStatusInfo(ThingStatusInfo status) {
        return this.thingTypeUID;
    public void setThingTypeUID(ThingTypeUID thingTypeUID) {
    public @Nullable String setProperty(String name, @Nullable String value) {
            throw new IllegalArgumentException("Property name must not be null or empty");
            String val = value;
            return val == null ? properties.remove(name) : properties.put(name, val);
    public void setProperties(Map<String, String> properties) {
        this.properties = new HashMap<>(properties);
    public @Nullable String getLocation() {
    public void setLocation(@Nullable String location) {
    public boolean isEnabled() {
        return ThingStatusDetail.DISABLED != getStatusInfo().getStatusDetail();
        // Configuration is deliberately excluded because it might include sensitive data like passwords.
        StringBuilder sb = new StringBuilder(getUID().toString());
        sb.append(" (ThingTypeUID=");
        sb.append(getThingTypeUID());
        sb.append(", Bridge=False");
        if (getBridgeUID() != null) {
            sb.append(", BridgeUID=");
            sb.append(getBridgeUID());
        sb.append(", Label=");
        sb.append(getLabel());
        if (getLocation() != null) {
            sb.append(", Location=");
            sb.append(getLocation());
        sb.append(", Status=");
        sb.append(getStatus());
        sb.append(", StatusInfo=");
        sb.append(getStatusInfo());
        sb.append(", SemanticEquipmentTag=");
        sb.append(getSemanticEquipmentTag());
        ThingImpl other = (ThingImpl) obj;
        return uid.equals(other.uid);
    public @Nullable String getSemanticEquipmentTag() {
        return semanticEquipmentTag;
    public void setSemanticEquipmentTag(@Nullable String semanticEquipmentTag) {
    public void setSemanticEquipmentTag(@Nullable SemanticTag semanticEquipmentTag) {
        setSemanticEquipmentTag(semanticEquipmentTag.getName());
