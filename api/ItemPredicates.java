 * Provides some default predicates that are helpful when working with items.
public final class ItemPredicates {
     * Creates a {@link Predicate} which can be used to filter {@link Item}s for a given label.
     * @param label to filter
    public static Predicate<Item> hasLabel(String label) {
        return i -> label.equalsIgnoreCase(i.getLabel());
