import org.openhab.core.io.console.rfc147.internal.extension.HelpConsoleCommandExtension;
 * Implementation of the console support using the OSGi interface to support commands proposal RFC 147.
 * Read this link to get a short overview about the way to implement commands for RFC 147:
 * https://felix.apache.org/site/rfc-147-overview.html
@Component(immediate = true, service = {})
public class ConsoleSupportRfc147 implements ConsoleCommandsContainer {
    // private static final String KEY_SCOPE = CommandProcessor.COMMAND_SCOPE;
    // private static final String KEY_FUNCTION = CommandProcessor.COMMAND_FUNCTION;
    private static final String KEY_SCOPE = "osgi.command.scope";
    private static final String KEY_FUNCTION = "osgi.command.function";
    private static final String SCOPE = "openhab";
    public static final OSGiConsole CONSOLE = new OSGiConsole(SCOPE);
    private final Logger logger = LoggerFactory.getLogger(ConsoleSupportRfc147.class);
    private final HelpConsoleCommandExtension helpCommand = new HelpConsoleCommandExtension();
    private @Nullable BundleContext bundleContext;
     * This map will contain all console command extensions.
     * The key consists of the reference to the console command extensions.
     * The value is set to null, if the console command extension is not registered, yet (e.g. the bundle context is not
     * known). Otherwise it stores the registered service reference, so we could unregister the command extension later.
    private final Map<ConsoleCommandExtension, @Nullable ServiceRegistration<?>> commands = Collections
            .synchronizedMap(new HashMap<>());
     * Constructs a new RFC 147 console support instance.
     * Initializes the commands map and registers the built-in help command.
    public ConsoleSupportRfc147() {
        // Add our custom help console command extensions.
        commands.put(helpCommand, null);
     * Activates this OSGi component.
     * Registers all pending console command extensions as OSGi services and activates the help command.
     * @param ctx the component context provided by OSGi
    public void activate(ComponentContext ctx) {
        // Save bundle context to register services.
        this.bundleContext = ctx.getBundleContext();
         * The bundle context is available.
         * Register all console command extensions that are not registered before.
        for (Map.Entry<ConsoleCommandExtension, @Nullable ServiceRegistration<?>> entry : commands.entrySet()) {
            if (entry.getValue() == null) {
                entry.setValue(registerCommand(entry.getKey()));
        // We are activated now, so the help command should be able to fetch all our commands.
        helpCommand.setConsoleCommandsContainer(this);
     * Deactivates this OSGi component.
     * Unregisters all console command extensions and clears the help command reference.
        // If we get deactivated, remove from help command (so GC could do their work).
        helpCommand.setConsoleCommandsContainer(null);
         * De-register all previously registered command extensions.
            ServiceRegistration<?> value = entry.getValue();
                unregisterCommand(value);
        // Remove bundle context reference.
        this.bundleContext = null;
     * The command is immediately registered as an OSGi service if the component is active.
        final ServiceRegistration<?> old;
        old = commands.put(consoleCommandExtension, registerCommand(consoleCommandExtension));
        if (old != null) {
            unregisterCommand(old);
     * The command's OSGi service registration is also removed.
        old = commands.remove(consoleCommandExtension);
     * Creates an empty properties dictionary for OSGi service registration.
     * @return an empty dictionary that can be used to store service properties
    private Dictionary<String, Object> createProperties() {
        return (Dictionary) new Properties();
     * Register a console command extension.
     * @param cmd the console command extension that should be registered.
     * @return the service registration reference on success, null if the registration was not successful.
    private @Nullable ServiceRegistration<?> registerCommand(final ConsoleCommandExtension cmd) {
        // We could only register the service if the bundle context is known.
        BundleContext bundleContext = this.bundleContext;
        if (bundleContext == null) {
        Dictionary<String, Object> props = createProperties();
        props.put(KEY_SCOPE, SCOPE);
        props.put(KEY_FUNCTION, cmd.getCommand());
            final ServiceRegistration<?> serviceRegistration;
            serviceRegistration = bundleContext.registerService(CommandWrapper.class.getName(), new CommandWrapper(cmd),
                    props);
            return serviceRegistration;
        } catch (final IllegalStateException ex) {
            logger.trace("Registration failed.");
     * Unregister a service registration.
     * @param serviceRegistration the service registration for the service that should be unregistered.
    private void unregisterCommand(final ServiceRegistration<?> serviceRegistration) {
            serviceRegistration.unregister();
            logger.trace("Service already unregistered.");
    public Collection<ConsoleCommandExtension> getConsoleCommandExtensions() {
        final Set<ConsoleCommandExtension> set = new HashSet<>();
        // Fill set with registered commands only.
            if (entry.getValue() != null) {
                set.add(entry.getKey());
        return set;
