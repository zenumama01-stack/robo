package org.openhab.core.model.yaml.internal.things;
 * The {@link YamlChannelDTO} is a data transfer object used to serialize a channel in a YAML configuration file.
public class YamlChannelDTO {
    public String kind;
    public String itemType;
    public String itemDimension;
    @JsonAlias("configuration")
    public YamlChannelDTO() {
                new Configuration(config);
                errors.add("invalid data in \"config\" field: %s".formatted(e.getMessage()));
                new ChannelTypeUID("dummy", type);
                errors.add("invalid value \"%s\" for \"type\" field: %s".formatted(type, e.getMessage()));
            if (kind != null) {
                warnings.add("\"kind\" field ignored; channel kind will be retrieved from the channel type");
            if (itemType != null) {
                warnings.add("\"itemType\" field ignored; item type will be retrieved from the channel type");
            if (itemDimension != null) {
                        "\"itemDimension\" field ignored; item type and dimension will be retrieved from the channel type");
        } else if (itemType != null) {
            if (!YamlElementUtils.isValidItemType(itemType)) {
                errors.add("invalid value \"%s\" for \"itemType\" field".formatted(itemType));
            } else if (YamlElementUtils.isNumberItemType(itemType)) {
                if (!YamlElementUtils.isValidItemDimension(itemDimension)) {
                    errors.add("invalid value \"%s\" for \"itemDimension\" field".formatted(itemDimension));
            } else if (itemDimension != null) {
                warnings.add("\"itemDimension\" field ignored as item type is not Number");
                ChannelKind.parse(kind);
                        "invalid value \"%s\" for \"kind\" field; only \"state\" and \"trigger\" whatever the case are valid; \"state\" will be considered"
                                .formatted(kind != null ? kind : "null"));
            errors.add("one of the \"type\" and \"itemType\" fields is mandatory");
    public ChannelKind getKind() {
            return ChannelKind.parse(kind);
            return ChannelKind.STATE;
    public @Nullable String getItemType() {
        return YamlElementUtils.getItemTypeWithDimension(itemType, itemDimension);
        return Objects.hash(type, getKind(), getItemType(), label, description, config);
        YamlChannelDTO other = (YamlChannelDTO) obj;
        return Objects.equals(type, other.type) && getKind() == other.getKind()
                && Objects.equals(getItemType(), other.getItemType()) && Objects.equals(label, other.label)
                && Objects.equals(description, other.description) && Objects.equals(config, other.config);
