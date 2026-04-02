import java.util.function.LongSupplier;
import org.mockito.Spy;
import org.openhab.core.io.transport.modbus.internal.ModbusManagerImpl;
import gnu.io.SerialPort;
import net.wimpi.modbus.ModbusCoupler;
import net.wimpi.modbus.io.ModbusTransport;
import net.wimpi.modbus.net.ModbusSerialListener;
import net.wimpi.modbus.net.ModbusTCPListener;
import net.wimpi.modbus.net.ModbusUDPListener;
import net.wimpi.modbus.net.SerialConnectionFactory;
import net.wimpi.modbus.net.TCPSlaveConnection;
import net.wimpi.modbus.net.TCPSlaveConnection.ModbusTCPTransportFactory;
import net.wimpi.modbus.net.TCPSlaveConnectionFactory;
import net.wimpi.modbus.net.UDPSlaveTerminal;
import net.wimpi.modbus.net.UDPSlaveTerminal.ModbusUDPTransportFactoryImpl;
import net.wimpi.modbus.net.UDPSlaveTerminalFactory;
import net.wimpi.modbus.net.UDPTerminal;
import net.wimpi.modbus.procimg.SimpleProcessImage;
import net.wimpi.modbus.util.AtomicCounter;
public class IntegrationTestSupport extends JavaTest {
    private final Logger logger = LoggerFactory.getLogger(IntegrationTestSupport.class);
    public enum ServerType {
        TCP,
        UDP,
        SERIAL
     * Servers to test
     * Serial is system dependent
    public static final ServerType[] TEST_SERVERS = new ServerType[] { ServerType.TCP
            // ServerType.UDP,
            // ServerType.SERIAL
    // One can perhaps test SERIAL with https://github.com/freemed/tty0tty
    // and using those virtual ports? Not the same thing as real serial device of course
    private static final String SERIAL_SERVER_PORT = "/dev/pts/7";
    private static final String SERIAL_CLIENT_PORT = "/dev/pts/8";
    private static final SerialParameters SERIAL_PARAMETERS_CLIENT = new SerialParameters(SERIAL_CLIENT_PORT, 115200,
            SerialPort.FLOWCONTROL_NONE, SerialPort.FLOWCONTROL_NONE, SerialPort.DATABITS_8, SerialPort.STOPBITS_1,
            SerialPort.PARITY_NONE, Modbus.SERIAL_ENCODING_ASCII, false, 1000);
    private static final SerialParameters SERIAL_PARAMETERS_SERVER = new SerialParameters(SERIAL_SERVER_PORT,
            SERIAL_PARAMETERS_CLIENT.getBaudRate(), SERIAL_PARAMETERS_CLIENT.getFlowControlIn(),
            SERIAL_PARAMETERS_CLIENT.getFlowControlOut(), SERIAL_PARAMETERS_CLIENT.getDatabits(),
            SERIAL_PARAMETERS_CLIENT.getStopbits(), SERIAL_PARAMETERS_CLIENT.getParity(),
            SERIAL_PARAMETERS_CLIENT.getEncoding(), SERIAL_PARAMETERS_CLIENT.isEcho(), 1000);
        System.setProperty("org.slf4j.simpleLogger.defaultLogLevel", "trace");
        System.setProperty("gnu.io.rxtx.SerialPorts", SERIAL_SERVER_PORT + File.pathSeparator + SERIAL_CLIENT_PORT);
     * Max time to wait for connections/requests from client
    protected static final int MAX_WAIT_REQUESTS_MILLIS = 1000;
     * The server runs in single thread, only one connection is accepted at a time.
     * This makes the tests as strict as possible -- connection must be closed.
    private static final int SERVER_THREADS = 1;
    protected static final int SLAVE_UNIT_ID = 1;
    private static AtomicCounter udpServerIndex = new AtomicCounter(0);
    protected @Spy TCPSlaveConnectionFactory tcpConnectionFactory = new TCPSlaveConnectionFactoryImpl();
    protected @Spy UDPSlaveTerminalFactory udpTerminalFactory = new UDPSlaveTerminalFactoryImpl();
    protected @Spy SerialConnectionFactory serialConnectionFactory = new SerialConnectionFactoryImpl();
    protected @NonNullByDefault({}) ResultCaptor<ModbusRequest> modbustRequestCaptor;
    protected @NonNullByDefault({}) ModbusTCPListener tcpListener;
    protected @NonNullByDefault({}) ModbusUDPListener udpListener;
    protected @NonNullByDefault({}) ModbusSerialListener serialListener;
    protected @NonNullByDefault({}) SimpleProcessImage spi;
    protected int tcpModbusPort = -1;
    protected int udpModbusPort = -1;
    protected ServerType serverType = ServerType.TCP;
    protected long artificialServerWait = 0;
    protected @NonNullByDefault({}) NonOSGIModbusManager modbusManager;
    private Thread serialServerThread = new Thread("ModbusTransportTestsSerialServer") {
            serialListener = new ModbusSerialListener(SERIAL_PARAMETERS_SERVER);
    protected static InetAddress localAddress() {
            return InetAddress.getByName("127.0.0.1");
            throw new UncheckedIOException(e);
        modbustRequestCaptor = new ResultCaptor<>(new LongSupplier() {
            public long getAsLong() {
                return artificialServerWait;
        modbusManager = new NonOSGIModbusManager();
        startServer();
        stopServer();
        modbusManager.close();
    protected void waitForRequests(int expectedRequestCount) {
        waitForAssert(
                () -> assertThat(modbustRequestCaptor.getAllReturnValues().size(), is(equalTo(expectedRequestCount))),
                MAX_WAIT_REQUESTS_MILLIS, 10);
    protected void waitForConnectionsReceived(int expectedConnections) {
            if (ServerType.TCP.equals(serverType)) {
                verify(tcpConnectionFactory, times(expectedConnections)).create(any(Socket.class));
            } else if (ServerType.UDP.equals(serverType)) {
                logger.debug("No-op, UDP server type");
            } else if (ServerType.SERIAL.equals(serverType)) {
                logger.debug("No-op, SERIAL server type");
        }, MAX_WAIT_REQUESTS_MILLIS, 10);
    private void startServer() {
        spi = new SimpleProcessImage();
        ModbusCoupler.getReference().setProcessImage(spi);
        ModbusCoupler.getReference().setMaster(false);
        ModbusCoupler.getReference().setUnitID(SLAVE_UNIT_ID);
            startTCPServer();
            startUDPServer();
            startSerialServer();
    private void stopServer() {
            tcpListener.stop();
            logger.debug("Stopped TCP listener, tcpModbusPort={}", tcpModbusPort);
            udpListener.stop();
            logger.debug("Stopped UDP listener, udpModbusPort={}", udpModbusPort);
                serialServerThread.join(100);
                logger.debug("Serial server thread .join() interrupted! Will interrupt it now.");
            serialServerThread.interrupt();
    private void startUDPServer() {
        udpListener = new ModbusUDPListener(localAddress(), udpTerminalFactory);
        for (int portCandidate = 10000 + udpServerIndex.increment(); portCandidate < 20000; portCandidate++) {
                DatagramSocket socket = new DatagramSocket(portCandidate);
                udpListener.setPort(portCandidate);
        udpListener.start();
        waitForUDPServerStartup();
        assertNotSame(-1, udpModbusPort);
        assertNotSame(0, udpModbusPort);
    private void waitForUDPServerStartup() {
        // Query server port. It seems to take time (probably due to thread starting)
        waitFor(() -> udpListener.getLocalPort() > 0, 5, 10_000);
        udpModbusPort = udpListener.getLocalPort();
    private void startTCPServer() {
        // Serve single user at a time
        tcpListener = new ModbusTCPListener(SERVER_THREADS, localAddress(), tcpConnectionFactory);
        // Use any open port
        tcpListener.setPort(0);
        tcpListener.start();
        waitForTCPServerStartup();
        assertNotSame(-1, tcpModbusPort);
        assertNotSame(0, tcpModbusPort);
    private void waitForTCPServerStartup() {
        waitFor(() -> tcpListener.getLocalPort() > 0, 10_000, 5);
        tcpModbusPort = tcpListener.getLocalPort();
    private void startSerialServer() {
        serialServerThread.start();
        assertDoesNotThrow(() -> Thread.sleep(1000));
        assertTrue(tcpModbusPort > 0);
        return new ModbusTCPSlaveEndpoint("127.0.0.1", tcpModbusPort, false);
     * Transport factory that spies the created transport items
    public class SpyingModbusTCPTransportFactory extends ModbusTCPTransportFactory {
        public ModbusTransport create(@NonNullByDefault({}) Socket socket) {
            ModbusTransport transport = spy(super.create(socket));
            // Capture requests produced by our server transport
            assertDoesNotThrow(() -> doAnswer(modbustRequestCaptor).when(transport).readRequest());
            return transport;
    public class SpyingModbusUDPTransportFactory extends ModbusUDPTransportFactoryImpl {
        public ModbusTransport create(@NonNullByDefault({}) UDPTerminal terminal) {
            ModbusTransport transport = spy(super.create(terminal));
    public class TCPSlaveConnectionFactoryImpl implements TCPSlaveConnectionFactory {
        public TCPSlaveConnection create(@NonNullByDefault({}) Socket socket) {
            return new TCPSlaveConnection(socket, new SpyingModbusTCPTransportFactory());
    public class UDPSlaveTerminalFactoryImpl implements UDPSlaveTerminalFactory {
        public UDPSlaveTerminal create(@NonNullByDefault({}) InetAddress interfac, int port) {
            UDPSlaveTerminal terminal = new UDPSlaveTerminal(interfac, new SpyingModbusUDPTransportFactory(), 1);
            terminal.setLocalPort(port);
            return terminal;
    public class SerialConnectionFactoryImpl implements SerialConnectionFactory {
        public SerialConnection create(@NonNullByDefault({}) SerialParameters parameters) {
            return new SerialConnection(parameters) {
                public ModbusTransport getModbusTransport() {
                    ModbusTransport transport = spy(super.getModbusTransport());
    public static class NonOSGIModbusManager extends ModbusManagerImpl implements AutoCloseable {
        public NonOSGIModbusManager() {
            activate(new HashMap<>());
