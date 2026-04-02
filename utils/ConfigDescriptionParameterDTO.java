 * This is a data transfer object that is used to serialize parameter of a configuration description.
 * @author Alex Tugarev - Extended for options and filter criteria
 * @author Chris Jackson - Added group, advanced, limitToOptions, multipleLimit, verify attributes
@Schema(name = "ConfigDescriptionParameter")
public class ConfigDescriptionParameterDTO {
    public String context;
    @Schema(name = "default")
    @SerializedName(value = "default", alternate = "defaultValue")
    public String defaultValue;
    public boolean required;
    public Type type;
    public BigDecimal min;
    public BigDecimal max;
    @Schema(name = "step")
    @SerializedName(value = "step", alternate = "stepsize")
    public BigDecimal stepsize;
    public String pattern;
    public Boolean readOnly;
    public Boolean multiple;
    public Integer multipleLimit;
    public String groupName;
    public Boolean advanced;
    public Boolean verify;
    public Boolean limitToOptions;
    public String unit;
    public String unitLabel;
    public List<ParameterOptionDTO> options;
    public List<FilterCriteriaDTO> filterCriteria;
    public ConfigDescriptionParameterDTO() {
    public ConfigDescriptionParameterDTO(String name, Type type, BigDecimal minimum, BigDecimal maximum,
            BigDecimal stepsize, String pattern, Boolean required, Boolean readOnly, Boolean multiple, String context,
            String defaultValue, String label, String description, List<ParameterOptionDTO> options,
            List<FilterCriteriaDTO> filterCriteria, String groupName, Boolean advanced, Boolean limitToOptions,
            Integer multipleLimit, String unit, String unitLabel, Boolean verify) {
        this.stepsize = stepsize;
