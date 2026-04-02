package org.openhab.core.automation.internal.provider.i18n;
 * This class is used as utility for resolving the localized {@link Module}s. It automatically infers the key if the
 * default text is not a constant with the assistance of {@link TranslationProvider}.
public class ModuleI18nUtil {
    public ModuleI18nUtil(TranslationProvider i18nProvider) {
    public <T extends Module> List<T> getLocalizedModules(List<T> modules, Bundle bundle, String uid, String prefix,
        List<T> lmodules = new ArrayList<>();
        for (T module : modules) {
            String label = getModuleLabel(bundle, uid, module.getId(), module.getLabel(), prefix, locale);
            String description = getModuleDescription(bundle, uid, module.getId(), module.getDescription(), prefix,
            T lmodule = createLocalizedModule(module, label, description);
            lmodules.add(lmodule == null ? module : lmodule);
        return lmodules;
    private <T extends Module> @Nullable T createLocalizedModule(T module, @Nullable String label,
            return (T) createLocalizedAction(action, label, description);
            return (T) createLocalizedCondition(condition, label, description);
            return (T) createLocalizedTrigger(trigger, label, description);
    private Trigger createLocalizedTrigger(Trigger module, @Nullable String label, @Nullable String description) {
        return ModuleBuilder.createTrigger(module).withLabel(label).withDescription(description).build();
    private Condition createLocalizedCondition(Condition module, @Nullable String label, @Nullable String description) {
        return ModuleBuilder.createCondition(module).withLabel(label).withDescription(description).build();
    private Action createLocalizedAction(Action module, @Nullable String label, @Nullable String description) {
        return ModuleBuilder.createAction(module).withLabel(label).withDescription(description).build();
    private @Nullable String getModuleLabel(Bundle bundle, String uid, String moduleName, @Nullable String defaultLabel,
            String prefix, @Nullable Locale locale) {
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferModuleKey(prefix, uid, moduleName, "label"));
        return i18nProvider.getText(bundle, key, defaultLabel, locale);
    private @Nullable String getModuleDescription(Bundle bundle, String uid, String moduleName,
            @Nullable String defaultDescription, String prefix, @Nullable Locale locale) {
        String key = I18nUtil.stripConstantOr(defaultDescription,
                () -> inferModuleKey(prefix, uid, moduleName, "description"));
        return i18nProvider.getText(bundle, key, defaultDescription, locale);
    private String inferModuleKey(String prefix, String uid, String moduleName, String lastSegment) {
        return prefix + uid + ".input." + moduleName + "." + lastSegment;
