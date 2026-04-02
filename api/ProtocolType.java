 * Holds the {@link PathType}, which specifies whether its a local or remote path and the scheme.
public class ProtocolType {
     * Remote (NET) or Local path.
    public enum PathType {
        NET,
        LOCAL;
        public static PathType fromURI(URI uri) {
            return uri.getSchemeSpecificPart().startsWith("//") ? NET : LOCAL;
    private final PathType pathType;
    private final String scheme;
    public ProtocolType(PathType pathType, String scheme) {
        this.pathType = pathType;
        this.scheme = scheme;
    public PathType getPathType() {
        return pathType;
    public String getScheme() {
        return scheme;
        return "ProtocolType [pathType=" + pathType + ", scheme=" + scheme + "]";
