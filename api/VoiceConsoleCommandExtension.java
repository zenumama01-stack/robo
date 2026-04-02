import org.openhab.core.voice.DialogRegistration;
 * Console command extension for all voice features.
 * @author Wouter Born - Sort TTS voices
 * @author Laurent Garnier - Added sub-commands startdialog and stopdialog
 * @author Miguel Álvarez - Add transcribe command
public class VoiceConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String SUBCMD_SAY = "say";
    private static final String SUBCMD_TRANSCRIBE = "transcribe";
    private static final String SUBCMD_INTERPRET = "interpret";
    private static final String SUBCMD_VOICES = "voices";
    private static final String SUBCMD_START_DIALOG = "startdialog";
    private static final String SUBCMD_STOP_DIALOG = "stopdialog";
    private static final String SUBCMD_REGISTER_DIALOG = "registerdialog";
    private static final String SUBCMD_UNREGISTER_DIALOG = "unregisterdialog";
    private static final String SUBCMD_LISTEN_ANSWER = "listenandanswer";
    private static final String SUBCMD_DIALOGS = "dialogs";
    private static final String SUBCMD_DIALOG_REGS = "dialogregs";
    private static final String SUBCMD_INTERPRETERS = "interpreters";
    private static final String SUBCMD_KEYWORD_SPOTTERS = "keywordspotters";
    private static final String SUBCMD_STT_SERVICES = "sttservices";
    private static final String SUBCMD_TTS_SERVICES = "ttsservices";
    public VoiceConsoleCommandExtension(final @Reference VoiceManager voiceManager,
            final @Reference AudioManager audioManager, final @Reference LocaleProvider localeProvider,
            final @Reference ItemRegistry itemRegistry) {
        super("voice", "Commands around voice enablement features.");
        return List.of(buildCommandUsage(SUBCMD_SAY + " <text>", "speaks a text"), buildCommandUsage(
                SUBCMD_TRANSCRIBE + " [--source <source>]|[--file <file>] [--stt <stt>] [--locale <locale>]",
                "transcribe audio from default source, optionally specify a different source/file, speech-to-text service or locale"),
                buildCommandUsage(SUBCMD_INTERPRET + " [--hlis <comma,separated,interpreters>] <command>",
                        "interprets a human language command"),
                buildCommandUsage(SUBCMD_VOICES, "lists available voices of the TTS services"),
                buildCommandUsage(SUBCMD_DIALOGS, "lists the running dialog and their audio/voice services"),
                buildCommandUsage(SUBCMD_DIALOG_REGS,
                        "lists the existing dialog registrations and their selected audio/voice services"),
                buildCommandUsage(SUBCMD_REGISTER_DIALOG
                        + " [--source <source>] [--sink <sink>] [--hlis <comma,separated,interpreters>] [--tts <tts> [--voice <voice>]] [--stt <stt>] [--ks ks [--keyword <ks>]] [--listening-item <listeningItem>] [--location-item <locationItem>] [--dialog-group <dialogGroup>]",
                        "register a new dialog processing using the default services or the services identified with provided arguments, it will be persisted and keep running whenever is possible."),
                buildCommandUsage(SUBCMD_UNREGISTER_DIALOG + " [source]",
                        "unregister the dialog processing for the default audio source or the audio source identified with provided argument, stopping it if started"),
                buildCommandUsage(SUBCMD_START_DIALOG
                        "start a new dialog processing using the default services or the services identified with provided arguments"),
                buildCommandUsage(SUBCMD_STOP_DIALOG + " [<source>]",
                        "stop the dialog processing for the default audio source or the audio source identified with provided argument"),
                buildCommandUsage(SUBCMD_LISTEN_ANSWER
                        + " [--source <source>] [--sink <sink>] [--hlis <comma,separated,interpreters>] [--tts <tts> [--voice <voice>]] [--stt <stt>] [--listening-item <listeningItem>] [--location-item <locationItem>] [--dialog-group <dialogGroup>]",
                        "Execute a simple dialog sequence without keyword spotting using the default services or the services identified with provided arguments"),
                buildCommandUsage(SUBCMD_INTERPRETERS, "lists the interpreters"),
                buildCommandUsage(SUBCMD_KEYWORD_SPOTTERS, "lists the keyword spotters"),
                buildCommandUsage(SUBCMD_STT_SERVICES, "lists the Speech-to-Text services"),
                buildCommandUsage(SUBCMD_TTS_SERVICES, "lists the Text-to-Speech services"));
                case SUBCMD_SAY -> {
                        say(Arrays.copyOfRange(args, 1, args.length), console);
                        console.println("Specify text to say (e.g. 'say hello')");
                case SUBCMD_TRANSCRIBE -> {
                    transcribe(args, console);
                case SUBCMD_INTERPRET -> {
                        interpret(Arrays.copyOfRange(args, 1, args.length), console);
                        console.println("Specify text to interpret (e.g. 'interpret turn all lights off')");
                case SUBCMD_VOICES -> {
                    Voice defaultVoice = getDefaultVoice();
                    for (Voice voice : voiceManager.getAllVoices()) {
                        TTSService ttsService = voiceManager.getTTS(voice.getUID().split(":")[0]);
                        if (ttsService != null) {
                            console.println(String.format("%s %s - %s - %s (%s)",
                                    voice.equals(defaultVoice) ? "*" : " ", ttsService.getLabel(locale),
                                    voice.getLocale().getDisplayName(locale), voice.getLabel(), voice.getUID()));
                case SUBCMD_REGISTER_DIALOG -> {
                    DialogRegistration dialogRegistration;
                        dialogRegistration = parseDialogRegistration(args);
                                "An error occurred while parsing the dialog options"));
                        voiceManager.registerDialog(dialogRegistration);
                                "An error occurred while registering the dialog"));
                case SUBCMD_UNREGISTER_DIALOG -> {
                        var sourceId = args.length < 2 ? audioManager.getSourceId() : args[1];
                        if (sourceId == null) {
                            console.println("No source provided nor default source available");
                        voiceManager.unregisterDialog(sourceId);
                                "An error occurred while stopping the dialog"));
                case SUBCMD_START_DIALOG -> {
                    DialogContext.Builder dialogContextBuilder;
                        dialogContextBuilder = parseDialogContext(args);
                        voiceManager.startDialog(dialogContextBuilder.build());
                                "An error occurred while starting the dialog"));
                case SUBCMD_STOP_DIALOG -> {
                        voiceManager.stopDialog(args.length < 2 ? null : audioManager.getSource(args[1]));
                case SUBCMD_LISTEN_ANSWER -> {
                        voiceManager.listenAndAnswer(dialogContextBuilder.build());
                                "An error occurred while executing the simple dialog sequence"));
                case SUBCMD_DIALOGS -> {
                    listDialogs(console);
                case SUBCMD_DIALOG_REGS -> {
                    listDialogRegistrations(console);
                case SUBCMD_INTERPRETERS -> {
                    listInterpreters(console);
                case SUBCMD_KEYWORD_SPOTTERS -> {
                    listKeywordSpotters(console);
                case SUBCMD_STT_SERVICES -> {
                    listSTTs(console);
                case SUBCMD_TTS_SERVICES -> {
                    listTTSs(console);
    private @Nullable Voice getDefaultVoice() {
        Voice defaultVoice = voiceManager.getDefaultVoice();
        if (defaultVoice == null) {
            TTSService tts = voiceManager.getTTS();
                return voiceManager.getPreferredVoice(tts.getAvailableVoices());
        return defaultVoice;
    private void interpret(String[] args, Console console) {
        String hliIdList = null;
        String[] arguments;
        if (args.length > 0 && "--hlis".equals(args[0])) {
            if (args.length == 1) {
                console.println("No hli id list provided.");
            hliIdList = args[1];
            arguments = Arrays.copyOfRange(args, 2, args.length);
            arguments = args;
        if (arguments.length == 0) {
            console.println("No command provided.");
        StringBuilder sb = new StringBuilder(arguments[0]);
        for (int i = 1; i < arguments.length; i++) {
            sb.append(" ");
            sb.append(arguments[i]);
        String msg = sb.toString();
            String result = voiceManager.interpret(msg, hliIdList);
            console.println(result);
        } catch (InterpretationException ie) {
            console.println(Objects.requireNonNullElse(ie.getMessage(),
                    String.format("An error occurred while interpreting '%s'", msg)));
    private void say(String[] args, Console console) {
        StringBuilder msg = new StringBuilder();
        for (String word : args) {
            if (word.startsWith("%") && word.endsWith("%") && word.length() > 2) {
                String itemName = word.substring(1, word.length() - 1);
                    Item item = this.itemRegistry.getItemByPattern(itemName);
                    msg.append(item.getState());
                msg.append(word);
            msg.append(" ");
        voiceManager.say(msg.toString());
    private void transcribe(String[] args, Console console) {
        HashMap<String, String> parameters;
            parameters = parseNamedParameters(args);
            console.println(Objects.requireNonNullElse(e.getMessage(), "An error parsing positional parameters"));
        Locale locale;
            locale = parameters.containsKey("locale")
                    ? Locale.forLanguageTag(Objects.requireNonNull(parameters.get("locale")))
            console.println("Error: Locale '" + parameters.get("locale") + "' is not correct.");
        String text;
        if (parameters.containsKey("file")) {
            FileAudioStream fileAudioStream;
                var file = Path.of(OpenHAB.getConfigFolder(), AudioManager.SOUND_DIR, parameters.get("file")).toFile();
                    throw new FileNotFoundException();
                fileAudioStream = new FileAudioStream(file);
                console.println("Error: Unable to open '" + parameters.get("file") + "' file audio stream.");
                console.println("Error: File '" + parameters.get("file") + "' not found in sound folder.");
            text = voiceManager.transcribe(fileAudioStream, parameters.get("stt"), locale);
            text = voiceManager.transcribe(parameters.get("source"), parameters.get("stt"), null);
        if (!text.isBlank()) {
            console.println("Transcription: " + text);
            console.println("No transcription generated");
    private void listDialogRegistrations(Console console) {
        Collection<DialogRegistration> registrations = voiceManager.getDialogRegistrations();
        if (!registrations.isEmpty()) {
            registrations.stream().sorted(comparing(dr -> dr.sourceId)).forEach(dr -> {
                String locationText = dr.locationItem != null ? String.format(" Location: %s", dr.locationItem) : "";
                        " Source: %s - Sink: %s (STT: %s, TTS: %s, HLIs: %s, KS: %s, Keyword: %s, Dialog Group: %s)%s",
                        dr.sourceId, dr.sinkId, getOrDefault(dr.sttId), getOrDefault(dr.ttsId),
                        dr.hliIds.isEmpty() ? getOrDefault(null) : String.join("->", dr.hliIds), getOrDefault(dr.ksId),
                        getOrDefault(dr.keyword), getOrDefault(dr.dialogGroup), locationText));
            console.println("No dialog registrations.");
    private String getOrDefault(@Nullable String value) {
        return value != null && !value.isBlank() ? value : "**Default**";
    private void listDialogs(Console console) {
        Collection<DialogContext> dialogContexts = voiceManager.getDialogsContexts();
        if (!dialogContexts.isEmpty()) {
            dialogContexts.stream().sorted(comparing(s -> s.source().getId())).forEach(c -> {
                var ks = c.dt();
                String ksText = ks != null ? String.format(", KS: %s, Keyword: %s", ks.getId(), c.keyword()) : "";
                String locationText = c.locationItem() != null ? String.format(" Location: %s", c.locationItem()) : "";
                        " Source: %s - Sink: %s (STT: %s, TTS: %s, HLIs: %s%s, Dialog Group: %s)%s", c.source().getId(),
                        c.sink().getId(), c.stt().getId(), c.tts().getId(),
                        c.hlis().stream().map(HumanLanguageInterpreter::getId).collect(Collectors.joining("->")),
                        ksText, c.dialogGroup(), locationText));
            console.println("No running dialogs.");
    private void listInterpreters(Console console) {
        Collection<HumanLanguageInterpreter> interpreters = voiceManager.getHLIs();
        if (!interpreters.isEmpty()) {
            HumanLanguageInterpreter defaultHLI = voiceManager.getHLI();
            interpreters.stream().sorted(comparing(s -> s.getLabel(locale))).forEach(hli -> {
                console.println(String.format("%s %s (%s)", hli.equals(defaultHLI) ? "*" : " ", hli.getLabel(locale),
                        hli.getId()));
            console.println("No interpreters found.");
    private void listKeywordSpotters(Console console) {
        Collection<KSService> spotters = voiceManager.getKSs();
        if (!spotters.isEmpty()) {
            KSService defaultKS = voiceManager.getKS();
            spotters.stream().sorted(comparing(s -> s.getLabel(locale))).forEach(ks -> {
                        String.format("%s %s (%s)", ks.equals(defaultKS) ? "*" : " ", ks.getLabel(locale), ks.getId()));
            console.println("No keyword spotters found.");
    private void listSTTs(Console console) {
        Collection<STTService> services = voiceManager.getSTTs();
            STTService defaultSTT = voiceManager.getSTT();
            services.stream().sorted(comparing(s -> s.getLabel(locale))).forEach(stt -> {
                console.println(String.format("%s %s (%s)", stt.equals(defaultSTT) ? "*" : " ", stt.getLabel(locale),
                        stt.getId()));
            console.println("No Speech-to-Text services found.");
    private void listTTSs(Console console) {
        Collection<TTSService> services = voiceManager.getTTSs();
            TTSService defaultTTS = voiceManager.getTTS();
            services.stream().sorted(comparing(s -> s.getLabel(locale))).forEach(tts -> {
                console.println(String.format("%s %s (%s)", tts.equals(defaultTTS) ? "*" : " ", tts.getLabel(locale),
                        tts.getId()));
            console.println("No Text-to-Speech services found.");
    private @Nullable Voice getVoice(@Nullable String id) {
        return id == null ? null
                : voiceManager.getAllVoices().stream().filter(voice -> voice.getUID().equals(id)).findAny()
    private HashMap<String, String> parseNamedParameters(String[] args) {
        var parameters = new HashMap<String, String>();
        for (int i = 1; i < args.length; i++) {
            var arg = args[i].trim();
            if (arg.startsWith("--")) {
                if (i < args.length) {
                    parameters.put(arg.replace("--", ""), args[i].trim());
                    throw new IllegalStateException("Missing value for argument " + arg);
                throw new IllegalStateException("Argument name should start by -- " + arg);
    private DialogContext.Builder parseDialogContext(String[] args) {
            return dialogContextBuilder;
        var parameters = parseNamedParameters(args);
        String sourceId = parameters.remove("source");
            var source = audioManager.getSource(sourceId);
                throw new IllegalStateException("Audio source not found");
        String sinkId = parameters.remove("sink");
            var sink = audioManager.getSink(sinkId);
                throw new IllegalStateException("Audio sink not found");
        dialogContextBuilder //
                .withSTT(voiceManager.getSTT(parameters.remove("stt"))) //
                .withTTS(voiceManager.getTTS(parameters.remove("tts"))) //
                .withVoice(getVoice(parameters.remove("voice"))) //
                .withHLIs(voiceManager.getHLIsByIds(parameters.remove("hlis"))) //
                .withKS(voiceManager.getKS(parameters.remove("ks"))) //
                .withListeningItem(parameters.remove("listening-item")) //
                .withLocationItem(parameters.remove("location-item")) //
                .withDialogGroup(parameters.remove("dialog-group")) //
                .withKeyword(parameters.remove("keyword"));
        if (!parameters.isEmpty()) {
                    "Argument" + parameters.keySet().stream().findAny().orElse("") + "is not supported");
    private DialogRegistration parseDialogRegistration(String[] args) {
            sourceId = audioManager.getSourceId();
            throw new IllegalStateException("A source is required if the default is not configured");
        if (sinkId == null) {
            sinkId = audioManager.getSinkId();
            throw new IllegalStateException("A sink is required if the default is not configured");
        var dr = new DialogRegistration(sourceId, sinkId);
        dr.ksId = parameters.remove("ks");
        dr.keyword = parameters.remove("keyword");
        dr.sttId = parameters.remove("stt");
        dr.ttsId = parameters.remove("tts");
        dr.voiceId = parameters.remove("voice");
        dr.listeningItem = parameters.remove("listening-item");
        dr.locationItem = parameters.remove("location-item");
        dr.dialogGroup = parameters.remove("dialog-group");
        String hliIds = parameters.remove("hlis");
            dr.hliIds = Arrays.stream(hliIds.split(",")).map(String::trim).toList();
                    "Argument " + parameters.keySet().stream().findAny().orElse("") + " is not supported");
        return dr;
