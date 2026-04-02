import static org.openhab.core.model.core.ModelCoreConstants.*;
import org.eclipse.emf.common.util.URI;
import org.eclipse.emf.ecore.resource.Resource;
import org.eclipse.emf.ecore.resource.Resource.Diagnostic;
import org.eclipse.emf.ecore.resource.ResourceSet;
import org.eclipse.emf.ecore.util.Diagnostician;
import org.eclipse.xtext.resource.SynchronizedXtextResourceSet;
import org.eclipse.xtext.resource.XtextResource;
import org.eclipse.xtext.resource.XtextResourceSet;
import org.openhab.core.model.core.EventType;
import org.openhab.core.model.core.ModelRepository;
import org.openhab.core.model.core.ModelRepositoryChangeListener;
import org.openhab.core.model.core.SafeEMF;
 * @author Oliver Libutzki - Added reloadAllModelsOfType method
 * @author Simon Kaufmann - added validation of models before loading them
 *         + return errors and warnings when loading a model
public class ModelRepositoryImpl implements ModelRepository {
    private final Logger logger = LoggerFactory.getLogger(ModelRepositoryImpl.class);
    private final ResourceSet resourceSet;
    private final Map<String, String> resourceOptions = Map.of(XtextResource.OPTION_ENCODING,
            StandardCharsets.UTF_8.name());
    private final List<ModelRepositoryChangeListener> listeners = new CopyOnWriteArrayList<>();
    private final SafeEMF safeEmf;
    public ModelRepositoryImpl(final @Reference SafeEMF safeEmf) {
        this.safeEmf = safeEmf;
        XtextResourceSet xtextResourceSet = new SynchronizedXtextResourceSet();
        xtextResourceSet.addLoadOption(XtextResource.OPTION_RESOLVE_ALL, Boolean.TRUE);
        this.resourceSet = xtextResourceSet;
        // don't use XMI as a default
        Resource.Factory.Registry.INSTANCE.getExtensionToFactoryMap().remove("*");
    public @Nullable EObject getModel(String name) {
        synchronized (resourceSet) {
            Resource resource = getResource(name);
                if (!resource.getContents().isEmpty()) {
                    return resource.getContents().getFirst();
                    logger.warn("DSL model '{}' is either empty or cannot be parsed correctly!", name);
                    resourceSet.getResources().remove(resource);
                logger.trace("DSL model '{}' can not be found", name);
    public boolean addOrRefreshModel(String name, final InputStream originalInputStream) {
        if (isIsolatedModel(name)) {
            logger.info("Ignoring DSL model '{}'", name);
        return addOrRefreshModel(name, originalInputStream, null, null);
    public boolean addOrRefreshModel(String name, final InputStream originalInputStream, @Nullable List<String> errors,
            @Nullable List<String> warnings) {
        logger.info("Loading DSL model '{}'", name);
        Resource resource = null;
        try (InputStream inputStream = originalInputStream) {
            bytes = inputStream.readAllBytes();
            List<String> newErrors = new ArrayList<>();
            List<String> newWarnings = new ArrayList<>();
            boolean valid = validateModel(name, new ByteArrayInputStream(bytes), newErrors, newWarnings);
            if (errors != null) {
                errors.addAll(newErrors);
            if (warnings != null) {
                warnings.addAll(newWarnings);
                logger.warn("DSL model '{}' has errors, therefore ignoring it: {}", name, String.join("\n", newErrors));
                removeResource(name);
            if (!newWarnings.isEmpty()) {
                logger.info("Validation issues found in DSL model '{}', using it anyway:\n{}", name,
                        String.join("\n", newWarnings));
                errors.add("Model cannot be parsed correctly: %s".formatted(e.getMessage()));
            logger.warn("DSL model '{}' cannot be parsed correctly!", name, e);
        try (InputStream inputStream = new ByteArrayInputStream(bytes)) {
            resource = getResource(name);
            if (resource == null) {
                    // try again to retrieve the resource as it might have been created by now
                        // seems to be a new file
                        resource = resourceSet.createResource(URI.createURI(name));
                            resource.load(inputStream, resourceOptions);
                            notifyListeners(name, EventType.ADDED);
                            logger.warn("Ignoring file '{}' as we do not have a parser for it.", name);
                    resource.unload();
                    notifyListeners(name, EventType.MODIFIED);
    public boolean removeModel(String name) {
        logger.info("Unloading DSL model '{}'", name);
        return removeResource(name);
    private boolean removeResource(String name) {
                // do not physically delete it, but remove it from the resource set
                notifyListeners(name, EventType.REMOVED);
    public Iterable<String> getAllModelNamesOfType(final String modelType) {
            // Make a copy to avoid ConcurrentModificationException
            List<Resource> resourceListCopy = new ArrayList<>(resourceSet.getResources());
            return resourceListCopy.stream()
                    .filter(input -> input.getURI().lastSegment().contains(".") && input.isLoaded()
                            && modelType.equalsIgnoreCase(input.getURI().fileExtension())
                            && !isIsolatedModel(input.getURI().lastSegment()))
                    .map(from -> from.getURI().path()).toList();
    public void reloadAllModelsOfType(final String modelType) {
            for (Resource resource : resourceListCopy) {
                if (resource.getURI().lastSegment().contains(".") && resource.isLoaded()
                        && modelType.equalsIgnoreCase(resource.getURI().fileExtension())
                        && !isIsolatedModel(resource.getURI().lastSegment())) {
                    XtextResource xtextResource = (XtextResource) resource;
                    // It's not sufficient to discard the derived state.
                    // The quick & dirts solution is to reparse the whole resource.
                    // We trigger this by dummy updating the resource.
                    logger.debug("Refreshing resource '{}'", resource.getURI().lastSegment());
                    xtextResource.update(1, 0, "");
                    notifyListeners(resource.getURI().lastSegment(), EventType.MODIFIED);
    public Set<String> removeAllModelsOfType(final String modelType) {
        Set<String> ret = new HashSet<>();
                    logger.debug("Removing resource '{}'", resource.getURI().lastSegment());
                    ret.add(resource.getURI().lastSegment());
                    notifyListeners(resource.getURI().lastSegment(), EventType.REMOVED);
    public void addModelRepositoryChangeListener(ModelRepositoryChangeListener listener) {
    public void removeModelRepositoryChangeListener(ModelRepositoryChangeListener listener) {
    public @Nullable String createIsolatedModel(String modelType, InputStream inputStream, List<String> errors,
            List<String> warnings) {
        String name = "%smodel_%d.%s".formatted(PREFIX_TMP_MODEL, ++counter, modelType);
        return addOrRefreshModel(name, inputStream, errors, warnings) ? name : null;
    public void generateFileFormat(OutputStream out, String modelType, EObject modelContent) {
            String name = "%sgenerated_%d.%s".formatted(PREFIX_TMP_MODEL, ++counter, modelType);
            Resource resource = resourceSet.createResource(URI.createURI(name));
                resource.getContents().add(modelContent);
                resource.save(out, Map.of(XtextResource.OPTION_ENCODING, StandardCharsets.UTF_8.name()));
                logger.warn("Exception when saving DSL model {}", resource.getURI().lastSegment());
    private @Nullable Resource getResource(String name) {
        return resourceSet.getResource(URI.createURI(name), false);
     * Validates the given model.
     * There are two "layers" of validation
     * <li>
     * errors when loading the resource. Usually these are syntax violations which irritate the parser. They will be
     * returned as a String.
     * all kinds of other errors (i.e. violations of validation checks) will only be logged, but not included in the
     * return value.
     * Validation will be done on a separate resource, in order to keep the original one intact in case its content
     * needs to be removed because of syntactical errors.
     * @param name the model name
     * @return false if any syntactical error were found, false otherwise
     * @throws IOException if there was an error with the given {@link InputStream}, loading the resource from there
    private boolean validateModel(String name, InputStream inputStream, List<String> errors, List<String> warnings)
        // use another resource for validation in order to keep the original one for emergency-removal in case of errors
        Resource resource = resourceSet.createResource(URI.createURI(PREFIX_TMP_MODEL + name));
                // Check for syntactical errors
                for (Diagnostic diagnostic : resource.getErrors()) {
                    errors.add(MessageFormat.format("[{0},{1}]: {2}", Integer.toString(diagnostic.getLine()),
                            Integer.toString(diagnostic.getColumn()), diagnostic.getMessage()));
                if (!resource.getErrors().isEmpty()) {
                // Check for validation errors, but log them only
                    String modelType = resource.getURI().fileExtension();
                    final org.eclipse.emf.common.util.Diagnostic diagnostic = safeEmf
                            .call(() -> Diagnostician.INSTANCE.validate(resource.getContents().getFirst()));
                    for (org.eclipse.emf.common.util.Diagnostic d : diagnostic.getChildren()) {
                        if (d.getSeverity() == org.eclipse.emf.common.util.Diagnostic.ERROR
                                && !"rules".equalsIgnoreCase(modelType) && !"script".equalsIgnoreCase(modelType)) {
                            errors.add(d.getMessage());
                            warnings.add(d.getMessage());
                    if (!errors.isEmpty()) {
                } catch (NullPointerException e) {
                    // see https://github.com/eclipse/smarthome/issues/3335
                    logger.debug("Validation of '{}' skipped due to internal errors.", name);
    private void notifyListeners(String name, EventType type) {
        for (ModelRepositoryChangeListener listener : listeners) {
            listener.modelChanged(name, type);
