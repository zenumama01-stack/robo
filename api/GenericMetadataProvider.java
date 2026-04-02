import org.eclipse.emf.codegen.ecore.templates.edit.ItemProvider;
 * This class serves as a provider for all metadata that is found within item files.
 * It is filled with content by the {@link GenericItemProvider}, which cannot itself implement the
 * {@link MetadataProvider} interface as it already implements {@link ItemProvider}, which would lead to duplicate
 * methods.
 * @author Laurent Garnier - Store metadata per model + do not notify the registry for isolated models
@Component(service = { MetadataProvider.class, GenericMetadataProvider.class })
public class GenericMetadataProvider extends AbstractProvider<Metadata> implements MetadataProvider {
    private final Map<String, Set<Metadata>> metadata = new HashMap<>();
    private final ReadWriteLock lock = new ReentrantReadWriteLock(true);
     * Adds metadata to this provider
     * @param bindingType
     * @param itemName
     * @param configuration
    public void addMetadata(String modelName, String bindingType, String itemName, String value,
            @Nullable Map<String, Object> configuration) {
        MetadataKey key = new MetadataKey(bindingType, itemName);
        Metadata md = new Metadata(key, value, configuration);
            Set<Metadata> mdSet = Objects.requireNonNull(metadata.computeIfAbsent(modelName, k -> new HashSet<>()));
            mdSet.add(md);
            notifyListenersAboutAddedElement(md);
     * Removes all meta-data for a given namespace
     * @param namespace the namespace
    public void removeMetadataByNamespace(String namespace) {
        Map<String, Set<Metadata>> toBeNotified;
            toBeNotified = new HashMap<>();
            Set<String> modelsToRemove = new HashSet<>();
            for (Map.Entry<String, Set<Metadata>> entry : metadata.entrySet()) {
                String modelName = entry.getKey();
                Set<Metadata> mdSet = entry.getValue();
                Set<Metadata> toBeRemoved = mdSet.stream().filter(MetadataPredicates.hasNamespace(namespace))
                mdSet.removeAll(toBeRemoved);
                if (mdSet.isEmpty()) {
                    modelsToRemove.add(modelName);
                if (!isIsolatedModel(modelName) && !toBeRemoved.isEmpty()) {
                    toBeNotified.put(modelName, toBeRemoved);
            // Remove empty model entries after iteration to avoid ConcurrentModificationException
            modelsToRemove.forEach(metadata::remove);
        toBeNotified.values().forEach((set) -> {
            set.forEach(this::notifyListenersAboutRemovedElement);
     * Removes all meta-data for a given item
    public void removeMetadataByItemName(String modelName, String itemName) {
        Set<Metadata> toBeNotified;
            toBeNotified = new HashSet<>();
            Set<Metadata> mdSet = metadata.getOrDefault(modelName, new HashSet<>());
            Set<Metadata> toBeRemoved = mdSet.stream().filter(MetadataPredicates.ofItem(itemName)).collect(toSet());
                metadata.remove(modelName);
                toBeNotified.addAll(toBeRemoved);
        toBeNotified.forEach(this::notifyListenersAboutRemovedElement);
            // Ignore isolated models
            Set<Metadata> set = metadata.keySet().stream().filter(name -> !isIsolatedModel(name))
                    .map(name -> metadata.getOrDefault(name, Set.of())).flatMap(s -> s.stream()).collect(toSet());
            return Set.copyOf(set);
    public Collection<Metadata> getAllFromModel(String modelName) {
            Set<Metadata> set = metadata.getOrDefault(modelName, Set.of());
