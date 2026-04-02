import org.openhab.core.config.core.dto.ConfigDescriptionDTO;
import org.openhab.core.config.core.dto.ConfigDescriptionParameterGroupDTO;
 * The {@link EnrichedConfigDescriptionDTOMapper} is a utility class to map {@link ConfigDescription}s to config
 * descriptions data transform objects {@link ConfigDescriptionDTO} containing
 * {@link EnrichedConfigDescriptionParameterDTO}.
public class EnrichedConfigDescriptionDTOMapper extends ConfigDescriptionDTOMapper {
     * @return enriched configuration description DTO object
        List<ConfigDescriptionParameterDTO> parameters = mapEnrichedParameters(configDescription.getParameters());
     * Maps configuration description parameters into enriched DTO objects.
     * @return the parameter enriched DTO objects
    public static List<ConfigDescriptionParameterDTO> mapEnrichedParameters(
            List<ConfigDescriptionParameter> parameters) {
            ConfigDescriptionParameterDTO configDescriptionParameterBean = new EnrichedConfigDescriptionParameterDTO(
