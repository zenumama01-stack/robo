 * The {@link YamlActionDTO} is a data transfer object used to serialize an action in a YAML configuration file.
public class YamlActionDTO extends YamlModuleDTO {
    public Map<@NonNull String, @NonNull String> inputs;
    public YamlActionDTO() {
    public YamlActionDTO(@NonNull Action action) {
        int result = super.hashCode();
        result = prime * result + Objects.hash(inputs);
        if (!super.equals(obj)) {
        if (!(obj instanceof YamlActionDTO)) {
        YamlActionDTO other = (YamlActionDTO) obj;
        return Objects.equals(inputs, other.inputs);
            builder.append("inputs=").append(inputs).append(", ");
            builder.append("id=").append(id).append(", ");
            builder.append("config=").append(config);
