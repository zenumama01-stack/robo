public class ModuleTypeDTOMapper {
    protected static void fillProperties(final ModuleType from, final ModuleTypeDTO to) {
        to.uid = from.getUID();
        to.visibility = from.getVisibility();
        to.tags = from.getTags();
        to.configDescriptions = ConfigDescriptionDTOMapper.mapParameters(from.getConfigurationDescriptions());
