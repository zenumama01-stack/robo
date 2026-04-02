 * {@link ManagedMetadataProviderImpl} is an OSGi service, that allows to add or remove
 * metadata for items at runtime. Persistence of added metadata is handled by
 * a {@link StorageService}.
@Component(immediate = true, service = { MetadataProvider.class, ManagedMetadataProvider.class })
public class ManagedMetadataProviderImpl extends AbstractManagedProvider<Metadata, MetadataKey, Metadata>
        implements ManagedMetadataProvider {
    private final Logger logger = LoggerFactory.getLogger(ManagedMetadataProviderImpl.class);
    public ManagedMetadataProviderImpl(final @Reference StorageService storageService) {
        return Metadata.class.getName();
    protected String keyToString(MetadataKey key) {
    protected @Nullable Metadata toElement(String key, Metadata persistableElement) {
        return persistableElement;
    protected Metadata toPersistableElement(Metadata element) {
     * Removes all metadata of a given item
     * @param name the name of the item for which the metadata is to be removed.
        logger.debug("Removing all metadata for item {}", name);
        return super.getAll().stream().map(this::normalizeMetadata).toList();
        Metadata metadata = super.get(key);
            return normalizeMetadata(metadata);
    private Metadata normalizeMetadata(Metadata metadata) {
        return new Metadata(metadata.getUID(), metadata.getValue(), metadata.getConfiguration().entrySet().stream()
                .map(this::normalizeConfigEntry).collect(Collectors.toMap(Map.Entry::getKey, Map.Entry::getValue)));
    private Map.Entry<String, Object> normalizeConfigEntry(Map.Entry<String, Object> entry) {
        if (value instanceof Integer) {
            BigDecimal newValue = new BigDecimal(value.toString());
            newValue.setScale(0);
            return Map.entry(entry.getKey(), newValue);
            return Map.entry(entry.getKey(), new BigDecimal(value.toString()));
