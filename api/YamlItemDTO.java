 * The {@link YamlItemDTO} is a data transfer object used to serialize an item in a YAML configuration file.
@YamlElementName("items")
public class YamlItemDTO implements YamlElement, Cloneable {
    private static final Pattern ICON_SEGMENT_PATTERN = Pattern.compile("[a-zA-Z0-9_][a-zA-Z0-9_-]*");
    public YamlGroupDTO group;
    public String expire;
    public Boolean autoupdate;
    public List<@NonNull String> groups;
    public String channel;
    public Map<@NonNull String, @NonNull Map<@NonNull String, @NonNull Object>> channels;
    public Map<@NonNull String, @NonNull YamlMetadataDTO> metadata;
    public YamlItemDTO() {
    public @NonNull String getId() {
        return name == null ? "" : name;
    public void setId(@NonNull String id) {
        name = id;
    public YamlElement cloneWithoutId() {
        YamlItemDTO copy;
            copy = (YamlItemDTO) super.clone();
            copy.name = null;
        } catch (CloneNotSupportedException e) {
            // Will never happen
            return new YamlItemDTO();
    public boolean isValid(@Nullable List<@NonNull String> errors, @Nullable List<@NonNull String> warnings) {
        // Check that name is present
        if (name == null || name.isBlank()) {
            addToList(errors, "invalid item: name missing while mandatory");
        if (!ItemUtil.isValidItemName(name)) {
            addToList(errors,
                    "invalid item \"%s\": \"name\" must begin with a letter or underscore followed by alphanumeric characters and underscores, and must not contain any other symbols."
                            .formatted(name));
        List<String> subErrors = new ArrayList<>();
        List<String> subWarnings = new ArrayList<>();
        if (type == null || type.isBlank()) {
            addToList(errors, "invalid item \"%s\": \"type\" field missing while mandatory".formatted(name));
        } else if (GroupItem.TYPE.equalsIgnoreCase(type)) {
            if (dimension != null) {
                addToList(warnings, "item \"%s\": \"dimension\" field ignored as type is Group".formatted(name));
                ok &= group.isValid(subErrors, subWarnings);
                subErrors.forEach(error -> {
                    addToList(errors, "invalid item \"%s\": %s".formatted(name, error));
                subWarnings.forEach(warning -> {
                    addToList(warnings, "item \"%s\": %s".formatted(name, warning));
                addToList(warnings, "item \"%s\": \"group\" field ignored as type is not Group".formatted(name));
                addToList(errors, "invalid item \"%s\": invalid value \"%s\" for \"type\" field".formatted(name, type));
                    addToList(errors, "invalid item \"%s\": invalid value \"%s\" for \"dimension\" field"
                            .formatted(name, dimension));
                addToList(warnings,
                        "item \"%s\": \"dimension\" field ignored as type is not Number".formatted(name, dimension));
        if (icon != null) {
            subErrors.clear();
            ok &= isValidIcon(icon, subErrors);
        if (groups != null) {
            for (String gr : groups) {
                if (!ItemUtil.isValidItemName(gr)) {
                            "invalid item \"%s\": value \"%s\" in \"groups\" field must begin with a letter or underscore followed by alphanumeric characters and underscores, and must not contain any other symbols."
                                    .formatted(name, gr));
        if (channel != null) {
            ok &= isValidChannel(channel, null, subErrors);
        if (channels != null) {
            for (String ch : channels.keySet()) {
                ok &= isValidChannel(ch, channels.get(ch), subErrors);
            for (String namespace : metadata.keySet()) {
                    new MetadataKey(namespace, name);
                    addToList(errors, "invalid item \"%s\": invalid metadata key (\"%s\", \"%s\"): %s".formatted(name,
                            namespace, name, e.getMessage()));
            YamlMetadataDTO md = metadata.get("autoupdate");
            if (md != null && autoupdate != null) {
                        "item \"%s\": \"autoupdate\" field is redundant with \"autoupdate\" metadata; value \"%s\" will be considered"
                                .formatted(name, md.getValue()));
            md = metadata.get("unit");
            if (md != null && unit != null) {
                        "item \"%s\": \"unit\" field is redundant with \"unit\" metadata; value \"%s\" will be considered"
            md = metadata.get("stateDescription");
            Map<@NonNull String, @NonNull Object> mdConfig = md == null ? null : md.config;
            Object pattern = mdConfig == null ? null : mdConfig.get("pattern");
            if (pattern != null && format != null) {
                        "item \"%s\": \"format\" field is redundant with pattern in \"stateDescription\" metadata; \"%s\" will be considered"
                                .formatted(name, pattern));
            md = metadata.get("expire");
            if (md != null && expire != null) {
                        "item \"%s\": \"expire\" field is redundant with \"expire\" metadata; value \"%s\" will be considered"
    private boolean isValidIcon(String icon, List<@NonNull String> errors) {
        String[] segments = icon.split(AbstractUID.SEPARATOR);
        int nb = segments.length;
        if (nb > 3) {
            errors.add("too many segments in value \"%s\" for \"icon\" field; maximum 3 is expected".formatted(icon));
            nb = 3;
        for (int i = 0; i < nb; i++) {
            String segment = segments[i];
            if (!ICON_SEGMENT_PATTERN.matcher(segment).matches()) {
                errors.add("segment \"%s\" in \"icon\" field not matching the expected syntax %s".formatted(segment,
                        ICON_SEGMENT_PATTERN.pattern()));
    private boolean isValidChannel(String channelUID, @Nullable Map<@NonNull String, @NonNull Object> configuration,
            List<@NonNull String> errors) {
            new ChannelUID(channelUID);
            errors.add("invalid channel UID \"%s\": %s".formatted(channelUID, e.getMessage()));
        if (configuration != null && configuration.containsKey("profile")
                && configuration.get("profile") instanceof String profile) {
            String[] splittedProfile = profile.split(AbstractUID.SEPARATOR, 2);
                if (splittedProfile.length == 1) {
                    new ProfileTypeUID(ProfileTypeUID.SYSTEM_SCOPE, profile);
                    new ProfileTypeUID(splittedProfile[0], splittedProfile[1]);
                errors.add("invalid value \"%s\" for \"profile\" parameter of channel \"%s\": %s".formatted(profile,
                        channelUID, e.getMessage()));
    private void addToList(@Nullable List<@NonNull String> list, String value) {
        if (list != null) {
            list.add(value);
    public @Nullable String getType() {
    public @NonNull List<@NonNull String> getGroups() {
        return groups == null ? List.of() : groups;
    public @NonNull Set<@NonNull String> getTags() {
        return tags == null ? Set.of() : tags;
        return Objects.hash(name, getType(), group, label, icon, format, unit, expire, autoupdate, getGroups(),
                getTags(), channel, channels, metadata);
        YamlItemDTO other = (YamlItemDTO) obj;
        return Objects.equals(name, other.name) && Objects.equals(getType(), other.getType())
                && Objects.equals(group, other.group) && Objects.equals(label, other.label)
                && Objects.equals(icon, other.icon) && Objects.equals(format, other.format)
                && Objects.equals(unit, other.unit) && Objects.equals(expire, other.expire)
                && Objects.equals(autoupdate, other.autoupdate) && Objects.equals(getGroups(), other.getGroups())
                && Objects.equals(getTags(), other.getTags()) && Objects.equals(channel, other.channel)
                && Objects.equals(channels, other.channels) && Objects.equals(metadata, other.metadata);
