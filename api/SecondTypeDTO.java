 * The {@link SecondTypeDTO} is a test type implementing {@link YamlElement}
@YamlElementName("secondType")
public class SecondTypeDTO implements YamlElement, Cloneable {
    public SecondTypeDTO() {
    public SecondTypeDTO(String id, String label) {
        return id == null ? "" : id;
        SecondTypeDTO copy;
            copy = (SecondTypeDTO) super.clone();
            copy.id = null;
            return new SecondTypeDTO();
        return id != null && !id.isBlank();
        SecondTypeDTO that = (SecondTypeDTO) o;
        return Objects.equals(id, that.id) && Objects.equals(label, that.label);
        return Objects.hash(id, label);
