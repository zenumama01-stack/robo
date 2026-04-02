 * Unit tests for {@link Bin2Json}.
public class Bin2JsonTest {
    public void testParserRuleError() throws ConversionException {
        assertThrows(ConversionException.class,
                () -> new Bin2Json("byte a byte b ubyte c;").convert(new byte[] { 3, 34, (byte) 255 }));
    public void testHexStringData() throws ConversionException {
        JsonObject json = new Bin2Json("byte a; byte b; ubyte c;").convert("03FAFF");
        assertEquals("{\"a\":3,\"b\":-6,\"c\":255}", json.toString());
    public void testHexStringDataError() throws ConversionException {
        assertThrows(ConversionException.class, () -> new Bin2Json("byte a; byte b; ubyte c;").convert("0322F"));
    public void testByteArrayData() throws ConversionException {
        JsonObject json = new Bin2Json("ubyte length; ubyte[length] data;")
                .convert(new byte[] { 4, 8, 33, 1, 2, 3, 4 });
        assertEquals("{\"length\":4,\"data\":[8,33,1,2]}", json.toString());
    public void testByteArrayDataError() throws ConversionException {
                () -> new Bin2Json("byte a; byte b; ubyte c;").convert(new byte[] { 3, 34 }));
    public void testInputStreamData() throws ConversionException, IOException {
        InputStream inputStream = new ByteArrayInputStream(new byte[] { 4, 8, 33, 1, 2, 3, 4 });
        JsonObject json = new Bin2Json("ubyte length; ubyte[length] data;").convert(inputStream);
    public void testInputStreamDataError() throws ConversionException {
        InputStream inputStream = new ByteArrayInputStream(new byte[] { 4, 8, 33 });
                () -> new Bin2Json("ubyte length; ubyte[length] data;").convert(inputStream));
