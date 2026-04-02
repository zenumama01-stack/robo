package org.openhab.core.automation.internal.provider.file;
 * This class is base for {@link ModuleTypeProvider} and {@link TemplateProvider}, responsible for importing the
 * automation objects from local file system.
 * It provides functionality for tracking {@link Parser} services and provides common functionality for notifying the
 * {@link ProviderChangeListener}s for adding, updating and removing the {@link ModuleType}s or {@link Template}s.
 * @author Arne Seime - Added object validation support
public abstract class AbstractFileProvider<@NonNull E> implements Provider<E> {
    protected static final String CONFIG_PROPERTY_ROOTS = "roots";
    protected final Logger logger = LoggerFactory.getLogger(AbstractFileProvider.class);
    protected final String rootSubdirectory;
    protected String[] configurationRoots;
     * The Map has for keys URLs of the files containing automation objects and for values - parsed objects.
    private final Map<String, Parser<E>> parsers = new ConcurrentHashMap<>();
     * This map is used for mapping the imported automation objects to the file that contains them. This provides
     * opportunity when an event for deletion of the file is received, how to recognize which objects are removed.
    private final Map<URL, List<String>> providerPortfolio = new ConcurrentHashMap<>();
     * This Map holds URL resources that waiting for a parser to be loaded.
    private final Map<String, List<URL>> urls = new ConcurrentHashMap<>();
    private final List<ProviderChangeListener<E>> listeners = new ArrayList<>();
    protected AbstractFileProvider(String root) {
        this.rootSubdirectory = root;
        configurationRoots = new String[] { OpenHAB.getConfigFolder() + File.separator + "automation" };
    public void activate(Map<String, Object> config) {
        for (String root : this.configurationRoots) {
            deactivateWatchService(root + File.separator + rootSubdirectory);
        urls.clear();
    public synchronized void modified(Map<String, Object> config) {
        String roots = (String) config.get(CONFIG_PROPERTY_ROOTS);
        if (roots != null) {
                if (!roots.contains(root)) {
            this.configurationRoots = roots.split(",");
        for (String configurationRoot : this.configurationRoots) {
            initializeWatchService(configurationRoot + File.separator + rootSubdirectory);
    public void addProviderChangeListener(ProviderChangeListener<E> listener) {
    public void removeProviderChangeListener(ProviderChangeListener<E> listener) {
     * Imports resources from the specified file or directory.
     * @param file the file or directory to import resources from
    public void importResources(File file) {
        if (file.exists()) {
            File[] files = file.listFiles();
            if (files != null) {
                for (File f : files) {
                    if (!f.isHidden()) {
                        importResources(f);
                    URL url = file.toURI().toURL();
                    importFile(parserType, url);
                } catch (MalformedURLException e) {
                    // can't happen for the 'file' protocol handler with a correctly formatted URI
                    logger.debug("Can't create a URL", e);
     * Removes resources that were loaded from the specified file or directory when the file or directory disappears.
    public void removeResources(File file) {
        String path = file.getAbsolutePath();
        for (URL key : providerPortfolio.keySet()) {
                File f = new File(key.toURI());
                if (f.getAbsolutePath().startsWith(path)) {
                    List<String> portfolio = providerPortfolio.remove(key);
                    removeElements(portfolio);
                logger.debug("Can't create a URI", e);
     * @param properties
    public void addParser(Parser<E> parser, Map<String, String> properties) {
        List<URL> value = urls.get(parserType);
        if (value != null && !value.isEmpty()) {
            for (URL url : value) {
    public void removeParser(Parser<E> parser, Map<String, String> properties) {
     * This method is responsible for importing a set of Automation objects from a specified URL resource.
     * @param parserType is relevant to the format that you need for conversion of the Automation objects in text.
    protected void importFile(String parserType, URL url) {
        logger.debug("Reading file \"{}\"", url);
            InputStreamReader inputStreamReader = null;
                inputStreamReader = new InputStreamReader(bis);
                Set<E> providedObjects = parser.parse(inputStreamReader);
                updateProvidedObjectsHolder(url, providedObjects);
                logger.debug("{}", e.getMessage(), e);
                if (inputStreamReader != null) {
            synchronized (urls) {
                List<URL> value = Objects.requireNonNull(urls.computeIfAbsent(parserType, k -> new ArrayList<>()));
                value.add(url);
            logger.debug("Couldn't parse \"{}\", no \"{}\" parser available", url, parserType);
    protected void updateProvidedObjectsHolder(URL url, Set<E> providedObjects) {
            List<String> uids = new ArrayList<>();
            for (E providedObject : providedObjects) {
                    validateObject(providedObject);
                } catch (ValidationException e) {
                    logger.warn("Rejecting \"{}\" because the validation failed: {}", url, e.getMessage());
                    logger.trace("", e);
                String uid = getUID(providedObject);
                if (providerPortfolio.entrySet().stream().filter(e -> !url.equals(e.getKey()))
                        .flatMap(e -> e.getValue().stream()).anyMatch(u -> uid.equals(u))) {
                    logger.warn("Rejecting \"{}\" from \"{}\" because the UID is already registered", uid, url);
                final @Nullable E oldProvidedObject = providedObjectsHolder.put(uid, providedObject);
                notifyListeners(oldProvidedObject, providedObject);
            if (!uids.isEmpty()) {
                providerPortfolio.put(url, uids);
     * Validates that the parsed object is valid. For no validation, create an empty method.
     * @param object the object to validate.
    protected abstract void validateObject(E object) throws ValidationException;
    protected void removeElements(@Nullable List<String> objectsForRemove) {
        if (objectsForRemove != null) {
            for (String removedObject : objectsForRemove) {
                notifyListeners(providedObjectsHolder.remove(removedObject));
    protected void notifyListeners(@Nullable E oldElement, E newElement) {
            for (ProviderChangeListener<E> listener : listeners) {
    protected void notifyListeners(@Nullable E removedObject) {
    protected abstract String getUID(E providedObject);
    protected abstract void initializeWatchService(String watchingDir);
    protected abstract void deactivateWatchService(String watchingDir);
    private String getParserType(URL url) {
