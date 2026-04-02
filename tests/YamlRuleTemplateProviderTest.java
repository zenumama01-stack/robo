 * Tests some general behavior and parsing of specific YAML rule template files.
public class YamlRuleTemplateProviderTest {
    private static final Path SOURCE_PATH = Path.of("src/test/resources/model/rule-templates");
    private static final String TEMPLATES_NAME = "rule-templates.yaml";
    private static final Path TEMPLATES_PATH = Path.of(TEMPLATES_NAME);
    private @NonNullByDefault({}) Path templatesPath;
        templatesPath = watchPath.resolve(TEMPLATES_PATH);
        Files.copy(SOURCE_PATH.resolve("BasicRuleTemplate.yaml"), templatesPath);
        YamlRuleTemplateProvider templateProvider = new YamlRuleTemplateProvider();
        TestRuleTemplateChangeListener templateListener = new TestRuleTemplateChangeListener();
        templateProvider.addProviderChangeListener(templateListener);
        modelRepository.addYamlModelListener(templateProvider);
        modelRepository.processWatchEvent(WatchService.Kind.CREATE, templatesPath);
        assertThat(templateListener.templates, is(aMapWithSize(1)));
        assertThat(templateListener.templates, hasKey("basic:yaml-rule-template"));
        Files.copy(SOURCE_PATH.resolve("FullRuleTemplate.yaml"), templatesPath, StandardCopyOption.REPLACE_EXISTING);
        modelRepository.processWatchEvent(WatchService.Kind.MODIFY, templatesPath);
        assertThat(templateListener.templates, hasKey("test:full-rule-template"));
        modelRepository.processWatchEvent(WatchService.Kind.DELETE, templatesPath);
        assertThat(templateListener.templates, is(anEmptyMap()));
    public void basicRuleTemplateTest() throws IOException {
        YamlRuleTemplateProvider ruleTemplateProvider = new YamlRuleTemplateProvider();
        ruleTemplateProvider.addProviderChangeListener(templateListener);
        modelRepository.addYamlModelListener(ruleTemplateProvider);
        RuleTemplate template = Objects.requireNonNull(templateListener.templates.get("basic:yaml-rule-template"));
        assertThat(template.getUID(), is("basic:yaml-rule-template"));
        assertThat(template.getLabel(), is("Basic YAML Rule Template"));
        assertThat(template.getDescription(), is("A YAML rule made from a template."));
        assertThat(template.getTags(), hasItem("Basic"));
    public void fullRuleTemplateTest() throws IOException {
        Files.copy(SOURCE_PATH.resolve("FullRuleTemplate.yaml"), templatesPath);
        RuleTemplate template = Objects.requireNonNull(templateListener.templates.get("test:full-rule-template"));
        assertThat(template.getUID(), is("test:full-rule-template"));
        assertThat(template.getLabel(), is("Full Rule Template"));
        assertThat(template.getDescription(), is("The description of the template-based full rule"));
    public static class TestRuleTemplateChangeListener implements ProviderChangeListener<RuleTemplate> {
        public final Map<String, RuleTemplate> templates = new HashMap<>();
        public void added(Provider<RuleTemplate> provider, RuleTemplate element) {
            templates.put(element.getUID(), element);
        public void removed(Provider<RuleTemplate> provider, RuleTemplate element) {
            templates.remove(element.getUID());
        public void updated(Provider<RuleTemplate> provider, RuleTemplate oldelement, RuleTemplate element) {
