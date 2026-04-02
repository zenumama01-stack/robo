import org.openhab.core.common.registry.Provider;
 * This class is responsible for providing {@link Rule}s. {@link RuleProvider}s are tracked by the {@link RuleRegistry}
 * service, which collect all rules from different providers of the same type.
public interface RuleProvider extends Provider<Rule> {
