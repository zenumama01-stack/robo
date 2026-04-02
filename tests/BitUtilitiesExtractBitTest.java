 * Tests for extractBit
public class BitUtilitiesExtractBitTest {
    public void testExtractBitWithRegisterIndexAndBitIndex() {
        byte[] bytes = new byte[] { 0b00100001, // hi byte of 1st register
                0b00100101, // lo byte of 1st register
                0b00110001, // hi byte of 2nd register
                0b00101001 }; // lo byte of 2nd register
            int registerIndex = 0;
            int[] expectedBitsFromLSBtoMSB = new int[] { //
                    1, 0, 1, 0, 0, 1, 0, 0, // lo byte, with increasing significance
                    1, 0, 0, 0, 0, 1, 0, 0 // hi byte, with increasing significance
            for (int bitIndex = 0; bitIndex < expectedBitsFromLSBtoMSB.length; bitIndex++) {
                assertEquals(expectedBitsFromLSBtoMSB[bitIndex],
                        ModbusBitUtilities.extractBit(bytes, registerIndex, bitIndex),
                        String.format("bitIndex=%d", bitIndex));
            int registerIndex = 1;
                    1, 0, 0, 1, 0, 1, 0, 0, // lo byte, with increasing significance
                    1, 0, 0, 0, 1, 1, 0, 0 // hi byte, with increasing significance
    public void testExtractBitWithRegisterIndexAndBitIndexOOB() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractBit(bytes, 3, 0));
    public void testExtractBitWithRegisterIndexAndBitIndexOOB2() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractBit(bytes, 0, 17));
    public void testExtractBitWithRegisterIndexAndBitIndexOOB3() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractBit(bytes, 0, -1));
    public void testExtractBitWithRegisterIndexAndBitIndexOOB4() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractBit(bytes, -1, 0));
    public void testExtractBitWithSingleIndex() {
        int[] expectedBits = new int[] { //
                1, 0, 1, 0, 0, 1, 0, 0, // 1st register: lo byte, with increasing significance
                1, 0, 0, 0, 0, 1, 0, 0, // 1st register: hi byte, with increasing significance
                1, 0, 0, 1, 0, 1, 0, 0, // 2nd register: lo byte, with increasing significance
                1, 0, 0, 0, 1, 1, 0, 0 // 2nd register: hi byte, with increasing significance
        for (int bitIndex = 0; bitIndex < expectedBits.length; bitIndex++) {
            assertEquals(expectedBits[bitIndex], ModbusBitUtilities.extractBit(bytes, bitIndex),
    public void testExtractBitWithSingleIndexOOB() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractBit(bytes, 32));
    public void testExtractBitWithSingleIndexOOB2() {
        assertThrows(IllegalArgumentException.class, () -> ModbusBitUtilities.extractBit(bytes, -1));
