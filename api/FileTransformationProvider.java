import static org.openhab.core.transform.Transformation.FUNCTION;
 * The {@link FileTransformationProvider} implements a {@link TransformationProvider} for
 * supporting transformations stored in configuration files
@Component(service = TransformationProvider.class, immediate = true)
public class FileTransformationProvider implements WatchService.WatchEventListener, TransformationProvider {
    private static final Set<String> IGNORED_EXTENSIONS = Set.of("txt", "swp");
    private static final Pattern FILENAME_PATTERN = Pattern
            .compile("(?<filename>.+?)(_(?<language>[a-z]{2}))?\\.(?<extension>[^.]*)$");
    private final Logger logger = LoggerFactory.getLogger(FileTransformationProvider.class);
    private final Set<ProviderChangeListener<Transformation>> listeners = ConcurrentHashMap.newKeySet();
    private final Map<Path, Transformation> transformationConfigurations = new ConcurrentHashMap<>();
    private final Path transformationPath;
    public FileTransformationProvider(
        this.transformationPath = watchService.getWatchPath().resolve(TransformationService.TRANSFORM_FOLDER_NAME);
        watchService.registerListener(this, transformationPath);
        try (Stream<Path> files = Files.walk(transformationPath)) {
            files.filter(Files::isRegularFile).forEach(f -> processWatchEvent(CREATE, f));
            logger.warn("Could not list files in '{}', transformation configurations might be missing: {}",
                    transformationPath, e.getMessage());
        return transformationConfigurations.values();
        Path path = transformationPath.relativize(fullPath);
                Transformation oldElement = transformationConfigurations.remove(path);
                    logger.trace("Removed configuration from file '{}", path);
                    listeners.forEach(listener -> listener.removed(this, oldElement));
            } else if (Files.isRegularFile(fullPath) && !Files.isHidden(fullPath)
                    && ((kind == CREATE) || (kind == MODIFY))) {
                Matcher m = FILENAME_PATTERN.matcher(fileName);
                    logger.debug("Skipping {} event for '{}' - no file extensions found or remaining filename empty",
                            kind, path);
                String fileExtension = m.group("extension");
                if (IGNORED_EXTENSIONS.contains(fileExtension)) {
                    logger.debug("Skipping {} event for '{}' - file extension '{}' is ignored", kind, path,
                            fileExtension);
                String content = new String(Files.readAllBytes(fullPath));
                String uid = path.toString();
                Transformation newElement = new Transformation(uid, uid, fileExtension, Map.of(FUNCTION, content));
                Transformation oldElement = transformationConfigurations.put(path, newElement);
                    logger.trace("Added new configuration from file '{}'", path);
                    listeners.forEach(listener -> listener.added(this, newElement));
                    logger.trace("Updated new configuration from file '{}'", path);
                    listeners.forEach(listener -> listener.updated(this, oldElement, newElement));
                logger.trace("Skipping {} event for '{}' - not a regular file", kind, path);
            logger.warn("Skipping {} event for '{}' - failed to process it: {}", kind, path, e.getMessage());
