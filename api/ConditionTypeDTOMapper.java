 * @author Ana Dimova - extends Condition Module type DTOs with composites
public class ConditionTypeDTOMapper extends ModuleTypeDTOMapper {
    public static ConditionTypeDTO map(final ConditionType conditionType) {
        return map(conditionType, new ConditionTypeDTO());
    public static CompositeConditionTypeDTO map(final CompositeConditionType conditionType) {
        final CompositeConditionTypeDTO conditionTypeDto = map(conditionType, new CompositeConditionTypeDTO());
        conditionTypeDto.children = ConditionDTOMapper.map(conditionType.getChildren());
        return conditionTypeDto;
    public static ConditionType map(CompositeConditionTypeDTO conditionTypeDto) {
        if (conditionTypeDto.children == null || conditionTypeDto.children.isEmpty()) {
            return new ConditionType(conditionTypeDto.uid,
                    ConfigDescriptionDTOMapper.map(conditionTypeDto.configDescriptions), conditionTypeDto.label,
                    conditionTypeDto.description, conditionTypeDto.tags, conditionTypeDto.visibility,
                    conditionTypeDto.inputs);
            return new CompositeConditionType(conditionTypeDto.uid,
                    conditionTypeDto.inputs, ConditionDTOMapper.mapDto(conditionTypeDto.children));
    public static List<ConditionTypeDTO> map(final @Nullable Collection<ConditionType> types) {
        final List<ConditionTypeDTO> dtos = new ArrayList<>(types.size());
        for (final ConditionType type : types) {
            if (type instanceof CompositeConditionType conditionType) {
                dtos.add(map(conditionType));
    private static <T extends ConditionTypeDTO> T map(final ConditionType conditionType, final T conditionTypeDto) {
        fillProperties(conditionType, conditionTypeDto);
        conditionTypeDto.inputs = conditionType.getInputs();
