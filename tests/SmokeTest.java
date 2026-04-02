import static org.junit.jupiter.api.Assumptions.*;
import java.net.SocketImpl;
import java.net.SocketImplFactory;
import java.net.SocketOption;
import java.util.concurrent.CountDownLatch;
import net.wimpi.modbus.procimg.SimpleDigitalIn;
import net.wimpi.modbus.procimg.SimpleDigitalOut;
import net.wimpi.modbus.procimg.SimpleRegister;
public class SmokeTest extends IntegrationTestSupport {
    private static final int COIL_EVERY_N_TRUE = 2;
    private static final int DISCRETE_EVERY_N_TRUE = 3;
    private static final int HOLDING_REGISTER_MULTIPLIER = 1;
    private static final int INPUT_REGISTER_MULTIPLIER = 10;
    private static final SpyingSocketFactory SOCKET_SPY = new SpyingSocketFactory();
            Socket.setSocketImplFactory(SOCKET_SPY);
            fail("Could not install socket spy in SmokeTest");
     * Whether tests are run in Continuous Integration environment, i.e. Jenkins or Travis CI
     * Travis CI is detected using CI environment variable, see https://docs.travis-ci.com/user/environment-variables/
     * Jenkins CI is detected using JENKINS_HOME environment variable
    private boolean isRunningInCI() {
        String jenkinsHome = System.getenv("JENKINS_HOME");
        return "true".equals(System.getenv("CI")) || (jenkinsHome != null && !jenkinsHome.isBlank());
    private void generateData() {
        for (int i = 0; i < 100; i++) {
            spi.addRegister(new SimpleRegister(i * HOLDING_REGISTER_MULTIPLIER));
            spi.addInputRegister(new SimpleRegister(i * INPUT_REGISTER_MULTIPLIER));
            spi.addDigitalOut(new SimpleDigitalOut(i % COIL_EVERY_N_TRUE == 0));
            spi.addDigitalIn(new SimpleDigitalIn(i % DISCRETE_EVERY_N_TRUE == 0));
    private void testCoilValues(BitArray bits, int offsetInBitArray) {
        for (int i = 0; i < bits.size(); i++) {
            boolean expected = (i + offsetInBitArray) % COIL_EVERY_N_TRUE == 0;
            assertThat(String.format("i=%d, expecting %b, got %b", i, bits.getBit(i), expected), bits.getBit(i),
                    is(equalTo(expected)));
    private void testDiscreteValues(BitArray bits, int offsetInBitArray) {
            boolean expected = (i + offsetInBitArray) % DISCRETE_EVERY_N_TRUE == 0;
    private void testHoldingValues(ModbusRegisterArray registers, int offsetInRegisters) {
        for (int i = 0; i < registers.size(); i++) {
            int expected = (i + offsetInRegisters) * HOLDING_REGISTER_MULTIPLIER;
            assertThat(String.format("i=%d, expecting %d, got %d", i, registers.getRegister(i), expected),
                    registers.getRegister(i), is(equalTo(expected)));
    private void testInputValues(ModbusRegisterArray registers, int offsetInRegisters) {
            int expected = (i + offsetInRegisters) * INPUT_REGISTER_MULTIPLIER;
    public void setUpSocketSpy() throws IOException {
        SOCKET_SPY.sockets.clear();
     * Test handling of slave error responses. In this case, error code = 2, illegal data address, since no data.
    public void testSlaveReadErrorResponse() throws Exception {
        ModbusSlaveEndpoint endpoint = getEndpoint();
        AtomicInteger okCount = new AtomicInteger();
        AtomicInteger errorCount = new AtomicInteger();
        CountDownLatch callbackCalled = new CountDownLatch(1);
        AtomicReference<Exception> lastError = new AtomicReference<>();
        try (ModbusCommunicationInterface comms = modbusManager.newModbusCommunicationInterface(endpoint, null)) {
            comms.submitOneTimePoll(new ModbusReadRequestBlueprint(SLAVE_UNIT_ID,
                    ModbusReadFunctionCode.READ_MULTIPLE_REGISTERS, 0, 5, 1), result -> {
                        assertTrue(result.getRegisters().isPresent());
                        okCount.incrementAndGet();
                        callbackCalled.countDown();
                    }, failure -> {
                        errorCount.incrementAndGet();
                        lastError.set(failure.getCause());
            assertTrue(callbackCalled.await(60, TimeUnit.SECONDS));
            assertThat(okCount.get(), is(equalTo(0)));
            assertThat(errorCount.get(), is(equalTo(1)));
            assertInstanceOf(ModbusSlaveErrorResponseException.class, lastError.get(), lastError.toString());
     * Test handling of connection error responses.
    public void testSlaveConnectionError() throws Exception {
        // In the test we have non-responding slave (see http://stackoverflow.com/a/904609), and we use short connection
        // timeout
        ModbusSlaveEndpoint endpoint = new ModbusTCPSlaveEndpoint("10.255.255.1", 9999, false);
        EndpointPoolConfiguration configuration = new EndpointPoolConfiguration();
        configuration.setConnectTimeoutMillis(100);
        try (ModbusCommunicationInterface comms = modbusManager.newModbusCommunicationInterface(endpoint,
                configuration)) {
            assertInstanceOf(ModbusConnectionException.class, lastError.get(), lastError.toString());
     * Have super slow connection response, eventually resulting as timeout (due to default timeout of 3 s in
     * net.wimpi.modbus.Modbus.DEFAULT_TIMEOUT)
    public void testIOError() throws Exception {
        artificialServerWait = 60000;
            assertTrue(callbackCalled.await(15, TimeUnit.SECONDS));
            assertThat(lastError.toString(), errorCount.get(), is(equalTo(1)));
            assertInstanceOf(ModbusSlaveIOException.class, lastError.get(), lastError.toString());
    public void testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode functionCode, int count) throws Exception {
        assertThat(functionCode, is(anyOf(equalTo(ModbusReadFunctionCode.READ_INPUT_DISCRETES),
                equalTo(ModbusReadFunctionCode.READ_COILS))));
        generateData();
        AtomicInteger unexpectedCount = new AtomicInteger();
        AtomicReference<Object> lastData = new AtomicReference<>();
        final int offset = 1;
            comms.submitOneTimePoll(new ModbusReadRequestBlueprint(SLAVE_UNIT_ID, functionCode, offset, count, 1),
                    result -> {
                        Optional<BitArray> bitsOptional = result.getBits();
                        if (bitsOptional.isPresent()) {
                            lastData.set(bitsOptional.get());
                            unexpectedCount.incrementAndGet();
            assertThat(unexpectedCount.get(), is(equalTo(0)));
            BitArray bits = (BitArray) lastData.get();
            assertThat(bits, notNullValue());
            assertThat(bits.size(), is(equalTo(count)));
            if (functionCode == ModbusReadFunctionCode.READ_INPUT_DISCRETES) {
                testDiscreteValues(bits, offset);
                testCoilValues(bits, offset);
    public void testOneOffReadWithDiscrete1() throws Exception {
        testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode.READ_INPUT_DISCRETES, 1);
    public void testOneOffReadWithDiscrete7() throws Exception {
        // less than byte
        testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode.READ_INPUT_DISCRETES, 7);
    public void testOneOffReadWithDiscrete8() throws Exception {
        // exactly one byte
        testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode.READ_INPUT_DISCRETES, 8);
    public void testOneOffReadWithDiscrete13() throws Exception {
        // larger than byte, less than word (16 bit)
        testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode.READ_INPUT_DISCRETES, 13);
    public void testOneOffReadWithDiscrete18() throws Exception {
        // larger than word (16 bit)
        testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode.READ_INPUT_DISCRETES, 18);
    public void testOneOffReadWithCoils1() throws Exception {
        testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode.READ_COILS, 1);
    public void testOneOffReadWithCoils7() throws Exception {
        testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode.READ_COILS, 7);
    public void testOneOffReadWithCoils8() throws Exception {
        testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode.READ_COILS, 8);
    public void testOneOffReadWithCoils13() throws Exception {
        testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode.READ_COILS, 13);
    public void testOneOffReadWithCoils18() throws Exception {
        testOneOffReadWithDiscreteOrCoils(ModbusReadFunctionCode.READ_COILS, 18);
    public void testOneOffReadWithHolding() throws Exception {
                    ModbusReadFunctionCode.READ_MULTIPLE_REGISTERS, 1, 15, 1), result -> {
                        Optional<ModbusRegisterArray> registersOptional = result.getRegisters();
                        if (registersOptional.isPresent()) {
                            lastData.set(registersOptional.get());
            ModbusRegisterArray registers = (ModbusRegisterArray) lastData.get();
            assertThat(registers.size(), is(equalTo(15)));
            testHoldingValues(registers, 1);
    public void testOneOffReadWithInput() throws Exception {
                    ModbusReadFunctionCode.READ_INPUT_REGISTERS, 1, 15, 1), result -> {
            testInputValues(registers, 1);
    public void testOneOffWriteMultipleCoil() throws Exception {
        LoggerFactory.getLogger(this.getClass()).error("STARTING MULTIPLE");
        BitArray bits = new BitArray(true, true, false, false, true, true);
            comms.submitOneTimeWrite(new ModbusWriteCoilRequestBlueprint(SLAVE_UNIT_ID, 3, bits, true, 1), result -> {
                lastData.set(result.getResponse());
                assertThat(lastData.get(), is(notNullValue()));
                ModbusResponse response = (ModbusResponse) lastData.get();
                assertThat(response.getFunctionCode(), is(equalTo(15)));
                assertThat(modbustRequestCaptor.getAllReturnValues().size(), is(equalTo(1)));
                ModbusRequest request = modbustRequestCaptor.getAllReturnValues().getFirst();
                assertThat(request.getFunctionCode(), is(equalTo(15)));
                assertThat(((WriteMultipleCoilsRequest) request).getReference(), is(equalTo(3)));
                assertThat(((WriteMultipleCoilsRequest) request).getBitCount(), is(equalTo(bits.size())));
                BitVector writeRequestCoils = ((WriteMultipleCoilsRequest) request).getCoils();
                BitArray writtenBits = new BitArray(BitSet.valueOf(writeRequestCoils.getBytes()), bits.size());
                assertThat(writtenBits, is(equalTo(bits)));
            }, 6000, 10);
        LoggerFactory.getLogger(this.getClass()).error("ENDINGMULTIPLE");
     * Write is out-of-bounds, slave should return error
    public void testOneOffWriteMultipleCoilError() throws Exception {
        BitArray bits = new BitArray(500);
    public void testOneOffWriteSingleCoil() throws Exception {
        BitArray bits = new BitArray(true);
            comms.submitOneTimeWrite(new ModbusWriteCoilRequestBlueprint(SLAVE_UNIT_ID, 3, bits, false, 1), result -> {
            assertThat(response.getFunctionCode(), is(equalTo(5)));
            assertThat(request.getFunctionCode(), is(equalTo(5)));
            assertThat(((WriteCoilRequest) request).getReference(), is(equalTo(3)));
            assertThat(((WriteCoilRequest) request).getCoil(), is(equalTo(true)));
    public void testOneOffWriteSingleCoilError() throws Exception {
            comms.submitOneTimeWrite(new ModbusWriteCoilRequestBlueprint(SLAVE_UNIT_ID, 300, bits, false, 1),
            assertThat(((WriteCoilRequest) request).getReference(), is(equalTo(300)));
     * Testing regular polling of coils
     * Amount of requests is timed, and average poll period is checked
    public void testRegularReadEvery150msWithCoil() throws Exception {
        CountDownLatch callbackCalled = new CountDownLatch(5);
        AtomicInteger dataReceived = new AtomicInteger();
            comms.registerRegularPoll(
                    new ModbusReadRequestBlueprint(SLAVE_UNIT_ID, ModbusReadFunctionCode.READ_COILS, 1, 15, 1), 150, 0,
                            BitArray bits = bitsOptional.get();
                            dataReceived.incrementAndGet();
                                assertThat(bits.size(), is(equalTo(15)));
                                testCoilValues(bits, 1);
                            } catch (AssertionError e) {
            long end = System.currentTimeMillis();
            assertPollDetails(unexpectedCount, dataReceived, start, end, 145, 500);
     * Testing regular polling of holding registers
    public void testRegularReadEvery150msWithHolding() throws Exception {
            comms.registerRegularPoll(new ModbusReadRequestBlueprint(SLAVE_UNIT_ID,
                    ModbusReadFunctionCode.READ_MULTIPLE_REGISTERS, 1, 15, 1), 150, 0, result -> {
                            ModbusRegisterArray registers = registersOptional.get();
    public void testRegularReadFirstErrorThenOK() throws Exception {
     * @param unexpectedCount number of unexpected callback calls
     * @param expectedCount number of expected callback calls (onBits or onRegisters)
     * @param pollStartMillis poll start time in milliepoch
     * @param expectedPollAverageMin average poll period should be at least greater than this
     * @param expectedPollAverageMax average poll period less than this
    private void assertPollDetails(AtomicInteger unexpectedCount, AtomicInteger expectedCount, long pollStartMillis,
            long pollEndMillis, int expectedPollAverageMin, int expectedPollAverageMax) throws InterruptedException {
        int responses = expectedCount.get();
        assertTrue(responses > 1);
        // Rest of the (timing-sensitive) assertions are not run in CI
        assumeFalse(isRunningInCI(), "Running in CI! Will not test timing-sensitive details");
        float averagePollPeriodMillis = ((float) (pollEndMillis - pollStartMillis)) / (responses - 1);
        assertTrue(averagePollPeriodMillis > expectedPollAverageMin && averagePollPeriodMillis < expectedPollAverageMax,
                String.format(
                        "Measured avarage poll period %f ms (%d responses in %d ms) is not withing expected limits [%d, %d]",
                        averagePollPeriodMillis, responses, pollEndMillis - pollStartMillis, expectedPollAverageMin,
                        expectedPollAverageMax));
    public void testUnregisterPollingOnClose() throws Exception {
        CountDownLatch successfulCountDownLatch = new CountDownLatch(3);
        AtomicInteger expectedReceived = new AtomicInteger();
                    ModbusReadFunctionCode.READ_MULTIPLE_REGISTERS, 1, 15, 1), 200, 0, result -> {
                            expectedReceived.incrementAndGet();
                            successfulCountDownLatch.countDown();
                            // bits
                        if (spi.getDigitalInCount() > 0) {
                            // No errors expected after server filled with data
            // Wait for N successful responses before proceeding with assertions of poll rate
            assertTrue(successfulCountDownLatch.await(60, TimeUnit.SECONDS));
            assertPollDetails(unexpectedCount, expectedReceived, start, end, 190, 600);
            // wait some more and ensure nothing comes back
            Thread.sleep(500);
    public void testUnregisterPollingExplicit() throws Exception {
        CountDownLatch callbackCalled = new CountDownLatch(3);
            PollTask task = comms.registerRegularPoll(new ModbusReadRequestBlueprint(SLAVE_UNIT_ID,
            // Explicitly unregister the regular poll
            comms.unregisterRegularPoll(task);
    public void testPoolConfigurationWithoutListener() throws Exception {
        EndpointPoolConfiguration defaultConfig = modbusManager.getEndpointPoolConfiguration(getEndpoint());
        assertThat(defaultConfig, is(notNullValue()));
        EndpointPoolConfiguration newConfig = new EndpointPoolConfiguration();
        newConfig.setConnectMaxTries(5);
        try (ModbusCommunicationInterface unused = modbusManager.newModbusCommunicationInterface(getEndpoint(),
                newConfig)) {
            // Sets configuration for the endpoint implicitly
        assertThat(modbusManager.getEndpointPoolConfiguration(getEndpoint()).getConnectMaxTries(), is(equalTo(5)));
        assertThat(modbusManager.getEndpointPoolConfiguration(getEndpoint()), is(not(equalTo(defaultConfig))));
        // Reset config
        try (ModbusCommunicationInterface ignored = modbusManager.newModbusCommunicationInterface(getEndpoint(),
        // Should match the default
        assertThat(modbusManager.getEndpointPoolConfiguration(getEndpoint()), is(equalTo(defaultConfig)));
    public void testConnectionCloseAfterLastCommunicationInterfaceClosed() throws Exception {
        assumeTrue(endpoint instanceof ModbusTCPSlaveEndpoint,
                "Connection closing test supported only with TCP slaves");
        // Generate server data
        EndpointPoolConfiguration config = new EndpointPoolConfiguration();
        config.setReconnectAfterMillis(9_000_000);
        // 1. capture open connections at this point
        long openSocketsBefore = getNumberOfOpenClients(SOCKET_SPY);
        assertThat(openSocketsBefore, is(equalTo(0L)));
        // 2. make poll, binding opens the tcp connection
        try (ModbusCommunicationInterface comms = modbusManager.newModbusCommunicationInterface(endpoint, config)) {
                CountDownLatch latch = new CountDownLatch(1);
                comms.submitOneTimePoll(new ModbusReadRequestBlueprint(1, ModbusReadFunctionCode.READ_COILS, 0, 1, 1),
                        response -> {
                            latch.countDown();
                assertTrue(latch.await(60, TimeUnit.SECONDS));
                // 3. ensure one open connection
                long openSocketsAfter = getNumberOfOpenClients(SOCKET_SPY);
                assertThat(openSocketsAfter, is(equalTo(1L)));
            try (ModbusCommunicationInterface ignored = modbusManager.newModbusCommunicationInterface(endpoint,
                    config)) {
                    comms.submitOneTimePoll(
                            new ModbusReadRequestBlueprint(1, ModbusReadFunctionCode.READ_COILS, 0, 1, 1), response -> {
                assertThat(getNumberOfOpenClients(SOCKET_SPY), is(equalTo(1L)));
                // wait for moment (to check that no connections are closed)
                Thread.sleep(1000);
                // no more than 1 connection, even though requests are going through
            // Still one connection open even after closing second connection
        } // 4. close (the last) comms
          // ensure that open connections are closed
          // (despite huge "reconnect after millis")
            long openSocketsAfterClose = getNumberOfOpenClients(SOCKET_SPY);
            assertThat(openSocketsAfterClose, is(equalTo(0L)));
    public void testConnectionCloseAfterOneOffPoll() throws Exception {
        config.setReconnectAfterMillis(2_000);
            // Right after the poll we should have one connection open
            // 4. Connection should close itself by the commons pool eviction policy (checking for old idle connection
            // every now and then)
                assertThat(openSocketsAfter, is(equalTo(0L)));
            }, 60_000, 50);
    private long getNumberOfOpenClients(SpyingSocketFactory socketSpy) {
        localAddress();
        return socketSpy.sockets.stream().filter(this::isConnectedToTestServer).count();
     * Spy all sockets that are created
     * @author Sami Salonen
    private static class SpyingSocketFactory implements SocketImplFactory {
        Queue<SocketImpl> sockets = new ConcurrentLinkedQueue<>();
        public SocketImpl createSocketImpl() {
            SocketImpl socket = newSocksSocketImpl();
            sockets.add(socket);
            return socket;
    private static SocketImpl newSocksSocketImpl() {
            Class<?> socksSocketImplClass = Class.forName("java.net.SocksSocketImpl");
            Class<?> socketImplClass = SocketImpl.class;
            // // For Debugging
            // for (Method method : socketImplClass.getDeclaredMethods()) {
            // LoggerFactory.getLogger("foobar")
            // .error("SocketImpl." + method.getName() + Arrays.toString(method.getParameters()));
            // for (Constructor constructor : socketImplClass.getDeclaredConstructors()) {
            // .error("SocketImpl." + constructor.getName() + Arrays.toString(constructor.getParameters()));
            // for (Method method : socksSocketImplClass.getDeclaredMethods()) {
            // .error("SocksSocketImpl." + method.getName() + Arrays.toString(method.getParameters()));
            // for (Constructor constructor : socksSocketImplClass.getDeclaredConstructors()) {
            // LoggerFactory.getLogger("foobar").error(
            // "SocksSocketImpl." + constructor.getName() + Arrays.toString(constructor.getParameters()));
                Constructor<?> constructor = socksSocketImplClass.getDeclaredConstructor();
                constructor.setAccessible(true);
                return (SocketImpl) Objects.requireNonNull(constructor.newInstance());
                // Newer Javas (Java 14->) do not have default constructor 'SocksSocketImpl()'
                // Instead we use "static SocketImpl.createPlatformSocketImpl" and "SocksSocketImpl(SocketImpl)
                Method socketImplCreateMethod = socketImplClass.getDeclaredMethod("createPlatformSocketImpl",
                        boolean.class);
                socketImplCreateMethod.setAccessible(true);
                Object socketImpl = socketImplCreateMethod
                        .invoke(/* null since we deal with static method */ giveNull(), /* server */false);
                Constructor<?> socksSocketImplConstructor = socksSocketImplClass
                        .getDeclaredConstructor(socketImplClass);
                socksSocketImplConstructor.setAccessible(true);
                return (SocketImpl) Objects.requireNonNull(socksSocketImplConstructor.newInstance(socketImpl));
    private boolean isConnectedToTestServer(SocketImpl impl) {
        final InetAddress testServerAddress = localAddress();
        final int port;
        boolean connected = true;
        final InetAddress address;
            Method getPort = SocketImpl.class.getDeclaredMethod("getPort");
            getPort.setAccessible(true);
            port = (int) getPort.invoke(impl);
            Method getInetAddressMethod = SocketImpl.class.getDeclaredMethod("getInetAddress");
            getInetAddressMethod.setAccessible(true);
            address = (InetAddress) getInetAddressMethod.invoke(impl);
            // hacky (but java8-14 compatible) way to know if socket is open
            // SocketImpl.getOption throws IOException when socket is closed
            Method getOption = SocketImpl.class.getDeclaredMethod("getOption", SocketOption.class);
            getOption.setAccessible(true);
                getOption.invoke(impl, StandardSocketOptions.SO_KEEPALIVE);
            } catch (InvocationTargetException e) {
                if (e.getTargetException() instanceof IOException) {
                    connected = false;
        return port == tcpModbusPort && connected && address.equals(testServerAddress);
