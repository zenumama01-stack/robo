import org.openhab.core.items.ItemPredicates;
 * The internal implementation of the {@link SemanticsService} interface, which is registered as an OSGi service.
 * @author Jimmy Tanagra - Add Item semantic tag validation
 * @author Mark Herwege - Refactor validation for REST endpoint
public class SemanticsServiceImpl implements SemanticsService, RegistryChangeListener<Item> {
    private static final String SYNONYMS_NAMESPACE = "synonyms";
    private final org.slf4j.Logger logger = org.slf4j.LoggerFactory.getLogger(SemanticsServiceImpl.class);
    public SemanticsServiceImpl(final @Reference ItemRegistry itemRegistry,
        this.itemRegistry.stream().forEach(this::checkSemantics);
    public Set<Item> getItemsInLocation(Class<? extends Location> locationType) {
        Set<Item> locationItems = itemRegistry.stream().filter(SemanticsPredicates.isA(locationType))
        for (Item locationItem : locationItems) {
            if (locationItem instanceof GroupItem gItem) {
                items.addAll(gItem
                        .getMembers(SemanticsPredicates.isA(Point.class).or(SemanticsPredicates.isA(Equipment.class))));
    public Set<Item> getItemsInLocation(String labelOrSynonym, Locale locale) {
        List<Class<? extends Tag>> tagList = getByLabelOrSynonym(labelOrSynonym, locale);
        if (!tagList.isEmpty()) {
            for (Class<? extends Tag> tag : tagList) {
                if (Location.class.isAssignableFrom(tag)) {
                    items.addAll(getItemsInLocation((Class<? extends Location>) tag));
            Set<Item> locationItems = itemRegistry.stream().filter(ItemPredicates.hasLabel(labelOrSynonym)
                    .or(hasSynonym(labelOrSynonym)).and(SemanticsPredicates.isLocation())).collect(Collectors.toSet());
                    items.addAll(gItem.getMembers(
                            SemanticsPredicates.isA(Point.class).or(SemanticsPredicates.isA(Equipment.class))));
    private Predicate<? super Item> hasSynonym(String labelOrSynonym) {
        return item -> {
            MetadataKey key = new MetadataKey(SYNONYMS_NAMESPACE, item.getName());
                String[] synonyms = md.getValue().split(",");
                    if (synonym.equalsIgnoreCase(labelOrSynonym)) {
    public @Nullable Class<? extends Tag> getByLabel(String tagLabel, Locale locale) {
        Optional<SemanticTag> tag = semanticTagRegistry.getAll().stream()
                .filter(t -> t.localized(locale).getLabel().equalsIgnoreCase(tagLabel))
                .sorted(Comparator.comparing(SemanticTag::getUID)).findFirst();
        return tag.isPresent() ? semanticTagRegistry.getTagClassById(tag.get().getUID()) : null;
    public List<Class<? extends Tag>> getByLabelOrSynonym(String tagLabelOrSynonym, Locale locale) {
        List<SemanticTag> tags = semanticTagRegistry.getAll().stream()
                .filter(t -> getLabelAndSynonyms(t, locale).contains(tagLabelOrSynonym.toLowerCase(locale)))
        List<Class<? extends Tag>> tagList = new ArrayList<>();
        tags.forEach(t -> {
            Class<? extends Tag> tag = semanticTagRegistry.getTagClassById(t.getUID());
                tagList.add(tag);
        return tagList;
    public List<String> getLabelAndSynonyms(Class<? extends Tag> tagClass, Locale locale) {
        SemanticTag tag = semanticTagRegistry.get(SemanticTagRegistryImpl.buildId(tagClass));
        return tag == null ? List.of() : getLabelAndSynonyms(tag, locale);
    private List<String> getLabelAndSynonyms(SemanticTag tag, Locale locale) {
        SemanticTag localizedTag = tag.localized(locale);
        Stream<String> label = Stream.of(localizedTag.getLabel());
        Stream<String> synonyms = localizedTag.getSynonyms().stream();
        return Stream.concat(label, synonyms).map(s -> s.toLowerCase(locale)).distinct().toList();
     * Validates the semantic tags of an item.
     * It returns true only if one of the following is true:
     * - No semantic tags at all
     * - Only one Semantic tag of any kind.
     * - Note: having only one Property tag is allowed. It implies that the item is a Point.
     * - One Point tag and one Property tag
     * It returns false if two Semantic tags are found, but they don't consist of one Point and one Property.
     * It would also return false if more than two Semantic tags are found.
     * @param semanticTag the determined semantic tag of the item
     * @return true if the item contains no semantic tags, or a valid combination of semantic tags, otherwise false
    boolean validateTags(Item item, @Nullable Class<? extends Tag> semanticTag) {
        return getItemTagProblems(item, semanticTag, true) == null ? true : false;
     * It returns null if one of the following is true:
     * It returns a {@link ItemSemanticProblem} if two Semantic tags are found, but they don't consist of one Point and
     * one Property.
     * It would also return a {@link ItemSemanticProblem} if more than two Semantic tags are found.
     * @param logWarnings log a warning for the problem
     * @return null if the item contains no semantic tags, or a valid combination of semantic tags, otherwise a a
     *         {@link ItemSemanticProblem}
    ItemSemanticsProblem getItemTagProblems(Item item, @Nullable Class<? extends Tag> semanticTag,
            boolean logWarnings) {
        if (semanticTag == null) {
        String semanticType = SemanticTags.getSemanticRootName(semanticTag);
        // We're using Collectors here instead of Stream.toList() to resolve Java's wildcard capture conversion issue
        List<Class<? extends Tag>> tags = item.getTags().stream().map(SemanticTags::getById).filter(Objects::nonNull)
        switch (tags.size()) {
                Class<? extends Tag> firstTag = tags.getFirst();
                Class<? extends Tag> lastTag = tags.getLast();
                if ((Point.class.isAssignableFrom(firstTag) && Property.class.isAssignableFrom(lastTag))
                        || (Point.class.isAssignableFrom(lastTag) && Property.class.isAssignableFrom(firstTag))) {
                String firstType = SemanticTags.getSemanticRootName(firstTag);
                String lastType = SemanticTags.getSemanticRootName(lastTag);
                if (firstType.equals(lastType)) {
                    if (Point.class.isAssignableFrom(firstTag) || Property.class.isAssignableFrom(firstTag)) {
                        String reason = String.format("Invalid combination of semantic tags: %s (%s) and %s (%s).",
                                firstTag.getSimpleName(), firstType, lastTag.getSimpleName(), lastType);
                        String explanation = "Only one Point and optionally one Property tag may be assigned.";
                        if (logWarnings) {
                            logger.warn("Item '{}' ({}): {} {}", item.getName(), semanticType, reason, explanation);
                        return new ItemSemanticsProblem(item.getName(), semanticType, reason, explanation, null);
                        String explanation = String.format("Only one %s tag may be assigned.", firstType);
                    String explanation = String.format("%s and %s tags cannot be assigned at the same time.", firstType,
                            lastType);
                List<String> allTags = tags.stream().map(tag -> {
                    String tagType = SemanticTags.getSemanticRootName(tag);
                    return String.format("%s (%s)", tag.getSimpleName(), tagType);
                String reason = String.format("Invalid combination of semantic tags: %s.", allTags);
                String explanation = "An Item may only have one tag of Location, Equipment, or Point type. A Property tag may be assigned in conjunction with a Point tag.";
     * Verifies the semantics of an item and logs warnings if the semantics are invalid
     * @return true if the semantics are valid, false otherwise
    boolean checkSemantics(Item item) {
        return getItemSemanticsProblems(item, true).isEmpty();
    public List<ItemSemanticsProblem> getItemSemanticsProblems(Item item) {
        return getItemSemanticsProblems(item, false);
    List<ItemSemanticsProblem> getItemSemanticsProblems(Item item, boolean logWarnings) {
        Class<? extends Tag> semanticTag = SemanticTags.getSemanticType(item);
        ItemSemanticsProblem tagProblem = getItemTagProblems(item, semanticTag, logWarnings);
        if (tagProblem != null) {
            return List.of(tagProblem);
        List<String[]> warnings = new ArrayList<>();
        List<String> parentLocations = new ArrayList<>();
        List<String> parentEquipments = new ArrayList<>();
        for (String groupName : item.getGroupNames()) {
                if (itemRegistry.getItem(groupName) instanceof GroupItem groupItem) {
                    Class<? extends Tag> groupSemanticType = SemanticTags.getSemanticType(groupItem);
                    if (groupSemanticType != null) {
                        if (Equipment.class.isAssignableFrom(groupSemanticType)) {
                            parentEquipments.add(groupName);
                        } else if (Location.class.isAssignableFrom(groupSemanticType)) {
                            parentLocations.add(groupName);
                // we don't care about invalid parent groups here
        if (Point.class.isAssignableFrom(semanticTag)) {
            if (parentLocations.size() == 1 && parentEquipments.size() == 1) {
                // This case is allowed: a Point can belong to an Equipment and a Location
                // Case 1:
                // When a location contains multiple equipments -> temperature points,
                // the average of the points will be used in the location's UI.
                // However, when there is a point which is the direct member of the location,
                // it will be used in the location's UI instead of the average.
                // So setting one of the equipment's point as a direct member of the location
                // allows to override the average.
                // Case 2:
                // When a central Equipment e.g. a HVAC contains Points located in multiple locations,
                // e.g. room controls and sensors
                    logger.info("Item '{}' ({}): Belongs to Location {} and Equipment {}.", itemName, semanticType,
                            parentLocations, parentEquipments);
                if (parentLocations.size() > 1) {
                    String[] warning = { String.format("Belongs to multiple Locations %s.", parentLocations.toString()),
                            "It should only belong to one Equipment or one Location, preferably not both at the same time." };
                    warnings.add(warning);
                if (parentEquipments.size() > 1) {
                    String[] warning = {
                            String.format("Belongs to multiple Equipment %s.", parentEquipments.toString()),
                            "A Point can only belong to at most one Equipment." };
        } else if (Equipment.class.isAssignableFrom(semanticTag)) {
            if (!parentLocations.isEmpty() && !parentEquipments.isEmpty()) {
                        String.format("Belongs to Location(s) %s and Equipment %s.", parentLocations.toString(),
                                parentEquipments.toString()),
                        "An Equipment can only belong to one Location or another Equipment, but not both." };
                        "An Equipment can only belong to one Location or another Equipment." };
                String[] warning = { String.format("Belongs to multiple Equipment %s.", parentEquipments.toString()),
                        "An Equipment can only belong to at most one Equipment." };
        } else if (Location.class.isAssignableFrom(semanticTag)) {
                String[] warning = { String.format("Is a %s Item, not a Group Item.", item.getType()),
                        "A Location should be a Group Item." };
            if (!parentEquipments.isEmpty()) {
                String[] warning = { String.format("Belongs to Equipment %s.", parentEquipments.toString()),
                        "A Location can only belong to another Location, not Equipment." };
                        "It should only belong to one Location." };
        if (!warnings.isEmpty()) {
                logger.warn("Item '{}' ({}): Invalid semantic structure:{}", itemName, semanticType, warnings.stream()
                        .map((w) -> w[0] + " " + w[1]).reduce("", (result, w) -> result + "\n        " + w));
            return warnings.stream().map((w) -> new ItemSemanticsProblem(itemName, semanticType, w[0], w[1], false))
        checkSemantics(item);
        checkSemantics(element);
