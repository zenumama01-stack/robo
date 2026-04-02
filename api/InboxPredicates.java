 * Implements static factory methods for {@link Predicate}s to filter in streams of {@link DiscoveryResult}s.
 * @author Andre Fuechsel - Initial contribution
public class InboxPredicates {
    public static Predicate<DiscoveryResult> forBinding(@Nullable String bindingId) {
        return r -> bindingId != null && bindingId.equals(r.getBindingId());
    public static Predicate<DiscoveryResult> forThingTypeUID(@Nullable ThingTypeUID uid) {
        return r -> uid != null && uid.equals(r.getThingTypeUID());
    public static Predicate<DiscoveryResult> forThingUID(@Nullable ThingUID thingUID) {
        return r -> thingUID != null && thingUID.equals(r.getThingUID());
    public static Predicate<DiscoveryResult> withFlag(DiscoveryResultFlag flag) {
        return r -> flag == r.getFlag();
    public static Predicate<DiscoveryResult> withProperty(@Nullable String propertyName, String propertyValue) {
        return r -> r.getProperties().containsKey(propertyName)
                && propertyValue.equals(r.getProperties().get(propertyName));
    public static Predicate<DiscoveryResult> withRepresentationProperty(@Nullable String propertyName) {
        return r -> propertyName != null && propertyName.equals(r.getRepresentationProperty());
    public static Predicate<DiscoveryResult> withRepresentationPropertyValue(@Nullable String propertyValue) {
        return r -> propertyValue != null && propertyValue.equals(r.getProperties().get(r.getRepresentationProperty()));
