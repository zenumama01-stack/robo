package org.openhab.core.audio.utils;
 * Some utility methods for sink
public interface AudioSinkUtils {
     * Transfers data from an input stream to an output stream and computes on the fly its duration
     * @param in the input stream giving audio data ta play
     * @param out the output stream receiving data to play
     * @return the timestamp (from System.nanoTime) when the sound should be fully played. Returns null if computing
     *         time fails.
     * @throws IOException if reading from the stream or writing to the stream failed
    Long transferAndAnalyzeLength(InputStream in, OutputStream out, AudioFormat audioFormat) throws IOException;
