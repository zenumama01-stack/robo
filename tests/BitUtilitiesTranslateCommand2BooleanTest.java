public class BitUtilitiesTranslateCommand2BooleanTest {
    public void testZero() {
        Optional<Boolean> actual = ModbusBitUtilities.translateCommand2Boolean(DecimalType.ZERO);
        assertThat(actual, is(equalTo(Optional.of(false))));
    public void testNegative() {
        Optional<Boolean> actual = ModbusBitUtilities.translateCommand2Boolean(new DecimalType(-3.4));
        assertThat(actual, is(equalTo(Optional.of(true))));
    public void testPositive() {
        Optional<Boolean> actual = ModbusBitUtilities.translateCommand2Boolean(new DecimalType(3.4));
    public void testOn() {
        Optional<Boolean> actual = ModbusBitUtilities.translateCommand2Boolean(OnOffType.ON);
    public void testOpen() {
        Optional<Boolean> actual = ModbusBitUtilities.translateCommand2Boolean(OpenClosedType.OPEN);
    public void testOff() {
        Optional<Boolean> actual = ModbusBitUtilities.translateCommand2Boolean(OnOffType.OFF);
    public void testClosed() {
        Optional<Boolean> actual = ModbusBitUtilities.translateCommand2Boolean(OpenClosedType.CLOSED);
    public void testUnknown() {
        Optional<Boolean> actual = ModbusBitUtilities.translateCommand2Boolean(IncreaseDecreaseType.INCREASE);
        assertThat(actual, is(equalTo(Optional.empty())));
