package org.openhab.core.model.core.internal.folder;
import org.openhab.core.model.core.ModelParser;
 * This class is able to observe multiple folders for changes and notifies the
 * model repository about every change, so that it can update itself.
 * @author Fabio Marini - Refactoring to use WatchService
 * @author Ana Dimova - reduce to a single watch thread for all class instances
@Component(name = "org.openhab.core.folder", immediate = true, configurationPid = "org.openhab.folder", configurationPolicy = ConfigurationPolicy.REQUIRE)
public class FolderObserver implements WatchService.WatchEventListener {
    private final Logger logger = LoggerFactory.getLogger(FolderObserver.class);
    /* the model repository is provided as a service */
    private final ModelRepository modelRepository;
    private static final String READYMARKER_TYPE = "dsl";
    private boolean activated;
    /* map that stores a list of valid file extensions for each folder */
    private final Map<String, Set<String>> folderFileExtMap = new ConcurrentHashMap<>();
    /* set of file extensions for which we have parsers already registered */
    private final Set<String> parsers = new HashSet<>();
    /* set of file extensions for missing parsers during activation */
    private final Set<String> missingParsers = new HashSet<>();
    /* set of files that have been ignored due to a missing parser */
    private final Set<Path> ignoredPaths = new HashSet<>();
    private final Map<String, Path> namePathMap = new HashMap<>();
    public FolderObserver(final @Reference ModelRepository modelRepo, final @Reference ReadyService readyService,
        this.modelRepository = modelRepo;
        this.watchPath = watchService.getWatchPath();
    protected void addModelParser(ModelParser modelParser) {
        String extension = modelParser.getExtension();
        logger.debug("Adding parser for '{}' extension", extension);
        parsers.add(extension);
            processIgnoredPaths(extension);
            readyService.markReady(new ReadyMarker(READYMARKER_TYPE, extension));
            logger.debug("Marked extension '{}' as ready", extension);
            logger.debug("{} is not yet activated", FolderObserver.class.getSimpleName());
    protected void removeModelParser(ModelParser modelParser) {
        logger.debug("Removing parser for '{}' extension", extension);
        parsers.remove(extension);
        Set<String> removed = modelRepository.removeAllModelsOfType(extension);
        ignoredPaths
                .addAll(removed.stream().map(namePathMap::get).filter(Objects::nonNull).collect(Collectors.toSet()));
        logger.debug("FolderObserver activate");
        /* set of file extensions for added parsers before activation but without an existing directory */
        Set<String> parsersWithoutFolder = new HashSet<>();
        Dictionary<String, Object> config = ctx.getProperties();
        Enumeration<String> keys = config.keys();
            String folderName = keys.nextElement();
            if (!folderName.matches("[A-Za-z0-9_]*")) {
                // we allow only simple alphanumeric names for model folders - everything else might be other service
            Path folderPath = watchPath.resolve(folderName);
            Set<String> validExtensions = Set.of(((String) config.get(folderName)).split(","));
            if (Files.exists(folderPath) && Files.isDirectory(folderPath)) {
                folderFileExtMap.put(folderName, validExtensions);
                parsersWithoutFolder.addAll(validExtensions);
                logger.warn("Directory '{}' does not exist in '{}'. Please check your configuration settings!",
                        folderName, OpenHAB.getConfigFolder());
        watchService.registerListener(this, Path.of(""));
        addModelsToRepo();
        this.activated = true;
        logger.debug("{} has been activated", FolderObserver.class.getSimpleName());
        logger.debug("{} elements in parsersWithoutFolder and {} elements in missingParsers",
                parsersWithoutFolder.size(), missingParsers.size());
        // process parsers without existing directory which were added during activation
        for (String extension : parsersWithoutFolder) {
            if (parsers.contains(extension) && !missingParsers.contains(extension)) {
        // process ignored paths for missing parsers which were added during activation
        for (String extension : missingParsers) {
            if (parsers.contains(extension)) {
        missingParsers.clear();
        activated = false;
        deleteModelsFromRepo();
        ignoredPaths.clear();
        folderFileExtMap.clear();
        namePathMap.clear();
        logger.debug("{} has been deactivated", FolderObserver.class.getSimpleName());
    private void processIgnoredPaths(String extension) {
        logger.debug("Processing {} ignored paths for '{}' extension", ignoredPaths.size(), extension);
        Set<Path> clonedSet = new HashSet<>(ignoredPaths);
        for (Path path : clonedSet) {
            if (extension.equals(getExtension(path))) {
                checkPath(path, CREATE);
                ignoredPaths.remove(path);
        logger.debug("Finished processing ignored paths for '{}' extension. {} ignored paths remain", extension,
                ignoredPaths.size());
    private void addModelsToRepo() {
        for (Map.Entry<String, Set<String>> entry : folderFileExtMap.entrySet()) {
            String folderName = entry.getKey();
            Set<String> validExtensions = entry.getValue();
            if (validExtensions.isEmpty()) {
                logger.debug("Folder '{}' has no valid extensions", folderName);
            logger.debug("Adding files in '{}' to the model", folderPath);
            try (DirectoryStream<Path> stream = Files.newDirectoryStream(folderPath,
                    new FileExtensionsFilter(validExtensions))) {
                stream.forEach(path -> checkPath(path, CREATE));
                logger.warn("Failed to list entries in directory: {}", folderPath.toAbsolutePath(), e);
            for (String extension : validExtensions) {
        logger.debug("Added {} model files and {} ignored paths remain", namePathMap.size(), ignoredPaths.size());
    private void deleteModelsFromRepo() {
        for (String folder : folderFileExtMap.keySet()) {
            Iterable<String> models = modelRepository.getAllModelNamesOfType(folder);
            for (String model : models) {
                logger.debug("Removing file {} from the model repo.", model);
                modelRepository.removeModel(model);
    protected static class FileExtensionsFilter implements DirectoryStream.Filter<Path> {
        private final Set<String> validExtensions;
        public FileExtensionsFilter(Set<String> validExtensions) {
            this.validExtensions = validExtensions;
        public boolean accept(Path entry) throws IOException {
            String extension = getExtension(entry);
            return extension != null && validExtensions.contains(extension);
    private void checkPath(final Path path, final WatchService.Kind kind) {
            // Checking isHidden() on a deleted file will throw an IOException on some file systems,
            // so deal with deletion first.
                synchronized (FolderObserver.class) {
                    modelRepository.removeModel(fileName);
                    namePathMap.remove(fileName);
                    logger.debug("Removed '{}' model ", fileName);
            if (Files.isHidden(path)) {
                // we omit parsing of hidden files possibly created by editors or operating systems
                    logger.debug("Omitting update of hidden file '{}'", path.toAbsolutePath());
                if (kind == CREATE || kind == MODIFY) {
                    String extension = getExtension(fileName);
                        try (InputStream inputStream = Files.newInputStream(path)) {
                            namePathMap.put(fileName, path);
                            modelRepository.addOrRefreshModel(fileName, inputStream);
                            logger.debug("Added/refreshed '{}' model", fileName);
                            logger.warn("Error while opening file during update: {}", path.toAbsolutePath());
                    } else if (extension != null) {
                        ignoredPaths.add(path);
                        if (!activated) {
                            missingParsers.add(extension);
                            logger.debug("Missing parser for '{}' extension, added ignored path: {}", extension,
                                    path.toAbsolutePath());
            logger.error("Error handling update of file '{}': {}.", path.toAbsolutePath(), e.getMessage(), e);
    private static @Nullable String getExtension(String fileName) {
        return fileName.contains(".") ? fileName.substring(fileName.lastIndexOf(".") + 1) : null;
    private static @Nullable String getExtension(Path path) {
        return getExtension(path.getFileName().toString());
        Path path = watchPath.relativize(fullPath);
        if (path.getNameCount() != 2) {
            logger.trace("{} event for {} ignored (only depth 1 allowed)", kind, path);
        String extension = getExtension(path);
            logger.trace("{} event for {} ignored (extension null)", kind, path);
        String folderName = path.getName(0).toString();
        Set<String> validExtensions = folderFileExtMap.get(folderName);
        if (validExtensions == null) {
            logger.trace("{} event for {} ignored (folder '{}' extensions null)", kind, path, folderName);
        if (!validExtensions.contains(extension)) {
            logger.trace("{} event for {} ignored ('{}' extension is invalid)", kind, path, extension);
        checkPath(fullPath, kind);
