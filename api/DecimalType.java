import java.text.DecimalFormat;
import java.text.NumberFormat;
import java.text.ParsePosition;
import java.util.IllegalFormatConversionException;
 * The decimal type uses a BigDecimal internally and thus can be used for
 * integers, longs and floating point numbers alike.
public class DecimalType extends Number implements PrimitiveType, State, Command, Comparable<DecimalType> {
    private static final long serialVersionUID = 4226845847123464690L;
    protected static final BigDecimal BIG_DECIMAL_HUNDRED = BigDecimal.valueOf(100);
    public static final DecimalType ZERO = new DecimalType(0);
    protected BigDecimal value;
    public DecimalType() {
        this(BigDecimal.ZERO);
     * Creates a new {@link DecimalType} with the given value.
     * @param value a number
    public DecimalType(Number value) {
        if (value instanceof QuantityType type) {
            this.value = type.toBigDecimal();
        } else if (value instanceof BigDecimal decimal) {
            this.value = decimal;
            this.value = new BigDecimal(integer);
            this.value = new BigDecimal(value.toString());
     * The English locale is used to determine (decimal/grouping) separator characters.
     * @param value the value representing a number
     * @throws NumberFormatException when the number could not be parsed to a {@link BigDecimal}
    public DecimalType(String value) {
        this(value, Locale.ENGLISH);
     * @param locale the locale used to determine (decimal/grouping) separator characters
    public DecimalType(String value, Locale locale) {
        DecimalFormat df = (DecimalFormat) NumberFormat.getInstance(locale);
        df.setParseBigDecimal(true);
        ParsePosition position = new ParsePosition(0);
        BigDecimal parsedValue = (BigDecimal) df.parseObject(value.toUpperCase(locale), position);
        if (parsedValue == null || position.getErrorIndex() != -1 || position.getIndex() < value.length()) {
            throw new NumberFormatException("Invalid BigDecimal value: " + value);
        this.value = parsedValue;
        return value.toPlainString();
     * Static access to {@link DecimalType#DecimalType(String)}.
     * @param value the non null value representing a number
     * @return a new {@link DecimalType}
    public static DecimalType valueOf(String value) {
        return new DecimalType(value);
    public String format(String pattern) {
        // The value could be an integer value. Try to convert to BigInteger in
        // order to have access to more conversion formats.
            return String.format(pattern, value.toBigIntegerExact());
        } catch (ArithmeticException ae) {
            // Could not convert to integer value without loss of
            // information. Fall through to default behavior.
        } catch (IllegalFormatConversionException ifce) {
            // The conversion is not valid for the type BigInteger. This
            // happens, if the format is like "%.1f" but the value is an
            // integer. Fall through to default behavior.
        return String.format(pattern, value);
    public BigDecimal toBigDecimal() {
        result = prime * result + value.hashCode();
        if (!(obj instanceof DecimalType)) {
        DecimalType other = (DecimalType) obj;
        return value.compareTo(other.value) == 0;
    public int compareTo(DecimalType o) {
        return value.compareTo(o.toBigDecimal());
    public double doubleValue() {
        return value.doubleValue();
    public float floatValue() {
        return value.floatValue();
    public int intValue() {
        return value.intValue();
    public long longValue() {
        return value.longValue();
    protected <T extends State> @Nullable T defaultConversion(@Nullable Class<T> target) {
        return State.super.as(target);
    public <T extends State> @Nullable T as(@Nullable Class<T> target) {
        if (target == OnOffType.class) {
            return target.cast(OnOffType.from(!equals(ZERO)));
        } else if (target == PercentType.class) {
            return target.cast(new PercentType(toBigDecimal().multiply(BIG_DECIMAL_HUNDRED)));
        } else if (target == UpDownType.class) {
            if (equals(ZERO)) {
                return target.cast(UpDownType.UP);
            } else if (toBigDecimal().compareTo(BigDecimal.ONE) == 0) {
                return target.cast(UpDownType.DOWN);
        } else if (target == OpenClosedType.class) {
                return target.cast(OpenClosedType.CLOSED);
                return target.cast(OpenClosedType.OPEN);
        } else if (target == HSBType.class) {
            return target.cast(new HSBType(DecimalType.ZERO, PercentType.ZERO,
                    new PercentType(this.toBigDecimal().multiply(BIG_DECIMAL_HUNDRED))));
        } else if (target == DateTimeType.class) {
            return target.cast(new DateTimeType(value.toString()));
            return defaultConversion(target);
