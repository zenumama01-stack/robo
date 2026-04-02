import org.openhab.core.config.discovery.internal.PersistentInbox;
import org.openhab.core.io.console.ConsoleCommandCompleter;
import org.openhab.core.io.console.StringsCompleter;
 * This class provides console commands around the inbox functionality.
 * @author Laurent Garnier - New optional parameter for command approve
public class InboxConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String SUBCMD_APPROVE = "approve";
    private static final String SUBCMD_IGNORE = "ignore";
    private static final String SUBCMD_LIST = "list";
    private static final String SUBCMD_LIST_IGNORED = "listignored";
    private static final String SUBCMD_CLEAR = "clear";
    private static final String SUBCMD_REMOVE = "remove";
    private static final StringsCompleter SUBCMD_COMPLETER = new StringsCompleter(
            List.of(SUBCMD_APPROVE, SUBCMD_IGNORE, SUBCMD_LIST, SUBCMD_LIST_IGNORED, SUBCMD_CLEAR, SUBCMD_REMOVE),
    private class InboxConsoleCommandCompleter implements ConsoleCommandCompleter {
        public boolean complete(String[] args, int cursorArgumentIndex, int cursorPosition, List<String> candidates) {
            if (cursorArgumentIndex <= 0) {
                return SUBCMD_COMPLETER.complete(args, cursorArgumentIndex, cursorPosition, candidates);
            } else if (cursorArgumentIndex == 1) {
                if (SUBCMD_IGNORE.equalsIgnoreCase(args[0]) || SUBCMD_APPROVE.equalsIgnoreCase(args[0])) {
                    return new StringsCompleter(getThingUIDs(), true).complete(args, cursorArgumentIndex,
                            cursorPosition, candidates);
                } else if (SUBCMD_REMOVE.equalsIgnoreCase(args[0])) {
                    return new StringsCompleter(getThingAndThingTypeUIDs(), true).complete(args, cursorArgumentIndex,
        private Collection<String> getThingUIDs() {
            return inbox.stream().map(r -> r.getThingUID().getAsString()).toList();
        private Collection<String> getThingAndThingTypeUIDs() {
            return inbox.stream()
                    .flatMap(r -> Stream.of(r.getThingUID().getAsString(), r.getThingTypeUID().getAsString())).toList();
    public InboxConsoleCommandExtension(final @Reference Inbox inbox) {
        super("inbox", "Manage your inbox.");
            final String subCommand = args[0];
                case SUBCMD_APPROVE:
                        String label = args[2];
                        String newThingId = null;
                        if (args.length > 3) {
                            newThingId = args[3];
                            ThingUID thingUID = new ThingUID(args[1]);
                            List<DiscoveryResult> results = inbox.stream().filter(forThingUID(thingUID)).toList();
                                console.println("No matching inbox entry could be found.");
                            inbox.approve(thingUID, label, newThingId);
                                    String.format("An error occurred while approving '%s'", args[1])));
                        console.println("Specify thing id to approve: inbox approve <thingUID> <label> [<newThingID>]");
                case SUBCMD_IGNORE:
                            PersistentInbox persistentInbox = (PersistentInbox) inbox;
                            persistentInbox.setFlag(thingUID, DiscoveryResultFlag.IGNORED);
                            console.println("'" + args[1] + "' is no valid thing UID.");
                        console.println("Cannot approve thing as managed thing provider is missing.");
                case SUBCMD_LIST:
                    printInboxEntries(console, inbox.stream().filter(withFlag((DiscoveryResultFlag.NEW))).toList());
                case SUBCMD_LIST_IGNORED:
                    printInboxEntries(console, inbox.stream().filter(withFlag((DiscoveryResultFlag.IGNORED))).toList());
                case SUBCMD_CLEAR:
                    clearInboxEntries(console, inbox.getAll());
                case SUBCMD_REMOVE:
                        boolean validParam = true;
                                clearInboxEntries(console, results);
                            validParam = false;
                        if (!validParam) {
                                ThingTypeUID thingTypeUID = new ThingTypeUID(args[1]);
                                List<DiscoveryResult> results = inbox.stream().filter(forThingTypeUID(thingTypeUID))
                                console.println("'" + args[1] + "' is no valid thing UID or thing type.");
                                "Specify thing id or thing type to remove: inbox remove [<thingUID>|<thingTypeUID>]");
    private void printInboxEntries(Console console, List<DiscoveryResult> discoveryResults) {
        if (discoveryResults.isEmpty()) {
            console.println("No inbox entries found.");
        for (DiscoveryResult discoveryResult : discoveryResults) {
            ThingTypeUID thingTypeUID = discoveryResult.getThingTypeUID();
            String label = discoveryResult.getLabel();
            DiscoveryResultFlag flag = discoveryResult.getFlag();
            ThingUID bridgeId = discoveryResult.getBridgeUID();
            Map<String, Object> properties = discoveryResult.getProperties();
            String representationProperty = discoveryResult.getRepresentationProperty();
            String timestamp = discoveryResult.getCreationTime().toString();
            String timeToLive = discoveryResult.getTimeToLive() == DiscoveryResult.TTL_UNLIMITED ? "UNLIMITED"
                    : "" + discoveryResult.getTimeToLive();
            console.println(String.format(
                    "%s [%s]: %s [thingId=%s, bridgeId=%s, properties=%s, representationProperty=%s, timestamp=%s, timeToLive=%s]",
                    flag.name(), thingTypeUID, label, thingUID, bridgeId, properties, representationProperty, timestamp,
                    timeToLive));
    private void clearInboxEntries(Console console, List<DiscoveryResult> discoveryResults) {
            console.println(String.format("REMOVED [%s]: %s [label=%s, thingId=%s, bridgeId=%s, properties=%s]",
                    flag.name(), thingTypeUID, label, thingUID, bridgeId, properties));
            inbox.remove(thingUID);
        return List.of(buildCommandUsage(SUBCMD_LIST, "lists all current inbox entries"),
                buildCommandUsage(SUBCMD_LIST_IGNORED, "lists all ignored inbox entries"),
                buildCommandUsage(SUBCMD_APPROVE + " <thingUID> <label> [<newThingID>]",
                        "creates a thing for an inbox entry"),
                buildCommandUsage(SUBCMD_CLEAR, "clears all current inbox entries"),
                buildCommandUsage(SUBCMD_REMOVE + " [<thingUID>|<thingTypeUID>]",
                        "remove the inbox entries of a given thing id or thing type"),
                buildCommandUsage(SUBCMD_IGNORE + " <thingUID>", "ignores an inbox entry permanently"));
    public @Nullable ConsoleCommandCompleter getCompleter() {
        return new InboxConsoleCommandCompleter();
