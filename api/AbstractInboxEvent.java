package org.openhab.core.config.discovery.inbox.events;
import org.openhab.core.config.discovery.dto.DiscoveryResultDTO;
 * Abstract implementation of an inbox event which will be posted by the
 * {@link org.openhab.core.config.discovery.inbox.Inbox}
 * for added, removed and updated discovery results.
 * @author Stefan Bußweiler - Initial contribution
public abstract class AbstractInboxEvent extends AbstractEvent {
    private final DiscoveryResultDTO discoveryResult;
     * Must be called in subclass constructor to create an inbox event.
     * @param discoveryResult the discovery-result data transfer object
    protected AbstractInboxEvent(String topic, String payload, DiscoveryResultDTO discoveryResult) {
        this.discoveryResult = discoveryResult;
     * Gets the discovery result as data transfer object.
     * @return the discoveryResult
    public DiscoveryResultDTO getDiscoveryResult() {
