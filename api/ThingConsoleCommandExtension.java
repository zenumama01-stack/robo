 * {@link ThingConsoleCommandExtension} provides console commands for listing and removing things.
 * @author Thomas Höfer - Added localization of thing status
 * @author Stefan Triller - Added trigger channel command
 * @author Henning Sudbrock - Added show command
public class ThingConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String CMD_THINGS = "things";
    private static final String SUBCMD_SHOW = "show";
    private static final String SUBCMD_TRIGGER = "trigger";
    private static final String SUBCMD_DISABLE = "disable";
    private static final String SUBCMD_ENABLE = "enable";
    public ThingConsoleCommandExtension(final @Reference ManagedThingProvider managedThingProvider,
            final @Reference EventPublisher eventPublisher, final @Reference ThingManager thingManager) {
        super(CMD_THINGS, "Access your thing registry.");
                    printThings(console, things);
                case SUBCMD_SHOW:
                    printThingsDetails(console, List.of(args).subList(1, args.length));
                    removeAllThings(console, things);
                        removeThing(console, thingUID);
                        console.println("Specify thing id to remove: things remove <thingUID> (e.g. \"hue:light:1\")");
                case SUBCMD_TRIGGER:
                        triggerChannel(console, args[1], args[2]);
                    } else if (args.length == 2) {
                        triggerChannel(console, args[1], "");
                        console.println("Command '" + subCommand + "' needs arguments <channelUID> [<event>]");
                case SUBCMD_DISABLE:
                case SUBCMD_ENABLE:
                        enableThing(console, thingUID, SUBCMD_ENABLE.equals(subCommand));
                                "Command '" + subCommand + "' needs argument <thingUID> (e.g. \"hue:light:1\")");
    private void triggerChannel(Console console, String channelUid, String event) {
        eventPublisher.post(ThingEventFactory.createTriggerEvent(event, new ChannelUID(channelUid)));
    private void removeThing(Console console, ThingUID thingUID) {
        Thing removedThing = this.managedThingProvider.remove(thingUID);
        if (removedThing != null) {
            console.println("Thing '" + thingUID + "' successfully removed.");
            console.println("Could not delete thing " + thingUID + ".");
    private void removeAllThings(Console console, Collection<Thing> things) {
        int numberOfThings = things.size();
            managedThingProvider.remove(thing.getUID());
        console.println(numberOfThings + " things successfully removed.");
    private void enableThing(Console console, ThingUID thingUID, boolean isEnabled) {
        if (thingRegistry.get(thingUID) == null) {
            console.println("unknown thing for thingUID '" + thingUID.getAsString() + "'.");
        thingManager.setEnabled(thingUID, isEnabled);
        String command = isEnabled ? "enabled" : "disabled";
        console.println(thingUID.getAsString() + " successfully " + command + ".");
        return List.of(buildCommandUsage(SUBCMD_LIST, "lists all things"),
                buildCommandUsage(SUBCMD_SHOW + " <thingUID>*",
                        "show details about one or more things; show details for all things if no thingUID provided"),
                buildCommandUsage(SUBCMD_CLEAR, "removes all managed things"),
                buildCommandUsage(SUBCMD_REMOVE + " <thingUID>", "removes a thing"),
                buildCommandUsage(SUBCMD_TRIGGER + " <channelUID> [<event>]",
                        "triggers the <channelUID> with <event> (if given)"),
                buildCommandUsage(SUBCMD_DISABLE + " <thingUID>", "disables a thing"),
                buildCommandUsage(SUBCMD_ENABLE + " <thingUID>", "enables a thing"));
    private void printThings(Console console, Collection<Thing> things) {
            console.println("No things found.");
            String id = thing.getUID().toString();
            String thingType = thing instanceof Bridge ? "Bridge" : "Thing";
            ThingStatusInfo status = thingStatusInfoI18nLocalizationService.getLocalizedThingStatusInfo(thing, null);
            String label = thing.getLabel();
            console.println(String.format("%s (Type=%s, Status=%s, Label=%s, Bridge=%s)", id, thingType, status, label,
                    bridgeUID));
    private void printThingsDetails(Console console, List<String> thingUIDStrings) {
        Collection<Thing> things;
        if (thingUIDStrings.isEmpty()) {
            things = thingRegistry.getAll();
            things = new ArrayList<>();
            for (String thingUIDString : thingUIDStrings) {
                    thingUID = new ThingUID(thingUIDString);
                    console.println("This is not a valid thing UID: " + thingUIDString);
                    console.println("Could not find thing with UID " + thingUID);
        printThingsDetails(console, things);
    private void printThingsDetails(Console console, Collection<Thing> things) {
        for (Iterator<Thing> iter = things.iterator(); iter.hasNext();) {
            printThingDetails(console, iter.next());
            if (iter.hasNext()) {
                console.println("--- --- --- --- ---");
    private void printThingDetails(Console console, Thing thing) {
        console.println("UID: " + thing.getUID());
        console.println("Type: " + thing.getThingTypeUID());
        console.println("Label: " + thing.getLabel());
        console.println("Status: " + thingStatusInfoI18nLocalizationService.getLocalizedThingStatusInfo(thing, null));
            console.println("Bridge: " + thing.getBridgeUID());
        if (thing.getProperties().isEmpty()) {
            console.println("No properties");
            console.println("Properties:");
            for (Map.Entry<String, String> entry : thing.getProperties().entrySet()) {
                console.println("\t" + entry.getKey() + " : " + entry.getValue());
        if (thing.getConfiguration().getProperties().isEmpty()) {
            console.println("No configuration parameters");
            console.println("Configuration parameters:");
            for (Map.Entry<String, Object> entry : thing.getConfiguration().getProperties().entrySet()) {
        if (thing.getChannels().isEmpty()) {
            console.println("No channels");
            console.println("Channels:");
            for (Iterator<Channel> iter = thing.getChannels().iterator(); iter.hasNext();) {
                Channel channel = iter.next();
                console.println("\tID: " + channel.getUID().getId());
                console.println("\tLabel: " + channel.getLabel());
                console.println("\tType: " + channel.getChannelTypeUID());
                if (channel.getDescription() != null) {
                    console.println("\tDescription: " + channel.getDescription());
