 * The {@link PersistenceIncludeFilter} is a filter that allows only specific values to pass
public class PersistenceIncludeFilter extends PersistenceFilter {
    private final Logger logger = LoggerFactory.getLogger(PersistenceIncludeFilter.class);
    private final BigDecimal lower;
    private final BigDecimal upper;
    private final String unit;
    public PersistenceIncludeFilter(String name, BigDecimal lower, BigDecimal upper, @Nullable String unit,
            @Nullable Boolean inverted) {
        this.lower = lower;
        this.upper = upper;
        this.unit = (unit == null) ? "" : unit;
    public BigDecimal getLower() {
        return lower;
    public BigDecimal getUpper() {
        return upper;
    public String getUnit() {
        BigDecimal compareValue = null;
        if (state instanceof DecimalType decimalType) {
            compareValue = decimalType.toBigDecimal();
        } else if (state instanceof QuantityType<?> quantityType) {
            if (!unit.isBlank()) {
                QuantityType<?> convertedQuantity = quantityType.toUnit(unit);
                if (convertedQuantity != null) {
                    compareValue = convertedQuantity.toBigDecimal();
        if (compareValue == null) {
            logger.warn("Cannot compare {} to range {}{} - {}{} ", state, lower, unit, upper, unit);
        if (inverted) {
            return compareValue.compareTo(lower) <= 0 || compareValue.compareTo(upper) >= 0;
            return compareValue.compareTo(lower) >= 0 && compareValue.compareTo(upper) <= 0;
        return String.format("%s [name=%s, lower=%s, upper=%s, unit=%s, inverted=%b]", getClass().getSimpleName(),
                getName(), lower, upper, unit, inverted);
