import org.openhab.core.items.Metadata;
import org.openhab.core.items.MetadataKey;
import org.openhab.core.items.MetadataPredicates;
 * The {@link ProviderMetadataRegistryDelegate} is wrapping a {@link MetadataRegistry} to provide a comfortable way to
 * provide items from scripts without worrying about the need to remove metadata again when the script is unloaded.
 * Nonetheless, using the {@link #addPermanent(Metadata)} method it is still possible to add metadata permanently.
public class ProviderMetadataRegistryDelegate implements MetadataRegistry, ProviderRegistry {
    private final Set<MetadataKey> metadataKeys = new HashSet<>();
    private final ScriptedMetadataProvider scriptedProvider;
    public ProviderMetadataRegistryDelegate(MetadataRegistry metadataRegistry,
            ScriptedMetadataProvider scriptedProvider) {
    public void addRegistryChangeListener(RegistryChangeListener<Metadata> listener) {
        metadataRegistry.addRegistryChangeListener(listener);
    public Collection<Metadata> getAll() {
        return metadataRegistry.getAll();
    public Stream<Metadata> stream() {
        return metadataRegistry.stream();
    public @Nullable Metadata get(MetadataKey key) {
        return metadataRegistry.get(key);
    public void removeRegistryChangeListener(RegistryChangeListener<Metadata> listener) {
        metadataRegistry.removeRegistryChangeListener(listener);
    public Metadata add(Metadata element) {
        MetadataKey key = element.getUID();
        // Check for metadata already existing here because the metadata might exist in a different provider, so we need
        // to check the registry and not only the provider itself
        if (get(key) != null) {
                    "Cannot add metadata, because metadata with same name (" + key + ") already exists.");
        metadataKeys.add(key);
     * Adds metadata permanently to the registry.
     * This metadata will be kept in the registry even if the script is unloaded.
     * @param element the metadata to be added (must not be null)
     * @return the added metadata
    public Metadata addPermanent(Metadata element) {
        return metadataRegistry.add(element);
    public @Nullable Metadata update(Metadata element) {
        if (metadataKeys.contains(element.getUID())) {
        return metadataRegistry.update(element);
    public @Nullable Metadata remove(MetadataKey key) {
        if (metadataKeys.contains(key)) {
        return metadataRegistry.remove(key);
    public boolean isInternalNamespace(String namespace) {
        return metadataRegistry.isInternalNamespace(namespace);
    public Collection<String> getAllNamespaces(String itemname) {
        return metadataRegistry.getAllNamespaces(itemname);
    public void removeItemMetadata(String itemname) {
        if (scriptedProvider.getAll().stream().anyMatch(MetadataPredicates.ofItem(itemname))) {
            scriptedProvider.removeItemMetadata(itemname);
        metadataRegistry.removeItemMetadata(itemname);
        for (MetadataKey key : metadataKeys) {
            scriptedProvider.remove(key);
        metadataKeys.clear();
