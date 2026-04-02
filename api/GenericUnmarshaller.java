import com.thoughtworks.xstream.converters.MarshallingContext;
import com.thoughtworks.xstream.io.HierarchicalStreamWriter;
 * The {@link GenericUnmarshaller} is an abstract implementation of the {@code XStream} {@link Converter} interface used
 * to convert XML tags within an XML document into its
 * according objects.
 * <b>Hint:</b> This class only supports unmarshalling functionality.
public abstract class GenericUnmarshaller<T> implements Converter {
    private Class<T> clazz;
     * @param clazz the class of the result type (must not be null)
    public GenericUnmarshaller(Class<T> clazz) {
     * Returns the class of the result type
     * @return the class of the result type
    public Class<?> getResultType() {
        return this.clazz;
    public final boolean canConvert(@Nullable Class paramClass) {
        return (clazz.equals(paramClass));
    public final void marshal(@Nullable Object value, HierarchicalStreamWriter writer, MarshallingContext context) {
     * Checks that the specified object is not {@code null} and throws a customized {@link ConversionException} if it
     * is.
     * @param object the object to check for nullity
     * @param message detail message to be used in the event that a {@code ConversionException} is thrown
     * @return {@code object} if not {@code null}
     * @throws ConversionException if {@code object} is {@code null}
    protected static Object requireNonNull(@Nullable Object object, String message) {
            throw new ConversionException(message);
     * Checks that the specified string is not {@code null} and not empty and throws a customized
     * {@link ConversionException} if it is.
     * @param string the string to check for nullity and emptiness
     * @return {@code string} if not {@code null} and not empty
     * @throws ConversionException if {@code string} is {@code null} or empty
    protected static String requireNonEmpty(@Nullable String string, String message) {
        if (string == null || string.isEmpty()) {
