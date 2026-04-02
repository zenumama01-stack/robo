import java.nio.BufferOverflowException;
import java.nio.InvalidMarkException;
import java.util.concurrent.atomic.AtomicReference;
 * ByteBuffer-like interface for working with different types of data stored in byte arrays
public class ValueBuffer {
    private final AtomicInteger byteIndex = new AtomicInteger();
    private volatile AtomicReference<@Nullable AtomicInteger> mark = new AtomicReference<>();
     * Wrap modbus registers and create a new instance of ValueBuffer
     * The instance will have position of 0.
     * @param array set of registers
     * @return new instance of ValueBuffer referencing bytes represented by modbus register array
    public static ValueBuffer wrap(ModbusRegisterArray array) {
        return new ValueBuffer(array.getBytes());
     * Wrap given bytes and create a new instance of ValueBuffer
     * @param array set of bytes to wrap
     * @return new instance of ValueBuffer referencing bytes
    public static ValueBuffer wrap(byte[] array) {
        return new ValueBuffer(array);
    private ValueBuffer(byte[] bytes) {
     * Returns this buffer's position.
     * @return The position of this buffer
    public int position() {
        return byteIndex.get();
     * Sets this buffer's position. If the mark is defined and larger than the new position then it is discarded.
     * @return this buffer
    public ValueBuffer position(int byteIndex) {
        this.mark.getAndUpdate(curMark -> {
            if (curMark == null) {
            } else if (curMark.get() > byteIndex) {
                return curMark;
        this.byteIndex.set(byteIndex);
     * Sets this buffer's mark at its position.
    public ValueBuffer mark() {
        mark = new AtomicReference<>(new AtomicInteger(byteIndex.get()));
     * Resets this buffer's position to the previously-marked position.
     * Invoking this method neither changes nor discards the mark's value.
     * @throws InvalidMarkException If the mark has not been set
    public ValueBuffer reset() throws InvalidMarkException {
        int mark = Optional.ofNullable(this.mark.get()).map(AtomicInteger::get).orElse(-1);
        if (mark < 0) {
            throw new InvalidMarkException();
        byteIndex.set(mark);
     * Returns the number of bytes between the current position and the end.
     * @return The number of bytes remaining in this buffer
    public int remaining() {
        return bytes.length - byteIndex.get();
     * Returns underlying bytes
     * @return reference to underlying bytes
    public byte[] array() {
     * Tells whether there are any bytes left between current position and the end
     * @return true if, and only if, there is at least one byte remaining in this buffer
    public boolean hasRemaining() {
        return remaining() > 0;
     * Starting from current position, read dst.length number of bytes and copy the data to dst
     * @param dst copied bytes
     * @throws BufferOverflowException If there is insufficient space in this buffer for the remaining bytes in the
     *             source buffer
    public ValueBuffer get(byte[] dst) {
        int start = byteIndex.getAndAdd(dst.length);
            System.arraycopy(bytes, start, dst, 0, dst.length);
        } catch (IndexOutOfBoundsException e) {
            throw new BufferOverflowException();
     * Extract signed 8-bit integer at current position, and advance position.
     * @return signed 8-bit integer (byte)
     * @see ModbusBitUtilities#extractSInt8
     * @throws IllegalArgumentException when there are not enough bytes in this ValueBuffer
    public byte getSInt8() {
        return ModbusBitUtilities.extractSInt8(bytes, byteIndex.getAndAdd(1));
     * Extract unsigned 8-bit integer at current position, and advance position.
     * @return unsigned 8-bit integer
     * @see ModbusBitUtilities#extractUInt8
    public short getUInt8() {
        return ModbusBitUtilities.extractUInt8(bytes, byteIndex.getAndAdd(1));
     * Extract signed 16-bit integer at current position, and advance position.
     * @return signed 16-bit integer (short)
     * @see ModbusBitUtilities#extractSInt16
    public short getSInt16() {
        return ModbusBitUtilities.extractSInt16(bytes, byteIndex.getAndAdd(2));
     * Extract unsigned 16-bit integer at current position, and advance position.
     * @return unsigned 16-bit integer
     * @see ModbusBitUtilities#extractUInt16
    public int getUInt16() {
        return ModbusBitUtilities.extractUInt16(bytes, byteIndex.getAndAdd(2));
     * Extract signed 32-bit integer at current position, and advance position.
     * @return signed 32-bit integer
     * @see ModbusBitUtilities#extractSInt32
    public int getSInt32() {
        return ModbusBitUtilities.extractSInt32(bytes, byteIndex.getAndAdd(4));
     * Extract unsigned 32-bit integer at current position, and advance position.
     * @return unsigned 32-bit integer
     * @see ModbusBitUtilities#extractUInt32
    public long getUInt32() {
        return ModbusBitUtilities.extractUInt32(bytes, byteIndex.getAndAdd(4));
     * This is identical with getSInt32, but with registers swapped.
     * @see ModbusBitUtilities#extractSInt32Swap
    public int getSInt32Swap() {
        return ModbusBitUtilities.extractSInt32Swap(bytes, byteIndex.getAndAdd(4));
     * This is identical with getUInt32, but with registers swapped.
     * @see ModbusBitUtilities#extractUInt32Swap
    public long getUInt32Swap() {
        return ModbusBitUtilities.extractUInt32Swap(bytes, byteIndex.getAndAdd(4));
     * Extract signed 64-bit integer at current position, and advance position.
     * @return signed 64-bit integer
     * @see ModbusBitUtilities#extractSInt64
    public long getSInt64() {
        return ModbusBitUtilities.extractSInt64(bytes, byteIndex.getAndAdd(8));
     * Extract unsigned 64-bit integer at current position, and advance position.
     * @return unsigned 64-bit integer
     * @see ModbusBitUtilities#extractUInt64
    public BigInteger getUInt64() {
        return ModbusBitUtilities.extractUInt64(bytes, byteIndex.getAndAdd(8));
     * This is identical with getSInt64, but with registers swapped.
     * @see ModbusBitUtilities#extractSInt64Swap
    public long getSInt64Swap() {
        return ModbusBitUtilities.extractSInt64Swap(bytes, byteIndex.getAndAdd(8));
     * This is identical with getUInt64, but with registers swapped.
     * @see ModbusBitUtilities#extractUInt64Swap
    public BigInteger getUInt64Swap() {
        return ModbusBitUtilities.extractUInt64Swap(bytes, byteIndex.getAndAdd(8));
     * Extract single-precision 32-bit IEEE 754 floating point at current position, and advance position.
     * @return single-precision 32-bit IEEE 754 floating point
     * @see ModbusBitUtilities#extractFloat32
    public float getFloat32() {
        return ModbusBitUtilities.extractFloat32(bytes, byteIndex.getAndAdd(4));
     * This is identical with getFloat32, but with registers swapped.
    public float getFloat32Swap() {
        return ModbusBitUtilities.extractFloat32Swap(bytes, byteIndex.getAndAdd(4));
