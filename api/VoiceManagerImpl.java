 * @author Yannick Schaus - Added ability to provide an item for feedback during listening phases
 * @author Wouter Born - Sort TTS options
@Component(immediate = true, configurationPid = VoiceManagerImpl.CONFIGURATION_PID, //
        property = Constants.SERVICE_PID + "=org.openhab.voice")
@ConfigurableService(category = "system", label = "Voice", description_uri = VoiceManagerImpl.CONFIG_URI)
public class VoiceManagerImpl implements VoiceManager, ConfigOptionProvider, DialogProcessor.DialogEventListener {
    public static final String CONFIGURATION_PID = "org.openhab.voice";
    // the default keyword to use if no other is configured
    private static final String DEFAULT_KEYWORD = "Wakeup";
    protected static final String CONFIG_URI = "system:voice";
    private static final String CONFIG_KEYWORD = "keyword";
    private static final String CONFIG_LISTENING_ITEM = "listeningItem";
    private static final String CONFIG_LISTENING_MELODY = "listeningMelody";
    private static final String CONFIG_DEFAULT_HLI = "defaultHLI";
    private static final String CONFIG_DEFAULT_KS = "defaultKS";
    private static final String CONFIG_DEFAULT_STT = "defaultSTT";
    private static final String CONFIG_DEFAULT_TTS = "defaultTTS";
    private static final String CONFIG_DEFAULT_VOICE = "defaultVoice";
    private static final String CONFIG_PREFIX_DEFAULT_VOICE = "defaultVoice.";
    private final Logger logger = LoggerFactory.getLogger(VoiceManagerImpl.class);
    private final ScheduledExecutorService scheduledExecutorService = ThreadPoolManager
    private final Map<String, KSService> ksServices = new HashMap<>();
    private final Map<String, STTService> sttServices = new HashMap<>();
    private final Map<String, TTSService> ttsServices = new HashMap<>();
    private final Map<String, HumanLanguageInterpreter> humanLanguageInterpreters = new HashMap<>();
    private final WeakHashMap<String, DialogContext> activeDialogGroups = new WeakHashMap<>();
    private final Storage<DialogRegistration> dialogRegistrationStorage;
    private @Nullable Bundle bundle;
    private String keyword = DEFAULT_KEYWORD;
    private @Nullable String defaultTTS;
    private @Nullable String defaultSTT;
    private @Nullable String defaultKS;
    private @Nullable String defaultHLI;
    private @Nullable String defaultVoice;
    private final Map<String, String> defaultVoices = new HashMap<>();
    private final Map<String, DialogProcessor> dialogProcessors = new HashMap<>();
    private final Map<String, DialogProcessor> singleDialogProcessors = new ConcurrentHashMap<>();
    private @Nullable DialogContext lastDialogContext;
    private @Nullable ScheduledFuture<?> dialogRegistrationFuture;
    public VoiceManagerImpl(final @Reference LocaleProvider localeProvider, final @Reference AudioManager audioManager,
            final @Reference EventPublisher eventPublisher, final @Reference TranslationProvider i18nProvider,
            final @Reference StorageService storageService) {
        this.dialogRegistrationStorage = storageService.getStorage(DialogRegistration.class.getName(),
    protected void activate(BundleContext bundleContext, Map<String, Object> config) {
        this.bundle = bundleContext.getBundle();
        dialogProcessors.values().forEach(DialogProcessor::stop);
        dialogProcessors.clear();
        ScheduledFuture<?> dialogRegistrationFuture = this.dialogRegistrationFuture;
        if (dialogRegistrationFuture != null) {
            dialogRegistrationFuture.cancel(true);
            this.dialogRegistrationFuture = null;
            this.keyword = config.containsKey(CONFIG_KEYWORD) ? config.get(CONFIG_KEYWORD).toString() : DEFAULT_KEYWORD;
            this.listeningItem = config.containsKey(CONFIG_LISTENING_ITEM)
                    ? config.get(CONFIG_LISTENING_ITEM).toString()
            this.listeningMelody = config.containsKey(CONFIG_LISTENING_MELODY)
                    ? config.get(CONFIG_LISTENING_MELODY).toString()
            this.defaultTTS = config.containsKey(CONFIG_DEFAULT_TTS) ? config.get(CONFIG_DEFAULT_TTS).toString() : null;
            this.defaultSTT = config.containsKey(CONFIG_DEFAULT_STT) ? config.get(CONFIG_DEFAULT_STT).toString() : null;
            this.defaultKS = config.containsKey(CONFIG_DEFAULT_KS) ? config.get(CONFIG_DEFAULT_KS).toString() : null;
            this.defaultHLI = config.containsKey(CONFIG_DEFAULT_HLI) ? config.get(CONFIG_DEFAULT_HLI).toString() : null;
            this.defaultVoice = config.containsKey(CONFIG_DEFAULT_VOICE) ? config.get(CONFIG_DEFAULT_VOICE).toString()
                if (key.startsWith(CONFIG_PREFIX_DEFAULT_VOICE)) {
                    String tts = key.substring(CONFIG_PREFIX_DEFAULT_VOICE.length());
                    defaultVoices.put(tts, entry.getValue().toString());
    public void say(String text) {
    public void say(String text, @Nullable PercentType volume) {
    public void say(String text, String voiceId) {
        say(text, voiceId, null, null);
    public void say(String text, @Nullable String voiceId, @Nullable PercentType volume) {
        say(text, voiceId, null, volume);
    public void say(String text, @Nullable String voiceId, @Nullable String sinkId) {
        say(text, voiceId, sinkId, null);
    public void say(String text, @Nullable String voiceId, @Nullable String sinkId, @Nullable PercentType volume) {
        Objects.requireNonNull(text, "Text cannot be said as it is null.");
            TTSService tts = null;
            String selectedVoiceId = voiceId;
            if (selectedVoiceId == null) {
                // use the configured default, if set
                selectedVoiceId = defaultVoice;
                tts = getTTS();
                    voice = getPreferredVoice(tts.getAvailableVoices());
                voice = getVoice(selectedVoiceId);
                    tts = getTTS(voice);
                throw new TTSException("No TTS service can be found for voice " + selectedVoiceId);
                throw new TTSException(
                        "Unable to find a voice for language " + localeProvider.getLocale().getLanguage());
            Set<AudioFormat> ttsSupportedFormats = tts.getSupportedFormats();
                throw new TTSException("Unable to find the audio sink " + sinkId);
            AudioFormat ttsAudioFormat = getBestMatch(ttsSupportedFormats, sink.getSupportedFormats());
            if (ttsAudioFormat == null) {
                throw new TTSException("No compatible audio format found for TTS '" + tts.getId() + "' and sink '"
                        + sink.getId() + "'");
            AudioStream audioStream = tts.synthesize(text, voice, ttsAudioFormat);
            if (!sink.getSupportedStreams().stream().anyMatch(clazz -> clazz.isInstance(audioStream))) {
                        "Failed playing audio stream '" + audioStream + "' as audio sink doesn't support it");
            Runnable restoreVolume = audioManager.handleVolumeCommand(volume, sink);
    public String transcribe(@Nullable String audioSourceId, @Nullable String sttId, @Nullable Locale locale) {
        var audioSource = audioSourceId != null ? audioManager.getSource(audioSourceId) : audioManager.getSource();
            logger.warn("Audio source '{}' not available", audioSourceId != null ? audioSourceId : "default");
        var sttService = sttId != null ? getSTT(sttId) : getSTT();
            logger.warn("Speech-to-text service '{}' not available", sttId != null ? sttId : "default");
        var sttFormat = VoiceManagerImpl.getBestMatch(audioSource.getSupportedFormats(),
                sttService.getSupportedFormats());
        if (sttFormat == null) {
            logger.warn("No compatible audio format found for stt '{}' and the provided audio stream",
                    sttService.getId());
        AudioStream audioStream;
            audioStream = audioSource.getInputStream(sttFormat);
            logger.warn("AudioException creating source audio stream: {}", e.getMessage());
        return transcribe(audioStream, sttService, locale);
    public String transcribe(AudioStream audioStream, @Nullable String sttId, @Nullable Locale locale) {
        var sttFormat = VoiceManagerImpl.getBestMatch(Set.of(audioStream.getFormat()),
    private String transcribe(AudioStream audioStream, STTService sttService, @Nullable Locale locale) {
        Locale nullSafeLocale = locale != null ? locale : localeProvider.getLocale();
        CompletableFuture<String> transcriptionResult = new CompletableFuture<>();
        STTServiceHandle sttServiceHandle;
            sttServiceHandle = sttService.recognize(sttEvent -> {
                    String transcript = sre.getTranscript();
                    logger.debug("Text recognized: {}", transcript);
                    transcriptionResult.complete(transcript);
                    logger.debug("SpeechRecognitionErrorEvent event received");
                    transcriptionResult.completeExceptionally(
                            new IOException("SpeechRecognitionErrorEvent emitted: " + sre.getMessage()));
            }, audioStream, nullSafeLocale, new HashSet<>());
            logger.warn("STTException while running transcription");
            return transcriptionResult.get(60, TimeUnit.SECONDS);
            logger.warn("InterruptedException waiting for transcription: {}", e.getMessage());
            sttServiceHandle.abort();
            logger.warn("ExecutionException running transcription: {}", e.getCause().getMessage());
            logger.warn("TimeoutException waiting for transcription");
    public String interpret(String text) throws InterpretationException {
    public String interpret(String text, @Nullable String hliIdList) throws InterpretationException {
        List<HumanLanguageInterpreter> interpreters = new ArrayList<>();
        if (hliIdList == null) {
            HumanLanguageInterpreter interpreter = getHLI();
            if (interpreter != null) {
                interpreters.add(interpreter);
            interpreters = getHLIsByIds(hliIdList);
            InterpretationException exception = null;
            for (var interpreter : interpreters) {
                    String answer = interpreter.interpret(locale, text);
                    return answer;
            if (exception != null) { // this should always be the case here
            throw new InterpretationException("No human language interpreter available!");
            throw new InterpretationException("No human language interpreter can be found for " + hliIdList);
        } else if (id.contains(":")) {
            // it is a fully qualified unique id
            String[] segments = id.split(":");
            TTSService tts = getTTS(segments[0]);
                return getVoice(tts.getAvailableVoices(), segments[1]);
            // voiceId is not fully qualified
            TTSService tts = getTTS();
                return getVoice(tts.getAvailableVoices(), id);
    private @Nullable Voice getVoice(Set<Voice> voices, String id) {
        for (Voice voice : voices) {
            if (voice.getUID().endsWith(":" + id)) {
                return voice;
            if ((null == currentAudioFormat.getCodec()) || (null == currentAudioFormat.getContainer())
                    || (null == currentAudioFormat.isBigEndian()) || (null == currentAudioFormat.getBitDepth())) {
            if ((null == currentAudioFormat.getBitRate()) || (null == currentAudioFormat.getFrequency())
                    || !AudioFormat.CONTAINER_WAVE.equals(currentAudioFormat.getContainer())) {
            if ((null == format.getCodec()) || (null == format.getContainer())
                    || !AudioFormat.CONTAINER_WAVE.equals(format.getContainer())) {
                        format.getBitRate(), format.getFrequency());
                long defaultFrequency = 44100;
                        bitRate, frequency);
    public @Nullable Voice getPreferredVoice(Set<Voice> voices) {
        // Express preferences with a Language Priority List
        // Get collection of voice locales
        Collection<Locale> locales = new ArrayList<>();
        for (Voice currentVoice : voices) {
            locales.add(currentVoice.getLocale());
        // Determine preferred locale based on RFC 4647
        String ranges = locale.toLanguageTag();
        List<Locale.LanguageRange> languageRanges = Locale.LanguageRange.parse(ranges + "-*");
        Locale preferredLocale = Locale.lookup(languageRanges, locales);
        // As a last resort choose some Locale
        if (preferredLocale == null && !voices.isEmpty()) {
            preferredLocale = locales.iterator().next();
        if (preferredLocale == null) {
        // Determine preferred voice
        Voice preferredVoice = null;
            if (preferredLocale.equals(currentVoice.getLocale())) {
                preferredVoice = currentVoice;
        // Return preferred voice
        return preferredVoice;
    public DialogContext.Builder getDialogContextBuilder() {
        return new DialogContext.Builder(keyword, localeProvider.getLocale()) //
                .withSink(audioManager.getSink()) //
                .withSource(audioManager.getSource()) //
                .withKS(this.getKS()) //
                .withSTT(this.getSTT()) //
                .withTTS(this.getTTS()) //
                .withHLI(this.getHLI()) //
                .withVoice(this.getDefaultVoice()) //
                .withMelody(listeningMelody) //
                .withListeningItem(listeningItem);
    public List<DialogContext> getDialogsContexts() {
        return dialogProcessors.values().stream().map(DialogProcessor::getContext).toList();
    public @Nullable DialogContext getLastDialogContext() {
        return lastDialogContext;
    public @Nullable DTServiceHandle startDialog(DialogContext context) throws IllegalStateException {
        var dtService = context.dt();
        var ksKeyword = context.keyword();
        if (dtService == null) {
                    "Invalid dialog context for persistent dialog: missing dialog trigger implementation");
        if (dtService instanceof KSService && (ksKeyword == null || ksKeyword.isEmpty())) {
                    "Invalid dialog context for persistent dialog: missing keyword spot configuration");
        Bundle b = bundle;
        if (b == null) {
            throw new IllegalStateException("Bundle is not (yet?) set.");
        if ((dtService instanceof KSService ksService
                && !checkLocales(ksService.getSupportedLocales(), context.locale()))
                || !checkLocales(context.stt().getSupportedLocales(), context.locale()) || !context.hlis().stream()
                        .allMatch(interpreter -> checkLocales(interpreter.getSupportedLocales(), context.locale()))) {
            throw new IllegalStateException("Cannot start dialog as provided locale is not supported by all services.");
            DialogProcessor processor = dialogProcessors.get(context.source().getId());
            if (processor == null) {
                logger.debug("Starting a new dialog for source {} ({})", context.source().getLabel(null),
                        context.source().getId());
                processor = new DialogProcessor(context, this, this.eventPublisher, this.activeDialogGroups,
                        this.i18nProvider, b);
                dialogProcessors.put(context.source().getId(), processor);
                return processor.start();
                        String.format("Cannot start dialog as a dialog is already started for audio source '%s'.",
                                context.source().getLabel(null)));
    public void stopDialog(@Nullable AudioSource source) throws IllegalStateException {
        AudioSource audioSource = (source == null) ? audioManager.getSource() : source;
        if (audioSource != null) {
            DialogProcessor processor = dialogProcessors.remove(audioSource.getId());
            singleDialogProcessors.values().removeIf(e -> !e.isProcessing());
                processor = singleDialogProcessors.get(audioSource.getId());
            if (processor != null) {
                processor.stop();
                logger.debug("Dialog stopped for source {} ({})", audioSource.getLabel(null), audioSource.getId());
                        String.format("Cannot stop dialog as no dialog is started for audio source '%s'.",
                                audioSource.getLabel(null)));
            throw new IllegalStateException("Cannot stop dialog as audio source is missing.");
    public void stopDialog(DialogContext context) throws IllegalStateException {
        stopDialog(context.source());
    public void listenAndAnswer(DialogContext context) throws IllegalStateException {
            throw new IllegalStateException("Cannot execute a simple dialog as services are missing.");
        } else if (!checkLocales(context.stt().getSupportedLocales(), context.locale()) || !context.hlis().stream()
                    "Cannot execute a simple dialog as provided locale is not supported by all services.");
            boolean isSingleDialog = false;
            var audioSource = context.source();
            DialogProcessor activeProcessor = dialogProcessors.get(audioSource.getId());
            if (activeProcessor == null) {
                isSingleDialog = true;
                activeProcessor = singleDialogProcessors.get(audioSource.getId());
            var processor = new DialogProcessor(context, this, this.eventPublisher, this.activeDialogGroups,
                logger.debug("Executing a simple dialog for source {} ({})", audioSource.getLabel(null),
                        audioSource.getId());
                processor.startSimpleDialog();
                singleDialogProcessors.put(audioSource.getId(), processor);
            } else if (!isSingleDialog && activeProcessor.isCompatible(processor)) {
                logger.debug("Executing a simple dialog for active source {} ({})", audioSource.getLabel(null),
                activeProcessor.startSimpleDialog();
                        "Cannot execute a simple dialog as a dialog is already started for audio source '%s'.",
    public void registerDialog(DialogRegistration registration) throws IllegalStateException {
        if (dialogRegistrationStorage.containsKey(registration.sourceId)) {
                    "Cannot register dialog as a dialog is registered for audio source '%s'.", registration.sourceId));
        synchronized (dialogRegistrationStorage) {
            dialogRegistrationStorage.put(registration.sourceId, registration);
        scheduleDialogRegistrations();
    public void unregisterDialog(DialogRegistration registration) {
        unregisterDialog(registration.sourceId);
    public void unregisterDialog(String sourceId) {
            var registrationRef = dialogRegistrationStorage.remove(sourceId);
            if (registrationRef != null) {
                var dialog = dialogProcessors.get(sourceId);
                if (dialog != null) {
                    stopDialog(dialog.dialogContext);
    public List<DialogRegistration> getDialogRegistrations() {
        var list = new ArrayList<DialogRegistration>();
        dialogRegistrationStorage.getValues().forEach(dr -> {
            if (dr != null) {
                // update running state
                dr.running = dialogProcessors.containsKey(dr.sourceId);
                list.add(dr);
    private boolean checkLocales(Set<Locale> supportedLocales, Locale locale) {
        if (supportedLocales.isEmpty()) {
        return supportedLocales.stream().anyMatch(sLocale -> {
            var country = sLocale.getCountry();
            return Objects.equals(sLocale.getLanguage(), locale.getLanguage())
                    && (country == null || country.isBlank() || country.equals(locale.getCountry()));
        stopDialogs(dialog -> dialog.dialogContext.sink().getId().equals(audioSink.getId()));
        stopDialogs(dialog -> dialog.dialogContext.source().getId().equals(audioSource.getId()));
    protected void addKSService(KSService ksService) {
        this.ksServices.put(ksService.getId(), ksService);
    protected void removeKSService(KSService ksService) {
        this.ksServices.remove(ksService.getId());
        stopDialogs(dialog -> {
            var dt = dialog.dialogContext.dt();
            return dt != null && dt.getId().equals(ksService.getId());
    protected void addSTTService(STTService sttService) {
        this.sttServices.put(sttService.getId(), sttService);
    protected void removeSTTService(STTService sttService) {
        this.sttServices.remove(sttService.getId());
        stopDialogs(dialog -> dialog.dialogContext.stt().getId().equals(sttService.getId()));
    protected void addTTSService(TTSService ttsService) {
        this.ttsServices.put(ttsService.getId(), ttsService);
    protected void removeTTSService(TTSService ttsService) {
        this.ttsServices.remove(ttsService.getId());
        stopDialogs(dialog -> dialog.dialogContext.tts().getId().equals(ttsService.getId()));
    protected void addHumanLanguageInterpreter(HumanLanguageInterpreter humanLanguageInterpreter) {
        this.humanLanguageInterpreters.put(humanLanguageInterpreter.getId(), humanLanguageInterpreter);
    protected void removeHumanLanguageInterpreter(HumanLanguageInterpreter humanLanguageInterpreter) {
        this.humanLanguageInterpreters.remove(humanLanguageInterpreter.getId());
        stopDialogs(dialog -> dialog.dialogContext.hlis().stream()
                .anyMatch(hli -> hli.getId().equals(humanLanguageInterpreter.getId())));
    public @Nullable TTSService getTTS() {
        if (defaultTTS != null) {
            tts = ttsServices.get(defaultTTS);
                logger.warn("Default TTS service '{}' not available!", defaultTTS);
        } else if (!ttsServices.isEmpty()) {
            tts = ttsServices.values().iterator().next();
            logger.debug("No TTS service available!");
        return tts;
    public @Nullable TTSService getTTS(@Nullable String id) {
        return id == null ? null : ttsServices.get(id);
    private @Nullable TTSService getTTS(Voice voice) {
        return getTTS(voice.getUID().split(":")[0]);
    public Collection<TTSService> getTTSs() {
        return new HashSet<>(ttsServices.values());
    public @Nullable STTService getSTT() {
        STTService stt = null;
        if (defaultSTT != null) {
            stt = sttServices.get(defaultSTT);
                logger.warn("Default STT service '{}' not available!", defaultSTT);
        } else if (!sttServices.isEmpty()) {
            stt = sttServices.values().iterator().next();
            logger.debug("No STT service available!");
        return stt;
    public @Nullable STTService getSTT(@Nullable String id) {
        return id == null ? null : sttServices.get(id);
    public Collection<STTService> getSTTs() {
        return new HashSet<>(sttServices.values());
    public @Nullable KSService getKS() {
        KSService ks = null;
        if (defaultKS != null) {
            ks = ksServices.get(defaultKS);
                logger.warn("Default KS service '{}' not available!", defaultKS);
        } else if (!ksServices.isEmpty()) {
            ks = ksServices.values().iterator().next();
            logger.debug("No KS service available!");
        return ks;
    public @Nullable KSService getKS(@Nullable String id) {
        return id == null ? null : ksServices.get(id);
    public Collection<KSService> getKSs() {
        return new HashSet<>(ksServices.values());
    public @Nullable HumanLanguageInterpreter getHLI() {
        HumanLanguageInterpreter hli = null;
        if (defaultHLI != null) {
            hli = humanLanguageInterpreters.get(defaultHLI);
                logger.warn("Default HumanLanguageInterpreter '{}' not available!", defaultHLI);
        } else if (!humanLanguageInterpreters.isEmpty()) {
            hli = humanLanguageInterpreters.values().iterator().next();
            logger.debug("No HumanLanguageInterpreter available!");
        return hli;
    public @Nullable HumanLanguageInterpreter getHLI(@Nullable String id) {
        return id == null ? null : humanLanguageInterpreters.get(id);
    public List<HumanLanguageInterpreter> getHLIsByIds(@Nullable String ids) {
        return ids == null ? List.of() : getHLIsByIds(Arrays.asList(ids.split(",")));
    public List<HumanLanguageInterpreter> getHLIsByIds(List<String> ids) {
        for (String id : ids) {
            HumanLanguageInterpreter hli = humanLanguageInterpreters.get(id);
                logger.warn("HumanLanguageInterpreter '{}' not available!", id);
                interpreters.add(hli);
        return interpreters;
    public Collection<HumanLanguageInterpreter> getHLIs() {
        return new HashSet<>(humanLanguageInterpreters.values());
    public Set<Voice> getAllVoices() {
        return getAllVoicesSorted(localeProvider.getLocale());
    private Set<Voice> getAllVoicesSorted(Locale locale) {
        return ttsServices.values().stream().map(TTSService::getAvailableVoices).flatMap(Collection::stream)
                .sorted(createVoiceComparator(locale)).collect(Collectors
                        .collectingAndThen(Collectors.toCollection(LinkedHashSet::new), Collections::unmodifiableSet));
     * Creates a comparator which compares voices using the given locale in the following order:
     * <li>Voice TTSService label (localized with the given locale)
     * <li>Voice locale display name (localized with the given locale)
     * @param locale the locale used for comparing {@link TTSService} labels and {@link Voice} locale display names
     * @return the localized voice comparator
    private Comparator<Voice> createVoiceComparator(Locale locale) {
        Comparator<Voice> byTTSLabel = (Voice v1, Voice v2) -> {
            TTSService tts1 = getTTS(v1);
            TTSService tts2 = getTTS(v2);
            return (tts1 == null || tts2 == null) ? 0
                    : tts1.getLabel(locale).compareToIgnoreCase(tts2.getLabel(locale));
        Comparator<Voice> byVoiceLocale = (Voice v1, Voice v2) -> v1.getLocale().getDisplayName(locale)
                .compareToIgnoreCase(v2.getLocale().getDisplayName(locale));
        return byTTSLabel.thenComparing(byVoiceLocale).thenComparing(Voice::getLabel);
    public @Nullable Voice getDefaultVoice() {
        String localDefaultVoice = defaultVoice;
        return localDefaultVoice != null ? getVoice(localDefaultVoice) : null;
                case CONFIG_DEFAULT_HLI:
                    return humanLanguageInterpreters.values().stream()
                            .sorted((hli1, hli2) -> hli1.getLabel(locale).compareToIgnoreCase(hli2.getLabel(locale)))
                            .map(hli -> new ParameterOption(hli.getId(), hli.getLabel(locale))).toList();
                case CONFIG_DEFAULT_KS:
                    return ksServices.values().stream()
                            .sorted((ks1, ks2) -> ks1.getLabel(locale).compareToIgnoreCase(ks2.getLabel(locale)))
                            .map(ks -> new ParameterOption(ks.getId(), ks.getLabel(locale))).toList();
                case CONFIG_DEFAULT_STT:
                    return sttServices.values().stream()
                            .sorted((stt1, stt2) -> stt1.getLabel(locale).compareToIgnoreCase(stt2.getLabel(locale)))
                            .map(stt -> new ParameterOption(stt.getId(), stt.getLabel(locale))).toList();
                case CONFIG_DEFAULT_TTS:
                    return ttsServices.values().stream()
                            .sorted((tts1, tts2) -> tts1.getLabel(locale).compareToIgnoreCase(tts2.getLabel(locale)))
                            .map(tts -> new ParameterOption(tts.getId(), tts.getLabel(locale))).toList();
                case CONFIG_DEFAULT_VOICE:
                    return getAllVoicesSorted(nullSafeLocale)
                            .stream().filter(v -> getTTS(v) != null).map(
                                    v -> new ParameterOption(v.getUID(),
                                            String.format("%s - %s - %s", getTTS(v).getLabel(nullSafeLocale),
                                                    v.getLocale().getDisplayName(nullSafeLocale), v.getLabel())))
    private void stopDialogs(Predicate<DialogProcessor> filter) {
            var dialogsToStop = dialogProcessors.values().stream().filter(filter).toList();
            if (dialogsToStop.isEmpty()) {
            for (var dialog : dialogsToStop) {
                stopDialog(dialog.dialogContext.source());
     * In order to reduce the number of dialog registration builds
     * this method schedules a call to {@link #buildDialogRegistrations() buildDialogRegistrations} in five seconds
     * and cancel the previous scheduled call if any.
    private void scheduleDialogRegistrations() {
        ScheduledFuture<?> job = this.dialogRegistrationFuture;
        dialogRegistrationFuture = scheduledExecutorService.schedule(this::buildDialogRegistrations, 5,
     * This method tries to start a dialog for each dialog registration.
     * It's only called from {@link #scheduleDialogRegistrations() scheduleDialogRegistrations} in order to
     * reduce the number of executions.
    private void buildDialogRegistrations() {
                if (dr != null && !dialogProcessors.containsKey(dr.sourceId)) {
                        startDialog(getDialogContextBuilder() //
                                .withSink(audioManager.getSink(dr.sinkId)) //
                                .withSource(audioManager.getSource(dr.sourceId)) //
                                .withKS(getKS(dr.ksId)) //
                                .withKeyword(dr.keyword) //
                                .withSTT(getSTT(dr.sttId)) //
                                .withTTS(getTTS(dr.ttsId)) //
                                .withVoice(getVoice(dr.voiceId)) //
                                .withHLIs(getHLIsByIds(dr.hliIds)) //
                                .withLocale(dr.locale) //
                                .withDialogGroup(dr.dialogGroup) //
                                .withLocationItem(dr.locationItem) //
                                .withListeningItem(dr.listeningItem) //
                                .withMelody(dr.listeningMelody) //
                        logger.debug("Unable to start dialog registration: {}", e.getMessage());
    public void onBeforeDialogInterpretation(DialogContext context) {
        lastDialogContext = context;
    public void onDialogStopped(DialogContext context) {
        var registration = dialogRegistrationStorage.get(context.source().getId());
        if (registration != null) {
            // try to rebuild in case it was manually stopped
