package org.openhab.core.thing.internal.link;
 * Provider for framework config parameters on {@link ItemChannelLink}s.
public class ItemChannelLinkConfigDescriptionProvider implements ConfigDescriptionProvider {
    private static final String SCHEME = "link";
    public static final String PARAM_PROFILE = "profile";
    public ItemChannelLinkConfigDescriptionProvider(final @Reference ProfileTypeRegistry profileTypeRegistry, //
        if (SCHEME.equals(uri.getScheme())) {
            ItemChannelLink link = itemChannelLinkRegistry.get(uri.getSchemeSpecificPart());
            if (link == null) {
            Item item = itemRegistry.get(link.getItemName());
            Thing thing = thingRegistry.get(link.getLinkedUID().getThingUID());
            ConfigDescriptionParameter paramProfile = ConfigDescriptionParameterBuilder.create(PARAM_PROFILE, Type.TEXT)
                    .withLabel("Profile").withDescription("the profile to use").withRequired(false)
                    .withOptions(getOptions(link, item, channel, locale)).build();
            return ConfigDescriptionBuilder.create(uri).withParameter(paramProfile).build();
    private List<ParameterOption> getOptions(ItemChannelLink link, Item item, Channel channel,
        Collection<ProfileType> profileTypes = profileTypeRegistry.getProfileTypes(locale);
        return profileTypes.stream().filter(profileType -> {
            switch (channel.getKind()) {
                    return profileType instanceof StateProfileType && isSupportedItemType(profileType, item);
                    return profileType instanceof TriggerProfileType tpt && isSupportedItemType(profileType, item)
                            && isSupportedChannelType(tpt, channel);
                    throw new IllegalArgumentException("Unknown channel kind: " + channel.getKind());
        }).map(profileType -> new ParameterOption(profileType.getUID().toString(), profileType.getLabel())).toList();
    private boolean isSupportedItemType(ProfileType profileType, Item item) {
        return profileType.getSupportedItemTypes().isEmpty()
                || profileType.getSupportedItemTypes().contains(item.getType());
    private boolean isSupportedChannelType(TriggerProfileType profileType, Channel channel) {
        return profileType.getSupportedChannelTypeUIDs().isEmpty()
                || profileType.getSupportedChannelTypeUIDs().contains(channel.getChannelTypeUID());
