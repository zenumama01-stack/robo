public class BasicBitArrayTest {
    public void testGetBitAndSetBit() {
        BitArray data1 = new BitArray(true, false, true);
        assertThat(data1.size(), is(equalTo(3)));
        assertThat(data1.getBit(0), is(equalTo(true)));
        assertThat(data1.getBit(1), is(equalTo(false)));
        assertThat(data1.getBit(2), is(equalTo(true)));
        data1.setBit(1, true);
        data1.setBit(2, false);
        assertThat(data1.getBit(1), is(equalTo(true)));
        assertThat(data1.getBit(2), is(equalTo(false)));
    public void testGetBitAndSetBit2() {
        BitArray data1 = new BitArray(3);
        assertThat(data1.getBit(0), is(equalTo(false)));
        data1.setBit(1, false);
    public void testOutOfBounds() {
        assertThrows(IndexOutOfBoundsException.class, () -> data1.getBit(3));
    public void testOutOfBounds2() {
        assertThrows(IndexOutOfBoundsException.class, () -> data1.getBit(-1));
    public void testOutOfBounds3() {
    public void testOutOfBounds4() {
