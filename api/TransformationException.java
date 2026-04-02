 * The {@link TransformationException} is a clone of {@link org.openhab.core.transform.TransformationException} to
 * make it available to DSL rules
public class TransformationException extends Exception {
    private static final long serialVersionUID = -1L;
    public TransformationException(@Nullable String message) {
    public TransformationException(@Nullable String message, @Nullable Throwable cause) {
 * A TransformationException is thrown when any step of a transformation went
 * wrong. The originating exception should be attached to increase traceability.
    /** generated serial Version UID */
    private static final long serialVersionUID = -535237375844795145L;
    public TransformationException(String message) {
    public TransformationException(String message, Throwable cause) {
