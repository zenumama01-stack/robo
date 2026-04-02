import org.openhab.core.thing.binding.ThingTypeProvider;
 * The {@link ThingTypeRegistry} tracks all {@link ThingType}s provided by registered {@link ThingTypeProvider}s.
@Component(immediate = true, service = ThingTypeRegistry.class)
public class ThingTypeRegistry {
    private final List<ThingTypeProvider> thingTypeProviders = new CopyOnWriteArrayList<>();
    public ThingTypeRegistry(final @Reference ChannelTypeRegistry channelTypeRegistry) {
     * Returns all thing types.
     * @return all thing types
    public List<ThingType> getThingTypes(@Nullable Locale locale) {
        List<ThingType> thingTypes = new ArrayList<>();
        for (ThingTypeProvider thingTypeProvider : thingTypeProviders) {
            thingTypes.addAll(thingTypeProvider.getThingTypes(locale));
        return Collections.unmodifiableList(thingTypes);
    public List<ThingType> getThingTypes() {
        return getThingTypes((Locale) null);
     * Returns thing types for a given binding id.
     * @param bindingId binding id
     * @return thing types for given binding id
    public List<ThingType> getThingTypes(String bindingId, @Nullable Locale locale) {
        List<ThingType> thingTypesForBinding = new ArrayList<>();
        for (ThingType thingType : getThingTypes()) {
            if (thingType.getBindingId().equals(bindingId)) {
                thingTypesForBinding.add(thingType);
        return Collections.unmodifiableList(thingTypesForBinding);
    public List<ThingType> getThingTypes(String bindingId) {
        return getThingTypes(bindingId, null);
     * Returns a thing type for a given thing type UID.
     * @return thing type for given UID or null if no thing type with this UID
     *         was found
            ThingType thingType = thingTypeProvider.getThingType(thingTypeUID, locale);
                return thingType;
    public @Nullable ThingType getThingType(ThingTypeUID thingTypeUID) {
        return getThingType(thingTypeUID, null);
     * Returns the channel type for a given channel.
     * @return channel type or null if no channel type was found
    public @Nullable ChannelType getChannelType(Channel channel) {
        return getChannelType(channel, null);
     * Returns the channel type for a given channel and locale.
    public @Nullable ChannelType getChannelType(Channel channel, @Nullable Locale locale) {
            return channelTypeRegistry.getChannelType(channelTypeUID, locale);
    protected void addThingTypeProvider(ThingTypeProvider thingTypeProvider) {
        this.thingTypeProviders.add(thingTypeProvider);
    protected void removeThingTypeProvider(ThingTypeProvider thingTypeProvider) {
        this.thingTypeProviders.remove(thingTypeProvider);
