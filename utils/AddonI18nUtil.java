package org.openhab.core.addon.internal;
import org.openhab.core.i18n.I18nUtil;
 * The {@link AddonI18nUtil} uses the {@link TranslationProvider} to resolve the
 * localized texts. It automatically infers the key if the default text is not a
public class AddonI18nUtil {
    private final TranslationProvider i18nProvider;
    public AddonI18nUtil(TranslationProvider i18nProvider) {
        this.i18nProvider = i18nProvider;
    public String getDescription(Bundle bundle, String addonId, String defaultDescription, @Nullable Locale locale) {
        String key = I18nUtil.stripConstantOr(defaultDescription, () -> inferKey(addonId, "description"));
        String localizedText = i18nProvider.getText(bundle, key, defaultDescription, locale);
        return localizedText != null ? localizedText : defaultDescription;
    public String getName(Bundle bundle, String addonId, String defaultLabel, @Nullable Locale locale) {
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferKey(addonId, "name"));
        String localizedText = i18nProvider.getText(bundle, key, defaultLabel, locale);
        return localizedText != null ? localizedText : defaultLabel;
    private String inferKey(String addonId, String lastSegment) {
        return "addon." + addonId + "." + lastSegment;
