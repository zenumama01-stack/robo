package org.openhab.core.io.transport.serial.internal.console;
import org.openhab.core.io.transport.serial.internal.SerialPortRegistry;
 * {@link SerialCommandExtension} provides console commands for serial ports.
public class SerialCommandExtension extends AbstractConsoleCommandExtension {
    private static final String CMD_SERIAL = "serial";
    private static final String SUBCMD_IDENTIFIER_ALL = "identifiers";
    private static final String SUBCMD_IDENTIFIER_NAME = "identifier";
    private static final String SUBCMD_PORT_CREATORS = "creators";
    private final SerialPortRegistry serialPortRegistry;
    public SerialCommandExtension(final @Reference SerialPortManager serialPortManager,
            final @Reference SerialPortRegistry serialPortRegistry) {
        super(CMD_SERIAL, "Access your serial port interfaces.");
        this.serialPortRegistry = serialPortRegistry;
        final Deque<String> argList = new LinkedList<>(Arrays.asList(args));
        if (argList.isEmpty()) {
        final String subCmd = argList.removeFirst();
        switch (subCmd) {
            case SUBCMD_IDENTIFIER_ALL:
                serialPortManager.getIdentifiers().forEach(id -> {
                    console.println(str(id));
            case SUBCMD_IDENTIFIER_NAME:
                    console.println("Missing name");
                final String name = argList.removeFirst();
                console.println(str(serialPortManager.getIdentifier(name)));
            case SUBCMD_PORT_CREATORS:
                serialPortRegistry.getPortCreators().forEach(provider -> {
                    console.printf("%s, accepted protocols: %s, port identifiers: %s%n", provider.getClass(),
                            provider.getAcceptedProtocols().toList(),
                            provider.getSerialPortIdentifiers().map(SerialCommandExtension::str).toList());
                console.printf("Unknown sub command: %s%n", subCmd);
                buildCommandUsage(SUBCMD_IDENTIFIER_ALL, "lists all identifiers"), //
                buildCommandUsage(SUBCMD_IDENTIFIER_NAME, "lists a specific identifier"), //
                buildCommandUsage(SUBCMD_PORT_CREATORS, "gets details about the port creators") //
    private static String str(final @Nullable SerialPortIdentifier id) {
            return "<null>";
            return String.format("[name: %s, current owner: %s]", id.getName(), id.getCurrentOwner());
