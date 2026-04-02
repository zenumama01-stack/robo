 * Tests the MqttBrokerConnection class
public class MqttBrokerConnectionTests extends JavaTest {
    private static final byte[] HELLO_BYTES = "hello".getBytes();
    private static final byte[] GOODBYE_BYTES = "goodbye".getBytes();
    private static byte[] eqHelloBytes() {
        return eq(HELLO_BYTES);
    private static byte[] eqGoodbyeBytes() {
        return eq(GOODBYE_BYTES);
    public void subscribeBeforeOnlineThenConnect()
            throws ConfigurationException, MqttException, InterruptedException, ExecutionException, TimeoutException {
        MqttBrokerConnectionEx connection = new MqttBrokerConnectionEx("123.123.123.123", null, false, false,
                "MqttBrokerConnectionTests");
        // Add a subscriber while still offline
        MqttMessageSubscriber subscriber = mock(MqttMessageSubscriber.class);
        connection.subscribe("homie/device123/$name", subscriber);
        assertTrue(connection.start().get(200, TimeUnit.MILLISECONDS));
        assertTrue(connection.hasSubscribers());
        assertThat(connection.connectionState(), is(MqttConnectionState.CONNECTED));
        Mqtt3Publish publishMessage = Mqtt3Publish.builder().topic("homie/device123/$name").payload(HELLO_BYTES)
        // Test if subscription is active
        connection.getSubscribers().get("homie/device123/$name").messageArrived(publishMessage);
        verify(subscriber).processMessage(eq("homie/device123/$name"), eqHelloBytes());
    public void subscribeToWildcardTopic()
        connection.subscribe("homie/device123/+", subscriber);
        MqttMessageSubscriber subscriber2 = mock(MqttMessageSubscriber.class);
        connection.subscribe("#", subscriber2);
        MqttMessageSubscriber subscriber3 = mock(MqttMessageSubscriber.class);
        connection.subscribe("homie/#", subscriber3);
        connection.getSubscribers().get("homie/device123/+").messageArrived(publishMessage);
        connection.getSubscribers().get("#").messageArrived(publishMessage);
        connection.getSubscribers().get("homie/#").messageArrived(publishMessage);
        verify(subscriber2).processMessage(eq("homie/device123/$name"), eqHelloBytes());
        verify(subscriber3).processMessage(eq("homie/device123/$name"), eqHelloBytes());
    public void subscriber()
        // Expect no subscribers
        assertFalse(connection.hasSubscribers());
        // Add subscribers (while not connected)
        connection.subscribe("utf8- topic äö:", subscriber);
        connection.subscribe("subscribe/to/multiple/$topics", subscriber);
        connection.subscribe("second/subscriber", subscriber);
        connection.subscribe("second/subscriber", subscriber2);
        // Remove subscribers (while not connected)
        connection.unsubscribe("utf8- topic äö:", subscriber);
        connection.unsubscribe("subscribe/to/multiple/$topics", subscriber);
        connection.unsubscribe("second/subscriber", subscriber);
        connection.unsubscribe("second/subscriber", subscriber2);
        // Add subscriber (while connected)
        CompletableFuture<Boolean> future = connection.subscribe("topic", subscriber);
        verify(connection.client).subscribe(any(), anyInt(), any());
        assertTrue(future.get(200, TimeUnit.MILLISECONDS));
        // Remove subscriber (while connected)
        assertTrue(connection.unsubscribe("topic", subscriber).get(200, TimeUnit.MILLISECONDS));
    public void retain()
        MqttMessageSubscriber subscriber1 = mock(MqttMessageSubscriber.class);
        connection.subscribe("topic", subscriber1);
        Mqtt3Publish publishMessage = Mqtt3Publish.builder().topic("topic").payload(HELLO_BYTES).retain(true).build();
        connection.getSubscribers().get("topic").messageArrived(publishMessage);
        publishMessage = Mqtt3Publish.builder().topic("topic").payload(GOODBYE_BYTES).build();
        connection.subscribe("topic", subscriber2);
        // the retained message was updated even though the subsequent message didn't have the retained flag
        verify(subscriber2).processMessage(eq("topic"), eqGoodbyeBytes());
    public void reconnectPolicyDefault() throws ConfigurationException, MqttException, InterruptedException {
        // Check if the default policy is set and that the broker within the policy is set.
        assertInstanceOf(PeriodicReconnectStrategy.class, connection.getReconnectStrategy());
        AbstractReconnectStrategy p = connection.getReconnectStrategy();
        assertThat(p.getBrokerConnection(), equalTo(connection));
    public void reconnectPolicy()
            throws ConfigurationException, MqttException, InterruptedException, ConfigurationException {
        MqttBrokerConnectionEx connection = spy(
                new MqttBrokerConnectionEx("123.123.123.123", null, false, false, "MqttBrokerConnectionTests"));
        connection.setConnectionCallback(connection);
        // Check setter
        connection.setReconnectStrategy(new PeriodicReconnectStrategy());
        assertThat(connection.getReconnectStrategy().getBrokerConnection(), equalTo(connection));
        // Prepare a Mock to test if lostConnect is called and
        // if the PeriodicReconnectPolicy indeed calls start()
        PeriodicReconnectStrategy mockPolicy = spy(new PeriodicReconnectStrategy(10000, 0));
        doReturn(CompletableFuture.completedFuture(true)).when(connection).start();
        mockPolicy.start();
        // Fake a disconnect
        connection.setReconnectStrategy(mockPolicy);
        doReturn(MqttConnectionState.DISCONNECTED).when(connection).connectionState();
        connection.isConnecting = true; /* Pretend that start did something */
        connection.connectionCallback.onDisconnected(new Throwable("disconnected"));
        // Check lostConnect
        verify(mockPolicy).lostConnection();
        waitForAssert(() -> verify(connection).start());
        assertTrue(mockPolicy.isReconnecting());
        // Fake connection established
        connection.connectionCallback.onConnected(null);
        assertFalse(mockPolicy.isReconnecting());
    public void timeoutWhenNotReachable() throws ConfigurationException, MqttException, InterruptedException {
                new MqttBrokerConnectionEx("10.0.10.10", null, false, false, "MqttBrokerConnectionTests"));
        ScheduledThreadPoolExecutor s = new ScheduledThreadPoolExecutor(1);
        connection.setTimeoutExecutor(s, 10);
        assertThat(connection.timeoutExecutor, is(s));
        // Adjust test conditions
        connection.connectTimeout = true;
        CountDownLatch latch = new CountDownLatch(2);
        MqttConnectionObserver o = new MqttConnectionObserver() {
            public void connectionStateChanged(MqttConnectionState state, @Nullable Throwable error) {
                if (state == MqttConnectionState.DISCONNECTED) {
                if (state == MqttConnectionState.CONNECTING) {
        connection.addConnectionObserver(o);
        connection.start();
        assertNotNull(connection.timeoutFuture);
        assertThat(latch.await(5, TimeUnit.SECONDS), is(true));
    public void timeoutWhenNotReachableFuture()
        CompletableFuture<Boolean> future = connection.start();
        verify(connection.connectionCallback).createFuture();
        verify(connection.connectionCallback, times(0)).onConnected(any());
        verify(connection.connectionCallback, times(0)).onDisconnected(any(MqttClientDisconnectedContext.class));
        assertThat(future.get(5, TimeUnit.SECONDS), is(false));
    public void connectionObserver() throws ConfigurationException, MqttException {
                new MqttBrokerConnectionEx("123.123.123.123", null, false, false, "connectionObserver"));
        // Add an observer
        assertFalse(connection.hasConnectionObservers());
        MqttConnectionObserver connectionObserver = mock(MqttConnectionObserver.class);
        connection.addConnectionObserver(connectionObserver);
        assertTrue(connection.hasConnectionObservers());
        // Adding a connection observer should not immediately call its connectionStateChanged() method.
        verify(connectionObserver, times(0)).connectionStateChanged(eq(MqttConnectionState.DISCONNECTED), any());
        // Cause a success callback
        connection.connectionStateOverwrite = MqttConnectionState.CONNECTED;
        verify(connectionObserver, times(1)).connectionStateChanged(eq(MqttConnectionState.CONNECTED), isNull());
        Exception testException = new Exception("test message");
        connection.connectionStateOverwrite = MqttConnectionState.DISCONNECTED;
        connection.connectionCallback.onDisconnected(testException);
        verify(connectionObserver, times(1)).connectionStateChanged(eq(MqttConnectionState.DISCONNECTED),
                eq(testException));
        // Remove observer
        connection.removeConnectionObserver(connectionObserver);
    public void lastWillAndTestamentTests() throws ConfigurationException {
                "lastWillAndTestamentTests");
        assertNull(connection.getLastWill());
        assertNull(MqttWillAndTestament.fromString(""));
        connection.setLastWill(MqttWillAndTestament.fromString("topic:message:1:true"));
        assertEquals("topic", connection.getLastWill().getTopic());
        assertEquals(1, connection.getLastWill().getQos());
        assertTrue(connection.getLastWill().isRetain());
        byte[] b = { 'm', 'e', 's', 's', 'a', 'g', 'e' };
        assertArrayEquals(connection.getLastWill().getPayload(), b);
    public void lastWillAndTestamentConstructorTests() {
        assertThrows(IllegalArgumentException.class, () -> new MqttWillAndTestament("", new byte[0], 0, false));
    public void qosInvalid() throws ConfigurationException {
                "qosInvalid");
        assertThrows(IllegalArgumentException.class, () -> connection.setQos(10));
    public void setterGetterTests() {
                "setterGetterTests");
        assertEquals("123.123.123.123", connection.getHost(), "URL getter");
        assertEquals(1883, connection.getPort(), "Name getter"); // Check for non-secure port
        assertFalse(connection.isSecure(), "Secure getter");
        assertFalse(connection.isHostnameValidated(), "HostnameValidated getter");
        assertEquals("setterGetterTests", connection.getClientId(), "ClientID getter");
        connection.setCredentials("user@!", "password123@^");
        assertEquals("user@!", connection.getUser(), "User getter/setter");
        assertEquals("password123@^", connection.getPassword(), "Password getter/setter");
        assertEquals(MqttBrokerConnection.DEFAULT_KEEPALIVE_INTERVAL, connection.getKeepAliveInterval());
        connection.setKeepAliveInterval(80);
        assertEquals(80, connection.getKeepAliveInterval());
        assertEquals(MqttBrokerConnection.DEFAULT_QOS, connection.getQos());
        connection.setQos(2);
        assertEquals(2, connection.getQos());
        connection.setQos(1);
        assertEquals(1, connection.getQos());
        // Check for default ssl context provider and reconnect policy
        assertNotNull(connection.getReconnectStrategy());
        assertThat(connection.connectionState(), equalTo(MqttConnectionState.DISCONNECTED));
    public void gracefulStop()
        // Add test subscribers
        connection.subscribe("abc", subscriber);
        connection.subscribe("def/subtopic", subscriber);
        assertThat(connection.hasSubscribers(), is(true));
        // Let's observe the internal connection client
        MqttAsyncClientWrapper client = connection.client;
        // Stop
        CompletableFuture<Boolean> future = connection.stop();
        // Restart strategy must be stopped
        PeriodicReconnectStrategy p = (PeriodicReconnectStrategy) connection.getReconnectStrategy();
        assertThat(p.isStarted(), is(false));
        // Wait to complete stop
        future.get(1000, TimeUnit.MILLISECONDS);
        verify(connection).unsubscribeAll();
        verify(client).disconnect();
        // Subscribers should be removed
        assertThat(connection.hasSubscribers(), is(false));
