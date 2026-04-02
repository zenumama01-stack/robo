 * The {@link Transformation} is a wrapper for the {@link org.openhab.core.transform.actions.Transformation} class to
 * allow DSL rules to properly use the {@link TransformationException}
public class Transformation {
    public static @Nullable String transform(String type, String function, String value) {
        return org.openhab.core.transform.actions.Transformation.transform(type, function, value);
    public static @Nullable String transformRaw(String type, String function, String value)
            throws TransformationException {
            return org.openhab.core.transform.actions.Transformation.transformRaw(type, function, value);
        } catch (org.openhab.core.transform.TransformationException e) {
            throw new TransformationException(e.getMessage(), e.getCause());
 * The {@link Transformation} encapsulates a transformation configuration
public class Transformation implements Identifiable<String> {
    public static final String FUNCTION = "function";
    private final Map<String, String> configuration;
     * @param uid the configuration UID. The format is config:&lt;type&gt;:&lt;name&gt;[:&lt;locale&gt;]. For backward
     *            compatibility also filenames are allowed.
     * @param type the type of the configuration (file extension for file-based providers)
     * @param configuration the configuration (containing e.g. the transformation function)
    public Transformation(String uid, String label, String type, Map<String, String> configuration) {
    public Map<String, String> getConfiguration() {
        Transformation that = (Transformation) o;
        return uid.equals(that.uid) && label.equals(that.label) && type.equals(that.type)
                && configuration.equals(that.configuration);
        return Objects.hash(uid, label, type, configuration);
        return "TransformationConfiguration{uid='" + uid + "', label='" + label + "', type='" + type
                + "', configuration='" + configuration + "'}";
package org.openhab.core.transform.actions;
 * This class holds static "action" methods that can be used from within rules to execute
 * transformations.
    private static @Nullable String trans(String type, String function, String value) throws TransformationException {
        String result;
        TransformationService service = TransformationHelper.getTransformationService(type);
            result = service.transform(function, value);
            throw new TransformationException("No transformation service '" + type + "' could be found.");
     * Applies a transformation of a given type with some function to a value.
     * @param type the transformation type, e.g. REGEX or MAP
     * @param function the function to call, this value depends on the transformation type
     * @param value the value to apply the transformation to
     * @return the transformed value or the original one, if there was no service registered for the
     *         given type or a transformation exception occurred.
        Logger logger = LoggerFactory.getLogger(Transformation.class);
            result = trans(type, function, value);
            logger.debug("Error executing the transformation '{}': {}", type, e.getMessage());
     * @return the transformed value
     * @throws TransformationException, if there was no service registered for the
     *             given type or a transformation exception occurred
        return trans(type, function, value);
