package org.openhab.core.automation.module.media.internal;
import java.math.BigDecimal;
import org.openhab.core.automation.Visibility;
import org.openhab.core.automation.type.ActionType;
import org.openhab.core.automation.type.ModuleType;
import org.openhab.core.automation.type.ModuleTypeProvider;
import org.openhab.core.config.core.ConfigDescriptionParameter.Type;
import org.openhab.core.config.core.ConfigDescriptionParameterBuilder;
 * This class dynamically provides the Play, Say and Synthesize action types.
 * This is necessary since there is no other way to provide dynamic config param options for module types.
 * @author Simon Kaufmann - added "say" action
 * @author Christoph Weitkamp - Added parameter volume
@Component(service = ModuleTypeProvider.class)
public class MediaActionTypeProvider implements ModuleTypeProvider {
    public MediaActionTypeProvider(final @Reference AudioManager audioManager) {
    public @Nullable ModuleType getModuleType(String uid, @Nullable Locale locale) {
        return switch (uid) {
            case PlayActionHandler.TYPE_ID -> getPlayActionType(locale);
            case SayActionHandler.TYPE_ID -> getSayActionType(locale);
            case SynthesizeActionHandler.TYPE_ID -> getSynthesizeActionType(locale);
            default -> null;
    public Collection<ModuleType> getModuleTypes(@Nullable Locale locale) {
        return List.of(getPlayActionType(locale), getSayActionType(locale));
    private ModuleType getPlayActionType(@Nullable Locale locale) {
        return new ActionType(PlayActionHandler.TYPE_ID, getConfigPlayDesc(locale), "play a sound",
                "Plays a sound file. Optionally sets the volume.", null, Visibility.VISIBLE, null, null);
    private ModuleType getSayActionType(@Nullable Locale locale) {
        return new ActionType(SayActionHandler.TYPE_ID, getConfigSayDesc(locale), "say something",
                "Speaks a given text through a natural voice. Optionally sets the volume.", null, Visibility.VISIBLE,
    private ModuleType getSynthesizeActionType(@Nullable Locale locale) {
        return new ActionType(SynthesizeActionHandler.TYPE_ID, getConfigSynthesizeDesc(locale),
                "synthesize a tone melody", "Synthesize the given melody text and play it. Optionally sets the volume.",
                null, Visibility.VISIBLE, null, null);
    private List<ConfigDescriptionParameter> getConfigPlayDesc(@Nullable Locale locale) {
                ConfigDescriptionParameterBuilder.create(PlayActionHandler.PARAM_SOUND, Type.TEXT).withRequired(true)
                        .withLabel("Sound").withDescription("the sound to play").withOptions(getSoundOptions())
                        .withLimitToOptions(true).build(),
                getAudioSinkConfigDescParam(locale), getVolumeConfigDescParam(locale));
    private List<ConfigDescriptionParameter> getConfigSayDesc(@Nullable Locale locale) {
                ConfigDescriptionParameterBuilder.create(SayActionHandler.PARAM_TEXT, Type.TEXT).withRequired(true)
                        .withLabel("Text").withDescription("the text to speak").build(),
    private List<ConfigDescriptionParameter> getConfigSynthesizeDesc(@Nullable Locale locale) {
                ConfigDescriptionParameterBuilder.create(SynthesizeActionHandler.PARAM_MELODY, Type.TEXT)
                        .withRequired(true).withLabel("Melody")
                        .withDescription("the melody as spaced separated note names").build(),
    private ConfigDescriptionParameter getAudioSinkConfigDescParam(@Nullable Locale locale) {
        return ConfigDescriptionParameterBuilder.create(SayActionHandler.PARAM_SINK, Type.TEXT).withRequired(false)
                .withLabel("Sink").withDescription("the audio sink id").withOptions(getSinkOptions(locale))
                .withLimitToOptions(true).build();
    private ConfigDescriptionParameter getVolumeConfigDescParam(@Nullable Locale locale) {
        return ConfigDescriptionParameterBuilder.create(SayActionHandler.PARAM_VOLUME, Type.INTEGER).withLabel("Volume")
                .withDescription("the volume to use").withMinimum(BigDecimal.ZERO).withMaximum(BigDecimal.valueOf(100))
                .withStepSize(BigDecimal.ONE).build();
     * This method creates one option for every file that is found in the sounds directory.
     * As a label, the file extension is removed and the string is capitalized.
     * @return a list of parameter options representing the sound files
    private List<ParameterOption> getSoundOptions() {
        List<ParameterOption> options = new ArrayList<>();
        File soundsDir = Path.of(OpenHAB.getConfigFolder(), AudioManager.SOUND_DIR).toFile();
        if (soundsDir.isDirectory()) {
            for (String fileName : soundsDir.list()) {
                if (fileName.contains(".") && !fileName.startsWith(".")) {
                    String soundName = fileName.substring(0, fileName.lastIndexOf("."));
                    String capitalizedSoundName = soundName.substring(0, 1).toUpperCase()
                            + soundName.substring(1).toLowerCase();
                    options.add(new ParameterOption(fileName, capitalizedSoundName));
            options.sort(comparing(ParameterOption::getLabel));
     * This method creates one option for every sink that is found in the system.
     * @return a list of parameter options representing the audio sinks
    private List<ParameterOption> getSinkOptions(@Nullable Locale locale) {
        return audioManager.getAllSinks().stream().sorted(comparing(s -> s.getLabel(safeLocale)))
    public void addProviderChangeListener(ProviderChangeListener<ModuleType> listener) {
        // does nothing because this provider does not change
    public Collection<ModuleType> getAll() {
        return getModuleTypes(null);
    public void removeProviderChangeListener(ProviderChangeListener<ModuleType> listener) {
