import org.openhab.core.automation.internal.provider.i18n.ModuleI18nUtil;
import org.openhab.core.automation.internal.provider.i18n.RuleTemplateI18nUtil;
 * This class is implementation of {@link TemplateProvider}. It serves for providing {@link RuleTemplate}s by loading
 * <li>the path to resources, corresponding to the {@link RuleTemplate}s - root directory
 * {@link AbstractResourceBundleProvider#ROOT_DIRECTORY} with sub-directory "templates".
 * <li>type of the {@link Parser}s, corresponding to the {@link RuleTemplate}s - {@link Parser#PARSER_TEMPLATE}
 * <li>specific functionality for loading the {@link RuleTemplate}s
 * <li>tracking the managing of the {@link RuleTemplate}s.
@Component(immediate = true, service = { RuleTemplateProvider.class,
        Provider.class }, property = "provider.type=bundle")
public class TemplateResourceBundleProvider extends AbstractResourceBundleProvider<RuleTemplate>
    private final RuleTemplateI18nUtil ruleTemplateI18nUtil;
    private final ModuleI18nUtil moduleI18nUtil;
     * {@link ModuleType}s and the managing service of the {@link RuleTemplate}s.
    public TemplateResourceBundleProvider(final @Reference ConfigI18nLocalizationService configI18nService,
            final @Reference TranslationProvider i18nProvider) {
        super(ROOT_DIRECTORY + "/templates/");
        this.ruleTemplateI18nUtil = new RuleTemplateI18nUtil(i18nProvider);
        this.moduleI18nUtil = new ModuleI18nUtil(i18nProvider);
    protected void addParser(Parser<RuleTemplate> parser, Map<String, String> properties) {
    protected void removeParser(Parser<RuleTemplate> parser, Map<String, String> properties) {
     * @see TemplateProvider#getTemplate(java.lang.String, java.util.Locale)
        return getPerLocale(providedObjectsHolder.get(uid), locale);
     * @see TemplateProvider#getTemplates(Locale)
        List<RuleTemplate> templatesList = new ArrayList<>();
        for (RuleTemplate t : providedObjectsHolder.values()) {
            RuleTemplate rtPerLocale = getPerLocale(t, locale);
            if (rtPerLocale != null) {
                templatesList.add(rtPerLocale);
        return templatesList;
     * This method is used to localize the {@link RuleTemplate}s.
     * @param defTemplate is the {@link RuleTemplate} that must be localized.
     * @return the localized {@link RuleTemplate}.
    private @Nullable RuleTemplate getPerLocale(@Nullable RuleTemplate defTemplate, @Nullable Locale locale) {
        if (locale == null || defTemplate == null) {
            return defTemplate;
        String uid = defTemplate.getUID();
        if (bundle != null && defTemplate instanceof RuleTemplate) {
            String llabel = ruleTemplateI18nUtil.getLocalizedRuleTemplateLabel(bundle, uid, defTemplate.getLabel(),
                    locale);
            String ldescription = ruleTemplateI18nUtil.getLocalizedRuleTemplateDescription(bundle, uid,
                    defTemplate.getDescription(), locale);
            List<ConfigDescriptionParameter> lconfigDescriptions = getLocalizedConfigurationDescription(
                    defTemplate.getConfigurationDescriptions(), bundle, uid, RuleTemplateI18nUtil.RULE_TEMPLATE,
            List<Action> lactions = moduleI18nUtil.getLocalizedModules(defTemplate.getActions(), bundle, uid,
                    RuleTemplateI18nUtil.RULE_TEMPLATE, locale);
            List<Condition> lconditions = moduleI18nUtil.getLocalizedModules(defTemplate.getConditions(), bundle, uid,
            List<Trigger> ltriggers = moduleI18nUtil.getLocalizedModules(defTemplate.getTriggers(), bundle, uid,
            return new RuleTemplate(uid, llabel, ldescription, defTemplate.getTags(), ltriggers, lconditions, lactions,
                    lconfigDescriptions, defTemplate.getVisibility());
    protected String getUID(RuleTemplate parsedObject) {
