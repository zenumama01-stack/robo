import net.wimpi.modbus.Modbus;
 * Implementation of immutable representation of modbus read request
 * Equals and hashCode implemented keeping {@link PollTask} in mind: two instances of this class are considered the same
 * if they have
 * the equal parameters (same slave id, start, length, function code and maxTries).
public class ModbusReadRequestBlueprint {
    private final int slaveId;
    private final ModbusReadFunctionCode functionCode;
    private final int start;
    private final int maxTries;
    public ModbusReadRequestBlueprint(int slaveId, ModbusReadFunctionCode functionCode, int start, int length,
            int maxTries) {
        this.slaveId = slaveId;
        this.functionCode = functionCode;
        this.maxTries = maxTries;
     * Returns the unit identifier of this
     * <tt>ModbusMessage</tt> as <tt>int</tt>.<br>
     * The identifier is a 1-byte non negative
     * integer value valid in the range of 0-255.
     * @return the unit identifier as <tt>int</tt>.
    public int getUnitID() {
        return slaveId;
    public int getReference() {
    public ModbusReadFunctionCode getFunctionCode() {
        return functionCode;
    public int getDataLength() {
     * Maximum number of tries to execute the request, when request fails
     * For example, number 1 means on try only with no re-tries.
     * @return number of maximum tries
    public int getMaxTries() {
        return maxTries;
     * Returns the protocol identifier of this
     * The identifier is a 2-byte (short) non negative
     * integer value valid in the range of 0-65535.
     * @return the protocol identifier as <tt>int</tt>.
    public int getProtocolID() {
        return Modbus.DEFAULT_PROTOCOL_ID;
        return Objects.hash(functionCode, length, maxTries, slaveId, start);
        return "ModbusReadRequestBlueprint [slaveId=" + slaveId + ", functionCode=" + functionCode + ", start=" + start
                + ", length=" + length + ", maxTries=" + maxTries + "]";
        if (obj.getClass() != getClass()) {
        ModbusReadRequestBlueprint rhs = (ModbusReadRequestBlueprint) obj;
        return functionCode == rhs.functionCode && length == rhs.length && slaveId == rhs.slaveId && start == rhs.start;
