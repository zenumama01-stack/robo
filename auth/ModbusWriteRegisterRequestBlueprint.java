 * Implementation for writing registers
public class ModbusWriteRegisterRequestBlueprint extends ModbusWriteRequestBlueprint {
    private final ModbusRegisterArray registers;
     * @param registers register(s) to write
     *            {@link ModbusWriteFunctionCode#WRITE_COIL}. Useful with single register of data.
     *             <code>false</code> but there are many registers to write.
    public ModbusWriteRegisterRequestBlueprint(int slaveId, int reference, ModbusRegisterArray registers,
            boolean writeMultiple, int maxTries) throws IllegalArgumentException {
        if (!writeMultiple && registers.size() > 1) {
            throw new IllegalArgumentException("With multiple registers, writeMultiple must be true");
        if (registers.size() == 0) {
            throw new IllegalArgumentException("Must have at least one register");
            throw new IllegalArgumentException("maxTries should be positive");
        return writeMultiple ? ModbusWriteFunctionCode.WRITE_MULTIPLE_REGISTERS
                : ModbusWriteFunctionCode.WRITE_SINGLE_REGISTER;
    public ModbusRegisterArray getRegisters() {
        return registers;
        return "ModbusWriteRegisterRequestBlueprint [slaveId=" + slaveId + ", reference=" + reference + ", registers="
                + registers + ", maxTries=" + maxTries + ", getFunctionCode()=" + getFunctionCode() + "]";
