 * The normalizer for the {@link ConfigDescriptionParameter.Type#TEXT}. It basically ensures that the given value will
 * turned into its {@link String} representation.
final class TextNormalizer extends AbstractNormalizer {
        return value instanceof String ? value : value.toString();
