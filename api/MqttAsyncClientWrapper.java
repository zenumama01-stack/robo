import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.SSLSession;
import com.hivemq.client.mqtt.datatypes.MqttQos;
 * The {@link MqttAsyncClientWrapper} is the base class for async client wrappers
public abstract class MqttAsyncClientWrapper implements HostnameVerifier {
     * connect this client
     * @param lwt last-will and testament (optional)
     * @param keepAliveInterval keep-alive interval in ms
     * @param username username (optional)
     * @param password password (optional)
     * @param cleanSessionStart cleanSession (MQTT3) or cleanStart (MQTT5)
     * @return a CompletableFuture (exceptionally on fail)
    public abstract CompletableFuture<?> connect(@Nullable MqttWillAndTestament lwt, int keepAliveInterval,
            @Nullable String username, @Nullable String password, @Nullable Boolean cleanSessionStart);
     * disconnect this client
    public abstract CompletableFuture<Void> disconnect();
     * get the connection state of this client
     * @return the client state
    public abstract MqttClientState getState();
     * publish a message
     * @param payload the message as byte array
     * @param retain whether this message should be retained
     * @param qos the QoS level of this message
    public abstract CompletableFuture<?> publish(String topic, byte[] payload, boolean retain, int qos);
     * subscribe a client callback to a topic
     * @param qos QoS for this subscription
     * @param subscription the subscription which keeps track of subscribers and retained messages
    public abstract CompletableFuture<?> subscribe(String topic, int qos, Subscription subscription);
     * unsubscribes from a topic
    public abstract CompletableFuture<?> unsubscribe(String topic);
    protected MqttQos getMqttQosFromInt(int qos) {
        switch (qos) {
                return MqttQos.AT_MOST_ONCE;
                return MqttQos.AT_LEAST_ONCE;
                return MqttQos.EXACTLY_ONCE;
                throw new IllegalArgumentException("QoS needs to be 0, 1 or 2.");
    public boolean verify(@Nullable String hostname, @Nullable SSLSession sslSession) {
