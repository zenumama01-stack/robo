package org.openhab.core.model.lsp.internal;
import org.eclipse.xtext.ide.server.UriExtensions;
 * {@link UriExtensions} implementation.
 * It takes into account the fact that although language server and client both operate on the same set of files, their
 * file system location might be different due to remote access via SMB, SSH and the like.
public class MappingUriExtensions extends UriExtensions {
    private final Logger logger = LoggerFactory.getLogger(MappingUriExtensions.class);
    private final String rawConfigFolder;
    private final String serverLocation;
    private @Nullable String clientLocation;
    public MappingUriExtensions(String configFolder) {
        this.rawConfigFolder = configFolder;
        this.serverLocation = calcServerLocation(configFolder);
        logger.debug("The language server is using '{}' as its workspace", serverLocation);
    protected String calcServerLocation(String configFolder) {
        Path configPath = Path.of(configFolder);
        Path absoluteConfigPath = configPath.toAbsolutePath();
        java.net.URI configPathURI = absoluteConfigPath.toUri();
        return removeTrailingSlash(configPathURI.toString());
    public URI toUri(@NonNullByDefault({}) String pathWithScheme) {
        String decodedPathWithScheme = URLDecoder.decode(pathWithScheme, StandardCharsets.UTF_8);
        String localClientLocation = clientLocation;
        if (localClientLocation != null && decodedPathWithScheme.startsWith(localClientLocation)) {
            return map(decodedPathWithScheme);
        localClientLocation = clientLocation = guessClientPath(decodedPathWithScheme);
        if (localClientLocation != null) {
            logger.debug("Identified client workspace as '{}'", localClientLocation);
        clientLocation = pathWithScheme;
        logger.debug("Path mapping could not be done for '{}', leaving it untouched", pathWithScheme);
        java.net.URI javaNetUri = java.net.URI.create(pathWithScheme);
        return URI.createURI(toPathAsInXtext212(javaNetUri));
    public String toUriString(@NonNullByDefault({}) URI uri) {
        if (clientLocation == null) {
            return uri.toString();
        return mapToClientPath(uri.toString());
    public String toUriString(@NonNullByDefault({}) java.net.URI uri) {
        return toUriString(URI.createURI(uri.toString()));
    private String mapToClientPath(String pathWithScheme) {
        String clientLocation = this.clientLocation;
        String uriString = clientLocation == null ? serverLocation
                : pathWithScheme.replace(serverLocation, clientLocation);
        String clientPath = toPathAsInXtext212(java.net.URI.create(uriString));
        logger.trace("Mapping server path {} to client path {}", pathWithScheme, clientPath);
        return clientPath;
    protected final String removeTrailingSlash(String path) {
        if (path.endsWith("/")) {
            return path.substring(0, path.length() - 1);
     * Guess the client path.
     * It works as follows: It starts with replacing the full clients path with the path of the config folder.
     * In the next iteration it shortens the path to be replaced by one subfolder.
     * It repeats that until the resulting filename exists.
     * @param pathWithScheme the filename as coming from the client
     * @return the substring which needs to be replaced with the runtime's config folder path
    protected @Nullable String guessClientPath(String pathWithScheme) {
        if (isPointingToConfigFolder(pathWithScheme)) {
            return removeTrailingSlash(pathWithScheme);
        } else if (isFolder(pathWithScheme)) {
        String currentPath = pathWithScheme;
        int nextIndex = getLastPathSegmentIndex(currentPath);
        while (nextIndex > -1) {
            currentPath = currentPath.substring(0, nextIndex);
            java.net.URI uri = toURI(pathWithScheme, currentPath);
            File realFile = new File(uri);
            if (realFile.exists()) {
                return currentPath;
            nextIndex = getLastPathSegmentIndex(currentPath);
    private boolean isFolder(String currentPath) {
        return !currentPath.substring(getLastPathSegmentIndex(currentPath)).contains(".");
    private boolean isPointingToConfigFolder(String currentPath) {
        return currentPath.endsWith("/" + rawConfigFolder);
    private int getLastPathSegmentIndex(String currentPath) {
        return removeTrailingSlash(currentPath).lastIndexOf("/");
    private URI map(String pathWithScheme) {
        java.net.URI javaNetUri = toURI(pathWithScheme, clientLocation);
        logger.trace("Going to map path {}", javaNetUri);
        URI ret = URI.createURI(toPathAsInXtext212(javaNetUri));
        logger.trace("Mapped path {} to {}", pathWithScheme, ret);
    private java.net.URI toURI(String pathWithScheme, @Nullable String currentPath) {
        String path = currentPath == null ? pathWithScheme : pathWithScheme.replace(currentPath, serverLocation);
        return java.net.URI.create(path);
    private String toPathAsInXtext212(java.net.URI uri) {
        // org.eclipse.xtext.ide.server.UriExtensions:
        // In Xtext 2.14 the method "String toPath(java.netURI)" has been deprecated but still exist.
        // It delegate the logic internally to the new method "String toUriString(java.net.URI uri)".
        // That new method seems to return a different result for folder / directories with respect to
        // the present / absent of a trailing slash.
        // The old logic removes trailing slashes if it has been present in the input.
        // The new logic keeps trailing slashes if it has been present in the input.
        // input: file:///d/
        // output old: file:///d
        // output new: file:///d
        // output new: file:///d/
        // We use this method now to keep the old behavior.
        return Path.of(uri).toUri().toString();
