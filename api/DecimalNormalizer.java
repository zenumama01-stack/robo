 * The normalizer for {@link ConfigDescriptionParameter.Type#DECIMAL}. It converts all number types to BigDecimal,
 * having at least one digit after the floating point. Also {@link String}s are converted if possible.
final class DecimalNormalizer extends AbstractNormalizer {
                case BigDecimal bigDecimalValue -> stripTrailingZeros(bigDecimalValue);
                case String stringValue -> stripTrailingZeros(new BigDecimal(stringValue));
                case Byte byteValue -> new BigDecimal(byteValue);
                case Short shortValue -> new BigDecimal(shortValue);
                case Integer integerValue -> new BigDecimal(integerValue);
                case Long longValue -> new BigDecimal(longValue);
                case Float floatValue -> new BigDecimal(floatValue.toString());
                case Double doubleValue -> BigDecimal.valueOf(doubleValue);
                    logger.trace("Class \"{}\" cannot be converted to a decimal number.", value.getClass().getName());
        } catch (ArithmeticException | NumberFormatException e) {
            logger.trace("\"{}\" is not a valid decimal number.", value, e);
    private BigDecimal stripTrailingZeros(BigDecimal value) {
        BigDecimal ret = value;
        if (ret.scale() > 1) {
            ret = ret.stripTrailingZeros();
            if (ret.scale() == 0) {
                ret = ret.setScale(1);
