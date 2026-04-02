 * DTO for serialization of a property match regular expression.
public class AddonMatchProperty {
    private @NonNullByDefault({}) String name;
    private @NonNullByDefault({}) String regex;
    private transient @NonNullByDefault({}) Pattern pattern;
    public AddonMatchProperty(String name, String regex) {
        this.regex = regex;
        this.pattern = null;
    public Pattern getPattern() {
        Pattern pattern = this.pattern;
        if (pattern == null) {
            this.pattern = Pattern.compile(regex);
        return this.pattern;
    public String getRegex() {
        return regex;
        return Objects.hash(name, regex);
        AddonMatchProperty other = (AddonMatchProperty) obj;
        return Objects.equals(name, other.name) && Objects.equals(regex, other.regex);
