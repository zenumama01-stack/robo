 * Implementation of a rule provider that uses the storage service for persistence
 * @author Ana Dimova - Persistence implementation
 * @author Kai Kreuzer - refactored (managed) provider and registry implementation
 * @author Markus Rathgeb - fix mapping between element and persistable element
@Component(service = { RuleProvider.class, ManagedRuleProvider.class })
public class ManagedRuleProvider extends AbstractManagedProvider<Rule, String, RuleDTO> implements RuleProvider {
    public ManagedRuleProvider(final @Reference StorageService storageService) {
        return "automation_rules";
    protected @Nullable Rule toElement(String key, RuleDTO persistableElement) {
        return RuleDTOMapper.map(persistableElement);
    protected RuleDTO toPersistableElement(Rule element) {
        return RuleDTOMapper.map(element);
