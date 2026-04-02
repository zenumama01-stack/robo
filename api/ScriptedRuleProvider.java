import org.openhab.core.automation.RuleProvider;
 * This RuleProvider keeps Rules added by scripts during runtime. This ensures that Rules are not kept on reboot,
 * but have to be added by the scripts again.
@Component(immediate = true, service = { ScriptedRuleProvider.class, RuleProvider.class })
public class ScriptedRuleProvider extends AbstractProvider<Rule>
        implements RuleProvider, ManagedProvider<Rule, String> {
    private final Map<String, Rule> rules = new HashMap<>();
        return rules.values();
    public @Nullable Rule get(String ruleUID) {
        return rules.get(ruleUID);
    public void add(Rule rule) {
        rules.put(rule.getUID(), rule);
        notifyListenersAboutAddedElement(rule);
    public void addRule(Rule rule) {
        add(rule);
    public @Nullable Rule update(Rule rule) {
        Rule oldRule = rules.get(rule.getUID());
        if (oldRule != null) {
            notifyListenersAboutUpdatedElement(oldRule, rule);
        return oldRule;
    public @Nullable Rule remove(String ruleUID) {
        Rule rule = rules.remove(ruleUID);
        if (rule != null) {
            notifyListenersAboutRemovedElement(rule);
    public void removeRule(String ruleUID) {
        remove(ruleUID);
    public void removeRule(Rule rule) {
        remove(rule.getUID());
