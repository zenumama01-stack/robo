package org.openhab.core.voice.internal.cache;
 * Serializable AudioFormat storage class
 * We cannot use a record yet (requires Gson v2.10)
public class AudioFormatInfo {
    public final @Nullable Boolean bigEndian;
    public final @Nullable Integer bitDepth;
    public final @Nullable Integer bitRate;
    public final @Nullable Long frequency;
    public final @Nullable Integer channels;
    public final @Nullable String codec;
    public final @Nullable String container;
    public AudioFormatInfo(String text, @Nullable Boolean bigEndian, @Nullable Integer bitDepth,
            @Nullable Integer bitRate, @Nullable Long frequency, @Nullable Integer channels, @Nullable String codec,
            @Nullable String container) {
    public AudioFormatInfo(AudioFormat audioFormat) {
        this.bigEndian = audioFormat.isBigEndian();
        this.bitDepth = audioFormat.getBitDepth();
        this.bitRate = audioFormat.getBitRate();
        this.frequency = audioFormat.getFrequency();
        this.channels = audioFormat.getChannels();
        this.codec = audioFormat.getCodec();
        this.container = audioFormat.getContainer();
    public AudioFormat toAudioFormat() {
        return new AudioFormat(container, codec, bigEndian, bitDepth, bitRate, frequency, channels);
