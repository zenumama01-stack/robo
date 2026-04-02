package org.openhab.core.thing.profiles.i18n;
import org.openhab.core.thing.internal.profiles.i18n.ProfileI18nUtil;
 * This OSGi service could be used to localize a {@link ProfileType} using the i18n mechanism of the openHAB framework.
@Component(service = ProfileTypeI18nLocalizationService.class)
public class ProfileTypeI18nLocalizationService {
    private final ProfileI18nUtil profileI18nUtil;
    public ProfileTypeI18nLocalizationService(final @Reference TranslationProvider i18nProvider) {
        this.profileI18nUtil = new ProfileI18nUtil(i18nProvider);
    public ProfileType createLocalizedProfileType(@Nullable Bundle bundle, ProfileType profileType,
        ProfileTypeUID profileTypeUID = profileType.getUID();
        String defaultLabel = profileType.getLabel();
            String label = profileI18nUtil.getProfileLabel(bundle, profileTypeUID, defaultLabel, locale);
            label = label != null ? label : defaultLabel;
            if (profileType instanceof StateProfileType type) {
                return ProfileTypeBuilder.newState(profileTypeUID, label)
                        .withSupportedItemTypes(profileType.getSupportedItemTypes())
                        .withSupportedItemTypesOfChannel(type.getSupportedItemTypesOfChannel()).build();
            } else if (profileType instanceof TriggerProfileType type) {
                return ProfileTypeBuilder.newTrigger(profileTypeUID, label)
                        .withSupportedChannelTypeUIDs(type.getSupportedChannelTypeUIDs()).build();
