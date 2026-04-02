 * Utility class containing helper methods to be used in XML generation.
public class XmlHelper {
    public static final String SYSTEM_NAMESPACE_PREFIX = "system.";
    private static final String SYSTEM_NAMESPACE = "system";
     * Returns a UID in the format of {1}:{2}, where {1} is {@link #SYSTEM_NAMESPACE} and {2} is the
     * given typeId stripped of the prefix {@link #SYSTEM_NAMESPACE_PREFIX} if it exists.
     * @param typeId
     * @return system uid (e.g. "system:test")
    public static String getSystemUID(String typeId) {
        if (typeId.startsWith(SYSTEM_NAMESPACE_PREFIX)) {
            type = typeId.substring(SYSTEM_NAMESPACE_PREFIX.length());
            type = typeId;
        return String.format("%s:%s", SYSTEM_NAMESPACE, type);
