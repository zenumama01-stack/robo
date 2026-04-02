package org.openhab.core.model.script.lib;
import javax.measure.quantity.Dimensionless;
import javax.measure.quantity.Length;
import org.junit.jupiter.params.provider.ValueSource;
 * @author Fabio Marini - Initial contribution
public class NumberExtensionsTest {
    private static final DecimalType DECIMAL1 = new DecimalType(1);
    private static final DecimalType DECIMAL2 = new DecimalType(2);
    private static final QuantityType<Temperature> Q_CELSIUS_1 = new QuantityType<>("1 °C");
    private static final QuantityType<Temperature> Q_CELSIUS_2 = new QuantityType<>("2 °C");
    private static final QuantityType<Length> Q_LENGTH_1_M = new QuantityType<>("1 m");
    private static final QuantityType<Length> Q_LENGTH_2_CM = new QuantityType<>("2 cm");
    private static final QuantityType<Dimensionless> Q_ONE_1 = new QuantityType<>(1, Units.ONE);
    private static final QuantityType<Dimensionless> Q_ONE_2 = new QuantityType<>(2, Units.ONE);
    public void operatorPlusNumberNumber() {
        assertThat(NumberExtensions.operator_plus(DECIMAL1, DECIMAL2), is(BigDecimal.valueOf(3)));
    public void operatorPlusNumberQuantityOne() {
        assertThat(NumberExtensions.operator_plus(Q_ONE_1, DECIMAL2), is(BigDecimal.valueOf(3)));
    public void operatorPlusQuantityQuantity() {
        assertThat(NumberExtensions.operator_plus(Q_CELSIUS_1, Q_CELSIUS_2), is(QuantityType.valueOf("3 °C")));
    public void operatorMinusNumber() {
        assertThat(NumberExtensions.operator_minus(DECIMAL1), is(BigDecimal.valueOf(-1)));
    public void operatorMinusQuantity() {
        assertThat(NumberExtensions.operator_minus(Q_CELSIUS_1), is(QuantityType.valueOf("-1 °C")));
    public void operatorMinusNumberNumber() {
        assertThat(NumberExtensions.operator_minus(DECIMAL2, DECIMAL1), is(BigDecimal.ONE));
    public void operatorMinusNumberQuantityOne() {
        assertThat(NumberExtensions.operator_minus(Q_ONE_2, DECIMAL1), is(BigDecimal.ONE));
    public void operatorMinusQuantityQuantity() {
        assertThat(NumberExtensions.operator_minus(Q_LENGTH_1_M, Q_LENGTH_2_CM), is(QuantityType.valueOf("0.98 m")));
    public void operatorMultiplyNumberQuantity() {
        assertThat(NumberExtensions.operator_multiply(DECIMAL2, Q_LENGTH_2_CM), is(QuantityType.valueOf("4 cm")));
    public void operatorMultiplyQuantityQuantity() {
        assertThat(NumberExtensions.operator_multiply(Q_LENGTH_1_M, Q_LENGTH_2_CM), is(QuantityType.valueOf("2 m·cm")));
    public void operatorDivideQuantityNumber() {
        assertThat(NumberExtensions.operator_divide(Q_LENGTH_1_M, DECIMAL2), is(QuantityType.valueOf("0.5 m")));
    public void operatorDivideQuantityQuantity() {
        assertThat(NumberExtensions.operator_divide(Q_LENGTH_1_M, Q_LENGTH_2_CM), is(QuantityType.valueOf("0.5 m/cm")));
    public void operatorDivideNumberQuantity() {
        assertThat(NumberExtensions.operator_divide(DECIMAL1, Q_LENGTH_2_CM), is(QuantityType.valueOf("0.5 one/cm")));
    public void operatorEqualsNumberQuantity() {
        assertFalse(NumberExtensions.operator_equals((Number) DECIMAL1, Q_LENGTH_2_CM));
    public void operatorEqualsQuantityNumber() {
        assertFalse(NumberExtensions.operator_equals((Number) Q_LENGTH_2_CM, DECIMAL1));
    public void operatorEqualsQuantityQuantityFalse() {
        assertFalse(NumberExtensions.operator_equals(Q_LENGTH_1_M, Q_LENGTH_2_CM));
    public void operatorEqualsQuantityQuantityTrue() {
        assertTrue(NumberExtensions.operator_equals(Q_LENGTH_1_M, new QuantityType<>("100 cm")));
    public void operatorLessThanNumberQuantity() {
        assertFalse(NumberExtensions.operator_lessThan((Number) Q_LENGTH_1_M, Q_LENGTH_1_M));
    public void operatorLessThanTypeQuantity() {
        assertFalse(NumberExtensions.operator_lessThan((Type) Q_LENGTH_1_M, Q_LENGTH_1_M));
    public void operatorLessThanQuantityQuantityFalse() {
        assertFalse(NumberExtensions.operator_lessThan(Q_LENGTH_1_M, Q_LENGTH_2_CM));
    public void operatorLessThanQuantityQuantityTrue() {
        assertTrue(NumberExtensions.operator_lessThan(Q_LENGTH_2_CM, Q_LENGTH_1_M));
    public void operatorGreaterThanNumberQuantity() {
        assertFalse(NumberExtensions.operator_greaterThan((Number) Q_LENGTH_1_M, Q_LENGTH_1_M));
    public void operatorGreaterThanTypeQuantity() {
        assertFalse(NumberExtensions.operator_greaterThan((Type) Q_LENGTH_1_M, Q_LENGTH_1_M));
    public void operatorGreaterThanQuantityQuantityFalse() {
        assertFalse(NumberExtensions.operator_greaterThan(Q_LENGTH_2_CM, Q_LENGTH_1_M));
    public void operatorGreaterThanQuantityQuantityTrue() {
        assertTrue(NumberExtensions.operator_greaterThan(Q_LENGTH_1_M, Q_LENGTH_2_CM));
    public void operatorLessEqualsThanNumberQuantity() {
        assertFalse(NumberExtensions.operator_lessEqualsThan(BigDecimal.valueOf(100), new QuantityType<>("100 cm")));
    public void operatorLessEqualsThanTypeQuantity() {
        assertTrue(NumberExtensions.operator_lessEqualsThan((Type) Q_LENGTH_1_M, Q_LENGTH_1_M));
    public void operatorLessEqualsThanQuantityQuantityFalse() {
        assertFalse(NumberExtensions.operator_lessEqualsThan(Q_LENGTH_1_M, Q_LENGTH_2_CM));
    public void operatorLessEqualsThanQuantityQuantityTrue() {
        assertTrue(NumberExtensions.operator_lessEqualsThan(Q_LENGTH_2_CM, Q_LENGTH_1_M));
    public void operatorGreaterEqualsThanNumberQuantity() {
        assertFalse(NumberExtensions.operator_greaterEqualsThan(BigDecimal.ONE, new QuantityType<>("1 km")));
    public void operatorGreaterEqualsThanTypeQuantity() {
        assertTrue(NumberExtensions.operator_greaterEqualsThan((Type) Q_LENGTH_1_M, new QuantityType<>("100 cm")));
    public void operatorGreaterEqualsThanQuantityQuantityFalse() {
        assertFalse(NumberExtensions.operator_greaterEqualsThan(Q_LENGTH_2_CM, Q_LENGTH_1_M));
    public void operatorGreaterEqualsThanQuantityQuantityTrue() {
        assertTrue(NumberExtensions.operator_greaterEqualsThan(Q_LENGTH_1_M, Q_LENGTH_2_CM));
    public void operatorEqualsQuantityOneNumber() {
        assertTrue(NumberExtensions.operator_equals(Q_ONE_1, DECIMAL1));
        assertTrue(NumberExtensions.operator_equals((Number) DECIMAL1, Q_ONE_1));
    public void operatorLessThanQuantityOneNumber() {
        assertTrue(NumberExtensions.operator_lessThan(Q_ONE_1, DECIMAL2));
        assertTrue(NumberExtensions.operator_lessThan((Number) DECIMAL1, Q_ONE_2));
    public void operatorLessEqualsThanQuantityOneNumber() {
        assertTrue(NumberExtensions.operator_lessEqualsThan(Q_ONE_1, DECIMAL1));
        assertTrue(NumberExtensions.operator_lessEqualsThan((Number) DECIMAL1, Q_ONE_1));
    public void operatorGreaterThanQuantityOneNumber() {
        assertTrue(NumberExtensions.operator_greaterThan(Q_ONE_2, DECIMAL1));
        assertTrue(NumberExtensions.operator_greaterThan((Number) DECIMAL2, Q_ONE_1));
    public void operatorGreaterEqualsThanQuantityOneNumber() {
        assertTrue(NumberExtensions.operator_greaterEqualsThan(Q_ONE_1, DECIMAL1));
        assertTrue(NumberExtensions.operator_greaterEqualsThan((Number) DECIMAL1, Q_ONE_1));
     * Test method for {@link NumberExtensions#operator_plus(java.lang.Number, java.lang.Number)}
    public void testOperatorPlus() {
        Number x = 9;
        Number y = 0;
        BigDecimal result = NumberExtensions.operator_plus(x, y);
        assertEquals(new BigDecimal(9), result);
    public void testOperatorPlusNullLeft() {
        Number x = null;
        Number y = 5;
        assertEquals(new BigDecimal(5), result);
    public void testOperatorPlusNullRight() {
        Number x = 10;
        Number y = null;
        assertEquals(new BigDecimal(10), result);
     * Test method for {@link NumberExtensions#operator_minus(java.lang.Number)}
    public void testOperatorMinusNumber() {
        Number x = 2;
        BigDecimal result = NumberExtensions.operator_minus(x);
        assertEquals(new BigDecimal(-2), result);
    public void testOperatorMinusNull() {
        assertNull(result);
     * Test method for {@link NumberExtensions#operator_minus(java.lang.Number, java.lang.Number)}
    public void testOperatorMinusNumberNumber() {
        Number y = 100;
        BigDecimal result = NumberExtensions.operator_minus(x, y);
        assertEquals(new BigDecimal(10 - 100), result);
    public void testOperatorMinusNullNumber() {
        assertEquals(new BigDecimal(-100), result);
    public void testOperatorMinusNumberNull() {
     * Test method for {@link NumberExtensions#operator_multiply(java.lang.Number, java.lang.Number)}
    public void testOperatorMultiply() {
        Number x = 20;
        Number y = 30;
        BigDecimal result = NumberExtensions.operator_multiply(x, y);
        assertEquals(new BigDecimal(20 * 30), result);
    public void testOperatorMultiplyNullLeft() {
        assertEquals(new BigDecimal(0), result);
    public void testOperatorMultiplyNullRight() {
     * Test method for {@link NumberExtensions#operator_divide(java.lang.Number, java.lang.Number)}
    public void testOperatorDivide() {
        Number x = 12;
        Number y = 4;
        BigDecimal result = NumberExtensions.operator_divide(x, y);
        assertEquals(result, new BigDecimal(12).divide(new BigDecimal(4), 8, RoundingMode.HALF_UP));
    public void testOperatorDivideNullLeft() {
        assertThrows(NullPointerException.class, () -> NumberExtensions.operator_divide(x, y));
    public void testOperatorDivideNullRight() {
     * Test method for {@link NumberExtensions#operator_equals(java.lang.Number, java.lang.Number)}
    public void testOperatorEqualsNumberNumber() {
        Number x = 123;
        Number y = 123;
        boolean resutl = NumberExtensions.operator_equals(x, y);
        assertTrue(resutl);
        x = 123;
        y = 321;
        resutl = NumberExtensions.operator_equals(x, y);
        assertFalse(resutl);
    public void testOperatorEqualsNullNumber() {
    public void testOperatorEqualsNumberNull() {
    public void testOperatorEqualsNullrNull() {
     * Test method for {@link NumberExtensions#operator_notEquals(java.lang.Number, java.lang.Number)}
    public void testOperatorNotEqualsNumberNumber() {
        boolean resutl = NumberExtensions.operator_notEquals(x, y);
        resutl = NumberExtensions.operator_notEquals(x, y);
    public void testOperatorNotEqualsNullNumber() {
    public void testOperatorNotEqualsNumberNull() {
     * Test method for {@link NumberExtensions#operator_lessThan(java.lang.Number, java.lang.Number)}
    public void testOperatorLessThanNumberNumber() {
        boolean resutl = NumberExtensions.operator_lessThan(x, y);
        x = 90;
        y = 2;
        resutl = NumberExtensions.operator_lessThan(x, y);
    public void testOperatorLessThanNullNumber() {
    public void testOperatorLessThanNumberNull() {
     * Test method for {@link NumberExtensions#operator_greaterThan(java.lang.Number, java.lang.Number)}
    public void testOperatorGreaterThanNumberNumber() {
        boolean resutl = NumberExtensions.operator_greaterThan(x, y);
        resutl = NumberExtensions.operator_greaterThan(x, y);
    public void testOperatorGreaterThanNullNumber() {
    public void testOperatorGreaterThanNumberNull() {
     * Test method for {@link NumberExtensions#operator_lessEqualsThan(java.lang.Number, java.lang.Number)}
    public void testOperatorLessEqualsThanNumberNumber() {
        boolean resutl = NumberExtensions.operator_lessEqualsThan(x, y);
        resutl = NumberExtensions.operator_lessEqualsThan(x, y);
        x = 3;
        y = 3;
    public void testOperatorLessEqualsThanNullNumber() {
    public void testOperatorLessEqualsThanNumberNull() {
    public void testOperatorLessEqualsThanNullNull() {
     * Test method for {@link NumberExtensions#operator_greaterEqualsThan(java.lang.Number, java.lang.Number)}
    public void testOperatorGreaterEqualsThanNumberNumber() {
        boolean resutl = NumberExtensions.operator_greaterEqualsThan(x, y);
        resutl = NumberExtensions.operator_greaterEqualsThan(x, y);
    public void testOperatorGreaterEqualsThanNullNumber() {
    public void testOperatorGreaterEqualsThanNumberNull() {
    public void testOperatorGreaterEqualsThanNullNull() {
     * Test method for {@link NumberExtensions#operator_equals(org.openhab.core.types.Type, java.lang.Number)}
    public void testOperatorEqualsTypeNumber() {
        DecimalType type = new DecimalType(10);
        boolean result = NumberExtensions.operator_equals((Type) type, x);
        assertTrue(result);
        x = 1;
        result = NumberExtensions.operator_equals((Type) type, x);
        assertFalse(result);
     * Test method for
     * {@link NumberExtensions#operator_notEquals(org.openhab.core.types.Type, java.lang.Number)}
    public void testOperatorNotEqualsTypeNumber() {
        boolean result = NumberExtensions.operator_notEquals((Type) type, x);
        result = NumberExtensions.operator_notEquals((Type) type, x);
     * {@link NumberExtensions#operator_greaterThan(org.openhab.core.types.Type, java.lang.Number)}
    public void testOperatorGreaterThanTypeNumber() {
        boolean result = NumberExtensions.operator_greaterThan((Type) type, x);
        x = 2;
        result = NumberExtensions.operator_greaterThan((Type) type, x);
     * {@link NumberExtensions#operator_greaterEqualsThan(org.openhab.core.types.Type, java.lang.Number)}
    public void testOperatorGreaterEqualsThanTypeNumber() {
        boolean result = NumberExtensions.operator_greaterEqualsThan((Type) type, x);
        result = NumberExtensions.operator_greaterEqualsThan((Type) type, x);
        x = 10;
     * {@link NumberExtensions#operator_lessThan(org.openhab.core.types.Type, java.lang.Number)}
    public void testOperatorLessThanTypeNumber() {
        boolean result = NumberExtensions.operator_lessThan((Type) type, x);
        result = NumberExtensions.operator_lessThan((Type) type, x);
     * {@link NumberExtensions#operator_lessEqualsThan(org.openhab.core.types.Type, java.lang.Number)}
    public void testOperatorLessEqualsThanTypeNumber() {
        boolean result = NumberExtensions.operator_lessEqualsThan((Type) type, x);
        result = NumberExtensions.operator_lessEqualsThan((Type) type, x);
    @ValueSource(strings = { "0.2", "0.1111111111111111111111111111111111111111111111111111111", "1" })
    public void testNumberToBigDecimal(String value) {
        DecimalType dt = new DecimalType(value);
        assertEquals(value, NumberExtensions.numberToBigDecimal(dt).toString());
        PercentType pt = new PercentType(value);
        assertEquals(value, NumberExtensions.numberToBigDecimal(pt).toString());
        // HSBTye is a subclass of DecimalType, it converts part "V" to a BigDecimal
        HSBType hsb = new HSBType(dt, pt, pt);
        assertEquals(value, NumberExtensions.numberToBigDecimal(hsb).toString());
        int integerTestValue = (int) (Double.valueOf(value).doubleValue() * 1000);
        Integer intValue = Integer.valueOf(integerTestValue);
        assertEquals("" + integerTestValue, NumberExtensions.numberToBigDecimal(intValue).toString());
        Long longValue = Long.valueOf(integerTestValue);
        assertEquals("" + integerTestValue, NumberExtensions.numberToBigDecimal(longValue).toString());
        BigDecimal bd = new BigDecimal(value);
        assertEquals(value, NumberExtensions.numberToBigDecimal(bd).toString());
