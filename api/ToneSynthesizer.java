import java.io.ByteArrayOutputStream;
import java.nio.ByteOrder;
 * An audio tone synthesizer. A utility to sent tone melodies to audio sinks.
 * Limited to wav little endian streams.
 * @author Miguel Álvarez - Initial contribution
public class ToneSynthesizer {
    private final long sampleRate;
    private final int bitDepth;
    private final int bitRate;
    private final int channels;
    private final boolean bigEndian;
    public static Set<AudioFormat> getSupportedFormats() {
        return Set.of(new AudioFormat(AudioFormat.CONTAINER_WAVE, AudioFormat.CODEC_PCM_SIGNED, false, null, null, null,
     * Parses a tone melody into a list of {@link Tone} instances.
     * @param melody to be parsed.
     * @return list of {@link Tone} instances.
     * @throws ParseException if melody can not be played.
    public static List<Tone> parseMelody(String melody) throws ParseException {
        var melodySounds = new ArrayList<Tone>();
        var noteTextList = melody.split("\\s");
        var melodyTextIndex = 0;
        for (String noteText : noteTextList) {
            var noteTextParts = noteText.split(":");
            var soundMillis = 200;
            switch (noteTextParts.length) {
                        soundMillis = Integer.parseInt(noteTextParts[1]);
                        throw new ParseException("Unable to parse note duration " + noteText, melodyTextIndex);
                    var note = noteTextParts[0];
                    int octaves = (int) note.chars().filter(ch -> ch == '\'').count();
                    note = note.replace("'", "");
                    var noteObj = Note.fromString(note);
                    if (noteObj.isPresent()) {
                        melodySounds.add(noteTone(noteObj.get(), soundMillis, octaves));
                    } else if ("O".equals(note) || "0".equals(note)) {
                        melodySounds.add(silenceTone(soundMillis));
                    throw new ParseException("Unable to parse note " + noteText, melodyTextIndex);
            melodyTextIndex += noteText.length() + 1;
        return melodySounds;
    public static Tone noteTone(Note note, long millis) {
        return noteTone(note, millis, 0);
    public static Tone noteTone(Note note, long millis, int octaves) {
        return new Tone(note.getFrequency() * (octaves + 1), millis);
    public static Tone silenceTone(long millis) {
        return new Tone(0.0, millis);
    public ToneSynthesizer(AudioFormat audioFormat) {
        this.sampleRate = Objects.requireNonNull(audioFormat.getFrequency());
        this.bitDepth = Objects.requireNonNull(audioFormat.getBitDepth());
        this.bitRate = Objects.requireNonNull(audioFormat.getBitRate());
        this.channels = Objects.requireNonNull(audioFormat.getChannels());
        this.bigEndian = Objects.requireNonNull(audioFormat.isBigEndian());
     * Synthesize a list of {@link Tone} into a wav audio stream
     * @param tones the list of {@link Tone}
     * @return an audio stream with the synthesized tones
     * @throws IOException in case of problems writing the audio stream
    public AudioStream getStream(List<Tone> tones) throws IOException {
        int byteRate = (int) (sampleRate * bitDepth * channels / 8);
        byte[] audioBuffer = {};
        var fixedTones = new ArrayList<>(tones);
        fixedTones.add(silenceTone(100));
        for (var sound : fixedTones) {
            var frequency = sound.frequency;
            var millis = sound.millis;
            int samplesPerChannel = (int) Math.ceil(sampleRate * (((double) millis) / 1000));
            byte[] audioPart = getAudioBytes(frequency, samplesPerChannel);
            audioBuffer = ByteBuffer.allocate(audioBuffer.length + audioPart.length).put(audioBuffer).put(audioPart)
                    .array();
        // ensure min audio size
        int minByteSize = (int) Math.ceil(byteRate * 0.5);
        if (audioBuffer.length < minByteSize) {
            // ensure min duration of half second, prevents issue with pulseaudio sink
            byte[] padBytes = new byte[minByteSize - audioBuffer.length];
            audioBuffer = ByteBuffer.allocate(minByteSize).put(padBytes).put(audioBuffer).array();
        if (!bigEndian) {
            // for little endian add the RIFF header to the stream to increase compatibility
            return getAudioStreamWithRIFFHeader(audioBuffer);
            return getAudioStream(audioBuffer);
    private double getSample(double frequency, int sampleNum) {
        return 0xfff * Math.sin(frequency * (2 * Math.PI) * sampleNum / sampleRate);
    private ByteArrayAudioStream getAudioStreamWithRIFFHeader(byte[] audioBytes) throws IOException {
        var jAudioFormat = new javax.sound.sampled.AudioFormat(sampleRate, bitDepth, channels, true, bigEndian);
        AudioInputStream audioInputStreamTemp = new AudioInputStream(new ByteArrayInputStream(audioBytes), jAudioFormat,
                (long) Math.ceil(((double) audioBytes.length) / jAudioFormat.getFrameSize()));
        ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
        AudioSystem.write(audioInputStreamTemp, AudioFileFormat.Type.WAVE, outputStream);
        return getAudioStream(outputStream.toByteArray());
    private ByteArrayAudioStream getAudioStream(byte[] audioBytes) {
        return new ByteArrayAudioStream(audioBytes, new AudioFormat(AudioFormat.CONTAINER_WAVE,
                AudioFormat.CODEC_PCM_SIGNED, bigEndian, bitDepth, bitRate, sampleRate, channels));
    private byte[] getAudioBytes(double frequency, int samplesPerChannel) {
        var audioBuffer = ByteBuffer.allocate(samplesPerChannel * (bitDepth / 8) * channels);
        audioBuffer.order(bigEndian ? ByteOrder.BIG_ENDIAN : ByteOrder.LITTLE_ENDIAN);
        for (int i = 0; i < samplesPerChannel; i++) {
            short sample = (short) getSample(frequency, i);
            for (int c = 0; c < channels; c++) {
                switch (bitDepth) {
                        audioBuffer.put((byte) (sample & 0xff));
                    case 16:
                        audioBuffer.putShort(sample);
                    case 24:
                        putInt24Bits(audioBuffer, (sample) << 8);
                        audioBuffer.putInt((sample) << 16);
        return audioBuffer.array();
    private void putInt24Bits(ByteBuffer buffer, int value) {
        if (bigEndian) {
            buffer.put((byte) ((value >> 16) & 0xff));
            buffer.put((byte) ((value >> 8) & 0xff));
            buffer.put((byte) (value & 0xff));
    public static class Tone {
        private final double frequency;
        private final long millis;
        private Tone(double frequency, long millis) {
            this.millis = millis;
    public enum Note {
        B(List.of("B", "Si"), 493.88),
        Bb(List.of("A#", "Bb", "LA#", "SIb"), 466.16),
        A(List.of("A", "LA"), 440.0),
        Ab(List.of("G#", "Ab", "SOL#", "LAb"), 415.30),
        G(List.of("G", "SOL"), 392.0),
        Gb(List.of("F#", "Gb", "FA#", "SOLb"), 369.99),
        F(List.of("F", "FA"), 349.23),
        E(List.of("E", "MI"), 329.63),
        Eb(List.of("D#", "Eb", "RE#", "MIb"), 311.13),
        D(List.of("D", "RE"), 293.66),
        Cb(List.of("C#", "Db", "DO#", "REb"), 277.18),
        C(List.of("C", "DO"), 261.63);
        private final List<String> names;
        Note(List<String> names, double frequency) {
        public double getFrequency() {
        public static Optional<Note> fromString(String note) {
            return Arrays.stream(Note.values())
                    .filter(note1 -> note1.names.stream().filter(note::equalsIgnoreCase).count() == 1).findAny();
