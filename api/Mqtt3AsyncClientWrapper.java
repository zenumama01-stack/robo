package org.openhab.core.io.transport.mqtt.internal.client;
import org.openhab.core.io.transport.mqtt.MqttBrokerConnection.ConnectionCallback;
import org.openhab.core.io.transport.mqtt.MqttWillAndTestament;
import com.hivemq.client.mqtt.MqttClientState;
import com.hivemq.client.mqtt.mqtt3.Mqtt3AsyncClient;
import com.hivemq.client.mqtt.mqtt3.Mqtt3Client;
import com.hivemq.client.mqtt.mqtt3.Mqtt3ClientBuilder;
import com.hivemq.client.mqtt.mqtt3.message.connect.Mqtt3Connect;
import com.hivemq.client.mqtt.mqtt3.message.connect.Mqtt3ConnectBuilder;
import com.hivemq.client.mqtt.mqtt3.message.subscribe.Mqtt3Subscribe;
import com.hivemq.client.mqtt.mqtt3.message.unsubscribe.Mqtt3Unsubscribe;
 * The {@link Mqtt3AsyncClientWrapper} provides the wrapper for Mqttv3 async clients
 * @author Mark Herwege - Added parameter for cleanSession
public class Mqtt3AsyncClientWrapper extends MqttAsyncClientWrapper {
    private final Mqtt3AsyncClient client;
    public Mqtt3AsyncClientWrapper(String host, int port, String clientId, Protocol protocol, boolean secure,
            boolean hostnameValidated, ConnectionCallback connectionCallback,
            @Nullable TrustManagerFactory trustManagerFactory) {
        Mqtt3ClientBuilder clientBuilder = Mqtt3Client.builder().serverHost(host).serverPort(port).identifier(clientId)
                .addConnectedListener(connectionCallback).addDisconnectedListener(connectionCallback);
        if (protocol == Protocol.WEBSOCKETS) {
            clientBuilder.webSocketWithDefaultConfig();
                clientBuilder.sslWithDefaultConfig().sslConfig().trustManagerFactory(trustManagerFactory)
                        .applySslConfig();
                        .hostnameVerifier(this).applySslConfig();
        client = clientBuilder.buildAsync();
    public MqttClientState getState() {
        return client.getState();
    public CompletableFuture<?> subscribe(String topic, int qos, Subscription subscription) {
        Mqtt3Subscribe subscribeMessage = Mqtt3Subscribe.builder().topicFilter(topic).qos(getMqttQosFromInt(qos))
        return client.subscribe(subscribeMessage, subscription::messageArrived);
    public CompletableFuture<?> unsubscribe(String topic) {
        Mqtt3Unsubscribe unsubscribeMessage = Mqtt3Unsubscribe.builder().topicFilter(topic).build();
        return client.unsubscribe(unsubscribeMessage);
    public CompletableFuture<Mqtt3Publish> publish(String topic, byte[] payload, boolean retain, int qos) {
        Mqtt3Publish publishMessage = Mqtt3Publish.builder().topic(topic).qos(getMqttQosFromInt(qos)).payload(payload)
                .retain(retain).build();
        return client.publish(publishMessage);
    public CompletableFuture<?> connect(@Nullable MqttWillAndTestament lwt, int keepAliveInterval,
            @Nullable String username, @Nullable String password, @Nullable Boolean cleanSession) {
        Mqtt3ConnectBuilder connectMessageBuilder = Mqtt3Connect.builder().keepAlive(keepAliveInterval);
        if (lwt != null) {
            Mqtt3Publish willPublish = Mqtt3Publish.builder().topic(lwt.getTopic()).payload(lwt.getPayload())
                    .retain(lwt.isRetain()).qos(getMqttQosFromInt(lwt.getQos())).build();
            connectMessageBuilder.willPublish(willPublish);
        if (cleanSession != null) {
            connectMessageBuilder.cleanSession(cleanSession);
        if (username != null && !username.isBlank() && password != null && !password.isBlank()) {
            connectMessageBuilder.simpleAuth().username(username).password(password.getBytes()).applySimpleAuth();
        return client.connect(connectMessageBuilder.build());
    public CompletableFuture<Void> disconnect() {
        return client.disconnect();
