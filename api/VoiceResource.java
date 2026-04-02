import org.openhab.core.voice.KSService;
import org.openhab.core.voice.STTService;
import org.openhab.core.voice.TTSService;
import org.openhab.core.voice.text.InterpretationException;
 * This class acts as a REST resource for voice features.
 * @author Laurent Garnier - add TTS feature to the REST API
@JaxrsName(VoiceResource.PATH_VOICE)
@Path(VoiceResource.PATH_VOICE)
@Tag(name = VoiceResource.PATH_VOICE)
public class VoiceResource implements RESTResource {
    public static final String PATH_VOICE = "voice";
    private final Logger logger = LoggerFactory.getLogger(VoiceResource.class);
    public VoiceResource( //
    @Path("/interpreters")
    @Operation(operationId = "getVoiceInterpreters", summary = "Get the list of all interpreters.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = HumanLanguageInterpreterDTO.class)))) })
    public Response getInterpreters(
        List<HumanLanguageInterpreterDTO> dtos = voiceManager.getHLIs().stream().map(hli -> HLIMapper.map(hli, locale))
    @Path("/interpreters/{id: [a-zA-Z_0-9]+}")
    @Operation(operationId = "getVoiceInterpreterByUID", summary = "Gets a single interpreter.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = HumanLanguageInterpreterDTO.class)))),
            @ApiResponse(responseCode = "404", description = "Interpreter not found") })
    public Response getInterpreter(
            @PathParam("id") @Parameter(description = "interpreter id") String id) {
        HumanLanguageInterpreter hli = voiceManager.getHLI(id);
        if (hli == null) {
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "No interpreter found");
        HumanLanguageInterpreterDTO dto = HLIMapper.map(hli, locale);
    @Path("/interpreters/{ids: [a-zA-Z_0-9,]+}")
    @Operation(operationId = "interpretText", summary = "Sends a text to a given human language interpreter(s).", responses = {
            @ApiResponse(responseCode = "404", description = "No human language interpreter was found."),
            @ApiResponse(responseCode = "400", description = "interpretation exception occurs") })
    public Response interpret(
            @Parameter(description = "text to interpret", required = true) String text,
            @PathParam("ids") @Parameter(description = "comma separated list of interpreter ids") List<String> ids) {
        List<HumanLanguageInterpreter> hlis = voiceManager.getHLIsByIds(ids);
        if (hlis.isEmpty()) {
        String answer = "";
        String error = null;
        for (HumanLanguageInterpreter interpreter : hlis) {
                answer = interpreter.interpret(locale, text);
                logger.debug("Interpretation result: {}", answer);
            } catch (InterpretationException e) {
                logger.debug("Interpretation exception: {}", e.getMessage());
                error = Objects.requireNonNullElse(e.getMessage(), "Unexpected error");
        if (error != null) {
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, error);
            return Response.ok(answer, MediaType.TEXT_PLAIN).build();
    @Operation(operationId = "interpretTextByDefaultInterpreter", summary = "Sends a text to the default human language interpreter.", responses = {
            @Parameter(description = "text to interpret", required = true) String text) {
        HumanLanguageInterpreter hli = voiceManager.getHLI();
            String answer = hli.interpret(locale, text);
    @Path("/voices")
    @Operation(operationId = "getVoices", summary = "Get the list of all voices.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = VoiceDTO.class)))) })
    public Response getVoices() {
        List<VoiceDTO> dtos = voiceManager.getAllVoices().stream().map(VoiceMapper::map).toList();
    @Path("/defaultvoice")
    @Operation(operationId = "getDefaultVoice", summary = "Gets the default voice.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = VoiceDTO.class))),
            @ApiResponse(responseCode = "404", description = "No default voice was found.") })
    public Response getDefaultVoice() {
        Voice voice = voiceManager.getDefaultVoice();
        if (voice == null) {
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Default voice not found");
        VoiceDTO dto = VoiceMapper.map(voice);
    @Path("/say")
    @Operation(operationId = "textToSpeech", summary = "Speaks a given text with a given voice through the given audio sink.", responses = {
    public Response say(@Parameter(description = "text to speak", required = true) String text,
            @QueryParam("voiceid") @Parameter(description = "voice id") @Nullable String voiceId,
            @QueryParam("sinkid") @Parameter(description = "audio sink id") @Nullable String sinkId,
            @QueryParam("volume") @Parameter(description = "volume level") @Nullable String volume) {
        PercentType volumePercent = null;
        if (volume != null && !volume.isBlank()) {
            volumePercent = new PercentType(volume);
        voiceManager.say(text, voiceId, sinkId, volumePercent);
    @Path("/dialog/start")
    @Operation(operationId = "startDialog", summary = "Start dialog processing for a given audio source.", responses = {
            @ApiResponse(responseCode = "404", description = "One of the given ids is wrong."),
            @ApiResponse(responseCode = "400", description = "Services are missing or language is not supported by services or dialog processing is already started for the audio source.") })
    public Response startDialog(
            @QueryParam("sourceId") @Parameter(description = "source ID") @Nullable String sourceId,
            @QueryParam("ksId") @Parameter(description = "keywork spotter ID") @Nullable String ksId,
            @QueryParam("sttId") @Parameter(description = "Speech-to-Text ID") @Nullable String sttId,
            @QueryParam("ttsId") @Parameter(description = "Text-to-Speech ID") @Nullable String ttsId,
            @QueryParam("voiceId") @Parameter(description = "voice ID") @Nullable String voiceId,
            @QueryParam("hliIds") @Parameter(description = "comma separated list of interpreter IDs") @Nullable String hliIds,
            @QueryParam("sinkId") @Parameter(description = "audio sink ID") @Nullable String sinkId,
            @QueryParam("keyword") @Parameter(description = "keyword") @Nullable String keyword,
            @QueryParam("listeningItem") @Parameter(description = "listening item") @Nullable String listeningItem) {
        var dialogContextBuilder = voiceManager.getDialogContextBuilder();
        if (sourceId != null) {
            AudioSource source = audioManager.getSource(sourceId);
                return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Audio source not found");
            dialogContextBuilder.withSource(source);
        if (ksId != null) {
            KSService ks = voiceManager.getKS(ksId);
            if (ks == null) {
                return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Keyword spotter not found");
            dialogContextBuilder.withKS(ks);
        if (sttId != null) {
            STTService stt = voiceManager.getSTT(sttId);
            if (stt == null) {
                return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Speech-to-Text not found");
            dialogContextBuilder.withSTT(stt);
        if (ttsId != null) {
            TTSService tts = voiceManager.getTTS(ttsId);
            if (tts == null) {
                return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Text-to-Speech not found");
            dialogContextBuilder.withTTS(tts);
        if (voiceId != null) {
            Voice voice = getVoice(voiceId);
                return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Voice not found");
            dialogContextBuilder.withVoice(voice);
        if (hliIds != null) {
            List<HumanLanguageInterpreter> interpreters = voiceManager.getHLIsByIds(hliIds);
            if (interpreters.isEmpty()) {
                return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Interpreter not found");
            dialogContextBuilder.withHLIs(interpreters);
        if (sinkId != null) {
            AudioSink sink = audioManager.getSink(sinkId);
                return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Audio sink not found");
            dialogContextBuilder.withSink(sink);
        if (listeningItem != null) {
            dialogContextBuilder.withListeningItem(listeningItem);
        if (keyword != null) {
            dialogContextBuilder.withKeyword(keyword);
            voiceManager.startDialog(dialogContextBuilder.withLocale(localeService.getLocale(language)).build());
    @Path("/dialog/stop")
    @Operation(operationId = "stopDialog", summary = "Stop dialog processing for a given audio source.", responses = {
            @ApiResponse(responseCode = "404", description = "No audio source was found."),
            @ApiResponse(responseCode = "400", description = "No dialog processing is started for the audio source.") })
    public Response stopDialog(
            @QueryParam("sourceId") @Parameter(description = "source ID") @Nullable String sourceId) {
            source = audioManager.getSource(sourceId);
            voiceManager.stopDialog(source);
    @Path("/listenandanswer")
    @Operation(operationId = "listenAndAnswer", summary = "Executes a simple dialog sequence without keyword spotting for a given audio source.", responses = {
    public Response listenAndAnswer(
            @QueryParam("hliIds") @Parameter(description = "interpreter IDs") @Nullable List<String> hliIds,
            voiceManager.listenAndAnswer(dialogContextBuilder.withLocale(localeService.getLocale(language)).build());
    private @Nullable Voice getVoice(String id) {
        return voiceManager.getAllVoices().stream().filter(voice -> voice.getUID().equals(id)).findAny().orElse(null);
