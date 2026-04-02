 * Tests for 'special' float values such as infinity and NaN. These are not covered in detail in
 * {@link BitUtilitiesExtractIndividualMethodsTest} and
 * {@link BitUtilitiesExtractStateFromRegistersTest}
public class BitUtilitiesExtractFloat32Test {
     * Creates a byte array with byteOffset number of zeroes, followed by 32bit of data represented by data
     * @param data actual data payload
     * @param byteOffset number of zeros padded
     * @return byte array of size 4 + byteOffset
    private static byte[] bytes(int data, int byteOffset) {
        ByteBuffer buffer = ByteBuffer.allocate(4 + byteOffset);
        for (int i = 0; i < byteOffset; i++) {
            buffer.put((byte) 0);
        buffer.putInt(data);
        return buffer.array();
    private static void testFloat(float number) {
        int data = Float.floatToIntBits(number);
        for (int byteOffset = 0; byteOffset < 5; byteOffset++) {
            byte[] bytes = bytes(data, byteOffset);
            float actual = ModbusBitUtilities.extractFloat32(bytes, byteOffset);
            float expected = Float.intBitsToFloat(data);
            // Strict comparison of the float values with the exception of NaN
            assertTrue(Float.isNaN(expected) ? Float.isNaN(actual) : expected == actual,
                    String.format("Testing %f (%s) with offset %d, got %f (%s)", expected, Integer.toBinaryString(data),
                            byteOffset, actual, Integer.toBinaryString(Float.floatToRawIntBits(actual))));
    public void testExtractFloat32Inf() {
        testFloat(Float.POSITIVE_INFINITY);
    public void testExtractFloat32NegInf() {
        testFloat(Float.NEGATIVE_INFINITY);
    public void testExtractFloat32NaN() {
        testFloat(Float.NaN);
    public void testExtractFloat32Regular() {
        testFloat(1.3f);
