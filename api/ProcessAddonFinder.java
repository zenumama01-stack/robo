package org.openhab.core.config.discovery.addon.process;
import static org.openhab.core.config.discovery.addon.AddonFinderConstants.ADDON_SUGGESTION_FINDER;
 * This is a {@link ProcessAddonFinder} for finding suggested add-ons by checking processes running
 * on the openHAB server.
@Component(service = AddonFinder.class, name = ProcessAddonFinder.SERVICE_NAME)
public class ProcessAddonFinder extends BaseAddonFinder {
    public static final String SERVICE_TYPE = "process";
    public static final String CFG_FINDER_PROCESS = "suggestionFinderProcess";
    public static final String SERVICE_NAME = SERVICE_TYPE + ADDON_SUGGESTION_FINDER;
    private static final String COMMAND = "command";
    private static final String COMMAND_LINE = "commandLine";
    private static final Set<String> SUPPORTED_PROPERTIES = Set.of(COMMAND, COMMAND_LINE);
    private final Logger logger = LoggerFactory.getLogger(ProcessAddonFinder.class);
     * Private record to extract match property parameters from a {@link ProcessHandle.Info} object.
     * Tries to mitigate differences on different operating systems.
    protected static record ProcessInfo(@Nullable String command, @Nullable String commandLine) {
         * Initializes the command and commandLine fields.
         * If the command field is not present, it parses the first token in the command line.
        protected static ProcessInfo from(ProcessHandle.Info info) {
            String commandLine = info.commandLine().orElse(null);
            String cmd = info.command().orElse(null);
            if ((cmd == null || cmd.isEmpty()) && commandLine != null) {
                cmd = commandLine;
                String[] args = info.arguments().orElse(null);
                if (args != null) {
                    for (int i = args.length - 1; i >= 0; i--) {
                        int index = cmd.lastIndexOf(args[i]);
                            cmd = cmd.substring(0, index);
                cmd = cmd.stripTrailing();
            return new ProcessInfo(cmd, commandLine);
        logger.trace("ProcessAddonFinder::getSuggestedAddons");
        Set<ProcessInfo> processInfos;
            processInfos = ProcessHandle.allProcesses().map(process -> ProcessInfo.from(process.info()))
                    .filter(info -> (info.command != null) || (info.commandLine != null))
        } catch (SecurityException | UnsupportedOperationException unused) {
            logger.info("Cannot obtain process list, suggesting add-ons based on running processes is not possible");
                if (matchProperties.isEmpty()) {
                    logger.warn("Add-on info for '{}' contains no 'match-property'", candidate.getUID());
                Set<String> propertyNames = new HashSet<>(matchProperties.keySet());
                boolean noSupportedProperty = !propertyNames.removeAll(SUPPORTED_PROPERTIES);
                if (!propertyNames.isEmpty()) {
                    logger.warn("Add-on info for '{}' contains unsupported 'match-property' [{}]", candidate.getUID(),
                            String.join(",", propertyNames));
                    if (noSupportedProperty) {
                for (ProcessInfo processInfo : processInfos) {
                    if (propertyMatches(matchProperties, COMMAND, processInfo.command)
                            && propertyMatches(matchProperties, COMMAND_LINE, processInfo.commandLine)) {
