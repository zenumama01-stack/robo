import java.util.stream.StreamSupport;
 * The normalizer for configuration parameters allowing multiple values. It converts all collections/arrays to a
 * {@link List} and applies the underlying normalizer to each of the values inside that list.
final class ListNormalizer extends AbstractNormalizer {
    private final Normalizer delegate;
    ListNormalizer(Normalizer delegate) {
        if (!isList(value)) {
            return Set.of(value).stream().map(delegate::normalize).toList();
        } else if (isArray(value)) {
            return Arrays.stream((Object[]) value).map(delegate::normalize).toList();
        } else if (value instanceof List list) {
            return list.stream().map(delegate::normalize).toList();
        } else if (value instanceof Iterable iterable) {
            return StreamSupport.stream(iterable.spliterator(), false).map(delegate::normalize).toList();
    private static boolean isList(Object value) {
        return isArray(value) || value instanceof Iterable;
    private static boolean isArray(Object object) {
        return object.getClass().isArray();
