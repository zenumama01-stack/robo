 * Tests some general behavior and parsing of specific YAML rule files.
public class YamlRuleProviderTest {
    private static final Path SOURCE_PATH = Path.of("src/test/resources/model/rules");
    private static final String RULES_NAME = "rules.yaml";
    private static final Path RULES_PATH = Path.of(RULES_NAME);
    private @NonNullByDefault({}) Path rulesPath;
        rulesPath = watchPath.resolve(RULES_PATH);
    public void yamlModelListenerTest() throws IOException {
        Files.copy(SOURCE_PATH.resolve("BasicRule.yaml"), rulesPath);
        YamlRuleProvider ruleProvider = new YamlRuleProvider();
        TestRuleChangeListener ruleListener = new TestRuleChangeListener();
        ruleProvider.addProviderChangeListener(ruleListener);
        modelRepository.addYamlModelListener(ruleProvider);
        modelRepository.processWatchEvent(WatchService.Kind.CREATE, rulesPath);
        assertThat(ruleListener.rules, is(aMapWithSize(1)));
        assertThat(ruleListener.rules, hasKey("basic:basicyamlrule"));
        Files.copy(SOURCE_PATH.resolve("MixedRules.yaml"), rulesPath, StandardCopyOption.REPLACE_EXISTING);
        modelRepository.processWatchEvent(WatchService.Kind.MODIFY, rulesPath);
        assertThat(ruleListener.rules, is(aMapWithSize(4)));
        assertThat(ruleListener.rules, hasKey("mode-tv-rule"));
        assertThat(ruleListener.rules, hasKey("stub:mode-tv-rule"));
        assertThat(ruleListener.rules, hasKey("rules_tools:tsm"));
        assertThat(ruleListener.rules, hasKey("ysc:washing_machine_alert_test"));
        modelRepository.processWatchEvent(WatchService.Kind.DELETE, rulesPath);
        assertThat(ruleListener.rules, is(anEmptyMap()));
    public void emptyRuleTest() throws IOException {
        Files.copy(SOURCE_PATH.resolve("EmptyRule.yaml"), rulesPath);
    public void basicRuleTest() throws IOException {
        Rule rule = Objects.requireNonNull(ruleListener.rules.get("basic:basicyamlrule"));
        assertThat(rule.getUID(), is("basic:basicyamlrule"));
        assertThat(rule.getName(), is("Basic YAML Rule"));
        assertThat(rule.getTemplateState(), is(TemplateState.NO_TEMPLATE));
        assertThat(rule.getConfiguration().getProperties(), is(anEmptyMap()));
        assertThat(rule.getConfigurationDescriptions(), is(empty()));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("startlevel", BigDecimal.valueOf(100L)));
        assertThat(condition.getConfiguration().getProperties(), hasEntry("offset", BigDecimal.valueOf(0L)));
        assertThat(condition.getConfiguration().getProperties(), hasEntry("offset", BigDecimal.valueOf(2L)));
    public void mixedRulesTest() throws IOException {
        Files.copy(SOURCE_PATH.resolve("MixedRules.yaml"), rulesPath);
        Rule rule = Objects.requireNonNull(ruleListener.rules.get("mode-tv-rule"));
        assertThat(rule.getUID(), is("mode-tv-rule"));
        assertThat(rule.getName(), is("Mode TV"));
        assertThat(rule.getTemplateState(), is(TemplateState.INSTANTIATED));
        assertThat(config.getProperties(), hasEntry("sourceItem", "None"));
        assertThat(parameter.getName(), is("sourceItem"));
        assertThat(parameter.getLabel(), is("Source Item"));
        assertThat(parameter.getDescription(), is("The source Item whose state to monitor"));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("itemName", "TvPower"));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("state", "ON"));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("previousState", "OFF"));
        assertThat(trigger.getConfiguration().getProperties(), is(aMapWithSize(3)));
        assertThat(rule.getConditions(), is(empty()));
        assertThat(action.getId(), is("script"));
        assertThat(action.getConfiguration().getProperties(), hasKey("script"));
        assertThat(action.getConfiguration().getProperties().get("script"), is(instanceOf(String.class)));
        rule = Objects.requireNonNull(ruleListener.rules.get("stub:mode-tv-rule"));
        assertThat(rule.getUID(), is("stub:mode-tv-rule"));
        assertThat(rule.getName(), is("Template based mode TV"));
        assertThat(rule.getTemplateUID(), is("mode-tv-template"));
        assertThat(rule.getTemplateState(), is(TemplateState.PENDING));
        assertThat(config.getProperties(), hasEntry("sourceItem", "TvPower"));
        rule = Objects.requireNonNull(ruleListener.rules.get("rules_tools:tsm"));
        assertThat(rule.getUID(), is("rules_tools:tsm"));
        assertThat(rule.getName(), is("Time Based State Machine Test Rule"));
        assertThat(rule.getDescription(),
                is("Creates timers to transition a state Item to a new state at defined times of day."));
        assertThat(rule.getTemplateUID(), is("none"));
        assertThat(rule.getTemplateState(), is(TemplateState.TEMPLATE_MISSING));
        assertThat(config.getProperties(), is(aMapWithSize(3)));
        assertThat(config.getProperties(), hasEntry("namespace", "None"));
        assertThat(config.getProperties(), hasEntry("timesOfDayGrp", "No"));
        assertThat(config.getProperties(), hasEntry("timeOfDay", "Yes"));
        assertThat(parameter.getName(), is("timeOfDay"));
        assertThat(parameter.getLabel(), is("Time of Day State Item"));
        assertThat(parameter.getDescription(), is("String Item that holds the current time of day's state."));
        List<FilterCriteria> filterCriteria = parameter.getFilterCriteria();
        assertThat(filterCriteria, hasSize(1));
        assertThat(filterCriteria.getFirst().getName(), is("type"));
        assertThat(filterCriteria.getFirst().getValue(), is("String"));
        assertThat(parameter.getName(), is("timesOfDayGrp"));
        assertThat(parameter.getLabel(), is("Times of Day Group"));
                is("Has as members all the DateTime Items that define time of day states."));
        filterCriteria = parameter.getFilterCriteria();
        assertThat(filterCriteria.getFirst().getValue(), is("Group"));
        assertThat(parameter.getName(), is("namespace"));
        assertThat(parameter.getLabel(), is("Time of Day Namespace"));
        assertThat(parameter.getDescription(), is("The Item metadata namespace (e.g. \"tsm\")."));
        assertThat(triggers, hasSize(3));
        assertThat(trigger.getTypeUID(), is("core.GroupStateChangeTrigger"));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("groupName", "DemoSwitchGroup"));
        trigger = triggers.get(2);
        assertThat(trigger.getId(), is("4"));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("time", "00:05"));
        assertThat(action.getId(), is("3"));
        assertThat(action.getConfiguration().getProperties().get("script"), is(
                "// Version 1.0\nvar {TimerMgr, helpers} = require('openhab_rules_tools');\nconsole.loggerName = 'org.openhab.automation.rules_tools.TimeStateMachine';\n"));
        rule = Objects.requireNonNull(ruleListener.rules.get("ysc:washing_machine_alert_test"));
        assertThat(rule.getUID(), is("ysc:washing_machine_alert_test"));
        assertThat(rule.getName(), is("Alert when Washing Machine Finished Test"));
        assertThat(rule.getDescription(), is(
                "This will monitor the power consumption of a washing machine and send an alert command when it gets below a threshold, meaning it has finished."));
        assertThat(configDescriptions, hasSize(4));
        assertThat(parameter.getName(), is("powerItem"));
        assertThat(parameter.getLabel(), is("Power Item"));
                "Item that holds the power (in watts) of the washing machine. Can be a quantity type (Number:Power)."));
        assertThat(parameter.getName(), is("threshold"));
        assertThat(parameter.getLabel(), is("Threshold"));
                "When the power measurement was at or above the threshold and crosses below it, trigger the alert."));
        assertThat(parameter.getDefault(), is("2"));
        assertThat(parameter.getName(), is("alertItem"));
        assertThat(parameter.getLabel(), is("Alert Item"));
                "Item to send a command to when the measured power gets below the threshold. For instance, a Hue light advanced Alert channel."));
        assertThat(parameter.getName(), is("alertCommand"));
        assertThat(parameter.getLabel(), is("Alert Command"));
                "Command to send to the alert item (for an item linked to a Hue light alert channel, LSELECT will flash the light for a few seconds)."));
        assertThat(parameter.getDefault(), is("LSELECT"));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("itemName", "CurrentPower"));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("state", ""));
        assertThat(trigger.getConfiguration().getProperties(), is(aMapWithSize(2)));
    public void fullRuleTest() throws IOException {
        Files.copy(SOURCE_PATH.resolve("FullRule.yaml"), rulesPath);
        Rule rule = Objects.requireNonNull(ruleListener.rules.get("test:full-rule"));
        assertThat(rule.getUID(), is("test:full-rule"));
        assertThat(rule.getName(), is("Full Rule"));
        assertThat(rule.getDescription(), is("The description of the full rule"));
        assertThat(rule.getVisibility(), is(Visibility.EXPERT));
        assertThat(config.getProperties(), hasEntry("integerParam", BigDecimal.valueOf(5L)));
        assertThat(config.getProperties(), hasEntry("booleanParam", true));
        assertThat(trigger.getConfiguration().getProperties(), hasEntry("startlevel", BigDecimal.valueOf(80L)));
        assertThat(action.getConfiguration().getProperties(), hasEntry("volume", BigDecimal.valueOf(100L)));
    public static class TestRuleChangeListener implements ProviderChangeListener<Rule> {
        public final Map<String, Rule> rules = new HashMap<>();
            rules.put(element.getUID(), element);
        public void removed(Provider<Rule> provider, Rule element) {
            rules.remove(element.getUID());
        public void updated(Provider<Rule> provider, Rule oldelement, Rule element) {
