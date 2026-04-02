 * DTO for serialization of a add-on discovery parameter.
 * @author Mark Herwege - Initial contribution
public class AddonParameter {
    private @NonNullByDefault({}) String value;
    public AddonParameter(String name, String value) {
    public String getValue() {
        return Objects.hash(name, value);
        AddonParameter other = (AddonParameter) obj;
        return Objects.equals(name, other.name) && Objects.equals(value, other.value);
