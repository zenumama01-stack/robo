 * This is a data transfer object that is used to serialize filter criteria of a
 * parameter.
@Schema(name = "FilterCriteria")
public class FilterCriteriaDTO {
    public String value;
    public FilterCriteriaDTO() {
    public FilterCriteriaDTO(String name, String value) {
