package org.openhab.core.common;
 * A non specific base class for unique identifiers.
public abstract class AbstractUID {
    private static final Pattern SEGMENT_PATTERN = Pattern.compile("[\\w-]*");
    public static final String SEPARATOR = ":";
    private final List<String> segments;
    private String uid = "";
     * Constructor must be protected, otherwise it can not be called by subclasses from another package.
    protected AbstractUID() {
        segments = List.of();
    protected AbstractUID(String uid) {
        this(splitToSegments(uid));
     * Creates an AbstractUID for a list of segments.
     * @param segments the id segments
    protected AbstractUID(final String... segments) {
        this(Arrays.asList(segments));
    protected AbstractUID(List<String> segments) {
        int minNumberOfSegments = getMinimalNumberOfSegments();
        int numberOfSegments = segments.size();
        if (numberOfSegments < minNumberOfSegments) {
                    String.format("UID must have at least %d segments: %s", minNumberOfSegments, segments));
        for (int i = 0; i < numberOfSegments; i++) {
            String segment = segments.get(i);
            validateSegment(segment, i, numberOfSegments);
        if (segments.get(numberOfSegments - 1).isBlank()) {
            throw new IllegalArgumentException("Last segment must not be blank: " + segments);
        this.segments = List.copyOf(segments);
     * Specifies how many segments the UID has to have at least.
     * @return the number of segments
    protected abstract int getMinimalNumberOfSegments();
    protected String getSegment(int segment) {
        return segments.get(segment);
    public static boolean isValid(@Nullable String segment) {
        return segment != null && SEGMENT_PATTERN.matcher(segment).matches();
        if (!isValid(segment)) {
                    "ID segment '%s' contains invalid characters. Each segment of the ID must match the pattern %s.",
                    segment, SEGMENT_PATTERN));
        return getAsString();
        if (uid.isEmpty()) {
            uid = String.join(SEPARATOR, segments);
    private static List<String> splitToSegments(final String id) {
        return Arrays.asList(id.split(SEPARATOR));
        result = prime * result + segments.hashCode();
        AbstractUID other = (AbstractUID) obj;
        return segments.equals(other.segments);
