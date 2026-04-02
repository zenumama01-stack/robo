package org.openhab.core.model.yaml.internal;
import static org.openhab.core.model.yaml.YamlModelUtils.*;
import java.nio.file.FileVisitResult;
import java.nio.file.SimpleFileVisitor;
import java.nio.file.attribute.BasicFileAttributes;
import org.openhab.core.io.dto.ModularDTO;
import org.openhab.core.io.dto.SerializationException;
import org.openhab.core.model.yaml.YamlElement;
import org.openhab.core.model.yaml.YamlElementName;
import org.openhab.core.model.yaml.YamlModelListener;
import org.openhab.core.model.yaml.YamlModelRepository;
import org.openhab.core.model.yaml.internal.items.YamlItemDTO;
import org.openhab.core.model.yaml.internal.rules.YamlRuleDTO;
import org.openhab.core.model.yaml.internal.rules.YamlRuleTemplateDTO;
import org.openhab.core.model.yaml.internal.semantics.YamlSemanticTagDTO;
import org.openhab.core.model.yaml.internal.things.YamlThingDTO;
import org.openhab.core.service.WatchService.Kind;
import com.fasterxml.jackson.annotation.JsonAutoDetect;
import com.fasterxml.jackson.annotation.JsonInclude.Include;
import com.fasterxml.jackson.annotation.PropertyAccessor;
import com.fasterxml.jackson.core.JsonGenerator;
import com.fasterxml.jackson.databind.node.JsonNodeFactory;
import com.fasterxml.jackson.databind.node.ObjectNode;
import com.fasterxml.jackson.dataformat.yaml.YAMLGenerator;
import com.fasterxml.jackson.dataformat.yaml.YAMLParser;
 * The {@link YamlModelRepositoryImpl} is an OSGi service, that encapsulates all YAML file processing
 * including file monitoring to detect created, updated and removed YAML configuration files.
 * Data processed from these files are consumed by registered OSGi services that implement {@link YamlModelListener}.
 * @author Jan N. Klug - Refactored for multiple types per file and add modifying possibility
 * @author Laurent Garnier - Map used instead of table
 * @author Laurent Garnier - new parameters to retrieve errors and warnings when loading a file
public class YamlModelRepositoryImpl implements WatchService.WatchEventListener, YamlModelRepository {
    private static final int DEFAULT_MODEL_VERSION = 1;
    private static final String VERSION = "version";
    private static final String READ_ONLY = "readOnly";
    private static final Set<String> KNOWN_ELEMENTS = Set.of( //
            getElementName(YamlRuleDTO.class), // "rules"
            getElementName(YamlRuleTemplateDTO.class), // "ruleTemplates"
            getElementName(YamlSemanticTagDTO.class), // "tags"
            getElementName(YamlThingDTO.class), // "things"
            getElementName(YamlItemDTO.class) // "items"
    private static final String UNWANTED_EXCEPTION_TEXT = "at [Source: UNKNOWN; byte offset: #UNKNOWN] ";
    private static final String UNWANTED_EXCEPTION_TEXT2 = "\\n \\(through reference chain: .*";
    private static final List<Path> WATCHED_PATHS = Stream.of("things", "items", "tags", "rules", "yaml").map(Path::of)
    private final Logger logger = LoggerFactory.getLogger(YamlModelRepositoryImpl.class);
    private final Path mainWatchPath;
    private final ObjectMapper objectMapper;
    private final Map<String, List<YamlModelListener<?>>> elementListeners = new ConcurrentHashMap<>();
    // all model nodes, ordered by model name (full path as string) and type
    private final Map<String, YamlModelWrapper> modelCache = new ConcurrentHashMap<>();
    private final Map<String, List<YamlElement>> elementsToGenerate = new ConcurrentHashMap<>();
    public YamlModelRepositoryImpl(@Reference(target = WatchService.CONFIG_WATCHER_FILTER) WatchService watchService) {
        YAMLFactory yamlFactory = YAMLFactory.builder() //
                .disable(YAMLGenerator.Feature.WRITE_DOC_START_MARKER) // omit "---" at file start
                .disable(YAMLGenerator.Feature.SPLIT_LINES) // do not split long lines
                .enable(YAMLGenerator.Feature.INDENT_ARRAYS_WITH_INDICATOR) // indent arrays
                .enable(YAMLGenerator.Feature.MINIMIZE_QUOTES) // use quotes only where necessary
                .enable(YAMLGenerator.Feature.ALWAYS_QUOTE_NUMBERS_AS_STRINGS) // use quotes for numbers stored as
                .enable(YAMLParser.Feature.PARSE_BOOLEAN_LIKE_WORDS_AS_STRINGS).build(); // do not parse ON/OFF/... as
                                                                                         // booleans
        this.objectMapper = new ObjectMapper(yamlFactory);
        objectMapper.findAndRegisterModules();
        objectMapper.setVisibility(PropertyAccessor.ALL, JsonAutoDetect.Visibility.NONE);
        objectMapper.setVisibility(PropertyAccessor.FIELD, JsonAutoDetect.Visibility.ANY);
        objectMapper.setSerializationInclusion(Include.NON_NULL);
        objectMapper.enable(JsonGenerator.Feature.WRITE_BIGDECIMAL_AS_PLAIN);
        this.mainWatchPath = watchService.getWatchPath();
        watchService.registerListener(this, WATCHED_PATHS);
        // read initial contents
        WATCHED_PATHS.forEach(watchPath -> {
            Path fullPath = mainWatchPath.resolve(watchPath);
            if (!Files.exists(fullPath)) {
            } else if (!Files.isDirectory(fullPath)) {
                logger.warn("Expecting '{}' to be a directory, but it's a file. Ignoring it.", fullPath);
                Files.walkFileTree(fullPath, new SimpleFileVisitor<>() {
                    public FileVisitResult visitFile(@NonNullByDefault({}) Path file,
                            @NonNullByDefault({}) BasicFileAttributes attrs) throws IOException {
                        if (attrs.isRegularFile()) {
                            processWatchEvent(CREATE, file);
                        return FileVisitResult.CONTINUE;
                    public FileVisitResult visitFileFailed(@NonNullByDefault({}) Path file,
                            @NonNullByDefault({}) IOException exc) throws IOException {
                        String message = exc.getMessage();
                        if (file.toString().equals(message)) {
                            // If the message is just the path, we do not need to log it again.
                            // This is the case for FileNotFoundException, AccessDeniedException, etc.
                            // Instead of the path, we log the exception class to provide additional details.
                            message = exc.getClass().getSimpleName();
                        logger.warn("Failed to process {}: {}", file.toAbsolutePath(), message);
                logger.warn("Could not list YAML files in '{}', models might be missing: {}", watchPath,
    // The method is "synchronized" to avoid concurrent files processing
    public synchronized void processWatchEvent(Kind kind, Path fullPath) {
        Path relativePath = mainWatchPath.relativize(fullPath);
        String modelName = relativePath.toString();
        if (!modelName.endsWith(".yaml") && !modelName.endsWith(".yml")) {
            logger.trace("Ignored {}", fullPath);
                removeModel(modelName);
            } else if (!Files.isHidden(fullPath) && Files.isReadable(fullPath) && !Files.isDirectory(fullPath)) {
                processModelContent(modelName, kind, objectMapper.readTree(fullPath.toFile()), errors, warnings);
            errors.add("Failed to process model: %s".formatted(e.getMessage()));
        errors.forEach(error -> {
            logger.warn("YAML model {}: {}", modelName, error);
        warnings.forEach(warning -> {
            logger.info("YAML model {}: {}", modelName, warning);
    private boolean processModelContent(String modelName, Kind kind, JsonNode fileContent, List<String> errors,
        // check version
        JsonNode versionNode = fileContent.get(VERSION);
        if (versionNode == null || !versionNode.canConvertToInt()) {
            errors.add("version is missing or not a number. Ignoring model.");
        int modelVersion = versionNode.asInt();
        if (modelVersion < 1 || modelVersion > DEFAULT_MODEL_VERSION) {
            errors.add("model has version %d, but only versions between 1 and %d are supported. Ignoring model."
                    .formatted(modelVersion, DEFAULT_MODEL_VERSION));
        if (kind == Kind.CREATE) {
            logger.info("Adding YAML model {}", modelName);
            logger.info("Updating YAML model {}", modelName);
        JsonNode readOnlyNode = fileContent.get(READ_ONLY);
        boolean readOnly = readOnlyNode == null || readOnlyNode.asBoolean(false);
        YamlModelWrapper model = Objects.requireNonNull(
                modelCache.computeIfAbsent(modelName, k -> new YamlModelWrapper(modelVersion, readOnly)));
        boolean valid = true;
        List<String> newElementNames = new ArrayList<>();
        // get sub-elements
        Iterator<Map.Entry<String, JsonNode>> it = fileContent.properties().iterator();
            Map.Entry<String, JsonNode> element = it.next();
            String elementName = element.getKey();
            if (elementName.equals(VERSION) || elementName.equals(READ_ONLY)) {
            newElementNames.add(elementName);
            JsonNode node = element.getValue();
            if (!node.isContainerNode() || node.isArray()) {
                // all processable sub-elements are container nodes (not array)
                logger.warn("Element {} in model {} is not a container object, ignoring it", elementName, modelName);
                if (getElementName(YamlSemanticTagDTO.class).equals(elementName) && node.isArray()) {
                            "Your YAML model {} contains custom tags with an old and now unsupported syntax. An upgrade of this model is required to upgrade to the new syntax. This can be done by running the upgrade tool.",
            JsonNode newNodeElements = node;
            JsonNode oldNodeElements = model.getNodes().get(elementName);
            for (YamlModelListener<?> elementListener : getElementListeners(elementName, modelVersion)) {
                Class<? extends YamlElement> elementClass = elementListener.getElementClass();
                List<String> errors2 = new ArrayList<>();
                List<String> warnings2 = new ArrayList<>();
                Map<String, ? extends YamlElement> oldElements = listToMap(
                        parseJsonMapNode(oldNodeElements, elementClass, null, null));
                Map<String, ? extends YamlElement> newElements = listToMap(
                        parseJsonMapNode(newNodeElements, elementClass, errors2, warnings2));
                valid &= errors2.isEmpty();
                errors.addAll(errors2);
                warnings.addAll(warnings2);
                List addedElements = newElements.values().stream().filter(e -> !oldElements.containsKey(e.getId()))
                List removedElements = oldElements.values().stream().filter(e -> !newElements.containsKey(e.getId()))
                List updatedElements = newElements.values().stream()
                        .filter(e -> oldElements.containsKey(e.getId()) && !e.equals(oldElements.get(e.getId())))
                if (elementListener.isDeprecated() && (!addedElements.isEmpty() || !updatedElements.isEmpty())) {
                    warnings.add(
                            "Element %s in version %d is still supported but deprecated, please consider migrating your model to a more recent version"
                                    .formatted(elementName, modelVersion));
                if (!addedElements.isEmpty()) {
                    elementListener.addedModel(modelName, addedElements);
                if (!removedElements.isEmpty()) {
                    elementListener.removedModel(modelName, removedElements);
                if (!updatedElements.isEmpty()) {
                    elementListener.updatedModel(modelName, updatedElements);
            // replace cache
            model.getNodes().put(elementName, newNodeElements);
        // remove removed elements
        model.getNodes().entrySet().removeIf(e -> {
            String elementName = e.getKey();
            if (newElementNames.contains(elementName)) {
            JsonNode removedNode = e.getValue();
            getElementListeners(elementName, modelVersion).forEach(listener -> {
                List removedElements = parseJsonMapNode(removedNode, listener.getElementClass(), null, null);
                listener.removedModel(modelName, removedElements);
        checkElementNames(modelName, model, warnings);
    private void removeModel(String modelName) {
        YamlModelWrapper removedModel = modelCache.remove(modelName);
        if (removedModel == null) {
        logger.info("Removing YAML model {}", modelName);
        int version = removedModel.getVersion();
        for (Map.Entry<String, @Nullable JsonNode> modelEntry : removedModel.getNodes().entrySet()) {
            String elementName = modelEntry.getKey();
            JsonNode removedMapNode = modelEntry.getValue();
            if (removedMapNode != null) {
                getElementListeners(elementName, version).forEach(listener -> {
                    List removedElements = parseJsonMapNode(removedMapNode, listener.getElementClass(), null, null);
    public void addYamlModelListener(YamlModelListener<? extends YamlElement> listener) {
        Class<? extends YamlElement> elementClass = listener.getElementClass();
        String elementName = getElementName(elementClass);
        Objects.requireNonNull(elementListeners.computeIfAbsent(elementName, k -> new CopyOnWriteArrayList<>()))
                .add(listener);
        // iterate over all models and notify the new listener of already existing models with this type
        modelCache.forEach((modelName, model) -> {
            int modelVersion = model.getVersion();
            if (!listener.isVersionSupported(modelVersion)) {
            if (listener.isDeprecated()) {
                        "Element {} in model {} version {} is still supported but deprecated, please consider migrating your model to a more recent version",
                        elementName, modelName, modelVersion);
            JsonNode modelMapNode = model.getNodes().get(elementName);
            if (modelMapNode == null) {
            List modelElements = parseJsonMapNode(modelMapNode, elementClass, errors, warnings);
            listener.addedModel(modelName, modelElements);
    public void removeYamlModelListener(YamlModelListener<? extends YamlElement> listener) {
        String elementName = getElementName(listener.getElementClass());
        elementListeners.computeIfPresent(elementName, (k, v) -> {
            v.remove(listener);
            return v.isEmpty() ? null : v;
    private void checkElementNames(String modelName, YamlModelWrapper model, List<String> warnings) {
        Set<String> elementListenerNames = elementListeners.keySet();
        if (elementListenerNames.containsAll(KNOWN_ELEMENTS)) {
            Set<String> modelElementNames = model.getNodes().keySet();
            modelElementNames.stream().filter(e -> !KNOWN_ELEMENTS.contains(e)).forEach(unknownElement -> {
                warnings.add("Element '%s' is unknown.".formatted(unknownElement));
    private static String getElementName(Class<? extends YamlElement> elementClass) {
        YamlElementName annotation = elementClass.getAnnotation(YamlElementName.class);
        if (annotation == null) {
            throw new IllegalStateException("Class " + elementClass.getName()
                    + " is missing the mandatory YamlElementName annotation. This is a bug.");
        return annotation.value();
    public void addElementToModel(String modelName, YamlElement element) {
        String elementName = getElementName(element.getClass());
        String id = element.getId();
                modelCache.computeIfAbsent(modelName, k -> new YamlModelWrapper(DEFAULT_MODEL_VERSION, false)));
        if (model.isReadOnly()) {
            logger.warn("Modifying {} is not allowed, model is marked read-only", modelName);
        JsonNode mapAddedNode = objectMapper.valueToTree(Map.of(id, element.cloneWithoutId()));
            model.getNodes().put(elementName, mapAddedNode);
            JsonNode newNode = objectMapper.convertValue(element.cloneWithoutId(), JsonNode.class);
            ((ObjectNode) modelMapNode).set(id, newNode);
        // notify listeners
        for (YamlModelListener<?> l : getElementListeners(elementName, model.getVersion())) {
            List newElements = parseJsonMapNode(mapAddedNode, l.getElementClass(), errors, warnings);
            if (!newElements.isEmpty()) {
                l.addedModel(modelName, newElements);
        writeModel(modelName);
    public void removeElementFromModel(String modelName, YamlElement element) {
        YamlModelWrapper model = modelCache.get(modelName);
            logger.warn("Failed to remove {} from model {} because the model is not known.", element, modelName);
            logger.warn("Failed to remove {} from model {} because type {} is not known in the model.", element,
                    modelName, elementName);
        if (!modelMapNode.has(id)) {
            logger.warn("Failed to remove {} from model {} because element is not in model.", element, modelName);
        ((ObjectNode) modelMapNode).remove(id);
        JsonNode mapRemovedNode = objectMapper.valueToTree(Map.of(id, element.cloneWithoutId()));
            List oldElements = parseJsonMapNode(mapRemovedNode, l.getElementClass(), null, null);
            if (!oldElements.isEmpty()) {
                l.removedModel(modelName, oldElements);
    public void updateElementInModel(String modelName, YamlElement element) {
            logger.warn("Failed to update {} in model {} because the model is not known.", element, modelName);
            logger.warn("Failed to update {} in model {} because type {} is not known in the model.", element,
            logger.warn("Failed to update {} in model {} because element is not in model.", element, modelName);
        JsonNode mapUpdatedNode = objectMapper.valueToTree(Map.of(id, element.cloneWithoutId()));
            List newElements = parseJsonMapNode(mapUpdatedNode, l.getElementClass(), errors, warnings);
                l.updatedModel(modelName, newElements);
    private void writeModel(String modelName) {
            logger.warn("Failed to write model {} to disk because it is not known.", modelName);
        if (isIsolatedModel(modelName)) {
            logger.warn("Failed to write model {} to disk because it is a temporary model.", modelName);
            logger.warn("Failed to write model {} to disk because it is marked as read-only.", modelName);
        // create the model
        JsonNodeFactory nodeFactory = objectMapper.getNodeFactory();
        ObjectNode rootNode = nodeFactory.objectNode();
        rootNode.put(VERSION, model.getVersion());
        rootNode.put(READ_ONLY, model.isReadOnly());
        for (Map.Entry<String, @Nullable JsonNode> elementNodes : model.getNodes().entrySet()) {
            if (elementNodes.getValue() != null) {
                rootNode.set(elementNodes.getKey(), elementNodes.getValue());
            Path outFile = mainWatchPath.resolve(modelName);
            String fileContent = objectMapper.writeValueAsString(rootNode);
            if (Files.exists(outFile) && !Files.isWritable(outFile)) {
                logger.warn("Failed writing model {}: model exists but is read-only.", modelName);
            Files.writeString(outFile, fileContent);
            logger.warn("Failed to serialize model {}: {}", modelName, e.getMessage());
            logger.warn("Failed writing model {}: {}", modelName, e.getMessage());
    public synchronized @Nullable String createIsolatedModel(InputStream inputStream, List<String> errors,
        String modelName = "%smodel_%d.yaml".formatted(PREFIX_TMP_MODEL, ++counter);
        boolean valid;
            valid = processModelContent(modelName, Kind.CREATE, objectMapper.readTree(inputStream), errors, warnings);
            logger.warn("Failed to process model {}: {}", modelName, e.getMessage());
            valid = false;
        return valid ? modelName : null;
    public void removeIsolatedModel(String modelName) {
    public void addElementsToBeGenerated(String id, List<YamlElement> elements) {
        List<YamlElement> elts = Objects.requireNonNull(elementsToGenerate.computeIfAbsent(id, k -> new ArrayList<>()));
        elts.addAll(elements);
    public void generateFileFormat(String id, OutputStream out) {
        List<YamlElement> elements = elementsToGenerate.remove(id);
        rootNode.put(VERSION, DEFAULT_MODEL_VERSION);
        // First separate elements per type
        Map<String, List<YamlElement>> elementsPerTypes = new HashMap<>();
        if (elements != null) {
            elements.forEach(element -> {
                List<YamlElement> elts = elementsPerTypes.get(elementName);
                if (elts == null) {
                    elts = new ArrayList<>();
                    elementsPerTypes.put(elementName, elts);
                elts.add(element);
        // Generate one entry for each element type
        elementsPerTypes.entrySet().forEach(entry -> {
            Map<String, YamlElement> mapElts = new LinkedHashMap<>();
            entry.getValue().forEach(elt -> {
                mapElts.put(elt.getId(), elt.cloneWithoutId());
            rootNode.set(entry.getKey(), objectMapper.valueToTree(mapElts));
            objectMapper.writeValue(out, rootNode);
            logger.warn("Failed to serialize model: {}", e.getMessage());
    private List<YamlModelListener<?>> getElementListeners(String elementName) {
        return elementListeners.getOrDefault(elementName, List.of());
    private List<YamlModelListener<?>> getElementListeners(String elementName, int version) {
        return getElementListeners(elementName).stream().filter(l -> l.isVersionSupported(version)).toList();
    private Map<String, ? extends YamlElement> listToMap(List<? extends YamlElement> elements) {
        LinkedHashMap<String, YamlElement> result = new LinkedHashMap<>(elements.size());
        for (YamlElement element : elements) {
            result.put(element.getId(), element);
    private <T extends YamlElement> List<T> parseJsonMapNode(@Nullable JsonNode mapNode, Class<T> elementClass,
            @Nullable List<String> errors, @Nullable List<String> warnings) {
        List<T> elements = new ArrayList<>();
        if (mapNode != null) {
            Iterator<String> it = mapNode.fieldNames();
                String id = it.next();
                T elt = null;
                JsonNode node = mapNode.get(id);
                if ((node.isContainerNode() && node.isEmpty()) || node.isNull()
                        || (node.isTextual() && node.asText().isBlank())) {
                    elt = createElement(elementClass, errors);
                    if (elt != null) {
                        elt.setId(id);
                        if (ModularDTO.class.isAssignableFrom(elementClass)) {
                            elt = modularToDto(node, elementClass, errors);
                            elt = objectMapper.treeToValue(node, elementClass);
                    } catch (JsonProcessingException | SerializationException e) {
                            String msg = e.getMessage();
                            errors.add("could not parse element with ID %s to %s: %s".formatted(id,
                                    elementClass.getSimpleName(),
                                    msg == null ? ""
                                            : msg.replace(UNWANTED_EXCEPTION_TEXT, "")
                                                    .replaceAll(UNWANTED_EXCEPTION_TEXT2, "")));
                if (elt != null && elt.isValid(errors, warnings)) {
                    elements.add(elt);
    private <T extends YamlElement> @Nullable T modularToDto(JsonNode node, Class<T> elementClass,
            @Nullable List<String> errors) throws SerializationException {
        T result = createElement(elementClass, errors);
            result = (T) ((ModularDTO<?, ObjectMapper, JsonNode>) result).toDto(node, objectMapper);
    private <T extends YamlElement> @Nullable T createElement(Class<T> elementClass, @Nullable List<String> errors) {
            result = elementClass.getDeclaredConstructor().newInstance();
        } catch (InstantiationException | IllegalAccessException | IllegalArgumentException | InvocationTargetException
                | NoSuchMethodException | SecurityException e) {
                errors.add("could not create new instance of %s".formatted(elementClass.getSimpleName()));
