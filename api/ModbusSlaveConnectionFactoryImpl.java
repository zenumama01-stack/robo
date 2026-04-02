import org.apache.commons.pool2.BaseKeyedPooledObjectFactory;
import org.apache.commons.pool2.impl.DefaultPooledObject;
import org.openhab.core.io.transport.modbus.endpoint.ModbusIPSlaveEndpoint;
 * ModbusSlaveConnectionFactoryImpl responsible of the lifecycle of modbus slave connections
 * The actual pool uses instance of this class to create and destroy connections as-needed.
 * The overall functionality goes as follow
 * - create: create connection object but do not connect it yet
 * - destroyObject: close connection and free all resources. Called by the pool when the pool is being closed or the
 * object is invalidated.
 * - activateObject: prepare connection to be used. In practice, connect if disconnected
 * - passivateObject: passivate connection before returning it back to the pool. Currently, passivateObject closes all
 * IP-based connections every now and then (reconnectAfterMillis). Serial connections we keep open.
 * - wrap: wrap created connection to pooled object wrapper class. It tracks usage statistics and last connection time.
 * Note that the implementation must be thread safe.
public class ModbusSlaveConnectionFactoryImpl
        extends BaseKeyedPooledObjectFactory<ModbusSlaveEndpoint, @Nullable ModbusSlaveConnection> {
    class PooledConnection extends DefaultPooledObject<@Nullable ModbusSlaveConnection> {
        private volatile long lastConnected;
        private volatile @Nullable ModbusSlaveEndpoint endpoint;
        public PooledConnection(@Nullable ModbusSlaveConnection object) {
            super(object);
        public long getLastConnected() {
            return lastConnected;
        public void setLastConnected(ModbusSlaveEndpoint endpoint, long lastConnected) {
            this.lastConnected = lastConnected;
         * Reset connection if it is too old or fulfills some of the other criteria
         * @param activityName ongoing activity calling this method. For logging
         * @return whether connection was reseted
        public boolean maybeResetConnection(String activityName) {
            ModbusSlaveEndpoint localEndpoint = endpoint;
            if (localEndpoint == null) {
                // We have not connected yet, abort
                // Without endpoint we have no age parameters available (endpointPoolConfigs &
                // disconnectIfConnectedBefore)
            long localLastConnected = lastConnected;
            ModbusSlaveConnection connection = getObject();
            if (connection == null) {
            EndpointPoolConfiguration configuration = getEndpointPoolConfiguration(localEndpoint);
            long reconnectAfterMillis = configuration.getReconnectAfterMillis();
            long connectionAgeMillis = System.currentTimeMillis() - localLastConnected;
            long disconnectIfConnectedBeforeMillis = disconnectIfConnectedBefore.getOrDefault(localEndpoint, -1L);
            boolean disconnectSinceTooOldConnection = disconnectIfConnectedBeforeMillis >= 0L
                    && localLastConnected <= disconnectIfConnectedBeforeMillis;
            boolean shouldBeDisconnected = (reconnectAfterMillis == 0
                    || (reconnectAfterMillis > 0 && connectionAgeMillis > reconnectAfterMillis)
                    || disconnectSinceTooOldConnection);
            if (shouldBeDisconnected) {
                        "({}) Connection {} (endpoint {}) age {}ms is over the reconnectAfterMillis={}ms limit or has been connection time ({}) is after the \"disconnectBeforeConnectedMillis\"={} -> disconnecting.",
                        activityName, connection, localEndpoint, connectionAgeMillis, reconnectAfterMillis,
                        localLastConnected, disconnectIfConnectedBeforeMillis);
                connection.resetConnection();
                        "({}) Connection {} (endpoint {}) age ({}ms) is below the reconnectAfterMillis ({}ms) limit and connection time ({}) is after the \"disconnectBeforeConnectedMillis\"={}. Keep the connection open.",
    private final Logger logger = LoggerFactory.getLogger(ModbusSlaveConnectionFactoryImpl.class);
    private volatile Map<ModbusSlaveEndpoint, EndpointPoolConfiguration> endpointPoolConfigs = new ConcurrentHashMap<>();
    private volatile Map<ModbusSlaveEndpoint, Long> lastPassivateMillis = new ConcurrentHashMap<>();
    private volatile Map<ModbusSlaveEndpoint, Long> lastConnectMillis = new ConcurrentHashMap<>();
    private volatile Map<ModbusSlaveEndpoint, Long> disconnectIfConnectedBefore = new ConcurrentHashMap<>();
    private final Function<ModbusSlaveEndpoint, EndpointPoolConfiguration> defaultPoolConfigurationFactory;
    public ModbusSlaveConnectionFactoryImpl(
            Function<ModbusSlaveEndpoint, EndpointPoolConfiguration> defaultPoolConfigurationFactory) {
        this.defaultPoolConfigurationFactory = defaultPoolConfigurationFactory;
    private @Nullable InetAddress getInetAddress(ModbusIPSlaveEndpoint key) {
            return InetAddress.getByName(key.getAddress());
            logger.warn("KeyedPooledModbusSlaveConnectionFactory: Unknown host: {}. Connection creation failed.",
    public @Nullable ModbusSlaveConnection create(ModbusSlaveEndpoint endpoint) throws Exception {
        return endpoint.accept(new ModbusSlaveEndpointVisitor<@Nullable ModbusSlaveConnection>() {
            public @Nullable ModbusSlaveConnection visit(ModbusSerialSlaveEndpoint modbusSerialSlavePoolingKey) {
                SerialConnection connection = new SerialConnection(modbusSerialSlavePoolingKey.getSerialParameters());
                logger.trace("Created connection {} for endpoint {}", connection, modbusSerialSlavePoolingKey);
            public @Nullable ModbusSlaveConnection visit(ModbusTCPSlaveEndpoint key) {
                InetAddress address = getInetAddress(key);
                if (address == null) {
                int connectTimeoutMillis = getEndpointPoolConfiguration(key).getConnectTimeoutMillis();
                TCPMasterConnection connection = new TCPMasterConnection(address, key.getPort(), connectTimeoutMillis,
                        key.getRtuEncoded());
                logger.trace("Created connection {} for endpoint {}", connection, key);
            public @Nullable ModbusSlaveConnection visit(ModbusUDPSlaveEndpoint key) {
                UDPMasterConnection connection = new UDPMasterConnection(address, key.getPort());
    public PooledObject<@Nullable ModbusSlaveConnection> wrap(@Nullable ModbusSlaveConnection connection) {
        return new PooledConnection(connection);
    public void destroyObject(ModbusSlaveEndpoint endpoint,
            @Nullable PooledObject<@Nullable ModbusSlaveConnection> obj) {
        ModbusSlaveConnection connection = obj.getObject();
        logger.trace("destroyObject for connection {} and endpoint {} -> closing the connection", connection, endpoint);
    public void activateObject(ModbusSlaveEndpoint endpoint,
            @Nullable PooledObject<@Nullable ModbusSlaveConnection> obj) throws Exception {
            EndpointPoolConfiguration config = getEndpointPoolConfiguration(endpoint);
            if (!connection.isConnected()) {
                tryConnect(endpoint, obj, connection, config);
            long waited = waitAtleast(lastPassivateMillis.get(endpoint), config.getInterTransactionDelayMillis());
                    "Waited {}ms (interTransactionDelayMillis {}ms) before giving returning connection {} for endpoint {}, to ensure delay between transactions.",
                    waited, config.getInterTransactionDelayMillis(), obj.getObject(), endpoint);
            // Someone wants to cancel us, reset the connection and abort
            if (connection.isConnected()) {
            logger.warn("Error connecting connection {} for endpoint {}: {}", obj.getObject(), endpoint,
    public void passivateObject(ModbusSlaveEndpoint endpoint,
        logger.trace("Passivating connection {} for endpoint {}...", connection, endpoint);
        lastPassivateMillis.put(endpoint, System.currentTimeMillis());
        ((PooledConnection) obj).maybeResetConnection("passivate");
        logger.trace("...Passivated connection {} for endpoint {}", obj.getObject(), endpoint);
    public boolean validateObject(ModbusSlaveEndpoint key, @Nullable PooledObject<@Nullable ModbusSlaveConnection> p) {
        @SuppressWarnings("null") // p.getObject() cannot be null due to short-circuiting boolean condition
        boolean valid = p != null && p.getObject() != null && p.getObject().isConnected();
        ModbusSlaveConnection slaveConnection = p != null ? p.getObject() : null;
        logger.trace("Validating endpoint {} connection {} -> {}", key, slaveConnection, valid);
     * Configure general connection settings with a given endpoint
     * @param endpoint endpoint to configure
     * @param config configuration for the endpoint. Use null to reset the configuration to default settings.
    public void setEndpointPoolConfiguration(ModbusSlaveEndpoint endpoint, @Nullable EndpointPoolConfiguration config) {
            endpointPoolConfigs.remove(endpoint);
            endpointPoolConfigs.put(endpoint, config);
        return Optional.ofNullable(endpointPoolConfigs.get(endpoint))
                .orElseGet(() -> defaultPoolConfigurationFactory.apply(endpoint));
    private void tryConnect(ModbusSlaveEndpoint endpoint, PooledObject<@Nullable ModbusSlaveConnection> obj,
            ModbusSlaveConnection connection, EndpointPoolConfiguration config) throws Exception {
        Long lastConnect = lastConnectMillis.get(endpoint);
        int maxTries = config.getConnectMaxTries();
                long waited = waitAtleast(lastConnect,
                        Math.max(config.getInterConnectDelayMillis(), config.getInterTransactionDelayMillis()));
                if (waited > 0) {
                            "Waited {}ms (interConnectDelayMillis {}ms, interTransactionDelayMillis {}ms) before "
                                    + "connecting disconnected connection {} for endpoint {}, to allow delay "
                                    + "between connections re-connects",
                            waited, config.getInterConnectDelayMillis(), config.getInterTransactionDelayMillis(),
                            obj.getObject(), endpoint);
                logger.trace("Waiting {}ms for connection to warm up...", config.getAfterConnectionDelayMillis());
                if (config.getAfterConnectionDelayMillis() > 0) {
                        Thread.sleep(config.getAfterConnectionDelayMillis());
                        // sleep interrupted, continue with the method execution (only fast operations)
                long curTime = System.currentTimeMillis();
                ((PooledConnection) obj).setLastConnected(endpoint, curTime);
                lastConnectMillis.put(endpoint, curTime);
                logger.info("Connect try {}/{} failed: {}. Aborting since interrupted. Connection {}. Endpoint {}.",
                        tryIndex, maxTries, e.getMessage(), connection, endpoint);
                logger.debug("Connect try {}/{} failed: {}. Connection {}. Endpoint {}", tryIndex, maxTries,
                        e.getMessage(), connection, endpoint);
                if (tryIndex >= maxTries) {
                    logger.warn("Connect reached max tries {}, throwing last error: {}. Connection {}. Endpoint {}",
                            maxTries, e.getMessage(), connection, endpoint);
                lastConnect = System.currentTimeMillis();
        } while (true);
     * Sleep until <code>waitMillis</code> has passed from <code>lastOperation</code>
     * @param lastOperation last time operation was executed, or null if it has not been executed
     * @param waitMillis
     * @return milliseconds slept
     * @throws InterruptedException
    public static long waitAtleast(@Nullable Long lastOperation, long waitMillis) throws InterruptedException {
        if (lastOperation == null) {
        long millisSinceLast = System.currentTimeMillis() - lastOperation;
        long millisToWaitStill = Math.min(waitMillis, Math.max(0, waitMillis - millisSinceLast));
            Thread.sleep(millisToWaitStill);
            LoggerFactory.getLogger(ModbusSlaveConnectionFactoryImpl.class).debug("wait interrupted", e);
        return millisToWaitStill;
     * Disconnect returning connections which have been connected before certain time
     * @param disconnectBeforeConnectedMillis disconnected connections that have been connected before this time
    public void disconnectOnReturn(ModbusSlaveEndpoint endpoint, long disconnectBeforeConnectedMillis) {
        disconnectIfConnectedBefore.put(endpoint, disconnectBeforeConnectedMillis);
