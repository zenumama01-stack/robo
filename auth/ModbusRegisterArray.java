 * Immutable {@link ModbusRegisterArray} implementation
public class ModbusRegisterArray {
    public ModbusRegisterArray(byte... bytes) {
        if (bytes.length % 2 != 0) {
        this.bytes = Arrays.copyOf(bytes, bytes.length);
     * Construct plain <code>ModbusRegisterArray</code> array from register values
     * @param registerValues register values, each <code>int</code> corresponding to one register
    public ModbusRegisterArray(int... registerValues) {
        bytes = new byte[registerValues.length * 2];
        for (int registerIndex = 0; registerIndex < registerValues.length; registerIndex++) {
            int register = registerValues[registerIndex] & 0xffff;
            // hi-byte
            bytes[registerIndex * 2] = (byte) (register >> 8);
            // lo byte
            bytes[registerIndex * 2 + 1] = (byte) register;
     * Get register index i as unsigned integer
     * @param i register index
     * @return register value interpreted as unsigned integer (big-endian byte ordering)
    public int getRegister(int i) {
        int hi = bytes[i * 2] & 0xff;
        int lo = bytes[i * 2 + 1] & 0xff;
        return ((hi << 8) | lo) & 0xffff;
     * Return bytes representing the registers
     * Index 0: hi-byte of 1st register
     * Index 1: low-byte of 1st register
     * Index 3: hi-byte of 2nd register
     * Index 4: low-byte of 2nd register
     * ...
     * @return set of bytes
    public byte[] getBytes() {
     * Get number of registers stored in this instance
        return bytes.length / 2;
            return "ModbusRegisterArray(<empty>)";
        return "ModbusRegisterArray(" + toHexString() + ')';
     * Get register data as a hex string
     * For example, 04 45 00 00
     * @return string representing the bytes of the register array
    public String toHexString() {
        if (size() == 0) {
        return HexUtils.bytesToHex(getBytes());
        result = prime * result + Arrays.hashCode(bytes);
        ModbusRegisterArray other = (ModbusRegisterArray) obj;
        return Arrays.equals(bytes, other.bytes);
