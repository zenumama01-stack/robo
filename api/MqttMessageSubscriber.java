 * Implement this interface and register on the {@see MqttBrokerConnection} to get notified
 * of incoming Mqtt messages on the given topic.
public interface MqttMessageSubscriber {
     * Process a received MQTT message.
     * @param topic The mqtt topic on which the message was received.
     * @param payload content of the message.
    void processMessage(String topic, byte[] payload);
