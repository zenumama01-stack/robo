 * {@link LinkConsoleCommandExtension} provides console commands for listing,
 * adding and removing links.
 * @author Alex Tugarev - Added support for links between items and things
 * @author Kai Kreuzer - Removed Thing link commands
 * @author Jan N. Klug - Add orphan link handling
public class LinkConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String SUBCMD_LINK = "link";
    private static final String SUBCMD_UNLINK = "unlink";
    public LinkConsoleCommandExtension(@Reference ThingRegistry thingRegistry, @Reference ItemRegistry itemRegistry,
            @Reference ItemChannelLinkRegistry itemChannelLinkRegistry) {
        super("links", "Manage your links.");
                    list(console, itemChannelLinkRegistry.getAll());
                        orphan(console, args[1], itemChannelLinkRegistry.getAll(), thingRegistry.getAll(),
                                itemRegistry.getAll());
                case SUBCMD_LINK:
                        String itemName = args[1];
                        ChannelUID channelUID = new ChannelUID(args[2]);
                        addChannelLink(console, itemName, channelUID);
                        console.println("Specify item name and channel UID to link: " + SUBCMD_LINK
                                + " <itemName> <channelUID>");
                case SUBCMD_UNLINK:
                        removeChannelLink(console, itemName, channelUID);
                        console.println("Specify item name and channel UID to unlink: " + SUBCMD_UNLINK
                    clear(console);
    private void orphan(Console console, String action, Collection<ItemChannelLink> itemChannelLinks,
            Collection<Thing> things, Collection<Item> items) {
        Collection<ChannelUID> channelUIDS = things.stream().map(Thing::getChannels).flatMap(List::stream)
                .map(Channel::getUID).collect(Collectors.toCollection(HashSet::new));
        itemChannelLinks.forEach(itemChannelLink -> {
            if (!channelUIDS.contains(itemChannelLink.getLinkedUID())) {
                console.println("Thing channel missing: " + itemChannelLink.toString() + " "
                        + itemChannelLink.getConfiguration().toString());
                    removeChannelLink(console, itemChannelLink.getUID());
            } else if (!itemNames.contains(itemChannelLink.getItemName())) {
                console.println("Item missing: " + itemChannelLink.toString() + " "
        return List.of(buildCommandUsage(SUBCMD_LIST, "lists all links"),
                buildCommandUsage(SUBCMD_LINK + " <itemName> <channelUID>", "links an item with a channel"),
                buildCommandUsage(SUBCMD_UNLINK + " <itemName> <thingUID>", "unlinks an item with a channel"),
                buildCommandUsage(SUBCMD_CLEAR, "removes all managed links"),
                buildCommandUsage(SUBCMD_ORPHAN, "<list|purge> lists/purges all links with one missing element"));
    private void clear(Console console) {
        Collection<ItemChannelLink> itemChannelLinks = itemChannelLinkRegistry.getAll();
            itemChannelLinkRegistry.remove(itemChannelLink.getUID());
        console.println(itemChannelLinks.size() + " links successfully removed.");
    private void addChannelLink(Console console, String itemName, ChannelUID channelUID) {
        ItemChannelLink itemChannelLink = new ItemChannelLink(itemName, channelUID);
        itemChannelLinkRegistry.add(itemChannelLink);
        console.println("Link " + itemChannelLink.toString() + " successfully added.");
    private void list(Console console, Collection<ItemChannelLink> itemChannelLinks) {
            console.println(itemChannelLink.toString() + " " + itemChannelLink.getConfiguration().toString());
    private void removeChannelLink(Console console, String itemName, ChannelUID channelUID) {
        removeChannelLink(console, AbstractLink.getIDFor(itemName, channelUID));
    private void removeChannelLink(Console console, String linkId) {
        ItemChannelLink removedItemChannelLink = itemChannelLinkRegistry.remove(linkId);
        if (removedItemChannelLink != null) {
            console.println("Link " + linkId + " successfully removed.");
            console.println("Could not remove link " + linkId + ".");
