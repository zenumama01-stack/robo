package org.openhab.core.config.core.internal.i18n;
 * The {@link ConfigDescriptionGroupI18nUtil} uses the {@link TranslationProvider} to
 * resolve the localized texts in the configuration parameter groups. It automatically infers the key if the default
 * text is not a constant.
public class ConfigDescriptionGroupI18nUtil {
    public ConfigDescriptionGroupI18nUtil(TranslationProvider i18nProvider) {
    public @Nullable String getGroupDescription(Bundle bundle, URI configDescriptionURI, String groupName,
                () -> inferKey(configDescriptionURI, groupName, "description"));
    public @Nullable String getGroupLabel(Bundle bundle, URI configDescriptionURI, String groupName,
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferKey(configDescriptionURI, groupName, "label"));
    private String inferKey(URI configDescriptionURI, String groupName, String lastSegment) {
        String uri = configDescriptionURI.getSchemeSpecificPart().replace(":", ".");
        return configDescriptionURI.getScheme() + ".config." + uri + ".group." + groupName + "." + lastSegment;
