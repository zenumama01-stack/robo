 * The {@link PersistenceServiceBundleTracker} tracks bundles that provide {@link PersistenceService} and sets the
public class PersistenceServiceBundleTracker extends AbstractServiceBundleTracker {
    public static final ReadyMarker READY_MARKER = new ReadyMarker("persistence", "services");
    public PersistenceServiceBundleTracker(final @Reference ReadyService readyService, BundleContext bc) {
        return provideCapability != null && provideCapability.contains(PersistenceService.class.getName());
