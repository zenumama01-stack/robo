public class BitUtilitiesExtractIndividualMethodsTest {
    public static Collection<Object[]> data() {
        // We use test data from BitUtilitiesExtractStateFromRegistersTest
        // In BitUtilitiesExtractStateFromRegistersTest the data is aligned to registers
        // Here (in registerVariations) we generate offsetted variations of the byte data
        // to test extractXX which can operate on data aligned on byte-level, not just data aligned on-register level
        Collection<Object[]> data = BitUtilitiesExtractStateFromRegistersTest.data();
        return data.stream().flatMap(values -> {
            Object expectedResult = values[0];
            ValueType type = (ValueType) values[1];
            ModbusRegisterArray registers = (ModbusRegisterArray) values[2];
            int index = (int) values[3];
            return registerVariations(expectedResult, type, registers, index);
    public static Stream<Object[]> filteredTestData(ValueType type) {
        return data().stream().filter(values -> (ValueType) values[1] == type);
     * Generate register variations for extractXX functions
     * @return entries of (byte[], byteIndex)
    private static Stream<Object[]> registerVariations(Object expectedResult, ValueType type,
            ModbusRegisterArray registers, int index) {
        byte[] origBytes = registers.getBytes();
        int origRegisterIndex = index;
        int origByteIndex = origRegisterIndex * 2;
        Builder<Object[]> streamBuilder = Stream.builder();
        for (int offset = 0; offset < 5; offset++) {
            int byteIndex = origByteIndex + offset;
            byte[] bytesOffsetted = new byte[origBytes.length + offset];
            Arrays.fill(bytesOffsetted, (byte) 99);
            System.arraycopy(origBytes, 0, bytesOffsetted, offset, origBytes.length);
            // offsetted:
            streamBuilder.add(new Object[] { expectedResult, type, bytesOffsetted, byteIndex });
            // offsetted, with no extra bytes following
            // (this is only done for successful cases to avoid copyOfRange padding with zeros
            if (!(expectedResult instanceof Class)) {
                byte[] bytesOffsettedCutExtra = Arrays.copyOfRange(bytesOffsetted, 0, byteIndex + type.getBits() / 8);
                if (bytesOffsettedCutExtra.length != bytesOffsetted.length) {
                    streamBuilder.add(new Object[] { expectedResult, type, bytesOffsettedCutExtra, byteIndex });
        return streamBuilder.build();
    private void testIndividual(Object expectedResult, ValueType type, byte[] bytes, int byteIndex,
            Supplier<Number> methodUnderTest, Function<DecimalType, Number> expectedPrimitive) {
        testIndividual(expectedResult, type, bytes, byteIndex, methodUnderTest, expectedPrimitive, null);
            Supplier<Number> methodUnderTest, Function<DecimalType, Number> expectedPrimitive,
            @Nullable Number defaultWhenEmptyOptional) {
        String testExplanation = String.format("bytes=%s, byteIndex=%d, type=%s", Arrays.toString(bytes), byteIndex,
        final Object expectedNumber;
            assertThrows((Class<? extends Throwable>) expectedResult, methodUnderTest::get);
        } else if (expectedResult instanceof Optional<?> optional) {
            assertTrue(optional.isEmpty());
            if (defaultWhenEmptyOptional == null) {
                fail("Should provide defaultWhenEmptyOptional");
            DecimalType expectedDecimal = (DecimalType) expectedResult;
            expectedNumber = expectedPrimitive.apply(expectedDecimal);
            assertEquals(expectedNumber, methodUnderTest.get(), testExplanation);
    public static Stream<Object[]> filteredTestDataSInt16() {
        return filteredTestData(ValueType.INT16);
    @MethodSource("filteredTestDataSInt16")
    public void testExtractIndividualSInt16(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
            throws InstantiationException, IllegalAccessException {
        testIndividual(expectedResult, type, bytes, byteIndex, () -> ModbusBitUtilities.extractSInt16(bytes, byteIndex),
                Number::shortValue);
    public static Stream<Object[]> filteredTestDataUInt16() {
        return filteredTestData(ValueType.UINT16);
    @MethodSource("filteredTestDataUInt16")
    public void testExtractIndividualUInt16(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
        testIndividual(expectedResult, type, bytes, byteIndex, () -> ModbusBitUtilities.extractUInt16(bytes, byteIndex),
                DecimalType::intValue);
    public static Stream<Object[]> filteredTestDataSInt32() {
        return filteredTestData(ValueType.INT32);
    @MethodSource("filteredTestDataSInt32")
    public void testExtractIndividualSInt32(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
        testIndividual(expectedResult, type, bytes, byteIndex, () -> ModbusBitUtilities.extractSInt32(bytes, byteIndex),
    public static Stream<Object[]> filteredTestDataUInt32() {
        return filteredTestData(ValueType.UINT32);
    @MethodSource("filteredTestDataUInt32")
    public void testExtractIndividualUInt32(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
        testIndividual(expectedResult, type, bytes, byteIndex, () -> ModbusBitUtilities.extractUInt32(bytes, byteIndex),
                DecimalType::longValue);
    public static Stream<Object[]> filteredTestDataSInt32Swap() {
        return filteredTestData(ValueType.INT32_SWAP);
    @MethodSource("filteredTestDataSInt32Swap")
    public void testExtractIndividualSInt32Swap(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
        testIndividual(expectedResult, type, bytes, byteIndex,
                () -> ModbusBitUtilities.extractSInt32Swap(bytes, byteIndex), DecimalType::intValue);
    public static Stream<Object[]> filteredTestDataUInt32Swap() {
        return filteredTestData(ValueType.UINT32_SWAP);
    @MethodSource("filteredTestDataUInt32Swap")
    public void testExtractIndividualUInt32Swap(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
                () -> ModbusBitUtilities.extractUInt32Swap(bytes, byteIndex), DecimalType::longValue);
    public static Stream<Object[]> filteredTestDataSInt64() {
        return filteredTestData(ValueType.INT64);
    @MethodSource("filteredTestDataSInt64")
    public void testExtractIndividualSInt64(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
        testIndividual(expectedResult, type, bytes, byteIndex, () -> ModbusBitUtilities.extractSInt64(bytes, byteIndex),
    public static Stream<Object[]> filteredTestDataUInt64() {
        return filteredTestData(ValueType.UINT64);
    @MethodSource("filteredTestDataUInt64")
    public void testExtractIndividualUInt64(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
        testIndividual(expectedResult, type, bytes, byteIndex, () -> ModbusBitUtilities.extractUInt64(bytes, byteIndex),
                decimal -> decimal.toBigDecimal().toBigIntegerExact());
    public static Stream<Object[]> filteredTestDataSInt64Swap() {
        return filteredTestData(ValueType.INT64_SWAP);
    @MethodSource("filteredTestDataSInt64Swap")
    public void testExtractIndividualSInt64Swap(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
                () -> ModbusBitUtilities.extractSInt64Swap(bytes, byteIndex), DecimalType::longValue);
    public static Stream<Object[]> filteredTestDataUInt64Swap() {
        return filteredTestData(ValueType.UINT64_SWAP);
    @MethodSource("filteredTestDataUInt64Swap")
    public void testExtractIndividualUInt64Swap(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
                () -> ModbusBitUtilities.extractUInt64Swap(bytes, byteIndex),
    public static Stream<Object[]> filteredTestDataFloat32() {
        return filteredTestData(ValueType.FLOAT32);
    @MethodSource("filteredTestDataFloat32")
    public void testExtractIndividualFloat32(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
                () -> ModbusBitUtilities.extractFloat32(bytes, byteIndex), DecimalType::floatValue, Float.NaN);
    public static Stream<Object[]> filteredTestDataFloat32Swap() {
        return filteredTestData(ValueType.FLOAT32_SWAP);
    @MethodSource("filteredTestDataFloat32Swap")
    public void testExtractIndividualFloat32Swap(Object expectedResult, ValueType type, byte[] bytes, int byteIndex)
                () -> ModbusBitUtilities.extractFloat32Swap(bytes, byteIndex), DecimalType::floatValue, Float.NaN);
