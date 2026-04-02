import java.util.concurrent.CopyOnWriteArrayList;
import org.openhab.core.common.registry.ProviderChangeListener;
import org.openhab.core.transform.ManagedTransformationProvider.PersistedTransformation;
import org.openhab.core.transform.Transformation;
import org.openhab.core.transform.TransformationProvider;
import com.fasterxml.jackson.databind.introspect.AnnotationIntrospectorPair;
import com.google.gson.JsonParseException;
 * A {@link MarketplaceAddonHandler} implementation, which handles community provided transformations
 * @author Jan N. Klugg - Initial contribution
public class CommunityTransformationAddonHandler implements MarketplaceAddonHandler, TransformationProvider {
    private final Logger logger = LoggerFactory.getLogger(CommunityTransformationAddonHandler.class);
    private final ObjectMapper yamlMapper;
    private final Gson gson = new Gson();
    private final Storage<PersistedTransformation> storage;
    private final List<ProviderChangeListener<Transformation>> changeListeners = new CopyOnWriteArrayList<>();
    public CommunityTransformationAddonHandler(final @Reference StorageService storageService) {
        this.storage = storageService.getStorage("org.openhab.marketplace.transformation",
                this.getClass().getClassLoader());
        yamlMapper.setAnnotationIntrospector(new AnnotationIntrospectorPair(new SerializedNameAnnotationIntrospector(),
                yamlMapper.getSerializationConfig().getAnnotationIntrospector()));
        return "transformation".equals(type) && TRANSFORMATIONS_CONTENT_TYPE.equals(contentType);
        return storage.containsKey(id);
            PersistedTransformation persistedTransformation;
                persistedTransformation = addTransformationFromYAML(addon.getUid(),
                        downloadTransformation(yamlDownloadUrl));
                persistedTransformation = addTransformationFromYAML(addon.getUid(), yamlContent);
            } else if (jsonDownloadUrl != null) {
                persistedTransformation = addTransformationFromJSON(addon.getUid(),
                        downloadTransformation(jsonDownloadUrl));
                persistedTransformation = addTransformationFromJSON(addon.getUid(), jsonContent);
                logger.error("Transformation {} has neither download URL nor embedded content", addon.getUid());
                throw new MarketplaceHandlerException("Transformation has neither download URL nor embedded content",
            Transformation transformation = map(persistedTransformation);
            changeListeners.forEach(l -> l.added(this, transformation));
            logger.error("Transformation from marketplace cannot be downloaded: {}", e.getMessage());
            throw new MarketplaceHandlerException("Transformation cannot be downloaded.", e);
            logger.error("Transformation from marketplace is invalid: {}", e.getMessage());
            throw new MarketplaceHandlerException("Transformation is not valid.", e);
        PersistedTransformation toRemoveElement = storage.remove(addon.getUid());
        if (toRemoveElement != null) {
            Transformation toRemove = map(toRemoveElement);
            changeListeners.forEach(l -> l.removed(this, toRemove));
    private String downloadTransformation(String urlString) throws IOException {
    private PersistedTransformation addTransformationFromYAML(String id, String yaml) {
            PersistedTransformation transformation = yamlMapper.readValue(yaml, PersistedTransformation.class);
            storage.put(id, transformation);
            return transformation;
    private PersistedTransformation addTransformationFromJSON(String id, String json) {
            PersistedTransformation transformation = Objects
                    .requireNonNull(gson.fromJson(json, PersistedTransformation.class));
        } catch (JsonParseException e) {
            logger.error("Unable to parse JSON: {}", e.getMessage());
            throw new IllegalArgumentException("Unable to parse JSON");
    public void addProviderChangeListener(ProviderChangeListener<Transformation> listener) {
        changeListeners.add(listener);
    public Collection<Transformation> getAll() {
        return storage.getValues().stream().filter(Objects::nonNull).map(Objects::requireNonNull).map(this::map)
    public void removeProviderChangeListener(ProviderChangeListener<Transformation> listener) {
        changeListeners.remove(listener);
    private Transformation map(PersistedTransformation persistedTransformation) {
        return new Transformation(persistedTransformation.uid, persistedTransformation.label,
                persistedTransformation.type, persistedTransformation.configuration);
