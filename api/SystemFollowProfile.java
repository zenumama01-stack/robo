 * This is the default implementation for a follow profile.
 * In contrast to the {@link SystemDefaultProfile} it does not forward any commands to the ThingHandler. Instead, it
 * turn {@link State} updates into {@link Command}s (if possible) and then forwards those to the {@link ThingHandler}.
 * This allows devices to be operated as "followers" of another one directly, without the need to write any rules.
 * The ThingHandler may send commands to the framework, but no state updates are forwarded.
public class SystemFollowProfile implements StateProfile {
    private final Logger logger = LoggerFactory.getLogger(SystemFollowProfile.class);
    public SystemFollowProfile(ProfileCallback callback) {
        return SystemProfiles.FOLLOW;
        if (!(state instanceof Command)) {
            logger.debug("The given state {} could not be transformed to a command", state);
        Command command = (Command) state;
