 * This service contains different {@link StateDescriptionFragmentProvider}s and provides a getStateDescription method
 * that returns a single {@link StateDescription} using all of the providers.
public class StateDescriptionServiceImpl implements StateDescriptionService {
    private final Set<StateDescriptionFragmentProvider> stateDescriptionFragmentProviders = Collections
            .synchronizedSet(new TreeSet<>(new Comparator<>() {
                public int compare(StateDescriptionFragmentProvider provider1,
                        StateDescriptionFragmentProvider provider2) {
                    return provider2.getRank().compareTo(provider1.getRank());
    public void addStateDescriptionFragmentProvider(StateDescriptionFragmentProvider provider) {
        stateDescriptionFragmentProviders.add(provider);
    public void removeStateDescriptionFragmentProvider(StateDescriptionFragmentProvider provider) {
        stateDescriptionFragmentProviders.remove(provider);
    public @Nullable StateDescription getStateDescription(String itemName, @Nullable Locale locale) {
        StateDescriptionFragment stateDescriptionFragment = getMergedStateDescriptionFragments(itemName, locale);
        return stateDescriptionFragment != null ? stateDescriptionFragment.toStateDescription() : null;
    private @Nullable StateDescriptionFragment getMergedStateDescriptionFragments(String itemName,
        StateDescriptionFragmentImpl result = null;
        for (StateDescriptionFragmentProvider provider : stateDescriptionFragmentProviders) {
            StateDescriptionFragment fragment = provider.getStateDescriptionFragment(itemName, locale);
            if (fragment == null) {
            // we pick up the first valid StateDescriptionFragment here:
                // create a deep copy of the first found fragment before merging other fragments into it
                result = new StateDescriptionFragmentImpl((StateDescriptionFragmentImpl) fragment);
                result.merge(fragment);
