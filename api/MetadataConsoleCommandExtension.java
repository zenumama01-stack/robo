 * Console command extension for the {@link MetadataRegistry}.
 * @author Jan N. Klug - Added removal of orphaned metadata
public class MetadataConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String SUBCMD_LIST_INTERNAL = "listinternal";
    private static final String SUBCMD_ADD = "add";
    private static final String SUBCMD_ORPHAN = "orphan";
    public MetadataConsoleCommandExtension(final @Reference ItemRegistry itemRegistry,
            final @Reference MetadataRegistry metadataRegistry) {
        super("metadata", "Access the metadata registry.");
        return List.of( //
                buildCommandUsage(SUBCMD_LIST + " [<itemName> [<namespace>]]",
                        "lists all available metadata, can be filtered for a specifc item and namespace"),
                buildCommandUsage(SUBCMD_LIST_INTERNAL + " [<itemName> [<namespace>]]",
                        "lists all available INTERNAL metadata, can be filtered for a specifc item and namespace"),
                buildCommandUsage(SUBCMD_REMOVE + " <itemName> [<namespace>]",
                        "removes metadata for the specific item (for all namespaces or for the given namespace only)"),
                buildCommandUsage(SUBCMD_ADD + " <itemName> <namespace> <value> [\"{key1=value1, key2=value2, ...}\"]",
                        "adds or updates metadata value (and optional config values) for the specific item in the given namespace"),
                buildCommandUsage(SUBCMD_ORPHAN + " list|purge",
                        "lists or removes all metadata for which no corresponding item is present"));
                    listMetadata(console, args.length > 1 ? args[1] : null, args.length > 2 ? args[2] : null, false);
                case SUBCMD_LIST_INTERNAL:
                    listMetadata(console, args.length > 1 ? args[1] : null, args.length > 2 ? args[2] : null, true);
                case SUBCMD_ADD:
                    if (args.length < 4) {
                        addMetadata(console, args[1], args[2], args[3], args.length > 4 ? args[4] : null);
                case SUBCMD_ORPHAN:
                    if (args.length == 2 && ("list".equals(args[1]) || "purge".equals(args[1]))) {
                        orphan(console, args[1], metadataRegistry.getAll(), itemRegistry.getAll());
                        console.println("Specify action 'list' or 'purge' to be executed: orphan <list|purge>");
                    removeMetadata(console, args[1], args.length > 2 ? args[2] : null);
    private void listMetadata(Console console, @Nullable String itemName, @Nullable String namespace,
            boolean internal) {
        if (itemName == null) {
            metadataRegistry.stream().filter(m -> isInternal(m, internal)).map(Metadata::toString)
                    .forEach(console::println);
        } else if (namespace == null) {
            metadataRegistry.stream().filter(MetadataPredicates.ofItem(itemName)).filter(m -> isInternal(m, internal))
                    .map(Metadata::toString).forEach(console::println);
            MetadataKey key = new MetadataKey(namespace, itemName);
            if (metadataRegistry.isInternalNamespace(namespace) == internal) {
                Metadata metadata = metadataRegistry.get(key);
                    console.println(metadata.toString());
    private boolean isInternal(Metadata metadata, boolean internal) {
        return metadataRegistry.isInternalNamespace(metadata.getUID().getNamespace()) == internal;
    private void addMetadata(Console console, String itemName, String namespace, String value,
            @Nullable String config) {
            console.println("Item " + itemName + " does not exist.");
            Map<String, Object> configMap = getConfigMap(config);
            Metadata metadata = new Metadata(key, value, configMap);
                if (metadataRegistry.get(key) == null) {
                    metadataRegistry.add(metadata);
                    console.println("Added: " + metadata);
                    if (metadataRegistry.update(metadata) == null) {
                        console.println("Cannot update metadata in unmanaged provider: " + metadata);
                        console.println("Updated: " + metadata);
            } catch (UnsupportedOperationException e) {
                console.println("Namespace reserved in unmanaged provider: " + metadata);
                console.println("No managed provider available for: " + metadata);
    private @Nullable Map<String, Object> getConfigMap(@Nullable String config) {
        if (config == null) {
        String configStr = config;
        if (configStr.startsWith("{") && configStr.endsWith("}")) {
            configStr = configStr.substring(1, configStr.length() - 1);
        Map<String, Object> map = new HashMap<>();
        for (String part : configStr.split("\\s*,\\s*")) {
            String[] subparts = part.split("=", 2);
            if (subparts.length == 2) {
                map.put(subparts[0].trim(), subparts[1].trim());
    private void removeMetadata(Console console, String itemName, @Nullable String namespace) {
            console.println("Warning: Item " + itemName + " does not exist, removing metadata anyway.");
        if (namespace == null) {
            metadataRegistry.stream().filter(MetadataPredicates.ofItem(itemName)).map(Metadata::getUID)
                    .forEach(key -> removeMetadata(console, key));
            removeMetadata(console, key);
    private void removeMetadata(Console console, MetadataKey key) {
            if (metadataRegistry.get(key) != null) {
                Metadata removedMetadata = metadataRegistry.remove(key);
                if (removedMetadata != null) {
                    console.println("Removed: " + removedMetadata);
                        console.println("Unmanaged metadata element for " + key + ", could not be removed.");
                        console.println("Metadata element for " + key + " could not be found.");
            console.println("Unmanaged metadata element for " + key + " in reserved namespace, could not be removed.");
            console.println("No managed provider available for metadata with key: " + key);
    private void orphan(Console console, String action, Collection<Metadata> metadata, Collection<Item> items) {
        Collection<String> itemNames = items.stream().map(Item::getName).collect(Collectors.toCollection(HashSet::new));
        metadata.forEach(md -> {
            if (!itemNames.contains(md.getUID().getItemName())) {
                console.println("Item missing: " + md.getUID());
                if ("purge".equals(action)) {
                        metadataRegistry.remove(md.getUID());
                    } catch (UnsupportedOperationException | IllegalStateException e) {
                        // ignore metadata that cannot be removed
