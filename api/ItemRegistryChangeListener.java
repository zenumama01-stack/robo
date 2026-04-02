 * This is a listener interface which should be implemented where ever the item registry is
 * used in order to be notified of any dynamic changes in the provided items.
public interface ItemRegistryChangeListener extends RegistryChangeListener<Item> {
     * Notifies the listener that all items in the registry have changed and thus should be reloaded.
     * @param oldItemNames a collection of all previous item names, so that references can be removed
    void allItemsChanged(Collection<String> oldItemNames);
