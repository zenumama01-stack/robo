import org.apache.karaf.shell.api.action.lifecycle.Manager;
import org.apache.karaf.shell.api.console.SessionFactory;
 * A simple container for all command extensions.
public class ConsoleSupportKaraf {
    private final Logger logger = LoggerFactory.getLogger(ConsoleSupportKaraf.class);
    private SessionFactory sessionFactory;
    // This collection contains all available / known commands.
    private final Collection<ConsoleCommandExtension> commands = new HashSet<>();
    // This map contains all registered commands.
    private final Map<ConsoleCommandExtension, CommandWrapper> registeredCommands = new HashMap<>();
    private Manager manager;
    public void setSessionFactory(SessionFactory sessionFactory) {
        this.sessionFactory = sessionFactory;
        manager = sessionFactory.getRegistry().getService(Manager.class);
        manager.register(CommandWrapper.class);
        registerCommands();
    public void unsetSessionFactory(SessionFactory sessionFactory) {
        if (this.sessionFactory == sessionFactory) {
            unregisterCommands();
            if (manager != null) {
                manager.unregister(CommandWrapper.class);
            this.sessionFactory = null;
    public void addConsoleCommandExtension(final ConsoleCommandExtension consoleCommandExtension) {
        commands.add(consoleCommandExtension);
        if (sessionFactory != null) {
            registerCommand(consoleCommandExtension);
    public void removeConsoleCommandExtension(final ConsoleCommandExtension consoleCommandExtension) {
        commands.remove(consoleCommandExtension);
            unregisterCommand(consoleCommandExtension);
     * Register all commands.
     * The session factory must be not null.
    private void registerCommands() {
        for (final ConsoleCommandExtension command : commands) {
            registerCommand(command);
     * Unregister all commands.
    private void unregisterCommands() {
            unregisterCommand(command);
     * Register a command.
     * @param command The command that should be registered.
    private void registerCommand(final ConsoleCommandExtension command) {
        final Registry registry = sessionFactory.getRegistry();
        final CommandWrapper wrapperOld;
        final CommandWrapper wrapperNew;
        wrapperNew = new CommandWrapper(command);
        wrapperOld = registeredCommands.put(command, wrapperNew);
        if (wrapperOld != null) {
            registry.unregister(wrapperOld);
            registry.register(wrapperNew);
        } catch (final Exception ex) {
            logger.error("Cannot register command '{}'.", command.getCommand(), ex);
     * Unregister a command.
     * @param command The command that should be unregistered.
    private void unregisterCommand(final ConsoleCommandExtension command) {
        wrapperOld = registeredCommands.get(command);
                logger.error("Cannot unregister command '{}'.", command.getCommand(), ex);
