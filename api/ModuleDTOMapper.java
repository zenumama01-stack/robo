public class ModuleDTOMapper {
    protected static void fillProperties(final Module from, final ModuleDTO to) {
        to.id = from.getId();
        to.label = from.getLabel();
        to.description = from.getDescription();
        to.configuration = from.getConfiguration().getProperties();
        to.type = from.getTypeUID();
