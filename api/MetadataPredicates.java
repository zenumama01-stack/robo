 * Provides some default predicates that are helpful when working with metadata.
public final class MetadataPredicates {
     * Creates a {@link Predicate} which can be used to filter {@link Metadata} for a given namespace.
     * @param namespace to filter
    public static Predicate<Metadata> hasNamespace(String namespace) {
        return md -> md.getUID().getNamespace().equals(namespace);
     * Creates a {@link Predicate} which can be used to filter {@link Metadata} of a given item.
     * @param itemname to filter
    public static Predicate<Metadata> ofItem(String itemname) {
        return md -> md.getUID().getItemName().equals(itemname);
