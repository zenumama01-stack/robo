 * @author Kai Kreuzer - Changed to using ModuleBuilder
public class ActionDTOMapper extends ModuleDTOMapper {
    public static ActionDTO map(final Action action) {
        final ActionDTO actionDto = new ActionDTO();
        fillProperties(action, actionDto);
        actionDto.inputs = action.getInputs();
        return actionDto;
    public static Action mapDto(final ActionDTO actionDto) {
        return ModuleBuilder.createAction().withId(actionDto.id).withTypeUID(actionDto.type)
                .withConfiguration(new Configuration(actionDto.configuration)).withInputs(actionDto.inputs)
                .withLabel(actionDto.label).withDescription(actionDto.description).build();
    public static List<ActionDTO> map(final @Nullable Collection<? extends Action> actions) {
        if (actions == null) {
        final List<ActionDTO> dtos = new ArrayList<>(actions.size());
        for (final Action action : actions) {
            dtos.add(map(action));
        return dtos;
    public static List<Action> mapDto(final @Nullable Collection<ActionDTO> dtos) {
        if (dtos == null) {
        final List<Action> actions = new ArrayList<>(dtos.size());
        for (final ActionDTO dto : dtos) {
            actions.add(mapDto(dto));
