package org.openhab.core.automation.internal.template;
 * The implementation of {@link TemplateRegistry} that is registered as a service.
 * @author Ana Dimova - TemplateRegistry extends AbstractRegistry
@Component(service = { TemplateRegistry.class, RuleTemplateRegistry.class }, immediate = true)
public class RuleTemplateRegistry extends AbstractRegistry<RuleTemplate, String, RuleTemplateProvider>
        implements TemplateRegistry<RuleTemplate> {
    public RuleTemplateRegistry() {
        super(RuleTemplateProvider.class);
    protected void addProvider(Provider<RuleTemplate> provider) {
        if (provider instanceof TemplateProvider) {
    public @Nullable RuleTemplate get(String templateUID) {
        return get(templateUID, null);
    public @Nullable RuleTemplate get(String templateUID, @Nullable Locale locale) {
        Entry<Provider<RuleTemplate>, RuleTemplate> prt = getValueAndProvider(templateUID);
        if (prt == null) {
            RuleTemplate t = locale == null ? prt.getValue()
                    : ((RuleTemplateProvider) prt.getKey()).getTemplate(templateUID, locale);
            return t != null ? createCopy(t) : null;
    private RuleTemplate createCopy(RuleTemplate template) {
        return new RuleTemplate(template.getUID(), template.getLabel(), template.getDescription(),
                new HashSet<>(template.getTags()), new ArrayList<>(template.getTriggers()),
                new ArrayList<>(template.getConditions()), new ArrayList<>(template.getActions()),
                new LinkedList<>(template.getConfigurationDescriptions()), template.getVisibility());
    public Collection<RuleTemplate> getByTag(@Nullable String tag) {
        return getByTag(tag, null);
    public Collection<RuleTemplate> getByTag(@Nullable String tag, @Nullable Locale locale) {
        Collection<RuleTemplate> result = new ArrayList<>();
        forEach((provider, resultTemplate) -> {
            Collection<String> tags = resultTemplate.getTags();
            RuleTemplate t = locale == null ? resultTemplate
                    : ((RuleTemplateProvider) provider).getTemplate(resultTemplate.getUID(), locale);
            if (t != null && (tag == null || tags.contains(tag))) {
                result.add(t);
    public Collection<RuleTemplate> getByTags(String... tags) {
        return getByTags(null, tags);
    public Collection<RuleTemplate> getByTags(@Nullable Locale locale, String... tags) {
            Collection<String> tTags = resultTemplate.getTags();
            if (t != null && tTags.containsAll(tagSet)) {
    public Collection<RuleTemplate> getAll(@Nullable Locale locale) {
        return getByTag(null, locale);
