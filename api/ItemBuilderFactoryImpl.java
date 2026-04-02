import org.openhab.core.items.ItemBuilder;
 * Provides an {@link ItemBuilder} with all available {@link ItemFactory}s set.
 * The {@link org.openhab.core.library.CoreItemFactory} will always be present.
public class ItemBuilderFactoryImpl implements ItemBuilderFactory {
    public ItemBuilderFactoryImpl(
            final @Reference(target = "(component.name=org.openhab.core.library.CoreItemFactory)") ItemFactory coreItemFactory) {
        itemFactories.add(coreItemFactory);
    public ItemBuilder newItemBuilder(Item item) {
        return new ItemBuilderImpl(itemFactories, item);
    public ItemBuilder newItemBuilder(String itemType, String itemName) {
        return new ItemBuilderImpl(itemFactories, itemType, itemName);
