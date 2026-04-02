package org.openhab.core.io.rest.audio.internal;
 * Mapper class that maps {@link AudioSink} and {@link AudioSource} instanced to their respective DTOs.
public class AudioMapper {
     * Maps an {@link AudioSource} to an {@link AudioSourceDTO}.
     * @param source the audio source
     * @param locale the locale to use for the DTO
     * @return the corresponding DTO
    public static AudioSourceDTO map(AudioSource source, Locale locale) {
        return new AudioSourceDTO(source.getId(), source.getLabel(locale));
     * Maps an {@link AudioSink} to an {@link AudioSinkDTO}.
     * @param sink the audio sink
    public static AudioSinkDTO map(AudioSink sink, Locale locale) {
        return new AudioSinkDTO(sink.getId(), sink.getLabel(locale));
