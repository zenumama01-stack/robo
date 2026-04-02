 * Describes dialog desired services and options.
public class DialogRegistration {
     * Dialog audio source id
    public String sourceId;
     * Dialog audio sink id
    public String sinkId;
     * Preferred keyword-spotting service
    public @Nullable String ksId;
     * Selected keyword for spotting
    public @Nullable String keyword;
     * Preferred speech-to-text service id
    public @Nullable String sttId;
     * Preferred text-to-speech service id
    public @Nullable String ttsId;
     * Preferred voice id
    public @Nullable String voiceId;
     * List of interpreters
    public List<String> hliIds = List.of();
     * Dialog locale
    public @Nullable Locale locale;
     * Linked listening item
    public @Nullable String listeningItem;
     * Linked location item
    public @Nullable String locationItem;
     * Dialog group name
    public @Nullable String dialogGroup;
     * Custom listening melody
    public @Nullable String listeningMelody;
     * True if an associated dialog is running
    public boolean running = false;
    public DialogRegistration(String sourceId, String sinkId) {
        this.sourceId = sourceId;
        this.sinkId = sinkId;
