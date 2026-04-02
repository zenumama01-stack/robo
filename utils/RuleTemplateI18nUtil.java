 * This class is used as utility for resolving the localized {@link RuleTemplate}s. It automatically infers the key if
 * the default text is not a constant with the assistance of {@link TranslationProvider}.
public class RuleTemplateI18nUtil {
    public static final String RULE_TEMPLATE = "rule-template";
    public RuleTemplateI18nUtil(TranslationProvider i18nProvider) {
    public @Nullable String getLocalizedRuleTemplateLabel(Bundle bundle, String ruleTemplateUID,
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferRuleTemplateKey(ruleTemplateUID, "label"));
    public @Nullable String getLocalizedRuleTemplateDescription(Bundle bundle, String ruleTemplateUID,
                () -> inferRuleTemplateKey(ruleTemplateUID, "description"));
    private String inferRuleTemplateKey(String ruleTemplateUID, String lastSegment) {
        return RULE_TEMPLATE + "." + ruleTemplateUID + "." + lastSegment;
