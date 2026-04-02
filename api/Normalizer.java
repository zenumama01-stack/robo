 * A {@link Normalizer} tries to normalize a given value according to the {@link ConfigDescriptionParameter.Type}
 * definition of a config description parameter. As an example a boolean normalizer would convert a given numeric value
 * 0 to false and a given numeric value 1 to true.
 * @author Thomas Höfer - renamed from INormalizer and minor javadoc changes
public interface Normalizer {
     * Normalizes the given object to the expected type, if possible. The expected type is defined by the
     * {@link ConfigDescriptionParameter.Type} of the corresponding config description parameter.
     * @param value the object to be normalized
     * @return the well-defined type or the given object, if it was not possible to convert it
    Object normalize(@Nullable Object value);
