import java.util.BitSet;
import java.util.stream.IntStream;
 * Class that implements a collection for
 * bits
public class BitArray implements Iterable<Boolean> {
    private final BitSet wrapped;
    private final int length;
    public BitArray(int nbits) {
        this(new BitSet(nbits), nbits);
    public BitArray(boolean... bits) {
        this(bitSetFromBooleans(bits), bits.length);
    public BitArray(BitSet wrapped, int length) {
        this.wrapped = wrapped;
    private static BitSet bitSetFromBooleans(boolean... bits) {
        BitSet bitSet = new BitSet(bits.length);
        for (int i = 0; i < bits.length; i++) {
            bitSet.set(i, bits[i]);
        return bitSet;
    private boolean sizeAndValuesEquals(@Nullable Object obj) {
        if (obj == this) {
        if (!(obj instanceof BitArray)) {
        BitArray other = (BitArray) obj;
        if (this.size() != other.size()) {
        for (int i = 0; i < this.size(); i++) {
            if (this.getBit(i) != other.getBit(i)) {
     * Returns the state of the bit at the given index
     * Index 0 matches LSB (rightmost) bit
     * @param index the index of the bit to be returned.
     * @return true if the bit at the specified index is set,
     *         false otherwise.
     * @throws IndexOutOfBoundsException if the index is out of bounds.
    public boolean getBit(int index) {
        if (index >= size()) {
            throw new IndexOutOfBoundsException();
        return this.wrapped.get(index);
    public void setBit(int index, boolean value) {
            this.wrapped.set(index);
            this.wrapped.clear(index);
     * Get number of bits stored in this instance
        return "BitArray(bits=" + (length == 0 ? "<empty>" : toBinaryString()) + ")";
    public Iterator<Boolean> iterator() {
        return IntStream.range(0, size()).mapToObj(this::getBit).iterator();
        return sizeAndValuesEquals(obj);
     * Get data as binary string
     * For example, 0010
     * @return string representing the data
    public String toBinaryString() {
        final StringBuilder buffer = new StringBuilder(size());
        IntStream.range(0, size()).mapToObj(i -> getBit(i) ? '1' : '0').forEach(buffer::append);
