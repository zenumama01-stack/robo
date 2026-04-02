 * Utils to read/write the audio packets send though the websocket binary protocol.
public final class PCMWebSocketStreamIdUtil {
    private PCMWebSocketStreamIdUtil() {
     * Packet header length in bytes:
     * <li>2 for id</li>
     * <li>4 for sample rate as int little-endian</li>
     * <li>1 for bitDepth</li>
     * <li>1 for channels</li>
    public static final int PACKET_HEADER_BYTE_LENGTH = 2 + 4 + 1 + 1;
    public static AudioPacketData parseAudioPacket(byte[] bytes) throws IOException {
        if (bytes.length < PACKET_HEADER_BYTE_LENGTH) {
            throw new IOException("Audio packet byte length is too small, invalid data.");
        var byteBuffer = ByteBuffer.wrap(bytes).order(ByteOrder.LITTLE_ENDIAN);
        byte[] idBytes = new byte[] { byteBuffer.get(), byteBuffer.get() };
        int sampleRate = byteBuffer.getInt();
        byte bitDepth = byteBuffer.get();
        byte channels = byteBuffer.get();
        byte[] audioBytes = new byte[byteBuffer.remaining()];
        byteBuffer.get(audioBytes);
        return new AudioPacketData(idBytes, sampleRate, bitDepth, channels, audioBytes);
    public static ByteBuffer generateAudioPacketHeader(int sampleRate, byte bitDepth, byte channels) {
        var byteBuffer = ByteBuffer.allocate(PACKET_HEADER_BYTE_LENGTH).order(ByteOrder.LITTLE_ENDIAN);
        SecureRandom sr = new SecureRandom();
        byte[] rndBytes = new byte[2];
        sr.nextBytes(rndBytes);
        byteBuffer.put(rndBytes[0]);
        byteBuffer.put(rndBytes[1]);
        byteBuffer.putInt(sampleRate);
        byteBuffer.put(bitDepth);
        byteBuffer.put(channels);
        return byteBuffer;
    public record AudioPacketData(byte[] id, int sampleRate, byte bitDepth, byte channels, byte[] audioData) {
