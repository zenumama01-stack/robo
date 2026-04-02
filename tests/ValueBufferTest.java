import org.openhab.core.io.transport.modbus.ValueBuffer;
public class ValueBufferTest {
    public void testInt32Int8() {
        ValueBuffer wrap = ValueBuffer.wrap(new byte[] { (byte) 0xFF, (byte) 0xFF, (byte) 0xFC, 0x14, 3, -1, -2 });
        assertEquals(7, wrap.remaining());
        assertTrue(wrap.hasRemaining());
        assertEquals(-1004, wrap.getSInt32());
        assertEquals(3, wrap.remaining());
        assertEquals(3, wrap.getSInt8());
        assertEquals(2, wrap.remaining());
        assertEquals(-1, wrap.getSInt8());
        assertEquals(1, wrap.remaining());
        assertEquals(254, wrap.getUInt8());
        assertEquals(0, wrap.remaining());
        assertFalse(wrap.hasRemaining());
        wrap.position(7);
        assertThrows(IllegalArgumentException.class, () -> wrap.getSInt8());
    public void testOutOfBound2s() {
        wrap.position(6);
        assertThrows(IllegalArgumentException.class, () -> wrap.getSInt16());
    public void testMarkReset() {
        wrap.mark();
        wrap.reset();
        assertEquals(4294966292L, wrap.getUInt32());
    public void testMarkHigherThanPosition() {
        wrap.position(4);
        assertEquals(4, wrap.position());
        // mark = position
        // position < mark
        wrap.position(3); // Mark is removed here
        assertEquals(3, wrap.position());
        boolean caughtException = false;
        } catch (InvalidMarkException e) {
            // OK, expected
            caughtException = true;
        assertTrue(caughtException);
        // Mark is removed. Reset unaccessible even with original position of 4
        caughtException = false;
    public void testMarkLowerThanPosition() {
        // mark > position
        assertEquals(6, wrap.position());
    public void testPosition() {
        ValueBuffer wrap = ValueBuffer.wrap(new byte[] { 0, 0, 0, 1, 3, -1, -2 });
        assertEquals(0, wrap.position());
        assertEquals(5, wrap.position());
    public void testBulkGetBufferOverflow() {
        ValueBuffer wrap = ValueBuffer.wrap(new byte[] { 0, 0 });
        byte[] threeBytes = new byte[3];
        assertThrows(BufferOverflowException.class, () -> wrap.get(threeBytes));
    public void testBulkGetAtCapacity() {
        ValueBuffer wrap = ValueBuffer.wrap(new byte[] { 1, 2 });
        byte[] twoBytes = new byte[2];
        wrap.get(twoBytes);
        assertEquals(1, twoBytes[0]);
        assertEquals(2, twoBytes[1]);
        assertEquals(2, wrap.position());
    public void testBulkGet() {
        ValueBuffer wrap = ValueBuffer.wrap(new byte[] { 1, 2, 3 });
        byte[] onebyte = new byte[1];
        wrap.get(onebyte);
        assertEquals(1, onebyte[0]);
        assertEquals(1, wrap.position());
        // non-zero position
        wrap.position(1);
        assertEquals(2, twoBytes[0]);
        assertEquals(3, twoBytes[1]);
