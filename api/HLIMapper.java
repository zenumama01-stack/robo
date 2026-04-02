package org.openhab.core.io.rest.voice.internal;
import org.openhab.core.voice.text.HumanLanguageInterpreter;
 * Mapper class that maps {@link HumanLanguageInterpreter} instanced to their respective DTOs.
public class HLIMapper {
     * Maps a {@link HumanLanguageInterpreter} to a {@link HumanLanguageInterpreterDTO}.
     * @param hli the human language interpreter
    public static HumanLanguageInterpreterDTO map(HumanLanguageInterpreter hli, Locale locale) {
        HumanLanguageInterpreterDTO dto = new HumanLanguageInterpreterDTO();
        dto.id = hli.getId();
        dto.label = hli.getLabel(locale);
        final Set<Locale> supportedLocales = hli.getSupportedLocales();
        if (supportedLocales != null) {
            dto.locales = new HashSet<>(supportedLocales.size());
            for (final Locale supportedLocale : supportedLocales) {
                dto.locales.add(supportedLocale.toString());
