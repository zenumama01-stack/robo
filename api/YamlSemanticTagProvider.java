import org.openhab.core.semantics.SemanticTagProvider;
 * {@link YamlSemanticTagProvider} is an OSGi service, that allows to define semantic tags
 * in YAML configuration files in folder conf/tags.
 * These semantic tags are automatically exposed to the {@link org.openhab.core.semantics.SemanticTagRegistry}.
@Component(immediate = true, service = { SemanticTagProvider.class, YamlSemanticTagProvider.class,
public class YamlSemanticTagProvider extends AbstractProvider<SemanticTag>
        implements SemanticTagProvider, YamlModelListener<YamlSemanticTagDTO> {
    private final Logger logger = LoggerFactory.getLogger(YamlSemanticTagProvider.class);
    private final Set<SemanticTag> tags = new TreeSet<>(Comparator.comparing(SemanticTag::getUID));
    public YamlSemanticTagProvider() {
        tags.clear();
    public Collection<SemanticTag> getAll() {
    public Class<YamlSemanticTagDTO> getElementClass() {
        return YamlSemanticTagDTO.class;
    public void addedModel(String modelName, Collection<YamlSemanticTagDTO> elements) {
        List<SemanticTag> added = elements.stream().map(this::mapSemanticTag)
                .sorted(Comparator.comparing(SemanticTag::getUID)).toList();
        tags.addAll(added);
            logger.debug("model {} added tag {}", modelName, t.getUID());
    public void updatedModel(String modelName, Collection<YamlSemanticTagDTO> elements) {
        List<SemanticTag> updated = elements.stream().map(this::mapSemanticTag).toList();
            tags.stream().filter(tag -> tag.getUID().equals(t.getUID())).findFirst().ifPresentOrElse(oldTag -> {
                tags.remove(oldTag);
                tags.add(t);
                logger.debug("model {} updated tag {}", modelName, t.getUID());
                notifyListenersAboutUpdatedElement(oldTag, t);
            }, () -> logger.debug("model {} tag {} not found", modelName, t.getUID()));
    public void removedModel(String modelName, Collection<YamlSemanticTagDTO> elements) {
        elements.stream().map(elt -> elt.uid).sorted(Comparator.reverseOrder()).forEach(uid -> {
            tags.stream().filter(tag -> tag.getUID().equals(uid)).findFirst().ifPresentOrElse(oldTag -> {
                logger.debug("model {} removed tag {}", modelName, uid);
                notifyListenersAboutRemovedElement(oldTag);
            }, () -> logger.debug("model {} tag {} not found", modelName, uid));
    private SemanticTag mapSemanticTag(YamlSemanticTagDTO tagDTO) {
        return new SemanticTagImpl(tagDTO.uid, tagDTO.label, tagDTO.description, tagDTO.synonyms);
