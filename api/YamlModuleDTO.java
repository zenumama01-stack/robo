 * The {@link YamlModuleDTO} is a data transfer object used to serialize a module in a YAML configuration file.
public class YamlModuleDTO {
    @JsonAlias({ "configuration" })
    public YamlModuleDTO() {
    public YamlModuleDTO(@NonNull Module module) {
        this.type = ModuleTypeAliases.typeToAlias(module.getClass(), module.getTypeUID());
        this.config = new LinkedHashMap<>(module.getConfiguration().getProperties());
        if (this.config.containsKey("script") && this.config.get("type") instanceof String type) {
            String typeAlias = MIMETypeAliases.mimeTypeToAlias(type);
            if (!type.equals(typeAlias)) {
                this.config.put("type", typeAlias);
        return Objects.hash(config, description, id, label, type);
        if (!(obj instanceof YamlModuleDTO)) {
        YamlModuleDTO other = (YamlModuleDTO) obj;
        return Objects.equals(config, other.config) && Objects.equals(description, other.description)
                && Objects.equals(id, other.id) && Objects.equals(label, other.label)
                && Objects.equals(type, other.type);
