package org.openhab.core.automation.module.script.rulesupport.shared;
 * The {@link RuleSupportRuleRegistryDelegate} is wrapping a {@link RuleRegistry} to provide a comfortable way to add
 * rules to the RuleManager without worrying about the need to remove rules again. Nonetheless, using the addPermanent
 * method it is still possible to add rules permanently.
public class RuleSupportRuleRegistryDelegate implements RuleRegistry {
    private final Set<String> rules = new HashSet<>();
    public RuleSupportRuleRegistryDelegate(RuleRegistry ruleRegistry, ScriptedRuleProvider ruleProvider) {
    public void addRegistryChangeListener(RegistryChangeListener<Rule> listener) {
        ruleRegistry.addRegistryChangeListener(listener);
    public Collection<Rule> getAll() {
        return ruleRegistry.getAll();
    public Stream<Rule> stream() {
        return ruleRegistry.stream();
    public @Nullable Rule get(String key) {
        return ruleRegistry.get(key);
    public void removeRegistryChangeListener(RegistryChangeListener<Rule> listener) {
        ruleRegistry.removeRegistryChangeListener(listener);
    public Rule add(Rule element) {
        ruleProvider.add(element);
        rules.add(element.getUID());
     * add a rule permanently to the RuleManager
     * @param element the rule
    public void addPermanent(Rule element) {
        ruleRegistry.add(element);
    public @Nullable Rule update(Rule element) {
        return ruleRegistry.update(element);
    public @Nullable Rule remove(String key) {
        if (rules.remove(key)) {
            return ruleProvider.remove(key);
        return ruleRegistry.remove(key);
    public Collection<Rule> getByTag(@Nullable String tag) {
        return ruleRegistry.getByTag(tag);
     * called when the script is unloaded or reloaded
        for (String rule : rules) {
            ruleProvider.remove(rule);
        rules.clear();
    public Collection<Rule> getByTags(String... tags) {
        return ruleRegistry.getByTags(tags);
    public void regenerateFromTemplate(String ruleUID) {
        ruleRegistry.regenerateFromTemplate(ruleUID);
