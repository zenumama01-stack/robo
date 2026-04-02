package org.openhab.core.internal.service;
 * Default implementation of {@link BundleResolver}. Use the {@link FrameworkUtil} to resolve bundles.
@Component(service = BundleResolver.class)
public class BundleResolverImpl implements BundleResolver {
    public Bundle resolveBundle(Class<?> clazz) {
        return FrameworkUtil.getBundle(clazz);
