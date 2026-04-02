 * The {@link YamlConditionDTO} is a data transfer object used to serialize a condition in a YAML configuration file.
public class YamlConditionDTO extends YamlModuleDTO {
    public YamlConditionDTO() {
    public YamlConditionDTO(@NonNull Condition condition) {
        if (!(obj instanceof YamlConditionDTO)) {
        YamlConditionDTO other = (YamlConditionDTO) obj;
