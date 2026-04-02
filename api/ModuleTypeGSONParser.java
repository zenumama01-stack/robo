import org.openhab.core.automation.dto.CompositeActionTypeDTO;
import org.openhab.core.automation.dto.CompositeConditionTypeDTO;
import org.openhab.core.automation.dto.CompositeTriggerTypeDTO;
 * This class can parse and serialize sets of {@link ModuleType}.
@Component(immediate = true, service = Parser.class, property = { "parser.type=parser.module.type", "format=json" })
public class ModuleTypeGSONParser extends AbstractGSONParser<ModuleType> {
    public ModuleTypeGSONParser() {
    public Set<ModuleType> parse(InputStreamReader reader) throws ParsingException {
            ModuleTypeParsingContainer mtContainer = gson.fromJson(reader, ModuleTypeParsingContainer.class);
            Set<ModuleType> result = new HashSet<>();
            addAll(result, mtContainer.triggers);
            addAll(result, mtContainer.conditions);
            addAll(result, mtContainer.actions);
            throw new ParsingException(new ParsingNestedException(ParsingNestedException.MODULE_TYPE, null, e));
    public void serialize(Set<ModuleType> dataObjects, OutputStreamWriter writer) throws Exception {
        Map<String, List<? extends ModuleType>> map = createMapByType(dataObjects);
        gson.toJson(map, writer);
    private void addAll(Set<ModuleType> result, @Nullable List<? extends ModuleTypeDTO> moduleTypes) {
        if (moduleTypes != null) {
            for (ModuleTypeDTO mt : moduleTypes) {
                if (mt instanceof CompositeTriggerTypeDTO tO) {
                    result.add(TriggerTypeDTOMapper.map(tO));
                } else if (mt instanceof CompositeConditionTypeDTO tO) {
                    result.add(ConditionTypeDTOMapper.map(tO));
                } else if (mt instanceof CompositeActionTypeDTO tO) {
                    result.add(ActionTypeDTOMapper.map(tO));
    private Map<String, List<? extends ModuleType>> createMapByType(Set<ModuleType> dataObjects) {
        Map<String, List<? extends ModuleType>> map = new HashMap<>();
        List<TriggerType> triggers = new ArrayList<>();
        List<ConditionType> conditions = new ArrayList<>();
        List<ActionType> actions = new ArrayList<>();
        for (ModuleType moduleType : dataObjects) {
                triggers.add(type);
            } else if (moduleType instanceof ConditionType type) {
                conditions.add(type);
            } else if (moduleType instanceof ActionType type) {
                actions.add(type);
        map.put("triggers", triggers);
        map.put("conditions", conditions);
        map.put("actions", actions);
