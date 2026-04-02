 * This {@link MetadataProvider} collects semantic information about items and provides them as metadata under the
 * "semantics" namespace.
 * The main value of the metadata holds the semantic type of the item, i.e. a sub-class of Location, Equipment or Point.
 * The metadata configuration contains the information about the relations with the key being the name of the relation
 * (e.g. "hasLocation") and the value being the id of the referenced entity (e.g. its item name).
@Component(immediate = true, service = MetadataProvider.class)
public class SemanticsMetadataProvider extends AbstractProvider<Metadata>
        implements ItemRegistryChangeListener, MetadataProvider {
    private final Logger logger = LoggerFactory.getLogger(SemanticsMetadataProvider.class);
    // the namespace to use for the metadata
    public static final String NAMESPACE = "semantics";
    // holds the static definition of the relations between entities
    private final Map<List<Class<? extends Tag>>, String> parentRelations = new HashMap<>();
    private final Map<List<Class<? extends Tag>>, String> memberRelations = new HashMap<>();
    private final Map<List<Class<? extends Tag>>, String> propertyRelations = new HashMap<>();
    // local cache of the created metadata as a map from itemName->Metadata
    private final Map<String, Metadata> semantics = new TreeMap<>(String::compareTo);
    private SemanticTagRegistryChangeListener listener;
    public SemanticsMetadataProvider(final @Reference ItemRegistry itemRegistry,
            final @Reference SemanticTagRegistry semanticTagRegistry) {
        this.listener = new SemanticTagRegistryChangeListener(this);
        initRelations();
            processItem(item);
        semanticTagRegistry.addRegistryChangeListener(listener);
        semanticTagRegistry.removeRegistryChangeListener(listener);
        semantics.clear();
        return semantics.values();
    public Collection<String> getReservedNamespaces() {
        return Set.of(NAMESPACE);
     * Updates the semantic metadata for an item and notifies all listeners about changes
     * @param item the item to update the metadata for
    private void processItem(Item item) {
        processItem(item, new ArrayList<>());
    private void processItem(Item item, List<String> parentItems) {
        MetadataKey key = new MetadataKey(NAMESPACE, item.getName());
        Map<String, Object> configuration = new HashMap<>();
        Class<? extends Tag> type = SemanticTags.getSemanticType(item);
            processProperties(item, configuration);
            processHierarchy(item, configuration);
            Metadata md = new Metadata(key, SemanticTagRegistryImpl.buildId(type), configuration);
            Metadata oldMd = semantics.put(item.getName(), md);
            Metadata removedMd = semantics.remove(item.getName());
            if (removedMd != null) {
                notifyListenersAboutRemovedElement(removedMd);
            parentItems.add(item.getName());
            for (Item memberItem : groupItem.getMembers()) {
                if (parentItems.contains(memberItem.getName())) {
                            memberItem.getName(), groupItem.getName());
                    processItem(memberItem, new ArrayList<>(parentItems));
     * Processes Property tags on items and if found, adds it to the metadata configuration.
     * @param item the item to process
     * @param configuration the metadata configuration that should be amended
    private void processProperties(Item item, Map<String, Object> configuration) {
        for (Entry<List<Class<? extends Tag>>, String> relation : propertyRelations.entrySet()) {
            Class<? extends Tag> entityClass = relation.getKey().getFirst();
            if (entityClass.isAssignableFrom(type)) {
                Class<? extends Property> p = SemanticTags.getProperty(item);
                if (p != null) {
                    configuration.put(relation.getValue(), SemanticTagRegistryImpl.buildId(p));
     * Retrieves semantic information from parent or member items.
     * @param item the item to gather the semantic metadata for
    private void processHierarchy(Item item, Map<String, Object> configuration) {
            for (String parent : item.getGroupNames()) {
                Item parentItem = itemRegistry.get(parent);
                if (parentItem != null) {
                    processParent(type, parentItem, configuration);
            if (item instanceof GroupItem gItem) {
                for (Item memberItem : gItem.getMembers()) {
                    processMember(type, memberItem, configuration);
     * Retrieves semantic information from a parent items.
     * @param type the semantic type of the item for which the semantic information is gathered
     * @param parentItem the parent item to process
    private void processParent(Class<? extends Tag> type, Item parentItem, Map<String, Object> configuration) {
        Class<? extends Tag> typeParent = SemanticTags.getSemanticType(parentItem);
        if (typeParent == null) {
        for (Entry<List<Class<? extends Tag>>, String> relation : parentRelations.entrySet()) {
            List<Class<? extends Tag>> relClasses = relation.getKey();
            Class<? extends Tag> entityClass = relClasses.getFirst();
            Class<? extends Tag> parentClass = relClasses.get(1);
            // process relations of locations
                if (parentClass.isAssignableFrom(typeParent)) {
                    configuration.put(relation.getValue(), parentItem.getName());
     * Retrieves semantic information from a member items.
     * @param memberItem the member item to process
    private void processMember(Class<? extends Tag> type, Item memberItem, Map<String, Object> configuration) {
        Class<? extends Tag> typeMember = SemanticTags.getSemanticType(memberItem);
        if (typeMember == null) {
        for (Entry<List<Class<? extends Tag>>, String> relation : memberRelations.entrySet()) {
                if (parentClass.isAssignableFrom(typeMember)) {
                    configuration.put(relation.getValue(), memberItem.getName());
    private void initRelations() {
        parentRelations.put(List.of(Equipment.class, Location.class), "hasLocation");
        parentRelations.put(List.of(Point.class, Location.class), "hasLocation");
        parentRelations.put(List.of(Location.class, Location.class), "isPartOf");
        parentRelations.put(List.of(Equipment.class, Equipment.class), "isPartOf");
        parentRelations.put(List.of(Point.class, Equipment.class), "isPointOf");
        memberRelations.put(List.of(Equipment.class, Point.class), "hasPoint");
        propertyRelations.put(List.of(Point.class), "relatesTo");
        for (Item item : itemRegistry.getItems()) {
                    processItem(memberItem);
    private static class SemanticTagRegistryChangeListener implements RegistryChangeListener<SemanticTag> {
        private SemanticsMetadataProvider provider;
        public SemanticTagRegistryChangeListener(SemanticsMetadataProvider provider) {
        public void added(SemanticTag element) {
            provider.allItemsChanged(List.of());
        public void removed(SemanticTag element) {
        public void updated(SemanticTag oldElement, SemanticTag element) {
