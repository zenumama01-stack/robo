public class TriggerDTOMapper extends ModuleDTOMapper {
    public static TriggerDTO map(final Trigger trigger) {
        final TriggerDTO triggerDto = new TriggerDTO();
        fillProperties(trigger, triggerDto);
        return triggerDto;
    public static Trigger mapDto(final TriggerDTO triggerDto) {
        return ModuleBuilder.createTrigger().withId(triggerDto.id).withTypeUID(triggerDto.type)
                .withConfiguration(new Configuration(triggerDto.configuration)).withLabel(triggerDto.label)
                .withDescription(triggerDto.description).build();
    public static List<TriggerDTO> map(final @Nullable Collection<? extends Trigger> triggers) {
        if (triggers == null) {
        final List<TriggerDTO> dtos = new ArrayList<>(triggers.size());
        for (final Trigger trigger : triggers) {
            dtos.add(map(trigger));
    public static List<Trigger> mapDto(final @Nullable Collection<TriggerDTO> dtos) {
        final List<Trigger> triggers = new ArrayList<>(dtos.size());
        for (final TriggerDTO dto : dtos) {
            triggers.add(mapDto(dto));
