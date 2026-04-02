class RegisterMatcher extends AbstractRequestComparer<ModbusWriteRegisterRequestBlueprint> {
    private Integer[] expectedRegisterValues;
    public RegisterMatcher(int expectedUnitId, int expectedAddress, int expectedMaxTries,
            ModbusWriteFunctionCode expectedFunctionCode, Integer... expectedRegisterValues) {
        this.expectedRegisterValues = expectedRegisterValues;
        description.appendText(" registers=");
        description.appendValue(Arrays.toString(expectedRegisterValues));
    protected boolean doMatchData(ModbusWriteRegisterRequestBlueprint item) {
        ModbusRegisterArray registers = item.getRegisters();
        Object[] actual = StreamSupport
                .stream(IntStream.range(0, registers.size()).mapToObj(registers::getRegister).spliterator(), false)
                .toArray();
        return Objects.deepEquals(actual, expectedRegisterValues);
