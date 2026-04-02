package org.openhab.core.automation.module.provider.i18n;
 * Interface for a service that offer i18n functionality
public interface ModuleTypeI18nService {
     * Builds a {@link ModuleType} with the given {@link Locale}
     * @param defModuleType the ModuleType as defined
     * @param locale a Locale into which the type should be translated
     * @param bundle the bundle containing the localization files
     * @return the localized ModuleType
    ModuleType getModuleTypePerLocale(@Nullable ModuleType defModuleType, @Nullable Locale locale, Bundle bundle);
