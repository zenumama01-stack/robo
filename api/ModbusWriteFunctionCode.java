 * Modbus write function codes supported by this transport
public enum ModbusWriteFunctionCode {
    WRITE_COIL(Modbus.WRITE_COIL),
    WRITE_MULTIPLE_COILS(Modbus.WRITE_MULTIPLE_COILS),
    WRITE_SINGLE_REGISTER(Modbus.WRITE_SINGLE_REGISTER),
    WRITE_MULTIPLE_REGISTERS(Modbus.WRITE_MULTIPLE_REGISTERS);
    private final int functionCode;
    ModbusWriteFunctionCode(int code) {
        functionCode = code;
     * Get numeric function code represented by this instance
    public int getFunctionCode() {
     * Construct {@link ModbusWriteFunctionCode} from the numeric function code
     * @param functionCode numeric function code
     * @return {@link ModbusWriteFunctionCode} matching the numeric function code
     * @throws IllegalArgumentException with unsupported functions
    public static ModbusWriteFunctionCode fromFunctionCode(int functionCode) throws IllegalArgumentException {
        return Stream.of(ModbusWriteFunctionCode.values()).filter(v -> v.getFunctionCode() == functionCode).findFirst()
                .orElseThrow(() -> new IllegalArgumentException("Invalid functionCode"));
