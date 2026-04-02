package org.openhab.core.common.osgi;
 * The {@link ResourceBundleClassLoader} is a user defined classloader which is
 * responsible to map files within an <i>OSGi</i> bundle to {@link URL}s. This
 * implementation only supports the method {@link #getResource(String)} for
 * mappings.
 * @author Martin Herbst - UTF-8 replaced by ISO-8859-1 to follow Java standards
public class ResourceBundleClassLoader extends ClassLoader {
    private Bundle bundle;
    private String path;
    private String filePattern;
     * @param bundle the bundle whose files should be mapped (must not be null)
     * @param path the path within the bundle which should be considered to be
     *            mapped. If null is set, all files within the bundle are
     *            considered.
     * @param filePattern the pattern for files to be considered within the specified
     *            path. If null is set, all files within the specified path are
     * @throws IllegalArgumentException if the bundle is null
    public ResourceBundleClassLoader(@Nullable Bundle bundle, @Nullable String path, @Nullable String filePattern)
            throw new IllegalArgumentException("The bundle must not be null!");
        this.path = path != null ? path : "/";
        this.filePattern = filePattern != null ? filePattern : "*";
    public @Nullable URL getResource(String name) {
        Enumeration<URL> resourceFiles = this.bundle.findEntries(this.path, this.filePattern, true);
        List<URL> allResources = new LinkedList<>();
        if (resourceFiles != null) {
            while (resourceFiles.hasMoreElements()) {
                URL resourceURL = resourceFiles.nextElement();
                String resourcePath = resourceURL.getFile();
                File resourceFile = new File(resourcePath);
                String resourceFileName = resourceFile.getName();
                if (resourceFileName.equals(name)) {
                    allResources.add(resourceURL);
        if (allResources.isEmpty()) {
        if (allResources.size() == 1) {
            return allResources.getFirst();
        // handle fragment resources. return first one.
        for (URL url : allResources) {
            boolean isHostResource = bundle.getEntry(url.getPath()) != null
                    && bundle.getEntry(url.getPath()).equals(url);
            if (isHostResource) {
    public @Nullable InputStream getResourceAsStream(String name) {
        URL resourceURL = getResource(name);
        if (resourceURL != null) {
            try (InputStream resourceStream = resourceURL.openStream()) {
                if (resourceStream != null) {
                    try (Reader resourceReader = new InputStreamReader(resourceStream, StandardCharsets.ISO_8859_1)) {
                        Properties props = new Properties();
                        props.load(resourceReader);
                        ByteArrayOutputStream baos = new ByteArrayOutputStream();
                        props.store(baos, "converted");
                        return new ByteArrayInputStream(baos.toByteArray());
            return super.getResourceAsStream(name);
