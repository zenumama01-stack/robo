 * The {@link YamlThingDTO} is a data transfer object used to serialize a thing in a YAML configuration file.
@YamlElementName("things")
public class YamlThingDTO implements YamlElement, Cloneable {
    public Boolean isBridge;
    public String bridge;
    public String location;
    public Map<@NonNull String, @NonNull YamlChannelDTO> channels;
    public YamlThingDTO() {
        YamlThingDTO copy;
            copy = (YamlThingDTO) super.clone();
            return new YamlThingDTO();
            addToList(errors, "invalid thing: uid is missing while mandatory");
        ThingUID thingUID;
            thingUID = new ThingUID(uid);
            thingUID = new ThingUID("dummy:dummy:dummy");
            addToList(errors, "invalid thing \"%s\": %s".formatted(uid, e.getMessage()));
        if (bridge != null && !bridge.isBlank()) {
                new ThingUID(bridge);
                addToList(errors, "invalid thing \"%s\": invalid value \"%s\" for \"bridge\" field: %s".formatted(uid,
                        bridge, e.getMessage()));
                        "invalid thing \"%s\": invalid data in \"config\" field: %s".formatted(uid, e.getMessage()));
            for (Map.Entry<@NonNull String, @NonNull YamlChannelDTO> entry : channels.entrySet()) {
                String channelId = entry.getKey();
                String[] splittedChannelId = channelId.split(ChannelUID.CHANNEL_GROUP_SEPARATOR, 2);
                    if (splittedChannelId.length == 1) {
                        new ChannelUID(thingUID, channelId);
                        new ChannelUID(thingUID, splittedChannelId[0], splittedChannelId[1]);
                    addToList(errors, "invalid thing \"%s\": invalid channel id \"%s\": %s".formatted(uid, channelId,
                            e.getMessage()));
                List<String> channelErrors = new ArrayList<>();
                List<String> channelWarnings = new ArrayList<>();
                ok &= entry.getValue().isValid(channelErrors, channelWarnings);
                channelErrors.forEach(error -> {
                    addToList(errors, "invalid thing \"%s\": channel \"%s\": %s".formatted(uid, channelId, error));
                channelWarnings.forEach(warning -> {
                    addToList(warnings, "thing \"%s\": channel \"%s\": %s".formatted(uid, channelId, warning));
    public boolean isBridge() {
        return isBridge == null ? false : isBridge.booleanValue();
        return Objects.hash(uid, isBridge(), bridge, label, location, config, channels);
        YamlThingDTO other = (YamlThingDTO) obj;
        return Objects.equals(uid, other.uid) && isBridge() == other.isBridge() && Objects.equals(bridge, other.bridge)
                && Objects.equals(label, other.label) && Objects.equals(location, other.location)
                && Objects.equals(config, other.config) && Objects.equals(channels, other.channels);
