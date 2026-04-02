package org.openhab.core.automation.module.script.profile;
import org.openhab.core.thing.profiles.ProfileCallback;
import org.openhab.core.thing.profiles.ProfileContext;
import org.openhab.core.thing.profiles.ProfileTypeUID;
import org.openhab.core.thing.profiles.TimeSeriesProfile;
import org.openhab.core.types.Type;
 * The {@link ScriptProfile} is generic profile for managing values with scripts
public class ScriptProfile implements TimeSeriesProfile {
    public static final String CONFIG_TO_ITEM_SCRIPT = "toItemScript";
    public static final String CONFIG_TO_HANDLER_SCRIPT = "toHandlerScript";
    public static final String CONFIG_COMMAND_FROM_ITEM_SCRIPT = "commandFromItemScript";
    public static final String CONFIG_STATE_FROM_ITEM_SCRIPT = "stateFromItemScript";
    private final Logger logger = LoggerFactory.getLogger(ScriptProfile.class);
    private final ProfileCallback callback;
    private final TransformationService transformationService;
    private final List<Class<? extends State>> acceptedDataTypes;
    private final List<Class<? extends Command>> acceptedCommandTypes;
    private final List<Class<? extends Command>> handlerAcceptedCommandTypes;
    private final String toItemScript;
    private final String commandFromItemScript;
    private final String stateFromItemScript;
    private final ProfileTypeUID profileTypeUID;
    private final boolean isConfigured;
    public ScriptProfile(ProfileTypeUID profileTypeUID, ProfileCallback callback, ProfileContext profileContext,
            TransformationService transformationService) {
        this.profileTypeUID = profileTypeUID;
        this.transformationService = transformationService;
        this.acceptedCommandTypes = profileContext.getAcceptedCommandTypes();
        this.acceptedDataTypes = profileContext.getAcceptedDataTypes();
        this.handlerAcceptedCommandTypes = profileContext.getHandlerAcceptedCommandTypes();
        this.toItemScript = ConfigParser.valueAsOrElse(profileContext.getConfiguration().get(CONFIG_TO_ITEM_SCRIPT),
                String.class, "");
        String toHandlerScript = ConfigParser
                .valueAsOrElse(profileContext.getConfiguration().get(CONFIG_TO_HANDLER_SCRIPT), String.class, "");
        String localCommandFromItemScript = ConfigParser.valueAsOrElse(
                profileContext.getConfiguration().get(CONFIG_COMMAND_FROM_ITEM_SCRIPT), String.class, "");
        this.commandFromItemScript = localCommandFromItemScript.isBlank() ? toHandlerScript
                : localCommandFromItemScript;
        this.stateFromItemScript = ConfigParser
                .valueAsOrElse(profileContext.getConfiguration().get(CONFIG_STATE_FROM_ITEM_SCRIPT), String.class, "");
        if (!toHandlerScript.isBlank() && localCommandFromItemScript.isBlank()) {
                    "'toHandlerScript' has been deprecated! Please use 'commandFromItemScript' instead in link '{}'.",
                    callback.getItemChannelLink());
        if (toItemScript.isBlank() && commandFromItemScript.isBlank() && stateFromItemScript.isBlank()) {
                    "Neither 'toItemScript', 'commandFromItemScript' nor 'stateFromItemScript' defined in link '{}'. Profile will discard all states and commands.",
            isConfigured = false;
        isConfigured = true;
    public ProfileTypeUID getProfileTypeUID() {
        return profileTypeUID;
    public void onStateUpdateFromItem(State state) {
        if (isConfigured) {
            fromItem(stateFromItemScript, state);
    public void onCommandFromItem(Command command) {
            fromItem(commandFromItemScript, command);
    private void fromItem(String script, Type type) {
        String returnValue = executeScript(script, type);
        if (returnValue != null) {
            // try to parse the value
            Command newCommand = TypeParser.parseCommand(handlerAcceptedCommandTypes, returnValue);
            if (newCommand != null) {
                callback.handleCommand(newCommand);
                logger.debug("The given type {} could not be transformed to a command", type);
    public void onCommandFromHandler(Command command) {
            String returnValue = executeScript(toItemScript, command);
                Command newCommand = TypeParser.parseCommand(acceptedCommandTypes, returnValue);
                    callback.sendCommand(newCommand);
    public void onStateUpdateFromHandler(State state) {
            transformState(state).ifPresent(callback::sendUpdate);
    public void onTimeSeriesFromHandler(TimeSeries timeSeries) {
            TimeSeries transformedTimeSeries = new TimeSeries(timeSeries.getPolicy());
            timeSeries.getStates().forEach(entry -> {
                transformState(entry.state()).ifPresent(transformedState -> {
                    transformedTimeSeries.add(entry.timestamp(), transformedState);
            if (transformedTimeSeries.size() > 0) {
                callback.sendTimeSeries(transformedTimeSeries);
    private Optional<State> transformState(State state) {
        return Optional.ofNullable(executeScript(toItemScript, state)).map(output -> {
            return switch (output) {
                case "UNDEF" -> UnDefType.UNDEF;
                case "NULL" -> UnDefType.NULL;
                default -> TypeParser.parseState(acceptedDataTypes, output);
    private @Nullable String executeScript(String script, Type input) {
        if (!script.isBlank()) {
                return transformationService.transform(script, input.toFullString());
            } catch (TransformationException e) {
                if (e.getCause() instanceof ScriptException) {
                    logger.error("Failed to process script '{}' in link '{}': {}", script,
                            callback.getItemChannelLink(), e.getCause().getMessage());
                            callback.getItemChannelLink(), e.getMessage());
