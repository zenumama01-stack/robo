 * Tests for extractSInt8 and extractUInt8
public class BitUtilitiesExtractInt8Test {
    public void extractSInt8WithSingleIndex() {
        byte[] bytes = new byte[] { -1, 2, 3 };
        assertEquals(-1, ModbusBitUtilities.extractSInt8(bytes, 0));
        assertEquals(2, ModbusBitUtilities.extractSInt8(bytes, 1));
        assertEquals(3, ModbusBitUtilities.extractSInt8(bytes, 2));
    public void extractSInt8WithSingleIndexOOB() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractSInt8(bytes, 3));
    public void extractSInt8WithSingleIndexOOB2() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractSInt8(bytes, -1));
    public void extractSInt8WithRegisterIndexAndHiByte() {
        byte[] bytes = new byte[] { -1, 2, 3, 4 };
        assertEquals(-1, ModbusBitUtilities.extractSInt8(bytes, 0, true));
        assertEquals(2, ModbusBitUtilities.extractSInt8(bytes, 0, false));
        assertEquals(3, ModbusBitUtilities.extractSInt8(bytes, 1, true));
        assertEquals(4, ModbusBitUtilities.extractSInt8(bytes, 1, false));
    public void extractSInt8WithRegisterIndexAndHiByteOOB() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractSInt8(bytes, 2, true));
    public void extractSInt8WithRegisterIndexAndHiByteOOB2() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractSInt8(bytes, -1, true));
    // unsigned int8 follows
    public void extractUInt8WithSingleIndex() {
        assertEquals(255, ModbusBitUtilities.extractUInt8(bytes, 0));
        assertEquals(2, ModbusBitUtilities.extractUInt8(bytes, 1));
        assertEquals(3, ModbusBitUtilities.extractUInt8(bytes, 2));
    public void extractUInt8WithSingleIndexOOB() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractUInt8(bytes, 3));
    public void extractUInt8WithSingleIndexOOB2() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractUInt8(bytes, -1));
    public void extractUInt8WithRegisterIndexAndHiByte() {
        assertEquals(255, ModbusBitUtilities.extractUInt8(bytes, 0, true));
        assertEquals(2, ModbusBitUtilities.extractUInt8(bytes, 0, false));
        assertEquals(3, ModbusBitUtilities.extractUInt8(bytes, 1, true));
        assertEquals(4, ModbusBitUtilities.extractUInt8(bytes, 1, false));
    public void extractUInt8WithRegisterIndexAndHiByteOOB() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractUInt8(bytes, 2, true));
    public void extractUInt8WithRegisterIndexAndHiByteOOB2() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractUInt8(bytes, 255, true));
