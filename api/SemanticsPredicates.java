 * This class provides predicates that allow filtering item streams with regards to their semantics.
public class SemanticsPredicates {
     * Creates a {@link Predicate} which can be used to filter {@link Item}s that represent a Location.
     * @return created {@link Predicate}
    public static Predicate<Item> isLocation() {
        return i -> isA(Location.class).test(i);
     * Creates a {@link Predicate} which can be used to filter {@link Item}s that represent an Equipment.
    public static Predicate<Item> isEquipment() {
        return i -> isA(Equipment.class).test(i);
     * Creates a {@link Predicate} which can be used to filter {@link Item}s that represent a Point.
    public static Predicate<Item> isPoint() {
        return i -> isA(Point.class).test(i);
     * Creates a {@link Predicate} which can be used to filter {@link Item}s that represent a given semantic type.
     * @param type the semantic type to filter for
    public static Predicate<Item> isA(Class<? extends Tag> type) {
        return i -> {
            Class<? extends Tag> semanticType = SemanticTags.getSemanticType(i);
            return semanticType != null && type.isAssignableFrom(semanticType);
     * Creates a {@link Predicate} which can be used to filter {@link Item}s that relates to a given property.
     * @param property the semantic property to filter for
    public static Predicate<Item> relatesTo(Class<? extends Property> property) {
            Class<? extends Tag> semanticProperty = SemanticTags.getProperty(i);
            return semanticProperty != null && property.isAssignableFrom(semanticProperty);
