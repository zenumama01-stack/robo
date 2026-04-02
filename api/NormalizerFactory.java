 * The {@link NormalizerFactory} can be used in order to obtain the {@link Normalizer} for any concrete
 * {@link ConfigDescriptionParameter.Type}.
 * @author Thomas Höfer - introduced normalizers map and added precondition check as well as some additional javadoc
public final class NormalizerFactory {
    private static final Normalizer BOOLEAN_NORMALIZER = new BooleanNormalizer();
    private static final Normalizer TEXT_NORMALIZER = new TextNormalizer();
    private static final Normalizer INT_NORMALIZER = new IntNormalizer();
    private static final Normalizer DECIMAL_NORMALIZER = new DecimalNormalizer();
    private NormalizerFactory() {
     * Returns the {@link Normalizer} for the type of the given config description parameter.
     * @param configDescriptionParameter the config description parameter (must not be null)
     * @return the corresponding {@link Normalizer} (not null)
     * @throws IllegalArgumentException if the given config description parameter is null
    public static Normalizer getNormalizer(@Nullable ConfigDescriptionParameter configDescriptionParameter) {
        if (configDescriptionParameter == null) {
            throw new IllegalArgumentException("The config description parameter must not be null.");
        Normalizer ret = getNormalizer(configDescriptionParameter.getType());
        return configDescriptionParameter.isMultiple() ? new ListNormalizer(ret) : ret;
     * Returns the {@link Normalizer} for the given ConfigDescriptionParameter type.
     * @param type the type
    public static Normalizer getNormalizer(Type type) {
            case BOOLEAN -> BOOLEAN_NORMALIZER;
            case DECIMAL -> DECIMAL_NORMALIZER;
            case INTEGER -> INT_NORMALIZER;
            case TEXT -> TEXT_NORMALIZER;
