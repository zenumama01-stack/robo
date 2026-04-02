 * Implementation of a service that provides i18n functionality for automation modules
public class ModuleTypeI18nServiceImpl implements ModuleTypeI18nService {
    private final Logger logger = LoggerFactory.getLogger(ModuleTypeI18nServiceImpl.class);
    private final ModuleTypeI18nUtil moduleTypeI18nUtil;
    public ModuleTypeI18nServiceImpl(final @Reference ConfigI18nLocalizationService configI18nService,
        this.moduleTypeI18nUtil = new ModuleTypeI18nUtil(i18nProvider);
    public @Nullable ModuleType getModuleTypePerLocale(@Nullable ModuleType defModuleType, @Nullable Locale locale,
            Bundle bundle) {
        if (defModuleType == null || locale == null) {
            return defModuleType;
        String llabel = moduleTypeI18nUtil.getLocalizedModuleTypeLabel(bundle, uid, defModuleType.getLabel(), locale);
        String ldescription = moduleTypeI18nUtil.getLocalizedModuleTypeDescription(bundle, uid,
                defModuleType.getDescription(), locale);
        List<ConfigDescriptionParameter> lconfigDescriptionParameters = getLocalizedConfigDescriptionParameters(
                defModuleType.getConfigurationDescriptions(), ModuleTypeI18nUtil.MODULE_TYPE, uid, bundle, locale);
        if (defModuleType instanceof ActionType type) {
            return createLocalizedActionType(type, bundle, uid, locale, lconfigDescriptionParameters,
                    llabel == null ? defModuleType.getLabel() : llabel,
                    ldescription == null ? defModuleType.getDescription() : ldescription);
        if (defModuleType instanceof ConditionType type) {
            return createLocalizedConditionType(type, bundle, uid, locale, lconfigDescriptionParameters,
        if (defModuleType instanceof TriggerType type) {
            return createLocalizedTriggerType(type, bundle, uid, locale, lconfigDescriptionParameters,
                    llabel != null ? llabel : defModuleType.getLabel(),
    private @Nullable List<ConfigDescriptionParameter> getLocalizedConfigDescriptionParameters(
            List<ConfigDescriptionParameter> parameters, String prefix, String uid, Bundle bundle,
            return configI18nService
                    .getLocalizedConfigDescription(bundle, ConfigDescriptionBuilder
                            .create(new URI(prefix + ":" + uid + ".name")).withParameters(parameters).build(), locale)
                    .getParameters();
     * Utility method for localization of ActionTypes.
     * @param at is an ActionType for localization.
     * @param bundle the bundle providing localization resources.
     * @param moduleTypeUID is an ActionType uid.
     * @param lconfigDescriptions are ActionType localized config descriptions.
     * @param llabel is an ActionType localized label.
     * @param ldescription is an ActionType localized description.
     * @return localized ActionType.
    private @Nullable ActionType createLocalizedActionType(ActionType at, Bundle bundle, String moduleTypeUID,
            @Nullable Locale locale, @Nullable List<ConfigDescriptionParameter> lconfigDescriptions,
            @Nullable String llabel, @Nullable String ldescription) {
        List<Input> inputs = moduleTypeI18nUtil.getLocalizedInputs(at.getInputs(), bundle, moduleTypeUID, locale);
        List<Output> outputs = moduleTypeI18nUtil.getLocalizedOutputs(at.getOutputs(), bundle, moduleTypeUID, locale);
        ActionType lat;
        if (at instanceof CompositeActionType type) {
            List<Action> modules = moduleI18nUtil.getLocalizedModules(type.getChildren(), bundle, moduleTypeUID,
                    ModuleTypeI18nUtil.MODULE_TYPE, locale);
            lat = new CompositeActionType(moduleTypeUID, lconfigDescriptions, llabel, ldescription, at.getTags(),
                    at.getVisibility(), inputs, outputs, modules);
            lat = new ActionType(moduleTypeUID, lconfigDescriptions, llabel, ldescription, at.getTags(),
                    at.getVisibility(), inputs, outputs);
        return lat;
     * Utility method for localization of ConditionTypes.
     * @param ct is a ConditionType for localization.
     * @param moduleTypeUID is a ConditionType uid.
     * @param lconfigDescriptions are ConditionType localized config descriptions.
     * @param llabel is a ConditionType localized label.
     * @param ldescription is a ConditionType localized description.
     * @return localized ConditionType.
    private @Nullable ConditionType createLocalizedConditionType(ConditionType ct, Bundle bundle, String moduleTypeUID,
        List<Input> inputs = moduleTypeI18nUtil.getLocalizedInputs(ct.getInputs(), bundle, moduleTypeUID, locale);
        ConditionType lct;
        if (ct instanceof CompositeConditionType type) {
            List<Condition> modules = moduleI18nUtil.getLocalizedModules(type.getChildren(), bundle, moduleTypeUID,
            lct = new CompositeConditionType(moduleTypeUID, lconfigDescriptions, llabel, ldescription, ct.getTags(),
                    ct.getVisibility(), inputs, modules);
            lct = new ConditionType(moduleTypeUID, lconfigDescriptions, llabel, ldescription, ct.getTags(),
                    ct.getVisibility(), inputs);
        return lct;
     * Utility method for localization of TriggerTypes.
     * @param tt is a TriggerType for localization.
     * @param moduleTypeUID is a TriggerType uid.
     * @param lconfigDescriptions are TriggerType localized config descriptions.
     * @param llabel is a TriggerType localized label.
     * @param ldescription is a TriggerType localized description.
     * @return localized TriggerType.
    private @Nullable TriggerType createLocalizedTriggerType(TriggerType tt, Bundle bundle, String moduleTypeUID,
        List<Output> outputs = moduleTypeI18nUtil.getLocalizedOutputs(tt.getOutputs(), bundle, moduleTypeUID, locale);
        TriggerType ltt;
        if (tt instanceof CompositeTriggerType type) {
            List<Trigger> modules = moduleI18nUtil.getLocalizedModules(type.getChildren(), bundle, moduleTypeUID,
            ltt = new CompositeTriggerType(moduleTypeUID, lconfigDescriptions, llabel, ldescription, tt.getTags(),
                    tt.getVisibility(), outputs, modules);
            ltt = new TriggerType(moduleTypeUID, lconfigDescriptions, llabel, ldescription, tt.getTags(),
                    tt.getVisibility(), outputs);
        return ltt;
