package org.openhab.core.model.core.internal;
public class ModelCoreActivator implements BundleActivator {
    private static @Nullable BundleContext context;
    static @Nullable BundleContext getContext() {
    public void start(@Nullable BundleContext bundleContext) throws Exception {
        ModelCoreActivator.context = bundleContext;
    public void stop(@Nullable BundleContext bundleContext) throws Exception {
        ModelCoreActivator.context = null;
