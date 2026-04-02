import org.openhab.core.internal.types.CommandDescriptionImpl;
 * A {@link CommandDescription} provider from items' metadata
@Component(service = CommandDescriptionProvider.class)
public class MetadataCommandDescriptionProvider implements CommandDescriptionProvider {
    private final Logger logger = LoggerFactory.getLogger(MetadataCommandDescriptionProvider.class);
    public static final String COMMANDDESCRIPTION_METADATA_NAMESPACE = "commandDescription";
    private MetadataRegistry metadataRegistry;
    public MetadataCommandDescriptionProvider(final @Reference MetadataRegistry metadataRegistry,
            Map<String, Object> properties) {
        Metadata metadata = metadataRegistry.get(new MetadataKey(COMMANDDESCRIPTION_METADATA_NAMESPACE, itemName));
                CommandDescriptionImpl commandDescription = new CommandDescriptionImpl();
                Object options = metadata.getConfiguration().get("options");
                    Stream.of(options.toString().split(",")).forEach(o -> {
                        if (o.contains("=")) {
                            var pair = parseValueLabelPair(o.trim());
                            commandDescription.addCommandOption(new CommandOption(pair[0], pair[1]));
                            commandDescription.addCommandOption(new CommandOption(o.trim(), null));
                logger.warn("Unable to parse the commandDescription from metadata for item {}, ignoring it", itemName);
    public static String[] parseValueLabelPair(String text) {
        if (text.startsWith("\"") && text.contains("\"=\"") && text.endsWith("\"")) {
            String[] parts = text.split("\"=\"");
            value = parts[0].substring(1);
            label = parts[1].substring(0, parts[1].length() - 1);
            String[] parts = text.split("=");
            value = parts[0];
            label = parts[1];
        return new String[] { value.trim(), label.trim() };
