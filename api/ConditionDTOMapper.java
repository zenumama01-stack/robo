public class ConditionDTOMapper extends ModuleDTOMapper {
    public static ConditionDTO map(final Condition condition) {
        final ConditionDTO conditionDto = new ConditionDTO();
        fillProperties(condition, conditionDto);
        conditionDto.inputs = condition.getInputs();
        return conditionDto;
    public static Condition mapDto(final ConditionDTO conditionDto) {
        return ModuleBuilder.createCondition().withId(conditionDto.id).withTypeUID(conditionDto.type)
                .withConfiguration(new Configuration(conditionDto.configuration)).withInputs(conditionDto.inputs)
                .withLabel(conditionDto.label).withDescription(conditionDto.description).build();
    public static List<ConditionDTO> map(final @Nullable List<? extends Condition> conditions) {
        if (conditions == null) {
        final List<ConditionDTO> dtos = new ArrayList<>(conditions.size());
        for (final Condition action : conditions) {
    public static List<Condition> mapDto(final @Nullable List<ConditionDTO> dtos) {
        final List<Condition> conditions = new ArrayList<>(dtos.size());
        for (final ConditionDTO dto : dtos) {
            conditions.add(mapDto(dto));
