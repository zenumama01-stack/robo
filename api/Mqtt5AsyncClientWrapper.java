import com.hivemq.client.mqtt.mqtt5.Mqtt5AsyncClient;
import com.hivemq.client.mqtt.mqtt5.Mqtt5Client;
import com.hivemq.client.mqtt.mqtt5.Mqtt5ClientBuilder;
import com.hivemq.client.mqtt.mqtt5.message.connect.Mqtt5Connect;
import com.hivemq.client.mqtt.mqtt5.message.connect.Mqtt5ConnectBuilder;
import com.hivemq.client.mqtt.mqtt5.message.publish.Mqtt5PublishResult;
import com.hivemq.client.mqtt.mqtt5.message.subscribe.Mqtt5Subscribe;
import com.hivemq.client.mqtt.mqtt5.message.unsubscribe.Mqtt5Unsubscribe;
 * The {@link Mqtt5AsyncClientWrapper} provides the wrapper for Mqttv5 async clients
 * @author Mark Herwege - Added parameter for cleanStart
public class Mqtt5AsyncClientWrapper extends MqttAsyncClientWrapper {
    private final Mqtt5AsyncClient client;
    public Mqtt5AsyncClientWrapper(String host, int port, String clientId, Protocol protocol, boolean secure,
        Mqtt5ClientBuilder clientBuilder = Mqtt5Client.builder().serverHost(host).serverPort(port).identifier(clientId)
        Mqtt5Subscribe subscribeMessage = Mqtt5Subscribe.builder().topicFilter(topic).qos(getMqttQosFromInt(qos))
        Mqtt5Unsubscribe unsubscribeMessage = Mqtt5Unsubscribe.builder().topicFilter(topic).build();
    public CompletableFuture<Mqtt5PublishResult> publish(String topic, byte[] payload, boolean retain, int qos) {
        Mqtt5Publish publishMessage = Mqtt5Publish.builder().topic(topic).qos(getMqttQosFromInt(qos)).payload(payload)
            @Nullable String username, @Nullable String password, @Nullable Boolean cleanStart) {
        Mqtt5ConnectBuilder connectMessageBuilder = Mqtt5Connect.builder().keepAlive(keepAliveInterval);
            Mqtt5Publish willPublish = Mqtt5Publish.builder().topic(lwt.getTopic()).payload(lwt.getPayload())
        if (cleanStart != null) {
            connectMessageBuilder.cleanStart(cleanStart);
