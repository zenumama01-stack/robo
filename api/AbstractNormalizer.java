package org.openhab.core.config.core.internal.normalization;
 * Common base class for all normalizers, doing the specific type conversion.
 * @author Thomas Höfer - renamed normalizer interface and added javadoc
abstract class AbstractNormalizer implements Normalizer {
    protected final Logger logger = LoggerFactory.getLogger(AbstractNormalizer.class);
    public final @Nullable Object normalize(@Nullable Object value) {
        if (value instanceof String && "".equals(value)) {
        return doNormalize(value);
     * Executes the concrete normalization of the given value.
     * @param value the value to be normalized
     * @return the normalized value or the given value, if it was not possible to normalize it
    abstract Object doNormalize(Object value);
