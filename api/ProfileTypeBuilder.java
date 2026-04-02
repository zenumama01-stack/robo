import org.openhab.core.thing.internal.profiles.StateProfileTypeImpl;
import org.openhab.core.thing.internal.profiles.TriggerProfileTypeImpl;
 * Builder for {@link ProfileType} instances.
 * It can be used to obtain instances instead of implementing any of the interfaces derived from {@link ProfileType}.
 * @param <T> the concrete {@link ProfileType} sub-interface.
public final class ProfileTypeBuilder<@NonNull T extends ProfileType> {
    private interface ProfileTypeFactory<T extends ProfileType> {
        T create(ProfileTypeUID profileTypeUID, String label, Collection<String> supportedItemTypes,
                Collection<String> supportedItemTypesOfChannel, Collection<ChannelTypeUID> supportedChannelTypeUIDs);
    private final ProfileTypeFactory<T> profileTypeFactory;
    private final Collection<String> supportedItemTypes = new HashSet<>();
    private final Collection<String> supportedItemTypesOfChannel = new HashSet<>();
    private final Collection<ChannelTypeUID> supportedChannelTypeUIDs = new HashSet<>();
    private ProfileTypeBuilder(ProfileTypeUID profileTypeUID, String label, ProfileTypeFactory<T> profileTypeFactory) {
        this.profileTypeFactory = profileTypeFactory;
     * Obtain a new builder for a {@link StateProfileType} instance.
     * @param profileTypeUID the {@link ProfileTypeUID}
     * @param label a human-readable label
    public static ProfileTypeBuilder<StateProfileType> newState(ProfileTypeUID profileTypeUID, String label) {
        return new ProfileTypeBuilder<>(profileTypeUID, label,
                (leProfileTypeUID, leLabel, leSupportedItemTypes, leSupportedItemTypesOfChannel,
                        leSupportedChannelTypeUIDs) -> new StateProfileTypeImpl(leProfileTypeUID, leLabel,
                                leSupportedItemTypes, leSupportedItemTypesOfChannel));
     * Obtain a new builder for a {@link TriggerProfileType} instance.
    public static ProfileTypeBuilder<TriggerProfileType> newTrigger(ProfileTypeUID profileTypeUID, String label) {
                        leSupportedChannelTypeUIDs) -> new TriggerProfileTypeImpl(leProfileTypeUID, leLabel,
                                leSupportedItemTypes, leSupportedChannelTypeUIDs));
     * Declare that the given item type(s) are supported by a profile of this type.
     * @param itemType
     * @return the builder itself
    public ProfileTypeBuilder<T> withSupportedItemTypes(String... itemType) {
        supportedItemTypes.addAll(Arrays.asList(itemType));
     * @param itemTypes
    public ProfileTypeBuilder<T> withSupportedItemTypes(Collection<String> itemTypes) {
        supportedItemTypes.addAll(itemTypes);
     * Declare that the given channel type(s) are supported by a profile of this type.
     * @param channelTypeUIDs
    public ProfileTypeBuilder<T> withSupportedChannelTypeUIDs(ChannelTypeUID... channelTypeUIDs) {
        supportedChannelTypeUIDs.addAll(Arrays.asList(channelTypeUIDs));
    public ProfileTypeBuilder<T> withSupportedChannelTypeUIDs(Collection<ChannelTypeUID> channelTypeUIDs) {
        supportedChannelTypeUIDs.addAll(channelTypeUIDs);
     * Declare that channels with these item type(s) are compatible with profiles of this type.
     * @param supportedItemTypesOfChannel item types on channel to which this profile type is compatible with
    public ProfileTypeBuilder<T> withSupportedItemTypesOfChannel(String... supportedItemTypesOfChannel) {
        this.supportedItemTypesOfChannel.addAll(Arrays.asList(supportedItemTypesOfChannel));
    public ProfileTypeBuilder<T> withSupportedItemTypesOfChannel(Collection<String> supportedItemTypesOfChannel) {
        this.supportedItemTypesOfChannel.addAll(supportedItemTypesOfChannel);
     * Create a profile type instance with the previously given parameters.
     * @return the according subtype of {@link ProfileType}
    public T build() {
        return profileTypeFactory.create(profileTypeUID, label, supportedItemTypes, supportedItemTypesOfChannel,
                supportedChannelTypeUIDs);
