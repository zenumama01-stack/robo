import javax.measure.UnconvertibleException;
 * The {@link PersistenceThresholdFilter} is a filter to prevent persistence based on a threshold.
 * The filter returns {@code false} if the new value deviates by less than {@link #value}. If unit is "%" is
 * {@code true}, the filter returns {@code false} if the relative deviation is less than {@link #value}.
public class PersistenceThresholdFilter extends PersistenceFilter {
    private static final BigDecimal HUNDRED = BigDecimal.valueOf(100);
    private final Logger logger = LoggerFactory.getLogger(PersistenceThresholdFilter.class);
    private final BigDecimal value;
    private final boolean relative;
    private final transient Map<String, State> valueCache = new HashMap<>();
    public PersistenceThresholdFilter(String name, BigDecimal value, @Nullable String unit,
            @Nullable Boolean relative) {
        this.relative = relative != null && relative;
    public BigDecimal getValue() {
    public boolean isRelative() {
        return relative;
        if (!(state instanceof DecimalType || state instanceof QuantityType)) {
        State cachedState = valueCache.get(itemName);
        if (cachedState == null || !state.getClass().equals(cachedState.getClass())) {
        if (state instanceof DecimalType decimalState) {
            BigDecimal oldState = ((DecimalType) cachedState).toBigDecimal();
            BigDecimal delta = oldState.subtract(decimalState.toBigDecimal());
            if (relative && !BigDecimal.ZERO.equals(oldState)) {
                delta = delta.multiply(HUNDRED).divide(oldState, 2, RoundingMode.HALF_UP);
            return delta.abs().compareTo(value) > 0;
                QuantityType oldState = (QuantityType) cachedState;
                QuantityType delta = oldState.subtract((QuantityType) state);
                if (relative) {
                    if (BigDecimal.ZERO.equals(oldState.toBigDecimal())) {
                        // value is different and old value is 0 -> always above relative threshold
                        // calculate percent
                        delta = delta.multiply(HUNDRED).divide(oldState);
                } else if (!unit.isBlank()) {
                    // consider unit only if not relative threshold
                    delta = delta.toUnitRelative(unit);
                    if (delta == null) {
                        throw new UnconvertibleException("");
                return delta.toBigDecimal().abs().compareTo(value) > 0;
            } catch (UnconvertibleException e) {
                logger.warn("Cannot compare {} to {}", cachedState, state);
        valueCache.put(item.getName(), item.getState());
        return String.format("%s [name=%s, value=%s, unit=%s, relative=%b]", getClass().getSimpleName(), getName(),
                value, unit, relative);
