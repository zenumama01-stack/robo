 * This is a class that gives static access to the semantic tag library.
 * For everything that is not static, the {@link SemanticsService} should be used instead.
 * @author Jimmy Tanagra - Add the ability to add new tags at runtime
 * @author Laurent Garnier - Several methods moved into class SemanticsService or SemanticTagRegistry
public class SemanticTags {
    private static final Map<String, Class<? extends Tag>> TAGS = Collections.synchronizedMap(new TreeMap<>());
        addTagSet("Location", Location.class);
        addTagSet("Equipment", Equipment.class);
        addTagSet("Point", Point.class);
        addTagSet("Property", Property.class);
    public static @Nullable Class<? extends Tag> getById(String tagId) {
        return TAGS.get(tagId);
     * Determines the semantic type of an {@link Item} i.e. a sub-type of {@link Location}, {@link Equipment} or
     * {@link Point}.
        for (String tag : item.getTags()) {
            Class<? extends Tag> type = getById(tag);
            if (type != null && !Property.class.isAssignableFrom(type)) {
        // we haven't found any type as a tag, but if there is a Property tag, we can conclude that it is a Point
        if (getProperty(item) != null) {
            StateDescription stateDescription = item.getStateDescription();
            if (stateDescription != null && stateDescription.isReadOnly()) {
                return getById("Point_Measurement");
                return getById("Point_Control");
     * Determines the semantic root of a given tag type.
     * @param type the tag type
     * @return the semantic root of the tag type, or null if the type is not a semantic tag
    public static @Nullable Class<? extends Tag> getSemanticRoot(Class<? extends Tag> type) {
        if (Point.class.isAssignableFrom(type)) {
            return Point.class;
        } else if (Property.class.isAssignableFrom(type)) {
            return Property.class;
        } else if (Location.class.isAssignableFrom(type)) {
            return Location.class;
        } else if (Equipment.class.isAssignableFrom(type)) {
            return Equipment.class;
     * Determines the name of the semantic root of a given tag type.
     * @return the name of the semantic root of the tag type, or an empty string if the type is not a semantic tag
    public static String getSemanticRootName(Class<? extends Tag> type) {
        Class<? extends Tag> semanticRoot = getSemanticRoot(type);
        if (semanticRoot != null) {
            return semanticRoot.getSimpleName();
     * Determines the {@link Property} type that a {@link Point} relates to.
     * @param item the Item to get the property for
     * @return a sub-type of Property if the Item represents a Point, otherwise null
    public static @Nullable Class<? extends Property> getProperty(Item item) {
            if (type != null && Property.class.isAssignableFrom(type)) {
                return (Class<? extends Property>) type;
     * Determines the semantic {@link Point} type of an {@link Item}.
     * @param item the Item to get the Point for
     * @return a sub-type of a {@link Point}if the Item represents a Point, otherwise null
    public static @Nullable Class<? extends Point> getPoint(Item item) {
        Set<String> tags = item.getTags();
            if (type != null && Point.class.isAssignableFrom(type)) {
                return (Class<? extends Point>) type;
     * Determines the semantic {@link Equipment} type of an {@link Item}.
     * @param item the Item to get the Equipment for
     * @return a sub-type of {@link Equipment} if the Item represents an Equipment, otherwise null
    public static @Nullable Class<? extends Equipment> getEquipment(Item item) {
            if (type != null && Equipment.class.isAssignableFrom(type)) {
                return (Class<? extends Equipment>) type;
     * Determines the semantic {@link Location} type of an {@link Item}.
     * @param item the item to get the location for
     * @return a sub-type of {@link Location} if the item represents a location, otherwise null
    public static @Nullable Class<? extends Location> getLocation(Item item) {
            if (type != null && Location.class.isAssignableFrom(type)) {
                return (Class<? extends Location>) type;
    public static void addTagSet(String id, Class<? extends Tag> tagSet) {
        TAGS.put(id, tagSet);
    public static void removeTagSet(String id, Class<? extends Tag> tagSet) {
        TAGS.remove(id, tagSet);
