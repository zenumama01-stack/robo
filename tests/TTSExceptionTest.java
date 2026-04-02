 * Test general purpose TTS exception
public class TTSExceptionTest {
     * Test TTSException() constructor
        TTSException ttsException = new TTSException();
        assertNotNull(ttsException, "TTSException() constructor failed");
     * Test TTSException(String message, Throwable cause) constructor
        TTSException ttsException = new TTSException("Message", new Throwable());
        assertNotNull(ttsException, "TTSException(String, Throwable) constructor failed");
     * Test TTSException(String message) constructor
        TTSException ttsException = new TTSException("Message");
        assertNotNull(ttsException, "TTSException(String) constructor failed");
     * Test TTSException(Throwable cause) constructor
        TTSException ttsException = new TTSException(new Throwable());
        assertNotNull(ttsException, "TTSException(Throwable) constructor failed");
