package org.openhab.core.model.script;
import org.eclipse.xtext.xbase.typesystem.internal.FeatureScopeTracker;
import org.eclipse.xtext.xbase.typesystem.internal.IFeatureScopeTracker;
import org.eclipse.xtext.xbase.typesystem.internal.OptimizingFeatureScopeTrackerProvider;
 * {@link OptimizingFeatureScopeTrackerProvider} implementation
 * ...with a workaround for https://github.com/eclipse/xtext-extras/issues/144
public class OptimizingFeatureScopeTrackerProvider2 extends OptimizingFeatureScopeTrackerProvider {
    public IFeatureScopeTracker track(EObject root) {
        return new FeatureScopeTracker() {
