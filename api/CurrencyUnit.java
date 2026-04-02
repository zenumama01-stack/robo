import javax.measure.Prefix;
import org.openhab.core.internal.library.unit.CurrencyConverter;
import org.openhab.core.internal.library.unit.CurrencyService;
import tech.units.indriya.function.RationalNumber;
import tech.units.indriya.unit.UnitDimension;
 * The {@link CurrencyUnit} is a UoM compatible unit for currencies.
@NonNullByDefault({ PARAMETER, RETURN_TYPE, FIELD, TYPE_BOUND })
public final class CurrencyUnit extends AbstractUnit<Currency> {
    private static final Dimension DIMENSION = UnitDimension.parse('$');
    private @Nullable String symbol;
     * Create a new <code>Currency</code>
     * @param name 3-letter ISO-Code
     * @param symbol an (optional) symbol
     * @throws IllegalArgumentException if name is not valid
    public CurrencyUnit(String name, @Nullable String symbol) throws IllegalArgumentException {
        if (name.length() != 3) {
            throw new IllegalArgumentException("Only three characters allowed for currency name");
    public UnitConverter getSystemConverter() {
        return internalGetConverterTo(getSystemUnit());
    protected Unit<Currency> toSystemUnit() {
        return BASE_CURRENCY;
    public @NonNullByDefault({}) Map<? extends Unit<?>, Integer> getBaseUnits() {
        return DIMENSION;
    public void setName(@NonNullByDefault({}) String name) {
    public @Nullable String getSymbol() {
    public void setSymbol(@Nullable String s) {
        this.symbol = s;
    public Unit<Currency> shift(double offset) {
        return shift(RationalNumber.of(offset));
    public Unit<Currency> multiply(double multiplier) {
        return multiply(RationalNumber.of(multiplier));
    public Unit<Currency> divide(double divisor) {
        return divide(RationalNumber.of(divisor));
    private UnitConverter internalGetConverterTo(Unit<Currency> that) throws UnconvertibleException {
        if (this.equals(that)) {
            return AbstractConverter.IDENTITY;
        if (BASE_CURRENCY.equals(this)) {
            BigDecimal factor = CurrencyService.FACTOR_FCN.apply(that);
            if (factor != null) {
                return new CurrencyConverter(factor);
        } else if (BASE_CURRENCY.equals(that)) {
            BigDecimal factor = CurrencyService.FACTOR_FCN.apply(this);
                return new CurrencyConverter(factor).inverse();
            BigDecimal f1 = CurrencyService.FACTOR_FCN.apply(this);
            BigDecimal f2 = CurrencyService.FACTOR_FCN.apply(that);
            if (f1 != null && f2 != null) {
                return new CurrencyConverter(f2.divide(f1, MathContext.DECIMAL128));
        throw new UnconvertibleException(
                "Could not get factor for converting " + this.getName() + " to " + that.getName());
    public Unit<?> pow(int n) {
        if (n > 0) {
            return this.multiply(this.pow(n - 1));
        } else if (n == 0) {
            return ONE;
            // n < 0
            return ONE.divide(this.pow(-n));
    public Unit<Currency> prefix(@NonNullByDefault({}) Prefix prefix) {
        return this.transform(MultiplyConverter.ofPrefix(prefix));
    public int compareTo(Unit<Currency> that) {
        int nameCompare = getName().compareTo(that.getName());
        String thatSymbol = that.getSymbol();
        if (symbol instanceof String localSymbol && thatSymbol != null) {
            return localSymbol.compareTo(thatSymbol);
        } else if (symbol != null) {
        } else if (thatSymbol != null) {
    public boolean isEquivalentTo(@NonNullByDefault({}) Unit<Currency> that) {
        return this.getConverterTo(that).isIdentity();
        if (obj instanceof CurrencyUnit that) {
            return (name.equals(that.name) && Objects.equals(symbol, that.symbol));
        return Objects.hash(name, symbol);
