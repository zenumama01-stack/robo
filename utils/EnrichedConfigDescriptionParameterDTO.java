import org.openhab.core.config.core.dto.FilterCriteriaDTO;
import org.openhab.core.config.core.dto.ParameterOptionDTO;
 * This is an enriched data transfer object that is used to serialize config descriptions parameters with a list of
 * default values if a configuration description defines <code>multiple="true"</code>.
 * The default values are split by a comma. Individual values that contain a comma
 * must be escaped with a backslash character. The backslash character will be
 * removed from the value.
@Schema(name = "EnrichedConfigDescriptionParameter")
public class EnrichedConfigDescriptionParameterDTO extends ConfigDescriptionParameterDTO {
    public Collection<String> defaultValues;
    public EnrichedConfigDescriptionParameterDTO(String name, Type type, BigDecimal minimum, BigDecimal maximum,
        super(name, type, minimum, maximum, stepsize, pattern, required, readOnly, multiple, context, defaultValue,
                label, description, options, filterCriteria, groupName, advanced, limitToOptions, multipleLimit, unit,
                unitLabel, verify);
        if (multiple && defaultValue != null) {
            ConfigDescriptionParameter parameter = ConfigDescriptionParameterBuilder.create(name, type)
                    .withMultiple(multiple).withDefault(defaultValue).withMultipleLimit(multipleLimit).build();
            if (ConfigUtil.getDefaultValueAsCorrectType(parameter) instanceof List defaultValues) {
                this.defaultValues = (Collection<String>) defaultValues;
