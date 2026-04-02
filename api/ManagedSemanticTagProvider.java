import org.openhab.core.semantics.dto.SemanticTagDTO;
import org.openhab.core.semantics.dto.SemanticTagDTOMapper;
 * {@link ManagedSemanticTagProvider} is an OSGi service, that allows to add or remove
 * semantic tags at runtime by calling {@link ManagedSemanticTagProvider#add}
 * or {@link ManagedSemanticTagProvider#remove}.
 * An added semantic tag is automatically exposed to the {@link SemanticTagRegistry}.
 * Persistence of added semantic tags is handled by a {@link StorageService}.
@Component(immediate = true, service = { SemanticTagProvider.class, ManagedSemanticTagProvider.class })
public class ManagedSemanticTagProvider extends AbstractManagedProvider<SemanticTag, String, SemanticTagDTO>
        implements SemanticTagProvider {
    public ManagedSemanticTagProvider(final @Reference StorageService storageService) {
        return SemanticTag.class.getName();
        // Sort tags by uid to be sure that tag classes will be created in the right order
        return super.getAll().stream().sorted(Comparator.comparing(SemanticTag::getUID)).toList();
    protected @Nullable SemanticTag toElement(String uid, SemanticTagDTO persistedTag) {
        return SemanticTagDTOMapper.map(persistedTag);
    protected SemanticTagDTO toPersistableElement(SemanticTag tag) {
        return SemanticTagDTOMapper.map(tag);
