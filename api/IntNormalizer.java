 * The normalizer for {@link ConfigDescriptionParameter.Type#INTEGER}. All different number formats will get converted
 * to BigDecimal, not allowing any fractions. Also, {@link String}s will be converted if possible.
final class IntNormalizer extends AbstractNormalizer {
                case BigDecimal bigDecimalValue -> bigDecimalValue.setScale(0, RoundingMode.UNNECESSARY);
                case Long longValue -> BigDecimal.valueOf(longValue);
                case String stringValue -> new BigDecimal(stringValue).setScale(0, RoundingMode.UNNECESSARY);
                case Float floatValue -> new BigDecimal(floatValue.toString()).setScale(0, RoundingMode.UNNECESSARY);
                case Double doubleValue -> BigDecimal.valueOf(doubleValue).setScale(0, RoundingMode.UNNECESSARY);
                    logger.trace("Class \"{}\" cannot be converted to an integer number.", value.getClass().getName());
            logger.trace("\"{}\" is not a valid integer number.", value, e);
