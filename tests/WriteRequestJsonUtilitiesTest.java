import static org.hamcrest.collection.ArrayMatching.arrayContaining;
import static org.openhab.core.io.transport.modbus.ModbusConstants.*;
import org.hamcrest.Matcher;
import org.openhab.core.io.transport.modbus.json.WriteRequestJsonUtilities;
public class WriteRequestJsonUtilitiesTest {
    private static final List<String> MAX_REGISTERS = IntStream.range(0, MAX_REGISTERS_WRITE_COUNT).mapToObj(i -> "1")
    private static final List<String> OVER_MAX_REGISTERS = IntStream.range(0, MAX_REGISTERS_WRITE_COUNT + 1)
            .mapToObj(i -> "1").toList();
    private static final List<String> MAX_COILS = IntStream.range(0, MAX_BITS_WRITE_COUNT).mapToObj(i -> "1").toList();
    private static final List<String> OVER_MAX_COILS = IntStream.range(0, MAX_BITS_WRITE_COUNT + 1).mapToObj(i -> "1")
    public void testEmptyArray() {
        assertThat(WriteRequestJsonUtilities.fromJson(3, "[]").size(), is(equalTo(0)));
    public void testFC6NoRegister() {
        assertThrows(IllegalArgumentException.class, () -> WriteRequestJsonUtilities.fromJson(55, "[{"//
                + "\"functionCode\": 6,"//
                + "\"address\": 5412,"//
                + "\"value\": []"//
                + "}]"));
    public void testFC6SingleRegister() {
        assertThat(WriteRequestJsonUtilities.fromJson(55, "[{"//
                + "\"value\": [3]"//
                + "}]").toArray(),
                arrayContaining((Matcher) new RegisterMatcher(55, 5412, WriteRequestJsonUtilities.DEFAULT_MAX_TRIES,
                        ModbusWriteFunctionCode.WRITE_SINGLE_REGISTER, 3)));
    public void testFC6SingleRegisterMaxTries99() {
                + "\"value\": [3],"//
                + "\"maxTries\": 99"//
                arrayContaining(
                        (Matcher) new RegisterMatcher(55, 5412, 99, ModbusWriteFunctionCode.WRITE_SINGLE_REGISTER, 3)));
    public void testFC6MultipleRegisters() {
                + "\"value\": [3, 4]"//
    public void testFC16NoRegister() {
                + "\"functionCode\": 16,"//
    public void testFC16SingleRegister() {
                        ModbusWriteFunctionCode.WRITE_MULTIPLE_REGISTERS, 3)));
    public void testFC16MultipleRegisters() {
                + "\"value\": [3, 4, 2]"//
                        ModbusWriteFunctionCode.WRITE_MULTIPLE_REGISTERS, 3, 4, 2)));
    public void testFC16MultipleRegistersMaxRegisters() {
        Collection<ModbusWriteRequestBlueprint> writes = WriteRequestJsonUtilities.fromJson(55, "[{"//
                + "\"value\": [" + String.join(",", MAX_REGISTERS) + "]"//
                + "}]");
        assertThat(writes.size(), is(equalTo(1)));
    public void testFC16MultipleRegistersTooManyRegisters() {
                + "\"value\": [" + String.join(",", OVER_MAX_REGISTERS) + "]"//
    public void testFC5SingeCoil() {
                + "\"functionCode\": 5,"//
                + "\"value\": [3]" // value 3 (!= 0) is converted to boolean true
                arrayContaining((Matcher) new CoilMatcher(55, 5412, WriteRequestJsonUtilities.DEFAULT_MAX_TRIES,
                        ModbusWriteFunctionCode.WRITE_COIL, true)));
    public void testFC5MultipleCoils() {
    public void testFC15SingleCoil() {
                + "\"functionCode\": 15,"//
                        ModbusWriteFunctionCode.WRITE_MULTIPLE_COILS, true)));
    public void testFC15MultipleCoils() {
                + "\"value\": [1, 0, 5]"//
                        ModbusWriteFunctionCode.WRITE_MULTIPLE_COILS, true, false, true)));
    public void testFC15MultipleCoilsMaxCoils() {
                + "\"value\": [" + String.join(",", MAX_COILS) + "]"//
    public void testFC15MultipleCoilsTooManyCoils() {
                + "\"value\": [" + String.join(",", OVER_MAX_COILS) + "]"//
    public void testEmptyObject() {
        // we are expecting list, not object -> error
        assertThrows(IllegalStateException.class, () -> WriteRequestJsonUtilities.fromJson(3, "{}"));
    public void testNumber() {
        // we are expecting list, not primitive (number) -> error
        assertThrows(IllegalStateException.class, () -> WriteRequestJsonUtilities.fromJson(3, "3"));
    public void testEmptyList() {
