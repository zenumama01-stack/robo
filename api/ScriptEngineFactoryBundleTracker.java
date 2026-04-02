package org.openhab.core.automation.module.script.internal;
import org.openhab.core.service.AbstractServiceBundleTracker;
 * The {@link ScriptEngineFactoryBundleTracker} tracks bundles that provide {@link ScriptEngineFactory} and sets the
 * {@link #READY_MARKER} when all registered bundles are active
public class ScriptEngineFactoryBundleTracker extends AbstractServiceBundleTracker {
    public static final ReadyMarker READY_MARKER = new ReadyMarker("automation", "scriptEngineFactories");
    public ScriptEngineFactoryBundleTracker(final @Reference ReadyService readyService, BundleContext bc) {
        super(readyService, bc, READY_MARKER);
    protected boolean isRelevantBundle(Bundle bundle) {
        Dictionary<String, String> headers = bundle.getHeaders();
        String provideCapability = headers.get("Provide-Capability");
        return provideCapability != null && provideCapability.contains(ScriptEngineFactory.class.getName());
