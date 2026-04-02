import org.openhab.core.items.GenericItem;
import org.openhab.core.items.ManagedItemProvider;
 * Console command extension to get item list
 * @author Markus Rathgeb - Create DS for command extension
 * @author Dennis Nobel - Changed service references to be injected via DS
 * @author Simon Kaufmann - Added commands to clear and remove items
 * @author Stefan Triller - Added commands for adding and removing tags
public class ItemConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String SUBCMD_ADDTAG = "addTag";
    private static final String SUBCMD_RMTAG = "rmTag";
            List.of(SUBCMD_LIST, SUBCMD_CLEAR, SUBCMD_REMOVE, SUBCMD_ADDTAG, SUBCMD_RMTAG), false);
    private class ItemConsoleCommandCompleter implements ConsoleCommandCompleter {
            if (cursorArgumentIndex == 1) {
                Collection<Item> items;
                switch (args[0]) {
                    case SUBCMD_ADDTAG:
                    case SUBCMD_RMTAG:
                        items = managedItemProvider.getAll();
                        items = itemRegistry.getAll();
                return new StringsCompleter(items.stream().map(Item::getName).toList(), true).complete(args,
            if (cursorArgumentIndex == 2 && args[0].equals(SUBCMD_RMTAG)) {
                Item item = managedItemProvider.get(args[1]);
                return new StringsCompleter(item.getTags(), true).complete(args, cursorArgumentIndex, cursorPosition,
                        candidates);
    private final ManagedItemProvider managedItemProvider;
    public ItemConsoleCommandExtension(final @Reference ItemRegistry itemRegistry,
            final @Reference ManagedItemProvider managedItemProvider) {
        super("items", "Access the item registry.");
        this.managedItemProvider = managedItemProvider;
                buildCommandUsage(SUBCMD_LIST + " [<pattern>]",
                        "lists names and types of all items (matching the pattern, if given)"),
                buildCommandUsage(SUBCMD_CLEAR, "removes all items"),
                buildCommandUsage(SUBCMD_REMOVE + " <itemName>", "removes the given item"),
                buildCommandUsage(SUBCMD_ADDTAG + " <itemName> <tag>", "adds a tag to the given item"),
                buildCommandUsage(SUBCMD_RMTAG + " <itemName> <tag>", "removes a tag from the given item"));
                    listItems(console, (args.length < 2) ? "*" : args[1]);
                    removeItems(console, itemRegistry.getAll());
                        Item item = itemRegistry.get(args[1]);
                            removeItems(console, Set.of(item));
                            console.println("0 item(s) removed.");
                        console.println("Specify the name of the item to remove: " + getCommand() + " " + SUBCMD_REMOVE
                                + " <itemName>");
                        if (item instanceof GenericItem gItem) {
                            handleTags(gItem::addTag, args[2], gItem, console);
                        console.println("Specify the name of the item and the tag: " + getCommand() + " "
                                + SUBCMD_ADDTAG + " <itemName> <tag>");
                            handleTags(gItem::removeTag, args[2], gItem, console);
                        console.println("Specify the name of the item and the tag: " + getCommand() + " " + SUBCMD_RMTAG
                                + " <itemName> <tag>");
        return new ItemConsoleCommandCompleter();
    private <T> void handleTags(final Consumer<T> func, final T tag, GenericItem gItem, Console console) {
        // allow adding/removing of tags only for managed items
        if (managedItemProvider.get(gItem.getName()) != null) {
            // add or remove tag method is passed here
            func.accept(tag);
            Item oldItem = itemRegistry.update(gItem);
                console.println("Successfully changed tag " + tag + " on item " + gItem.getName());
            console.println("Error: Cannot change tag " + tag + " on item " + gItem.getName()
                    + " because this item does not belong to a ManagedProvider");
    private void removeItems(Console console, Collection<Item> items) {
        int count = items.size();
            itemRegistry.remove(item.getName());
        console.println(count + " item(s) removed successfully.");
    private void listItems(Console console, String pattern) {
        Collection<Item> items = itemRegistry.getItems(pattern);
        if (!items.isEmpty()) {
                console.println(item.toString());
            if (pattern.isEmpty()) {
                console.println("No item found.");
                console.println("No item found for this pattern.");
