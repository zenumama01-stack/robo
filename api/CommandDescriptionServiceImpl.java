 * The {@link CommandDescriptionService} combines all available {@link CommandDescriptionProvider} implementations to
 * build a resulting {@link CommandDescription}.
public class CommandDescriptionServiceImpl implements CommandDescriptionService {
    private final List<CommandDescriptionProvider> commandDescriptionProviders = new CopyOnWriteArrayList<>();
         * As of now there is only the ChannelCommandDescriptionProvider, so there is no merge logic as for
         * {@link StateDescriptionFragment}s. Just take the first CommandDescription which was provided.
        for (CommandDescriptionProvider cdp : commandDescriptionProviders) {
            CommandDescription commandDescription = cdp.getCommandDescription(itemName, locale);
            if (commandDescription != null) {
    protected void addCommandDescriptionProvider(CommandDescriptionProvider commandDescriptionProvider) {
        commandDescriptionProviders.add(commandDescriptionProvider);
    protected void removeCommandDescriptionProvider(CommandDescriptionProvider commandDescriptionProvider) {
        commandDescriptionProviders.remove(commandDescriptionProvider);
