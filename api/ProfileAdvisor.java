 * Implementors can give advice which {@link Profile}s can/should be used for a given link.
public interface ProfileAdvisor {
     * Suggest a custom profile for the given channel (and potentially also the itemType).
     * Please note:
     * <li>This will override any default behavior
     * <li>A "profile" configuration on the link will override this suggestion
     * @param channel the linked channel
     * @param itemType the linked itemType (not applicable for trigger channels)
     * @return the profile identifier or {@code null} if this advisor does not have any advice
    ProfileTypeUID getSuggestedProfileTypeUID(Channel channel, @Nullable String itemType);
     * Suggest a custom profile for a given {@link ChannelType} (and potentially also the itemType).
     * @param channelType the {@link ChannelType} of the linked channel
    ProfileTypeUID getSuggestedProfileTypeUID(ChannelType channelType, @Nullable String itemType);
