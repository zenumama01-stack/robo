 * This is a data transfer object that is used to serialize channels.
 * @author Chris Jackson - Added properties and configuration
 * @author Kai Kreuzer - Added default tags
@Schema(name = "Channel")
public class ChannelDTO {
    public String channelTypeUID;
    public Set<String> defaultTags;
    public String autoUpdatePolicy;
    public ChannelDTO() {
    public ChannelDTO(ChannelUID uid, String channelTypeUID, String itemType, ChannelKind kind, String label,
            String description, Map<String, String> properties, Configuration configuration, Set<String> defaultTags,
            AutoUpdatePolicy autoUpdatePolicy) {
        this.uid = uid.toString();
        this.id = uid.getId();
        this.itemType = itemType;
        this.configuration = toMap(configuration);
        this.defaultTags = new HashSet<>(defaultTags);
        this.kind = kind.toString();
        if (autoUpdatePolicy != null) {
            this.autoUpdatePolicy = autoUpdatePolicy.toString();
    private Map<String, Object> toMap(Configuration configuration) {
        Map<String, Object> configurationMap = new HashMap<>(configuration.keySet().size());
        for (String key : configuration.keySet()) {
            configurationMap.put(key, configuration.get(key));
        return configurationMap;
