import java.util.regex.Matcher;
import java.util.regex.Pattern;
 * The {@link BundleVersion} wraps a bundle version and provides a method to compare them
public class BundleVersion {
    private static final Pattern VERSION_PATTERN = Pattern.compile(
            "(?<major>\\d+)\\.(?<minor>\\d+)\\.(?<micro>\\d+)(\\.((?<rc>RC)|(?<milestone>M))?(?<qualifier>\\d+))?");
    public static final Pattern RANGE_PATTERN = Pattern.compile(
            "\\[(?<start>\\d+\\.\\d+(?<startmicro>\\.\\d+(\\.\\w+)?)?);(?<end>\\d+\\.\\d+(?<endmicro>\\.\\d+(\\.\\w+)?)?)(?<endtype>[)\\]])");
    private final String version;
    private final int major;
    private final int minor;
    private final int micro;
    private final @Nullable Long qualifier;
    public BundleVersion(String version) {
        Matcher matcher = VERSION_PATTERN.matcher(version);
        if (matcher.matches()) {
            this.version = version;
            this.major = Integer.parseInt(matcher.group("major"));
            this.minor = Integer.parseInt(matcher.group("minor"));
            this.micro = Integer.parseInt(matcher.group("micro"));
            String qualifier = matcher.group("qualifier");
            if (qualifier != null) {
                long intQualifier = Long.parseLong(qualifier);
                if (matcher.group("rc") != null) {
                    // we can safely assume that there are less than Integer.MAX_VALUE milestones
                    // so RCs are always newer than milestones
                    // since snapshot qualifiers are larger than 10*Integer.MAX_VALUE they are
                    // still considered newer
                    this.qualifier = intQualifier + Integer.MAX_VALUE;
                    this.qualifier = intQualifier;
                this.qualifier = null;
            throw new IllegalArgumentException("Input does not match pattern");
     * Test if this version is within the provided range
     * @param range a Maven like version range
     * @return {@code true} if this version is inside range, {@code false} otherwise
     * @throws IllegalArgumentException if {@code range} does not represent a valid range
    public boolean inRange(@Nullable String range) throws IllegalArgumentException {
        if (range == null || range.isBlank()) {
            // if no range is given, we assume the range covers everything
        Matcher matcher = RANGE_PATTERN.matcher(range);
        if (!matcher.matches()) {
            throw new IllegalArgumentException(range + "is not a valid version range");
        String startString = matcher.group("startmicro") != null ? matcher.group("start")
                : matcher.group("start") + ".0";
        BundleVersion startVersion = new BundleVersion(startString);
        if (this.compareTo(startVersion) < 0) {
        String endString = matcher.group("endmicro") != null ? matcher.group("end") : matcher.group("stop") + ".0";
        boolean inclusive = "]".equals(matcher.group("endtype"));
        BundleVersion endVersion = new BundleVersion(endString);
        int comparison = this.compareTo(endVersion);
        return (inclusive && comparison == 0) || comparison < 0;
    public boolean equals(@Nullable Object o) {
        if (this == o) {
        if (o == null || getClass() != o.getClass()) {
        BundleVersion version = (BundleVersion) o;
        return major == version.major && minor == version.minor && micro == version.micro
                && Objects.equals(qualifier, version.qualifier);
    public int hashCode() {
        return Objects.hash(major, minor, micro, qualifier);
     * Compares two bundle versions
     * @param other the other bundle version
     * @return a positive integer if this version is newer than the other version, a negative number if this version is
     *         older than the other version and 0 if the versions are equal
    public int compareTo(BundleVersion other) {
        int result = major - other.major;
        if (result != 0) {
        result = minor - other.minor;
        result = micro - other.micro;
        if (Objects.equals(qualifier, other.qualifier)) {
        // the release is always newer than a milestone or snapshot
        Long thisQualifier = qualifier;
        if (thisQualifier == null) { // we are the release
        Long otherQualifier = other.qualifier;
        if (otherQualifier == null) { // the other is the release
        // both versions are milestones, we can compare them
        return Long.compare(thisQualifier, otherQualifier);
    public String toString() {
