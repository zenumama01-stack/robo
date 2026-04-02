 * Implementation for writing coils
public class ModbusWriteCoilRequestBlueprint extends ModbusWriteRequestBlueprint {
    private final int reference;
    private final BitArray bits;
    private final boolean writeMultiple;
     * Construct coil write request with single bit of data
     * @param slaveId slave id to write to
     * @param reference reference address
     * @param data bit to write
     * @param writeMultiple whether to use {@link ModbusWriteFunctionCode#WRITE_MULTIPLE_COILS} over
     *            {@link ModbusWriteFunctionCode#WRITE_COIL}
     * @param maxTries maximum number of tries in case of errors, should be at least 1
    public ModbusWriteCoilRequestBlueprint(int slaveId, int reference, boolean data, boolean writeMultiple,
        this(slaveId, reference, new BitArray(data), writeMultiple, maxTries);
     * Construct coil write request with many bits of data
     * @param data bit(s) to write
     *            {@link ModbusWriteFunctionCode#WRITE_COIL}. Useful with single bit of data.
     * @throws IllegalArgumentException in case <code>data</code> is empty, <code>writeMultiple</code> is
     *             <code>false</code> but there are many bits to write.
    public ModbusWriteCoilRequestBlueprint(int slaveId, int reference, BitArray data, boolean writeMultiple,
        this.bits = data;
        this.writeMultiple = writeMultiple;
        if (!writeMultiple && bits.size() > 1) {
            throw new IllegalArgumentException("With multiple coils, writeMultiple must be true");
        if (bits.size() == 0) {
            throw new IllegalArgumentException("Must have at least one bit");
        if (maxTries <= 0) {
            throw new IllegalArgumentException("maxTries should be positive, was " + maxTries);
    public ModbusWriteFunctionCode getFunctionCode() {
        return writeMultiple ? ModbusWriteFunctionCode.WRITE_MULTIPLE_COILS : ModbusWriteFunctionCode.WRITE_COIL;
    public BitArray getCoils() {
        return "ModbusWriteCoilRequestBlueprint [slaveId=" + slaveId + ", reference=" + reference + ", bits=" + bits
                + ", maxTries=" + maxTries + ", getFunctionCode()=" + getFunctionCode() + "]";
    public void accept(ModbusWriteRequestBlueprintVisitor visitor) {
        visitor.visit(this);
