 * The {@link DynamicStateDescriptionProvider} is responsible for providing {@link StateDescription} for a
 * {@link Channel} dynamically in the runtime. Therefore the provider must be registered as OSGi service.
 * @author Pawel Pieczul - Initial contribution
public interface DynamicStateDescriptionProvider {
     * For a given {@link Channel}, return a {@link StateDescription} that should be used for the channel, instead of
     * For a particular channel, there should be only one provider of the dynamic state description. When more than one
     * description is provided for the same channel (by different providers), only one will be used, from the provider
     * that registered first.
     * original state description in such case.
     * @param originalStateDescription original state description retrieved from the channel type
     * @return state description or null if none provided
    StateDescription getStateDescription(Channel channel, @Nullable StateDescription originalStateDescription,
