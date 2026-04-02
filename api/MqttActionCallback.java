package org.openhab.core.io.transport.mqtt;
 * Implement this to be notified of the success or error of any method in {@link MqttBrokerConnection} that takes a
 * callback.
public interface MqttActionCallback {
    void onSuccess(String topic);
    void onFailure(String topic, Throwable error);
