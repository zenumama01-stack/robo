 * The {@link ScriptEngineFactoryHelper} contains helper methods for handling script engines
public class ScriptEngineFactoryHelper {
    private static final Logger LOGGER = LoggerFactory.getLogger(ScriptEngineFactoryHelper.class);
    private ScriptEngineFactoryHelper() {
        // prevent instantiation of static utility class
    public static Map.@Nullable Entry<String, String> getParameterOption(ScriptEngineFactory engineFactory) {
        List<String> scriptTypes = engineFactory.getScriptTypes();
        if (!scriptTypes.isEmpty()) {
            ScriptEngine scriptEngine = engineFactory.createScriptEngine(scriptTypes.getFirst());
            if (scriptEngine != null) {
                Map.Entry<String, String> parameterOption = Map.entry(getPreferredMimeType(engineFactory),
                        getLanguageName(scriptEngine.getFactory()));
                LOGGER.trace("ParameterOptions: {}", parameterOption);
                return parameterOption;
                LOGGER.trace("setScriptEngineFactory: engine was null");
            LOGGER.trace("addScriptEngineFactory: scriptTypes was empty");
    public static String getPreferredMimeType(ScriptEngineFactory factory) {
        List<String> scriptTypes = factory.getScriptTypes();
        if (scriptTypes.isEmpty()) {
                    factory.getClass().getName() + " does not support any scriptTypes. Please report it as a bug.");
        List<String> mimeTypes = new ArrayList<>(scriptTypes);
        mimeTypes.removeIf(mimeType -> !mimeType.contains("application") || "application/python".equals(mimeType));
        return mimeTypes.isEmpty() ? scriptTypes.getFirst() : mimeTypes.getFirst();
    public static String getLanguageName(javax.script.ScriptEngineFactory factory) {
        return String.format("%s (%s)",
                factory.getLanguageName().substring(0, 1).toUpperCase() + factory.getLanguageName().substring(1),
                factory.getLanguageVersion());
    public static Optional<String> getPreferredExtension(ScriptEngineFactory factory) {
        return factory.getScriptTypes().stream().filter(type -> !type.contains("/"))
                .min(Comparator.comparing(String::length));
