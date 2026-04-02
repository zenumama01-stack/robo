import org.openhab.core.io.transport.mqtt.MqttBrokerConnection.MqttVersion;
import org.openhab.core.io.transport.mqtt.MqttBrokerConnection.Protocol;
 * Contains configuration for a MqttBrokerConnection.
public class MqttBrokerConnectionConfig {
    // Optional connection name
    public @Nullable String name;
    // Connection parameters (host+port+secure+hostnameValidated)
    public @Nullable String host;
    public @Nullable Integer port;
    public boolean secure = true;
    public boolean hostnameValidated = true;
    public boolean cleanSessionStart = true;
    // Protocol parameters
    public Protocol protocol = MqttBrokerConnection.DEFAULT_PROTOCOL;
    public MqttVersion mqttVersion = MqttBrokerConnection.DEFAULT_MQTT_VERSION;
    // Authentication parameters
    public @Nullable String username;
    public @Nullable String password;
    public @Nullable String clientID;
    // MQTT parameters
    public Integer qos = MqttBrokerConnection.DEFAULT_QOS;
    /** Keepalive in seconds */
    public @Nullable Integer keepAlive;
    // Last will parameters
    public @Nullable String lwtTopic;
    public @Nullable String lwtMessage;
    public Integer lwtQos = MqttBrokerConnection.DEFAULT_QOS;
    public Boolean lwtRetain = false;
     * Return the brokerID of this connection. This is either the name or host:port(:s), for instance "myhost:8080:s".
     * This method will return an empty string, if none of the parameters is set.
    public String getBrokerID() {
        final String name = this.name;
        if (name != null && !name.isEmpty()) {
            StringBuilder b = new StringBuilder();
                b.append(host);
            final Integer port = this.port;
            if (port != null) {
                b.append(":");
                b.append(port.toString());
            if (secure) {
                b.append(":s");
            return b.toString();
     * Output the name, host, port, secure flag and hostname validation flag
            b.append(name);
            b.append(", ");
        if (hostnameValidated) {
            b.append(":v");
