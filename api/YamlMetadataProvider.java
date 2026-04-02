 * This class serves as a provider for all metadata that is found within YAML files.
@Component(service = { MetadataProvider.class, YamlMetadataProvider.class })
public class YamlMetadataProvider extends AbstractProvider<Metadata> implements MetadataProvider {
    private final Logger logger = LoggerFactory.getLogger(YamlMetadataProvider.class);
    // Map the metadata to each namespace and then to each item name and finally to each model name
    private Map<String, Map<String, Map<String, Metadata>>> metadataMap = new ConcurrentHashMap<>();
        return metadataMap.keySet().stream().filter(name -> !isIsolatedModel(name))
                .map(name -> metadataMap.getOrDefault(name, Map.of())).flatMap(m -> m.values().stream())
        return metadataMap.getOrDefault(modelName, Map.of()).values().stream().flatMap(m -> m.values().stream())
    public void updateMetadata(String modelName, String itemName, Map<String, YamlMetadataDTO> metadata) {
        Map<String, Map<String, Metadata>> itemsMetadataMap = Objects
                .requireNonNull(metadataMap.computeIfAbsent(modelName, k -> new ConcurrentHashMap<>()));
        Map<String, Metadata> namespacesMetadataMap = Objects
                .requireNonNull(itemsMetadataMap.computeIfAbsent(itemName, k -> new ConcurrentHashMap<>()));
        Set<String> namespaceToBeRemoved = new HashSet<>(namespacesMetadataMap.keySet());
        for (Map.Entry<String, YamlMetadataDTO> entry : metadata.entrySet()) {
            String namespace = entry.getKey();
            YamlMetadataDTO mdDTO = entry.getValue();
            Metadata md = new Metadata(key, mdDTO.value == null ? "" : mdDTO.value, mdDTO.config);
            namespaceToBeRemoved.remove(namespace);
            Metadata oldMd = namespacesMetadataMap.get(namespace);
            if (oldMd == null) {
                namespacesMetadataMap.put(namespace, md);
                logger.debug("model {} added metadata {}", modelName, namespace);
            } else if (!md.getValue().equals(oldMd.getValue())
                    || !Objects.equals(md.getConfiguration(), oldMd.getConfiguration())) {
                logger.debug("model {} updated metadata {}", modelName, namespace);
                    notifyListenersAboutUpdatedElement(oldMd, md);
        namespaceToBeRemoved.forEach(namespace -> {
            Metadata md = namespacesMetadataMap.remove(namespace);
            if (md != null) {
                logger.debug("model {} removed metadata {}", modelName, namespace);
                    notifyListenersAboutRemovedElement(md);
        if (namespacesMetadataMap.isEmpty()) {
            itemsMetadataMap.remove(itemName);
        if (itemsMetadataMap.isEmpty()) {
            metadataMap.remove(modelName);
