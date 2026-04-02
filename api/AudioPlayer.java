package org.openhab.core.audio.internal.javasound;
import javax.sound.sampled.AudioFormat;
import javax.sound.sampled.DataLine;
import javax.sound.sampled.Line.Info;
import javax.sound.sampled.Mixer;
import javax.sound.sampled.SourceDataLine;
 * This is a class that plays an AudioStream through the Java sound API
 * @author Kelly Davis - Initial contribution and API
 * @author Kai Kreuzer - Refactored to use AudioStream and logging
public class AudioPlayer extends Thread {
    private final Logger logger = LoggerFactory.getLogger(AudioPlayer.class);
     * The AudioStream to play
    private final AudioStream audioStream;
     * Constructs an AudioPlayer to play the passed AudioSource
     * @param audioStream The AudioStream to play
    public AudioPlayer(AudioStream audioStream) {
        this.audioStream = audioStream;
     * This method plays the contained AudioSource
    public void run() {
        SourceDataLine line;
        org.openhab.core.audio.AudioFormat openhabAudioFormat = audioStream.getFormat();
        AudioFormat audioFormat = convertAudioFormat(openhabAudioFormat);
            logger.warn("Audio format is unsupported or does not have enough details in order to be played");
        DataLine.Info info = new DataLine.Info(SourceDataLine.class, audioFormat);
            line = (SourceDataLine) AudioSystem.getLine(info);
            line.open(audioFormat);
            logger.warn("No line found: {}", e.getMessage());
            logger.info("Available lines are:");
            Mixer.Info[] mixerInfo = AudioSystem.getMixerInfo(); // get available mixers
            Mixer mixer;
            for (Mixer.Info value : mixerInfo) {
                mixer = AudioSystem.getMixer(value);
                Info[] lineInfos = mixer.getSourceLineInfo();
                for (Info lineInfo : lineInfos) {
                    logger.info("{}", lineInfo);
        line.start();
        int nRead = 0;
        byte[] abData = new byte[65532]; // needs to be a multiple of 4 and 6, to support both 16 and 24 bit stereo
            // If this is a wav container, we should remove the header from the stream
            // to avoid a "clack" noise at the beginning
            if (org.openhab.core.audio.AudioFormat.CONTAINER_WAVE.equals(openhabAudioFormat.getContainer())) {
            while (-1 != nRead) {
                nRead = audioStream.read(abData, 0, abData.length);
                if (nRead >= 0) {
                    line.write(abData, 0, nRead);
            logger.error("Error while playing audio: {}", e.getMessage());
            line.drain();
            line.close();
     * Converts an org.openhab.core.audio.AudioFormat
     * to a javax.sound.sampled.AudioFormat
     * @param audioFormat The AudioFormat to convert
     * @return The corresponding AudioFormat
    protected @Nullable AudioFormat convertAudioFormat(org.openhab.core.audio.AudioFormat audioFormat) {
        String codec = audioFormat.getCodec();
        AudioFormat.Encoding encoding = new AudioFormat.Encoding(codec);
        if (org.openhab.core.audio.AudioFormat.CODEC_PCM_SIGNED.equals(codec)) {
            encoding = AudioFormat.Encoding.PCM_SIGNED;
        } else if (org.openhab.core.audio.AudioFormat.CODEC_PCM_ULAW.equals(codec)) {
            encoding = AudioFormat.Encoding.ULAW;
        } else if (org.openhab.core.audio.AudioFormat.CODEC_PCM_ALAW.equals(codec)) {
            encoding = AudioFormat.Encoding.ALAW;
        final Long frequency = audioFormat.getFrequency();
        if (frequency == null) {
        final float sampleRate = frequency.floatValue();
        final Integer bitDepth = audioFormat.getBitDepth();
        if (bitDepth == null) {
        final int sampleSizeInBits = bitDepth.intValue();
        final int channels = audioFormat.getChannels() == null ? Integer.valueOf(1) : audioFormat.getChannels();
        final int frameSize = channels * sampleSizeInBits / 8;
        final float frameRate = channels * sampleRate / frameSize;
        final Boolean bigEndian = audioFormat.isBigEndian();
        if (bigEndian == null) {
        return new AudioFormat(encoding, sampleRate, sampleSizeInBits, channels, frameSize, frameRate, bigEndian);
