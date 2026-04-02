 * The PercentType extends the {@link DecimalType} by putting constraints for its value on top (0-100).
public class PercentType extends DecimalType {
    private static final long serialVersionUID = -9066279845951780879L;
    public static final PercentType ZERO = new PercentType(0);
    public static final PercentType HUNDRED = new PercentType(100);
     * Creates a new {@link PercentType} with 0 as value.
    public PercentType() {
        this(0);
     * Creates a new {@link PercentType} with the given value.
     * @param value the value representing a percentage
     * @throws IllegalArgumentException when the value is not between 0 and 100
    public PercentType(int value) {
        super(value);
        validateValue(this.value);
     * @param value the non null value representing a percentage
    public PercentType(String value) {
    public PercentType(String value, Locale locale) {
        super(value, locale);
     * @param value the value representing a percentage.
    public PercentType(BigDecimal value) {
    private void validateValue(BigDecimal value) {
            throw new IllegalArgumentException("Value must be between 0 and 100");
     * Static access to {@link PercentType#PercentType(String)}.
     * @return new {@link PercentType}
    public static PercentType valueOf(String value) {
            return target.cast(new DecimalType(toBigDecimal().divide(BIG_DECIMAL_HUNDRED, 8, RoundingMode.UP)));
            } else if (equals(HUNDRED)) {
            return target.cast(new HSBType(DecimalType.ZERO, PercentType.ZERO, this));
        } else if (target == QuantityType.class) {
            return target.cast(new QuantityType<>(toBigDecimal(), Units.PERCENT));
