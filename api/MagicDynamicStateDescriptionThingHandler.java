import static org.openhab.core.magic.binding.MagicBindingConstants.*;
import org.openhab.core.magic.binding.internal.MagicDynamicCommandDescriptionProvider;
import org.openhab.core.magic.binding.internal.MagicDynamicStateDescriptionProvider;
import org.openhab.core.types.CommandOption;
 * ThingHandler which provides channels with dynamic state descriptions.
public class MagicDynamicStateDescriptionThingHandler extends BaseThingHandler {
    private static final String SYSTEM_COMMAND_HIBERNATE = "Hibernate";
    private static final String SYSTEM_COMMAND_REBOOT = "Reboot";
    private static final String SYSTEM_COMMAND_SHUTDOWN = "Shutdown";
    private static final String SYSTEM_COMMAND_SUSPEND = "Suspend";
    private static final String SYSTEM_COMMAND_QUIT = "Quit";
    private final MagicDynamicCommandDescriptionProvider commandDescriptionProvider;
    private final MagicDynamicStateDescriptionProvider stateDescriptionProvider;
    public MagicDynamicStateDescriptionThingHandler(Thing thing,
            MagicDynamicCommandDescriptionProvider commandDescriptionProvider,
            MagicDynamicStateDescriptionProvider stateDescriptionProvider) {
        this.commandDescriptionProvider = commandDescriptionProvider;
        this.stateDescriptionProvider = stateDescriptionProvider;
        ChannelUID systemCommandChannelUID = new ChannelUID(getThing().getUID(), CHANNEL_SYSTEM_COMMAND);
        commandDescriptionProvider.setCommandOptions(systemCommandChannelUID,
                List.of(new CommandOption(SYSTEM_COMMAND_HIBERNATE, SYSTEM_COMMAND_HIBERNATE),
                        new CommandOption(SYSTEM_COMMAND_REBOOT, SYSTEM_COMMAND_REBOOT),
                        new CommandOption(SYSTEM_COMMAND_SHUTDOWN, SYSTEM_COMMAND_SHUTDOWN),
                        new CommandOption(SYSTEM_COMMAND_SUSPEND, SYSTEM_COMMAND_SUSPEND),
                        new CommandOption(SYSTEM_COMMAND_QUIT, SYSTEM_COMMAND_QUIT)));
        ChannelUID signalStrengthChannelUID = new ChannelUID(getThing().getUID(), CHANNEL_SIGNAL_STRENGTH);
        stateDescriptionProvider.setStateOptions(signalStrengthChannelUID, List.of(new StateOption("1", "Unusable"),
                new StateOption("2", "Okay"), new StateOption("3", "Amazing")));
