 * Test SpeechRecognitionEvent event
public class SpeechRecognitionEventTest {
     * Test SpeechRecognitionEvent(String, float) constructor
        SpeechRecognitionEvent sRE = new SpeechRecognitionEvent("Message", 0.5f);
        assertNotNull(sRE, "SpeechRecognitionEvent(String, float) constructor failed");
     * Test SpeechRecognitionEvent.getTranscript() method
    public void getTranscriptTest() {
        assertEquals("Message", sRE.getTranscript(), "SpeechRecognitionEvent.getTranscript() method failed");
     * Test SpeechRecognitionEvent.getConfidence() method
    public void getConfidenceTest() {
        assertEquals((double) 0.5f, (double) sRE.getConfidence(), (double) 0.001f,
                "SpeechRecognitionEvent.getConfidence() method failed");
