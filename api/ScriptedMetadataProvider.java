import org.openhab.core.items.ManagedMetadataProvider;
import org.openhab.core.items.MetadataProvider;
 * This {@link org.openhab.core.items.MetadataProvider} keeps metadata provided by scripts during runtime.
 * This ensures that metadata is not kept on reboot, but has to be provided by the scripts again.
@Component(immediate = true, service = { ScriptedMetadataProvider.class, MetadataProvider.class })
public class ScriptedMetadataProvider extends AbstractProvider<Metadata> implements ManagedMetadataProvider {
    private final Logger logger = LoggerFactory.getLogger(ScriptedMetadataProvider.class);
    private final Map<MetadataKey, Metadata> metadataStorage = new HashMap<>();
    public void removeItemMetadata(String name) {
        getAll().stream().filter(MetadataPredicates.ofItem(name)).map(Metadata::getUID).forEach(this::remove);
        return metadataStorage.values();
    public @Nullable Metadata get(MetadataKey metadataKey) {
        return metadataStorage.get(metadataKey);
    public void add(Metadata metadata) {
        if (metadataStorage.containsKey(metadata.getUID())) {
            throw new IllegalArgumentException("Cannot add metadata, because metadata with the same UID ("
                    + metadata.getUID() + ") already exists");
        metadataStorage.put(metadata.getUID(), metadata);
        notifyListenersAboutAddedElement(metadata);
    public @Nullable Metadata update(Metadata metadata) {
        Metadata oldMetadata = metadataStorage.get(metadata.getUID());
        if (oldMetadata != null) {
            notifyListenersAboutUpdatedElement(oldMetadata, metadata);
            logger.warn("Could not update metadata with UID '{}', because it does not exist", metadata.getUID());
        return oldMetadata;
    public @Nullable Metadata remove(MetadataKey metadataKey) {
        Metadata metadata = metadataStorage.remove(metadataKey);
        if (metadata != null) {
            notifyListenersAboutRemovedElement(metadata);
