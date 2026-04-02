package org.openhab.core.thing.internal.profiles.i18n;
 * A utility service which localizes {@link Profile}s.
 * Falls back to a localized {@link ProfileType} for label and description when not given otherwise.
 * @see org.openhab.core.thing.profiles.i18n.ProfileTypeI18nLocalizationService
public class ProfileI18nUtil {
     * @param i18nProvider an instance of {@link TranslationProvider}.
    public ProfileI18nUtil(TranslationProvider i18nProvider) {
    public @Nullable String getProfileLabel(@Nullable Bundle bundle, ProfileTypeUID profileTypeUID, String defaultLabel,
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferProfileTypeKey(profileTypeUID, "label"));
    private String inferProfileTypeKey(ProfileTypeUID profileTypeUID, String lastSegment) {
        return "profile-type.%s.%s.%s".formatted(profileTypeUID.getBindingId(), profileTypeUID.getId(), lastSegment);
