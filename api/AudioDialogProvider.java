package org.openhab.core.audio;
 * This interface is designed so the voice bundle can inject the start dialog functionality for audio bundle to consume,
 * which a programmatic way of trigger the dialog execution.
 * @author Miguel Álvarez Díez - Initial contribution
public interface AudioDialogProvider {
     * Starts a dialog and returns a runnable that triggers it or null if the dialog initialization fails
     * @param audioSink the audio sink to play sound
     * @param audioSource the audio source to capture sound
     * @param locationItem an optional Item name to scope dialog commands
     * @param listeningItem an optional Item name to toggle while dialog is listening, overwrites default
     * @param onAbort an optional {@link Runnable} instance to call on abort.
     * @return a {@link Runnable} instance to trigger dialog processing or null if the dialog initialization fails
    Runnable startDialog(AudioSink audioSink, AudioSource audioSource, @Nullable String locationItem,
            @Nullable String listeningItem, @Nullable Runnable onAbort);
