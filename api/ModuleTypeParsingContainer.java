 * This is a helper data structure for GSON that represents the JSON format used when having different module types
 * within a single input stream.
public class ModuleTypeParsingContainer {
    public @Nullable List<CompositeTriggerTypeDTO> triggers;
    public @Nullable List<CompositeConditionTypeDTO> conditions;
    public @Nullable List<CompositeActionTypeDTO> actions;
