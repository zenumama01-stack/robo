package org.openhab.core.ui.icon;
import org.openhab.core.ui.icon.IconSet.Format;
 * This is an abstract base class for implementing icon providers that serve icons from file resources.
 * These files could be read from the file system, directly from the bundle itself or from somewhere else that can
 * provide an {@link InputStream}.
 * The resources are expected to follow the naming convention "<category>[-<state>].<format>", e.g. "alarm.png" or
 * "alarm-on.svg".
 * Resource names must be all lower case. Whether an icon is provided or not is determined by the existence of a
 * resource without a state postfix.
 * If a specific resource for a state is available, it will be used. If not, the default icon without a state postfix is
 * used. If the state is a decimal number between 0 and 100, the implementation will look for a resource with the next
 * smaller state postfix available. Example: For category "Light" and state 84, it will check for the resources
 * light-82.png, light-81.png, light-80.png and return the first one it can find.
public abstract class AbstractResourceIconProvider implements IconProvider {
    private final Logger logger = LoggerFactory.getLogger(AbstractResourceIconProvider.class);
    protected final TranslationProvider i18nProvider;
    protected AbstractResourceIconProvider(final TranslationProvider i18nProvider) {
    public Set<IconSet> getIconSets() {
        return getIconSets(null);
    public @Nullable Integer hasIcon(String category, String iconSetId, Format format) {
        return hasResource(iconSetId, category.toLowerCase() + "." + format.toString().toLowerCase()) ? getPriority()
    public @Nullable InputStream getIcon(String category, String iconSetId, @Nullable String state, Format format) {
        String resourceWithoutState = category.toLowerCase() + "." + format.toString().toLowerCase();
            return getResource(iconSetId, resourceWithoutState);
        String iconState;
        if (state.contains(" ")) {
                String firstPart = state.substring(0, state.indexOf(" "));
                Double.valueOf(firstPart);
                iconState = firstPart;
                // firstPart is not a number, pass on the full state
                iconState = state;
        String resourceWithState = category.toLowerCase() + "-" + iconState.toLowerCase() + "."
                + format.toString().toLowerCase();
        if (hasResource(iconSetId, resourceWithState)) {
            return getResource(iconSetId, resourceWithState);
            // let's treat all percentage-based categories
                double stateAsDouble = Double.parseDouble(iconState);
                if (stateAsDouble >= 0 && stateAsDouble <= 100) {
                    for (int i = (int) stateAsDouble; i >= 0; i--) {
                        String resourceWithNumberState = category.toLowerCase() + "-" + i + "."
                        if (hasResource(iconSetId, resourceWithNumberState)) {
                            return getResource(iconSetId, resourceWithNumberState);
                // does not seem to be a number, so ignore it
            logger.debug("Use icon {} as {} is not found", resourceWithoutState, resourceWithState);
     * Provides the priority of this provider. A higher value will give this provider a precedence over others.
     * @return the priority as a positive integer
    protected abstract Integer getPriority();
     * Provides the content of a resource for a certain icon set as a stream or null, if the resource does not exist.
     * @param iconSetId the id of the icon set for which the resource is requested
     * @param resourceName the name of the resource
     * @return the content as a stream or null, if the resource does not exist
    protected abstract @Nullable InputStream getResource(String iconSetId, String resourceName);
     * Checks whether a certain resource exists for a given icon set.
     * @return true, if the resource exists, false otherwise
    protected abstract boolean hasResource(String iconSetId, String resourceName);
