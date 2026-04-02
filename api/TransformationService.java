 * A TransformationProcessor transforms a given input and returns the transformed
 * result. Transformations could make sense in various situations, for example:
 * <li>extract certain informations from a weather forecast website</li>
 * <li>extract the status of your TV which provides it's status on a webpage</li>
 * <li>postprocess the output from a serial device to be human readable</li>
 * One could provide his own processors by providing a new implementation of this
 * Interface.
public interface TransformationService {
    String SERVICE_PROPERTY_NAME = "openhab.transform";
    String SERVICE_PROPERTY_LABEL = "openhab.transform.label";
    String TRANSFORM_FOLDER_NAME = "transform";
    String TRANSFORM_PROFILE_SCOPE = "transform";
     * Transforms the input <code>source</code> by means of the given <code>function</code> and returns the transformed
     * output. The transformation may return <code>null</code> to express its operation resulted in a <code>null</code>
     * output. In case of any error a {@link TransformationException} should be thrown.
     * @param function the function to be used to transform the input
     * @param source the input to be transformed
     * @return the transformed result or <code>null</code> if the
     *         transformation's output is <code>null</code>.
     * @throws TransformationException if any error occurs
    String transform(String function, String source) throws TransformationException;
