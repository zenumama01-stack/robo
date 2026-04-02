package org.openhab.core.automation.module.script.providersupport.shared;
import org.openhab.core.automation.module.script.providersupport.internal.ProviderRegistry;
import org.openhab.core.common.registry.Registry;
import org.openhab.core.common.registry.RegistryChangeListener;
import org.openhab.core.thing.ThingUID;
import org.openhab.core.thing.link.ItemChannelLink;
 * The {@link ProviderItemChannelLinkRegistry} is implementing a {@link Registry} to provide a comfortable way to
 * provide {@link ItemChannelLink}s from scripts without worrying about the need to remove them again when the script is
 * unloaded.
 * Nonetheless, using the {@link #addPermanent(ItemChannelLink)} method it is still possible to them permanently.
 * Use a new instance of this class for each {@link javax.script.ScriptEngine}.
 * ATTENTION: This class does not provide the same methods as {@link ItemChannelLinkRegistry}.
public class ProviderItemChannelLinkRegistry implements Registry<ItemChannelLink, String>, ProviderRegistry {
    private final Set<String> uids = new HashSet<>();
    private final ScriptedItemChannelLinkProvider scriptedProvider;
    public ProviderItemChannelLinkRegistry(ItemChannelLinkRegistry itemChannelLinkRegistry,
            ScriptedItemChannelLinkProvider scriptedProvider) {
        this.scriptedProvider = scriptedProvider;
    public void addRegistryChangeListener(RegistryChangeListener<ItemChannelLink> listener) {
        itemChannelLinkRegistry.addRegistryChangeListener(listener);
    public Collection<ItemChannelLink> getAll() {
        return itemChannelLinkRegistry.getAll();
    public Stream<ItemChannelLink> stream() {
        return itemChannelLinkRegistry.stream();
    public @Nullable ItemChannelLink get(String key) {
        return itemChannelLinkRegistry.get(key);
    public void removeRegistryChangeListener(RegistryChangeListener<ItemChannelLink> listener) {
        itemChannelLinkRegistry.removeRegistryChangeListener(listener);
    public ItemChannelLink add(ItemChannelLink element) {
        String uid = element.getUID();
        // Check for item->channel link already existing here because the item->channel link might exist in a different
        // provider, so we need to
        // check the registry and not only the provider itself
        if (get(uid) != null) {
                    "Cannot add item->channel link, because an item->channel link with same UID (" + uid
                            + ") already exists.");
        scriptedProvider.add(element);
        uids.add(uid);
        return element;
     * Adds an {@link ItemChannelLink} permanently to the registry.
     * This {@link ItemChannelLink} will be kept in the registry even if the script is unloaded
     * @param element the {@link ItemChannelLink} to be added (must not be null)
     * @return the added {@link ItemChannelLink}
    public ItemChannelLink addPermanent(ItemChannelLink element) {
        return itemChannelLinkRegistry.add(element);
    public @Nullable ItemChannelLink update(ItemChannelLink element) {
        if (uids.contains(element.getUID())) {
            return scriptedProvider.update(element);
        return itemChannelLinkRegistry.update(element);
    public @Nullable ItemChannelLink remove(String key) {
        if (uids.contains(key)) {
            return scriptedProvider.remove(key);
        return itemChannelLinkRegistry.remove(key);
    public int removeLinksForThing(ThingUID thingUID) {
        int removedLinks = 0;
        Collection<ItemChannelLink> itemChannelLinks = getAll();
        for (ItemChannelLink itemChannelLink : itemChannelLinks) {
            if (itemChannelLink.getLinkedUID().getThingUID().equals(thingUID)) {
                this.remove(itemChannelLink.getUID());
                removedLinks++;
        return removedLinks;
    public int removeLinksForItem(String itemName) {
            if (itemChannelLink.getItemName().equals(itemName)) {
    public int purge() {
        List<String> toRemove = itemChannelLinkRegistry.getOrphanLinks().keySet().stream().map(ItemChannelLink::getUID)
                .filter(i -> scriptedProvider.get(i) != null).toList();
        toRemove.forEach(this::remove);
        return toRemove.size() + itemChannelLinkRegistry.purge();
    public void removeAllAddedByScript() {
        for (String uid : uids) {
            scriptedProvider.remove(uid);
        uids.clear();
