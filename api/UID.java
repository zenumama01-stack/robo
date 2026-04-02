 * Base class for binding related unique identifiers.
 * A UID must always start with a binding ID.
 * @author Oliver Libutzki - Added possibility to define UIDs with variable amount of segments
 * @author Jochen Hiller - Bugfix 455434: added default constructor, object is now mutable
public abstract class UID extends AbstractUID {
     * For reflection only.
     * Constructor must be public, otherwise it cannot be called by subclasses from another package.
    public UID() {
     * Parses a UID for a given string. The UID must be in the format
     * @param uid uid in form a string
    public UID(String uid) {
     * Creates a UID for list of segments.
    public UID(String... segments) {
    protected UID(List<String> segments) {
     * Returns the binding id.
     * @return binding id
        return getSegment(0);
    // Avoid subclasses to require importing the org.openhab.core.common package
    protected List<String> getAllSegments() {
        return super.getAllSegments();
    // Avoid bindings to require importing the org.openhab.core.common package
        return super.toString();
    public String getAsString() {
        return super.getAsString();
        return super.hashCode();
