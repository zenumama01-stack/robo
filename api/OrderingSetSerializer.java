 * Serializes set by ordering the elements
public class OrderingSetSerializer implements JsonSerializer<Set<@Nullable Object>> {
    public static boolean allSameClassAndComparable(Set<@Nullable Object> src) {
        Class<?> expectedClass = null;
        for (Object object : src) {
            if (!(object instanceof Comparable<?>)) {
                // not comparable or simply null
            } else if (expectedClass == null) {
                // first item
                expectedClass = object.getClass();
            } else if (!object.getClass().equals(expectedClass)) {
                // various classes in the Set, let's not try to sort
    public JsonElement serialize(Set<@Nullable Object> src, Type typeOfSrc, JsonSerializationContext context) {
        JsonArray ordered = new JsonArray();
        final Stream<@Nullable Object> possiblySortedStream;
        if (allSameClassAndComparable(src)) {
            possiblySortedStream = src.stream().sorted();
            possiblySortedStream = src.stream();
        possiblySortedStream.map(context::serialize).forEachOrdered(ordered::add);
