 * This is an audio source, which can provide a continuous live stream of audio.
 * Its main use is for microphones and other "line-in" sources and it can be registered as a service in order to make
 * it available throughout the system.
public interface AudioSource {
     * Obtain the audio formats supported by this AudioSource
     * @return The audio formats supported by this service
     * Gets an AudioStream for reading audio data in supported audio format
     * @param format the expected audio format of the stream
     * @return AudioStream for reading audio data
     * @throws AudioException If problem occurs obtaining the stream
    AudioStream getInputStream(AudioFormat format) throws AudioException;
