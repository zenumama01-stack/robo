 * @author Ana Dimova - extends Trigger Module type DTOs with composites
public class TriggerTypeDTOMapper extends ModuleTypeDTOMapper {
    public static TriggerTypeDTO map(final TriggerType triggerType) {
        return map(triggerType, new TriggerTypeDTO());
    public static CompositeTriggerTypeDTO map(final CompositeTriggerType triggerType) {
        final CompositeTriggerTypeDTO triggerTypeDto = map(triggerType, new CompositeTriggerTypeDTO());
        triggerTypeDto.children = TriggerDTOMapper.map(triggerType.getChildren());
        return triggerTypeDto;
    public static TriggerType map(final CompositeTriggerTypeDTO triggerTypeDto) {
        if (triggerTypeDto.children == null || triggerTypeDto.children.isEmpty()) {
            return new TriggerType(triggerTypeDto.uid,
                    ConfigDescriptionDTOMapper.map(triggerTypeDto.configDescriptions), triggerTypeDto.label,
                    triggerTypeDto.description, triggerTypeDto.tags, triggerTypeDto.visibility, triggerTypeDto.outputs);
            return new CompositeTriggerType(triggerTypeDto.uid,
                    triggerTypeDto.description, triggerTypeDto.tags, triggerTypeDto.visibility, triggerTypeDto.outputs,
                    TriggerDTOMapper.mapDto(triggerTypeDto.children));
    public static List<TriggerTypeDTO> map(final @Nullable Collection<TriggerType> types) {
        final List<TriggerTypeDTO> dtos = new ArrayList<>(types.size());
        for (final TriggerType type : types) {
            if (type instanceof CompositeTriggerType triggerType) {
                dtos.add(map(triggerType));
    private static <T extends TriggerTypeDTO> T map(final TriggerType triggerType, final T triggerTypeDto) {
        fillProperties(triggerType, triggerTypeDto);
        triggerTypeDto.outputs = triggerType.getOutputs();
