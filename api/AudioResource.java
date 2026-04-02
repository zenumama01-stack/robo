 * This class acts as a REST resource for audio features.
@JaxrsName(AudioResource.PATH_AUDIO)
@Path(AudioResource.PATH_AUDIO)
@Tag(name = AudioResource.PATH_AUDIO)
public class AudioResource implements RESTResource {
    public static final String PATH_AUDIO = "audio";
    public AudioResource( //
            final @Reference AudioManager audioManager, //
            final @Reference LocaleService localeService) {
    @Path("/sources")
    @Operation(operationId = "getAudioSources", summary = "Get the list of all sources.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = AudioSourceDTO.class)))) })
    public Response getSources(
        Collection<AudioSource> sources = audioManager.getAllSources();
        List<AudioSourceDTO> dtos = new ArrayList<>(sources.size());
        for (AudioSource source : sources) {
            dtos.add(AudioMapper.map(source, locale));
        return Response.ok(dtos).build();
    @Path("/defaultsource")
    @Operation(operationId = "getAudioDefaultSource", summary = "Get the default source if defined or the first available source.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = AudioSourceDTO.class))),
            @ApiResponse(responseCode = "404", description = "Source not found") })
    public Response getDefaultSource(
        AudioSource source = audioManager.getSource();
        if (source != null) {
            return Response.ok(AudioMapper.map(source, locale)).build();
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Source not found");
    @Path("/sinks")
    @Operation(operationId = "getAudioSinks", summary = "Get the list of all sinks.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = AudioSinkDTO.class)))) })
    public Response getSinks(
        Collection<AudioSink> sinks = audioManager.getAllSinks();
        List<AudioSinkDTO> dtos = new ArrayList<>(sinks.size());
        for (AudioSink sink : sinks) {
            dtos.add(AudioMapper.map(sink, locale));
    @Path("/defaultsink")
    @Operation(operationId = "getAudioDefaultSink", summary = "Get the default sink if defined or the first available sink.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = AudioSinkDTO.class))),
            @ApiResponse(responseCode = "404", description = "Sink not found") })
    public Response getDefaultSink(
        AudioSink sink = audioManager.getSink();
            return Response.ok(AudioMapper.map(sink, locale)).build();
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Sink not found");
