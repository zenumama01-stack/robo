 * Console command extension to send status update to item
public class UpdateConsoleCommandExtension extends AbstractConsoleCommandExtension {
    public UpdateConsoleCommandExtension(final @Reference ItemRegistry itemRegistry,
        super("update", "Send a state update to an item.");
        return List.of(buildCommandUsage("<item> <state>", "sends a status update for an item"));
                    String stateName = args[1];
                    State state = TypeParser.parseState(item.getAcceptedDataTypes(), stateName);
                        eventPublisher.post(ItemEventFactory.createStateEvent(item.getName(), state,
                        console.println("Update has been sent successfully.");
                        console.println("Error: State '" + stateName + "' is not valid for item '" + itemName + "'");
                        console.print("Valid data types are: ( ");
                        for (Class<? extends State> acceptedType : item.getAcceptedDataTypes()) {
                            console.print(acceptedType.getSimpleName() + " ");
                        console.println(")");
                (Item i) -> i.getAcceptedDataTypes().toArray(Class<?>[]::new));
