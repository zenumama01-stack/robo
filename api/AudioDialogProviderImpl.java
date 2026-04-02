package org.openhab.core.voice.internal;
import org.openhab.core.voice.BasicDTService;
import org.openhab.core.voice.DTListener;
import org.openhab.core.voice.DTService;
import org.openhab.core.voice.DTServiceHandle;
import org.openhab.core.voice.DTTriggeredEvent;
 * Allows the audio bundle to register a dialog that can be triggered programmatically.
@Component(service = AudioDialogProvider.class)
public class AudioDialogProviderImpl implements AudioDialogProvider {
    private final Logger logger = LoggerFactory.getLogger(AudioDialogProviderImpl.class);
    public AudioDialogProviderImpl(@Reference VoiceManager voiceManager) {
    public @Nullable Runnable startDialog(AudioSink audioSink, AudioSource audioSource, @Nullable String locationItem,
            @Nullable String listeningItem, @Nullable Runnable onAbort) {
        DTService dt = new TriggerService(onAbort);
        TriggerServiceHandle triggerHandle = (TriggerServiceHandle) voiceManager.startDialog( //
                voiceManager.getDialogContextBuilder() //
                        .withSource(audioSource) //
                        .withSink(audioSink) //
                        .withLocationItem(locationItem) //
                        .withListeningItem(listeningItem) //
                        .withDT(dt) //
                        .build() //
        if (triggerHandle == null) {
        return triggerHandle::trigger;
    private static class TriggerService implements BasicDTService {
        Runnable onAbort;
        public TriggerService(@Nullable Runnable onAbort) {
            this.onAbort = onAbort;
        public DTServiceHandle registerListener(DTListener dtListener) {
            return new TriggerServiceHandle(dtListener, onAbort);
            return "audiodialog::anonymous::trigger";
            // never shown
            return "Anonymous";
    private static class TriggerServiceHandle implements DTServiceHandle {
        public final DTListener dtListener;
        public @Nullable Runnable abortCallback;
        public TriggerServiceHandle(DTListener dtListener, @Nullable Runnable abortCallback) {
            this.dtListener = dtListener;
            this.abortCallback = abortCallback;
        public void trigger() {
            dtListener.dtEventReceived(new DTTriggeredEvent());
        public void abort() {
            if (this.abortCallback != null) {
                this.abortCallback.run();
                this.abortCallback = null;
