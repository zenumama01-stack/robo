 * Console command extension to send command to item
 * @author Stefan Bußweiler - Migration to new ESH event concept
public class SendConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String CONSOLE_SOURCE = "org.openhab.core.io.console";
    public SendConsoleCommandExtension(final @Reference ItemRegistry itemRegistry,
            final @Reference EventPublisher eventPublisher) {
        super("send", "Send a command to an item.");
        return List.of(buildCommandUsage("<item> <command>", "sends a command for an item"));
            String itemName = args[0];
                Item item = itemRegistry.getItemByPattern(itemName);
                    String commandName = args[1];
                    Command command = TypeParser.parseCommand(item.getAcceptedCommandTypes(), commandName);
                        eventPublisher.post(ItemEventFactory.createCommandEvent(itemName, command,
                                AbstractEvent.buildSource(CONSOLE_SOURCE, console.getUser())));
                        console.println("Command has been sent successfully.");
                                "Error: Command '" + commandName + "' is not valid for item '" + itemName + "'");
                        console.println("Valid command types are:");
                        for (Class<? extends Command> acceptedType : item.getAcceptedCommandTypes()) {
                            console.print("  " + acceptedType.getSimpleName());
                            if (acceptedType.isEnum()) {
                                console.print(": ");
                                for (Object e : Objects.requireNonNull(acceptedType.getEnumConstants())) {
                                    console.print(e + " ");
                            console.println("");
                console.println("Error: Item '" + itemName + "' does not exist.");
            } catch (ItemNotUniqueException e) {
                console.print("Error: Multiple items match this pattern: ");
                for (Item item : e.getMatchingItems()) {
                    console.print(item.getName() + " ");
        return new ItemConsoleCommandCompleter(itemRegistry,
                (Item i) -> i.getAcceptedCommandTypes().toArray(Class<?>[]::new));
