 * Test general purpose STT exception
public class STTExceptionTest {
     * Test STTException() constructor
    public void testConstructor0() {
        STTException ttsException = new STTException();
        assertNotNull(ttsException, "STTException() constructor failed");
     * Test STTException(String message, Throwable cause) constructor
    public void testConstructor1() {
        STTException ttsException = new STTException("Message", new Throwable());
        assertNotNull(ttsException, "STTException(String, Throwable) constructor failed");
     * Test STTException(String message) constructor
    public void testConstructor2() {
        STTException ttsException = new STTException("Message");
        assertNotNull(ttsException, "STTException(String) constructor failed");
     * Test STTException(Throwable cause) constructor
    public void testConstructor3() {
        STTException ttsException = new STTException(new Throwable());
        assertNotNull(ttsException, "STTException(Throwable) constructor failed");
