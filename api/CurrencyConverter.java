package org.openhab.core.internal.library.unit;
import javax.measure.UnitConverter;
import tech.units.indriya.function.AbstractConverter;
import tech.units.indriya.function.Calculus;
 * The {@link CurrencyConverter} implements an {@link UnitConverter} for
 * {@link org.openhab.core.library.unit.CurrencyUnit}
public class CurrencyConverter extends AbstractConverter {
    private final BigDecimal factor;
    public CurrencyConverter(BigDecimal factor) {
        this.factor = factor;
    public boolean equals(@Nullable Object cvtr) {
        return cvtr instanceof CurrencyConverter currencyConverter && factor.equals(currencyConverter.factor);
        return Objects.hashCode(factor);
    protected @Nullable String transformationLiteral() {
    protected AbstractConverter inverseWhenNotIdentity() {
        return new CurrencyConverter(BigDecimal.ONE.divide(factor, MathContext.DECIMAL128));
    protected boolean canReduceWith(@Nullable AbstractConverter that) {
    protected Number convertWhenNotIdentity(@NonNullByDefault({}) Number value) {
        return new BigDecimal(value.toString()).multiply(factor, MathContext.DECIMAL128);
    public int compareTo(@Nullable UnitConverter o) {
        return o instanceof CurrencyConverter currencyConverter ? factor.compareTo(currencyConverter.factor) : -1;
    public boolean isIdentity() {
    public boolean isLinear() {
     * This is currently necessary because conversion of {@link tech.units.indriya.unit.ProductUnit}s requires a
     * converter that is properly registered. This is currently not possible. We can't use the registered providers,
     * because they only have package-private constructors.
     * {@see https://github.com/unitsofmeasurement/indriya/issues/402}
        // call to ensure map is initialized
        Map<Class<? extends AbstractConverter>, Integer> unused = (Map<Class<? extends AbstractConverter>, Integer>) Calculus
                .getNormalFormOrder();
            Field field = Calculus.class.getDeclaredField("normalFormOrder");
            Map<Class<? extends AbstractConverter>, Integer> original = (Map<Class<? extends AbstractConverter>, Integer>) field
                    .get(null);
            Objects.requireNonNull(original).put(CurrencyConverter.class, 1000);
        } catch (NoSuchFieldException | IllegalAccessException e) {
            throw new IllegalStateException("Could not add currency converter", e);
