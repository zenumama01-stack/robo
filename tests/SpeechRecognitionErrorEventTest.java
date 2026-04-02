 * Test SpeechRecognitionErrorEvent event
public class SpeechRecognitionErrorEventTest {
     * Test SpeechRecognitionErrorEvent(String) constructor
        SpeechRecognitionErrorEvent sRE = new SpeechRecognitionErrorEvent("Message");
        assertNotNull(sRE, "SpeechRecognitionErrorEvent(String) constructor failed");
     * Test SpeechRecognitionErrorEvent.getMessage() method
    public void getMessageTest() {
        assertEquals("Message", sRE.getMessage(), "SpeechRecognitionErrorEvent.getMessage() method failed");
