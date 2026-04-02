 * Console command extension to show the current state of an item
public class StatusConsoleCommandExtension extends AbstractConsoleCommandExtension {
    public StatusConsoleCommandExtension(final @Reference ItemRegistry itemRegistry) {
        super("status", "Get the current status of an item.");
        return List.of(buildCommandUsage("<item>", "shows the current status of an item"));
                console.println(item.getState().toString());
        return new ItemConsoleCommandCompleter(itemRegistry);
