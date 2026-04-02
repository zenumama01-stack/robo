 * {@link org.osgi.service.component.annotations.Component}s implementing this interface participate in the
 * {@link org.openhab.core.config.discovery.internal.AutomaticInboxProcessor}'s
 * decision whether to automatically approve an inbox result or not.
 * If this {@link Predicate} returns <code>true</code> the {@link DiscoveryResult} will be automatically approved by the
 * {@link org.openhab.core.config.discovery.internal.AutomaticInboxProcessor}.
 * Note that if this {@link Predicate} returns <code>false</code> the {@link DiscoveryResult} might still be
 * automatically approved (e.g., because another such {@link Predicate} returned <code>true</code>) - i.e., it is not
 * possible to veto the automatic approval of a {@link DiscoveryResult}.
 * Please note that this interface is intended to be implemented by solutions integrating openHAB. This
 * interface is <em>not</em> intended to be implemented by openHAB addons (like, e.g., bindings).
public interface InboxAutoApprovePredicate extends Predicate<DiscoveryResult> {
