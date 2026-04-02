package org.openhab.core.thing.internal.console;
 * {@link FirmwareUpdateConsoleCommandExtension} provides console commands for managing the firmwares of things.
 * @author Christoph Knauf - added cancel command
 * @author Dimitar Ivanov - The listing of the firmwares is done for thing UID
public final class FirmwareUpdateConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String SUBCMD_STATUS = "status";
    private static final String SUBCMD_UPDATE = "update";
    private static final String SUBCMD_CANCEL = "cancel";
    private final List<FirmwareUpdateHandler> firmwareUpdateHandlers = new CopyOnWriteArrayList<>();
    public FirmwareUpdateConsoleCommandExtension(final @Reference FirmwareUpdateService firmwareUpdateService,
            final @Reference FirmwareRegistry firmwareRegistry, final @Reference ThingRegistry thingRegistry) {
        super("firmware", "Manage your things' firmwares.");
        int numberOfArguments = args.length;
        if (numberOfArguments < 1) {
            console.println("No firmware subcommand given.");
                listFirmwares(console, args);
            case SUBCMD_STATUS:
                listFirmwareStatus(console, args);
            case SUBCMD_UPDATE:
                updateFirmware(console, args);
            case SUBCMD_CANCEL:
                cancelUpdate(console, args);
                console.println(String.format("Unknown firmware sub command '%s'.", subCommand));
    private void listFirmwares(Console console, String[] args) {
        if (args.length != 2) {
            console.println("Specify the thing UID to get its available firmwares: firmware list <thingUID>");
            console.println("There is no present thing with UID " + thingUID);
        Collection<Firmware> firmwares = firmwareRegistry.getFirmwares(thing);
            console.println("No firmwares found for thing with UID " + thingUID);
        for (Firmware firmware : firmwares) {
            console.println(firmware.toString());
    private void listFirmwareStatus(Console console, String[] args) {
            console.println("Specify the thing id to get its firmware status: firmware status <thingUID>");
        FirmwareStatusInfo firmwareStatusInfo = firmwareUpdateService.getFirmwareStatusInfo(thingUID);
        if (firmwareStatusInfo != null) {
            sb.append(String.format("Firmware status for thing with UID %s is %s.", thingUID,
                    firmwareStatusInfo.getFirmwareStatus()));
            if (firmwareStatusInfo.getUpdatableFirmwareVersion() != null) {
                sb.append(String.format(" The latest updatable firmware version is %s.",
            console.println(sb.toString());
                    String.format("The firmware status for thing with UID %s could not be determined.", thingUID));
    private void cancelUpdate(Console console, String[] args) {
            console.println("Specify the thing id to cancel the update: firmware cancel <thingUID>");
        FirmwareUpdateHandler firmwareUpdateHandler = getFirmwareUpdateHandler(thingUID);
        if (firmwareUpdateHandler == null) {
            console.println(String.format("No firmware update handler available for thing with UID %s.", thingUID));
        firmwareUpdateService.cancelFirmwareUpdate(thingUID);
        console.println("Firmware update canceled.");
    private void updateFirmware(Console console, String[] args) {
        if (args.length != 3) {
                    "Specify the thing id and the firmware version to update the firmware: firmware update <thingUID> <firmware version>");
        firmwareUpdateService.updateFirmware(thingUID, args[2], null);
        console.println("Firmware update started.");
    private @Nullable FirmwareUpdateHandler getFirmwareUpdateHandler(ThingUID thingUID) {
        for (FirmwareUpdateHandler firmwareUpdateHandler : firmwareUpdateHandlers) {
            if (thingUID.equals(firmwareUpdateHandler.getThing().getUID())) {
                return firmwareUpdateHandler;
        return List.of(buildCommandUsage(SUBCMD_LIST + " <thingUID>", "lists the available firmwares for a thing"),
                buildCommandUsage(SUBCMD_STATUS + " <thingUID>", "lists the firmware status for a thing"),
                buildCommandUsage(SUBCMD_CANCEL + " <thingUID>", "cancels the update for a thing"), buildCommandUsage(
                        SUBCMD_UPDATE + " <thingUID> <firmware version>", "updates the firmware for a thing"));
    protected void addFirmwareUpdateHandler(FirmwareUpdateHandler firmwareUpdateHandler) {
        firmwareUpdateHandlers.add(firmwareUpdateHandler);
    protected void removeFirmwareUpdateHandler(FirmwareUpdateHandler firmwareUpdateHandler) {
        firmwareUpdateHandlers.remove(firmwareUpdateHandler);
