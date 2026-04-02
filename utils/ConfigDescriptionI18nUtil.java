 * The {@link ConfigDescriptionI18nUtil} uses the {@link TranslationProvider} to
 * resolve the localized texts. It automatically infers the key if the default
 * @author Alex Tugarev - Extended for pattern and option label
 * @author Thomas Höfer - Extended for unit label
 * @author Laurent Garnier - Changed inferred key for add-ons + alternative key
public class ConfigDescriptionI18nUtil {
    private static final Pattern DELIMITER = Pattern.compile("[:=\\s]");
    private static final Set<String> ADDON_TYPES = Set.of("automation", "binding", "io", "misc", "persistence", "voice",
            "ui");
    public ConfigDescriptionI18nUtil(TranslationProvider i18nProvider) {
    public @Nullable String getParameterPattern(Bundle bundle, URI configDescriptionURI, String parameterName,
            @Nullable String defaultPattern, @Nullable Locale locale) {
        return getParameterValue(bundle, configDescriptionURI, parameterName, "pattern", defaultPattern, locale);
    public @Nullable String getParameterDescription(Bundle bundle, URI configDescriptionURI, String parameterName,
        return getParameterValue(bundle, configDescriptionURI, parameterName, "description", defaultDescription,
    public @Nullable String getParameterLabel(Bundle bundle, URI configDescriptionURI, String parameterName,
        return getParameterValue(bundle, configDescriptionURI, parameterName, "label", defaultLabel, locale);
    public @Nullable String getParameterOptionLabel(Bundle bundle, URI configDescriptionURI, String parameterName,
            @Nullable String optionValue, @Nullable String defaultOptionLabel, @Nullable Locale locale) {
        if (!isValidPropertyKey(optionValue)) {
            return defaultOptionLabel;
        return getParameterValue(bundle, configDescriptionURI, parameterName, "option." + optionValue,
                defaultOptionLabel, locale);
    public @Nullable String getParameterUnitLabel(Bundle bundle, URI configDescriptionURI, String parameterName,
            @Nullable String unit, @Nullable String defaultUnitLabel, @Nullable Locale locale) {
        if (unit != null && defaultUnitLabel == null) {
            String label = i18nProvider.getText(FrameworkUtil.getBundle(this.getClass()), "unit." + unit, null, locale);
        return getParameterValue(bundle, configDescriptionURI, parameterName, "unitLabel", defaultUnitLabel, locale);
    private @Nullable String getParameterValue(Bundle bundle, URI configDescriptionURI, String parameterName,
            String parameterAttribute, @Nullable String defaultValue, @Nullable Locale locale) {
        String key = I18nUtil.stripConstantOr(defaultValue,
                () -> inferKey(true, configDescriptionURI, parameterName, parameterAttribute));
        String value = i18nProvider.getText(bundle, key, null, locale);
        if (value == null && ADDON_TYPES.contains(configDescriptionURI.getScheme())) {
            key = I18nUtil.stripConstantOr(defaultValue,
                    () -> inferKey(false, configDescriptionURI, parameterName, parameterAttribute));
            value = i18nProvider.getText(bundle, key, null, locale);
        return value != null ? value : defaultValue;
    private String inferKey(boolean checkAddonType, URI configDescriptionURI, String parameterName,
            String lastSegment) {
        String prefix = checkAddonType && ADDON_TYPES.contains(configDescriptionURI.getScheme()) ? "addon"
                : configDescriptionURI.getScheme();
        return prefix + ".config." + uri + "." + parameterName + "." + lastSegment;
    private boolean isValidPropertyKey(@Nullable String key) {
            return !DELIMITER.matcher(key).find();
