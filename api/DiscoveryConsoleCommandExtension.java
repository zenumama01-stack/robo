package org.openhab.core.config.discovery.internal.console;
 * {@link DiscoveryConsoleCommandExtension} provides console commands for thing discovery.
 * @author Dennis Nobel - Added background discovery commands
 * @author Laurent Garnier - Updated command to start discovery with a new optional input parameter
@Component(immediate = true, service = ConsoleCommandExtension.class)
public class DiscoveryConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String SUBCMD_START = "start";
    private static final String SUBCMD_BACKGROUND_DISCOVERY_ENABLE = "enableBackgroundDiscovery";
    private static final String SUBCMD_BACKGROUND_DISCOVERY_DISABLE = "disableBackgroundDiscovery";
    private final Logger logger = LoggerFactory.getLogger(DiscoveryConsoleCommandExtension.class);
    public DiscoveryConsoleCommandExtension(final @Reference DiscoveryServiceRegistry discoveryServiceRegistry,
            final @Reference ConfigurationAdmin configurationAdmin) {
        super("discovery", "Control the discovery mechanism.");
                case SUBCMD_START:
                        String arg1 = args[1];
                        if (arg1.contains(":")) {
                            ThingTypeUID thingTypeUID = new ThingTypeUID(arg1);
                            runDiscoveryForThingType(console, thingTypeUID, args.length > 2 ? args[2] : null);
                            runDiscoveryForBinding(console, arg1, args.length > 2 ? args[2] : null);
                        console.println("Specify thing type id or binding id to discover: discovery "
                                + "start <thingTypeUID|bindingID> (e.g. \"hue:bridge\" or \"hue\")");
                case SUBCMD_BACKGROUND_DISCOVERY_ENABLE:
                        String discoveryServiceName = args[1];
                        configureBackgroundDiscovery(console, discoveryServiceName, true);
                        console.println("Specify discovery service PID to configure background discovery: discovery "
                                + "enableBackgroundDiscovery <PID> (e.g. \"hue.discovery\")");
                case SUBCMD_BACKGROUND_DISCOVERY_DISABLE:
                        configureBackgroundDiscovery(console, discoveryServiceName, false);
                                + "disableBackgroundDiscovery <PID> (e.g. \"hue.discovery\")");
            console.println(getUsages().getFirst());
    private void configureBackgroundDiscovery(Console console, String discoveryServicePID, boolean enabled) {
            Configuration configuration = configurationAdmin.getConfiguration(discoveryServicePID);
            properties.put(DiscoveryService.CONFIG_PROPERTY_BACKGROUND_DISCOVERY, enabled);
            console.println("Background discovery for discovery service '" + discoveryServicePID + "' was set to "
                    + enabled + ".");
            String errorText = "Error occurred while trying to configure background discovery with PID '"
                    + discoveryServicePID + "': " + ex.getMessage();
            logger.error(errorText, ex);
            console.println(errorText);
    private void runDiscoveryForThingType(Console console, ThingTypeUID thingTypeUID, @Nullable String input) {
        discoveryServiceRegistry.startScan(thingTypeUID, input, null);
    private void runDiscoveryForBinding(Console console, String bindingId, @Nullable String input) {
        discoveryServiceRegistry.startScan(bindingId, input, null);
                buildCommandUsage(SUBCMD_START + " <thingTypeUID|bindingID> [<code>]",
                        "runs a discovery on a given thing type or binding"),
                buildCommandUsage(SUBCMD_BACKGROUND_DISCOVERY_ENABLE + " <PID>",
                        "enables background discovery for the discovery service with the given PID"),
                buildCommandUsage(SUBCMD_BACKGROUND_DISCOVERY_DISABLE + " <PID>",
                        "disables background discovery for the discovery service with the given PID"));
