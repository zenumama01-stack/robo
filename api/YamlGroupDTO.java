import org.openhab.core.model.yaml.internal.util.YamlElementUtils;
 * The {@link YamlGroupDTO} is a data transfer object used to serialize the details of a group item
 * in a YAML configuration file.
public class YamlGroupDTO {
    public String dimension;
    public String function;
    public List<@NonNull String> parameters;
    public YamlGroupDTO() {
    public boolean isValid(@NonNull List<@NonNull String> errors, @NonNull List<@NonNull String> warnings) {
        if (!YamlElementUtils.isValidItemType(type)) {
            errors.add("invalid value \"%s\" for \"type\" field in group".formatted(type));
        } else if (YamlElementUtils.isNumberItemType(type)) {
            if (!YamlElementUtils.isValidItemDimension(dimension)) {
                errors.add("invalid value \"%s\" for \"dimension\" field in group".formatted(dimension));
        } else if (dimension != null) {
            warnings.add("\"dimension\" field in group ignored as type is not Number");
        if (!GroupFunction.VALID_FUNCTIONS.contains(getFunction())) {
            errors.add("invalid value \"%s\" for \"function\" field".formatted(function));
    public @Nullable String getBaseType() {
        return YamlElementUtils.getItemTypeWithDimension(type, dimension);
    public String getFunction() {
        return function != null ? function.toUpperCase() : GroupFunction.DEFAULT;
    public @NonNull List<@NonNull String> getParameters() {
        return parameters == null ? List.of() : parameters;
        return Objects.hash(getBaseType(), getFunction(), getParameters());
        } else if (obj == null || getClass() != obj.getClass()) {
        YamlGroupDTO other = (YamlGroupDTO) obj;
        return Objects.equals(getBaseType(), other.getBaseType()) && Objects.equals(getFunction(), other.getFunction())
                && Objects.equals(getParameters(), other.getParameters());
