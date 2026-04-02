 * This interface is a container for dimension based functions that require {@link QuantityType}s for its calculations.
 * @author Andrew Fiddian-Green - Normalise calculations based on the Unit of the GroupItem
public interface QuantityTypeArithmeticGroupFunction extends GroupFunction {
    abstract class DimensionalGroupFunction implements GroupFunction {
        protected final Unit<?> baseItemUnit; // the actual unit of the owning group item
        protected final Unit<?> systemUnit; // the reference unit for group member calculations
        public DimensionalGroupFunction(Unit<?> baseItemUnit) {
            this.baseItemUnit = baseItemUnit;
            this.systemUnit = baseItemUnit.getSystemUnit();
                if (state instanceof QuantityType<?> quantity) {
                    state = quantity.toInvertibleUnit(baseItemUnit);
         * Convert the given item {@link State} to a {@link QuantityType} based on the {@link Unit} of the
         * {@link GroupItem} i.e. 'referenceUnit'. Returns null if the {@link State} is not a {@link QuantityType} or
         * if the {@link QuantityType} could not be converted to 'referenceUnit'.
         * The conversion can be made to both inverted and non-inverted units, so invertible type conversions (e.g.
         * Mirek <=> Kelvin) are supported.
         * @param state the State of any given group member item
         * @return a QuantityType or null
        private @Nullable QuantityType<?> toQuantityTypeOfUnit(@Nullable State state, Unit<?> unit) {
            return state instanceof QuantityType<?> quantity //
                    ? quantity.toInvertibleUnit(unit)
         * Convert a set of {@link Item} to a respective list of {@link QuantityType}. Exclude any {@link Item}s whose
         * current {@link State} is not a {@link QuantityType}. Convert any remaining {@link QuantityType} to the
         * 'referenceUnit' and exclude any values that did not convert.
         * @param items a list of {@link Item}
         * @return a list of {@link QuantityType} converted to the 'referenceUnit'
        protected List<QuantityType> toQuantityTypesOfUnit(Set<Item> items, Unit<?> unit) {
            return items.stream().map(i -> i.getState()).map(s -> toQuantityTypeOfUnit(s, unit))
                    .filter(Objects::nonNull).map(s -> (QuantityType) s).toList();
     * Calculates the average of a set of item states whose value could be converted to the 'referenceUnit'.
    class Avg extends DimensionalGroupFunction {
        public Avg(Unit<?> baseItemUnit) {
            super(baseItemUnit);
                List<QuantityType> systemUnitQuantities = toQuantityTypesOfUnit(items, systemUnit);
                if (!systemUnitQuantities.isEmpty()) {
                    return systemUnitQuantities.stream().reduce(new QuantityType<>(0, systemUnit), QuantityType::add)
                            .divide(BigDecimal.valueOf(systemUnitQuantities.size()));
     * Calculates the median of a set of item states whose value could be converted to the 'referenceUnit'.
    class Median extends DimensionalGroupFunction {
        public Median(Unit<?> baseItemUnit) {
                BigDecimal median = Statistics
                        .median(toQuantityTypesOfUnit(items, systemUnit).stream().map(q -> q.toBigDecimal()).toList());
                    return new QuantityType<>(median, systemUnit);
     * Calculates the sum of a set of item states whose value could be converted to the 'referenceUnit'.
     * Uses the {@link QuantityType#add} method so the result is an incremental sum based on the 'referenceUnit'. As
     * a general rule this class is instantiated with a 'referenceUnit' that is a "system unit" (which are zero based)
     * so such incremental sum is in fact also an absolute sum. However the class COULD be instantiated with a "non-
     * system unit" (e.g. °C, °F) in which case the result would be an incremental sum based on that unit.
    class Sum extends DimensionalGroupFunction {
        public Sum(Unit<?> baseItemUnit) {
                List<QuantityType> systemUnitQuantities = toQuantityTypesOfUnit(items, baseItemUnit);
                    return systemUnitQuantities.stream().reduce(new QuantityType<>(0, baseItemUnit), QuantityType::add);
     * Calculates the minimum of a set of item states whose value could be converted to the 'referenceUnit'.
    class Min extends DimensionalGroupFunction {
        public Min(Unit<?> baseItemUnit) {
                Optional<QuantityType> min = toQuantityTypesOfUnit(items, systemUnit).stream()
                        .min(QuantityType::compareTo);
                if (min.isPresent()) {
                    return min.get();
     * Calculates the maximum of a set of item states whose value could be converted to the 'referenceUnit'.
    class Max extends DimensionalGroupFunction {
        public Max(Unit<?> targetUnit) {
            super(targetUnit);
                Optional<QuantityType> max = toQuantityTypesOfUnit(items, systemUnit).stream()
                        .max(QuantityType::compareTo);
                if (max.isPresent()) {
                    return max.get();
