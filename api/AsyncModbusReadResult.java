public class AsyncModbusReadResult {
    private final ModbusReadRequestBlueprint request;
    private final @Nullable BitArray bits;
    private final @Nullable ModbusRegisterArray registers;
    public AsyncModbusReadResult(ModbusReadRequestBlueprint request, ModbusRegisterArray registers) {
        Objects.requireNonNull(registers, "Registers must not be null!");
        this.registers = registers;
        this.bits = null;
    public AsyncModbusReadResult(ModbusReadRequestBlueprint request, BitArray bits) {
        Objects.requireNonNull(bits, "Bits must not be null!");
        this.registers = null;
        this.bits = bits;
    public ModbusReadRequestBlueprint getRequest() {
     * Get "coil" or "discrete input" bit data in the case of no errors
     * @return bit data
    public Optional<BitArray> getBits() {
        return Optional.ofNullable(bits);
     * Get "input register" or "holding register" data in the case of no errors
     * @return register data
    public Optional<ModbusRegisterArray> getRegisters() {
        return Optional.ofNullable(registers);
        if (bits != null) {
            builder.append(", bits = ");
            builder.append(bits);
        if (registers != null) {
            builder.append(", registers = ");
            builder.append(registers);
