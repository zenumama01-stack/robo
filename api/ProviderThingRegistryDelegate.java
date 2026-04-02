import org.openhab.core.config.core.Configuration;
import org.openhab.core.thing.Channel;
import org.openhab.core.thing.ChannelUID;
import org.openhab.core.thing.Thing;
import org.openhab.core.thing.ThingTypeUID;
 * The {@link ProviderThingRegistryDelegate} is wrapping a {@link ThingRegistry} to provide a comfortable way to provide
 * Things from scripts without worrying about the need to remove Things again when the script is unloaded.
 * Nonetheless, using the {@link #addPermanent(Thing)} method it is still possible to add Things permanently.
public class ProviderThingRegistryDelegate implements ThingRegistry, ProviderRegistry {
    private final Set<ThingUID> things = new HashSet<>();
    private final ScriptedThingProvider scriptedProvider;
    public ProviderThingRegistryDelegate(ThingRegistry thingRegistry, ScriptedThingProvider scriptedProvider) {
    public void addRegistryChangeListener(RegistryChangeListener<Thing> listener) {
        thingRegistry.addRegistryChangeListener(listener);
    public Collection<Thing> getAll() {
        return thingRegistry.getAll();
    public Stream<Thing> stream() {
        return thingRegistry.stream();
    public void removeRegistryChangeListener(RegistryChangeListener<Thing> listener) {
        thingRegistry.removeRegistryChangeListener(listener);
    public Thing add(Thing element) {
        ThingUID thingUID = element.getUID();
        // Check for Thing already existing here because the Thing might exist in a different provider, so we need to
        if (get(thingUID) != null) {
                    "Cannot add Thing, because a Thing with same UID (" + thingUID + ") already exists.");
        things.add(thingUID);
     * Add a Thing permanently to the registry.
     * This Thing will be kept in the registry even if the script is unloaded.
     * @param element the Thing to be added (must not be null)
     * @return the added Thing
    public Thing addPermanent(Thing element) {
        return thingRegistry.add(element);
    public @Nullable Thing update(Thing element) {
        if (things.contains(element.getUID())) {
        return thingRegistry.update(element);
    public @Nullable Thing get(ThingUID uid) {
        return thingRegistry.get(uid);
    public @Nullable Channel getChannel(ChannelUID channelUID) {
        return thingRegistry.getChannel(channelUID);
    public void updateConfiguration(ThingUID thingUID, Map<String, Object> configurationParameters) {
        thingRegistry.updateConfiguration(thingUID, configurationParameters);
    public @Nullable Thing remove(ThingUID thingUID) {
        // Give the ThingHandler the chance to perform any removal operations instead of forcefully removing from
        // ScriptedThingProvider
        // If the Thing was provided by ScriptedThingProvider, it will be removed from there by listening to
        // ThingStatusEvent for ThingStatus.REMOVED in ScriptedThingProvider
        return thingRegistry.remove(thingUID);
        for (ThingUID thing : things) {
            scriptedProvider.remove(thing);
        things.clear();
    public @Nullable Thing forceRemove(ThingUID thingUID) {
        if (things.remove(thingUID)) {
            return scriptedProvider.remove(thingUID);
        return thingRegistry.forceRemove(thingUID);
    public @Nullable Thing createThingOfType(ThingTypeUID thingTypeUID, @Nullable ThingUID thingUID,
            @Nullable ThingUID bridgeUID, @Nullable String label, Configuration configuration) {
        return thingRegistry.createThingOfType(thingTypeUID, thingUID, bridgeUID, label, configuration);
