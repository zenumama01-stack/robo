 * @author Ana Dimova - extends Action Module type DTOs with composites
public class ActionTypeDTOMapper extends ModuleTypeDTOMapper {
    public static ActionTypeDTO map(final ActionType actionType) {
        return map(actionType, new ActionTypeDTO());
    public static CompositeActionTypeDTO map(final CompositeActionType actionType) {
        final CompositeActionTypeDTO actionTypeDto = map(actionType, new CompositeActionTypeDTO());
        actionTypeDto.children = ActionDTOMapper.map(actionType.getChildren());
        return actionTypeDto;
    public static ActionType map(CompositeActionTypeDTO actionTypeDto) {
        if (actionTypeDto.children == null || actionTypeDto.children.isEmpty()) {
            return new ActionType(actionTypeDto.uid, ConfigDescriptionDTOMapper.map(actionTypeDto.configDescriptions),
                    actionTypeDto.label, actionTypeDto.description, actionTypeDto.tags, actionTypeDto.visibility,
                    actionTypeDto.inputs, actionTypeDto.outputs);
            return new CompositeActionType(actionTypeDto.uid,
                    ConfigDescriptionDTOMapper.map(actionTypeDto.configDescriptions), actionTypeDto.label,
                    actionTypeDto.description, actionTypeDto.tags, actionTypeDto.visibility, actionTypeDto.inputs,
                    actionTypeDto.outputs, ActionDTOMapper.mapDto(actionTypeDto.children));
    public static List<ActionTypeDTO> map(final @Nullable Collection<ActionType> types) {
        if (types == null) {
        final List<ActionTypeDTO> dtos = new ArrayList<>(types.size());
        for (final ActionType type : types) {
            if (type instanceof CompositeActionType actionType) {
                dtos.add(map(actionType));
                dtos.add(map(type));
    private static <T extends ActionTypeDTO> T map(final ActionType actionType, final T actionTypeDto) {
        fillProperties(actionType, actionTypeDto);
        actionTypeDto.inputs = actionType.getInputs();
        actionTypeDto.outputs = actionType.getOutputs();
