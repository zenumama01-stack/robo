class CoilMatcher extends AbstractRequestComparer<ModbusWriteCoilRequestBlueprint> {
    private Boolean[] expectedCoils;
    public CoilMatcher(int expectedUnitId, int expectedAddress, int expectedMaxTries,
            ModbusWriteFunctionCode expectedFunctionCode, Boolean... expectedCoils) {
        super(expectedUnitId, expectedAddress, expectedFunctionCode, expectedMaxTries);
        this.expectedCoils = expectedCoils;
        super.describeTo(description);
        description.appendText(" coils=");
        description.appendValue(Arrays.toString(expectedCoils));
    protected boolean doMatchData(ModbusWriteCoilRequestBlueprint item) {
        Object[] actual = StreamSupport.stream(item.getCoils().spliterator(), false).toArray();
        return Objects.deepEquals(actual, expectedCoils);
