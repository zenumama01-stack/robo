import org.openhab.core.model.script.engine.action.ActionDoc;
import org.openhab.core.model.script.engine.action.ParamDoc;
import org.openhab.core.model.script.internal.engine.action.AudioActionService;
 * This allows a script to use audio features.
public class Audio {
    private static final Logger logger = LoggerFactory.getLogger(Audio.class);
    @ActionDoc(text = "plays a sound from the sounds folder to the default sink")
    public static void playSound(@ParamDoc(name = "filename", text = "the filename with extension") String filename) {
            AudioActionService.audioManager.playFile(filename);
            logger.warn("Failed playing audio file: {}", e.getMessage());
    @ActionDoc(text = "plays a sound with the given volume from the sounds folder to the default sink")
    public static void playSound(@ParamDoc(name = "filename", text = "the filename with extension") String filename,
            @ParamDoc(name = "volume", text = "the volume to be used") PercentType volume) {
            AudioActionService.audioManager.playFile(filename, volume);
            @ParamDoc(name = "volume", text = "volume in the range [0;1]") float volume) {
        playSound(filename, floatVolumeToPercentType(volume));
    @ActionDoc(text = "plays a sound from the sounds folder to the given sink(s)")
    public static void playSound(@ParamDoc(name = "sink", text = "the id of the sink") String sink,
            @ParamDoc(name = "filename", text = "the filename with extension") String filename) {
            AudioActionService.audioManager.playFile(filename, sink);
    @ActionDoc(text = "plays a sound with the given volume from the sounds folder to the given sink(s)")
            @ParamDoc(name = "filename", text = "the filename with extension") String filename,
            AudioActionService.audioManager.playFile(filename, sink, volume);
        playSound(sink, filename, floatVolumeToPercentType(volume));
    @ActionDoc(text = "plays an audio stream from a url to the default sink")
    public static synchronized void playStream(
            @ParamDoc(name = "url", text = "the url of the audio stream") String url) {
            AudioActionService.audioManager.stream(url);
            logger.warn("Failed streaming audio url: {}", e.getMessage());
    @ActionDoc(text = "plays an audio stream from a url to the given sink(s)")
    public static synchronized void playStream(@ParamDoc(name = "sink", text = "the id of the sink") String sink,
            AudioActionService.audioManager.stream(url, sink);
    @ActionDoc(text = "gets the master volume", returns = "volume as a float in the range [0;1]")
    public static float getMasterVolume() throws IOException {
        return AudioActionService.audioManager.getVolume(null).floatValue() / 100f;
    @ActionDoc(text = "sets the master volume")
    public static void setMasterVolume(
            @ParamDoc(name = "volume", text = "volume in the range [0;1]") final float volume) throws IOException {
        setMasterVolume(floatVolumeToPercentType(volume));
    public static void setMasterVolume(@ParamDoc(name = "percent") final PercentType percent) throws IOException {
        AudioActionService.audioManager.setVolume(percent, null);
    @ActionDoc(text = "increases the master volume")
    public static void increaseMasterVolume(@ParamDoc(name = "percent") final float percent) throws IOException {
        if (percent <= 0 || percent > 100) {
            throw new IllegalArgumentException("Percent must be in the range (0;100]!");
        Float volume = getMasterVolume();
        if (volume == 0) {
            // as increasing 0 by x percent will still be 0, we have to set some initial positive value
            volume = 0.001f;
        float newVolume = volume * (1f + percent / 100f);
        if (newVolume - volume < .01) {
            // the getMasterVolume() may only returns integers, so we have to make sure that we
            // increase the volume level at least by 1%.
            newVolume += .01f;
        if (newVolume > 1) {
            newVolume = 1;
        setMasterVolume(newVolume);
    @ActionDoc(text = "decreases the master volume")
    public static void decreaseMasterVolume(@ParamDoc(name = "percent") final float percent) throws IOException {
        float volume = getMasterVolume();
        float newVolume = volume * (1f - percent / 100f);
        if (newVolume > 0 && volume - newVolume < .01) {
            // the getMasterVolume() may only return integers, so we have to make sure that we
            // decrease the volume level at least by 1%.
            newVolume -= .01;
        if (newVolume < 0) {
            newVolume = 0;
     * Converts a float volume to a {@link PercentType} volume and checks if float volume is in the [0;1] range.
     * @param volume
    private static PercentType floatVolumeToPercentType(float volume) {
        if (volume < 0 || volume > 1) {
            throw new IllegalArgumentException("Volume value must be in the range [0;1]!");
        return new PercentType(new BigDecimal(volume * 100f));
