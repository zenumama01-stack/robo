 * This class holds all important constants relevant for this bundle.
public class ModelCoreConstants {
    /** The service pid used for the managed service (without the "org.openhab.core" prefix */
    public static final String SERVICE_PID = "folder";
    public static final String PREFIX_TMP_MODEL = "___tmp_";
     * Indicates if a model is an isolated model
     * An isolated model is a temporary model loaded without impacting any object registry.
     * @param modelName the model name
     * @return true if the model identified by the provided name is an isolated model, false otherwise
    public static boolean isIsolatedModel(String modelName) {
        return modelName.startsWith(PREFIX_TMP_MODEL);
